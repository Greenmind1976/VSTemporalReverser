using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSTemporalReverser;

public class GuiDialogBlockEntityTemporalDisposalUnit : GuiDialogBlockEntity
{
    private const string InputSlotKey = "inputslot";
    private const string ActivationSlotKey = "activationslot";
    private const string StatusTextKey = "statustext";
    private const int StatusRefreshMs = 100;

    private readonly EnumPosFlag screenPos;
    private long statusListenerId;
    private string lastStatusText = string.Empty;

    protected override double FloatyDialogPosition => 0.6;
    protected override double FloatyDialogAlign => 0.8;
    public override double DrawOrder => 0.2;

    public GuiDialogBlockEntityTemporalDisposalUnit(
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

        GuiComposerHelpers.GetSlotGrid(SingleComposer, InputSlotKey)?.OnGuiClosed(capi);
        GuiComposerHelpers.GetSlotGrid(SingleComposer, ActivationSlotKey)?.OnGuiClosed(capi);

        base.OnGuiClosed();
        FreePos("smallblockgui", screenPos);
    }

    private void SetupDialog()
    {
        ElementBounds contentBounds = ElementBounds.Fixed(0.0, 0.0, 420.0, 250.0);
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

        ElementBounds inputLabelBounds = ElementBounds.Fixed(0.0, 10.0, 220.0, 20.0);
        ElementBounds inputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 102.0, 38.0, 4, 2);
        ElementBounds activationLabelBounds = ElementBounds.Fixed(0.0, 140.0, 220.0, 20.0);
        ElementBounds activationSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 180.0, 166.0, 1, 1);
        ElementBounds warningLabelBounds = ElementBounds.Fixed(0.0, 198.0, 220.0, 20.0);
        ElementBounds statusBounds = ElementBounds.Fixed(0.0, 222.0, 360.0, 36.0);

        SingleComposer = capi.Gui
            .CreateCompo("temporaldisposalunit-" + BlockEntityPosition, dialogBounds)
            .AddShadedDialogBG(insetBounds, true)
            .AddDialogTitleBar(DialogTitle, CloseIconPressed)
            .BeginChildElements(insetBounds)
            .AddStaticText("Disposal Queue", CairoFont.WhiteDetailText(), inputLabelBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 4, new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, inputSlotBounds, InputSlotKey)
            .AddStaticText("Temporal Gear Arming Slot", CairoFont.WhiteDetailText(), activationLabelBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 8 }, activationSlotBounds, ActivationSlotKey)
            .AddStaticText("Irreversible Disposal", CairoFont.WhiteDetailText(), warningLabelBounds)
            .AddDynamicText(string.Empty, CairoFont.WhiteSmallText(), statusBounds, StatusTextKey)
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
        if (capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) is not BlockEntityTemporalDisposalUnit be)
        {
            return "Disposal unit unavailable.";
        }

        return be.GetStatusText();
    }
}
