using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTemporalReverser;

public class BlockEntityRestoredBookSurface : BlockEntityDisplay
{
    private readonly InventoryGeneric inventory;

    public override string InventoryClassName => "temporalbooksurface";

    public override InventoryBase Inventory => inventory;

    public override string AttributeTransformCode => "onDisplayTransform";

    public BlockEntityRestoredBookSurface()
    {
        inventory = new InventoryGeneric(1, "temporalbooksurface-0", null, (slotId, inv) => new ItemSlot(inv));
    }

    public bool OnInteract(IPlayer byPlayer)
    {
        ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        bool shiftPressed = byPlayer.Entity.Controls.ShiftKey;

        if (activeSlot.Empty)
        {
            return TryTake(byPlayer);
        }

        if (!IsPlaceableBook(activeSlot.Itemstack))
        {
            return false;
        }

        if (!shiftPressed)
        {
            if (Api is ICoreClientAPI capi)
            {
                capi.TriggerIngameError(this, "booksurface-shiftplace", Lang.Get("Hold Shift and right click to place a book on this surface."));
            }

            return true;
        }

        return TryPut(activeSlot, byPlayer);
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        inventory.LateInitialize("temporalbooksurface-0", api);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        inventory.FromTreeAttributes(tree.GetTreeAttribute("inventory"));
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        ITreeAttribute invTree = new TreeAttribute();
        inventory.ToTreeAttributes(invTree);
        tree["inventory"] = invTree;
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        ItemStack? stack = inventory[0].Itemstack;
        if (stack == null || stack.Collectible?.Code == null)
        {
            return true;
        }

        MeshData mesh = getDefaultMesh(stack);

        ModelTransform? meshTransform = Block.Attributes?["displayedItemMeshTransform"].AsObject<ModelTransform>(null);
        if (meshTransform != null)
        {
            meshTransform.EnsureDefaultValues();
            mesh.ModelTransform(meshTransform);
        }

        ModelTransform placementTransform = Block.Attributes?["displayedItemTransform"].AsObject(ModelTransform.NoTransform) ?? ModelTransform.NoTransform;
        placementTransform.EnsureDefaultValues();
        mesher.AddMeshData(mesh, placementTransform.AsMatrix, 1);

        return true;
    }

    protected override float[][] genTransformationMatrices()
    {
        float[][] matrices = new float[1][];
        ModelTransform transform = Block.Attributes?["displayedItemTransform"].AsObject(ModelTransform.NoTransform) ?? ModelTransform.NoTransform;
        transform.EnsureDefaultValues();
        matrices[0] = transform.AsMatrix;
        return matrices;
    }

    private bool TryPut(ItemSlot activeSlot, IPlayer byPlayer)
    {
        if (!inventory[0].Empty)
        {
            return false;
        }

        int moved = activeSlot.TryPutInto(Api.World, inventory[0], 1);
        if (moved <= 0)
        {
            return false;
        }

        Api.World.Logger.Audit("{0} Put 1x{1} onto {2} at {3}.", byPlayer.PlayerName, inventory[0].Itemstack?.Collectible?.Code, Block.Code, Pos);

        if (Api is ICoreClientAPI capi)
        {
            capi.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
        }

        MarkDirty(true);
        return true;
    }

    private bool TryTake(IPlayer byPlayer)
    {
        if (inventory[0].Empty)
        {
            return false;
        }

        ItemStack stack = inventory[0].TakeOut(1);
        if (!byPlayer.InventoryManager.TryGiveItemstack(stack, false))
        {
            Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.25, 0.5));
        }

        Api.World.Logger.Audit("{0} Took 1x{1} from {2} at {3}.", byPlayer.PlayerName, stack.Collectible?.Code, Block.Code, Pos);

        if (Api is ICoreClientAPI capi)
        {
            capi.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
        }

        MarkDirty(true);
        return true;
    }

    private static bool IsPlaceableBook(ItemStack? stack)
    {
        string? path = stack?.Collectible?.Code?.Path;
        if (path == null)
        {
            return false;
        }

        return path.StartsWith("book-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("lore-book-", StringComparison.OrdinalIgnoreCase);
    }
}
