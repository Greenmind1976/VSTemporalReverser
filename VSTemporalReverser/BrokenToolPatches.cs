using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace VSTemporalReverser;

[HarmonyPatch(typeof(CollectibleObject))]
internal static class BrokenToolPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CollectibleObject.DamageItem))]
    private static bool BeforeDamageItem(CollectibleObject __instance, IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, int amount, bool destroyOnZeroDurability)
    {
        return TryInterceptBreak(__instance, world, itemSlot, amount);
    }

    [HarmonyPrefix]
    [HarmonyPatch("DestroyItem")]
    private static bool BeforeDestroyItem(CollectibleObject __instance, IWorldAccessor world, Entity byEntity, ItemSlot itemSlot)
    {
        return TryInterceptBreak(__instance, world, itemSlot, 1);
    }

    private static bool TryInterceptBreak(CollectibleObject collectible, IWorldAccessor world, ItemSlot itemSlot, int damageAmount)
    {
        if (!VSTemporalReverserModSystem.Config.PreserveBrokenToolsAtZeroDurability)
        {
            return true;
        }

        if (itemSlot.Itemstack is not ItemStack stack || stack.StackSize != 1 || stack.Item == null)
        {
            return true;
        }

        if (!BrokenToolStackHelper.IsManagedDurable(collectible))
        {
            return true;
        }

        int maxDurability = collectible.GetMaxDurability(stack);
        if (maxDurability <= 0)
        {
            return true;
        }

        int remainingDurability = collectible.GetRemainingDurability(stack);
        if (remainingDurability <= 0)
        {
            return true;
        }

        if (remainingDurability - damageAmount > 0)
        {
            return true;
        }

        if (!BrokenToolStackHelper.TryCreateBrokenProxy(world.Api, stack, out ItemStack? brokenStack) || brokenStack == null)
        {
            return true;
        }

        itemSlot.Itemstack = brokenStack;
        itemSlot.MarkDirty();
        return false;
    }
}
