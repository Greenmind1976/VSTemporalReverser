using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockEntityTemporalDeconstructorDevice : BlockEntityGenericContainer
{
    private static readonly string[] RunningVisualStates = ["-running-", "-running0-", "-running20-", "-running40-", "-running60-", "-running80-", "-running100-"];
    private const int InputSlotCount = 8;
    private const int FuelSlotId = 8;
    private const int OutputSlotStart = 9;
    private const int OutputSlotCount = 12;
    private const int TemporalDustFuelCost = 10;
    private const int DeconstructionDurationMs = 20000;
    private const int PhaseStateTickMs = 100;
    private const int ProgressTickMs = 120;
    private const int VisualPulseIntervalMs = 200;
    private const int SwitchItemPauseDurationMs = 2400;
    private const int ShutdownEffectDurationMs = 7760;
    private const int CompletionHoldDurationMs = 500;
    private const float MachineLoopBaseVolume = 0.22f;
    private const float MachineLoopFadeDistance = 16f;
    private const float MachineLoopFadeInSeconds = 0.9f;
    private static readonly object DebugLogLock = new();
    private static string? debugLogPath;

    private bool isDeconstructing;
    private bool temporalStabilityLost;
    private bool outputCapacityBlocked;
    private int activeInputSlotId = -1;
    private int activeInputBatchSize = 1;
    private long deconstructionStartedAtMs;
    private long deconstructionCompleteAtMs;
    private long nextVisualPulseAtMs;
    private long phaseStateListenerId;
    private long progressListenerId;
    private long visualParticleListenerId;
    private long deferredResumeListenerId;
    private long queuePauseUntilMs;
    private long queueResumeCallbackId;
    private long shutdownVisualUntilMs;
    private long clientSoundCueId;
    private bool suppressInventoryChanged;
    private string clientSoundCuePath = string.Empty;
    private long lastHandledClientSoundCueId;
    private ILoadedSound? machineLoopSound;
    private TreeAttribute salvageRemainders = new();

    private sealed class DeconstructionJob
    {
        public int ConsumedInputCount { get; init; }
        public List<ItemStack> OutputStacks { get; init; } = new();
        public Dictionary<string, double> UpdatedRemainders { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private ItemSlot FuelSlot => Inventory[FuelSlotId];

    public bool IsDeconstructing => isDeconstructing;
    public bool HasTemporalDustFuel => FuelSlot.Itemstack != null && IsTemporalDust(FuelSlot.Itemstack) && FuelSlot.StackSize >= TemporalDustFuelCost;
    public bool HasInputItem => FindFirstOccupiedInputSlotId() >= 0;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        Inventory.SlotModified += OnInventorySlotModified;
        QueueDeferredDeconstructionResume();
        UpdateServerPhaseListener();
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
        EvaluateDeconstructionState();
    }

    protected override void OnTick(float dt)
    {
        base.OnTick(dt);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        isDeconstructing = tree.GetBool("isDeconstructing");
        temporalStabilityLost = tree.GetBool("temporalStabilityLost");
        outputCapacityBlocked = tree.GetBool("outputCapacityBlocked");
        activeInputSlotId = tree.GetInt("activeInputSlotId", -1);
        activeInputBatchSize = tree.GetInt("activeInputBatchSize", 1);
        deconstructionStartedAtMs = tree.GetLong("deconstructionStartedAtMs");
        deconstructionCompleteAtMs = tree.GetLong("deconstructionCompleteAtMs");
        queuePauseUntilMs = tree.GetLong("queuePauseUntilMs");
        shutdownVisualUntilMs = tree.GetLong("shutdownVisualUntilMs");
        clientSoundCueId = tree.GetLong("clientSoundCueId");
        clientSoundCuePath = tree.GetString("clientSoundCuePath", string.Empty);
        salvageRemainders = tree["salvageRemainders"] as TreeAttribute ?? new TreeAttribute();

        if (Api != null)
        {
            UpdateVisualState(ShouldDisplayRunningEffects());
            UpdateClientParticleListener();
            UpdateClientMachineLoopSound();
            if (Api.Side == EnumAppSide.Client)
            {
                TryPlayQueuedClientSound();
            }
            else
            {
                QueueDeferredDeconstructionResume();
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetBool("isDeconstructing", isDeconstructing);
        tree.SetBool("temporalStabilityLost", temporalStabilityLost);
        tree.SetBool("outputCapacityBlocked", outputCapacityBlocked);
        tree.SetInt("activeInputSlotId", activeInputSlotId);
        tree.SetInt("activeInputBatchSize", activeInputBatchSize);
        tree.SetLong("deconstructionStartedAtMs", deconstructionStartedAtMs);
        tree.SetLong("deconstructionCompleteAtMs", deconstructionCompleteAtMs);
        tree.SetLong("queuePauseUntilMs", queuePauseUntilMs);
        tree.SetLong("shutdownVisualUntilMs", shutdownVisualUntilMs);
        tree.SetLong("clientSoundCueId", clientSoundCueId);
        tree.SetString("clientSoundCuePath", clientSoundCuePath);
        tree["salvageRemainders"] = salvageRemainders;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);

        string targetText = GetActiveInputSlot()?.Itemstack?.GetName()
            ?? (FindFirstOccupiedInputSlotId() >= 0 ? "Queued items ready" : "Empty");
        sb.AppendLine($"Deconstruction queue: {CountQueuedInputItems()} item(s)");
        sb.AppendLine($"Active item: {targetText}");
        sb.AppendLine($"Temporal dust: {FuelSlot.StackSize}");

        if (isDeconstructing)
        {
            sb.AppendLine("Deconstruction in progress...");
            sb.AppendLine($"Time remaining: {Math.Max(0, (deconstructionCompleteAtMs - Api.World.ElapsedMilliseconds + 999) / 1000)}s");
        }
        else if (IsShutdownVisualActive())
        {
            sb.AppendLine("Deconstruction complete. Powering down...");
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (Api?.Side == EnumAppSide.Client)
        {
            toggleInventoryDialogClient(byPlayer, () =>
            {
                ICoreClientAPI capi = (ICoreClientAPI)Api;
                return new GuiDialogBlockEntityTemporalDeconstructorDevice(
                    Lang.Get(dialogTitleLangCode),
                    Inventory,
                    Pos,
                    capi);
            });
        }

        return true;
    }

    public string GetStatusText()
    {
        int slotId = isDeconstructing && activeInputSlotId >= 0 ? activeInputSlotId : FindFirstOccupiedInputSlotId();
        ItemStack? inputStack = slotId >= 0 ? Inventory[slotId].Itemstack : null;
        if (inputStack == null)
        {
            return string.Empty;
        }

        if (IsRestoredToyCandidate(inputStack))
        {
            return "The device hums at the toy, then thinks better of it. Some little histories are better left unbroken.";
        }

        if (!IsDeconstructableCandidate(inputStack))
        {
            return "The device cannot find a stable point in this item's timeline.";
        }

        if (!TryResolveDeconstructionJob(inputStack, requireAvailableBatch: false, out DeconstructionJob? job))
        {
            return "Its past is too tangled for the device to unwind safely.";
        }

        if (isDeconstructing)
        {
            return "Deconstruction in progress...";
        }

        if (IsQueuePauseActive())
        {
            return "Preparing next item...";
        }

        if (outputCapacityBlocked)
        {
            return "Output inventory is full for the next job. Remove reclaimed items to continue.";
        }

        if (!HasTemporalDustFuel)
        {
            return $"Add {TemporalDustFuelCost} temporal dust to power deconstruction.";
        }

        return "Ready for deconstruction.";
    }

    private static bool IsRestoredToyCandidate(ItemStack stack)
    {
        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        return path.StartsWith("restored-toy-", StringComparison.OrdinalIgnoreCase);
    }

    private bool CanContinueCurrentJob()
    {
        if (activeInputSlotId < 0 || activeInputSlotId >= InputSlotCount)
        {
            return false;
        }

        ItemStack? inputStack = Inventory[activeInputSlotId].Itemstack;
        if (inputStack == null)
        {
            return false;
        }

        return TryResolveDeconstructionJob(inputStack, requireAvailableBatch: true, out _);
    }

    private void CompleteDeconstruction()
    {
        ItemSlot? inputSlot = GetActiveInputSlot();
        ItemStack? inputStack = inputSlot?.Itemstack;
        if (inputStack == null || !TryResolveDeconstructionJob(inputStack, requireAvailableBatch: true, out DeconstructionJob? job))
        {
            StopDeconstruction(playShutdown: false);
            return;
        }

        if (!CanStoreOutputs(inputStack, job!.OutputStacks))
        {
            outputCapacityBlocked = true;
            deconstructionCompleteAtMs = Api!.World.ElapsedMilliseconds + ProgressTickMs;
            MarkDirty(true);
            return;
        }

        inputSlot!.TakeOut(job!.ConsumedInputCount);
        inputSlot.MarkDirty();
        ApplySalvageRemainders(job.UpdatedRemainders);
        StoreDeconstructionOutputs(job.OutputStacks);

        bool hasQueuedFollowup = FindNextProcessableSlotId() >= 0 && HasTemporalDustFuel;
        if (hasQueuedFollowup)
        {
            StopDeconstruction(playShutdown: false);
            BeginSwitchPause();
            return;
        }

        BeginShutdownSequence();
    }

    private void StopDeconstruction(bool playShutdown = true)
    {
        isDeconstructing = false;
        activeInputSlotId = -1;
        activeInputBatchSize = 1;
        deconstructionStartedAtMs = 0;
        deconstructionCompleteAtMs = 0;
        UnregisterProgressListener();
        if (!IsShutdownVisualActive())
        {
            UpdateVisualState(false);
        }
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
        MarkDirty(true);
    }

    private void BeginShutdownSequence()
    {
        shutdownVisualUntilMs = Api?.World.ElapsedMilliseconds + ShutdownEffectDurationMs ?? 0;
        StopDeconstruction(playShutdown: false);
        QueueClientSoundCue("machine-shutoff");

        UpdateVisualState(true);
        UpdateClientParticleListener();
        UpdateServerPhaseListener();
        MarkDirty(true);
    }

    private void FinalizeShutdown()
    {
        shutdownVisualUntilMs = 0;
        ClearQueuedClientSoundCue();
        UpdateVisualState(false);
        UpdateClientParticleListener();
        UpdateServerPhaseListener();
        MarkDirty(true);
        EvaluateDeconstructionState();
    }

    private void OnInventorySlotModified(int slotId)
    {
        if (suppressInventoryChanged)
        {
            return;
        }

        if (Api?.Side == EnumAppSide.Server
            && isDeconstructing
            && slotId == activeInputSlotId
            && !CanContinueCurrentJob())
        {
            outputCapacityBlocked = false;
            BeginShutdownSequence();
            return;
        }

        if (Api?.Side == EnumAppSide.Server && !isDeconstructing && HasTemporalDustFuel && FindNextProcessableSlotId() >= 0)
        {
            outputCapacityBlocked = false;
            if (shutdownVisualUntilMs > 0)
            {
                shutdownVisualUntilMs = 0;
                ClearQueuedClientSoundCue();
                UpdateVisualState(false);
                UpdateClientParticleListener();
            }
        }

        EvaluateDeconstructionState();
    }

    private void EvaluateDeconstructionState()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        outputCapacityBlocked = HasTemporalDustFuel
            && FindNextProcessableSlotId() < 0
            && FindNextProcessableSlotIdIgnoringOutputCapacity() >= 0;

        if (isDeconstructing || IsQueuePauseActive() || IsShutdownVisualActive() || !HasTemporalDustFuel || queueResumeCallbackId != 0)
        {
            MarkDirty(true);
            return;
        }

        if (FindNextProcessableSlotId() >= 0)
        {
            outputCapacityBlocked = false;
            BeginSwitchPause();
            return;
        }

        MarkDirty(true);
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        if (phaseStateListenerId != 0)
        {
            UnregisterGameTickListener(phaseStateListenerId);
            phaseStateListenerId = 0;
        }
        if (deferredResumeListenerId != 0)
        {
            UnregisterGameTickListener(deferredResumeListenerId);
            deferredResumeListenerId = 0;
        }
        queueResumeCallbackId = 0;
        DisposeMachineLoopSound();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        if (phaseStateListenerId != 0)
        {
            UnregisterGameTickListener(phaseStateListenerId);
            phaseStateListenerId = 0;
        }
        if (deferredResumeListenerId != 0)
        {
            UnregisterGameTickListener(deferredResumeListenerId);
            deferredResumeListenerId = 0;
        }
        queueResumeCallbackId = 0;
        DisposeMachineLoopSound();
    }

    public override void OnExchanged(Block block)
    {
        base.OnExchanged(block);

        if (isDeconstructing && Api?.Side == EnumAppSide.Server && progressListenerId == 0)
        {
            RegisterProgressListener();
        }

        UpdateServerPhaseListener();
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
    }

    private void RegisterProgressListener()
    {
        UnregisterProgressListener();
        progressListenerId = RegisterGameTickListener(OnProgressTick, ProgressTickMs, 0);
    }

    private void UpdateServerPhaseListener()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        bool needsPhaseListener = queuePauseUntilMs > 0 || shutdownVisualUntilMs > 0;
        if (needsPhaseListener)
        {
            if (phaseStateListenerId == 0)
            {
                phaseStateListenerId = RegisterGameTickListener(OnServerPhaseTick, PhaseStateTickMs, 0);
            }

            return;
        }

        if (phaseStateListenerId != 0)
        {
            UnregisterGameTickListener(phaseStateListenerId);
            phaseStateListenerId = 0;
        }
    }

    private void OnServerPhaseTick(float dt)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        long now = Api.World.ElapsedMilliseconds;

        if (queuePauseUntilMs > 0 && now >= queuePauseUntilMs)
        {
            queuePauseUntilMs = 0;
            queueResumeCallbackId = 0;
            MarkDirty(true);
            TryStartNextDeconstruction();
        }

        if (shutdownVisualUntilMs > 0 && now >= shutdownVisualUntilMs)
        {
            FinalizeShutdown();
            return;
        }

        UpdateServerPhaseListener();
    }

    private void UnregisterProgressListener()
    {
        if (progressListenerId == 0)
        {
            return;
        }

        UnregisterGameTickListener(progressListenerId);
        progressListenerId = 0;
    }

    private void OnProgressTick(float dt)
    {
        if (Api?.Side != EnumAppSide.Server || !isDeconstructing)
        {
            return;
        }

        UpdateVisualState(true);

        if (Api.World.ElapsedMilliseconds >= deconstructionCompleteAtMs)
        {
            CompleteDeconstruction();
        }
    }

    private ItemSlot? GetActiveInputSlot()
    {
        return activeInputSlotId >= 0 && activeInputSlotId < InputSlotCount ? Inventory[activeInputSlotId] : null;
    }

    private int FindNextProcessableSlotId()
    {
        for (int i = 0; i < InputSlotCount; i++)
        {
            ItemStack? stack = Inventory[i].Itemstack;
            if (stack == null)
            {
                continue;
            }

            if (TryResolveDeconstructionJob(stack, requireAvailableBatch: true, out DeconstructionJob? job)
                && job != null
                && CanStoreOutputs(stack, job.OutputStacks))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindNextProcessableSlotIdIgnoringOutputCapacity()
    {
        for (int i = 0; i < InputSlotCount; i++)
        {
            ItemStack? stack = Inventory[i].Itemstack;
            if (stack == null)
            {
                continue;
            }

            if (TryResolveDeconstructionJob(stack, requireAvailableBatch: true, out _))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindFirstOccupiedInputSlotId()
    {
        for (int i = 0; i < InputSlotCount; i++)
        {
            if (Inventory[i].Itemstack != null)
            {
                return i;
            }
        }

        return -1;
    }

    private int CountQueuedInputItems()
    {
        int count = 0;
        for (int i = 0; i < InputSlotCount; i++)
        {
            if (Inventory[i].Itemstack != null)
            {
                count++;
            }
        }

        return count;
    }

    private void UpdateVisualState(bool running)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        string currentPath = Block.Code.Path;
        string desiredSegment = running ? GetRunningVisualStateSegment() : "-idle-";
        if (currentPath.Contains(desiredSegment, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string updatedPath = ReplaceVisualStateSegment(currentPath, desiredSegment);
        Block? targetBlock = Api.World.GetBlock(new AssetLocation(Block.Code.Domain, updatedPath));
        if (targetBlock == null || targetBlock.Id == Block.Id)
        {
            return;
        }

        Api.World.BlockAccessor.ExchangeBlock(targetBlock.Id, Pos);
    }

    private string GetRunningVisualStateSegment()
    {
        if (deconstructionStartedAtMs <= 0 || deconstructionCompleteAtMs <= deconstructionStartedAtMs)
        {
            return "-running0-";
        }

        long effectiveCompleteAtMs = Math.Max(deconstructionStartedAtMs + 1, deconstructionCompleteAtMs - CompletionHoldDurationMs);
        double progress = GameMath.Clamp(
            (Api!.World.ElapsedMilliseconds - deconstructionStartedAtMs) / (double)(effectiveCompleteAtMs - deconstructionStartedAtMs),
            0d,
            1d);

        if (progress >= 0.999d) return "-running100-";
        if (progress >= 0.8d) return "-running80-";
        if (progress >= 0.6d) return "-running60-";
        if (progress >= 0.4d) return "-running40-";
        if (progress >= 0.2d) return "-running20-";
        return "-running0-";
    }

    private static string ReplaceVisualStateSegment(string path, string desiredSegment)
    {
        string updated = path.Replace("-idle-", desiredSegment, StringComparison.OrdinalIgnoreCase);
        foreach (string state in RunningVisualStates)
        {
            updated = updated.Replace(state, desiredSegment, StringComparison.OrdinalIgnoreCase);
        }

        return updated;
    }

    private void TrySpawnRunningParticles()
    {
        long now = Api.World.ElapsedMilliseconds;
        if (now < nextVisualPulseAtMs)
        {
            return;
        }

        nextVisualPulseAtMs = now + VisualPulseIntervalMs;

        Vec3d center = Pos.ToVec3d().Add(0.5, 1.02, 0.5);
        Api.World.SpawnParticles(
            6f,
            unchecked((int)0xC817C693),
            center.AddCopy(-0.12, -0.01, -0.12),
            center.AddCopy(0.12, 0.06, 0.12),
            new Vec3f(-0.18f, 0.04f, -0.18f),
            new Vec3f(0.18f, 0.22f, 0.18f),
            0.95f,
            0f,
            0.078f,
            EnumParticleModel.Quad
        );

        Api.World.SpawnParticles(
            5f,
            unchecked((int)0xC82DDBA8),
            center.AddCopy(-0.12, -0.01, -0.12),
            center.AddCopy(0.12, 0.06, 0.12),
            new Vec3f(-0.18f, 0.04f, -0.18f),
            new Vec3f(0.18f, 0.22f, 0.18f),
            0.9f,
            0f,
            0.078f,
            EnumParticleModel.Quad
        );

        Api.World.SpawnParticles(
            4f,
            unchecked((int)0xC8129B74),
            center.AddCopy(-0.12, -0.01, -0.12),
            center.AddCopy(0.12, 0.06, 0.12),
            new Vec3f(-0.18f, 0.04f, -0.18f),
            new Vec3f(0.18f, 0.22f, 0.18f),
            0.85f,
            0f,
            0.072f,
            EnumParticleModel.Quad
        );
    }

    private void UpdateClientParticleListener()
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return;
        }

        if (ShouldDisplayRunningEffects())
        {
            if (visualParticleListenerId == 0)
            {
                visualParticleListenerId = RegisterGameTickListener(_ =>
                {
                    TrySpawnRunningParticles();
                    UpdateMachineLoopRuntimeState();
                }, VisualPulseIntervalMs, 0);
            }

            return;
        }

        if (visualParticleListenerId != 0)
        {
            UnregisterGameTickListener(visualParticleListenerId);
            visualParticleListenerId = 0;
        }
    }

    private void UpdateClientMachineLoopSound()
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return;
        }

        if (!isDeconstructing)
        {
            if (machineLoopSound?.IsPlaying == true)
            {
                machineLoopSound.Stop();
            }

            return;
        }

        ICoreClientAPI capi = (ICoreClientAPI)Api;
        machineLoopSound ??= capi.World.LoadSound(new SoundParams
        {
            Location = new AssetLocation("vstemporalreverser", "sounds/machine-loop"),
            Position = new Vec3f(Pos.X + 0.5f, Pos.Y + 0.7f, Pos.Z + 0.5f),
            ShouldLoop = true,
            DisposeOnFinish = false,
            ReferenceDistance = 1.5f,
            Range = MachineLoopFadeDistance,
            Volume = MachineLoopBaseVolume
        });

        if (machineLoopSound is { IsDisposed: false, IsPlaying: false })
        {
            machineLoopSound.Start();
        }

        UpdateMachineLoopRuntimeState();
    }

    private void UpdateMachineLoopRuntimeState()
    {
        if (Api?.Side != EnumAppSide.Client || machineLoopSound == null || machineLoopSound.IsDisposed)
        {
            return;
        }

        Vec3f soundPos = new(Pos.X + 0.5f, Pos.Y + 0.7f, Pos.Z + 0.5f);
        machineLoopSound.SetPosition(soundPos);

        ICoreClientAPI capi = (ICoreClientAPI)Api;
        Vec3d? playerPos = capi.World.Player?.Entity?.Pos?.XYZ;
        if (playerPos == null)
        {
            machineLoopSound.SetVolume(MachineLoopBaseVolume);
            return;
        }

        float fade = GetClientDistanceFade(soundPos);
        float fadeIn = GameMath.Clamp((Api.World.ElapsedMilliseconds - deconstructionStartedAtMs) / (MachineLoopFadeInSeconds * 1000f), 0f, 1f);
        machineLoopSound.SetVolume(MachineLoopBaseVolume * fade * fadeIn);
    }

    private void DisposeMachineLoopSound()
    {
        if (machineLoopSound == null)
        {
            return;
        }

        if (machineLoopSound.IsPlaying)
        {
            machineLoopSound.Stop();
        }

        if (machineLoopSound is IDisposable disposable)
        {
            disposable.Dispose();
        }

        machineLoopSound = null;
    }

    private void BeginSwitchPause()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (queueResumeCallbackId != 0 || IsQueuePauseActive())
        {
            return;
        }

        queuePauseUntilMs = Api.World.ElapsedMilliseconds + SwitchItemPauseDurationMs;
        queueResumeCallbackId = 1;
        QueueClientSoundCue("switch-items");
        UpdateServerPhaseListener();
        MarkDirty(true);
    }

    private void TryStartNextDeconstruction()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isDeconstructing || IsQueuePauseActive() || IsShutdownVisualActive() || !HasTemporalDustFuel)
        {
            return;
        }

        int nextSlotId = FindNextProcessableSlotId();
        if (nextSlotId < 0)
        {
            return;
        }

        ItemStack inputStack = Inventory[nextSlotId].Itemstack!;
        if (!TryResolveDeconstructionJob(inputStack, requireAvailableBatch: true, out DeconstructionJob? job))
        {
            return;
        }

        if (job == null || !CanStoreOutputs(inputStack, job.OutputStacks))
        {
            outputCapacityBlocked = true;
            MarkDirty(true);
            return;
        }

        activeInputSlotId = nextSlotId;
        activeInputBatchSize = job!.ConsumedInputCount;
        temporalStabilityLost = false;
        outputCapacityBlocked = false;
        isDeconstructing = true;
        deconstructionStartedAtMs = Api.World.ElapsedMilliseconds;
        deconstructionCompleteAtMs = deconstructionStartedAtMs + DeconstructionDurationMs + CompletionHoldDurationMs;

        suppressInventoryChanged = true;
        try
        {
            FuelSlot.TakeOut(TemporalDustFuelCost);
            FuelSlot.MarkDirty();
        }
        finally
        {
            suppressInventoryChanged = false;
        }

        UpdateVisualState(true);
        RegisterProgressListener();
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
        MarkDirty(true);
    }

    private void QueueClientSoundCue(string soundPath)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        clientSoundCueId++;
        clientSoundCuePath = soundPath;
        MarkDirty(true);
    }

    private void ClearQueuedClientSoundCue()
    {
        clientSoundCueId++;
        clientSoundCuePath = string.Empty;
    }

    private bool IsQueuePauseActive()
    {
        return queuePauseUntilMs > 0 && (Api?.World.ElapsedMilliseconds ?? 0) < queuePauseUntilMs;
    }

    private void PlayOneShotSoundClient(string soundPath, float volume)
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return;
        }

        ICoreClientAPI capi = (ICoreClientAPI)Api;
        Vec3f soundPos = new(Pos.X + 0.5f, Pos.Y + 0.7f, Pos.Z + 0.5f);
        ILoadedSound? sound = capi.World.LoadSound(new SoundParams
        {
            Location = new AssetLocation("vstemporalreverser", $"sounds/{soundPath}"),
            Position = soundPos,
            DisposeOnFinish = true,
            ReferenceDistance = 1.5f,
            Range = MachineLoopFadeDistance,
            Volume = volume * GetClientDistanceFade(soundPos)
        });

        sound?.Start();
    }

    private void TryPlayQueuedClientSound()
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return;
        }

        if (clientSoundCueId <= 0 || clientSoundCueId == lastHandledClientSoundCueId || string.IsNullOrWhiteSpace(clientSoundCuePath))
        {
            return;
        }

        float volume = clientSoundCuePath switch
        {
            "switch-items" => MachineLoopBaseVolume * 1.5f,
            "click" => MachineLoopBaseVolume * 0.75f,
            "machine-shutoff" => MachineLoopBaseVolume,
            _ => MachineLoopBaseVolume
        };

        PlayOneShotSoundClient(clientSoundCuePath, volume);
        lastHandledClientSoundCueId = clientSoundCueId;
    }

    private float GetClientDistanceFade(Vec3f soundPos)
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return 1f;
        }

        ICoreClientAPI capi = (ICoreClientAPI)Api;
        Vec3d? playerPos = capi.World.Player?.Entity?.Pos?.XYZ;
        if (playerPos == null)
        {
            return 1f;
        }

        double distance = playerPos.DistanceTo(soundPos.X, soundPos.Y, soundPos.Z);
        return GameMath.Clamp(1f - (float)(distance / MachineLoopFadeDistance), 0f, 1f);
    }

    private bool ShouldDisplayRunningEffects()
    {
        return isDeconstructing || IsShutdownVisualActive();
    }

    private bool IsShutdownVisualActive()
    {
        return shutdownVisualUntilMs > 0 && (Api?.World.ElapsedMilliseconds ?? 0) < shutdownVisualUntilMs;
    }

    private void QueueDeferredDeconstructionResume()
    {
        if (Api?.Side != EnumAppSide.Server || deferredResumeListenerId != 0)
        {
            return;
        }

        WriteDeconstructionDebugEvent("resume-queued", GetResumeDebugStack(), null, null, null, null);
        deferredResumeListenerId = RegisterGameTickListener(_ =>
        {
            if (deferredResumeListenerId != 0)
            {
                UnregisterGameTickListener(deferredResumeListenerId);
                deferredResumeListenerId = 0;
            }

            WriteDeconstructionDebugEvent("resume-deferred-tick", GetResumeDebugStack(), null, null, null, null);
            ResumePersistedDeconstructionState();
            UpdateServerPhaseListener();
            if (!isDeconstructing)
            {
                EvaluateDeconstructionState();
            }
        }, 0, 1);
    }

    private void ResumePersistedDeconstructionState()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        long now = Api.World.ElapsedMilliseconds;

        if (shutdownVisualUntilMs > now + ShutdownEffectDurationMs)
        {
            shutdownVisualUntilMs = 0;
            UpdateVisualState(false);
            UpdateClientParticleListener();
            WriteDeconstructionDebugEvent("resume-cleared-stale-shutdown-visual", GetResumeDebugStack(), null, null, null, null);
            MarkDirty(true);
        }

        if (queuePauseUntilMs > now + SwitchItemPauseDurationMs)
        {
            queuePauseUntilMs = 0;
            queueResumeCallbackId = 0;
            WriteDeconstructionDebugEvent("resume-cleared-stale-queue-pause", GetResumeDebugStack(), null, null, null, null);
            MarkDirty(true);
        }

        if (shutdownVisualUntilMs > 0 && now >= shutdownVisualUntilMs)
        {
            WriteDeconstructionDebugEvent("resume-finalize-shutdown", GetResumeDebugStack(), null, null, null, null);
            FinalizeShutdown();
            return;
        }

        if (queuePauseUntilMs > 0)
        {
            if (now >= queuePauseUntilMs)
            {
                queuePauseUntilMs = 0;
                queueResumeCallbackId = 0;
                WriteDeconstructionDebugEvent("resume-expired-queue-pause", GetResumeDebugStack(), null, null, null, null);
                MarkDirty(true);
            }
            else
            {
                queueResumeCallbackId = 1;
                WriteDeconstructionDebugEvent("resume-queue-pause-active", GetResumeDebugStack(), null, null, null, null);
            }
        }

        if (!isDeconstructing)
        {
            if (queuePauseUntilMs > 0 || queueResumeCallbackId != 0)
            {
                queuePauseUntilMs = 0;
                queueResumeCallbackId = 0;
                WriteDeconstructionDebugEvent("resume-cleared-idle-queue-pause", GetResumeDebugStack(), null, null, null, null);
                MarkDirty(true);
            }

            WriteDeconstructionDebugEvent("resume-no-active-job", GetResumeDebugStack(), null, null, null, null);
            return;
        }

        if (activeInputSlotId < 0 || activeInputSlotId >= InputSlotCount || Inventory[activeInputSlotId].Itemstack == null)
        {
            WriteDeconstructionDebugEvent("resume-missing-active-slot", GetResumeDebugStack(), null, null, null, null);
            StopDeconstruction(playShutdown: false);
            return;
        }

        if (deconstructionStartedAtMs <= 0
            || deconstructionCompleteAtMs <= deconstructionStartedAtMs
            || now < deconstructionStartedAtMs)
        {
            deconstructionStartedAtMs = now;
            deconstructionCompleteAtMs = now + DeconstructionDurationMs + CompletionHoldDurationMs;
            WriteDeconstructionDebugEvent("resume-timer-rebased", GetResumeDebugStack(), null, null, null, null);
        }

        RegisterProgressListener();
        UpdateVisualState(true);
        MarkDirty(true);
    }

    private ItemStack GetResumeDebugStack()
    {
        if (activeInputSlotId >= 0 && activeInputSlotId < InputSlotCount && Inventory[activeInputSlotId].Itemstack != null)
        {
            return Inventory[activeInputSlotId].Itemstack!;
        }

        CollectibleObject? collectible = Api?.World.GetItem(new AssetLocation("vstemporalreverser", "temporal-dust"));
        collectible ??= Api?.World.GetBlock(new AssetLocation("game", "air"));
        return new ItemStack(collectible);
    }

    private bool CanStoreOutputs(ItemStack sourceStack, IEnumerable<ItemStack> outputs)
    {
        return TryStoreOutputs(outputs, false, sourceStack);
    }

    private void StoreDeconstructionOutputs(IEnumerable<ItemStack> outputs)
    {
        if (!TryStoreOutputs(outputs, true, null))
        {
            throw new InvalidOperationException("Unable to store deconstruction outputs despite passing output-space validation.");
        }
    }

    private bool TryStoreOutputs(IEnumerable<ItemStack> outputs, bool applyChanges, ItemStack? sourceStack)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return false;
        }

        ItemStack?[] simulatedStacks = new ItemStack?[OutputSlotCount];
        for (int i = 0; i < OutputSlotCount; i++)
        {
            simulatedStacks[i] = Inventory[OutputSlotStart + i].Itemstack?.Clone();
        }

        foreach (ItemStack output in outputs)
        {
            if (!TryInsertIntoOutputBuffer(simulatedStacks, output, sourceStack))
            {
                return false;
            }
        }

        if (!applyChanges)
        {
            return true;
        }

        for (int i = 0; i < OutputSlotCount; i++)
        {
            Inventory[OutputSlotStart + i].Itemstack = simulatedStacks[i]?.Clone();
            Inventory[OutputSlotStart + i].MarkDirty();
        }

        MarkDirty(true);
        return true;
    }

    private bool TryInsertIntoOutputBuffer(ItemStack?[] simulatedStacks, ItemStack output, ItemStack? sourceStack)
    {
        ItemStack pendingStack = output.Clone();
        int remaining = pendingStack.StackSize;
        string pendingKey = GetStackKey(pendingStack);

        for (int i = 0; i < simulatedStacks.Length && remaining > 0; i++)
        {
            ItemStack? existing = simulatedStacks[i];
            if (existing == null || !GetStackKey(existing).Equals(pendingKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int maxStackSize = existing.Collectible?.MaxStackSize ?? pendingStack.Collectible?.MaxStackSize ?? remaining;
            int space = Math.Max(0, maxStackSize - existing.StackSize);
            if (space <= 0)
            {
                continue;
            }

            int moved = Math.Min(space, remaining);
            existing.StackSize += moved;
            remaining -= moved;
        }

        for (int i = 0; i < simulatedStacks.Length && remaining > 0; i++)
        {
            if (simulatedStacks[i] != null)
            {
                continue;
            }

            int maxStackSize = pendingStack.Collectible?.MaxStackSize ?? remaining;
            int moved = Math.Min(maxStackSize, remaining);
            ItemStack inserted = pendingStack.Clone();
            inserted.StackSize = moved;
            simulatedStacks[i] = inserted;
            remaining -= moved;
        }

        if (remaining > 0)
        {
            WriteDeconstructionDebugEvent(
                "output-capacity-failed",
                sourceStack ?? pendingStack,
                null,
                null,
                new List<ItemStack> { pendingStack.Clone() },
                CreateOutputCapacityDebugRemainders(simulatedStacks, remaining));
        }

        return remaining <= 0;
    }

    private Dictionary<string, double> CreateOutputCapacityDebugRemainders(ItemStack?[] simulatedStacks, int remaining)
    {
        Dictionary<string, double> data = new(StringComparer.OrdinalIgnoreCase)
        {
            ["remaining"] = remaining
        };

        for (int i = 0; i < simulatedStacks.Length; i++)
        {
            ItemStack? stack = simulatedStacks[i];
            if (stack == null)
            {
                continue;
            }

            data[$"slot{OutputSlotStart + i}"] = stack.StackSize;
        }

        return data;
    }

    private bool TryResolveDeconstructionJob(ItemStack stack, bool requireAvailableBatch, out DeconstructionJob? job)
    {
        job = null;

        if (!IsDeconstructableCandidate(stack)
            || (!IsExplicitlyAllowedDeconstructionOutput(stack) && !IsRestoredSalvageDeconstructionCandidate(stack)))
        {
            WriteDeconstructionDebugEvent("rejected-not-allowed", stack, null, null, null, null);
            return false;
        }

        if (TryResolveSpecialCaseDeconstructionJob(stack, requireAvailableBatch, out job))
        {
            WriteDeconstructionDebugEvent("selected-special-case", stack, null, null, job?.OutputStacks, job?.UpdatedRemainders);
            return true;
        }

        foreach (object recipe in GetGridRecipes())
        {
            if (IsRepairLikeRecipe(recipe))
            {
                WriteDeconstructionDebugEvent("skip-repair-like-recipe", stack, recipe, null, null, null);
                continue;
            }

            if (!TryMatchRecipeOutput(stack, recipe, out Dictionary<string, string> captures, out int inputBatchSize))
            {
                continue;
            }

            if (RecipeConsumesInputItem(stack, recipe, captures))
            {
                WriteDeconstructionDebugEvent("skip-self-consuming-recipe", stack, recipe, captures, null, null);
                continue;
            }

            if (requireAvailableBatch && stack.StackSize <= 0)
            {
                continue;
            }

            int consumedInputCount = Math.Min(Math.Max(1, stack.StackSize), Math.Max(1, inputBatchSize));
            if (consumedInputCount <= 0)
            {
                continue;
            }

            BuildDeconstructionOutputs(stack, recipe, captures, inputBatchSize, consumedInputCount, out List<ItemStack> outputs, out Dictionary<string, double> updatedRemainders);
            if (outputs.Count == 0 && updatedRemainders.Count == 0)
            {
                continue;
            }

            job = new DeconstructionJob
            {
                ConsumedInputCount = consumedInputCount,
                OutputStacks = outputs,
                UpdatedRemainders = updatedRemainders
            };

            WriteDeconstructionDebugEvent("selected-recipe", stack, recipe, captures, outputs, updatedRemainders);
            return true;
        }

        WriteDeconstructionDebugEvent("no-matching-recipe", stack, null, null, null, null);
        return false;
    }

    private bool TryResolveSpecialCaseDeconstructionJob(ItemStack stack, bool requireAvailableBatch, out DeconstructionJob? job)
    {
        job = null;

        if (requireAvailableBatch && stack.StackSize <= 0)
        {
            return false;
        }

        if (TryResolveCuratedRestoredClothingDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveRestoredSalvageDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveNonCraftableFootwearDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveMetalToolDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveHelveHammerHeadDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveFiredPotteryDeconstructionJob(stack, out job))
        {
            return true;
        }

        if (TryResolveAnvilDeconstructionJob(stack, out job))
        {
            return true;
        }

        return false;
    }

    private bool TryResolveMetalToolDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        List<ItemStack> outputs = [];

        if (path.Equals("solderingiron", StringComparison.OrdinalIgnoreCase))
        {
            if (!AddOutputByCode(outputs, "ingot-copper", 1)
                || !AddOutputByCode(outputs, "stick", 1))
            {
                return false;
            }
        }
        else
        {
            if (!IsMetalToolCandidatePath(path)
                || !TryExtractKnownMetalFromPath(path, out string? metal))
            {
                return false;
            }

            if (!AddOutputByCode(outputs, $"ingot-{metal}", 1))
            {
                return false;
            }

            if (RequiresStickHandledToolOutput(path)
                && !AddOutputByCode(outputs, "stick", 1))
            {
                return false;
            }
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private static bool RequiresStickHandledToolOutput(string path)
    {
        return path.StartsWith("axe-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("pickaxe-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("hammer-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("saw-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("shovel-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("hoe-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("knife-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("cleaver-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("spear-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("scythe-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("prospectingpick-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMetalOnlyDurableToolCandidatePath(string path)
    {
        return path.StartsWith("chisel-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("crowbar-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("shears-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("wrench-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("tongsmetal-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMetalToolCandidatePath(string path)
    {
        return RequiresStickHandledToolOutput(path)
            || IsMetalOnlyDurableToolCandidatePath(path);
    }

    private bool TryResolveHelveHammerHeadDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (!path.StartsWith("helvehammerhead-", StringComparison.OrdinalIgnoreCase)
            || !TryExtractKnownMetalFromPath(path, out string? metal))
        {
            return false;
        }

        List<ItemStack> outputs = [];
        if (!AddOutputByCode(outputs, $"ingot-{metal}", 1))
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private bool TryResolveFiredPotteryDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path)
            || !IsFiredPotteryCandidatePath(path)
            || !TryExtractPotteryClayCode(path, out string? clayCode))
        {
            return false;
        }

        List<ItemStack> outputs = [];
        if (!AddOutputByCode(outputs, clayCode!, 1))
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private static bool IsFiredPotteryCandidatePath(string path)
    {
        if (!path.EndsWith("-fired", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return path.StartsWith("bowl-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("clayplanter-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("claypot-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("crock-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("crucible-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("flowerpot-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("storagevessel-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("jug-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("wateringcan-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryExtractPotteryClayCode(string path, out string? clayCode)
    {
        clayCode = null;

        if (path.Contains("-blue-", StringComparison.OrdinalIgnoreCase))
        {
            clayCode = "clay-blue";
            return true;
        }

        if (path.Contains("-brown-", StringComparison.OrdinalIgnoreCase))
        {
            clayCode = "clay-brown";
            return true;
        }

        if (path.Contains("-cream-", StringComparison.OrdinalIgnoreCase))
        {
            clayCode = "clay-fire";
            return true;
        }

        if (path.Contains("-red-", StringComparison.OrdinalIgnoreCase))
        {
            clayCode = "clay-red";
            return true;
        }

        return false;
    }

    private bool TryResolveCuratedRestoredClothingDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (!CuratedRestoredClothingOutputs.TryGetValue(path, out string[]? outputCodes) || outputCodes.Length == 0)
        {
            return false;
        }

        Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);
        foreach (string outputCode in outputCodes)
        {
            if (string.IsNullOrWhiteSpace(outputCode))
            {
                continue;
            }

            counts[outputCode] = counts.TryGetValue(outputCode, out int existing)
                ? existing + 1
                : 1;
        }

        List<ItemStack> outputs = new();
        foreach ((string outputCode, int quantity) in counts)
        {
            CollectibleObject? collectible = Api?.World.GetItem(ToGameAssetLocation(outputCode));
            if (collectible == null)
            {
                continue;
            }

            outputs.Add(new ItemStack(collectible)
            {
                StackSize = quantity
            });
        }

        if (outputs.Count == 0)
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private static bool IsRestoredSalvageDeconstructionCandidate(ItemStack stack)
    {
        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        return path.StartsWith("torchholder-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-normal-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-dim-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chandelier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-canopy-bed-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-short-bed-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-metal-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-agedwood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-largewood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-ornatewood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-ruinedwood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-metal-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-back", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-colored-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-crude", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-ebony", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-long-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-crate-large-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-crate-medium-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-crate-small-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-decoration-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-metal-bed-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-metal-table-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-metal-table-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-table-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-bookstand-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-censer-", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryResolveRestoredSalvageDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (TryResolveRestoredWoodOrLibraryDeconstructionJob(path, out job))
        {
            return true;
        }

        if (TryResolveRestoredBedCrateOrDecorationDeconstructionJob(path, out job))
        {
            return true;
        }

        if (!TryExtractKnownMetalFromPath(path, out string? metal))
        {
            return false;
        }

        int ingotCount;
        int metalPlateCount = 0;
        int mordantClothCount = 0;

        if (path.StartsWith("restored-chair-metal-", StringComparison.OrdinalIgnoreCase))
        {
            ingotCount = 3;
            mordantClothCount = 3;
        }
        else if (path.StartsWith("torchholder-", StringComparison.OrdinalIgnoreCase))
        {
            ingotCount = 0;
            metalPlateCount = 1;
        }
        else if (path.StartsWith("restored-metal-table-", StringComparison.OrdinalIgnoreCase))
        {
            ingotCount = 1;
            mordantClothCount = 3;
        }
        else if (path.StartsWith("restored-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-normal-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-dim-brazier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chandelier-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-metal-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-censer-", StringComparison.OrdinalIgnoreCase))
        {
            ingotCount = 1;
        }
        else
        {
            return false;
        }

        List<ItemStack> outputs = [];
        if (ingotCount > 0)
        {
            CollectibleObject? ingotCollectible = Api?.World.GetItem(new AssetLocation("game", $"ingot-{metal}"));
            if (ingotCollectible == null)
            {
                return false;
            }

            outputs.Add(new ItemStack(ingotCollectible) { StackSize = ingotCount });
        }

        if (metalPlateCount > 0)
        {
            if (VSTemporalReverserModSystem.Config.DeconstructMetalOutputsToIngots)
            {
                CollectibleObject? ingotCollectible = Api?.World.GetItem(new AssetLocation("game", $"ingot-{metal}"));
                if (ingotCollectible == null)
                {
                    return false;
                }

                outputs.Add(new ItemStack(ingotCollectible) { StackSize = metalPlateCount * 2 });
            }
            else
            {
                CollectibleObject? metalPlateCollectible = Api?.World.GetItem(new AssetLocation("game", $"metalplate-{metal}"));
                if (metalPlateCollectible == null)
                {
                    return false;
                }

                outputs.Add(new ItemStack(metalPlateCollectible) { StackSize = metalPlateCount });
            }
        }

        if (mordantClothCount > 0 && Api?.World.GetItem(ToGameAssetLocation("cloth-mordant")) is CollectibleObject clothCollectible)
        {
            outputs.Add(new ItemStack(clothCollectible) { StackSize = mordantClothCount });
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private bool TryResolveRestoredBedCrateOrDecorationDeconstructionJob(string path, out DeconstructionJob? job)
    {
        job = null;

        List<ItemStack> outputs = [];

        if (path.StartsWith("restored-decoration-", StringComparison.OrdinalIgnoreCase))
        {
            if (!AddOutputByCode(outputs, "paper-parchment", 2))
            {
                return false;
            }
        }
        else if (path.StartsWith("restored-crate-large-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-crate-medium-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-crate-small-", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryExtractKnownWoodFromPath(path, out string? crateWood))
            {
                return false;
            }

            int plankCount = path.StartsWith("restored-crate-large-", StringComparison.OrdinalIgnoreCase) ? 5
                : path.StartsWith("restored-crate-medium-", StringComparison.OrdinalIgnoreCase) ? 4
                : 3;
            int nailCount = plankCount;

            if (!AddOutputByCode(outputs, $"plank-{crateWood}", plankCount)
                || !AddOutputByCode(outputs, "metalnailsandstrips-copper", nailCount))
            {
                return false;
            }
        }
        else if (path.StartsWith("restored-canopy-bed-", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryExtractKnownWoodFromPath(path, out string? canopyWood))
            {
                return false;
            }

            if (!AddOutputByCode(outputs, $"plank-{canopyWood}", 10)
                || !AddOutputByCode(outputs, "metalnailsandstrips-copper", 10)
                || !AddOutputByCode(outputs, "cloth-white", 6))
            {
                return false;
            }
        }
        else if (path.StartsWith("restored-short-bed-", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryExtractKnownWoodFromPath(path, out string? shortBedWood))
            {
                return false;
            }

            if (!AddOutputByCode(outputs, $"plank-{shortBedWood}", 5)
                || !AddOutputByCode(outputs, "metalnailsandstrips-copper", 5)
                || !AddOutputByCode(outputs, "cloth-white", 3))
            {
                return false;
            }
        }
        else if (path.StartsWith("restored-metal-bed-", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryExtractKnownMetalFromPath(path, out string? bedMetal))
            {
                return false;
            }

            if (!AddOutputByCode(outputs, $"ingot-{bedMetal}", 4)
                || !AddOutputByCode(outputs, "cloth-white", 3))
            {
                return false;
            }
        }
        else if (path.StartsWith("restored-metal-table-", StringComparison.OrdinalIgnoreCase))
        {
            string[] metals = ExtractAllKnownMetalsFromPath(path);
            if (metals.Length == 0)
            {
                return false;
            }

            foreach (string metal in metals)
            {
                if (!AddOutputByCode(outputs, $"ingot-{metal}", 1))
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private bool TryResolveRestoredWoodOrLibraryDeconstructionJob(string path, out DeconstructionJob? job)
    {
        job = null;

        if (!TryExtractKnownWoodFromPath(path, out string? wood))
        {
            return false;
        }

        int plankCount = 0;
        int nailCount = 0;
        int clothCount = 0;
        int parchmentCount = 0;
        int candleCount = 0;

        if (path.StartsWith("restored-bookstand-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-agedwood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-largewood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-ornatewood-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-lectern-ruinedwood-", StringComparison.OrdinalIgnoreCase))
        {
            plankCount = 5;
            parchmentCount = 2;
        }
        else if (path.StartsWith("restored-table-", StringComparison.OrdinalIgnoreCase))
        {
            plankCount = 5;
            nailCount = 5;
            if (UsesRestoredTableClothSalvage(path))
            {
                clothCount = 3;
            }

            if (path.Contains("scribeaccessories", StringComparison.OrdinalIgnoreCase))
            {
                candleCount = 5;
            }
        }
        else if (path.StartsWith("restored-chair-colored-", StringComparison.OrdinalIgnoreCase))
        {
            plankCount = 4;
            nailCount = 4;
            clothCount = 2;
        }
        else if (path.StartsWith("restored-chair-long-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-back", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-crude", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("restored-chair-ebony", StringComparison.OrdinalIgnoreCase))
        {
            plankCount = 4;
            nailCount = 4;
        }
        else
        {
            return false;
        }

        List<ItemStack> outputs = [];
        if (!AddOutputByCode(outputs, $"plank-{wood}", plankCount))
        {
            return false;
        }

        if (!AddOutputByCode(outputs, "paper-parchment", parchmentCount))
        {
            return false;
        }

        if (!AddOutputByCode(outputs, "cloth-mordant", clothCount))
        {
            return false;
        }

        if (!AddOutputByCode(outputs, "candle", candleCount))
        {
            return false;
        }

        if (nailCount > 0)
        {
            string nailMetal = VSTemporalReverserModSystem.Config.DeconstructMetalOutputsToIngots ? "copper" : "copper";
            if (!AddOutputByCode(outputs, $"metalnailsandstrips-{nailMetal}", nailCount))
            {
                return false;
            }
        }

        if (outputs.Count == 0)
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks = outputs,
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private bool TryResolveNonCraftableFootwearDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        bool isFootwear = path.StartsWith("clothes-foot-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("clothes-nadiya-foot-", StringComparison.OrdinalIgnoreCase);
        if (!isFootwear)
        {
            return false;
        }

        foreach (object recipe in GetGridRecipes())
        {
            if (IsRepairLikeRecipe(recipe))
            {
                continue;
            }

            if (!TryMatchRecipeOutput(stack, recipe, out Dictionary<string, string> captures, out _))
            {
                continue;
            }

            if (RecipeConsumesInputItem(stack, recipe, captures))
            {
                continue;
            }

            return false;
        }

        CollectibleObject? leatherCollectible = Api?.World.GetItem(ToGameAssetLocation("leather-normal-plain"));
        if (leatherCollectible == null)
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks =
            [
                new ItemStack(leatherCollectible) { StackSize = 1 }
            ],
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private bool TryResolveAnvilDeconstructionJob(ItemStack stack, out DeconstructionJob? job)
    {
        job = null;

        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (!TryExtractMetalVariant(path, "anvil-", out string? metal))
        {
            return false;
        }

        CollectibleObject? ingotCollectible = Api?.World.GetItem(new AssetLocation("game", $"ingot-{metal}"));
        if (ingotCollectible == null)
        {
            return false;
        }

        job = new DeconstructionJob
        {
            ConsumedInputCount = 1,
            OutputStacks =
            [
                new ItemStack(ingotCollectible) { StackSize = 8 }
            ],
            UpdatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        };

        return true;
    }

    private static bool TryExtractKnownMetalFromPath(string path, out string? metal)
    {
        string[] knownMetals =
        [
            "copper",
            "brass",
            "blackbronze",
            "bismuthbronze",
            "tinbronze",
            "silver",
            "gold",
            "electrum",
            "iron",
            "meteoriciron",
            "steel",
            "molybdochalkos",
            "bismuth"
        ];

        metal = knownMetals.FirstOrDefault(candidate => path.Contains(candidate, StringComparison.OrdinalIgnoreCase));
        return metal != null;
    }

    private static bool TryExtractKnownWoodFromPath(string path, out string? wood)
    {
        string[] knownWoods =
        [
            "veryaged",
            "purpleheart",
            "baldcypress",
            "mahogany",
            "redwood",
            "acacia",
            "walnut",
            "maple",
            "kapok",
            "larch",
            "ebony",
            "birch",
            "aged",
            "pine",
            "oak"
        ];

        wood = knownWoods.FirstOrDefault(candidate => path.Contains(candidate, StringComparison.OrdinalIgnoreCase));
        return wood != null;
    }

    private static string[] ExtractAllKnownMetalsFromPath(string path)
    {
        string[] knownMetals =
        [
            "copper",
            "brass",
            "blackbronze",
            "bismuthbronze",
            "tinbronze",
            "silver",
            "gold",
            "electrum",
            "iron",
            "meteoriciron",
            "steel",
            "molybdochalkos",
            "bismuth"
        ];

        return knownMetals
            .Where(candidate => path.Contains(candidate, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private bool AddOutputByCode(List<ItemStack> outputs, string code, int quantity)
    {
        if (quantity <= 0)
        {
            return true;
        }

        AssetLocation location = ToGameAssetLocation(code);
        CollectibleObject? collectible = Api?.World.GetItem(location);
        collectible ??= Api?.World.GetBlock(location);
        if (collectible == null)
        {
            return false;
        }

        outputs.Add(new ItemStack(collectible)
        {
            StackSize = quantity
        });

        return true;
    }

    private static bool UsesRestoredTableClothSalvage(string path)
    {
        return path.Contains("agedwhite", StringComparison.OrdinalIgnoreCase)
            || path.Contains("agedblue", StringComparison.OrdinalIgnoreCase)
            || path.Contains("agedgreen", StringComparison.OrdinalIgnoreCase)
            || path.Contains("agedpurple", StringComparison.OrdinalIgnoreCase)
            || path.Contains("agedred", StringComparison.OrdinalIgnoreCase)
            || path.Contains("scribeblue", StringComparison.OrdinalIgnoreCase)
            || path.Contains("scribegreen", StringComparison.OrdinalIgnoreCase)
            || path.Contains("scribepurple", StringComparison.OrdinalIgnoreCase)
            || path.Contains("scribered", StringComparison.OrdinalIgnoreCase)
            || path.Contains("scribeaccessories", StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<object> GetGridRecipes()
    {
        object? recipes = GetMemberValue(Api?.World, "GridRecipes");
        if (recipes is not IEnumerable enumerable)
        {
            yield break;
        }

        foreach (object? recipe in enumerable)
        {
            if (recipe != null)
            {
                yield return recipe;
            }
        }
    }

    private void BuildDeconstructionOutputs(
        ItemStack sourceStack,
        object recipe,
        Dictionary<string, string> captures,
        int recipeOutputBatchSize,
        int consumedInputCount,
        out List<ItemStack> outputs,
        out Dictionary<string, double> updatedRemainders)
    {
        object? ingredientContainer = GetMemberValue(recipe, "RecipeIngredients", "Ingredients");
        Dictionary<string, int> ingredientCounts = GetIngredientSymbolCounts(recipe);
        Dictionary<string, (ItemStack Stack, int Quantity)> aggregated = new(StringComparer.OrdinalIgnoreCase);
        outputs = new List<ItemStack>();
        updatedRemainders = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach ((string? ingredientKey, object ingredient) in EnumerateIngredientEntries(ingredientContainer))
        {
            if (TryGetBoolMember(ingredient, "IsTool") || TryGetBoolMember(ingredient, "Tool"))
            {
                continue;
            }

            if (!TryGetIngredientStack(ingredient, captures, out ItemStack? resolved) || resolved == null)
            {
                continue;
            }

            int baseQuantity = TryGetIntMember(ingredient, "Quantity")
                ?? TryGetIntMember(ingredient, "StackSize")
                ?? Math.Max(1, resolved.StackSize);
            baseQuantity = Math.Max(1, baseQuantity);

            int patternCount = 1;
            if (!string.IsNullOrWhiteSpace(ingredientKey)
                && ingredientCounts.TryGetValue(ingredientKey!, out int matchedCount)
                && matchedCount > 0)
            {
                patternCount = matchedCount;
            }

            int quantity = baseQuantity * patternCount;

            if (TryFlattenArmorIngredient(sourceStack, resolved, quantity, aggregated))
            {
                continue;
            }

            AddAggregatedStackForSource(sourceStack, aggregated, resolved, quantity);
        }

        foreach ((ItemStack stack, int quantity) in aggregated.Values)
        {
            double scaledRefundQuantity = quantity * (consumedInputCount / (double)Math.Max(1, recipeOutputBatchSize));
            string key = GetStackKey(stack);
            double totalRefundQuantity = GetSalvageRemainder(key) + scaledRefundQuantity;
            int wholeRefundQuantity = (int)Math.Floor(totalRefundQuantity + 1e-9);
            double fractionalRefundQuantity = totalRefundQuantity - wholeRefundQuantity;

            if (wholeRefundQuantity > 0)
            {
                ItemStack refundStack = stack.Clone();
                refundStack.StackSize = wholeRefundQuantity;
                refundStack = NormalizeArmorRefundStack(sourceStack, refundStack);
                outputs.Add(refundStack);
            }

            if (fractionalRefundQuantity > 1e-9)
            {
                if (ShouldConvertMetalOutputToIngots(sourceStack, stack)
                    && TryCreatePartialMetalBitRefund(stack, fractionalRefundQuantity, out ItemStack? metalBitStack, out string? metalBitRemainderKey, out double metalBitRemainder))
                {
                    if (metalBitStack != null)
                    {
                        outputs.Add(metalBitStack);
                    }

                    if (!string.IsNullOrWhiteSpace(metalBitRemainderKey) && metalBitRemainder > 1e-9)
                    {
                        updatedRemainders[metalBitRemainderKey!] = metalBitRemainder;
                    }
                }
                else
                {
                    updatedRemainders[key] = fractionalRefundQuantity;
                }
            }
        }
    }

    private bool TryFlattenArmorIngredient(
        ItemStack sourceStack,
        ItemStack ingredientStack,
        int quantity,
        Dictionary<string, (ItemStack Stack, int Quantity)> aggregated)
    {
        if (!VSTemporalReverserModSystem.Config.DeconstructMetalOutputsToIngots)
        {
            return false;
        }

        if (!IsArmorStack(sourceStack) || !IsArmorStack(ingredientStack) || quantity <= 0)
        {
            return false;
        }

        return TryFlattenArmorIngredientRecursive(ingredientStack, quantity, aggregated, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    private bool TryFlattenArmorIngredientRecursive(
        ItemStack ingredientStack,
        int quantity,
        Dictionary<string, (ItemStack Stack, int Quantity)> aggregated,
        HashSet<string> visited)
    {
        if (quantity <= 0)
        {
            return false;
        }

        string stackKey = GetStackKey(ingredientStack);
        if (!visited.Add(stackKey))
        {
            AddAggregatedStack(aggregated, ingredientStack, quantity);
            return true;
        }

        if (!TryResolveArmorCraftRecipe(ingredientStack, out object? armorRecipe, out Dictionary<string, string>? captures) || armorRecipe == null || captures == null)
        {
            AddAggregatedStack(aggregated, ingredientStack, quantity);
            return true;
        }

        object? ingredientContainer = GetMemberValue(armorRecipe, "RecipeIngredients", "Ingredients");
        Dictionary<string, int> ingredientCounts = GetIngredientSymbolCounts(armorRecipe);
        bool flattenedAny = false;

        foreach ((string? ingredientKey, object ingredient) in EnumerateIngredientEntries(ingredientContainer))
        {
            if (TryGetBoolMember(ingredient, "IsTool") || TryGetBoolMember(ingredient, "Tool"))
            {
                continue;
            }

            if (!TryGetIngredientStack(ingredient, captures, out ItemStack? resolved) || resolved == null)
            {
                continue;
            }

            int baseQuantity = TryGetIntMember(ingredient, "Quantity")
                ?? TryGetIntMember(ingredient, "StackSize")
                ?? Math.Max(1, resolved.StackSize);
            baseQuantity = Math.Max(1, baseQuantity);

            int patternCount = 1;
            if (!string.IsNullOrWhiteSpace(ingredientKey)
                && ingredientCounts.TryGetValue(ingredientKey!, out int matchedCount)
                && matchedCount > 0)
            {
                patternCount = matchedCount;
            }

            int expandedQuantity = quantity * baseQuantity * patternCount;
            if (IsArmorStack(resolved))
            {
                TryFlattenArmorIngredientRecursive(resolved, expandedQuantity, aggregated, visited);
            }
            else
            {
                AddAggregatedStackForSource(ingredientStack, aggregated, resolved, expandedQuantity);
            }

            flattenedAny = true;
        }

        if (!flattenedAny)
        {
            AddAggregatedStack(aggregated, ingredientStack, quantity);
        }

        visited.Remove(stackKey);
        return true;
    }

    private bool TryResolveArmorCraftRecipe(ItemStack stack, out object? recipe, out Dictionary<string, string>? captures)
    {
        recipe = null;
        captures = null;

        foreach (object candidate in GetGridRecipes())
        {
            if (IsRepairLikeRecipe(candidate))
            {
                continue;
            }

            if (!TryMatchRecipeOutput(stack, candidate, out Dictionary<string, string> matchedCaptures, out _))
            {
                continue;
            }

            if (RecipeConsumesInputItem(stack, candidate, matchedCaptures))
            {
                continue;
            }

            recipe = candidate;
            captures = matchedCaptures;
            return true;
        }

        return false;
    }

    private static void AddAggregatedStack(Dictionary<string, (ItemStack Stack, int Quantity)> aggregated, ItemStack stack, int quantity)
    {
        string key = GetStackKey(stack);
        if (aggregated.TryGetValue(key, out (ItemStack Stack, int Quantity) entry))
        {
            aggregated[key] = (entry.Stack, entry.Quantity + quantity);
        }
        else
        {
            ItemStack template = stack.Clone();
            template.StackSize = 1;
            aggregated[key] = (template, quantity);
        }
    }

    private void AddAggregatedStackForSource(
        ItemStack sourceStack,
        Dictionary<string, (ItemStack Stack, int Quantity)> aggregated,
        ItemStack stack,
        int quantity)
    {
        if (ShouldConvertMetalOutputToIngots(sourceStack, stack)
            && TryGetMetalIngotConversion(stack, out AssetLocation ingotCode, out int ingotsPerUnit))
        {
            CollectibleObject? ingotCollectible = Api?.World.GetItem(ingotCode);
            if (ingotCollectible != null)
            {
                AddAggregatedStack(aggregated, new ItemStack(ingotCollectible), quantity * ingotsPerUnit);
                return;
            }
        }

        AddAggregatedStack(aggregated, stack, quantity);
    }

    private static bool ShouldConvertMetalOutputToIngots(ItemStack sourceStack, ItemStack outputStack)
    {
        return VSTemporalReverserModSystem.Config.DeconstructMetalOutputsToIngots;
    }

    private static bool IsArmorStack(ItemStack stack)
    {
        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        return path.StartsWith("armor-", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryMatchRecipeOutput(ItemStack input, object recipe, out Dictionary<string, string> captures, out int inputBatchSize)
    {
        captures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        inputBatchSize = 1;

        object? output = GetMemberValue(recipe, "RecipeOutput", "Output");
        if (output == null)
        {
            return false;
        }

        string outputType = TryGetStringMember(output, "Type") ?? "item";
        bool expectsBlock = outputType.Equals("block", StringComparison.OrdinalIgnoreCase);
        if ((input.Block != null) != expectsBlock)
        {
            return false;
        }

        inputBatchSize = Math.Max(1, TryGetIntMember(output, "Quantity") ?? 1);

        if (TryGetResolvedRecipeOutput(recipe, out ItemStack? recipeOutput) && recipeOutput != null && MatchesCollectibleCode(input, recipeOutput))
        {
            string? resolvedTemplate = TryGetStringMember(output, "Code");
            if (!string.IsNullOrWhiteSpace(resolvedTemplate)
                && TryMatchCodeTemplate(input.Collectible.Code.Path, resolvedTemplate, out Dictionary<string, string> resolvedCaptures))
            {
                captures = resolvedCaptures;
            }

            MergeCollectibleVariantCaptures(input, captures);

            return true;
        }

        string? templateCode = TryGetStringMember(output, "Code");
        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return false;
        }

        bool matched = TryMatchCodeTemplate(input.Collectible.Code.Path, templateCode, out captures);
        if (matched)
        {
            MergeCollectibleVariantCaptures(input, captures);
        }

        return matched;
    }

    private bool TryGetIngredientStack(object ingredient, Dictionary<string, string> captures, out ItemStack? stack)
    {
        stack = null;

        if (TryGetResolvedIngredientStack(ingredient, out ItemStack? resolved) && resolved != null)
        {
            string? rawCode = TryGetStringMember(ingredient, "Code");
            if (string.IsNullOrWhiteSpace(rawCode) || (!rawCode.Contains('*') && !rawCode.Contains('{')))
            {
                stack = resolved;
                return true;
            }
        }

        string? type = TryGetStringMember(ingredient, "Type") ?? "item";
        string? code = TryGetStringMember(ingredient, "Code");
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        string resolvedCode = ApplyTemplateCaptures(code, captures, TryGetStringMember(ingredient, "Name"));
        if (resolvedCode.Contains('*') || resolvedCode.Contains('{'))
        {
            return false;
        }

        AssetLocation location = ToGameAssetLocation(resolvedCode);
        CollectibleObject? collectible = type.Equals("block", StringComparison.OrdinalIgnoreCase)
            ? Api?.World.GetBlock(location)
            : Api?.World.GetItem(location);

        if (collectible == null)
        {
            return false;
        }

        stack = new ItemStack(collectible);
        return true;
    }

    private bool RecipeConsumesInputItem(ItemStack inputStack, object recipe, Dictionary<string, string> captures)
    {
        object? ingredientContainer = GetMemberValue(recipe, "RecipeIngredients", "Ingredients");
        foreach ((_, object ingredient) in EnumerateIngredientEntries(ingredientContainer))
        {
            if (TryGetBoolMember(ingredient, "IsTool") || TryGetBoolMember(ingredient, "Tool"))
            {
                continue;
            }

            if (!TryGetIngredientStack(ingredient, captures, out ItemStack? resolved) || resolved == null)
            {
                continue;
            }

            if (MatchesCollectibleCode(inputStack, resolved))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<(string? Key, object Ingredient)> EnumerateIngredientEntries(object? value)
    {
        if (value == null)
        {
            yield break;
        }

        if (value is string || value is ItemStack)
        {
            yield return (null, value);
            yield break;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (object? item in enumerable)
            {
                if (item == null)
                {
                    continue;
                }

                if (TryGetDictionaryLikeEntry(item, out string? key, out object? ingredient) && ingredient != null)
                {
                    yield return (key, ingredient);
                    continue;
                }

                foreach ((string? nestedKey, object nestedIngredient) in EnumerateIngredientEntries(item))
                {
                    yield return (nestedKey, nestedIngredient);
                }
            }

            yield break;
        }

        yield return (null, value);
    }

    private static bool TryGetResolvedRecipeOutput(object recipe, out ItemStack? stack)
    {
        stack = null;
        object? output = GetMemberValue(recipe, "RecipeOutput", "Output");
        if (output == null)
        {
            return false;
        }

        if (output is ItemStack outputStack)
        {
            stack = outputStack;
            return true;
        }

        object? resolved = GetMemberValue(output, "ResolvedItemstack", "ResolvedItemStack");
        if (resolved is ItemStack resolvedStack)
        {
            stack = resolvedStack;
            return true;
        }

        return false;
    }

    private static bool TryGetResolvedIngredientStack(object ingredient, out ItemStack? stack)
    {
        stack = null;

        if (ingredient is ItemStack itemStack)
        {
            stack = itemStack;
            return true;
        }

        object? resolved = GetMemberValue(ingredient, "ResolvedItemstack", "ResolvedItemStack");
        if (resolved is ItemStack resolvedStack)
        {
            stack = resolvedStack;
            return true;
        }

        return false;
    }

    private static bool TryGetDictionaryLikeEntry(object value, out string? key, out object? ingredient)
    {
        key = null;
        ingredient = null;

        if (value is DictionaryEntry dictionaryEntry)
        {
            key = dictionaryEntry.Key?.ToString();
            ingredient = dictionaryEntry.Value;
            return ingredient != null;
        }

        object? entryKey = GetMemberValue(value, "Key");
        object? entryValue = GetMemberValue(value, "Value");
        if (entryValue == null)
        {
            return false;
        }

        key = entryKey?.ToString();
        ingredient = entryValue;
        return true;
    }

    private static object? GetMemberValue(object? obj, params string[] names)
    {
        if (obj == null)
        {
            return null;
        }

        Type type = obj.GetType();
        foreach (string name in names)
        {
            var property = type.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.GetValue(obj);
            }

            var field = type.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null)
            {
                return field.GetValue(obj);
            }
        }

        return null;
    }

    private static int? TryGetIntMember(object obj, params string[] names)
    {
        object? value = GetMemberValue(obj, names);
        return value switch
        {
            byte b => b,
            short s => s,
            int i => i,
            long l => (int)l,
            _ => null
        };
    }

    private static bool TryGetBoolMember(object obj, params string[] names)
    {
        return GetMemberValue(obj, names) is bool b && b;
    }

    private static string? TryGetStringMember(object obj, params string[] names)
    {
        return GetMemberValue(obj, names)?.ToString();
    }

    private static Dictionary<string, int> GetIngredientSymbolCounts(object recipe)
    {
        Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);
        string? pattern = TryGetStringMember(recipe, "IngredientPattern");
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return counts;
        }

        foreach (char c in pattern)
        {
            if (char.IsWhiteSpace(c) || c == '_' || c == ',')
            {
                continue;
            }

            string key = c.ToString();
            counts[key] = counts.TryGetValue(key, out int count) ? count + 1 : 1;
        }

        return counts;
    }

    private static bool IsRepairLikeRecipe(object recipe)
    {
        if (TryGetBoolMember(recipe, "Shapeless"))
        {
            return true;
        }

        if (GetMemberValue(recipe, "CopyAttributesFrom") != null)
        {
            return true;
        }

        string? name = TryGetStringMember(recipe, "Name");
        if (!string.IsNullOrWhiteSpace(name) && name.Contains("repair", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool MatchesCollectibleCode(ItemStack input, ItemStack output)
    {
        AssetLocation? inputCode = input.Collectible?.Code;
        AssetLocation? outputCode = output.Collectible?.Code;
        return inputCode != null
            && outputCode != null
            && inputCode.Domain.Equals(outputCode.Domain, StringComparison.OrdinalIgnoreCase)
            && inputCode.Path.Equals(outputCode.Path, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetStackKey(ItemStack stack)
    {
        string type = stack.Block != null ? "block" : "item";
        return $"{type}:{stack.Collectible?.Code}";
    }

    private ItemStack NormalizeArmorRefundStack(ItemStack sourceStack, ItemStack refundStack)
    {
        if (!VSTemporalReverserModSystem.Config.DeconstructMetalOutputsToIngots)
        {
            return refundStack;
        }

        string sourcePath = sourceStack.Collectible?.Code?.Path ?? string.Empty;
        if (!sourcePath.StartsWith("armor-", StringComparison.OrdinalIgnoreCase))
        {
            return refundStack;
        }

        if (!TryGetMetalIngotConversion(refundStack, out AssetLocation? ingotCode, out int ingotsPerUnit))
        {
            return refundStack;
        }

        CollectibleObject? ingotCollectible = Api?.World.GetItem(ingotCode);
        if (ingotCollectible == null)
        {
            return refundStack;
        }

        return new ItemStack(ingotCollectible)
        {
            StackSize = refundStack.StackSize * ingotsPerUnit
        };
    }

    private bool TryCreatePartialMetalBitRefund(ItemStack stack, double fractionalRefundQuantity, out ItemStack? metalBitStack, out string? metalBitRemainderKey, out double metalBitRemainder)
    {
        metalBitStack = null;
        metalBitRemainderKey = null;
        metalBitRemainder = 0;

        if (!TryGetMetalBitConversion(stack, out AssetLocation? metalBitCode, out AssetLocation? ingotCode, out int bitsPerUnit))
        {
            return false;
        }

        metalBitRemainderKey = $"item:{metalBitCode}";
        double totalBits = GetSalvageRemainder(metalBitRemainderKey) + (fractionalRefundQuantity * bitsPerUnit);
        int wholeBits = (int)Math.Floor(totalBits + 1e-9);
        metalBitRemainder = totalBits - wholeBits;

        if (wholeBits > 0)
        {
            if (wholeBits % 20 == 0 && ingotCode != null && Api?.World.GetItem(ingotCode) is CollectibleObject ingotCollectible)
            {
                metalBitStack = new ItemStack(ingotCollectible)
                {
                    StackSize = wholeBits / 20
                };
            }
            else
            {
                CollectibleObject? metalBitCollectible = Api?.World.GetItem(metalBitCode);
                if (metalBitCollectible == null)
                {
                    return false;
                }

                metalBitStack = new ItemStack(metalBitCollectible)
                {
                    StackSize = wholeBits
                };
            }
        }

        return true;
    }

    private static bool TryGetMetalBitConversion(ItemStack stack, out AssetLocation metalBitCode, out AssetLocation? ingotCode, out int bitsPerUnit)
    {
        metalBitCode = default!;
        ingotCode = null;
        bitsPerUnit = 0;

        AssetLocation? code = stack.Collectible?.Code;
        string? path = code?.Path;
        string? domain = code?.Domain;
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        if (TryExtractMetalVariant(path, "metalplate-", out string? plateMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{plateMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{plateMetal}");
            bitsPerUnit = 40;
            return true;
        }

        if (TryExtractMetalVariant(path, "metalchain-", out string? chainMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{chainMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{chainMetal}");
            bitsPerUnit = 40;
            return true;
        }

        if (TryExtractMetalVariant(path, "ingot-", out string? ingotMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{ingotMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{ingotMetal}");
            bitsPerUnit = 20;
            return true;
        }

        if (TryExtractMetalVariant(path, "metallamellae-", out string? lamellaeMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{lamellaeMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{lamellaeMetal}");
            bitsPerUnit = 20;
            return true;
        }

        if (TryExtractMetalVariant(path, "metalscale-", out string? scaleMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{scaleMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{scaleMetal}");
            bitsPerUnit = 20;
            return true;
        }

        if (TryExtractMetalVariant(path, "metalsheet-", out string? sheetMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{sheetMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{sheetMetal}");
            bitsPerUnit = 20;
            return true;
        }

        if (TryExtractMetalVariant(path, "rod-", out string? rodMetal))
        {
            metalBitCode = new AssetLocation(domain, $"metalbit-{rodMetal}");
            ingotCode = new AssetLocation(domain, $"ingot-{rodMetal}");
            bitsPerUnit = 20;
            return true;
        }

        return false;
    }

    private static bool TryGetMetalIngotConversion(ItemStack stack, out AssetLocation ingotCode, out int ingotsPerUnit)
    {
        ingotCode = default!;
        ingotsPerUnit = 0;

        AssetLocation? code = stack.Collectible?.Code;
        string? path = code?.Path;
        string? domain = code?.Domain;
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        if (TryExtractMetalVariant(path, "metalplate-", out string? plateMetal) || TryExtractMetalVariant(path, "metalchain-", out plateMetal))
        {
            ingotCode = new AssetLocation(domain, $"ingot-{plateMetal}");
            ingotsPerUnit = 2;
            return true;
        }

        if (TryExtractMetalVariant(path, "metalscale-", out string? scaleMetal)
            || TryExtractMetalVariant(path, "metallamellae-", out scaleMetal)
            || TryExtractMetalVariant(path, "metalsheet-", out scaleMetal)
            || TryExtractMetalVariant(path, "rod-", out scaleMetal))
        {
            ingotCode = new AssetLocation(domain, $"ingot-{scaleMetal}");
            ingotsPerUnit = 1;
            return true;
        }

        return false;
    }

    private static bool TryExtractMetalVariant(string path, string prefix, out string? metal)
    {
        metal = null;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || path.Length <= prefix.Length)
        {
            return false;
        }

        metal = path[prefix.Length..];
        return !string.IsNullOrWhiteSpace(metal);
    }

    private double GetSalvageRemainder(string key)
    {
        return salvageRemainders.GetDouble(key);
    }

    private void ApplySalvageRemainders(Dictionary<string, double> updatedRemainders)
    {
        TreeAttribute nextRemainders = new();
        foreach (string key in salvageRemainders.Keys)
        {
            double remainder = salvageRemainders.GetDouble(key);
            if (remainder > 1e-9)
            {
                nextRemainders.SetDouble(key, remainder);
            }
        }

        foreach ((string key, double remainder) in updatedRemainders)
        {
            if (remainder > 1e-9)
            {
                nextRemainders.SetDouble(key, remainder);
            }
            else
            {
                nextRemainders.RemoveAttribute(key);
            }
        }

        salvageRemainders = nextRemainders;
        MarkDirty(true);
    }

    private static bool IsTemporalDust(ItemStack stack)
    {
        return stack.Collectible?.Code?.Domain == "vstemporalreverser"
            && stack.Collectible.Code.Path.Equals("temporal-dust", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDeconstructableCandidate(ItemStack stack)
    {
        return stack.Collectible != null
            && stack.StackSize > 0
            && !IsTemporalDust(stack);
    }

    private static bool IsExplicitlyAllowedDeconstructionOutput(ItemStack stack)
    {
        string path = stack.Collectible?.Code?.Path ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith("armor-", StringComparison.OrdinalIgnoreCase)
            && path.Contains("-lamellar-wood", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (AllowedDeconstructionOutputs.ExactCodes.Contains(path))
        {
            return true;
        }

        foreach (string prefix in AllowedDeconstructionOutputs.Prefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void MergeCollectibleVariantCaptures(ItemStack stack, Dictionary<string, string> captures)
    {
        object? variantData = GetMemberValue(stack.Collectible, "Variant", "VariantStrict");
        if (variantData is not IEnumerable enumerable)
        {
            return;
        }

        foreach (object? entry in enumerable)
        {
            if (entry == null || !TryGetDictionaryLikeEntry(entry, out string? key, out object? value) || string.IsNullOrWhiteSpace(key) || value == null)
            {
                continue;
            }

            captures.TryAdd(key!, value.ToString() ?? string.Empty);
        }
    }

    private static bool TryMatchCodeTemplate(string inputPath, string template, out Dictionary<string, string> captures)
    {
        captures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] inputParts = inputPath.Split('-');
        string[] templateParts = template.Split('-');
        if (inputParts.Length != templateParts.Length)
        {
            return false;
        }

        int wildcardIndex = 0;
        for (int i = 0; i < templateParts.Length; i++)
        {
            string current = templateParts[i];
            string actual = inputParts[i];

            if (current == "*")
            {
                captures[$"wildcard{wildcardIndex++}"] = actual;
                continue;
            }

            if (current.StartsWith("{", StringComparison.Ordinal) && current.EndsWith("}", StringComparison.Ordinal) && current.Length > 2)
            {
                captures[current[1..^1]] = actual;
                continue;
            }

            if (!current.Equals(actual, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static string ApplyTemplateCaptures(string template, Dictionary<string, string> captures, string? preferredWildcardName)
    {
        foreach ((string key, string value) in captures)
        {
            template = template.Replace("{" + key + "}", value, StringComparison.OrdinalIgnoreCase);
        }

        if (!template.Contains('*'))
        {
            return template;
        }

        string? wildcardValue = null;
        if (!string.IsNullOrWhiteSpace(preferredWildcardName) && captures.TryGetValue(preferredWildcardName!, out string? namedValue))
        {
            wildcardValue = namedValue;
        }
        else if (!string.IsNullOrWhiteSpace(preferredWildcardName)
            && preferredWildcardName!.Equals("wood", StringComparison.OrdinalIgnoreCase)
            && captures.TryGetValue("material", out string? materialValue))
        {
            wildcardValue = materialValue;
        }
        else if (!string.IsNullOrWhiteSpace(preferredWildcardName)
            && preferredWildcardName!.Equals("metal", StringComparison.OrdinalIgnoreCase)
            && captures.TryGetValue("material", out string? metalMaterialValue))
        {
            wildcardValue = metalMaterialValue;
        }
        else if (captures.TryGetValue("wildcard0", out string? starValue))
        {
            wildcardValue = starValue;
        }
        else if (captures.Count == 1)
        {
            foreach (string onlyValue in captures.Values)
            {
                wildcardValue = onlyValue;
            }
        }
        else if (captures.TryGetValue("wood", out string? woodValue))
        {
            wildcardValue = woodValue;
        }

        return wildcardValue == null ? template : template.Replace("*", wildcardValue, StringComparison.Ordinal);
    }

    private static AssetLocation ToGameAssetLocation(string code)
    {
        return code.Contains(':', StringComparison.Ordinal)
            ? new AssetLocation(code)
            : new AssetLocation("game", code);
    }

    private void WriteDeconstructionDebugEvent(
        string eventType,
        ItemStack inputStack,
        object? recipe,
        Dictionary<string, string>? captures,
        List<ItemStack>? outputs,
        Dictionary<string, double>? remainders)
    {
        if (!VSTemporalReverserModSystem.Config.EnableDebugMode)
        {
            return;
        }

        try
        {
            EnsureDebugLogPath();
            if (debugLogPath == null)
            {
                return;
            }

            List<string>? describedOutputs = null;
            if (outputs != null)
            {
                describedOutputs = new List<string>(outputs.Count);
                foreach (ItemStack output in outputs)
                {
                    describedOutputs.Add(DescribeStackForRecord(output));
                }
            }

            Dictionary<string, object?> record = new()
            {
                ["timestampUtc"] = DateTime.UtcNow.ToString("O"),
                ["event"] = eventType,
                ["system"] = "deconstructor",
                ["input"] = DescribeStackForRecord(inputStack),
                ["recipeName"] = recipe != null ? TryGetStringMember(recipe, "Name") : null,
                ["recipeGroup"] = recipe != null ? TryGetIntMember(recipe, "RecipeGroup") : null,
                ["recipeOutputCode"] = recipe != null ? TryGetStringMember(GetMemberValue(recipe, "RecipeOutput", "Output") ?? new object(), "Code") : null,
                ["captures"] = captures,
                ["outputs"] = describedOutputs,
                ["remainders"] = remainders
            };

            string line = JsonSerializer.Serialize(record);
            lock (DebugLogLock)
            {
                File.AppendAllText(debugLogPath, line + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Api?.Logger?.Warning($"[TemporalReverser] Failed to write deconstructor debug log: {ex.Message}");
        }
    }

    private static void EnsureDebugLogPath()
    {
        if (debugLogPath != null)
        {
            return;
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string logDir = Path.Combine(appData, "VintagestoryData", "Logs", "VSTemporalReverser");
        Directory.CreateDirectory(logDir);
        debugLogPath = Path.Combine(logDir, "restore-debug.jsonl");
    }

    private static string DescribeStackForRecord(ItemStack stack)
    {
        string code = stack.Collectible?.Code?.ToString() ?? "<unknown>";
        string? type = stack.Attributes?.GetString("type");
        return string.IsNullOrWhiteSpace(type) ? $"{code} x{stack.StackSize}" : $"{code} [type={type}] x{stack.StackSize}";
    }

    private static class AllowedDeconstructionOutputs
    {
        public static readonly string[] Prefixes =
        [
            // Armor
            "armor-",
            "clothes-",

            // Furnishings and storage
            "antlermount",
            "armorstand",
            "barrel",
            "bookshelf",
            "cabinet-",
            "chair-",
            "chest-",
            "crate",
            "displaycase",
            "labeledchest-",
            "lantern-",
            "scrollrack",
            "shelf-",
            "table-",
            "talldisplaycase",
            "toolrack-",
            "trunk-",

            // Tools and weapons
            "axe-",
            "pickaxe-",
            "hammer-",
            "saw-",
            "shovel-",
            "hoe-",
            "knife-",
            "cleaver-",
            "spear-",
            "scythe-",
            "prospectingpick-",
            "chisel-",
            "crowbar-",
            "shears-",
            "wrench-",
            "tongsmetal-",

            // Fired pottery
            "bowl-",
            "clayplanter-",
            "claypot-",
            "crock-",
            "crucible-",
            "flowerpot-",
            "storagevessel-",
            "jug-",
            "wateringcan-",

            // Doors, ladders, fences, and fixtures
            "door-",
            "ladder-",
            "supportbeam-",
            "torchholder-",
            "trapdoor-",
            "wattle-",
            "wattlegate-",
            "woodenfence-",
            "woodenfencegate-",
            "roughhewnfence-",
            "roughhewnfencegate-",

            // Mechanical assemblies
            "chute-",
            "woodenaxle-",
            "angledgears-",
            "spurgear-",
            "brake-",
            "clutch-",
            "transmission-",
            "archimedesscrew-",
            "windmillrotor-",
            "waterwheel-",
            "woodenaxlehub-",
            "helvehammer-",
            "helvehammerhead-",
            "helvehammerbase-",
            "pulverizerframe-",
            "pulverizertoggle-",
            "pounder-",
            "poundercap-",
            "anvil-"
        ];

        public static readonly HashSet<string> ExactCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            // Mechanical parts and container items that do not fit neatly into a prefix bucket
            "largegear3",
            "sail",
            "sail-large-oak",
            "largegearsection-wood",
            "linkage-heavy-oak",
            "backpack-normal",
            "backpack-sturdy",
            "hunterbackpack",
            "miningbag",
            "miningbagsturdy",
            "solderingiron"
        };
    }

    private static readonly Dictionary<string, string[]> CuratedRestoredClothingOutputs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["clothes-hand-clockmaker-wristguard"] = ["leather-normal-plain"],
        ["clothes-hand-commoner-gloves"] = ["leather-normal-plain"],
        ["clothes-hand-tailor-gloves"] = ["leather-normal-plain"],
        ["clothes-head-midsummer"] = ["cloth-yellow", "cloth-yellow"],
        ["clothes-head-popinjay"] = ["cloth-black", "cloth-black"],
        ["clothes-head-ruralhunter"] = ["cloth-brown", "cloth-brown"],

        ["clothes-lowerbody-beggar"] = ["cloth-black", "cloth-brown"],
        ["clothes-lowerbody-centurion"] = ["cloth-brown", "cloth-brown"],
        ["clothes-lowerbody-farmhand"] = ["cloth-green", "cloth-green"],
        ["clothes-lowerbody-popinjay"] = ["cloth-red", "cloth-brown"],
        ["clothes-lowerbody-wanderer"] = ["cloth-gray", "cloth-gray"],
        ["clothes-lowerbody-warrior"] = ["cloth-black", "cloth-purple"],

        ["clothes-shoulder-clockmaker-apron"] = ["cloth-red", "cloth-brown"],
        ["clothes-shoulder-patchwork"] = ["cloth-black", "cloth-brown"],
        ["clothes-shoulder-ruralhunter"] = ["cloth-brown", "cloth-black"],
        ["clothes-shoulder-wanderer"] = ["cloth-brown", "cloth-black"],

        ["clothes-upperbody-beggar"] = ["cloth-gray", "cloth-gray"],
        ["clothes-upperbody-centurion"] = ["cloth-red", "cloth-brown"],
        ["clothes-upperbody-clockmaker-shirt"] = ["cloth-plain", "cloth-plain"],
        ["clothes-upperbody-farmhand"] = ["cloth-plain", "cloth-gray"],
        ["clothes-upperbody-midsummer"] = ["cloth-white", "cloth-white"],
        ["clothes-upperbody-popinjay"] = ["cloth-black", "cloth-black"],
        ["clothes-upperbody-ruralfarmer"] = ["cloth-white", "cloth-plain"],
        ["clothes-upperbody-ruralhunter"] = ["cloth-plain", "cloth-plain"],
        ["clothes-upperbody-wanderer"] = ["cloth-brown", "cloth-brown"],
        ["clothes-upperbody-warrior"] = ["cloth-brown", "cloth-brown"],

        ["clothes-upperbodyover-clockmaker-tunic"] = ["cloth-purple", "cloth-black"]
,
        ["clothes-waist-beggar"] = ["leather-normal-plain"],
        ["clothes-waist-centurion"] = ["leather-normal-plain"],
        ["clothes-waist-farmhand"] = ["leather-normal-plain"],
        ["clothes-waist-midsummer"] = ["leather-normal-plain"],
        ["clothes-waist-popinjay"] = ["leather-normal-plain"],
        ["clothes-waist-ruralfarmer"] = ["leather-normal-plain"],
        ["clothes-waist-ruralhunter"] = ["leather-normal-plain"],
        ["clothes-waist-wanderer"] = ["leather-normal-plain"],
        ["clothes-waist-warrior"] = ["leather-normal-plain"]
    };
}
