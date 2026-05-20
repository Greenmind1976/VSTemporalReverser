using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace VSTemporalReverser;

public class BlockTemporalReconstructionDevice : Block
{
    private WorldInteraction[]? interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        interactions = ObjectCacheUtil.GetOrCreate(api, "temporalReconstructionDeviceInteractions", () => new[]
        {
            new WorldInteraction
            {
                MouseButton = EnumMouseButton.Right,
                ActionLangCode = "vstemporalreverser:blockhelp-trd-open"
            }
        });
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityTemporalReconstructionDevice be)
        {
            return be.OnPlayerRightClick(byPlayer, blockSel);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityTemporalReconstructionDevice be)
        {
            be.OnBlockBroken(byPlayer);
        }

        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        return ArrayExtensions.Append(interactions, base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }
}
