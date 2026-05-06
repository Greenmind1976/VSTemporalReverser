using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSTemporalReverser;

public class BlockRestoredMetalTableLow : Block
{
    public const string ExtensionBlockCode = "restored-metal-table-low-extension";

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (!base.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
        {
            return false;
        }

        BlockPos extensionPos = GetExtensionPos(blockSel.Position);
        BlockSelection extensionSel = new()
        {
            Position = extensionPos,
            Face = blockSel.Face,
            DidOffset = blockSel.DidOffset,
            HitPosition = blockSel.HitPosition,
            SelectionBoxIndex = blockSel.SelectionBoxIndex
        };

        if (!base.CanPlaceBlock(world, byPlayer, extensionSel, ref failureCode))
        {
            return false;
        }

        if (!base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
        {
            return false;
        }

        Block? extensionBlock = world.GetBlock(GetExtensionCode());
        if (extensionBlock == null)
        {
            world.BlockAccessor.SetBlock(0, blockSel.Position);
            failureCode = "missingblock";
            return false;
        }

        world.BlockAccessor.SetBlock(extensionBlock.BlockId, extensionPos);
        extensionBlock.OnBlockPlaced(world, extensionPos, itemstack);
        return true;
    }

    public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
    {
        base.OnBlockRemoved(world, pos);

        BlockPos extensionPos = GetExtensionPos(pos);
        Block extensionBlock = world.BlockAccessor.GetBlock(extensionPos);
        if (extensionBlock.Code != null && extensionBlock.Code.Equals(GetExtensionCode()))
        {
            world.BlockAccessor.SetBlock(0, extensionPos);
        }
    }

    public static BlockPos GetExtensionPos(BlockPos pos)
    {
        return pos.WestCopy();
    }

    private AssetLocation GetExtensionCode()
    {
        string metal = Variant["metal"];
        string topmetal = Variant["topmetal"];
        return new AssetLocation(Code.Domain, $"{ExtensionBlockCode}-{metal}-{topmetal}");
    }
}
