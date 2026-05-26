using Vintagestory.API.Client;
using System.Text;
using Vintagestory.API.Common;

namespace VSTemporalReverser;

public sealed class ItemBrokenDurableProxy : Item
{
    private ItemSlot? renderSlot;

    public override int GetMaxDurability(ItemStack itemstack)
    {
        if (api?.World != null
            && BrokenToolStackHelper.TryGetOriginalStack(itemstack, api.World, out ItemStack? originalStack)
            && originalStack != null)
        {
            return originalStack.Collectible.GetMaxDurability(originalStack);
        }

        return base.GetMaxDurability(itemstack);
    }

    public override int GetRemainingDurability(ItemStack itemstack)
    {
        return BrokenToolStackHelper.IsBrokenProxy(itemstack) ? 0 : base.GetRemainingDurability(itemstack);
    }

    public override string GetHeldItemName(ItemStack itemStack)
    {
        return BrokenToolStackHelper.GetBrokenDisplayName(itemStack, api?.World);
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);

        if (!BrokenToolStackHelper.TryGetBrokenRenderStack(itemstack, capi.World, out ItemStack? renderStack) || renderStack == null)
        {
            return;
        }

        renderSlot ??= new DummySlot();
        renderSlot.Itemstack = renderStack;
        renderinfo = capi.Render.GetItemStackRenderInfo(renderSlot, target, 0);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        BrokenToolStackHelper.AppendBrokenItemInfo(inSlot, dsc, world);
    }
}
