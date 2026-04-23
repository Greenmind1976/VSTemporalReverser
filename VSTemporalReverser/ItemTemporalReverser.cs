using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSTemporalReverser;

public class ItemTemporalReverser : Item
{
    private const int AgedDurabilityCost = 1;
    private const int RuinedDurabilityCost = 2;
    private static readonly string[] RandomRestoredWoodTypes =
    [
        "mahogany",
        "walnut",
        "oak",
        "maple",
        "pine",
        "redwood"
    ];
    private static readonly string[] RandomOpenRestoredCanopyBedStyles =
    [
        "morningstaropen",
        "blueplaidopen",
        "greenplaidopen",
        "redplaidopen",
        "honeycombopen"
    ];

    private static readonly string[] RandomAnyRestoredCanopyBedStyles =
    [
        "morningstaropen",
        "blueplaidopen",
        "greenplaidopen",
        "redplaidopen",
        "honeycombopen",
        "morningstaropened",
        "blueplaidopened",
        "greenplaidopened",
        "redplaidopened",
        "honeycombopened",
        "morningstarclosed",
        "blueplaidclosed",
        "greenplaidclosed",
        "redplaidclosed",
        "honeycombclosed"
    ];

    private static readonly string[] RandomNonGreenOpenRestoredCanopyBedStyles =
    [
        "morningstaropen",
        "blueplaidopen",
        "redplaidopen",
        "honeycombopen"
    ];

    private static readonly string[] RandomNonGreenOpenedRestoredCanopyBedStyles =
    [
        "honeycombopened",
        "morningstaropened",
        "blueplaidopened",
        "redplaidopened"
    ];

    private static readonly string[] RandomNonGreenClosedRestoredCanopyBedStyles =
    [
        "morningstarclosed",
        "blueplaidclosed",
        "redplaidclosed",
        "honeycombclosed"
    ];

