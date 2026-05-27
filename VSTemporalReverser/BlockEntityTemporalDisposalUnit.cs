using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockEntityTemporalDisposalUnit : BlockEntityGenericContainer
{
    private static readonly string[] RunningVisualStates = ["-running-", "-running0-", "-running100-"];
    private const int InputSlotCount = 8;
    private const int ActivationSlotId = 8;
    private const int StartupDelayMs = 3000;
    private const int DisposalCycleMs = 5000;
    private const int DisposalVisualPulseMs = 250;
    private const int PhaseStateTickMs = 100;
    private const float MachineLoopBaseVolume = 0.22f;
    private const float MachineLoopFadeDistance = 16f;
    private const float MachineLoopFadeInSeconds = 0.9f;

    private bool isDisposing;
    private int activeInputSlotId = -1;
    private long startupUntilMs;
    private long disposalStartedAtMs;
    private long disposalCompleteAtMs;
    private long progressListenerId;
    private long phaseStateListenerId;
    private long clientSoundCueId;
    private string clientSoundCuePath = string.Empty;
    private long lastHandledClientSoundCueId;
    private ILoadedSound? machineLoopSound;

    public bool IsDisposing => isDisposing;
    public bool HasActivationGear => IsTemporalGear(Inventory[ActivationSlotId].Itemstack);

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        Inventory.SlotModified += OnInventorySlotModified;
        UpdateServerPhaseListener();
        UpdateClientMachineLoopSound();
        EvaluateDisposalState();
        UpdateVisualState(ShouldDisplayRunningEffects());
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        isDisposing = tree.GetBool("isDisposing");
        activeInputSlotId = tree.GetInt("activeInputSlotId", -1);
        startupUntilMs = tree.GetLong("startupUntilMs");
        disposalStartedAtMs = tree.GetLong("disposalStartedAtMs");
        disposalCompleteAtMs = tree.GetLong("disposalCompleteAtMs");
        clientSoundCueId = tree.GetLong("clientSoundCueId");
        clientSoundCuePath = tree.GetString("clientSoundCuePath", string.Empty);

        if (Api != null)
        {
            if (Api.Side == EnumAppSide.Server && isDisposing && progressListenerId == 0)
            {
                RegisterProgressListener();
            }

            UpdateVisualState(ShouldDisplayRunningEffects());
            UpdateServerPhaseListener();
            UpdateClientMachineLoopSound();
            if (Api.Side == EnumAppSide.Client)
            {
                TryPlayQueuedClientSound();
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetBool("isDisposing", isDisposing);
        tree.SetInt("activeInputSlotId", activeInputSlotId);
        tree.SetLong("startupUntilMs", startupUntilMs);
        tree.SetLong("disposalStartedAtMs", disposalStartedAtMs);
        tree.SetLong("disposalCompleteAtMs", disposalCompleteAtMs);
        tree.SetLong("clientSoundCueId", clientSoundCueId);
        tree.SetString("clientSoundCuePath", clientSoundCuePath);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);
        sb.AppendLine($"Queued items: {CountQueuedItems()}");
        sb.AppendLine(HasActivationGear ? "Temporal gear: Inserted" : "Temporal gear: Missing");

        if (IsStartupActive())
        {
            sb.AppendLine($"Startup in {(Math.Max(0, startupUntilMs - Api.World.ElapsedMilliseconds) + 999) / 1000}s");
        }
        else if (isDisposing)
        {
            sb.AppendLine($"Temporal erasure in {(Math.Max(0, disposalCompleteAtMs - Api.World.ElapsedMilliseconds) + 999) / 1000}s");
        }
        else
        {
            sb.AppendLine("Awaiting chronally expendable matter.");
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (Api?.Side == EnumAppSide.Client)
        {
            toggleInventoryDialogClient(byPlayer, () =>
            {
                ICoreClientAPI capi = (ICoreClientAPI)Api;
                return new GuiDialogBlockEntityTemporalDisposalUnit(
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
        if (CountQueuedItems() == 0)
        {
            return "Place unwanted items inside. Insert a temporal gear to arm permanent disposal.";
        }

        ItemStack? activationStack = Inventory[ActivationSlotId].Itemstack;
        if (activationStack != null && !IsTemporalGear(activationStack))
        {
            return "Only a temporal gear can stabilize the disposal aperture.";
        }

        if (!HasActivationGear)
        {
            return "Insert a temporal gear to arm the disposal aperture. Stored items will remain untouched until then.";
        }

        if (IsStartupActive())
        {
            return "The disposal aperture is winding up.";
        }

        if (isDisposing)
        {
            long remainingMs = Math.Max(0, disposalCompleteAtMs - Api!.World.ElapsedMilliseconds);
            return $"Queued matter is slipping out of time. Erasure in {(remainingMs + 999) / 1000}s.";
        }

        return "Disposal aperture armed. Queued items are ready to be lost to time.";
    }

    private void OnInventorySlotModified(int slotId)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        bool removedActiveItem = slotId == activeInputSlotId && GetActiveInputSlot()?.Empty != false;
        if ((isDisposing || IsStartupActive()) && (!HasActivationGear || removedActiveItem))
        {
            StopDisposal();
            return;
        }

        EvaluateDisposalState();
    }

    private void EvaluateDisposalState()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isDisposing || IsStartupActive())
        {
            MarkDirty(true);
            return;
        }

        int nextSlotId = FindFirstOccupiedInputSlotId();
        if (!HasActivationGear || nextSlotId < 0)
        {
            UpdateVisualState(false);
            UpdateClientMachineLoopSound();
            MarkDirty(true);
            return;
        }

        BeginStartup(nextSlotId);
    }

    private void BeginStartup(int slotId)
    {
        activeInputSlotId = slotId;
        startupUntilMs = Api!.World.ElapsedMilliseconds + StartupDelayMs;
        UpdateVisualState(true);
        UpdateServerPhaseListener();
        MarkDirty(true);
    }

    private void BeginDisposal()
    {
        isDisposing = true;
        startupUntilMs = 0;
        disposalStartedAtMs = Api!.World.ElapsedMilliseconds;
        disposalCompleteAtMs = disposalStartedAtMs + DisposalCycleMs;
        UpdateVisualState(true);
        RegisterProgressListener();
        UpdateServerPhaseListener();
        UpdateClientMachineLoopSound();
        MarkDirty(true);
    }

    private void RegisterProgressListener()
    {
        if (progressListenerId != 0 || Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        progressListenerId = RegisterGameTickListener(_ => OnProgressTick(), 100);
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

    private void OnProgressTick()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (!isDisposing)
        {
            UnregisterProgressListener();
            return;
        }

        if (Api.World.ElapsedMilliseconds < disposalCompleteAtMs)
        {
            UpdateVisualState(true);
            return;
        }

        CompleteDisposal();
    }

    private void CompleteDisposal()
    {
        for (int slotId = 0; slotId < InputSlotCount; slotId++)
        {
            ItemSlot slot = Inventory[slotId];
            if (slot.Itemstack == null)
            {
                continue;
            }

            slot.Itemstack = null;
            slot.MarkDirty();
        }
        QueueClientSoundCue("machine-shutoff");
        StopDisposal();
    }

    private void StopDisposal()
    {
        isDisposing = false;
        activeInputSlotId = -1;
        startupUntilMs = 0;
        disposalStartedAtMs = 0;
        disposalCompleteAtMs = 0;
        UnregisterProgressListener();
        UpdateVisualState(false);
        UpdateServerPhaseListener();
        UpdateClientMachineLoopSound();
        MarkDirty(true);
    }

    private ItemSlot? GetActiveInputSlot()
    {
        return activeInputSlotId >= 0 && activeInputSlotId < InputSlotCount ? Inventory[activeInputSlotId] : null;
    }

    private int FindFirstOccupiedInputSlotId()
    {
        for (int slotId = 0; slotId < InputSlotCount; slotId++)
        {
            if (Inventory[slotId].Itemstack != null)
            {
                return slotId;
            }
        }

        return -1;
    }

    private int CountQueuedItems()
    {
        int count = 0;
        for (int slotId = 0; slotId < InputSlotCount; slotId++)
        {
            ItemStack? stack = Inventory[slotId].Itemstack;
            if (stack != null)
            {
                count += stack.StackSize;
            }
        }

        return count;
    }

    private static bool IsTemporalGear(ItemStack? stack)
    {
        return stack?.Collectible?.Code?.Domain == "game"
            && string.Equals(stack.Collectible.Code.Path, "gear-temporal", StringComparison.OrdinalIgnoreCase);
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
        Block? updatedBlock = Api.World.GetBlock(new AssetLocation(Block.Code.Domain, updatedPath));
        if (updatedBlock == null || updatedBlock.Id == Block.Id)
        {
            return;
        }

        Api.World.BlockAccessor.ExchangeBlock(updatedBlock.Id, Pos);
    }

    private string GetRunningVisualStateSegment()
    {
        if (IsStartupActive())
        {
            long prepElapsedMs = Math.Max(0, Api!.World.ElapsedMilliseconds - Math.Max(0, startupUntilMs - StartupDelayMs));
            return (prepElapsedMs / DisposalVisualPulseMs) % 2 == 0 ? "-idle-" : "-running0-";
        }

        return "-running100-";
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

    private bool IsStartupActive()
    {
        return startupUntilMs > 0 && (Api?.World.ElapsedMilliseconds ?? 0) < startupUntilMs;
    }

    private void UpdateServerPhaseListener()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        bool needsPhaseListener = startupUntilMs > 0;
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
        if (startupUntilMs > 0 && now >= startupUntilMs)
        {
            BeginDisposal();
            return;
        }

        if (startupUntilMs > 0)
        {
            UpdateVisualState(true);
        }

        UpdateServerPhaseListener();
    }

    private bool ShouldDisplayRunningEffects()
    {
        return isDisposing || IsStartupActive();
    }

    private void UpdateClientMachineLoopSound()
    {
        if (Api?.Side != EnumAppSide.Client)
        {
            return;
        }

        if (!isDisposing)
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

        float fade = GetClientDistanceFade(soundPos);
        float fadeIn = GameMath.Clamp((Api.World.ElapsedMilliseconds - Math.Max(0, disposalStartedAtMs)) / (MachineLoopFadeInSeconds * 1000f), 0f, 1f);
        machineLoopSound.SetVolume(MachineLoopBaseVolume * fade * fadeIn);
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

        PlayOneShotSoundClient(clientSoundCuePath, MachineLoopBaseVolume);
        lastHandledClientSoundCueId = clientSoundCueId;
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

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        UnregisterProgressListener();
        if (phaseStateListenerId != 0)
        {
            UnregisterGameTickListener(phaseStateListenerId);
            phaseStateListenerId = 0;
        }
        DisposeMachineLoopSound();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        UnregisterProgressListener();
        if (phaseStateListenerId != 0)
        {
            UnregisterGameTickListener(phaseStateListenerId);
            phaseStateListenerId = 0;
        }
        DisposeMachineLoopSound();
    }

    public override void OnExchanged(Block block)
    {
        base.OnExchanged(block);

        if (isDisposing && Api?.Side == EnumAppSide.Server && progressListenerId == 0)
        {
            RegisterProgressListener();
        }

        UpdateServerPhaseListener();
        UpdateClientMachineLoopSound();
    }
}
