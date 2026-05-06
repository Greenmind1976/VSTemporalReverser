using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSTemporalReverser;

public class BlockRestoredMetalTable : Block
{
    public const string ExtensionBlockCode = "restored-metal-table-extension";

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (!base.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
        {
            return false;
        }

        if (!base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
        {
            return false;
        }

        Block placedBlock = world.BlockAccessor.GetBlock(blockSel.Position);
        if (placedBlock is not BlockRestoredMetalTable placedTable)
        {
            failureCode = "missingblock";
            world.BlockAccessor.SetBlock(0, blockSel.Position);
            return false;
        }

        BlockPos extensionPos = placedTable.GetExtensionPos(blockSel.Position);
        Block? extensionBlock = world.GetBlock(placedTable.GetExtensionCode());
        if (extensionBlock == null)
        {
            world.BlockAccessor.SetBlock(0, blockSel.Position);
            failureCode = "missingblock";
            return false;
        }

        BlockSelection extensionSel = new()
        {
            Position = extensionPos,
            Face = blockSel.Face,
            DidOffset = blockSel.DidOffset,
            HitPosition = blockSel.HitPosition,
            SelectionBoxIndex = blockSel.SelectionBoxIndex
        };

        if (!extensionBlock.CanPlaceBlock(world, byPlayer, extensionSel, ref failureCode))
        {
            world.BlockAccessor.SetBlock(0, blockSel.Position);
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

    public BlockPos GetExtensionPos(BlockPos pos)
    {
        return pos.AddCopy(BlockFacing.FromCode(Variant["side"]).Opposite);
    }

    public AssetLocation GetExtensionCode()
    {
        string style = Variant["style"];
        string metal = Variant["metal"];
        string side = Variant["side"];
        return new AssetLocation(Code.Domain, $"{ExtensionBlockCode}-{style}-{metal}-{side}");
    }
}
