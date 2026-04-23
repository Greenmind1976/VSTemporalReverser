using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockRestoredCanopyBed : BlockBed
{
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (!IsClosedCanopyVariant())
        {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
        {
            return false;
        }

        if (!TryGetBedEntity(world, blockSel, out BlockEntityBed? bedEntity))
        {
            return false;
        }

        if (bedEntity!.MountedBy != null)
        {
            return false;
        }

        if (api?.World?.Config?.GetString("temporalStormSleeping", "0") == "0" &&
            api.ModLoader.GetModSystem<SystemTemporalStability>(true).StormStrength > 0f)
        {
            if (world.Side == EnumAppSide.Client)
            {
                ((ICoreClientAPI)api).TriggerIngameError(this, "cantsleep-tempstorm", Lang.Get("cantsleep-tempstorm"));
            }
            else
            {
                byPlayer.Entity.TryUnmount();
            }

            return false;
        }

        return byPlayer.Entity.TryMount(bedEntity);
    }

    private bool IsClosedCanopyVariant()
    {
        string material = LastCodePart(2);
        return material.EndsWith("closed", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryGetBedEntity(IWorldAccessor world, BlockSelection blockSel, out BlockEntityBed? bedEntity)
    {
        string side = LastCodePart(0);
        string part = LastCodePart(1);
        var opposite = Vintagestory.API.MathTools.BlockFacing.FromCode(side).Opposite;
        var entityPos = part == "feet" ? blockSel.Position.AddCopy(opposite) : blockSel.Position;
        bedEntity = world.BlockAccessor.GetBlockEntity(entityPos) as BlockEntityBed;
        return bedEntity != null;
    }
}