    private static readonly Dictionary<string, RestorationRule> CanopyBedRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fancy-bed-green"] = RestoredCanopyBedRule(AgedDurabilityCost, "greenplaidopen"),
        ["fancy-bed-stitched-ruined"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined1"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined2"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined3"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined4"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined5"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["bed/bed-fancy-ruined6"] = RandomRestoredCanopyBedRule(RuinedDurabilityCost, RandomAnyRestoredCanopyBedStyles),
        ["fancy-bed-old"] = RandomRestoredCanopyBedRule(AgedDurabilityCost, RandomNonGreenOpenRestoredCanopyBedStyles),
        ["fancy-bed-old-drapes-opened"] = RandomRestoredCanopyBedRule(AgedDurabilityCost, RandomNonGreenOpenedRestoredCanopyBedStyles),
        ["fancy-bed-old-drapes-closed"] = RandomRestoredCanopyBedRule(AgedDurabilityCost, RandomNonGreenClosedRestoredCanopyBedStyles),
        ["fancy-bed-green-drapes-opened"] = RestoredCanopyBedRule(AgedDurabilityCost, "greenplaidopened"),
        ["fancy-bed-green-drapes-closed"] = RestoredCanopyBedRule(AgedDurabilityCost, "greenplaidclosed"),
    };

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine();
        dsc.AppendLine("Restores aged or ruined canopy bed clutter into a restored canopy bed.");
    }

    public override void OnHeldInteractStart(
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handling)
    {
        if (!firstEvent || slot?.Itemstack == null || blockSel == null)
        {
            return;
        }

        IWorldAccessor world = byEntity.World;
        if (world.Side != EnumAppSide.Server)
        {
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        BlockPos pos = blockSel.Position;
        Block block = world.BlockAccessor.GetBlock(pos);
        if (block?.Code == null || block.Code.Domain != "game" || block.Code.Path != "clutter")
        {
            return;
        }

        string? clutterType = GetClutterType(world, pos);
        if (clutterType == null || !CanopyBedRules.TryGetValue(clutterType, out RestorationRule rule))
        {
            SendNotification(byEntity, "The reverser hums, but finds no canopy bed pattern to restore.");
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        ItemStack? restoredStack = CreateRestoredStack(world, rule);
        if (restoredStack == null)
        {
            SendNotification(byEntity, $"Could not find restored target {rule.Target}.");
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        Vec3d dropPos = pos.ToVec3d().Add(0.5, 0.25, 0.5);

        world.BlockAccessor.SetBlock(0, pos);
        world.SpawnItemEntity(restoredStack, dropPos);
        DamageItem(world, byEntity, slot, rule.DurabilityCost);

        world.PlaySoundAt(new AssetLocation("game", "sounds/effect/translocate"), pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5);
        SendNotification(byEntity, "The canopy bed settles back into a usable shape.");

        handling = EnumHandHandling.PreventDefault;
    }

    private static ItemStack? CreateRestoredStack(IWorldAccessor world, RestorationRule rule)
    {
        if (rule.TargetKind == RestorationTargetKind.RandomRestoredCanopyBed)
        {
            string[] styles = rule.Targets ?? Array.Empty<string>();
            if (styles.Length == 0)
            {
                return null;
            }

            string style = styles[Random.Shared.Next(styles.Length)];
            string wood = RandomRestoredWoodTypes[Random.Shared.Next(RandomRestoredWoodTypes.Length)];
            Block? randomBlock = world.GetBlock(ToAssetLocation($"vstemporalreverser:restored-canopy-bed-{style}-{wood}-head-north"));
            return randomBlock == null ? null : new ItemStack(randomBlock, 1);
        }

        if (rule.TargetKind == RestorationTargetKind.Block)
        {
            string wood = RandomRestoredWoodTypes[Random.Shared.Next(RandomRestoredWoodTypes.Length)];
            string targetCode = rule.Target.Replace("{wood}", wood, StringComparison.Ordinal);
            Block? block = world.GetBlock(ToAssetLocation(targetCode));
            return block == null ? null : new ItemStack(block, 1);
        }

        Block? clutterBlock = world.GetBlock(new AssetLocation("game", "clutter"));
        if (clutterBlock == null)
        {
            return null;
        }

        ItemStack stack = new(clutterBlock, 1);
        stack.Attributes.SetString("type", rule.Target);
        stack.Attributes.SetBool("collected", true);
        return stack;
    }

    private static AssetLocation ToAssetLocation(string code)
    {
        int domainSeparator = code.IndexOf(':');
        return domainSeparator < 0
            ? new AssetLocation("game", code)
            : new AssetLocation(code[..domainSeparator], code[(domainSeparator + 1)..]);
    }

    private static string? GetClutterType(IWorldAccessor world, BlockPos pos)
    {
        BlockEntity? blockEntity = world.BlockAccessor.GetBlockEntity(pos);
        if (blockEntity == null)
        {
            return null;
        }

        TreeAttribute tree = new();
        blockEntity.ToTreeAttributes(tree);

        string? direct = ReadNonEmptyString(tree, "type");
        if (direct != null) return direct;

        ITreeAttribute? attributes = tree.GetTreeAttribute("attributes");
        string? nested = ReadNonEmptyString(attributes, "type");
        if (nested != null) return nested;

        ITreeAttribute? stack = tree.GetTreeAttribute("stack");
        string? stackType = ReadNonEmptyString(stack, "type");
        if (stackType != null) return stackType;

        return null;
    }

    private static string? ReadNonEmptyString(ITreeAttribute? tree, string key)
    {
        if (tree == null) return null;

        string value = tree.GetString(key, "");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void SendNotification(EntityAgent byEntity, string message)
    {
        if (byEntity is not EntityPlayer entityPlayer || entityPlayer.Player is not IServerPlayer serverPlayer)
        {
            return;
        }

        serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, message, EnumChatType.Notification);
    }

    private static RestorationRule ClutterRule(int durabilityCost, string clutterType)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.ClutterType, clutterType);
    }

    private static RestorationRule RestoredCanopyBedRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-canopy-bed-{style}-{{wood}}-head-north");
    }

    private static RestorationRule RandomRestoredCanopyBedRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomRestoredCanopyBed, string.Empty, styles);
    }

    private enum RestorationTargetKind
    {
        Block,
        RandomRestoredCanopyBed,
        ClutterType
    }

    private readonly record struct RestorationRule(int DurabilityCost, RestorationTargetKind TargetKind, string Target, string[]? Targets = null);
}
