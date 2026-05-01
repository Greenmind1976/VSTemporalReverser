using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace VSTemporalReverser;

public class BlockRestoredBookSurface : Block
{
    private WorldInteraction[]? interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        interactions = ObjectCacheUtil.GetOrCreate(api, "temporalBookSurfaceInteractions", () => new[]
        {
            new WorldInteraction
            {
                MouseButton = EnumMouseButton.Right,
                ActionLangCode = "blockhelp-displaycase-place"
            },
            new WorldInteraction
            {
                MouseButton = EnumMouseButton.Right,
                RequireFreeHand = true,
                ActionLangCode = "blockhelp-displaycase-remove"
            }
        });
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityRestoredBookSurface be)
        {
            return be.OnInteract(byPlayer);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityRestoredBookSurface be)
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
