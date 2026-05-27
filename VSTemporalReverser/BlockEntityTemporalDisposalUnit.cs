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
    private static readonly string[] RunningVisualStates = ["-running-"];
    private const int InputSlotCount = 8;
    private const int ActivationSlotId = 8;
    private const int DisposalDelayMs = 1500;
    private bool isDisposing;
    private int activeInputSlotId = -1;
    private long disposalCompleteAtMs;
    private long progressListenerId;

    public bool IsDisposing => isDisposing;
    public bool HasActivationGear => IsTemporalGear(Inventory[ActivationSlotId].Itemstack);

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        Inventory.SlotModified += OnInventorySlotModified;
        EvaluateDisposalState();
        UpdateVisualState(isDisposing);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        isDisposing = tree.GetBool("isDisposing");
        activeInputSlotId = tree.GetInt("activeInputSlotId", -1);
        disposalCompleteAtMs = tree.GetLong("disposalCompleteAtMs");

        if (Api != null)
        {
            if (Api.Side == EnumAppSide.Server && isDisposing && progressListenerId == 0)
            {
                RegisterProgressListener();
            }

            UpdateVisualState(isDisposing);
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetBool("isDisposing", isDisposing);
        tree.SetInt("activeInputSlotId", activeInputSlotId);
        tree.SetLong("disposalCompleteAtMs", disposalCompleteAtMs);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);
        sb.AppendLine($"Queued items: {CountQueuedItems()}");
        sb.AppendLine(HasActivationGear ? "Temporal gear: Inserted" : "Temporal gear: Missing");

        if (isDisposing)
        {
            long remainingMs = Math.Max(0, disposalCompleteAtMs - Api.World.ElapsedMilliseconds);
            sb.AppendLine($"Temporal erasure in {(remainingMs + 999) / 1000}s");
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

        if (isDisposing)
        {
            long remainingMs = Math.Max(0, disposalCompleteAtMs - Api!.World.ElapsedMilliseconds);
            return $"This item is slipping out of time. Erasure in {(remainingMs + 999) / 1000}s.";
        }

        return "Disposal aperture armed. Queued items will be lost to time permanently.";
    }

    private void OnInventorySlotModified(int slotId)
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isDisposing && (!HasActivationGear || (slotId == activeInputSlotId && GetActiveInputSlot()?.Empty != false)))
        {
            StopDisposal();
        }

        EvaluateDisposalState();
    }

    private void EvaluateDisposalState()
    {
        if (Api?.Side != EnumAppSide.Server)
        {
            return;
        }

        if (isDisposing)
        {
            MarkDirty(true);
            return;
        }

        int nextSlotId = FindFirstOccupiedInputSlotId();
        if (!HasActivationGear || nextSlotId < 0)
        {
            UpdateVisualState(false);
            MarkDirty(true);
            return;
        }

        BeginDisposal(nextSlotId);
    }

    private void BeginDisposal(int slotId)
    {
        isDisposing = true;
        activeInputSlotId = slotId;
        disposalCompleteAtMs = Api!.World.ElapsedMilliseconds + DisposalDelayMs;
        UpdateVisualState(true);
        RegisterProgressListener();
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
            return;
        }

        CompleteDisposal();
    }

    private void CompleteDisposal()
    {
        ItemSlot? activeSlot = GetActiveInputSlot();
        if (activeSlot?.Itemstack != null)
        {
            activeSlot.Itemstack = null;
            activeSlot.MarkDirty();
        }

        StopDisposal();
        EvaluateDisposalState();
    }

    private void StopDisposal()
    {
        isDisposing = false;
        activeInputSlotId = -1;
        disposalCompleteAtMs = 0;
        UnregisterProgressListener();
        UpdateVisualState(false);
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
        string desiredSegment = running ? "-running-" : "-idle-";
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

    private static string ReplaceVisualStateSegment(string path, string desiredSegment)
    {
        string updated = path.Replace("-idle-", desiredSegment, StringComparison.OrdinalIgnoreCase);
        foreach (string state in RunningVisualStates)
        {
            updated = updated.Replace(state, desiredSegment, StringComparison.OrdinalIgnoreCase);
        }

        return updated;
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        UnregisterProgressListener();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        UnregisterProgressListener();
    }
}
