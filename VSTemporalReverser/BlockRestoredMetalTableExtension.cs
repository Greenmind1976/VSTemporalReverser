using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSTemporalReverser;

public class BlockRestoredMetalTableExtension : Block, IMultiblockOffset
{
    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        return [OnPickBlock(world, pos)];
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        Block mainBlock = world.BlockAccessor.GetBlock(GetControlBlockPos(pos));
        return new ItemStack(mainBlock.Code != null ? mainBlock : this);
    }

    public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
    {
        base.OnBlockRemoved(world, pos);

        BlockPos mainPos = GetControlBlockPos(pos);
        Block mainBlock = world.BlockAccessor.GetBlock(mainPos);
        if (mainBlock is BlockRestoredMetalTable)
        {
            world.BlockAccessor.SetBlock(0, mainPos);
        }
    }

    public BlockPos GetControlBlockPos(BlockPos pos)
    {
        return pos.AddCopy(BlockFacing.FromCode(Variant["side"]));
    }
}
