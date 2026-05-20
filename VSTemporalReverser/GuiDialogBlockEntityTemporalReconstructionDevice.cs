using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSTemporalReverser;

public class GuiDialogBlockEntityTemporalReconstructionDevice : GuiDialogBlockEntity
{
    private const string RepairSlotKey = "repairslot";
    private const string FuelSlotKey = "fuelslot";
    private const string OutputSlotKey = "outputslot";
    private const string StatusTextKey = "statustext";
    private const int StatusRefreshMs = 100;

    private readonly EnumPosFlag screenPos;
    private long statusListenerId;
    private string lastStatusText = string.Empty;

    protected override double FloatyDialogPosition => 0.6;
    protected override double FloatyDialogAlign => 0.8;

    public override double DrawOrder => 0.2;

    public GuiDialogBlockEntityTemporalReconstructionDevice(
        string dialogTitle,
        InventoryBase inventory,
        BlockPos blockEntityPos,
        ICoreClientAPI capi)
        : base(dialogTitle, inventory, blockEntityPos, capi)
    {
        if (IsDuplicate)
        {
            return;
        }

        screenPos = GetFreePos("smallblockgui");
        SetupDialog();
    }

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();

        if (capi.Gui.GetDialogPosition(SingleComposer.DialogName) == null)
        {
            OccupyPos("smallblockgui", screenPos);
        }

        RefreshStatusText();
        statusListenerId = capi.Event.RegisterGameTickListener(_ => RefreshStatusText(), StatusRefreshMs);
    }

    public override void OnGuiClosed()
    {
        if (statusListenerId != 0)
        {
            capi.Event.UnregisterGameTickListener(statusListenerId);
            statusListenerId = 0;
        }

        GuiComposerHelpers.GetSlotGrid(SingleComposer, RepairSlotKey)?.OnGuiClosed(capi);
        GuiComposerHelpers.GetSlotGrid(SingleComposer, FuelSlotKey)?.OnGuiClosed(capi);
        GuiComposerHelpers.GetSlotGrid(SingleComposer, OutputSlotKey)?.OnGuiClosed(capi);

        base.OnGuiClosed();
        FreePos("smallblockgui", screenPos);
    }

    private void SetupDialog()
    {
        ElementBounds contentBounds = ElementBounds.Fixed(0.0, 0.0, 520.0, 360.0);
        ElementBounds insetBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        insetBounds.BothSizing = ElementSizing.FitToChildren;
        insetBounds.WithChildren(contentBounds);

        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
            .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0.0)
            .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle);

        if (!capi.Settings.Bool["immersiveMouseMode"])
        {
            dialogBounds.fixedOffsetY += (contentBounds.fixedHeight + 30.0) * YOffsetMul(screenPos);
            dialogBounds.fixedOffsetX += (contentBounds.fixedWidth + 10.0) * XOffsetMul(screenPos);
        }

        ElementBounds repairLabelBounds = ElementBounds.Fixed(0.0, 10.0, 140.0, 20.0);
        ElementBounds repairSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 30.0, 4, 2);
        ElementBounds fuelLabelBounds = ElementBounds.Fixed(0.0, 142.0, 100.0, 20.0);
        ElementBounds fuelSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24.0, 166.0, 1, 1);
        ElementBounds statusLabelBounds = ElementBounds.Fixed(116.0, 142.0, 180.0, 20.0);
        ElementBounds statusBounds = ElementBounds.Fixed(116.0, 166.0, 208.0, 58.0);
        ElementBounds outputLabelBounds = ElementBounds.Fixed(0.0, 242.0, 180.0, 20.0);
        ElementBounds outputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24.0, 266.0, 6, 2);

        SingleComposer = capi.Gui
            .CreateCompo("temporalreconstructiondevice-" + BlockEntityPosition, dialogBounds)
            .AddShadedDialogBG(insetBounds, true)
            .AddDialogTitleBar(DialogTitle, CloseIconPressed)
            .BeginChildElements(insetBounds)
            .AddStaticText("Repair Queue", CairoFont.WhiteDetailText(), repairLabelBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 4, new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, repairSlotBounds, RepairSlotKey)
            .AddStaticText("Fuel", CairoFont.WhiteDetailText(), fuelLabelBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 8 }, fuelSlotBounds, FuelSlotKey)
            .AddStaticText("Temporal Reading", CairoFont.WhiteDetailText(), statusLabelBounds)
            .AddDynamicText(string.Empty, CairoFont.WhiteSmallText(), statusBounds, StatusTextKey)
            .AddStaticText("Reconstructed Output", CairoFont.WhiteDetailText(), outputLabelBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 6, new[] { 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, outputSlotBounds, OutputSlotKey)
            .EndChildElements()
            .Compose(true);

        SingleComposer.UnfocusOwnElements();
    }

    private void RefreshStatusText()
    {
        if (SingleComposer == null)
        {
            return;
        }

        string statusText = GetCurrentStatusText();
        if (statusText == lastStatusText)
        {
            return;
        }

        SingleComposer.GetDynamicText(StatusTextKey)?.SetNewText(statusText, true, true, false);
        lastStatusText = statusText;
    }

    private string GetCurrentStatusText()
    {
        if (capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) is not BlockEntityTemporalReconstructionDevice be)
        {
            return "Reconstructor unavailable.";
        }

        string status = be.GetStatusText();
        return string.IsNullOrWhiteSpace(status)
            ? "Insert damaged items and add 10 temporal dust to begin reconstruction."
            : status;
    }
}
