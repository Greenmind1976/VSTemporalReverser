using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSTemporalReverser;

public class ItemRestoredToy : Item
{
    private ItemSlot? iconSlot;

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);

        if (target != EnumItemRenderTarget.Gui)
        {
            return;
        }

        string? iconCode = itemstack?.Collectible?.Attributes?["iconItemCode"].AsString(null);
        if (string.IsNullOrWhiteSpace(iconCode))
        {
            return;
        }

        Item? iconItem = capi.World.GetItem(new AssetLocation("vstemporalreverser", iconCode));
        if (iconItem == null)
        {
            return;
        }

        iconSlot ??= new DummySlot();
        iconSlot.Itemstack = new ItemStack(iconItem, 1);

        renderinfo = capi.Render.GetItemStackRenderInfo(iconSlot, target, 0);
    }
}
