using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockRestoredTorchHolder : BlockTorchHolder
{
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel == null || world.BlockAccessor.GetBlockEntity(blockSel.Position) == null)
        {
            return false;
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}
