using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace VSTemporalReverser;

internal static class BrokenToolStackHelper
{
    private const string BrokenProxyCode = "vstemporalreverser:broken-durable-item";
    private const string OriginalStackAttribute = "vstemporalreverser:originalStack";
    private const string BrokenHintSentAttribute = "vstemporalreverser:brokenHintSent";

    public static bool IsManagedDurable(CollectibleObject? collectible)
    {
        return collectible?.CollectibleBehaviors is { Length: > 0 } behaviors
            && Array.Exists(behaviors, behavior => behavior is BrokenToolBehavior);
    }

    public static bool IsBrokenProxy(ItemStack? stack)
    {
        string? code = stack?.Collectible?.Code?.ToShortString();
        return code != null
            && code.StartsWith(BrokenProxyCode, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryCreateBrokenProxy(ICoreAPI api, ItemStack originalStack, out ItemStack? brokenStack)
    {
        brokenStack = null;

        Item? brokenItem = api.World.GetItem(new AssetLocation(BrokenProxyCode));
        if (brokenItem == null)
        {
            return false;
        }

        ItemStack originalClone = originalStack.Clone();
        originalClone.StackSize = 1;

        brokenStack = new ItemStack(brokenItem, 1);
        TreeAttribute tree = brokenStack.Attributes as TreeAttribute ?? new TreeAttribute();
        tree.SetItemstack(OriginalStackAttribute, originalClone);
        tree.SetBool(BrokenHintSentAttribute, false);
        ApplyBrokenTextureOverrides(originalStack, tree);
        brokenStack.Attributes = tree;
        return true;
    }

    private static void ApplyBrokenTextureOverrides(ItemStack originalStack, TreeAttribute tree)
    {
        TreeAttribute? textures = CreateBrokenTextureOverrides(originalStack);
        if (textures == null)
        {
            return;
        }

        TreeAttribute typeAttributes = new();
        typeAttributes["textures"] = textures.Clone();

        tree["textures"] = textures.Clone();
        tree["typeAttributes"] = typeAttributes;
    }

    private static TreeAttribute? CreateBrokenTextureOverrides(ItemStack originalStack)
    {
        TreeAttribute textures = new();
        string path = originalStack.Collectible.Code?.Path ?? string.Empty;

        // Reuse the original silhouette and just age the wood or metal textures.
        textures.SetString("handle", "vstemporalreverser:block/wood/debarked/aged");
        textures.SetString("maple", "vstemporalreverser:block/wood/debarked/aged");

        if (path.StartsWith("bow-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("sling", StringComparison.OrdinalIgnoreCase))
        {
            textures.SetString("feather", "game:item/tool/feather2");
            return textures;
        }

        string? tarnishedMetal = TryGetTarnishedMetalTexturePath(originalStack);
        if (string.IsNullOrWhiteSpace(tarnishedMetal))
        {
            return textures.Count > 0 ? textures : null;
        }

        textures.SetString("material", tarnishedMetal);
        textures.SetString("metal", tarnishedMetal);
        textures.SetString("iron", tarnishedMetal);

        return textures;
    }

    private static string? TryGetTarnishedMetalTexturePath(ItemStack originalStack)
    {
        string path = originalStack.Collectible.Code?.Path ?? string.Empty;
        string[] parts = path.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        string material = parts[^1].ToLowerInvariant() switch
        {
            "bronze" => "tinbronze",
            var value => value
        };

        return material switch
        {
            "copper" => "vstemporalreverser:block/metal/tarnished/copper",
            "tinbronze" => "vstemporalreverser:block/metal/tarnished/tinbronze",
            "bismuthbronze" => "vstemporalreverser:block/metal/tarnished/bismuthbronze",
            "blackbronze" => "vstemporalreverser:block/metal/tarnished/blackbronze",
            "iron" => "vstemporalreverser:block/metal/tarnished/iron",
            "meteoriciron" => "vstemporalreverser:block/metal/tarnished/meteoriciron",
            "steel" => "vstemporalreverser:block/metal/tarnished/steel",
            _ => null
        };
    }

    public static bool TryGetOriginalStack(ItemStack? brokenProxyStack, IWorldAccessor world, out ItemStack? originalStack)
    {
        originalStack = null;
        if (!IsBrokenProxy(brokenProxyStack) || brokenProxyStack?.Attributes is not TreeAttribute tree)
        {
            return false;
        }

        ItemStack? storedStack = tree.GetItemstack(OriginalStackAttribute, null);
        if (storedStack == null)
        {
            return false;
        }

        ItemStack clone = storedStack.Clone();
        if (!clone.ResolveBlockOrItem(world))
        {
            return false;
        }

        originalStack = clone;
        return true;
    }

    public static bool TryGetBrokenRenderStack(ItemStack? brokenProxyStack, IWorldAccessor world, out ItemStack? renderStack)
    {
        renderStack = null;
        if (!TryGetOriginalStack(brokenProxyStack, world, out ItemStack? originalStack) || originalStack == null)
        {
            return false;
        }

        renderStack = originalStack.Clone();

        if (brokenProxyStack?.Attributes is not TreeAttribute brokenTree)
        {
            return true;
        }

        TreeAttribute renderAttributes = renderStack.Attributes as TreeAttribute ?? new TreeAttribute();

        ITreeAttribute? textures = brokenTree.GetTreeAttribute("textures");
        if (textures != null)
        {
            renderAttributes["textures"] = textures.Clone();
        }

        ITreeAttribute? typeAttributes = brokenTree.GetTreeAttribute("typeAttributes");
        if (typeAttributes != null)
        {
            renderAttributes["typeAttributes"] = typeAttributes.Clone();
        }

        renderStack.Attributes = renderAttributes;
        return true;
    }

    public static string GetBrokenDisplayName(ItemStack itemStack, IWorldAccessor? world)
    {
        if (world != null && TryGetOriginalStack(itemStack, world, out ItemStack? originalStack) && originalStack != null)
        {
            return Lang.Get("vstemporalreverser:broken-durable-name", originalStack.GetName());
        }

        return Lang.Get("vstemporalreverser:broken-durable-generic-name");
    }

    public static void AppendBrokenItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world)
    {
        dsc.AppendLine(Lang.Get("vstemporalreverser:broken-durable-info"));
        if (TryGetOriginalStack(inSlot.Itemstack, world, out ItemStack? originalStack) && originalStack != null)
        {
            dsc.AppendLine(Lang.Get("vstemporalreverser:broken-durable-original", originalStack.GetName()));
        }
    }
}
