using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockEntityTemporalReconstructionDevice : BlockEntityGenericContainer
{
    private const int RepairSlotCount = 8;
    private const int FuelSlotId = 8;
    private const int OutputSlotStart = 9;
    private const int OutputSlotCount = 12;
    private const int TemporalDustFuelCost = 10;
    private const int ClothingMinRepairDurationMs = 2000;
    private const int ClothingMaxRepairDurationMs = 10000;
    private const int ToolRepairDurationPer100DurabilityMs = 2000;
    private const int ToolMaxRepairDurationMs = 60000;
    private const int DurabilityRepairStep = 100;
    private const int RepairProgressTickMs = 120;
    private const int PhaseStateTickMs = 100;
    private const int VisualPulseIntervalMs = 200;
    private const int SwitchItemPauseDurationMs = 1626;
    private const int ShutdownEffectDurationMs = 7760;
    private const float MachineLoopBaseVolume = 0.22f;
    private const float MachineLoopFadeDistance = 16f;
    private const float MachineLoopFadeInSeconds = 0.9f;

    private bool isRepairing;
    private bool temporalStabilityLost;
    private bool outputCapacityBlocked;
    private int activeRepairSlotId = -1;
    private long repairStartedAtMs;
    private long repairCompleteAtMs;
    private int activeRepairDurationMs;
    private int initialRemainingDurability;
    private float initialCondition = 1f;
    private long nextVisualPulseAtMs;
    private long phaseStateListenerId;
    private long repairProgressListenerId;
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

    private ItemSlot FuelSlot => Inventory[FuelSlotId];

    public bool IsRepairing => isRepairing;
    public bool HasTemporalDustFuel => FuelSlot.Itemstack != null && IsTemporalDust(FuelSlot.Itemstack) && FuelSlot.StackSize >= TemporalDustFuelCost;
    public bool HasRepairItem => FindFirstOccupiedRepairSlotId() >= 0;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        Inventory.SlotModified += OnInventorySlotModified;
        QueueDeferredRepairResume();
        UpdateServerPhaseListener();
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
        EvaluateRepairState();
    }

    protected override void OnTick(float dt)
    {
        base.OnTick(dt);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        isRepairing = tree.GetBool("isRepairing");
        temporalStabilityLost = tree.GetBool("temporalStabilityLost");
        outputCapacityBlocked = tree.GetBool("outputCapacityBlocked");
        activeRepairSlotId = tree.GetInt("activeRepairSlotId", -1);
        repairStartedAtMs = tree.GetLong("repairStartedAtMs");
        repairCompleteAtMs = tree.GetLong("repairCompleteAtMs");
        activeRepairDurationMs = tree.GetInt("activeRepairDurationMs");
        initialRemainingDurability = tree.GetInt("initialRemainingDurability");
        initialCondition = tree.GetFloat("initialCondition", 1f);
        queuePauseUntilMs = tree.GetLong("queuePauseUntilMs");
        shutdownVisualUntilMs = tree.GetLong("shutdownVisualUntilMs");
        clientSoundCueId = tree.GetLong("clientSoundCueId");
        clientSoundCuePath = tree.GetString("clientSoundCuePath", string.Empty);

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
                QueueDeferredRepairResume();
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetBool("isRepairing", isRepairing);
        tree.SetBool("temporalStabilityLost", temporalStabilityLost);
        tree.SetBool("outputCapacityBlocked", outputCapacityBlocked);
        tree.SetInt("activeRepairSlotId", activeRepairSlotId);
        tree.SetLong("repairStartedAtMs", repairStartedAtMs);
        tree.SetLong("repairCompleteAtMs", repairCompleteAtMs);
        tree.SetInt("activeRepairDurationMs", activeRepairDurationMs);
        tree.SetInt("initialRemainingDurability", initialRemainingDurability);
        tree.SetFloat("initialCondition", initialCondition);
        tree.SetLong("queuePauseUntilMs", queuePauseUntilMs);
        tree.SetLong("shutdownVisualUntilMs", shutdownVisualUntilMs);
        tree.SetLong("clientSoundCueId", clientSoundCueId);
        tree.SetString("clientSoundCuePath", clientSoundCuePath);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);

        string targetText = GetActiveRepairSlot()?.Itemstack?.GetName()
            ?? (GetStatusInspectionSlotId() >= 0 ? Inventory[GetStatusInspectionSlotId()].Itemstack?.GetName() ?? "Queued items ready" : "Empty");
        sb.AppendLine($"Repair queue: {CountQueuedRepairItems()} item(s)");
        sb.AppendLine($"Active item: {targetText}");
        sb.AppendLine($"Temporal dust: {FuelSlot.StackSize}");

        if (isRepairing)
        {
            sb.AppendLine("Reconstruction in progress...");
            sb.AppendLine($"Time remaining: {Math.Max(0, (repairCompleteAtMs - Api.World.ElapsedMilliseconds + 999) / 1000)}s");
        }
        else if (IsShutdownVisualActive())
        {
            sb.AppendLine("Reconstruction complete. Powering down...");
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (Api?.Side == EnumAppSide.Client)
        {
            toggleInventoryDialogClient(byPlayer, () =>
            {
                ICoreClientAPI capi = (ICoreClientAPI)Api;
                return new GuiDialogBlockEntityTemporalReconstructionDevice(
                    Lang.Get(dialogTitleLangCode),
                    Inventory,
                    Pos,
                    capi);
            });
        }

        return true;
    }

    public bool CanRepairCurrentItemForDialog()
    {
        return CanRepairCurrentItem();
    }

    public string GetStatusText()
    {
        int slotId = GetStatusInspectionSlotId();
        ItemStack? repairStack = slotId >= 0 ? Inventory[slotId].Itemstack : null;
        if (repairStack == null)
        {
            return string.Empty;
        }

        if (IsTemporalReverserItem(repairStack))
        {
            return "Using one temporal device to repair another would be wildly irresponsible.";
        }

        if (!IsRepairableTarget(repairStack))
        {
            return "This item cannot be reconstructed here.";
        }

        if (outputCapacityBlocked)
        {
            return "Output inventory is full. Remove reconstructed items to continue.";
        }

        if (!HasTemporalDustFuel)
        {
            return $"Add {TemporalDustFuelCost} temporal dust to power reconstruction.";
        }

        if (isRepairing)
        {
            return "Reconstruction in progress...";
        }

        if (!NeedsRepair(repairStack))
        {
            return "No items need repair.";
        }

        return "Ready for reconstruction.";
    }

    private bool CanRepairCurrentItem()
    {
        if (activeRepairSlotId < 0 || activeRepairSlotId >= RepairSlotCount)
        {
            return false;
        }

        ItemSlot repairSlot = Inventory[activeRepairSlotId];
        ItemStack? repairStack = repairSlot.Itemstack;
        if (repairStack == null)
        {
            return false;
        }

        if (!IsRepairableTarget(repairStack))
        {
            return false;
        }

        return NeedsRepair(repairStack);
    }

    private void CompleteRepair()
    {
        ItemSlot? repairSlot = GetActiveRepairSlot();
        ItemStack? repairStack = repairSlot?.Itemstack;
        if (repairStack == null)
        {
            StopRepair();
            return;
        }

        int maxDurability = repairStack.Collectible.GetMaxDurability(repairStack);
        if (maxDurability > 0)
        {
            repairStack.Collectible.SetDurability(repairStack, maxDurability);
        }
        else
        {
            repairStack.Attributes.SetFloat("condition", 1f);
        }

        ItemStack outputStack = repairStack.Clone();
        if (!CanStoreOutputs([outputStack]))
        {
            outputCapacityBlocked = true;
            repairCompleteAtMs = Api!.World.ElapsedMilliseconds + RepairProgressTickMs;
            MarkDirty(true);
            return;
        }

        repairSlot!.TakeOut(1);
        repairSlot.MarkDirty();
        StoreOutputs([outputStack]);
        outputCapacityBlocked = false;

        bool hasQueuedFollowup = FindNextRepairableSlotId() >= 0 && HasTemporalDustFuel;
        if (hasQueuedFollowup)
        {
            StopRepair(playShutdown: false);
            BeginSwitchPause();
            return;
        }

        BeginShutdownSequence();
    }

    private void StopRepair(bool playShutdown = true)
    {
        isRepairing = false;
        activeRepairSlotId = -1;
        repairStartedAtMs = 0;
        repairCompleteAtMs = 0;
        activeRepairDurationMs = 0;
        initialRemainingDurability = 0;
        initialCondition = 1f;
        UnregisterRepairProgressListener();
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
        StopRepair(playShutdown: false);
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
        EvaluateRepairState();
    }

    private void OnInventorySlotModified(int slotId)
    {
        if (suppressInventoryChanged)
        {
            return;
        }

        if (Api?.Side == EnumAppSide.Server && !isRepairing && HasTemporalDustFuel && FindNextRepairableSlotId() >= 0)
        {
            if (shutdownVisualUntilMs > 0)
            {
                shutdownVisualUntilMs = 0;
                ClearQueuedClientSoundCue();
                UpdateVisualState(false);
                UpdateClientParticleListener();
            }

            outputCapacityBlocked = false;
        }

        EvaluateRepairState();
    }

    private void EvaluateRepairState()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isRepairing)
        {
            if (!CanRepairCurrentItem())
            {
                if (TryFinalizeCompletedActiveRepair())
                {
                    return;
                }

                BeginShutdownSequence();
                return;
            }
        }

        if (isRepairing)
        {
            return;
        }

        outputCapacityBlocked = HasTemporalDustFuel
            && FindNextRepairableSlotId() >= 0
            && !CanStoreOutputsForNextRepair();

        if (IsQueuePauseActive())
        {
            MarkDirty(true);
            return;
        }

        if (IsShutdownVisualActive())
        {
            MarkDirty(true);
            return;
        }

        if (!HasTemporalDustFuel)
        {
            MarkDirty(true);
            return;
        }

        if (queueResumeCallbackId != 0)
        {
            MarkDirty(true);
            return;
        }

        if (!outputCapacityBlocked && FindNextRepairableSlotId() >= 0)
        {
            BeginSwitchPause();
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

        if (isRepairing && Api?.Side == EnumAppSide.Server && repairProgressListenerId == 0)
        {
            RegisterRepairProgressListener();
        }

        UpdateServerPhaseListener();
        UpdateClientParticleListener();
        UpdateClientMachineLoopSound();
    }

    private void RegisterRepairProgressListener()
    {
        UnregisterRepairProgressListener();
        repairProgressListenerId = RegisterGameTickListener(OnRepairProgressTick, RepairProgressTickMs, 0);
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
            TryStartNextRepair();
        }

        if (shutdownVisualUntilMs > 0 && now >= shutdownVisualUntilMs)
        {
            FinalizeShutdown();
            return;
        }

        UpdateServerPhaseListener();
    }

    private void UnregisterRepairProgressListener()
    {
        if (repairProgressListenerId == 0)
        {
            return;
        }

        UnregisterGameTickListener(repairProgressListenerId);
        repairProgressListenerId = 0;
    }

    private void OnRepairProgressTick(float dt)
    {
        if (Api?.Side != EnumAppSide.Server || !isRepairing)
        {
            return;
        }

        ApplyIncrementalRepairProgress();

        if (Api.World.ElapsedMilliseconds >= repairCompleteAtMs)
        {
            CompleteRepair();
        }
    }

    private void ApplyIncrementalRepairProgress()
    {
        ItemSlot? repairSlot = GetActiveRepairSlot();
        ItemStack? repairStack = repairSlot?.Itemstack;
        if (repairStack == null || activeRepairDurationMs <= 0)
        {
            return;
        }

        double elapsedMs = Math.Max(0, Api.World.ElapsedMilliseconds - repairStartedAtMs);
        float progress = GameMath.Clamp((float)(elapsedMs / activeRepairDurationMs), 0f, 1f);

        int maxDurability = repairStack.Collectible.GetMaxDurability(repairStack);
        if (maxDurability > 0)
        {
            int missingDurability = Math.Max(0, maxDurability - initialRemainingDurability);
            int restoredDurability = (int)Math.Floor(missingDurability * progress);
            if (progress < 1f)
            {
                restoredDurability = (restoredDurability / DurabilityRepairStep) * DurabilityRepairStep;
            }
            else
            {
                restoredDurability = missingDurability;
            }

            int targetRemaining = initialRemainingDurability + restoredDurability;
            targetRemaining = Math.Clamp(targetRemaining, initialRemainingDurability, maxDurability);

            if (repairStack.Collectible.GetRemainingDurability(repairStack) != targetRemaining)
            {
                repairStack.Collectible.SetDurability(repairStack, targetRemaining);
                Inventory.MarkSlotDirty(activeRepairSlotId);
                MarkDirty(true);
            }

            return;
        }

        string code = repairStack.Collectible.Code?.Path ?? string.Empty;
        if (!code.StartsWith("clothes-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        float targetCondition = initialCondition + (1f - initialCondition) * progress;
        targetCondition = GameMath.Clamp(targetCondition, 0f, 1f);

        if (Math.Abs(repairStack.Attributes.GetFloat("condition", 1f) - targetCondition) > 0.001f)
        {
            repairStack.Attributes.SetFloat("condition", targetCondition);
            Inventory.MarkSlotDirty(activeRepairSlotId);
            MarkDirty(true);
        }
    }

    private ItemSlot? GetActiveRepairSlot()
    {
        return activeRepairSlotId >= 0 && activeRepairSlotId < RepairSlotCount ? Inventory[activeRepairSlotId] : null;
    }

    private int FindNextRepairableSlotId()
    {
        for (int i = 0; i < RepairSlotCount; i++)
        {
            ItemStack? stack = Inventory[i].Itemstack;
            if (stack == null)
            {
                continue;
            }

            if (IsRepairableTarget(stack) && NeedsRepair(stack))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindFirstOccupiedRepairSlotId()
    {
        for (int i = 0; i < RepairSlotCount; i++)
        {
            if (Inventory[i].Itemstack != null)
            {
                return i;
            }
        }

        return -1;
    }

    private int CountQueuedRepairItems()
    {
        int count = 0;
        for (int i = 0; i < RepairSlotCount; i++)
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
        string desiredSegment = running ? "-running-" : "-idle-";
        if (currentPath.Contains(desiredSegment, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string updatedPath = currentPath
            .Replace("-idle-", desiredSegment, System.StringComparison.OrdinalIgnoreCase)
            .Replace("-running-", desiredSegment, System.StringComparison.OrdinalIgnoreCase);

        Block? targetBlock = Api.World.GetBlock(new AssetLocation(Block.Code.Domain, updatedPath));
        if (targetBlock == null || targetBlock.Id == Block.Id)
        {
            return;
        }

        Api.World.BlockAccessor.ExchangeBlock(targetBlock.Id, Pos);
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

        if (!isRepairing)
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
        float fadeIn = GameMath.Clamp((Api.World.ElapsedMilliseconds - repairStartedAtMs) / (MachineLoopFadeInSeconds * 1000f), 0f, 1f);
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

    private void TryStartNextRepair()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isRepairing || IsQueuePauseActive() || IsShutdownVisualActive() || !HasTemporalDustFuel)
        {
            return;
        }

        int nextSlotId = FindNextRepairableSlotId();
        if (nextSlotId < 0)
        {
            return;
        }

        if (!CanStoreOutputsForNextRepair())
        {
            outputCapacityBlocked = true;
            MarkDirty(true);
            return;
        }

        ItemStack repairStack = Inventory[nextSlotId].Itemstack!;
        activeRepairSlotId = nextSlotId;
        isRepairing = true;
        outputCapacityBlocked = false;
        repairStartedAtMs = Api.World.ElapsedMilliseconds;
        activeRepairDurationMs = GetRepairDurationMs(repairStack);
        temporalStabilityLost = false;
        initialRemainingDurability = repairStack.Collectible.GetRemainingDurability(repairStack);
        initialCondition = repairStack.Attributes.GetFloat("condition", 1f);
        repairCompleteAtMs = repairStartedAtMs + activeRepairDurationMs;
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
        RegisterRepairProgressListener();
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
            "switch-items" => MachineLoopBaseVolume * 1.35f,
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
        return isRepairing || IsShutdownVisualActive();
    }

    private bool IsShutdownVisualActive()
    {
        return shutdownVisualUntilMs > 0 && (Api?.World.ElapsedMilliseconds ?? 0) < shutdownVisualUntilMs;
    }

    private void QueueDeferredRepairResume()
    {
        if (Api?.Side != EnumAppSide.Server || deferredResumeListenerId != 0)
        {
            return;
        }

        deferredResumeListenerId = RegisterGameTickListener(_ =>
        {
            if (deferredResumeListenerId != 0)
            {
                UnregisterGameTickListener(deferredResumeListenerId);
                deferredResumeListenerId = 0;
            }

            ResumePersistedRepairState();
            UpdateServerPhaseListener();
            EvaluateRepairState();
        }, 0, 1);
    }

    private void ResumePersistedRepairState()
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
            MarkDirty(true);
        }

        if (queuePauseUntilMs > now + SwitchItemPauseDurationMs)
        {
            queuePauseUntilMs = 0;
            queueResumeCallbackId = 0;
            MarkDirty(true);
        }

        if (shutdownVisualUntilMs > 0 && now >= shutdownVisualUntilMs)
        {
            FinalizeShutdown();
            return;
        }

        if (queuePauseUntilMs > 0)
        {
            if (now >= queuePauseUntilMs)
            {
                queuePauseUntilMs = 0;
                queueResumeCallbackId = 0;
                MarkDirty(true);
            }
            else
            {
                queueResumeCallbackId = 1;
            }
        }

        if (!isRepairing)
        {
            return;
        }

        if (!CanRepairCurrentItem())
        {
            if (TryFinalizeCompletedActiveRepair())
            {
                return;
            }

            StopRepair(playShutdown: false);
            return;
        }

        if (repairCompleteAtMs <= now)
        {
            CompleteRepair();
            return;
        }

        if (repairStartedAtMs > now || repairCompleteAtMs > now + Math.Max(activeRepairDurationMs, RepairProgressTickMs))
        {
            repairStartedAtMs = now;
            repairCompleteAtMs = now + Math.Max(activeRepairDurationMs, RepairProgressTickMs);
            MarkDirty(true);
        }

        RegisterRepairProgressListener();
        UpdateVisualState(true);
        MarkDirty(true);
    }

    private static bool IsTemporalDust(ItemStack stack)
    {
        return stack.Collectible?.Code?.Domain == "vstemporalreverser"
            && stack.Collectible.Code.Path.Equals("temporal-dust", System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTemporalReverserItem(ItemStack stack)
    {
        string? code = stack.Collectible?.Code?.Path;
        return !string.IsNullOrWhiteSpace(code)
            && code.StartsWith("temporal-reverser", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRepairableTarget(ItemStack stack)
    {
        if (stack.StackSize != 1 || stack.Collectible == null || stack.Block != null)
        {
            return false;
        }

        if (IsTemporalReverserItem(stack))
        {
            return false;
        }

        string code = stack.Collectible.Code?.Path ?? string.Empty;
        if (stack.Collectible.GetMaxDurability(stack) > 0)
        {
            return code.StartsWith("clothes-", System.StringComparison.OrdinalIgnoreCase) || stack.Item != null;
        }

        if (!code.StartsWith("clothes-", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return stack.Item != null;
    }

    private static bool NeedsRepair(ItemStack stack)
    {
        int maxDurability = stack.Collectible.GetMaxDurability(stack);
        if (maxDurability > 0)
        {
            int remainingDurability = stack.Collectible.GetRemainingDurability(stack);
            return remainingDurability < maxDurability;
        }

        string code = stack.Collectible.Code?.Path ?? string.Empty;
        if (!code.StartsWith("clothes-", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return stack.Attributes.GetFloat("condition", 1f) < 0.999f;
    }

    private static int GetRepairDurationMs(ItemStack stack)
    {
        int maxDurability = stack.Collectible.GetMaxDurability(stack);
        if (maxDurability > 0)
        {
            int remainingDurability = stack.Collectible.GetRemainingDurability(stack);
            int missingDurability = Math.Max(0, maxDurability - remainingDurability);
            double durationMs = missingDurability * (ToolRepairDurationPer100DurabilityMs / 100d);
            return (int)Math.Clamp(Math.Round(durationMs), 0, ToolMaxRepairDurationMs);
        }

        string code = stack.Collectible.Code?.Path ?? string.Empty;
        if (!code.StartsWith("clothes-", System.StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        float condition = stack.Attributes.GetFloat("condition", 1f);
        float clampedFraction = GameMath.Clamp(1f - condition, 0f, 1f);
        return (int)Math.Round(ClothingMinRepairDurationMs + (ClothingMaxRepairDurationMs - ClothingMinRepairDurationMs) * clampedFraction);
    }

    private int GetStatusInspectionSlotId()
    {
        if (isRepairing && activeRepairSlotId >= 0)
        {
            return activeRepairSlotId;
        }

        int repairableSlotId = FindNextRepairableSlotId();
        if (repairableSlotId >= 0)
        {
            return repairableSlotId;
        }

        return FindFirstOccupiedRepairSlotId();
    }

    private bool CanStoreOutputsForNextRepair()
    {
        int nextSlotId = FindNextRepairableSlotId();
        if (nextSlotId < 0)
        {
            return true;
        }

        ItemStack? stack = Inventory[nextSlotId].Itemstack;
        if (stack == null)
        {
            return false;
        }

        ItemStack outputStack = stack.Clone();
        int maxDurability = outputStack.Collectible.GetMaxDurability(outputStack);
        if (maxDurability > 0)
        {
            outputStack.Collectible.SetDurability(outputStack, maxDurability);
        }
        else
        {
            outputStack.Attributes.SetFloat("condition", 1f);
        }

        return CanStoreOutputs([outputStack]);
    }

    private bool TryFinalizeCompletedActiveRepair()
    {
        ItemStack? activeStack = GetActiveRepairSlot()?.Itemstack;
        if (activeStack == null || !IsRepairableTarget(activeStack) || NeedsRepair(activeStack))
        {
            return false;
        }

        CompleteRepair();
        return true;
    }

    private bool CanStoreOutputs(ItemStack[] outputs)
    {
        ItemStack?[] simulatedStacks = new ItemStack?[OutputSlotCount];
        for (int i = 0; i < OutputSlotCount; i++)
        {
            simulatedStacks[i] = Inventory[OutputSlotStart + i].Itemstack?.Clone();
        }

        foreach (ItemStack output in outputs)
        {
            if (!TryInsertIntoOutputBuffer(simulatedStacks, output))
            {
                return false;
            }
        }

        return true;
    }

    private void StoreOutputs(ItemStack[] outputs)
    {
        ItemStack?[] simulatedStacks = new ItemStack?[OutputSlotCount];
        for (int i = 0; i < OutputSlotCount; i++)
        {
            simulatedStacks[i] = Inventory[OutputSlotStart + i].Itemstack?.Clone();
        }

        foreach (ItemStack output in outputs)
        {
            if (!TryInsertIntoOutputBuffer(simulatedStacks, output))
            {
                throw new InvalidOperationException("Unable to store reconstruction outputs despite passing output-space validation.");
            }
        }

        for (int i = 0; i < OutputSlotCount; i++)
        {
            Inventory[OutputSlotStart + i].Itemstack = simulatedStacks[i]?.Clone();
            Inventory[OutputSlotStart + i].MarkDirty();
        }

        MarkDirty(true);
    }

    private static bool TryInsertIntoOutputBuffer(ItemStack?[] simulatedStacks, ItemStack output)
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
            ItemStack clone = pendingStack.Clone();
            clone.StackSize = moved;
            simulatedStacks[i] = clone;
            remaining -= moved;
        }

        return remaining <= 0;
    }

    private static string GetStackKey(ItemStack stack)
    {
        string code = stack.Collectible?.Code?.ToString() ?? string.Empty;
        string? attrs = stack.Attributes?.ToJsonToken()?.ToString();
        return $"{code}|{attrs}";
    }
}
