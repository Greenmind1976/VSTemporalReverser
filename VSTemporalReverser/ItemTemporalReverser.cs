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
    private static readonly string[] RandomLanternMaterials =
    [
        "copper",
        "brass",
        "blackbronze",
        "bismuth",
        "tinbronze",
        "bismuthbronze",
        "iron",
        "molybdochalkos",
        "silver",
        "gold",
        "steel",
        "meteoriciron",
        "electrum"
    ];
    private static readonly string[] RandomLanternLinings =
    [
        "plain",
        "silver",
        "gold",
        "electrum"
    ];
    private static readonly string[] RandomTorchholderMaterials =
    [
        "aged",
        "brass"
    ];
    private static readonly string[] RandomTableTypes =
    [
        "normal",
        "aged",
        "whitemarble",
        "redmarble",
        "greenmarble"
    ];
    private static readonly string[] RandomRestoredWoodTypes =
    [
        "mahogany",
        "walnut",
        "oak",
        "maple",
        "pine",
        "redwood"
    ];
    private static readonly string[] RandomRestoredTableWoodTypes =
    [
        "walnut",
        "mahogany",
        "ebony",
        "acacia"
    ];

    private static readonly string[] RandomRestoredAgedTableStyles =
    [
        "agedwhite",
        "agedblue",
        "agedgreen",
        "agedpurple",
        "agedred"
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

    private static readonly string[] RandomRestoredShortBedStyles =
    [
        "morningstar",
        "blueplaid",
        "greenplaid",
        "redplaid",
        "honeycomb"
    ];

    private static readonly string[] RandomNonGreenRestoredShortBedStyles =
    [
        "morningstar",
        "blueplaid",
        "redplaid",
        "honeycomb"
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

    private static readonly Dictionary<string, RestorationRule> BedRules = new(StringComparer.OrdinalIgnoreCase)
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
        ["bed-short-green"] = RestoredShortBedRule(AgedDurabilityCost, "greenplaid"),
        ["bed-short-old"] = RandomRestoredShortBedRule(AgedDurabilityCost, RandomNonGreenRestoredShortBedStyles),
        ["bed-short-stitched-ruined"] = RandomRestoredShortBedRule(RuinedDurabilityCost, RandomRestoredShortBedStyles),
        ["bed/bed-ruined1"] = RandomRestoredShortBedRule(RuinedDurabilityCost, RandomRestoredShortBedStyles),
        ["bed/bed-ruined2"] = RandomRestoredShortBedRule(RuinedDurabilityCost, RandomRestoredShortBedStyles),
        ["bed/bed-ruined3"] = VanillaBedRule(RuinedDurabilityCost, "game:bed-woodaged-head-north"),
        ["bed/bed-ruined4"] = VanillaBedRule(RuinedDurabilityCost, "game:bed-woodaged-head-north"),
        ["bed/bed-ruined5"] = VanillaBedRule(RuinedDurabilityCost, "game:bed-woodaged-head-north"),
        ["bed/bed-ruined6"] = VanillaBedRule(RuinedDurabilityCost, "game:bed-woodaged-head-north"),
        ["table-aged"] = RestoredTableRule(AgedDurabilityCost, "agedwhite"),
        ["table-long"] = RestoredTableRule(AgedDurabilityCost, "scribe"),
        ["table-long-with-accessories"] = RestoredTableRule(AgedDurabilityCost, "scribeaccessories"),
        ["table-long-with-cloth-blue"] = RestoredTableRule(AgedDurabilityCost, "scribeblue"),
        ["table-long-with-cloth-green"] = RestoredTableRule(AgedDurabilityCost, "scribegreen"),
        ["table-long-with-cloth-purple"] = RestoredTableRule(AgedDurabilityCost, "scribepurple"),
        ["table-long-with-cloth-red"] = RestoredTableRule(AgedDurabilityCost, "scribered"),
        ["table-ruined1"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined2"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined3"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined4"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined5"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined6"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["brazier3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-lit"),
        ["brazier4"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-lit"),
        ["brazier-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-lit"),
        ["lantern/ground1"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground2"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground3"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground4"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground5"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground6"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/wall1"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/wall2"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/wall3"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ceiling1"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ceiling2"] = RandomVanillaLanternRule(AgedDurabilityCost),
        ["lantern/ground7"] = RandomVanillaLanternRule(RuinedDurabilityCost),
        ["lantern/ground8"] = RandomVanillaLanternRule(RuinedDurabilityCost),
        ["lantern/wall5"] = RandomVanillaLanternRule(RuinedDurabilityCost),
        ["lantern/ceiling3"] = RandomVanillaLanternRule(RuinedDurabilityCost),
        ["chandelier-ruined1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-chandelier-{material}-candle0"),
        ["chandelier-ruined2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-chandelier-{material}-candle0"),
        ["chandelier-ruined3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-chandelier-{material}-candle0")
    };

    private static readonly Dictionary<string, RestorationRule> BlockRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["torchholder-ruined-empty-north"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-north"),
        ["torchholder-ruined-empty-east"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-east"),
        ["torchholder-ruined-empty-south"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-south"),
        ["torchholder-ruined-empty-west"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-west"),
        ["torchholder-ruined-filled-north"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-north"),
        ["torchholder-ruined-filled-east"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-east"),
        ["torchholder-ruined-filled-south"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-south"),
        ["torchholder-ruined-filled-west"] = VanillaBlockRule(RuinedDurabilityCost, "game:torchholder-{torchholdermaterial}-empty-west")
    };

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine();
        dsc.AppendLine("Restores selected aged or ruined clutter into usable furnishings.");
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
        if (block?.Code == null || block.Code.Domain != "game")
        {
            return;
        }

        RestorationRule? matchedRule = null;
        if (block.Code.Path == "clutter")
        {
            string? clutterType = GetClutterType(world, pos);
            if (clutterType != null && BedRules.TryGetValue(clutterType, out RestorationRule clutterRule))
            {
                matchedRule = clutterRule;
            }
        }
        else if (BlockRules.TryGetValue(block.Code.Path, out RestorationRule blockRule))
        {
            matchedRule = blockRule;
        }

        if (matchedRule == null)
        {
            SendNotification(byEntity, "The reverser hums, but finds no restorable pattern.");
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        RestorationRule rule = matchedRule.Value;

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
        SendNotification(byEntity, "The restored item drops free in a usable shape.");

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

        if (rule.TargetKind == RestorationTargetKind.RandomRestoredShortBed)
        {
            string[] styles = rule.Targets ?? Array.Empty<string>();
            if (styles.Length == 0)
            {
                return null;
            }

            string style = styles[Random.Shared.Next(styles.Length)];
            string wood = RandomRestoredWoodTypes[Random.Shared.Next(RandomRestoredWoodTypes.Length)];
            Block? randomBlock = world.GetBlock(ToAssetLocation($"vstemporalreverser:restored-short-bed-{style}-{wood}-head-north"));
            return randomBlock == null ? null : new ItemStack(randomBlock, 1);
        }

        if (rule.TargetKind == RestorationTargetKind.RandomVanillaLantern)
        {
            Block? lanternBlock = world.GetBlock(ToAssetLocation("game:lantern-large-up"));
            if (lanternBlock == null)
            {
                return null;
            }

            ItemStack lanternStack = new(lanternBlock, 1);
            lanternStack.Attributes.SetString("material", RandomLanternMaterials[Random.Shared.Next(RandomLanternMaterials.Length)]);
            lanternStack.Attributes.SetString("lining", RandomLanternLinings[Random.Shared.Next(RandomLanternLinings.Length)]);
            lanternStack.Attributes.SetString("glass", "quartz");
            lanternStack.ResolveBlockOrItem(world);
            return lanternStack;
        }

        if (rule.TargetKind == RestorationTargetKind.RandomVanillaTable)
        {
            string tableType = RandomTableTypes[Random.Shared.Next(RandomTableTypes.Length)];
            Block? tableBlock = world.GetBlock(ToAssetLocation($"game:table-{tableType}"));
            if (tableBlock == null)
            {
                return null;
            }

            ItemStack tableStack = new(tableBlock, 1);
            tableStack.ResolveBlockOrItem(world);
            return tableStack;
        }

        if (rule.TargetKind == RestorationTargetKind.Block)
        {
            string wood = RandomRestoredWoodTypes[Random.Shared.Next(RandomRestoredWoodTypes.Length)];
            string tableWood = RandomRestoredTableWoodTypes[Random.Shared.Next(RandomRestoredTableWoodTypes.Length)];
            string tableStyle = rule.Targets != null && rule.Targets.Length > 0
                ? rule.Targets[Random.Shared.Next(rule.Targets.Length)]
                : string.Empty;
            string material = RandomLanternMaterials[Random.Shared.Next(RandomLanternMaterials.Length)];
            string torchholderMaterial = RandomTorchholderMaterials[Random.Shared.Next(RandomTorchholderMaterials.Length)];
            string targetCode = rule.Target
                .Replace("{wood}", wood, StringComparison.Ordinal)
                .Replace("{tablestyle}", tableStyle, StringComparison.Ordinal)
                .Replace("{tablewood}", tableWood, StringComparison.Ordinal)
                .Replace("{material}", material, StringComparison.Ordinal);
            targetCode = targetCode.Replace("{torchholdermaterial}", torchholderMaterial, StringComparison.Ordinal);
            Block? block = world.GetBlock(ToAssetLocation(targetCode));
            if (block == null)
            {
                return null;
            }

            ItemStack blockStack = new(block, 1);
            blockStack.ResolveBlockOrItem(world);
            return blockStack;
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

    private static RestorationRule RestoredShortBedRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-short-bed-{style}-{{wood}}-head-north");
    }

    private static RestorationRule RestoredTableRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-table-{style}-{{tablewood}}-north");
    }

    private static RestorationRule RandomRestoredTableRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-table-{{tablestyle}}-{{tablewood}}-north",
            styles);
    }

    private static RestorationRule VanillaBedRule(int durabilityCost, string code)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            code);
    }

    private static RestorationRule VanillaBlockRule(int durabilityCost, string code)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            code);
    }

    private static RestorationRule RandomRestoredShortBedRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomRestoredShortBed, string.Empty, styles);
    }

    private static RestorationRule RandomVanillaLanternRule(int durabilityCost)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomVanillaLantern, "game:lantern-large-up");
    }

    private static RestorationRule RandomVanillaTableRule(int durabilityCost)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomVanillaTable, "game:table-normal");
    }

    private enum RestorationTargetKind
    {
        Block,
        RandomRestoredCanopyBed,
        RandomRestoredShortBed,
        RandomVanillaLantern,
        RandomVanillaTable,
        ClutterType
    }

    private readonly record struct RestorationRule(int DurabilityCost, RestorationTargetKind TargetKind, string Target, string[]? Targets = null);
}
