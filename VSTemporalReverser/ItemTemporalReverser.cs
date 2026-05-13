using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSTemporalReverser;

public class ItemTemporalReverser : Item
{
    private static readonly object DebugLogLock = new();
    private const bool DefaultBonusLootEnabled = true;
    private const bool DefaultDepleted = false;
    private const bool DefaultRustWardEnabled = true;
    private const bool DefaultSalvageEnabled = false;
    private const string DefaultMetalRestriction = "full";
    private const string CopperMetalRestriction = "copper";
    private const string ToolModeAttribute = "vstemporalreverser:toolMode";
    private const int RestoreToolMode = 0;
    private const int SalvageToolMode = 1;
    private const int AgedDurabilityCost = 1;
    private const int DefaultRestoreSpawnDelayMs = 175;
    private const long DefaultRestoreCooldownMs = 1500;
    private const int RuinedDurabilityCost = 2;
    private const long RustWardPulseIntervalMs = 350;
    private const float RustWardDamage = 0.25f;
    private static string? DebugLogPath;
    private static readonly Dictionary<long, long> LastRestoreUseByEntityId = [];
    private static readonly Dictionary<long, long> LastRustWardPulseByEntityId = [];
    private readonly Dictionary<string, LoadedTexture> toolModeTextures = [];
    private ItemSlot? iconSlot;
    private ICoreClientAPI? capi;
    private SkillItem[]? toolModes;
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
    private static readonly string[] RandomVanillaChairColors =
    [
        "blue",
        "red",
        "yellow",
        "purple",
        "brown",
        "green",
        "orange",
        "black",
        "gray",
        "pink",
        "white"
    ];
    private static readonly string[] RandomRestoredMetalTableClothColors =
    [
        "white",
        "blue",
        "green",
        "purple",
        "red"
    ];
    private static readonly string[] RandomRestoredCenserMetalFinishes =
    [
        "copper",
        "brass",
        "blackbronze",
        "silver",
        "gold",
        "electrum"
    ];
    private static readonly string[] RandomRestoredBedTopMetals =
    [
        "bismuth",
        "blackbronze",
        "molybdochalkos"
    ];
    private static readonly string[] RandomRestoredCenserCeramicFinishes =
    [
        "blue1",
        "brown1",
        "fire1",
        "red1"
    ];
    private static readonly string[] RandomRestoredLibraryMaterials =
    [
        "birch",
        "oak",
        "maple",
        "pine",
        "acacia",
        "kapok",
        "redwood",
        "baldcypress",
        "larch",
        "ebony",
        "walnut",
        "purpleheart",
        "aged",
        "veryaged"
    ];
    private static readonly string[] RandomCrateWoodTypes =
    [
        "aged",
        "birch",
        "oak",
        "maple",
        "pine",
        "acacia",
        "kapok",
        "baldcypress",
        "larch",
        "redwood",
        "ebony",
        "walnut",
        "purpleheart"
    ];
    private static readonly string[] RandomLargeCrateRaccoonEntities =
    [
        "game:raccoon-common-adult-female",
        "game:raccoon-common-adult-male"
    ];
    private static readonly string[] RandomLargeRotCrateMouseEntities =
    [
        "vstemporalreverser:mouse"
    ];
    private static readonly string[] RandomMouseCreatureEntities =
    [
        "vstemporalreverser:mouse"
    ];
    private static readonly string[] RandomMothCreatureEntities =
    [
        "game:butterfly-atlasmothfemale",
        "game:butterfly-atlasmothmale",
        "game:butterfly-gardentigermothmale",
        "game:butterfly-limehawkmoth",
        "game:butterfly-macrocosmamoth",
        "game:butterfly-madagascansunsetmoth",
        "game:butterfly-oleanderhawkmothmale",
        "game:butterfly-sagebrushgirdlemoth"
    ];
    private static readonly string[] RandomTemporalGearItems =
    [
        "game:gear-temporal"
    ];
    private static readonly string[][] RandomCrateCreatureEntityGroups =
    [
        RandomLargeCrateRaccoonEntities,
        RandomLargeRotCrateMouseEntities
    ];
    private static readonly string[] RandomRestoredCommonMetals =
    [
        "copper",
        "tinbronze",
        "bismuthbronze",
        "blackbronze",
        "gold",
        "silver",
        "iron",
        "meteoriciron",
        "steel"
    ];
    private static readonly string[] RandomRestoredSpearMetals =
    [
        "copper",
        "tinbronze",
        "bismuthbronze",
        "blackbronze",
        "ornategold",
        "ornatesilver"
    ];
    private static readonly string[] RandomSalvageIngotMetals =
    [
        "copper",
        "tinbronze",
        "bismuthbronze",
        "blackbronze",
        "silver",
        "gold",
        "iron",
        "meteoriciron",
        "steel"
    ];
    private static readonly string[] RandomSalvageNailMetals =
    [
        "copper",
        "tinbronze",
        "bismuthbronze",
        "blackbronze",
        "iron",
        "meteoriciron",
        "steel"
    ];
    private static readonly string[] RandomSalvageClothItems = BuildItemCodes("cloth-", ["plain", "blue", "red", "yellow", "green", "purple", "brown", "gray", "black", "white"]);
    private static readonly string[] RandomRestoredAxeItems = BuildItemCodes("axe-felling-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredHammerItems = BuildItemCodes("hammer-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredHoeItems = BuildItemCodes("hoe-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredKnifeItems = BuildItemCodes("knife-generic-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredPickaxeItems = BuildItemCodes("pickaxe-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredSawItems = BuildItemCodes("saw-", RandomRestoredCommonMetals);

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);

        if (target != EnumItemRenderTarget.Gui)
        {
            return;
        }

        string? iconCode = itemstack?.Collectible?.Attributes?["iconItemCode"].AsString(null);
        if (string.IsNullOrWhiteSpace(iconCode))
        {
            return;
        }

        Item? iconItem = capi.World.GetItem(new AssetLocation("vstemporalreverser", iconCode));
        if (iconItem == null)
        {
            return;
        }

        iconSlot ??= new DummySlot();
        iconSlot.Itemstack = new ItemStack(iconItem, 1);

        renderinfo = capi.Render.GetItemStackRenderInfo(iconSlot, target, 0);
    }
    private static readonly string[] RandomRestoredScytheItems = BuildItemCodes("scythe-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredShovelItems = BuildItemCodes("shovel-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredSpearItems = BuildItemCodes("spear-generic-", RandomRestoredSpearMetals);
    private static readonly string[] RandomRestoredWeaponItems =
    [
        .. RandomRestoredAxeItems,
        .. RandomRestoredKnifeItems,
        .. RandomRestoredSpearItems,
        .. BuildItemCodes("blade-falx-", RandomRestoredCommonMetals)
    ];
    private static readonly string[] RandomRestoredPrecisionToolItems =
    [
        .. RandomRestoredHammerItems,
        .. BuildItemCodes("chisel-", ["copper", "tinbronze", "bismuthbronze", "blackbronze", "iron", "meteoriciron", "steel"]),
        .. BuildItemCodes("wrench-", RandomRestoredCommonMetals),
    ];
    private static readonly string[] RandomRestoredPileTools1Items =
    [
        .. RandomRestoredSawItems,
        .. RandomRestoredHammerItems,
        .. RandomRestoredAxeItems
    ];
    private static readonly string[] RandomRestoredPileTools2Items =
    [
        .. RandomRestoredShovelItems,
        "tongs",
        .. RandomRestoredKnifeItems
    ];
    private static readonly string[] RandomRestoredPileTools3Items =
    [
        .. RandomRestoredHoeItems,
        .. RandomRestoredScytheItems,
        .. BuildItemCodes("shears-", ["copper", "tinbronze", "bismuthbronze", "blackbronze", "gold", "silver", "iron", "meteoriciron", "steel"])
    ];
    private static readonly string[] RandomRestoredPileTools4Items =
    [
        .. RandomRestoredPickaxeItems,
        .. BuildItemCodes("prospectingpick-", RandomRestoredCommonMetals)
    ];
    private static readonly string[] RandomRestoredToolItems =
    [
        .. RandomRestoredAxeItems,
        .. RandomRestoredHammerItems,
        .. RandomRestoredHoeItems,
        .. RandomRestoredKnifeItems,
        .. RandomRestoredPickaxeItems,
        .. RandomRestoredSawItems,
        .. RandomRestoredScytheItems,
        .. RandomRestoredShovelItems,
        .. RandomRestoredSpearItems,
        .. BuildItemCodes("chisel-", ["copper", "tinbronze", "bismuthbronze", "blackbronze", "iron", "meteoriciron", "steel"]),
        .. BuildItemCodes("wrench-", RandomRestoredCommonMetals),
        .. BuildItemCodes("prospectingpick-", RandomRestoredCommonMetals),
        .. BuildItemCodes("shears-", ["copper", "tinbronze", "bismuthbronze", "blackbronze", "gold", "silver", "iron", "meteoriciron", "steel"]),
        "tongs"
    ];
    private static readonly string[] RandomRestoredWoodworkingToolItems =
    [
        .. RandomRestoredSawItems,
        .. RandomRestoredAxeItems,
        .. RandomRestoredHammerItems
    ];
    private static readonly string[] RandomNormalBookItems =
    [
        "book-normal-brickred",
        "book-normal-cherryred",
        "book-normal-darkbeige",
        "book-normal-darkgray",
        "book-normal-darkgreen",
        "book-normal-darkolive",
        "book-normal-gray",
        "book-normal-olive",
        "book-normal-orange",
        "book-normal-orangebrown",
        "book-normal-purple",
        "book-normal-purpleorange",
        "book-normal-teal"
    ];
    private static readonly string[] RandomAgedBookItems =
    [
        "book-aged-orangebrown",
        "book-aged-orange",
        "book-aged-darkgreen",
        "book-aged-darkgray",
        "book-aged-cherryred",
        "book-aged-brickred",
        "book-aged-darkolive",
        "book-aged-darkbeige",
        "book-aged-olive",
        "book-aged-purpleorange",
        "book-aged-gray"
    ];
    private static readonly string[] RandomRottenBookItems =
    [
        "book-rotten-gray",
        "book-rotten-brown",
        "book-rotten-rust",
        "book-rotten-purple",
        "book-rotten-green"
    ];
    private static readonly string[] RandomScrollItems =
    [
        "lore-scroll",
        "paper-parchment"
    ];
    private static readonly string[] RandomPotteryItems =
    [
        "bowl-blue-fired",
        "bowl-brown-fired",
        "bowl-cream-fired",
        "bowl-red-fired",
        "clayplanter-blue-fired",
        "clayplanter-brown-fired",
        "clayplanter-cream-fired",
        "clayplanter-red-fired",
        "claypot-blue-fired",
        "claypot-brown-fired",
        "claypot-cream-fired",
        "claypot-red-fired",
        "crock-blue-fired",
        "crock-brown-fired",
        "crock-cream-fired",
        "crock-red-fired",
        "crucible-blue-fired",
        "crucible-brown-fired",
        "crucible-cream-fired",
        "crucible-red-fired",
        "flowerpot-blue-fired",
        "flowerpot-brown-fired",
        "flowerpot-cream-fired",
        "flowerpot-red-fired",
        "storagevessel-blue-fired",
        "storagevessel-brown-fired",
        "storagevessel-cream-fired",
        "storagevessel-red-fired",
        "jug-blue-fired",
        "jug-brown-fired",
        "jug-cream-fired",
        "jug-red-fired",
        "wateringcan-blue-fired",
        "wateringcan-brown-fired",
        "wateringcan-cream-fired",
        "wateringcan-red-fired"
    ];
    private static readonly string[] RandomMedicalSupplyItems =
    [
        "bandage-clean",
        "bandage-alcoholed"
    ];
    private static readonly string[] RandomShelfLabJonasItems =
    [
        "jonasparts-pumphead",
        "jonasparts-tank01",
        "jonasparts-tank02",
        "jonasparts-valve01",
        "jonasparts-connector01",
        "jonasparts-cylinder01",
        "jonasparts-cylinder02",
        "jonasframes-gearbox01",
        "jonasframes-gearbox02",
        "jonasframes-oscillator01",
        "jonasframes-spring01",
        "jonasframes-joint01",
        "jonasframes-gears01",
        "jonasframes-gears02"
    ];
    private static readonly string[] RandomShelfFlaskItems =
    [
        "glass-green",
        "glass-blue",
        "glass-violet",
        "glass-red",
        "glass-yellow",
        "glass-brown",
        "glass-vintage",
        "glass-plain",
        "glass-quartz"
    ];
    private static readonly string[] RandomShelfLampItems =
    [
        "oillamp-genie-earthyorange-fired"
    ];
    private static readonly string[] RandomShelfLabItems = RandomShelfLabJonasItems;
    private static readonly string[] RandomShelfAlchemyItems =
    [
        "clutter-art/bottle",
        "measuringrope",
        "clothes-face-glasses",
        "clothes-face-glasses-clockmaker",
        "paper-parchment"
    ];
    private static readonly string[] RandomShelfMiscItems =
    [
        "paper-parchment",
        "candle",
        "beeswax",
        "clutter-art/bottle"
    ];
    private static readonly string[] RandomShelfClothingItems =
    [
        "clothes-upperbody-farmhand",
        "clothes-upperbody-popinjay",
        "clothes-upperbody-centurion",
        "clothes-upperbody-midsummer",
        "clothes-upperbody-ruralhunter",
        "clothes-upperbody-ruralfarmer",
        "clothes-upperbody-warrior",
        "clothes-upperbody-wanderer",
        "clothes-upperbody-beggar",
        "clothes-upperbody-clockmaker-shirt",
        "clothes-upperbodyover-clockmaker-tunic",
        "clothes-shoulder-clockmaker-apron",
        "clothes-lowerbody-farmhand",
        "clothes-lowerbody-popinjay",
        "clothes-lowerbody-centurion",
        "clothes-lowerbody-warrior",
        "clothes-lowerbody-wanderer",
        "clothes-lowerbody-beggar",
        "clothes-head-ruralhunter",
        "clothes-head-popinjay",
        "clothes-head-midsummer",
        "clothes-hand-commoner-gloves",
        "clothes-hand-tailor-gloves",
        "clothes-hand-clockmaker-wristguard",
        "clothes-waist-farmhand",
        "clothes-waist-popinjay",
        "clothes-waist-centurion",
        "clothes-waist-midsummer",
        "clothes-waist-ruralfarmer",
        "clothes-waist-ruralhunter",
        "clothes-waist-warrior",
        "clothes-waist-wanderer",
        "clothes-waist-beggar",
        "clothes-shoulder-wanderer",
        "clothes-shoulder-patchwork",
        "clothes-shoulder-ruralhunter"
    ];
    private static readonly string[] RandomShoeItems =
    [
        "clothes-foot-leather",
        "clothes-foot-worn-leather-boots",
        "clothes-foot-wool-lined-knee-high-boots",
        "clothes-foot-tigh-high-boots",
        "clothes-foot-temptress-velvet-shoes",
        "clothes-foot-nomad-boots",
        "clothes-foot-squire-boots",
        "clothes-foot-soldier-boots",
        "clothes-foot-shepherd-sandals",
        "clothes-foot-prisoner-binds",
        "clothes-foot-prince-boots",
        "clothes-foot-peasent-slippers",
        "clothes-foot-noble-shoes",
        "clothes-foot-minstrel-boots",
        "clothes-foot-metalcap-boots",
        "clothes-foot-messenger-shoes",
        "clothes-foot-merchant-shoes",
        "clothes-foot-lackey-shoes",
        "clothes-foot-knee-high-fur-boots",
        "clothes-foot-jailor-boots",
        "clothes-foot-high-leather-boots",
        "clothes-foot-fur-lined-reindeer-herder-shoes",
        "clothes-foot-great-steppe-boots",
        "clothes-foot-aristocrat-shoes",
        "clothes-foot-blackguard-shoes",
        "clothes-foot-clockmaker-shoes",
        "clothes-foot-hunter-boots",
        "clothes-foot-malefactor-boots",
        "clothes-foot-forlorn-shoes",
        "clothes-foot-commoner-boots",
        "clothes-foot-tailor-shoes",
        "clothes-foot-marketeer",
        "clothes-foot-rotwalker",
        "clothes-foot-rottenking",
        "clothes-foot-king",
        "clothes-foot-surgeon",
        "clothes-foot-miner",
        "clothes-foot-alchemist",
        "clothes-foot-forgotten",
        "clothes-foot-survivor",
        "clothes-foot-scribe",
        "clothes-nadiya-foot-alchemist",
        "clothes-nadiya-foot-barber",
        "clothes-nadiya-foot-beekeeper",
        "clothes-nadiya-foot-blacksmith",
        "clothes-nadiya-foot-fisher",
        "clothes-nadiya-foot-guard",
        "clothes-nadiya-foot-hunter",
        "clothes-nadiya-foot-innkeeper",
        "clothes-nadiya-foot-miner-clean",
        "clothes-nadiya-foot-miner",
        "clothes-nadiya-foot-musician",
        "clothes-nadiya-foot-peasantbeige",
        "clothes-nadiya-foot-peasantblue",
        "clothes-nadiya-foot-peasantbrown",
        "clothes-nadiya-foot-peasantwhite",
        "clothes-nadiya-foot-shepherd",
        "clothes-nadiya-foot-tailor",
        "clothes-nadiya-foot-winter1",
        "clothes-nadiya-foot-winter2",
        "clothes-foot-embroideredfur",
        "clothes-foot-strawsandals",
        "clothes-foot-emeraldreindeerherder",
        "clothes-foot-jester",
        "clothes-foot-arcticfisher",
        "clothes-foot-hobnailboots",
        "clothes-foot-arctichunter",
        "clothes-foot-acrobat",
        "clothes-foot-fortuneteller",
        "clothes-foot-farmhand",
        "clothes-foot-popinjay",
        "clothes-foot-centurion",
        "clothes-foot-midsummer",
        "clothes-foot-ruralhunter",
        "clothes-foot-ruralfarmer",
        "clothes-foot-wanderer",
        "clothes-foot-warrior",
        "clothes-foot-beggar"
    ];
    private static readonly string[] RandomRestoredToyItems =
    [
        "vstemporalreverser:restored-toy-toy4",
        "vstemporalreverser:restored-toy-toy5",
        "vstemporalreverser:restored-toy-toy6",
        "vstemporalreverser:restored-toy-toy7",
        "vstemporalreverser:restored-toy-toy8",
        "vstemporalreverser:restored-toy-toy9",
        "vstemporalreverser:restored-toy-toy10",
        "vstemporalreverser:restored-toy-toy11",
        "vstemporalreverser:restored-toy-toy12",
        "vstemporalreverser:restored-toy-toy13",
        "vstemporalreverser:restored-toy-toy14",
        "vstemporalreverser:restored-toy-toy15",
        "vstemporalreverser:restored-toy-toy16"
    ];
    private static readonly string[] RandomToyShelf1Items =
    [
        "vstemporalreverser:restored-toy-toy8",
        "vstemporalreverser:restored-toy-toy5",
        "vstemporalreverser:restored-toy-toy10"
    ];
    private static readonly string[] RandomToyShelf2Items =
    [
        "vstemporalreverser:restored-toy-toy12",
        "vstemporalreverser:restored-toy-toy7",
        "vstemporalreverser:restored-toy-toy13"
    ];
    private static readonly string[] RandomToyShelf3Items =
    [
        "vstemporalreverser:restored-toy-toy10",
        "vstemporalreverser:restored-toy-toy15"
    ];
    private static readonly string[] RandomToyBox1Items =
    [
        "vstemporalreverser:restored-toy-toy10",
        "vstemporalreverser:restored-toy-toy8",
        "vstemporalreverser:restored-toy-toy9",
        "vstemporalreverser:restored-toy-toy6"
    ];
    private static readonly string[] RandomToyBox2Items =
    [
        "vstemporalreverser:restored-toy-toy7",
        "vstemporalreverser:restored-toy-toy8",
        "vstemporalreverser:restored-toy-toy9",
        "vstemporalreverser:restored-toy-toy10",
        "vstemporalreverser:restored-toy-toy5"
    ];
    private static readonly string[] RandomOreNuggetItems =
    [
        "nugget-bismuthinite",
        "nugget-cassiterite",
        "nugget-chromite",
        "nugget-galena",
        "nugget-hematite",
        "nugget-ilmenite",
        "nugget-limonite",
        "nugget-magnetite",
        "nugget-malachite",
        "nugget-nativecopper",
        "nugget-nativegold",
        "nugget-nativesilver",
        "nugget-pentlandite",
        "nugget-rhodochrosite",
        "nugget-sphalerite",
        "nugget-uranium",
        "nugget-wolframite"
    ];
    private static readonly string[] RandomClothingItems =
    [
        "cloth-black",
        "cloth-blue",
        "cloth-brown",
        "cloth-gray",
        "cloth-green",
        "cloth-orange",
        "cloth-pink",
        "cloth-plain",
        "cloth-purple",
        "cloth-red",
        "cloth-white",
        "cloth-yellow",
        "linen-normal-down",
        "linen-offset-down",
        "linen-diamond-down",
        "linen-square-down"
    ];
    private static readonly string[] RandomClothingCrateItems =
    [
        .. RandomClothingItems,
        .. RandomShelfClothingItems
    ];
    private static readonly string[] RandomRotItems =
    [
        "leather-normal-plain",
        "seeds-amaranth",
        "seeds-bellpepper",
        "seeds-cabbage",
        "seeds-carrot",
        "seeds-cassava",
        "seeds-fennel",
        "seeds-flax",
        "seeds-onion",
        "seeds-parsnip",
        "seeds-peanut",
        "seeds-pumpkin",
        "seeds-rice",
        "seeds-rye",
        "seeds-soybean",
        "seeds-spelt",
        "seeds-sunflower",
        "seeds-turnip"
    ];
    private static readonly string[] RandomJunkCommonItems =
    [
        "painting-elk-north",
        "painting-howl-north",
        "painting-forestdawn-north",
        "painting-prey-north",
        "painting-underwater-north",
        "painting-sleepingwolf-north",
        "painting-fishandtherain-north",
        "painting-oldvillage-north",
        "painting-lastday-north",
        "windmillrotor-north",
        "woodenaxle-ud",
        "angledgears-s",
        "largegear3",
        "helvehammerbase-north",
        "helvehammerhead-tinbronze",
        "helvehammerhead-bismuthbronze",
        "helvehammerhead-blackbronze",
        "helvehammerhead-iron",
        "helvehammerhead-meteoriciron",
        "helvehammerhead-steel",
        "metalnailsandstrips-copper",
        "metalnailsandstrips-tinbronze",
        "metalnailsandstrips-bismuthbronze",
        "metalnailsandstrips-blackbronze",
        "metalnailsandstrips-iron",
        "metalnailsandstrips-gold",
        "metalnailsandstrips-silver",
        "arrow-copper",
        "arrow-tinbronze",
        "arrow-bismuthbronze",
        "arrow-blackbronze",
        "arrow-iron",
        "arrow-gold",
        "arrow-silver",
        "bomb-ore",
        "bomb-stone",
        "bomb-scrap",
        "cloth-plain",
        "cloth-blue",
        "cloth-brown",
        "cloth-gray",
        "cloth-black",
        "cloth-green",
        "cloth-orange",
        "cloth-pink",
        "cloth-purple",
        "cloth-red",
        "cloth-white",
        "cloth-yellow",
        "linen-normal-down",
        "linen-offset-down",
        "linen-diamond-down",
        "linen-square-down",
        "leather-normal-plain",
        "vstemporalreverser:restored-toy-toy4",
        "vstemporalreverser:restored-toy-toy5",
        "vstemporalreverser:restored-toy-toy6",
        "vstemporalreverser:restored-toy-toy7",
        "vstemporalreverser:restored-toy-toy8",
        "vstemporalreverser:restored-toy-toy9",
        "vstemporalreverser:restored-toy-toy10",
        "vstemporalreverser:restored-toy-toy11",
        "vstemporalreverser:restored-toy-toy12",
        "vstemporalreverser:restored-toy-toy13",
        "vstemporalreverser:restored-toy-toy14",
        "vstemporalreverser:restored-toy-toy15",
        "vstemporalreverser:restored-toy-toy16",
        "resin",
        "seeds-flax",
        "seeds-cabbage",
        "seeds-parsnip",
        "seeds-onion",
        "seeds-turnip",
        "seeds-rye",
        "seeds-spelt",
        "seeds-carrot",
        "seeds-pumpkin",
        "seeds-soybean",
        "seeds-rice"
    ];
    private static readonly string[] RandomJunkUncommonItems =
    [
        "ingot-copper",
        "ingot-tinbronze",
        "ingot-bismuthbronze",
        "ingot-blackbronze",
        "metalplate-copper",
        "metalplate-tinbronze",
        "metalplate-bismuthbronze",
        "metalplate-blackbronze",
        "windmillrotor-north",
        "woodenaxle-ud",
        "angledgears-s",
        "largegear3",
        "helvehammerbase-north",
        "helvehammerhead-tinbronze",
        "helvehammerhead-bismuthbronze",
        "helvehammerhead-blackbronze",
        "helvehammerhead-iron",
        "helvehammerhead-meteoriciron",
        "helvehammerhead-steel"
    ];
    private static readonly string[] RandomJunkArmorItems =
    [
        .. BuildItemCodes("armor-head-", ["lamellar-copper", "brigandine-copper", "chain-copper", "scale-copper", "plate-copper"]),
        .. BuildItemCodes("armor-body-", ["lamellar-copper", "brigandine-copper", "chain-copper", "scale-copper", "plate-copper"]),
        .. BuildItemCodes("armor-legs-", ["lamellar-copper", "brigandine-copper", "chain-copper", "scale-copper", "plate-copper"]),
        "armor-head-lamellar-tinbronze",
        "armor-body-lamellar-tinbronze",
        "armor-legs-lamellar-tinbronze",
        "armor-head-lamellar-bismuthbronze",
        "armor-body-lamellar-bismuthbronze",
        "armor-legs-lamellar-bismuthbronze",
        "armor-head-lamellar-blackbronze",
        "armor-body-lamellar-blackbronze",
        "armor-legs-lamellar-blackbronze",
        "armor-head-brigandine-tinbronze",
        "armor-body-brigandine-tinbronze",
        "armor-legs-brigandine-tinbronze",
        "armor-head-brigandine-bismuthbronze",
        "armor-body-brigandine-bismuthbronze",
        "armor-legs-brigandine-bismuthbronze",
        "armor-head-brigandine-blackbronze",
        "armor-body-brigandine-blackbronze",
        "armor-legs-brigandine-blackbronze",
        "armor-head-chain-tinbronze",
        "armor-body-chain-tinbronze",
        "armor-legs-chain-tinbronze",
        "armor-head-chain-bismuthbronze",
        "armor-body-chain-bismuthbronze",
        "armor-legs-chain-bismuthbronze",
        "armor-head-chain-blackbronze",
        "armor-body-chain-blackbronze",
        "armor-legs-chain-blackbronze",
        "armor-head-scale-tinbronze",
        "armor-body-scale-tinbronze",
        "armor-legs-scale-tinbronze",
        "armor-head-scale-bismuthbronze",
        "armor-body-scale-bismuthbronze",
        "armor-legs-scale-bismuthbronze",
        "armor-head-scale-blackbronze",
        "armor-body-scale-blackbronze",
        "armor-legs-scale-blackbronze",
        "armor-head-plate-tinbronze",
        "armor-body-plate-tinbronze",
        "armor-legs-plate-tinbronze",
        "armor-head-plate-bismuthbronze",
        "armor-body-plate-bismuthbronze",
        "armor-legs-plate-bismuthbronze",
        "armor-head-plate-blackbronze",
        "armor-body-plate-blackbronze",
        "armor-legs-plate-blackbronze",
        "armor-head-sewn-leather",
        "armor-body-sewn-leather",
        "armor-legs-sewn-leather",
        "armor-body-jerkin-leather",
        "armor-legs-jerkin-leather"
    ];
    private static readonly string[] RandomJunkRareItems =
    [
        "ingot-iron",
        "ingot-meteoriciron",
        "metalplate-iron",
        "metalplate-meteoriciron",
        "armor-head-lamellar-iron",
        "armor-body-lamellar-iron",
        "armor-legs-lamellar-iron",
        "armor-head-lamellar-meteoriciron",
        "armor-body-lamellar-meteoriciron",
        "armor-legs-lamellar-meteoriciron",
        "armor-head-brigandine-iron",
        "armor-body-brigandine-iron",
        "armor-legs-brigandine-iron",
        "armor-head-brigandine-meteoriciron",
        "armor-body-brigandine-meteoriciron",
        "armor-legs-brigandine-meteoriciron",
        "armor-head-chain-iron",
        "armor-body-chain-iron",
        "armor-legs-chain-iron",
        "armor-head-chain-meteoriciron",
        "armor-body-chain-meteoriciron",
        "armor-legs-chain-meteoriciron",
        "armor-head-scale-iron",
        "armor-body-scale-iron",
        "armor-legs-scale-iron",
        "armor-head-scale-meteoriciron",
        "armor-body-scale-meteoriciron",
        "armor-legs-scale-meteoriciron",
        "armor-head-plate-iron",
        "armor-body-plate-iron",
        "armor-legs-plate-iron",
        "armor-head-plate-meteoriciron",
        "armor-body-plate-meteoriciron",
        "armor-legs-plate-meteoriciron"
    ];
    private static readonly string[] RandomJunkUltraRareItems =
    [
        "backpack-sturdy",
        "armor-head-brigandine-gold",
        "armor-body-brigandine-gold",
        "armor-legs-brigandine-gold",
        "armor-head-brigandine-silver",
        "armor-body-brigandine-silver",
        "armor-legs-brigandine-silver",
        "armor-head-chain-gold",
        "armor-body-chain-gold",
        "armor-legs-chain-gold",
        "armor-head-chain-silver",
        "armor-body-chain-silver",
        "armor-legs-chain-silver",
        "armor-head-scale-gold",
        "armor-body-scale-gold",
        "armor-legs-scale-gold",
        "armor-head-scale-silver",
        "armor-body-scale-silver",
        "armor-legs-scale-silver",
        "armor-head-plate-gold",
        "armor-body-plate-gold",
        "armor-legs-plate-gold",
        "armor-head-plate-silver",
        "armor-body-plate-silver",
        "armor-legs-plate-silver",
        "armor-head-brigandine-steel",
        "armor-body-brigandine-steel",
        "armor-legs-brigandine-steel",
        "armor-head-chain-steel",
        "armor-body-chain-steel",
        "armor-legs-chain-steel",
        "armor-head-scale-steel",
        "armor-body-scale-steel",
        "armor-legs-scale-steel",
        "armor-head-plate-steel",
        "armor-body-plate-steel",
        "armor-legs-plate-steel"
    ];
    private static readonly string[] RandomMetalJunkCommonItems =
    [
        "metalnailsandstrips-copper",
        "metalnailsandstrips-tinbronze",
        "metalnailsandstrips-bismuthbronze",
        "metalnailsandstrips-blackbronze",
        "metalnailsandstrips-iron",
        "metalnailsandstrips-gold",
        "metalnailsandstrips-silver",
        "metalbit-copper",
        "metalbit-tinbronze",
        "metalbit-bismuthbronze",
        "metalbit-blackbronze",
        "metalbit-brass",
        "metalbit-electrum",
        "metalbit-gold",
        "metalbit-iron",
        "metalbit-lead",
        "metalbit-meteoriciron",
        "metalbit-nickel",
        "metalbit-silver",
        "metalbit-tin",
        "metalbit-zinc",
        "metalbit-molybdochalkos",
        "metalbit-bismuth",
        "metalbit-blistersteel",
        "metalbit-leadsolder",
        "metalbit-silversolder"
    ];
    private static readonly string[] RandomMetalJunkUncommonItems =
    [
        "ingot-copper",
        "ingot-tinbronze",
        "ingot-bismuthbronze",
        "ingot-blackbronze",
        "metalplate-copper",
        "metalplate-tinbronze",
        "metalplate-bismuthbronze",
        "metalplate-blackbronze"
    ];
    private static readonly string[] RandomMetalJunkRareItems =
    [
        "ingot-iron",
        "ingot-meteoriciron",
        "ingot-silver",
        "ingot-gold",
        "metalplate-iron",
        "metalplate-meteoriciron",
        "metalplate-silver",
        "metalplate-gold",
        "armor-head-lamellar-iron",
        "armor-body-lamellar-iron",
        "armor-legs-lamellar-iron",
        "armor-head-lamellar-meteoriciron",
        "armor-body-lamellar-meteoriciron",
        "armor-legs-lamellar-meteoriciron"
    ];
    private static readonly string[] RandomMetalJunkUltraRareItems =
    [
        "armor-head-brigandine-silver",
        "armor-body-brigandine-silver",
        "armor-legs-brigandine-silver",
        "armor-head-brigandine-gold",
        "armor-body-brigandine-gold",
        "armor-legs-brigandine-gold",
        "armor-head-chain-silver",
        "armor-body-chain-silver",
        "armor-legs-chain-silver",
        "armor-head-chain-gold",
        "armor-body-chain-gold",
        "armor-legs-chain-gold",
        "armor-head-scale-silver",
        "armor-body-scale-silver",
        "armor-legs-scale-silver",
        "armor-head-scale-gold",
        "armor-body-scale-gold",
        "armor-legs-scale-gold",
        "armor-head-plate-silver",
        "armor-body-plate-silver",
        "armor-legs-plate-silver",
        "armor-head-plate-gold",
        "armor-body-plate-gold",
        "armor-legs-plate-gold",
        "armor-head-brigandine-steel",
        "armor-body-brigandine-steel",
        "armor-legs-brigandine-steel",
        "armor-head-chain-steel",
        "armor-body-chain-steel",
        "armor-legs-chain-steel",
        "armor-head-scale-steel",
        "armor-body-scale-steel",
        "armor-legs-scale-steel",
        "armor-head-plate-steel",
        "armor-body-plate-steel",
        "armor-legs-plate-steel"
    ];
    private static readonly string[] RandomRareClothingItems =
    [
        "backpack-sturdy"
    ];
    private static readonly string[] RandomToyCeramicTextures =
    [
        "brown1",
        "blue1",
        "red1",
        "fire1"
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
        "birch",
        "oak",
        "maple",
        "pine",
        "acacia",
        "kapok",
        "redwood",
        "baldcypress",
        "larch",
        "ebony",
        "walnut",
        "purpleheart",
        "aged",
        "veryaged"
    ];
    private static readonly string[] RandomRestoredTableWoodTypes =
    [
        "birch",
        "oak",
        "maple",
        "pine",
        "acacia",
        "kapok",
        "redwood",
        "baldcypress",
        "larch",
        "ebony",
        "walnut",
        "purpleheart",
        "aged",
        "veryaged"
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
        ["bed/bed-metal"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}"),
        ["bed/bed-metal-ruined1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}"),
        ["bed/bed-metal-ruined2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}"),
        ["bed/bed-metal-ruined3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}"),
        ["bed/metal2"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-mattress"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-pillow"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-ruined1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-ruined2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-ruined3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal1-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["bed/metal2-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north"),
        ["table-aged"] = RestoredTableRule(AgedDurabilityCost, "agedwhite"),
        ["table-long"] = RestoredTableRule(AgedDurabilityCost, "scribe"),
        ["table-long-with-accessories"] = RestoredTableRule(AgedDurabilityCost, "scribeaccessories"),
        ["table-long-with-cloth-blue"] = RestoredTableRule(AgedDurabilityCost, "scribeblue"),
        ["table-long-with-cloth-green"] = RestoredTableRule(AgedDurabilityCost, "scribegreen"),
        ["table-long-with-cloth-purple"] = RestoredTableRule(AgedDurabilityCost, "scribepurple"),
        ["table-long-with-cloth-red"] = RestoredTableRule(AgedDurabilityCost, "scribered"),
        ["table/metal1"] = RestoredMetalTableRule(AgedDurabilityCost),
        ["table/metal1-cloth"] = RestoredMetalTableRule(AgedDurabilityCost, "green"),
        ["table/metal1-ruined1"] = RestoredMetalTableRule(RuinedDurabilityCost),
        ["table/metal1-ruined2"] = RestoredMetalTableRule(RuinedDurabilityCost),
        ["table/metal1-ruined3"] = RestoredMetalTableRule(RuinedDurabilityCost),
        ["table-ruined1"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined2"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined3"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined4"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined5"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["table-ruined6"] = RandomRestoredTableRule(RuinedDurabilityCost, RandomRestoredAgedTableStyles),
        ["brazier1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-{material}"),
        ["brazier2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-{material}"),
        ["brazier3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-{material}"),
        ["brazier4"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-{material}"),
        ["brazier-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-brazier-{material}"),
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
        ["bellows"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-north"),
        ["bellows-north"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-north"),
        ["bellows-east"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-east"),
        ["bellows-south"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-south"),
        ["bellows-west"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-west"),
        ["torchholder-aged-empty-north"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-north"),
        ["torchholder-aged-empty-east"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-east"),
        ["torchholder-aged-empty-south"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-south"),
        ["torchholder-aged-empty-west"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-west"),
        ["torchholder-aged-filled-north"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-north"),
        ["torchholder-aged-filled-east"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-east"),
        ["torchholder-aged-filled-south"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-south"),
        ["torchholder-aged-filled-west"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-west"),
        ["torchholder-ruined-empty-north"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-north"),
        ["torchholder-ruined-empty-east"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-east"),
        ["torchholder-ruined-empty-south"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-south"),
        ["torchholder-ruined-empty-west"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-west"),
        ["torchholder-ruined-filled-north"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-north"),
        ["torchholder-ruined-filled-east"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-east"),
        ["torchholder-ruined-filled-south"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-south"),
        ["torchholder-ruined-filled-west"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:torchholder-{material}-empty-west")
    };

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        if (api is not ICoreClientAPI clientApi)
        {
            return;
        }

        capi = clientApi;

        toolModes =
        [
            new SkillItem { Code = new AssetLocation("restore"), Name = "Restore" },
            new SkillItem { Code = new AssetLocation("salvage"), Name = "Salvage" }
        ];

        AttachToolModeIcons(toolModes);
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);

        if (toolModes == null)
        {
            return;
        }

        foreach (SkillItem toolMode in toolModes)
        {
            toolMode.Dispose();
        }

        foreach (LoadedTexture texture in toolModeTextures.Values)
        {
            texture.Dispose();
        }

        toolModeTextures.Clear();
        toolModes = null;
        capi = null;
    }

    public override SkillItem[]? GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        return IsSalvageEnabled(slot?.Itemstack) ? toolModes : null;
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
    {
        ItemStack? stack = slot?.Itemstack;
        if (!IsSalvageEnabled(stack))
        {
            return RestoreToolMode;
        }

        int selectedMode = stack?.Attributes.GetInt(ToolModeAttribute, RestoreToolMode) ?? RestoreToolMode;
        return GameMath.Clamp(selectedMode, RestoreToolMode, SalvageToolMode);
    }

    public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
    {
        if (slot.Itemstack == null || !IsSalvageEnabled(slot.Itemstack))
        {
            return;
        }

        slot.Itemstack.Attributes.SetInt(ToolModeAttribute, GameMath.Clamp(toolMode, RestoreToolMode, SalvageToolMode));
        slot.MarkDirty();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine();

        if (IsDepleted(inSlot?.Itemstack))
        {
            dsc.AppendLine("Its temporal charge is spent.");
            dsc.AppendLine("Rebuild its field with fresh temporal gears at a crafting grid.");
            return;
        }

        dsc.AppendLine("Coaxes aged and ruined clutter back into usable form.");
        dsc.AppendLine("Aged patterns cost 1 durability. Ruined patterns cost 2.");
        dsc.AppendLine("Known patterns include furniture, storage, bookshelves, scroll racks, toys, trash piles, tools, weapons, and more.");
        dsc.AppendLine($"Mode: {GetToolModeName(GetSelectedToolMode(inSlot?.Itemstack))}");

        if (IsRustWardEnabled(inSlot?.Itemstack))
        {
            dsc.AppendLine("When paired with an offhand light source, its field pushes back nearby rust creatures.");
        }

        if (UsesCopperOnlyRestoration(inSlot?.Itemstack))
        {
            dsc.AppendLine("Its unstable field can recover bonus finds, but only stabilizes copper-grade metal results.");
        }

        if (IsSalvageEnabled(inSlot?.Itemstack))
        {
            dsc.AppendLine("Press F to shift between Restore and Salvage. Salvage costs double durability.");
        }
    }

    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        base.OnHeldIdle(slot, byEntity);

        if (slot?.Itemstack == null || byEntity.World.Side != EnumAppSide.Server)
        {
            return;
        }

        if (byEntity is not EntityPlayer player || player.RightHandItemSlot?.Itemstack != slot.Itemstack)
        {
            return;
        }

        if (!IsRustWardEnabled(slot.Itemstack))
        {
            return;
        }

        if (!HasQualifyingOffhandLight(player))
        {
            return;
        }

        long nowMs = byEntity.World.ElapsedMilliseconds;
        if (LastRustWardPulseByEntityId.TryGetValue(byEntity.EntityId, out long lastPulseMs)
            && nowMs - lastPulseMs < RustWardPulseIntervalMs)
        {
            return;
        }

        LastRustWardPulseByEntityId[byEntity.EntityId] = nowMs;
        PulseRustWard(player);
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

        if (IsDepleted(slot.Itemstack))
        {
            SendNotification(byEntity, "The reverser's field is spent. It needs fresh temporal gears.");
            handling = EnumHandHandling.PreventDefault;
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
        string? clutterType = null;
        List<ItemStack>? fallbackSalvageStacks = null;
        if (block.Code.Path == "clutter"
            || block.Code.Path.StartsWith("clutteredbookshelf", StringComparison.OrdinalIgnoreCase))
        {
            clutterType = GetClutterType(world, pos);
            clutterType ??= block.Code.Path;
            RestorationRule? censerRule = TryGetCenserRule(clutterType);
            if (censerRule != null)
            {
                matchedRule = censerRule;
            }
            else
            {
                RestorationRule? bellowsRule = TryGetBellowsRule(clutterType);
                if (bellowsRule != null)
                {
                    matchedRule = bellowsRule;
                }
                else
                {
                    RestorationRule? candleRule = TryGetCandleRule(clutterType);
                    if (candleRule != null)
                    {
                        matchedRule = candleRule;
                    }
                    else
                    {
                    RestorationRule? chairOrLibraryRule = TryGetChairOrLibraryRule(clutterType);
                    if (chairOrLibraryRule != null)
                    {
                        matchedRule = chairOrLibraryRule;
                    }
                    else
                    {
                        RestorationRule? toolOrWeaponRule = TryGetToolOrWeaponRule(clutterType);
                        if (toolOrWeaponRule != null)
                        {
                            matchedRule = toolOrWeaponRule;
                        }
                            else
                            {
                                RestorationRule? toyRule = TryGetToyRule(clutterType);
                                if (toyRule != null)
                                {
                                    matchedRule = toyRule;
                                }
                                else
                                {
                                    RestorationRule? toyShelfRule = TryGetToyShelfRule(clutterType);
                                    if (toyShelfRule != null)
                                    {
                                        matchedRule = toyShelfRule;
                                    }
                                    else
                                    {
                                        RestorationRule? shelfRule = TryGetShelfRule(clutterType);
                                        if (shelfRule != null)
                                        {
                                            matchedRule = shelfRule;
                                        }
                                        else
                                        {
                                            RestorationRule? crateJunkRule = TryGetCrateJunkRule(clutterType);
                                            if (crateJunkRule != null)
                                            {
                                                matchedRule = crateJunkRule;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    }
            }
            if (matchedRule == null)
            {
                matchedRule = TryGetAnvilRule(clutterType);
            }
            if (matchedRule == null && clutterType != null && BedRules.TryGetValue(clutterType, out RestorationRule clutterRule))
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
            bool isSalvageModeWithoutRule = GetSelectedToolMode(slot.Itemstack) == SalvageToolMode && IsSalvageEnabled(slot.Itemstack);
            if (clutterType != null && isSalvageModeWithoutRule)
            {
                fallbackSalvageStacks = CreateFallbackClutterSalvageStacks(world, pos, clutterType, slot.Itemstack).ToList();
                if (fallbackSalvageStacks.Count > 0)
                {
                    matchedRule = new RestorationRule(GetFallbackSalvageDurabilityCost(clutterType), RestorationTargetKind.ClutterType, clutterType);
                }
            }

            if (matchedRule != null)
            {
                goto MatchedRuleResolved;
            }

            if (clutterType != null)
            {
                WriteDebugEvent("no-match", clutterType, null, null, null);
                if (CreateFallbackClutterSalvageStacks(world, pos, clutterType, slot.Itemstack).Any())
                {
                    SendNotification(byEntity, "This object no longer remembers its former state. It may be salvagable.");
                }
                else
                {
                    SendNotification(byEntity, "The object's former state was lost in time.");
                }
            }
            else
            {
                SendNotification(byEntity, "The reverser hums, but finds no restorable pattern.");
            }

            handling = EnumHandHandling.PreventDefault;
            return;
        }

MatchedRuleResolved:
        RestorationRule rule = matchedRule.Value;
        string sourceCode = clutterType ?? block.Code.Path;
        bool isSalvageMode = GetSelectedToolMode(slot.Itemstack) == SalvageToolMode && IsSalvageEnabled(slot.Itemstack);
        long restoreCooldownMs = GetRestoreCooldownMs(slot.Itemstack);
        long nowMs = world.ElapsedMilliseconds;

        if (LastRestoreUseByEntityId.TryGetValue(byEntity.EntityId, out long lastRestoreUseMs)
            && nowMs - lastRestoreUseMs < restoreCooldownMs)
        {
            SendNotification(byEntity, "The reverser needs a moment to recharge.");
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        List<ItemStack> primaryAndExtraStacks;
        List<string> entityCodes;
        List<ItemStack> gearStacks;
        int durabilityCost;
        string successMessage;

        if (isSalvageMode)
        {
            primaryAndExtraStacks = fallbackSalvageStacks ?? CreateSalvageStacks(world, sourceCode, rule, slot.Itemstack).ToList();
            primaryAndExtraStacks.AddRange(CreateSupplementalRestoredStacks(world, rule, slot.Itemstack));
            entityCodes = CreateSupplementalRestoredEntities(rule).ToList();
            gearStacks = CreateSupplementalRestoredTemporalGearStacks(world, rule, slot.Itemstack).ToList();
            durabilityCost = Math.Max(1, rule.DurabilityCost * 2);
            successMessage = "Salvaged materials spill free from the unraveling pattern.";
        }
        else
        {
            ItemStack? restoredStack = CreateRestoredStack(world, rule, slot.Itemstack);
            if (restoredStack == null)
            {
                WriteDebugEvent("restore-null", sourceCode, rule, null, null);
                SendNotification(byEntity, "The reverser finds the pattern, but cannot draw it fully back into the present.");
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            primaryAndExtraStacks = [restoredStack];
            primaryAndExtraStacks.AddRange(CreateSupplementalRestoredStacks(world, rule, slot.Itemstack));
            entityCodes = CreateSupplementalRestoredEntities(rule).ToList();
            gearStacks = CreateSupplementalRestoredTemporalGearStacks(world, rule, slot.Itemstack).ToList();
            durabilityCost = rule.DurabilityCost;
            successMessage = "The restored item drops free in a usable shape.";
        }

        if (primaryAndExtraStacks.Count == 0)
        {
            SendNotification(byEntity, "The reverser cannot salvage anything useful from this pattern.");
            handling = EnumHandHandling.PreventDefault;
            return;
        }

        Vec3d dropPos = pos.ToVec3d().Add(0.5, 0.25, 0.5);
        List<string> spawnedEntries = primaryAndExtraStacks.Select(DescribeStackForRecord).ToList();

        world.BlockAccessor.SetBlock(0, pos);
        LastRestoreUseByEntityId[byEntity.EntityId] = nowMs;
        ConsumeDurabilityOrDeplete(world, byEntity, slot, durabilityCost);

        world.PlaySoundAt(GetRestoreSoundLocation(slot.Itemstack), pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5);
        SpawnTemporalSmoke(world, dropPos);
        int restoreSpawnDelayMs = GetRestoreSpawnDelayMs(slot.Itemstack);

        world.RegisterCallback(_ =>
        {
            foreach (ItemStack stackToSpawn in primaryAndExtraStacks)
            {
                world.SpawnItemEntity(stackToSpawn, dropPos);
            }

            foreach (string entityCode in entityCodes)
            {
                if (TrySpawnRestoredEntity(world, dropPos, entityCode))
                {
                    spawnedEntries.Add(DescribeEntityForRecord(entityCode));
                }
            }

            foreach (ItemStack gearStack in gearStacks)
            {
                spawnedEntries.Add(DescribeStackForRecord(gearStack));
                world.SpawnItemEntity(gearStack, dropPos);
            }

            WriteRestoreDebugRecord(sourceCode, rule, spawnedEntries);
            SendNotification(byEntity, successMessage);
        }, restoreSpawnDelayMs);

        handling = EnumHandHandling.PreventDefault;
    }

    private void PulseRustWard(EntityPlayer player)
    {
        Vec3d center = player.Pos.XYZ;
        IWorldAccessor world = player.World;
        VSTemporalReverserConfig config = VSTemporalReverserModSystem.Config;
        bool dealDamage = config.EnableRustWardDamage;
        float wardDamageAmount = Math.Max(0f, config.RustWardDamage);
        float wardRadius = Math.Clamp(config.RustWardRadius, 2f, 6f);
        float wardPushback = Math.Clamp(config.RustWardPushback, 0.5f, 3f);
        DamageSource wardDamage = new()
        {
            Source = EnumDamageSource.Internal,
            SourceEntity = player,
            KnockbackStrength = wardPushback
        };

        world.GetEntitiesAround(center, wardRadius, wardRadius, entity =>
        {
            if (entity == player || !entity.Alive || !IsRustCreature(entity))
            {
                return true;
            }

            Vec3d motion = entity.Pos.Motion;
            double dx = entity.Pos.X - player.Pos.X;
            double dz = entity.Pos.Z - player.Pos.Z;
            double length = Math.Sqrt(dx * dx + dz * dz);

            if (length > 0.001)
            {
                double pushX = dx / length * wardPushback;
                double pushZ = dz / length * wardPushback;
                motion.X += pushX;
                motion.Z += pushZ;
            }

            if (dealDamage && wardDamageAmount > 0)
            {
                entity.ReceiveDamage(wardDamage, wardDamageAmount);
            }

            return true;
        });
    }

    private bool HasQualifyingOffhandLight(EntityPlayer player)
    {
        ItemStack? offhandStack = player.LeftHandItemSlot?.Itemstack;
        if (offhandStack?.Collectible == null)
        {
            return false;
        }

        if (offhandStack.Collectible is ItemTemporalReverser)
        {
            return false;
        }

        byte[]? lightHsv = offhandStack.Collectible.GetLightHsv(player.World.BlockAccessor, player.Pos.AsBlockPos, offhandStack);
        return lightHsv != null && lightHsv.Length >= 3 && lightHsv[2] > 0;
    }

    private bool IsRustWardEnabled(ItemStack? stack)
    {
        return stack?.Collectible?.Attributes?["rustWardEnabled"].AsBool(DefaultRustWardEnabled) ?? DefaultRustWardEnabled;
    }

    private static bool IsDepleted(ItemStack? stack)
    {
        return stack?.Collectible?.Attributes?["depleted"].AsBool(DefaultDepleted) ?? DefaultDepleted;
    }

    private static bool IsSalvageEnabled(ItemStack? stack)
    {
        return stack?.Collectible?.Attributes?["salvageEnabled"].AsBool(DefaultSalvageEnabled) ?? DefaultSalvageEnabled;
    }

    private void ConsumeDurabilityOrDeplete(IWorldAccessor world, EntityAgent byEntity, ItemSlot slot, int durabilityCost)
    {
        ItemStack? stack = slot.Itemstack;
        if (stack == null)
        {
            return;
        }

        int remainingDurability = GetRemainingDurability(stack);
        string? depletedItemCode = stack.Collectible?.Attributes?["depletedItemCode"].AsString(null);
        if (remainingDurability > durabilityCost || string.IsNullOrWhiteSpace(depletedItemCode))
        {
            DamageItem(world, byEntity, slot, durabilityCost);
            return;
        }

        Item? depletedItem = world.GetItem(ToAssetLocation(depletedItemCode));
        if (depletedItem == null)
        {
            DamageItem(world, byEntity, slot, durabilityCost);
            return;
        }

        slot.Itemstack = new ItemStack(depletedItem, 1);
        slot.MarkDirty();
    }

    private static int GetSelectedToolMode(ItemStack? stack)
    {
        if (!IsSalvageEnabled(stack))
        {
            return RestoreToolMode;
        }

        int selectedMode = stack?.Attributes.GetInt(ToolModeAttribute, RestoreToolMode) ?? RestoreToolMode;
        return GameMath.Clamp(selectedMode, RestoreToolMode, SalvageToolMode);
    }

    private static string GetToolModeName(int toolMode)
    {
        return toolMode == SalvageToolMode ? "Salvage" : "Restore";
    }

    private void AttachToolModeIcons(SkillItem[] modes)
    {
        if (capi == null)
        {
            return;
        }

        for (int i = 0; i < modes.Length; i++)
        {
            string iconKey = i == SalvageToolMode ? "recycle" : "restore";
            LoadedTexture? texture = GetToolModeTexture(iconKey);
            if (texture != null)
            {
                modes[i].WithIcon(capi, texture);
            }
        }
    }

    private LoadedTexture? GetToolModeTexture(string iconKey)
    {
        if (capi == null)
        {
            return null;
        }

        if (toolModeTextures.TryGetValue(iconKey, out LoadedTexture? texture))
        {
            return texture;
        }

        AssetLocation location = new($"vstemporalreverser:textures/gui/toolmodes/{iconKey}.png");
        var loadedTexture = new LoadedTexture(capi);
        capi.Render.GetOrLoadTexture(location, ref loadedTexture);
        if (loadedTexture.TextureId <= 0)
        {
            loadedTexture.Dispose();
            return null;
        }

        toolModeTextures[iconKey] = loadedTexture;
        return loadedTexture;
    }

    private static bool IsRustCreature(Entity entity)
    {
        string path = entity.Code?.Path ?? string.Empty;
        return path.StartsWith("drifter", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("shiver", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("bowtorn", StringComparison.OrdinalIgnoreCase);
    }

    private static ItemStack? CreateRestoredStack(IWorldAccessor world, RestorationRule rule, ItemStack? reverserStack)
    {
        string[] enabledRestoredWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredWoodTypes);
        string[] enabledRestoredTableWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredTableWoodTypes);
        string[] enabledLibraryMaterials = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredLibraryMaterials);
        string[] enabledCrateWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomCrateWoodTypes);
        bool copperOnly = UsesCopperOnlyRestoration(reverserStack);

        if (rule.TargetKind == RestorationTargetKind.RandomRestoredCanopyBed)
        {
            string[] styles = rule.Targets ?? Array.Empty<string>();
            if (styles.Length == 0)
            {
                return null;
            }

            string style = styles[Random.Shared.Next(styles.Length)];
            string wood = enabledRestoredWoodTypes[Random.Shared.Next(enabledRestoredWoodTypes.Length)];
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
            string wood = enabledRestoredWoodTypes[Random.Shared.Next(enabledRestoredWoodTypes.Length)];
            Block? randomBlock = world.GetBlock(ToAssetLocation($"vstemporalreverser:restored-short-bed-{style}-{wood}-head-north"));
            return randomBlock == null ? null : new ItemStack(randomBlock, 1);
        }

        if (rule.TargetKind == RestorationTargetKind.RandomRestoredCenser)
        {
            string[] finishes = SelectMetalRestrictedPool(rule.Targets ?? Array.Empty<string>(), copperOnly);
            if (string.IsNullOrWhiteSpace(rule.Target) || finishes.Length == 0)
            {
                return null;
            }

            string finish = finishes[Random.Shared.Next(finishes.Length)];
            Block? censerBlock = world.GetBlock(ToAssetLocation($"vstemporalreverser:restored-censer-{rule.Target}-{finish}"));
            return censerBlock == null ? null : new ItemStack(censerBlock, 1);
        }

        if (rule.TargetKind == RestorationTargetKind.RestoredDecoration)
        {
            if (string.IsNullOrWhiteSpace(rule.Target))
            {
                return null;
            }

            string variant = SanitizeDecorationType(rule.Target);
            Item? decorationItem = world.GetItem(ToAssetLocation($"vstemporalreverser:restored-decoration-{variant}"));
            return decorationItem == null ? null : new ItemStack(decorationItem, 1);
        }

        if (rule.TargetKind == RestorationTargetKind.RandomVanillaLantern)
        {
            Block? lanternBlock = world.GetBlock(ToAssetLocation("game:lantern-large-up"));
            if (lanternBlock == null)
            {
                return null;
            }

            ItemStack lanternStack = new(lanternBlock, 1);
            string[] lanternMaterials = SelectMetalRestrictedPool(RandomLanternMaterials, copperOnly);
            lanternStack.Attributes.SetString("material", lanternMaterials[Random.Shared.Next(lanternMaterials.Length)]);
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

        if (rule.TargetKind == RestorationTargetKind.RandomizedClutterType)
        {
            string[] clutterTypes = rule.Targets ?? Array.Empty<string>();
            string[] textureOptions = rule.TextureOptions ?? Array.Empty<string>();
            if (clutterTypes.Length == 0)
            {
                return null;
            }

            Block? clutter = world.GetBlock(new AssetLocation("game", "clutter"));
            if (clutter == null)
            {
                return null;
            }

            ItemStack clutterStack = new(clutter, 1);
            clutterStack.Attributes.SetString("type", clutterTypes[Random.Shared.Next(clutterTypes.Length)]);
            clutterStack.Attributes.SetBool("collected", true);
            if (textureOptions.Length > 0)
            {
                string texturePath = textureOptions[Random.Shared.Next(textureOptions.Length)];
                TreeAttribute textures = new();
                textures.SetString(rule.TextureKey ?? "metal", texturePath);

                TreeAttribute typeAttributes = new();
                typeAttributes["textures"] = textures.Clone();

                clutterStack.Attributes["collectedTextures"] = textures;
                clutterStack.Attributes["textures"] = textures.Clone();
                clutterStack.Attributes["typeAttributes"] = typeAttributes;
            }
            clutterStack.ResolveBlockOrItem(world);
            return clutterStack;
        }

        if (rule.TargetKind == RestorationTargetKind.RandomVanillaItem)
        {
            string[] itemCodes = SelectItemCodePool(rule.Targets ?? Array.Empty<string>(), copperOnly);
            if (itemCodes.Length == 0)
            {
                return null;
            }

            string itemCode = itemCodes[Random.Shared.Next(itemCodes.Length)];
            int minCount = Math.Max(1, rule.PrimaryMinCount);
            int maxCount = Math.Max(minCount, rule.PrimaryMaxCount);
            int stackSize = maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;

            return CreateStackForCode(world, itemCode, stackSize);
        }

        if (rule.TargetKind == RestorationTargetKind.TieredJunkItem)
        {
            string itemCode = PickTieredJunkItemCode(copperOnly);
            int minCount = Math.Max(1, rule.PrimaryMinCount);
            int maxCount = Math.Max(minCount, rule.PrimaryMaxCount);
            int stackSize = maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;

            return CreateStackForCode(world, itemCode, stackSize);
        }

        if (rule.TargetKind == RestorationTargetKind.Block)
        {
            string wood = enabledRestoredWoodTypes[Random.Shared.Next(enabledRestoredWoodTypes.Length)];
            string tableWood = enabledRestoredTableWoodTypes[Random.Shared.Next(enabledRestoredTableWoodTypes.Length)];
            string tableStyle = rule.Targets != null && rule.Targets.Length > 0
                ? rule.Targets[Random.Shared.Next(rule.Targets.Length)]
                : string.Empty;
            string[] lanternMaterials = SelectMetalRestrictedPool(RandomLanternMaterials, copperOnly);
            string[] lecternMetalFinishes = SelectMetalRestrictedPool(RandomRestoredCenserMetalFinishes, copperOnly);
            string material = lanternMaterials[Random.Shared.Next(lanternMaterials.Length)];
            string lecternMetalFinish = lecternMetalFinishes[Random.Shared.Next(lecternMetalFinishes.Length)];
            string bedTopMetal = RandomRestoredBedTopMetals[Random.Shared.Next(RandomRestoredBedTopMetals.Length)];
            string libraryMaterial = enabledLibraryMaterials[Random.Shared.Next(enabledLibraryMaterials.Length)];
            string crateWood = enabledCrateWoodTypes[Random.Shared.Next(enabledCrateWoodTypes.Length)];
            string tableMetal = lanternMaterials[Random.Shared.Next(lanternMaterials.Length)];
            string tableClothColor = RandomRestoredMetalTableClothColors[Random.Shared.Next(RandomRestoredMetalTableClothColors.Length)];
            string chairColor = RandomVanillaChairColors[Random.Shared.Next(RandomVanillaChairColors.Length)];
            string targetCode = rule.Target
                .Replace("{wood}", wood, StringComparison.Ordinal)
                .Replace("{tablestyle}", tableStyle, StringComparison.Ordinal)
                .Replace("{tablewood}", tableWood, StringComparison.Ordinal)
                .Replace("{material}", material, StringComparison.Ordinal)
                .Replace("{tablemetal}", tableMetal, StringComparison.Ordinal)
                .Replace("{tableclothcolor}", tableClothColor, StringComparison.Ordinal)
                .Replace("{lecternmetal}", lecternMetalFinish, StringComparison.Ordinal)
                .Replace("{bedtopmetal}", bedTopMetal, StringComparison.Ordinal)
                .Replace("{librarymaterial}", libraryMaterial, StringComparison.Ordinal)
                .Replace("{cratewood}", crateWood, StringComparison.Ordinal)
                .Replace("{chaircolor}", chairColor, StringComparison.Ordinal);
            targetCode = ApplyCopperOnlyBlockTargetRestriction(targetCode, copperOnly);
            Block? block = world.GetBlock(ToAssetLocation(targetCode));
            if (block == null)
            {
                return null;
            }

            int minCount = Math.Max(1, rule.PrimaryMinCount);
            int maxCount = Math.Max(minCount, rule.PrimaryMaxCount);
            int stackCount = maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;
            ItemStack blockStack = new(block, stackCount);
            blockStack.ResolveBlockOrItem(world);
            return blockStack;
        }

        if (rule.TargetKind == RestorationTargetKind.VanillaAttributedBlock)
        {
            Block? block = world.GetBlock(ToAssetLocation(rule.Target));
            if (block == null)
            {
                return null;
            }

            ItemStack blockStack = new(block, 1);
            string libraryMaterial = enabledLibraryMaterials[Random.Shared.Next(enabledLibraryMaterials.Length)];
            string crateWood = enabledCrateWoodTypes[Random.Shared.Next(enabledCrateWoodTypes.Length)];
            string[] attributes = rule.Targets ?? Array.Empty<string>();
            for (int index = 0; index + 1 < attributes.Length; index += 2)
            {
                string value = attributes[index + 1]
                    .Replace("{librarymaterial}", libraryMaterial, StringComparison.Ordinal)
                    .Replace("{cratewood}", crateWood, StringComparison.Ordinal);
                blockStack.Attributes.SetString(attributes[index], value);
            }

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

    private static IEnumerable<ItemStack> CreateSalvageStacks(IWorldAccessor world, string sourceCode, RestorationRule rule, ItemStack? reverserStack)
    {
        string normalizedSource = sourceCode.ToLowerInvariant();
        bool copperOnly = UsesCopperOnlyRestoration(reverserStack);
        string plankCode = PickRandomPlankCode();
        string clothCode = PickRandomClothCode();

        if (normalizedSource.StartsWith("lantern/", StringComparison.OrdinalIgnoreCase)
            || rule.TargetKind == RestorationTargetKind.RandomVanillaLantern)
        {
            ItemStack? glassStack = CreateStackForCode(world, "glass-plain", 1);
            ItemStack? metalStack = CreateStackForCode(world, $"ingot-{PickRandomMetal(RandomLanternMaterials, copperOnly)}", 1);
            ItemStack? waxStack = CreateStackForCode(world, "beeswax", 1);

            if (glassStack != null)
            {
                yield return glassStack;
            }

            if (metalStack != null)
            {
                yield return metalStack;
            }

            if (waxStack != null)
            {
                yield return waxStack;
            }

            yield break;
        }

        if (normalizedSource.StartsWith("candlestub", StringComparison.OrdinalIgnoreCase))
        {
            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "beeswax", GetCandleCountForClutter(normalizedSource))))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (normalizedSource.Contains("anvil-broken", StringComparison.OrdinalIgnoreCase)
            || rule.Target.StartsWith("game:anvil-", StringComparison.OrdinalIgnoreCase))
        {
            string anvilMetal = GetAnvilMetalCode(rule);
            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, $"ingot-{anvilMetal}", 5)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (IsToolOrWeaponSource(normalizedSource))
        {
            ItemStack? ingotStack = CreateStackForCode(world, $"ingot-{PickRandomMetal(RandomSalvageIngotMetals, copperOnly)}", 1);
            ItemStack? stickStack = CreateStackForCode(world, "stick", 1);

            if (ingotStack != null)
            {
                yield return ingotStack;
            }

            if (stickStack != null)
            {
                yield return stickStack;
            }

            yield break;
        }

        if (IsBedSource(normalizedSource, rule))
        {
            int plankCount = 3;
            int nailCount = 0;
            string clothDropCode = clothCode;
            int clothCount = Random.Shared.Next(1, 3);

            if (IsCanopyBedSource(normalizedSource, rule))
            {
                plankCount = 10;
                nailCount = 10;
                clothDropCode = "cloth-white";
                clothCount = 6;
            }
            else if (IsShortBedSource(normalizedSource, rule))
            {
                plankCount = 5;
                nailCount = 5;
                clothDropCode = "cloth-white";
                clothCount = 3;
            }

            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, plankCode, plankCount)))
            {
                yield return itemStack;
            }

            if (nailCount > 0)
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, PickRandomNailCode(copperOnly), nailCount)))
                {
                    yield return itemStack;
                }
            }

            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, clothDropCode, clothCount)))
            {
                yield return itemStack;
            }

            if (normalizedSource.Contains("metal", StringComparison.OrdinalIgnoreCase) || rule.Target.Contains("metal", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, $"ingot-{PickRandomMetal(RandomLanternMaterials, copperOnly)}", 1)))
                {
                    yield return itemStack;
                }
            }

            yield break;
        }

        if (IsLibrarySource(normalizedSource))
        {
            int metalIngotCount = GetSalvageMetalIngotCount(rule);
            if (metalIngotCount > 0)
            {
                foreach (ItemStack itemStack in CreateMetalSalvageStacks(world, metalIngotCount, copperOnly))
                {
                    yield return itemStack;
                }
            }
            else
            {
                foreach (ItemStack itemStack in CreateWoodSalvageStacks(world, plankCode, 5, copperOnly, 5))
                {
                    yield return itemStack;
                }
            }

            int parchmentCount = CountLibraryParchmentDrops(normalizedSource);
            if (parchmentCount > 0)
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "paper-parchment", parchmentCount)))
                {
                    yield return itemStack;
                }
            }

            yield break;
        }

        if (IsCrateSource(normalizedSource, rule))
        {
            foreach (ItemStack itemStack in CreateWoodSalvageStacks(world, plankCode, 5, copperOnly, 5))
            {
                yield return itemStack;
            }

            if (normalizedSource.Contains("books", StringComparison.OrdinalIgnoreCase)
                || normalizedSource.Contains("label", StringComparison.OrdinalIgnoreCase)
                || normalizedSource.Contains("toybox", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "paper-parchment", 1)))
                {
                    yield return itemStack;
                }
            }

            yield break;
        }

        if (IsChairOrTableSource(normalizedSource, rule))
        {
            int metalIngotCount = GetSalvageMetalIngotCount(rule);
            if (metalIngotCount > 0)
            {
                foreach (ItemStack itemStack in CreateMetalSalvageStacks(world, metalIngotCount, copperOnly))
                {
                    yield return itemStack;
                }
            }
            else
            {
                foreach (ItemStack itemStack in CreateWoodSalvageStacks(world, plankCode, 5, copperOnly, 5))
                {
                    yield return itemStack;
                }
            }

            if (UsesMordantClothSalvage(normalizedSource, rule))
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "cloth-mordant", 3)))
                {
                    yield return itemStack;
                }
            }

            if (normalizedSource.Contains("table-long-with-accessories", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "candle", 5)))
                {
                    yield return itemStack;
                }
            }

            yield break;
        }

        int genericMetalIngotCount = GetSalvageMetalIngotCount(rule);
        if (genericMetalIngotCount > 0)
        {
            foreach (ItemStack itemStack in CreateMetalSalvageStacks(world, genericMetalIngotCount, copperOnly))
            {
                yield return itemStack;
            }

            if (UsesMordantClothSalvage(normalizedSource, rule))
            {
                foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "cloth-mordant", 3)))
                {
                    yield return itemStack;
                }
            }
        }
    }

    private static IEnumerable<ItemStack> CreateFallbackClutterSalvageStacks(IWorldAccessor world, BlockPos pos, string clutterType, ItemStack? reverserStack)
    {
        string normalizedSource = clutterType.ToLowerInvariant();
        bool copperOnly = UsesCopperOnlyRestoration(reverserStack);
        List<string> hints = GetClutterTextureHints(world, pos)
            .Select(hint => hint.ToLowerInvariant())
            .ToList();
        hints.Add(normalizedSource);

        bool hasWood = hints.Any(IsWoodHint);
        bool hasCloth = hints.Any(IsClothHint);
        bool hasGlass = hints.Any(IsGlassHint);
        bool hasCeramic = hints.Any(IsCeramicHint);
        int metalHintCount = CountDistinctMetalHints(hints);

        if (hasCeramic)
        {
            string clayCode = GetFallbackClayCode(hints);
            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, clayCode, 1)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (hasGlass)
        {
            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "glass-plain", 1)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (metalHintCount >= 2)
        {
            foreach (ItemStack itemStack in CreateMetalSalvageStacks(world, 2, copperOnly))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (metalHintCount == 1)
        {
            foreach (ItemStack itemStack in CreateMetalSalvageStacks(world, 1, copperOnly))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (hasWood && hasCloth)
        {
            foreach (ItemStack itemStack in CreateWoodSalvageStacks(world, PickRandomPlankCode(), 2, copperOnly, 2))
            {
                yield return itemStack;
            }

            foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, "cloth-mordant", 1)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (hasWood)
        {
            foreach (ItemStack itemStack in CreateWoodSalvageStacks(world, PickRandomPlankCode(), 2, copperOnly, 2))
            {
                yield return itemStack;
            }
        }
    }

    private static IEnumerable<ItemStack> CreateSupplementalRestoredStacks(IWorldAccessor world, RestorationRule rule, ItemStack? reverserStack)
    {
        if (!AreBonusRestorationDropsEnabled(reverserStack))
        {
            yield break;
        }

        if (rule.LootStyle == BonusLootStyle.TieredJunk)
        {
            bool copperOnly = UsesCopperOnlyRestoration(reverserStack);
            foreach (ItemStack itemStack in CreateTieredLootStacks(world, rule, () => PickTieredJunkItemCode(copperOnly)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (rule.LootStyle == BonusLootStyle.TieredMetalJunk)
        {
            bool copperOnly = UsesCopperOnlyRestoration(reverserStack);
            foreach (ItemStack itemStack in CreateTieredLootStacks(world, rule, () => PickWeightedMetalJunkItemCode(copperOnly)))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (rule.LootStyle == BonusLootStyle.ExactListedItems)
        {
            bool copperOnly = UsesCopperOnlyRestoration(reverserStack);
            string[] exactItemCodes = SelectBonusItemCodePool(rule.BonusTargets ?? Array.Empty<string>(), copperOnly);
            foreach (string itemCode in exactItemCodes)
            {
                AssetLocation code = ToAssetLocation(itemCode);
                Item? item = world.GetItem(code);
                if (item != null)
                {
                    yield return new ItemStack(item, GetBonusStackCount(rule));
                    continue;
                }

                Block? block = world.GetBlock(code);
                if (block != null)
                {
                    yield return new ItemStack(block, GetBonusStackCount(rule));
                }
            }

            yield break;
        }

        bool filteredCopperOnly = UsesCopperOnlyRestoration(reverserStack);
        string[] itemCodes = SelectBonusItemCodePool(rule.BonusTargets ?? Array.Empty<string>(), filteredCopperOnly);
        if (itemCodes.Length == 0 || rule.BonusMaxCount <= 0)
        {
            yield break;
        }

        int minCount = Math.Max(0, rule.BonusMinCount);
        int maxCount = Math.Max(minCount, rule.BonusMaxCount);
        int itemCount = Random.Shared.Next(minCount, maxCount + 1);

        for (int index = 0; index < itemCount; index++)
        {
            string itemCode = itemCodes[Random.Shared.Next(itemCodes.Length)];
            AssetLocation code = ToAssetLocation(itemCode);
            Item? item = world.GetItem(code);
            if (item != null)
            {
                int stackCount = GetBonusStackCount(rule);
                ItemStack itemStack = new(item, stackCount);
                itemStack.ResolveBlockOrItem(world);
                yield return itemStack;
                continue;
            }

            Block? block = world.GetBlock(code);
            if (block == null)
            {
                continue;
            }

            int stackCountBlock = GetBonusStackCount(rule);
            ItemStack blockStack = new(block, stackCountBlock);
            blockStack.ResolveBlockOrItem(world);
            yield return blockStack;
        }

        string[] rareItemCodes = SelectBonusItemCodePool(rule.RareBonusTargets ?? Array.Empty<string>(), filteredCopperOnly);
        if (rareItemCodes.Length > 0 && rule.RareBonusChancePercent > 0 && Random.Shared.Next(100) < rule.RareBonusChancePercent)
        {
            string rareItemCode = rareItemCodes[Random.Shared.Next(rareItemCodes.Length)];
            AssetLocation rareCode = ToAssetLocation(rareItemCode);
            int rareStackCount = rule.RareBonusCount <= 0 && string.Equals(rareItemCode, RandomTemporalGearItems[0], StringComparison.OrdinalIgnoreCase)
                ? Random.Shared.Next(1, 3)
                : Math.Max(1, rule.RareBonusCount);
            Item? rareItem = world.GetItem(rareCode);
            if (rareItem != null)
            {
                yield return new ItemStack(rareItem, rareStackCount);
                yield break;
            }

            Block? rareBlock = world.GetBlock(rareCode);
            if (rareBlock != null)
            {
                yield return new ItemStack(rareBlock, rareStackCount);
            }
        }
    }

    private static IEnumerable<ItemStack> CreateSupplementalRestoredTemporalGearStacks(IWorldAccessor world, RestorationRule rule, ItemStack? reverserStack)
    {
        if (!AreBonusRestorationDropsEnabled(reverserStack))
        {
            yield break;
        }

        int bonusChancePercent = GetTemporalGearBonusChancePercent(reverserStack);
        if (bonusChancePercent <= 0 || Random.Shared.Next(100) >= bonusChancePercent)
        {
            yield break;
        }

        ItemStack? gearStack = CreateStackForCode(world, RandomTemporalGearItems[0], 1);
        if (gearStack != null)
        {
            yield return gearStack;
        }
    }

    private static int GetTemporalGearBonusChancePercent(ItemStack? reverserStack)
    {
        string? itemCode = reverserStack?.Collectible?.Code?.Path;
        if (string.IsNullOrWhiteSpace(itemCode))
        {
            return 5;
        }

        if (itemCode.Contains("unstable", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        if (itemCode.Contains("stabilized", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        return 10;
    }

    private static IEnumerable<ItemStack> CreateTieredLootStacks(
        IWorldAccessor world,
        RestorationRule rule,
        Func<string> picker)
    {
        int minCount = Math.Max(1, rule.BonusMinCount);
        int maxCount = Math.Max(minCount, rule.BonusMaxCount);
        int itemCount = maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;

        for (int index = 0; index < itemCount; index++)
        {
            string itemCode = picker();
            int stackCount = itemCode.StartsWith("metalnailsandstrips-", StringComparison.OrdinalIgnoreCase) ? 4 :
                itemCode.StartsWith("metalbit-", StringComparison.OrdinalIgnoreCase) ? 10 : 1;
            ItemStack? itemStack = CreateStackForCode(world, itemCode, stackCount);
            if (itemStack != null)
            {
                yield return itemStack;
            }
        }
    }

    private static string PickTieredJunkItemCode(bool copperOnly = false)
    {
        if (copperOnly)
        {
            return PickCopperOnlyTieredJunkItemCode();
        }

        int roll = Random.Shared.Next(110);
        if (roll < 80)
        {
            return RandomJunkCommonItems[Random.Shared.Next(RandomJunkCommonItems.Length)];
        }

        if (roll < 105)
        {
            if (Random.Shared.Next(100) < 10)
            {
                return RandomJunkArmorItems[Random.Shared.Next(RandomJunkArmorItems.Length)];
            }

            return RandomJunkUncommonItems[Random.Shared.Next(RandomJunkUncommonItems.Length)];
        }

        if (roll < 109)
        {
            return RandomJunkRareItems[Random.Shared.Next(RandomJunkRareItems.Length)];
        }

        return RandomJunkUltraRareItems[Random.Shared.Next(RandomJunkUltraRareItems.Length)];
    }

    private static string PickCopperOnlyTieredJunkItemCode()
    {
        string[] commonPool = FilterTieredJunkPool(RandomJunkCommonItems);
        string[] uncommonPool = FilterTieredJunkPool(RandomJunkUncommonItems);
        string[] armorPool = FilterTieredJunkPool(RandomJunkArmorItems);
        string[] rarePool = FilterTieredJunkPool(RandomJunkRareItems);
        string[] ultraRarePool = FilterTieredJunkPool(RandomJunkUltraRareItems);

        int roll = Random.Shared.Next(110);
        if (roll < 80)
        {
            return PickFilteredTieredJunkCode(commonPool, uncommonPool, armorPool, ultraRarePool);
        }

        if (roll < 105)
        {
            if (Random.Shared.Next(100) < 10 && armorPool.Length > 0)
            {
                return armorPool[Random.Shared.Next(armorPool.Length)];
            }

            return PickFilteredTieredJunkCode(uncommonPool, commonPool, armorPool, ultraRarePool);
        }

        if (roll < 109)
        {
            return PickFilteredTieredJunkCode(rarePool, uncommonPool, commonPool, armorPool, ultraRarePool);
        }

        return PickFilteredTieredJunkCode(ultraRarePool, rarePool, uncommonPool, commonPool, armorPool);
    }

    private static string[] FilterTieredJunkPool(string[] sourcePool)
    {
        if (sourcePool.Length == 0)
        {
            return sourcePool;
        }

        return sourcePool
            .Where(IsCopperAllowedBonusCode)
            .ToArray();
    }

    private static string PickFilteredTieredJunkCode(params string[][] pools)
    {
        foreach (string[] pool in pools)
        {
            if (pool.Length > 0)
            {
                return pool[Random.Shared.Next(pool.Length)];
            }
        }

        return PickTieredJunkItemCode();
    }

    private static string[] SelectBonusItemCodePool(string[] sourcePool, bool copperOnly)
    {
        if (!copperOnly || sourcePool.Length == 0)
        {
            return sourcePool;
        }

        return sourcePool
            .Where(IsCopperAllowedBonusCode)
            .ToArray();
    }

    private static bool IsCopperAllowedBonusCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        if (string.Equals(code, "backpack-sturdy", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (code.StartsWith("armor-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (code.StartsWith("nugget-", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(code, "nugget-nativecopper", StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, "nugget-malachite", StringComparison.OrdinalIgnoreCase);
        }

        if (code.StartsWith("arrow-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("metalnailsandstrips-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("metalbit-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("ingot-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("metalplate-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("axe-felling-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("hammer-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("hoe-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("knife-generic-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("pickaxe-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("saw-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("scythe-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("shovel-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("spear-generic-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("blade-falx-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("chisel-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("wrench-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("prospectingpick-", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("shears-", StringComparison.OrdinalIgnoreCase))
        {
            return code.EndsWith("-copper", StringComparison.OrdinalIgnoreCase);
        }

        if (code.StartsWith("windmillrotor", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("woodenaxle", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("angledgears", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("largegear", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("helvehammerbase", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("helvehammerhead-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string PickWeightedMetalJunkItemCode(bool copperOnly = false)
    {
        if (copperOnly)
        {
            return PickCopperOnlyWeightedMetalJunkItemCode();
        }

        int roll = Random.Shared.Next(101);
        if (roll < 80)
        {
            return RandomMetalJunkCommonItems[Random.Shared.Next(RandomMetalJunkCommonItems.Length)];
        }

        if (roll < 95)
        {
            return RandomMetalJunkUncommonItems[Random.Shared.Next(RandomMetalJunkUncommonItems.Length)];
        }

        if (roll < 100)
        {
            return RandomMetalJunkRareItems[Random.Shared.Next(RandomMetalJunkRareItems.Length)];
        }

        return RandomMetalJunkUltraRareItems[Random.Shared.Next(RandomMetalJunkUltraRareItems.Length)];
    }

    private static string PickCopperOnlyWeightedMetalJunkItemCode()
    {
        string[] commonPool = FilterTieredJunkPool(RandomMetalJunkCommonItems);
        string[] uncommonPool = FilterTieredJunkPool(RandomMetalJunkUncommonItems);
        string[] rarePool = FilterTieredJunkPool(RandomMetalJunkRareItems);
        string[] ultraRarePool = FilterTieredJunkPool(RandomMetalJunkUltraRareItems);

        int roll = Random.Shared.Next(101);
        if (roll < 80)
        {
            return PickFilteredTieredJunkCode(commonPool, uncommonPool, rarePool, ultraRarePool);
        }

        if (roll < 95)
        {
            return PickFilteredTieredJunkCode(uncommonPool, commonPool, rarePool, ultraRarePool);
        }

        if (roll < 100)
        {
            return PickFilteredTieredJunkCode(rarePool, uncommonPool, commonPool, ultraRarePool);
        }

        return PickFilteredTieredJunkCode(ultraRarePool, rarePool, uncommonPool, commonPool);
    }

    private static ItemStack? CreateStackForCode(IWorldAccessor world, string itemCode, int stackCount)
    {
        AssetLocation code = ToAssetLocation(itemCode);
        Item? item = world.GetItem(code);
        if (item != null)
        {
            ItemStack itemStack = new(item, stackCount);
            itemStack.ResolveBlockOrItem(world);
            return itemStack;
        }

        Block? block = world.GetBlock(code);
        if (block == null)
        {
            return null;
        }

        ItemStack blockStack = new(block, stackCount);
        blockStack.ResolveBlockOrItem(world);
        return blockStack;
    }

    private static IEnumerable<ItemStack> YieldIfNotNull(ItemStack? itemStack)
    {
        if (itemStack != null)
        {
            yield return itemStack;
        }
    }

    private static IEnumerable<ItemStack> CreateWoodSalvageStacks(IWorldAccessor world, string plankCode, int plankCount, bool copperOnly, int nailCount)
    {
        foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, plankCode, plankCount)))
        {
            yield return itemStack;
        }

        foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, PickRandomNailCode(copperOnly), nailCount)))
        {
            yield return itemStack;
        }
    }

    private static IEnumerable<ItemStack> CreateMetalSalvageStacks(IWorldAccessor world, int ingotCount, bool copperOnly)
    {
        foreach (ItemStack itemStack in YieldIfNotNull(CreateStackForCode(world, $"ingot-{PickRandomMetal(RandomSalvageIngotMetals, copperOnly)}", ingotCount)))
        {
            yield return itemStack;
        }
    }

    private static string PickRandomPlankCode()
    {
        string[] enabledWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredWoodTypes);
        string woodType = enabledWoodTypes[Random.Shared.Next(enabledWoodTypes.Length)];
        return $"plank-{woodType}";
    }

    private static string PickRandomClothCode()
    {
        return RandomSalvageClothItems[Random.Shared.Next(RandomSalvageClothItems.Length)];
    }

    private static string PickRandomNailCode(bool copperOnly)
    {
        return $"metalnailsandstrips-{PickRandomMetal(RandomSalvageNailMetals, copperOnly)}";
    }

    private static int GetFallbackSalvageDurabilityCost(string clutterType)
    {
        return clutterType.Contains("ruined", StringComparison.OrdinalIgnoreCase)
            || clutterType.Contains("evaporating", StringComparison.OrdinalIgnoreCase)
            || clutterType.Contains("broken", StringComparison.OrdinalIgnoreCase)
            ? RuinedDurabilityCost
            : AgedDurabilityCost;
    }

    private static string GetAnvilMetalCode(RestorationRule rule)
    {
        if (rule.Target.EndsWith("-copper", StringComparison.OrdinalIgnoreCase))
        {
            return "copper";
        }

        if (rule.Target.EndsWith("-bismuthbronze", StringComparison.OrdinalIgnoreCase))
        {
            return "bismuthbronze";
        }

        if (rule.Target.EndsWith("-iron", StringComparison.OrdinalIgnoreCase))
        {
            return "iron";
        }

        return "iron";
    }

    private static string PickRandomMetal(string[] sourcePool, bool copperOnly)
    {
        string[] filteredPool = SelectMetalRestrictedPool(sourcePool, copperOnly);
        return filteredPool[Random.Shared.Next(filteredPool.Length)];
    }

    private static List<string> GetClutterTextureHints(IWorldAccessor world, BlockPos pos)
    {
        List<string> results = [];
        BlockEntity? blockEntity = world.BlockAccessor.GetBlockEntity(pos);
        if (blockEntity == null)
        {
            return results;
        }

        TreeAttribute tree = new();
        blockEntity.ToTreeAttributes(tree);
        CollectTextureHintsFromKnownTrees(tree, results);
        return results;
    }

    private static void CollectTextureHintsFromKnownTrees(ITreeAttribute? tree, List<string> results)
    {
        if (tree == null)
        {
            return;
        }

        AppendKnownTextureValues(tree, results);
        AppendKnownTextureValues(tree.GetTreeAttribute("textures"), results);
        AppendKnownTextureValues(tree.GetTreeAttribute("collectedTextures"), results);

        ITreeAttribute? typeAttributes = tree.GetTreeAttribute("typeAttributes");
        AppendKnownTextureValues(typeAttributes, results);
        AppendKnownTextureValues(typeAttributes?.GetTreeAttribute("textures"), results);

        ITreeAttribute? attributes = tree.GetTreeAttribute("attributes");
        AppendKnownTextureValues(attributes, results);
        AppendKnownTextureValues(attributes?.GetTreeAttribute("textures"), results);
        AppendKnownTextureValues(attributes?.GetTreeAttribute("collectedTextures"), results);

        ITreeAttribute? nestedTypeAttributes = attributes?.GetTreeAttribute("typeAttributes");
        AppendKnownTextureValues(nestedTypeAttributes, results);
        AppendKnownTextureValues(nestedTypeAttributes?.GetTreeAttribute("textures"), results);

        ITreeAttribute? stack = tree.GetTreeAttribute("stack");
        AppendKnownTextureValues(stack, results);
        AppendKnownTextureValues(stack?.GetTreeAttribute("textures"), results);
    }

    private static void AppendKnownTextureValues(ITreeAttribute? tree, List<string> results)
    {
        if (tree == null)
        {
            return;
        }

        string[] keys =
        [
            "metal",
            "cover",
            "cloth",
            "cloth-top",
            "cloth-side",
            "mordant",
            "pillow",
            "wood",
            "bottom",
            "oak",
            "charred",
            "glass",
            "iron",
            "mesh1",
            "mesh2",
            "texture",
            "type",
            "variant",
            "layout",
            "code"
        ];

        foreach (string key in keys)
        {
            string? value = ReadNonEmptyString(tree, key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                results.Add(value);
            }
        }
    }

    private static bool IsWoodHint(string hint)
    {
        return hint.Contains("wood", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("debarked", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("plank", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsClothHint(string hint)
    {
        return hint.Contains("cloth", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("linen", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("cover", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("pillow", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGlassHint(string hint)
    {
        return hint.Contains("glass", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("quartz", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCeramicHint(string hint)
    {
        return hint.Contains("ceramic", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("pottery", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("clay", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("bowl", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("crock", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("crucible", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("jug", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("planter", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("flowerpot", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("storagevessel", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountDistinctMetalHints(IEnumerable<string> hints)
    {
        HashSet<string> metals = [];

        foreach (string hint in hints)
        {
            string? metal = ExtractMetalHint(hint);
            if (metal != null)
            {
                metals.Add(metal);
            }
        }

        return metals.Count;
    }

    private static string? ExtractMetalHint(string hint)
    {
        string[] knownMetals =
        [
            "copper",
            "brass",
            "blackbronze",
            "bismuthbronze",
            "tinbronze",
            "silver",
            "gold",
            "electrum",
            "iron",
            "meteoriciron",
            "steel",
            "molybdochalkos",
            "bismuth"
        ];

        string? explicitMetal = knownMetals.FirstOrDefault(metal => hint.Contains(metal, StringComparison.OrdinalIgnoreCase));
        if (explicitMetal != null)
        {
            return explicitMetal;
        }

        if (hint.Contains("chain", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("shackle", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("manacle", StringComparison.OrdinalIgnoreCase)
            || hint.Contains("fetter", StringComparison.OrdinalIgnoreCase))
        {
            return "iron";
        }

        return null;
    }

    private static string GetFallbackClayCode(IEnumerable<string> hints)
    {
        foreach (string hint in hints)
        {
            if (hint.Contains("blue", StringComparison.OrdinalIgnoreCase))
            {
                return "clay-blue";
            }

            if (hint.Contains("brown", StringComparison.OrdinalIgnoreCase))
            {
                return "clay-brown";
            }

            if (hint.Contains("cream", StringComparison.OrdinalIgnoreCase))
            {
                return "clay-fire";
            }

            if (hint.Contains("red", StringComparison.OrdinalIgnoreCase))
            {
                return "clay-red";
            }
        }

        return "clay-blue";
    }

    private static int GetCandleCountForClutter(string normalizedSource)
    {
        if (normalizedSource.Contains("candlestub-single", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (normalizedSource.Contains("candlestubs-bunch1", StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (normalizedSource.Contains("candlestubs-bunch2", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        if (normalizedSource.Contains("candlestubs-bunch3", StringComparison.OrdinalIgnoreCase))
        {
            return 7;
        }

        if (normalizedSource.Contains("candlestubs-bunch4", StringComparison.OrdinalIgnoreCase))
        {
            return 9;
        }

        return 0;
    }

    private static bool IsToolOrWeaponSource(string normalizedSource)
    {
        return normalizedSource.Contains("tool-", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("pile-weapon", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("pile-tools", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("woodworkingtools", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("precisiontools", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLibrarySource(string normalizedSource)
    {
        return normalizedSource.StartsWith("bookshelves/", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("shelf-", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("shelf/", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("lecturn-", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Equals("full", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("half", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("doublesided", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bookshelf", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("scrollrack", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bookstand", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bookpile", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bookstack", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bookrow", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("large-book", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("cartography-book", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountLibraryParchmentDrops(string normalizedSource)
    {
        if (normalizedSource.Contains("full", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("book", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("scrollrack", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("doublesidednew", StringComparison.OrdinalIgnoreCase))
        {
            return Random.Shared.Next(1, 4);
        }

        return 0;
    }

    private static bool IsCrateSource(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.StartsWith("crate/", StringComparison.OrdinalIgnoreCase)
            || IsCrateRestorationRule(rule);
    }

    private static bool IsChairOrTableSource(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.StartsWith("chair", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.StartsWith("table", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-chair-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-table-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-metal-table-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool UsesMordantClothSalvage(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.Contains("cloth", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("pillow", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-table-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-chair-colored", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-chair-metal", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-metal-table-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBedSource(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.StartsWith("bed", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("fancy-bed", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bed-short", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-short-bed-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-canopy-bed-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("bed-woodaged", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("metal-bed-high", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCanopyBedSource(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.Contains("fancy-bed", StringComparison.OrdinalIgnoreCase)
            || normalizedSource.Contains("bed-fancy", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-canopy-bed-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShortBedSource(string normalizedSource, RestorationRule rule)
    {
        return normalizedSource.Contains("bed-short", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-short-bed-", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetSalvageMetalIngotCount(RestorationRule rule)
    {
        if (rule.TargetKind == RestorationTargetKind.RandomVanillaLantern)
        {
            return 1;
        }

        if (rule.TargetKind == RestorationTargetKind.RandomRestoredCenser)
        {
            return rule.Target.StartsWith("metal", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        if (rule.Target.Contains("restored-chair-metal", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-metal-bed-high", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-metal-table-low", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (rule.Target.Contains("restored-metal-table-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-brazier-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-chandelier-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-lectern-metal-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("torchholder-", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 0;
    }

    private static int GetBonusStackCount(RestorationRule rule)
    {
        int minCount = Math.Max(1, rule.BonusItemMinCount);
        int maxCount = Math.Max(minCount, rule.BonusItemMaxCount);
        return maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;
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

        string? directLayout = ReadNonEmptyString(tree, "layout");
        if (directLayout != null) return directLayout;

        string? directVariant = ReadFirstNonEmptyString(tree, "variant", "bookshelfVariant", "randomVariant", "code");
        if (directVariant != null) return directVariant;

        ITreeAttribute? attributes = tree.GetTreeAttribute("attributes");
        string? nested = ReadNonEmptyString(attributes, "type");
        if (nested != null) return nested;

        string? nestedLayout = ReadNonEmptyString(attributes, "layout");
        if (nestedLayout != null) return nestedLayout;

        string? nestedVariant = ReadFirstNonEmptyString(attributes, "variant", "bookshelfVariant", "randomVariant", "code");
        if (nestedVariant != null) return nestedVariant;

        ITreeAttribute? stack = tree.GetTreeAttribute("stack");
        string? stackType = ReadNonEmptyString(stack, "type");
        if (stackType != null) return stackType;

        string? stackLayout = ReadNonEmptyString(stack, "layout");
        if (stackLayout != null) return stackLayout;

        string? stackVariant = ReadFirstNonEmptyString(stack, "variant", "bookshelfVariant", "randomVariant", "code");
        if (stackVariant != null) return stackVariant;

        return null;
    }

    private static bool IsCrateRestorationRule(RestorationRule rule)
    {
        if (rule.TargetKind == RestorationTargetKind.VanillaAttributedBlock &&
            string.Equals(rule.Target, "game:crate", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (rule.TargetKind == RestorationTargetKind.Block &&
            !string.IsNullOrWhiteSpace(rule.Target) &&
            rule.Target.Contains("restored-crate-", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool IsFurnitureRestorationRule(RestorationRule rule)
    {
        if (rule.TargetKind != RestorationTargetKind.Block || string.IsNullOrWhiteSpace(rule.Target))
        {
            return false;
        }

        return rule.Target.Contains("restored-chair-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-short-bed-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-canopy-bed-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-table-", StringComparison.OrdinalIgnoreCase)
            || rule.Target.Contains("restored-metal-table-", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rule.Target, "vstemporalreverser:restored-table-{tablestyle}-{tablewood}-north", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rule.Target, "vstemporalreverser:restored-table-{tablestyle}-{tablewood}", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rule.Target, "vstemporalreverser:restored-table-{tablewood}", StringComparison.OrdinalIgnoreCase);
    }

    private static string DescribeStack(ItemStack stack)
    {
        string code = stack.Collectible?.Code?.ToShortString() ?? "<null>";
        string type = stack.Attributes?.GetString("type", "") ?? "";
        return string.IsNullOrWhiteSpace(type) ? code : $"{code} type={type}";
    }

    private static string SanitizeDecorationType(string clutterType)
    {
        return clutterType
            .Replace("/", "-", StringComparison.Ordinal)
            .ToLowerInvariant();
    }

    private static string? ReadNonEmptyString(ITreeAttribute? tree, string key)
    {
        if (tree == null) return null;

        string value = tree.GetString(key, "");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? ReadFirstNonEmptyString(ITreeAttribute? tree, params string[] keys)
    {
        foreach (string key in keys)
        {
            string? value = ReadNonEmptyString(tree, key);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private static bool AreBonusRestorationDropsEnabled(ItemStack? stack)
    {
        return stack?.Collectible?.Attributes?["bonusRestorationDropsEnabled"].AsBool(DefaultBonusLootEnabled) ?? DefaultBonusLootEnabled;
    }

    private static long GetRestoreCooldownMs(ItemStack? stack)
    {
        float configuredSeconds = VSTemporalReverserModSystem.Config.RestoreCooldownSeconds;
        float clampedSeconds = Math.Clamp(configuredSeconds, 0f, 3f);
        return (long)Math.Round(clampedSeconds * 1000f);
    }

    private static int GetRestoreSpawnDelayMs(ItemStack? stack)
    {
        return Math.Max(0, stack?.Collectible?.Attributes?["restoreSpawnDelayMs"].AsInt(DefaultRestoreSpawnDelayMs) ?? DefaultRestoreSpawnDelayMs);
    }

    private static AssetLocation GetRestoreSoundLocation(ItemStack? stack)
    {
        string soundCode = stack?.Collectible?.Attributes?["restoreSound"].AsString("game:sounds/effect/translocate")
            ?? "game:sounds/effect/translocate";
        return ToAssetLocation(soundCode);
    }

    private static void SpawnTemporalSmoke(IWorldAccessor world, Vec3d center)
    {
        world.SpawnParticles(
            18f,
            unchecked((int)0xD8C8F4FF),
            center.AddCopy(-0.75, -0.08, -0.75),
            center.AddCopy(0.75, 0.42, 0.75),
            new Vec3f(-0.75f, 0.04f, -0.75f),
            new Vec3f(0.75f, 1.15f, 0.75f),
            0.08f,
            0f,
            0.72f,
            EnumParticleModel.Quad,
            null
        );

        world.SpawnParticles(
            14f,
            unchecked((int)0xD25AB4FF),
            center.AddCopy(-0.9, -0.08, -0.9),
            center.AddCopy(0.9, 0.35, 0.9),
            new Vec3f(-0.55f, 0.06f, -0.55f),
            new Vec3f(0.55f, 0.55f, 0.55f),
            0.52f,
            -0.005f,
            0.16f,
            EnumParticleModel.Quad,
            null
        );

        world.RegisterCallback(_ =>
        {
            world.SpawnParticles(
                18f,
                unchecked((int)0xC84AA8FF),
                center.AddCopy(-1.15, -0.06, -1.15),
                center.AddCopy(1.15, 0.4, 1.15),
                new Vec3f(-0.42f, 0.03f, -0.42f),
                new Vec3f(0.42f, 0.28f, 0.42f),
                0.82f,
                -0.008f,
                0.14f,
                EnumParticleModel.Quad,
                null
            );
        }, 70);
    }

    private static bool UsesCopperOnlyRestoration(ItemStack? stack)
    {
        string restriction = stack?.Collectible?.Attributes?["restorationMetalRestriction"].AsString(DefaultMetalRestriction)
            ?? DefaultMetalRestriction;
        return string.Equals(restriction, CopperMetalRestriction, StringComparison.OrdinalIgnoreCase);
    }

    private static string ApplyCopperOnlyBlockTargetRestriction(string targetCode, bool copperOnly)
    {
        if (!copperOnly || string.IsNullOrWhiteSpace(targetCode))
        {
            return targetCode;
        }

        return targetCode switch
        {
            "game:anvil-bismuthbronze" => "game:anvil-copper",
            "game:anvil-iron" => "game:anvil-copper",
            _ => targetCode
        };
    }

    private static string[] SelectMetalRestrictedPool(string[] sourcePool, bool copperOnly)
    {
        if (!copperOnly || sourcePool.Length == 0)
        {
            return sourcePool;
        }

        string[] filtered = sourcePool
            .Where(code => string.Equals(code, "copper", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return filtered.Length > 0 ? filtered : sourcePool;
    }

    private static string[] SelectItemCodePool(string[] sourcePool, bool copperOnly)
    {
        if (!copperOnly || sourcePool.Length == 0)
        {
            return sourcePool;
        }

        string[] filtered = sourcePool
            .Where(code =>
                code.EndsWith("-copper", StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, "tongs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return filtered.Length > 0 ? filtered : sourcePool;
    }

    private static RestorationRule? TryGetCenserRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("censer/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["censer/".Length..]
            : clutterType;
        normalized = normalized.StartsWith("shelf/", StringComparison.OrdinalIgnoreCase)
            ? normalized["shelf/".Length..]
            : normalized;

        int durabilityCost = normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase)
            ? RuinedDurabilityCost
            : AgedDurabilityCost;

        return normalized switch
        {
            _ when normalized.StartsWith("ceramic1", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "ceramic1", RandomRestoredCenserCeramicFinishes),
            _ when normalized.StartsWith("ceramic2", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "ceramic2", RandomRestoredCenserCeramicFinishes),
            _ when normalized.StartsWith("ceramic3", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "ceramic3", RandomRestoredCenserCeramicFinishes),
            "tool-axe" => RandomVanillaItemRule(durabilityCost, RandomRestoredAxeItems),
            "tool-hammer" => RandomVanillaItemRule(durabilityCost, RandomRestoredHammerItems),
            "tool-hoe" => RandomVanillaItemRule(durabilityCost, RandomRestoredHoeItems),
            "tool-knife" => RandomVanillaItemRule(durabilityCost, RandomRestoredKnifeItems),
            "tool-pickaxe" => RandomVanillaItemRule(durabilityCost, RandomRestoredPickaxeItems),
            "tool-saw" => RandomVanillaItemRule(durabilityCost, RandomRestoredSawItems),
            "tool-scythe" => RandomVanillaItemRule(durabilityCost, RandomRestoredScytheItems),
            "tool-shovel" => RandomVanillaItemRule(durabilityCost, RandomRestoredShovelItems),
            "tool-spear" => RandomVanillaItemRule(durabilityCost, RandomRestoredSpearItems),
            _ when normalized.StartsWith("metal1-ceiling", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal1", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal1-wall", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal1", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal1", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal1", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal2-ceiling", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal2", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal2-wall", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal2", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal2", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal2", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal3-ceiling", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal3", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal3-wall", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal3", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal3", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal3", RandomRestoredCenserMetalFinishes),
            _ when normalized.StartsWith("metal4", StringComparison.OrdinalIgnoreCase) => RandomRestoredCenserRule(durabilityCost, "metal4", RandomRestoredCenserMetalFinishes),
            _ => null
        };
    }

    private static RestorationRule? TryGetAnvilRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;
        string simplified = normalized.Replace("/", "-", StringComparison.Ordinal);
        simplified = simplified.StartsWith("clutter-", StringComparison.OrdinalIgnoreCase)
            ? simplified["clutter-".Length..]
            : simplified;

        return simplified switch
        {
            "anvil-broken1" => VanillaBlockRule(RuinedDurabilityCost, "game:anvil-copper"),
            "anvil-broken2" => VanillaBlockRule(RuinedDurabilityCost, "game:anvil-bismuthbronze"),
            "anvil-broken3" => VanillaBlockRule(RuinedDurabilityCost, "game:anvil-iron"),
            _ => null
        };
    }

    private static RestorationRule? TryGetCandleRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;

        int candleCount = GetCandleCountForClutter(normalized);
        if (candleCount <= 0)
        {
            return null;
        }

        int durabilityCost = normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase)
            ? RuinedDurabilityCost
            : AgedDurabilityCost;

        return RandomVanillaItemRule(durabilityCost, ["candle"], candleCount, candleCount);
    }

    private static RestorationRule? TryGetToolOrWeaponRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;
        string simplified = normalized.Replace("/", "-", StringComparison.Ordinal);
        simplified = simplified.StartsWith("clutter-", StringComparison.OrdinalIgnoreCase)
            ? simplified["clutter-".Length..]
            : simplified;

        return simplified switch
        {
            "tool-axe" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredAxeItems),
            "tool-hammer" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredHammerItems),
            "tool-hoe" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredHoeItems),
            "tool-knife" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredKnifeItems),
            "tool-pickaxe" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPickaxeItems),
            "tool-saw" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredSawItems),
            "tool-scythe" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredScytheItems),
            "tool-shovel" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredShovelItems),
            "tool-spear" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredSpearItems),
            "pile-weapon1" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon2" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon3" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon4" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon5" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon6" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon7" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-weapon8" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWeaponItems),
            "pile-tools1" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPileTools1Items),
            "pile-tools2" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPileTools2Items),
            "pile-tools3" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPileTools3Items),
            "pile-tools4" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPileTools4Items),
            "pile-woodworkingtools" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWoodworkingToolItems),
            "pile-trash-pottery" => RandomVanillaItemRule(AgedDurabilityCost, RandomPotteryItems, 1, 1),
            "pile-trash-oldore" => RandomVanillaItemRule(AgedDurabilityCost, RandomOreNuggetItems, 6, 12),
            "pile-trash-scrap" => TieredJunkItemRule(AgedDurabilityCost, 1, 1, 1, 3),
            _ when simplified.Contains("pottery", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("potsherd", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("potsherds", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("potsher", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("sherd", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("sherds", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("shard", StringComparison.OrdinalIgnoreCase)
                || simplified.Contains("shards", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(AgedDurabilityCost, RandomPotteryItems, 1, 1),
            _ when simplified.Contains("trash", StringComparison.OrdinalIgnoreCase)
                && (simplified.Contains("pottery", StringComparison.OrdinalIgnoreCase)
                    || simplified.Contains("potsherd", StringComparison.OrdinalIgnoreCase)
                    || simplified.Contains("potsherds", StringComparison.OrdinalIgnoreCase)
                    || simplified.Contains("shard", StringComparison.OrdinalIgnoreCase)
                    || simplified.Contains("shards", StringComparison.OrdinalIgnoreCase)) => RandomVanillaItemRule(AgedDurabilityCost, RandomPotteryItems, 1, 1),
            _ when simplified.Contains("trash", StringComparison.OrdinalIgnoreCase) && simplified.Contains("oldore", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(AgedDurabilityCost, RandomOreNuggetItems, 6, 12),
            _ when simplified.Contains("trash", StringComparison.OrdinalIgnoreCase) && simplified.Contains("scrap", StringComparison.OrdinalIgnoreCase) => TieredJunkItemRule(AgedDurabilityCost, 1, 1, 1, 3),
            _ when normalized.Contains("precisiontools", StringComparison.OrdinalIgnoreCase)
                && !normalized.StartsWith("shelf", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPrecisionToolItems),
            _ when normalized.Contains("woodworkingtools", StringComparison.OrdinalIgnoreCase)
                && !normalized.StartsWith("shelf", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWoodworkingToolItems),
            "crate-large-tools1" => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:crate",
                RandomRestoredToolItems,
                2,
                2,
                "type",
                "wood-{cratewood}",
                "lidState",
                "closed",
                "label",
                "paper-tools"),
            _ => null
        };
    }

    private static RestorationRule? TryGetShelfRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;
        string simplified = normalized.Replace("/", "-", StringComparison.Ordinal);
        simplified = simplified.StartsWith("clutter-", StringComparison.OrdinalIgnoreCase)
            ? simplified["clutter-".Length..]
            : simplified;

        RestorationRule ShelfRule(int durabilityCost)
        {
            return VanillaBlockRule(
                durabilityCost,
                "game:shelf-normal-east");
        }

        RestorationRule ShelfRuleWithBonus(
            int durabilityCost,
            string[] bonusItems,
            int bonusMinCount = 1,
            int bonusMaxCount = 1,
            int bonusItemMinCount = 1,
            int bonusItemMaxCount = 1)
        {
            return VanillaBlockWithBonusAndRareItemsRule(
                durabilityCost,
                "game:shelf-normal-east",
                bonusItems,
                bonusMinCount,
                bonusMaxCount,
                bonusItemMinCount,
                bonusItemMaxCount);
        }

        RestorationRule ShelfRuleWithBonusAndRare(
            int durabilityCost,
            string[] bonusItems,
            int bonusMinCount,
            int bonusMaxCount,
            int rareBonusChancePercent,
            string[] rareBonusItems,
            int bonusItemMinCount = 1,
            int bonusItemMaxCount = 1,
            int rareBonusCount = 1)
        {
            return VanillaBlockWithBonusAndRareItemsRule(
                durabilityCost,
                "game:shelf-normal-east",
                bonusItems,
                bonusMinCount,
                bonusMaxCount,
                bonusItemMinCount,
                bonusItemMaxCount,
                rareBonusItems,
                rareBonusChancePercent,
                rareBonusCount: rareBonusCount);
        }

        return simplified switch
        {
            "shelf-medical" => ShelfRuleWithBonus(RuinedDurabilityCost, RandomMedicalSupplyItems, 1, 1, 4, 6),
            _ when simplified.StartsWith("shelf-shoes", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(RuinedDurabilityCost, RandomShoeItems),
            _ when simplified.StartsWith("shelf-clothing", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(RuinedDurabilityCost, RandomShelfClothingItems, 2, 2),
            _ when simplified.StartsWith("shelf-flasks", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(RuinedDurabilityCost, RandomShelfFlaskItems, 1, 2),
            "shelf-lab-equipment" => ShelfRuleWithBonusAndRare(RuinedDurabilityCost, RandomShelfLabItems, 1, 1, 50, RandomTemporalGearItems, 1, 1, 0),
            "shelf-lamp" => ShelfRuleWithBonus(RuinedDurabilityCost, RandomShelfLampItems),
            "shelf-tools" => ShelfRuleWithBonus(RuinedDurabilityCost, RandomRestoredToolItems),
            "shelf-woodworkingtools" => ShelfRuleWithBonus(RuinedDurabilityCost, RandomRestoredWoodworkingToolItems),
            "shelf-drafting-instrument" => ShelfRuleWithBonus(RuinedDurabilityCost, RandomRestoredPrecisionToolItems),
            _ when simplified.StartsWith("shelf-shelf-precisiontools", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(AgedDurabilityCost, RandomRestoredPrecisionToolItems),
            _ when simplified.StartsWith("shelf-shelf-drafting-instrument", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(AgedDurabilityCost, RandomRestoredPrecisionToolItems),
            _ when simplified.StartsWith("shelf-shelf-lab-equipment", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonusAndRare(AgedDurabilityCost, RandomShelfLabItems, 1, 1, 50, RandomTemporalGearItems, 1, 1, 0),
            _ when simplified.StartsWith("shelf-shelf-alchemy", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(AgedDurabilityCost, RandomShelfAlchemyItems, 2, 3),
            _ when simplified.StartsWith("shelf-shelf-reagents", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(AgedDurabilityCost, RandomShelfAlchemyItems, 2, 3),
            _ when simplified.StartsWith("shelf-shelf-stuff", StringComparison.OrdinalIgnoreCase) => ShelfRuleWithBonus(AgedDurabilityCost, RandomShelfMiscItems, 1, 3),
            _ when simplified.StartsWith("shelf-", StringComparison.OrdinalIgnoreCase) => ShelfRule(RuinedDurabilityCost),
            _ => null
        };
    }

    private static RestorationRule? TryGetToyRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;

        return normalized switch
        {
            "toy1" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy10"]),
            "toy2" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy10"]),
            "toy3" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy10"]),
            "toy4" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy4"]),
            "toy5" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy5"]),
            "toy6" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy6"]),
            "toy7" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy7"]),
            "toy8" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy8"]),
            "toy9" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy9"]),
            "toy10" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy10"]),
            "toy11" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy11"]),
            "toy12" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy12"]),
            "toy13" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy13"]),
            "toy14" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy14"]),
            "toy15" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy15"]),
            "toy16" => RandomVanillaItemRule(AgedDurabilityCost, ["vstemporalreverser:restored-toy-toy16"]),
            _ => null
        };
    }

    private static RestorationRule? TryGetToyShelfRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        return clutterType switch
        {
            "shelf-toys1" => VanillaAttributedBlockWithExactBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomToyShelf1Items,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            "shelf-toys2" => VanillaAttributedBlockWithExactBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomToyShelf2Items,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            "shelf-toys3" => VanillaAttributedBlockWithExactBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomToyShelf3Items,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            _ => null
        };
    }

    private static RestorationRule? TryGetCrateJunkRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        return clutterType switch
        {
            "crate/large-metaljunk1" => VanillaAttributedBlockWithBonusAndRareItemsRule(
                AgedDurabilityCost,
                "game:crate",
                Array.Empty<string>(),
                2,
                4,
                lootStyle: BonusLootStyle.TieredMetalJunk,
                rareBonusEntities: RandomLargeCrateRaccoonEntities,
                rareBonusEntityChancePercent: 10,
                rareBonusEntityMinCount: 1,
                rareBonusEntityMaxCount: 5,
                attributes: new[] { "type", "wood-{cratewood}", "lidState", "closed", "label", "paper-storage" }),
            "large-metaljunk1" => VanillaAttributedBlockWithBonusAndRareItemsRule(
                AgedDurabilityCost,
                "game:crate",
                Array.Empty<string>(),
                2,
                4,
                lootStyle: BonusLootStyle.TieredMetalJunk,
                rareBonusEntities: RandomLargeCrateRaccoonEntities,
                rareBonusEntityChancePercent: 10,
                rareBonusEntityMinCount: 1,
                rareBonusEntityMaxCount: 5,
                attributes: new[] { "type", "wood-{cratewood}", "lidState", "closed", "label", "paper-storage" }),
            _ => null
        };
    }

    private static RestorationRule? TryGetBellowsRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        return clutterType switch
        {
            _ when clutterType.StartsWith("bellowsagedcrude", StringComparison.OrdinalIgnoreCase) => VanillaBlockRule(AgedDurabilityCost, "game:bellows-crude-north"),
            _ when clutterType.StartsWith("bellowsagedsmall", StringComparison.OrdinalIgnoreCase) => VanillaBlockRule(AgedDurabilityCost, "game:bellows-small-north"),
            _ when clutterType.StartsWith("bellowsagedlarge", StringComparison.OrdinalIgnoreCase) => VanillaBlockRule(AgedDurabilityCost, "game:bellows-large-north"),
            _ => null
        };
    }

    private static RestorationRule? TryGetChairOrLibraryRule(string? clutterType)
    {
        if (string.IsNullOrWhiteSpace(clutterType))
        {
            return null;
        }

        string normalized = clutterType.StartsWith("clutter/", StringComparison.OrdinalIgnoreCase)
            ? clutterType["clutter/".Length..]
            : clutterType;

        static RestorationRule LecternFamilyRule(int durabilityCost, string targetCode, bool includeBook)
        {
            return includeBook
                ? VanillaBlockWithBonusItemsRule(durabilityCost, targetCode, RandomNormalBookItems, 1, 1)
                : VanillaBlockRule(durabilityCost, targetCode);
        }

        static RestorationRule RestoredCrateFamilyRule(int durabilityCost, string size, int primaryCount = 1)
        {
            return new RestorationRule(
                durabilityCost,
                RestorationTargetKind.Block,
                $"vstemporalreverser:restored-crate-{size}-{{cratewood}}",
                PrimaryMinCount: primaryCount,
                PrimaryMaxCount: primaryCount);
        }

        RestorationRule RestoredCrateFamilyWithBonusRule(
            int durabilityCost,
            string size,
            string[] bonusItems,
            int minCount,
            int maxCount,
            int bonusItemMinCount = 1,
        int bonusItemMaxCount = 1,
        string[]? rareBonusItems = null,
        int rareBonusChancePercent = 0,
        string[]? rareBonusEntities = null,
        string[][]? rareBonusEntityGroups = null,
        int rareBonusEntityChancePercent = 0,
        int rareBonusEntityMinCount = 1,
        int rareBonusEntityMaxCount = 1,
            BonusLootStyle lootStyle = BonusLootStyle.None)
        {
            return VanillaBlockWithBonusAndRareItemsRule(
                durabilityCost,
                $"vstemporalreverser:restored-crate-{size}-{{cratewood}}",
                bonusItems,
                minCount,
                maxCount,
                bonusItemMinCount,
                bonusItemMaxCount,
            rareBonusItems,
            rareBonusChancePercent,
            rareBonusEntities,
            rareBonusEntityGroups,
            rareBonusEntityChancePercent,
            rareBonusEntityMinCount,
            rareBonusEntityMaxCount,
            lootStyle: lootStyle);
        }

        static RestorationRule LargeCrateRule(int durabilityCost, string? label = null)
        {
            return label == null
                ? VanillaAttributedBlockWithBonusAndRareItemsRule(
                    durabilityCost,
                "game:crate",
                Array.Empty<string>(),
                0,
                0,
                rareBonusEntityGroups: RandomCrateCreatureEntityGroups,
                rareBonusEntityChancePercent: 10,
                rareBonusEntityMinCount: 1,
                rareBonusEntityMaxCount: 5,
                attributes: new[] { "type", "wood-{cratewood}", "lidState", "closed" })
                : VanillaAttributedBlockWithBonusAndRareItemsRule(
                    durabilityCost,
                "game:crate",
                Array.Empty<string>(),
                0,
                0,
                rareBonusEntityGroups: RandomCrateCreatureEntityGroups,
                rareBonusEntityChancePercent: 10,
                rareBonusEntityMinCount: 1,
                rareBonusEntityMaxCount: 5,
                attributes: new[] { "type", "wood-{cratewood}", "lidState", "closed", "label", label });
        }

        RestorationRule LargeCrateWithBonusRule(
            int durabilityCost,
            string[] bonusItems,
            int minCount,
            int maxCount,
            string? label = null,
        int bonusItemMinCount = 1,
        int bonusItemMaxCount = 1,
            string[]? rareBonusItems = null,
            int rareBonusChancePercent = 0,
            string[]? rareBonusEntities = null,
            string[][]? rareBonusEntityGroups = null,
            int rareBonusEntityChancePercent = 0,
        int rareBonusEntityMinCount = 1,
        int rareBonusEntityMaxCount = 1,
        BonusLootStyle lootStyle = BonusLootStyle.None)
        {
            rareBonusEntityGroups ??= RandomCrateCreatureEntityGroups;
            if (rareBonusEntityChancePercent <= 0)
            {
                rareBonusEntityChancePercent = 10;
            }

            return label == null
                ? VanillaAttributedBlockWithBonusAndRareItemsRule(durabilityCost, "game:crate", bonusItems, minCount, maxCount, bonusItemMinCount, bonusItemMaxCount, rareBonusItems, rareBonusChancePercent, rareBonusEntities, rareBonusEntityGroups, rareBonusEntityChancePercent, rareBonusEntityMinCount, rareBonusEntityMaxCount, lootStyle: lootStyle, attributes: ["type", "wood-{cratewood}", "lidState", "closed"])
                : VanillaAttributedBlockWithBonusAndRareItemsRule(durabilityCost, "game:crate", bonusItems, minCount, maxCount, bonusItemMinCount, bonusItemMaxCount, rareBonusItems, rareBonusChancePercent, rareBonusEntities, rareBonusEntityGroups, rareBonusEntityChancePercent, rareBonusEntityMinCount, rareBonusEntityMaxCount, lootStyle: lootStyle, attributes: ["type", "wood-{cratewood}", "lidState", "closed", "label", label]);
        }

        static int DecorativeDurabilityCost(string code)
        {
            return code.Contains("ruined", StringComparison.OrdinalIgnoreCase)
                || code.Contains("evaporating", StringComparison.OrdinalIgnoreCase)
                ? RuinedDurabilityCost
                : AgedDurabilityCost;
        }

        return normalized switch
        {
            "chair-aged" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-ebony" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-ebony", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-back" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-back", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-crude" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-crude", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-long" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-long-{librarymaterial}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-metal1" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-metal1-pillow" => VanillaBlockRuleWithCritters(AgedDurabilityCost, "vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-metal1-ruined1" => VanillaBlockRuleWithCritters(RuinedDurabilityCost, "vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-metal1-ruined2" => VanillaBlockRuleWithCritters(RuinedDurabilityCost, "vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "chair-metal1-ruined3" => VanillaBlockRuleWithCritters(RuinedDurabilityCost, "vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            _ when normalized.StartsWith("chair-ruined", StringComparison.OrdinalIgnoreCase) => VanillaBlockRuleWithCritters(RuinedDurabilityCost, "vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}", new[] { RandomMothCreatureEntities }, 50, new[] { RandomMouseCreatureEntities }, 20),
            "crate-large-tools1" => LargeCrateWithBonusRule(RuinedDurabilityCost, RandomRestoredToolItems, 2, 2, "paper-tools"),
            "crate/crate-medium-books" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "medium", RandomNormalBookItems, 2, 4, rareBonusEntityGroups: new[] { RandomCrateCreatureEntityGroups[0], RandomCrateCreatureEntityGroups[1], RandomMothCreatureEntities }, rareBonusEntityChancePercent: 40),
            "crate/crate-medium-pottery" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "medium", RandomPotteryItems, 1, 1, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 10),
            "crate/crate-medium-pottery-alt" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "medium", RandomPotteryItems, 1, 1, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 10),
            "crate/crate-small-pottery" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "small", RandomPotteryItems, 1, 2, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 10),
            "crate/crate-large-pottery" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomPotteryItems, 1, 2, "paper-decoration"),
            "crate/large-pottery1" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomPotteryItems, 1, 2, "paper-decoration"),
            "crate/large-pottery2" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomPotteryItems, 1, 2, "paper-decoration"),
            "crate/large-pottery3" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomPotteryItems, 1, 2, "paper-decoration"),
            "crate/crate-large-ore1" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomOreNuggetItems, 1, 1, "paper-ingredients", 20, 40),
            "crate/crate-large-ore2" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomOreNuggetItems, 1, 1, "paper-ingredients", 20, 40),
            "crate/crate-large-ore3" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomOreNuggetItems, 1, 1, "paper-ingredients", 20, 40),
            "crate/crate-large-oldore" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomOreNuggetItems, 1, 1, "paper-ingredients", 20, 40),
            _ when normalized.Contains("contamin", StringComparison.OrdinalIgnoreCase) && normalized.Contains("ore", StringComparison.OrdinalIgnoreCase) => LargeCrateWithBonusRule(
                AgedDurabilityCost,
                RandomOreNuggetItems,
                1,
                1,
                "paper-ingredients",
                20,
                40,
                new[] { "nugget-pentlandite", "nugget-uranium" }),
            "crate/large-clothing1" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomShelfClothingItems, 4, 4, "paper-decoration", 1, 1, RandomRareClothingItems, 1, rareBonusEntityGroups: new[] { RandomMothCreatureEntities }, rareBonusEntityChancePercent: 40),
            "crate/crate-large-junk" => LargeCrateWithBonusRule(AgedDurabilityCost, Array.Empty<string>(), 2, 4, "paper-storage", rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 25, rareBonusEntityMinCount: 1, rareBonusEntityMaxCount: 5, lootStyle: BonusLootStyle.TieredJunk),
            "crate/crate-medium-junk" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "medium", Array.Empty<string>(), 1, 2, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 25, rareBonusEntityMinCount: 1, rareBonusEntityMaxCount: 5, lootStyle: BonusLootStyle.TieredJunk),
            "crate/crate-small-junk" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "small", Array.Empty<string>(), 1, 2, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 25, rareBonusEntityMinCount: 1, rareBonusEntityMaxCount: 5, lootStyle: BonusLootStyle.TieredJunk),
            "crate/large-generic-junk1" => LargeCrateWithBonusRule(AgedDurabilityCost, Array.Empty<string>(), 2, 4, "paper-storage", rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 25, rareBonusEntityMinCount: 1, rareBonusEntityMaxCount: 5, lootStyle: BonusLootStyle.TieredJunk),
            "crate/large-metaljunk1" => VanillaAttributedBlockWithBonusAndRareItemsRule(
                AgedDurabilityCost,
                "game:crate",
                Array.Empty<string>(),
                2,
                4,
                lootStyle: BonusLootStyle.TieredMetalJunk,
                attributes: new[] { "type", "wood-{cratewood}", "lidState", "closed", "label", "paper-storage" }),
            "crate/crate-small-rot" => RestoredCrateFamilyWithBonusRule(AgedDurabilityCost, "small", RandomRotItems, 2, 4, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 40, rareBonusEntityMinCount: 1, rareBonusEntityMaxCount: 5),
            "crate/crate-large-rot" => LargeCrateWithBonusRule(
                AgedDurabilityCost,
                RandomRotItems,
                4,
                6,
                "paper-ingredients",
                rareBonusEntityGroups: new[] { RandomLargeCrateRaccoonEntities, RandomLargeRotCrateMouseEntities },
                rareBonusEntityChancePercent: 50,
                rareBonusEntityMinCount: 1,
                rareBonusEntityMaxCount: 5),
            "crate/medium-toybox1" => RestoredCrateFamilyWithExactBonusRule(AgedDurabilityCost, "medium", RandomToyBox1Items, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 60),
            "crate/medium-toybox2" => RestoredCrateFamilyWithExactBonusRule(AgedDurabilityCost, "medium", RandomToyBox2Items, rareBonusEntityGroups: RandomCrateCreatureEntityGroups, rareBonusEntityChancePercent: 60),
            "crate/crate-large-empty" => LargeCrateRule(AgedDurabilityCost, "paper-empty"),
            "crate/crate-medium-empty" => RestoredCrateFamilyRule(AgedDurabilityCost, "medium"),
            "crate/crate-small-empty" => RestoredCrateFamilyRule(AgedDurabilityCost, "small"),
            "crate/crate-small-stacked" => RestoredCrateFamilyRule(AgedDurabilityCost, "small", 2),
            "crate/crate-large-cobweb" => LargeCrateRule(RuinedDurabilityCost, "paper-empty"),
            "crate/crate-large-evaporating" => LargeCrateRule(RuinedDurabilityCost, "paper-empty"),
            "crate/crate-small-evaporating" => RestoredCrateFamilyRule(RuinedDurabilityCost, "small"),
            _ when normalized.StartsWith("crate/crate-large-ruined", StringComparison.OrdinalIgnoreCase) => LargeCrateRule(RuinedDurabilityCost, "paper-empty"),
            _ when normalized.StartsWith("crate/crate-small-ruined", StringComparison.OrdinalIgnoreCase) => RestoredCrateFamilyRule(RuinedDurabilityCost, "small"),
            _ when normalized.StartsWith("bookshelves/bookstand-", StringComparison.OrdinalIgnoreCase) => VanillaBlockWithBonusItemsRule(
                normalized.Contains("evaporating", StringComparison.OrdinalIgnoreCase) || normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase)
                    ? RuinedDurabilityCost
                    : AgedDurabilityCost,
                "vstemporalreverser:restored-bookstand-{librarymaterial}",
                RandomNormalBookItems,
                1,
                1),
            _ when normalized.StartsWith("bookshelves/lectern-large-book-", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                DecorativeDurabilityCost(normalized),
                "vstemporalreverser:restored-lectern-largewood-{librarymaterial}",
                true),
            _ when normalized.StartsWith("bookshelves/lecturn-aged-", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                DecorativeDurabilityCost(normalized),
                "vstemporalreverser:restored-lectern-agedwood-{librarymaterial}",
                normalized.Contains("book", StringComparison.OrdinalIgnoreCase)),
            _ when string.Equals(normalized, "bookshelves/lecturn-ruined", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                RuinedDurabilityCost,
                "vstemporalreverser:restored-lectern-ruinedwood-{librarymaterial}",
                false),
            _ when normalized.StartsWith("bookshelves/lecturn-", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                DecorativeDurabilityCost(normalized),
                "vstemporalreverser:restored-lectern-ornatewood-{librarymaterial}",
                normalized.Contains("book", StringComparison.OrdinalIgnoreCase)),
            _ when string.Equals(normalized, "lecturn-ruined", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                RuinedDurabilityCost,
                "vstemporalreverser:restored-lectern-metal-{lecternmetal}",
                false),
            _ when normalized.StartsWith("lecturn-", StringComparison.OrdinalIgnoreCase) => LecternFamilyRule(
                DecorativeDurabilityCost(normalized),
                "vstemporalreverser:restored-lectern-metal-{lecternmetal}",
                normalized.Contains("book", StringComparison.OrdinalIgnoreCase)),
            "full" => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 4,
                bonusMaxCount: 12,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            "doublesidednew" => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 4,
                bonusMaxCount: 12,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            "doublesidedold" => VanillaAttributedBlockWithBonusItemsRule(
                RuinedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 2,
                bonusMaxCount: 8,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            "doublesidedoldempty" => VanillaAttributedBlockRule(
                RuinedDurabilityCost,
                "game:bookshelf",
                "type", "2row1col",
                "material", "{librarymaterial}"),
            "half" => VanillaAttributedBlockRule(
                AgedDurabilityCost,
                "game:bookshelf",
                "type", "2row1col",
                "material", "{librarymaterial}"),
            "half-front" => VanillaAttributedBlockRule(
                AgedDurabilityCost,
                "game:bookshelf",
                "type", "2row1col",
                "material", "{librarymaterial}"),
            _ when normalized.StartsWith("bookshelves/bookshelf-full", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 4,
                bonusMaxCount: 12,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            _ when normalized.StartsWith("bookshelves/bookshelf-standard", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 4,
                bonusMaxCount: 12,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            _ when normalized.StartsWith("bookshelves/bookshelf-ruined-full", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                RuinedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: 2,
                bonusMaxCount: 8,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            _ when normalized.StartsWith("bookshelves/bookshelf-", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:bookshelf",
                "type", "2row1col",
                "material", "{librarymaterial}"),
            _ when normalized.StartsWith("bookshelves/scrollrack-full", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                AgedDurabilityCost,
                "game:scrollrack",
                RandomScrollItems,
                bonusMinCount: 3,
                bonusMaxCount: 12,
                attributes: new[] { "type", "normal", "material", "{librarymaterial}" }),
            _ when normalized.StartsWith("bookshelves/scrollrack-", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:scrollrack",
                "type", "normal",
                "material", "{librarymaterial}"),
            _ when normalized.StartsWith("bookshelves/bookpile-aged", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(AgedDurabilityCost, RandomNormalBookItems, 3, 6),
            _ when normalized.StartsWith("bookshelves/bookpile", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(AgedDurabilityCost, RandomNormalBookItems, 3, 6),
            _ when normalized.StartsWith("bookshelves/bookstack", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(AgedDurabilityCost, RandomNormalBookItems, 3, 6),
            _ when normalized.StartsWith("bookshelves/large-book", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(
                normalized.Contains("evaporating", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                RandomNormalBookItems,
                3,
                6),
            _ when normalized.StartsWith("bookshelves/cartography-book-open", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(
                normalized.Contains("evaporating", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                RandomNormalBookItems,
                3,
                6),
            _ when normalized.StartsWith("bookshelves/", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                normalized.Contains("scrollrack", StringComparison.OrdinalIgnoreCase) ? "game:scrollrack" : "game:bookshelf",
                "type", normalized.Contains("scrollrack", StringComparison.OrdinalIgnoreCase) ? "normal" : "2row1col",
                "material", "{librarymaterial}"),
            _ when normalized.Contains("scrollrack", StringComparison.OrdinalIgnoreCase) && normalized.Contains("full", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:scrollrack",
                RandomScrollItems,
                bonusMinCount: 3,
                bonusMaxCount: 12,
                attributes: new[] { "type", "normal", "material", "{librarymaterial}" }),
            _ when normalized.Contains("scrollrack", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:scrollrack",
                "type", "normal",
                "material", "{librarymaterial}"),
            _ when normalized.Contains("bookshelf", StringComparison.OrdinalIgnoreCase) && normalized.Contains("full", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockWithBonusItemsRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:bookshelf",
                RandomNormalBookItems,
                bonusMinCount: normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? 2 : 4,
                bonusMaxCount: normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? 8 : 12,
                attributes: new[] { "type", "2row1col", "material", "{librarymaterial}" }),
            _ when normalized.Contains("bookshelf", StringComparison.OrdinalIgnoreCase) => VanillaAttributedBlockRule(
                normalized.Contains("ruined", StringComparison.OrdinalIgnoreCase) ? RuinedDurabilityCost : AgedDurabilityCost,
                "game:bookshelf",
                "type", "2row1col",
                "material", "{librarymaterial}"),
            _ when normalized.StartsWith("bookrow/bookrow", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(AgedDurabilityCost, RandomNormalBookItems, 3, 6),
            _ when normalized.StartsWith("book-big-", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemBundleRule(AgedDurabilityCost, RandomNormalBookItems, 3, 6),
            _ => null
        };
    }

    private static void SendNotification(EntityAgent byEntity, string message)
    {
        if (byEntity is not EntityPlayer entityPlayer || entityPlayer.Player is not IServerPlayer serverPlayer)
        {
            return;
        }

        serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, message, EnumChatType.Notification);
    }

    private void WriteRestoreDebugRecord(string sourceCode, RestorationRule rule, IReadOnlyList<string> spawnedEntries)
    {
        if (!VSTemporalReverserModSystem.Config.EnableDebugMode)
        {
            return;
        }

        try
        {
            EnsureDebugLogPath();
            if (DebugLogPath == null)
            {
                return;
            }

            var record = new Dictionary<string, object?>
            {
                ["timestampUtc"] = DateTime.UtcNow.ToString("O"),
                ["source"] = sourceCode,
                ["restoredTargetKind"] = rule.TargetKind.ToString(),
                ["restoredTarget"] = rule.Target,
                ["drops"] = spawnedEntries.ToArray()
            };

            string line = JsonSerializer.Serialize(record);
            lock (DebugLogLock)
            {
                File.AppendAllText(DebugLogPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            api?.Logger?.Warning($"[TemporalReverser] Failed to write debug log: {ex.Message}");
        }
    }

    private void WriteDebugEvent(string eventType, string sourceCode, RestorationRule? rule, IReadOnlyList<string>? spawnedEntries, string? detail)
    {
        if (!VSTemporalReverserModSystem.Config.EnableDebugMode)
        {
            return;
        }

        try
        {
            EnsureDebugLogPath();
            if (DebugLogPath == null)
            {
                return;
            }

            var record = new Dictionary<string, object?>
            {
                ["timestampUtc"] = DateTime.UtcNow.ToString("O"),
                ["event"] = eventType,
                ["source"] = sourceCode,
                ["restoredTargetKind"] = rule?.TargetKind.ToString(),
                ["restoredTarget"] = rule?.Target,
                ["drops"] = spawnedEntries?.ToArray(),
                ["detail"] = detail
            };

            string line = JsonSerializer.Serialize(record);
            lock (DebugLogLock)
            {
                File.AppendAllText(DebugLogPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            api?.Logger?.Warning($"[TemporalReverser] Failed to write debug log: {ex.Message}");
        }
    }

    private static RestorationRule ClutterRule(int durabilityCost, string clutterType)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.ClutterType, clutterType);
    }

    private static RestorationRule RestoredDecorationRule(int durabilityCost, string clutterType)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RestoredDecoration, clutterType);
    }

    private static RestorationRule RestoredCanopyBedRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-canopy-bed-{style}-{{wood}}-feet-north",
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 5,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 5);
    }

    private static RestorationRule RandomRestoredCanopyBedRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.RandomRestoredCanopyBed,
            string.Empty,
            styles,
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 5,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 5);
    }

    private static RestorationRule RestoredShortBedRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-short-bed-{style}-{{wood}}-feet-north",
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 5,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 5);
    }

    private static RestorationRule RestoredTableRule(int durabilityCost, string style)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-table-{style}-{{tablewood}}-north",
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 1,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 1);
    }

    private static RestorationRule RestoredMetalTableRule(int durabilityCost, string? fixedClothColor = null)
    {
        string clothColorToken = string.IsNullOrWhiteSpace(fixedClothColor) ? "{tableclothcolor}" : fixedClothColor;

        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-metal-table-{clothColorToken}-{{tablemetal}}-north",
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 1,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 1);
    }

    private static RestorationRule RandomRestoredCenserRule(int durabilityCost, string style, string[] finishes)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.RandomRestoredCenser,
            style,
            finishes);
    }

    private static RestorationRule RandomRestoredTableRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-table-{{tablestyle}}-{{tablewood}}-north",
            styles,
            RareBonusEntityGroups: new[] { RandomMothCreatureEntities },
            RareBonusEntityChancePercent: 50,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 1,
            SecondaryRareBonusEntityGroups: new[] { RandomMouseCreatureEntities },
            SecondaryRareBonusEntityChancePercent: 20,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 1);
    }

    private static RestorationRule RandomizedClutterRule(int durabilityCost, string[] clutterTypes, string textureKey, string[] textureOptions)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.RandomizedClutterType,
            string.Empty,
            clutterTypes,
            textureKey,
            textureOptions);
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

    private static RestorationRule VanillaBlockRuleWithCritters(
        int durabilityCost,
        string code,
        string[][]? rareBonusEntityGroups,
        int rareBonusEntityChancePercent,
        string[][]? secondaryRareBonusEntityGroups,
        int secondaryRareBonusEntityChancePercent)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            code,
            RareBonusEntityGroups: rareBonusEntityGroups,
            RareBonusEntityChancePercent: rareBonusEntityChancePercent,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 1,
            SecondaryRareBonusEntityGroups: secondaryRareBonusEntityGroups,
            SecondaryRareBonusEntityChancePercent: secondaryRareBonusEntityChancePercent,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 1);
    }

    private static RestorationRule VanillaBlockWithBonusItemsRule(
        int durabilityCost,
        string code,
        string[] bonusItemCodes,
        int bonusMinCount,
        int bonusMaxCount)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            code,
            null,
            null,
            null,
            BonusTargets: bonusItemCodes,
            BonusMinCount: bonusMinCount,
            BonusMaxCount: bonusMaxCount);
    }

    private static RestorationRule VanillaAttributedBlockRule(int durabilityCost, string code, params string[] attributes)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.VanillaAttributedBlock,
            code,
            attributes);
    }

    private static RestorationRule VanillaAttributedBlockWithBonusItemsRule(
        int durabilityCost,
        string code,
        string[] bonusItemCodes,
        int bonusMinCount,
        int bonusMaxCount,
        params string[] attributes)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.VanillaAttributedBlock,
            code,
            attributes,
            null,
            null,
            BonusTargets: bonusItemCodes,
            BonusMinCount: bonusMinCount,
            BonusMaxCount: bonusMaxCount);
    }

    private static RestorationRule VanillaBlockWithBonusAndRareItemsRule(
        int durabilityCost,
        string code,
        string[] bonusItemCodes,
        int bonusMinCount,
        int bonusMaxCount,
        int bonusItemMinCount = 1,
        int bonusItemMaxCount = 1,
        string[]? rareBonusItems = null,
        int rareBonusChancePercent = 0,
        string[]? rareBonusEntities = null,
        string[][]? rareBonusEntityGroups = null,
        int rareBonusEntityChancePercent = 0,
        int rareBonusEntityMinCount = 1,
        int rareBonusEntityMaxCount = 1,
        int rareBonusCount = 1,
        BonusLootStyle lootStyle = BonusLootStyle.None)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            code,
            null,
            null,
            null,
            BonusTargets: bonusItemCodes,
            BonusMinCount: bonusMinCount,
            BonusMaxCount: bonusMaxCount,
            BonusItemMinCount: bonusItemMinCount,
            BonusItemMaxCount: bonusItemMaxCount,
            RareBonusTargets: rareBonusItems,
            RareBonusChancePercent: rareBonusChancePercent,
            RareBonusEntityTargets: rareBonusEntities,
            RareBonusEntityGroups: rareBonusEntityGroups,
            RareBonusEntityChancePercent: rareBonusEntityChancePercent,
            RareBonusEntityMinCount: rareBonusEntityMinCount,
            RareBonusEntityMaxCount: rareBonusEntityMaxCount,
            RareBonusCount: rareBonusCount,
            LootStyle: lootStyle);
    }

    private static RestorationRule VanillaAttributedBlockWithExactBonusItemsRule(
        int durabilityCost,
        string code,
        string[] exactItemCodes,
        params string[] attributes)
    {
        int count = Math.Max(1, exactItemCodes.Length);
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.VanillaAttributedBlock,
            code,
            attributes,
            null,
            null,
            BonusTargets: exactItemCodes,
            BonusMinCount: count,
            BonusMaxCount: count,
            BonusItemMinCount: 1,
            BonusItemMaxCount: 1,
            LootStyle: BonusLootStyle.ExactListedItems);
    }

    private static RestorationRule RestoredCrateFamilyWithExactBonusRule(
        int durabilityCost,
        string size,
        string[] exactItemCodes,
        int primaryCount = 1,
        string[]? rareBonusEntities = null,
        string[][]? rareBonusEntityGroups = null,
        int rareBonusEntityChancePercent = 0,
        int rareBonusEntityMinCount = 1,
        int rareBonusEntityMaxCount = 1)
    {
        rareBonusEntityGroups ??= RandomCrateCreatureEntityGroups;
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.Block,
            $"vstemporalreverser:restored-crate-{size}-{{cratewood}}",
            BonusTargets: exactItemCodes,
            BonusMinCount: exactItemCodes.Length,
            BonusMaxCount: exactItemCodes.Length,
            BonusItemMinCount: 1,
            BonusItemMaxCount: 1,
            RareBonusEntityTargets: rareBonusEntities,
            RareBonusEntityGroups: rareBonusEntityGroups,
            RareBonusEntityChancePercent: rareBonusEntityChancePercent,
            RareBonusEntityMinCount: rareBonusEntityMinCount,
            RareBonusEntityMaxCount: rareBonusEntityMaxCount,
            PrimaryMinCount: primaryCount,
            PrimaryMaxCount: primaryCount,
            LootStyle: BonusLootStyle.ExactListedItems);
    }

    private static RestorationRule VanillaAttributedBlockWithBonusAndRareItemsRule(
        int durabilityCost,
        string code,
        string[] bonusItemCodes,
        int bonusMinCount,
        int bonusMaxCount,
        int bonusItemMinCount = 1,
            int bonusItemMaxCount = 1,
            string[]? rareBonusItems = null,
            int rareBonusChancePercent = 0,
            string[]? rareBonusEntities = null,
            string[][]? rareBonusEntityGroups = null,
            int rareBonusEntityChancePercent = 0,
            int rareBonusEntityMinCount = 1,
            int rareBonusEntityMaxCount = 1,
            int rareBonusCount = 1,
            BonusLootStyle lootStyle = BonusLootStyle.None,
            params string[] attributes)
    {
        rareBonusEntityGroups ??= RandomCrateCreatureEntityGroups;
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.VanillaAttributedBlock,
            code,
            attributes,
            null,
            null,
            BonusTargets: bonusItemCodes,
            BonusMinCount: bonusMinCount,
            BonusMaxCount: bonusMaxCount,
            BonusItemMinCount: bonusItemMinCount,
            BonusItemMaxCount: bonusItemMaxCount,
            RareBonusTargets: rareBonusItems,
            RareBonusChancePercent: rareBonusChancePercent,
            RareBonusEntityTargets: rareBonusEntities,
            RareBonusEntityGroups: rareBonusEntityGroups,
            RareBonusEntityChancePercent: rareBonusEntityChancePercent,
            RareBonusEntityMinCount: rareBonusEntityMinCount,
            RareBonusEntityMaxCount: rareBonusEntityMaxCount,
            RareBonusCount: rareBonusCount,
            LootStyle: lootStyle);
    }

    private static RestorationRule RandomRestoredShortBedRule(int durabilityCost, string[] styles)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomRestoredShortBed, string.Empty, styles);
    }

    private static RestorationRule RandomVanillaLanternRule(int durabilityCost)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomVanillaLantern, "game:lantern-large-up");
    }

    private static RestorationRule RandomVanillaItemRule(int durabilityCost, string[] itemCodes, int primaryMinCount = 1, int primaryMaxCount = 1)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.RandomVanillaItem,
            string.Empty,
            itemCodes,
            TextureKey: null,
            TextureOptions: null,
            BonusTargets: null,
            BonusMinCount: 0,
            BonusMaxCount: 0,
            BonusItemMinCount: 1,
            BonusItemMaxCount: 1,
            RareBonusTargets: null,
            RareBonusChancePercent: 0,
            RareBonusEntityTargets: null,
            RareBonusEntityGroups: null,
            RareBonusEntityChancePercent: 0,
            RareBonusEntityMinCount: 1,
            RareBonusEntityMaxCount: 1,
            SecondaryRareBonusEntityTargets: null,
            SecondaryRareBonusEntityGroups: null,
            SecondaryRareBonusEntityChancePercent: 0,
            SecondaryRareBonusEntityMinCount: 1,
            SecondaryRareBonusEntityMaxCount: 1,
            RareBonusCount: 1,
            PrimaryMinCount: primaryMinCount,
            PrimaryMaxCount: primaryMaxCount);
    }

    private static RestorationRule RandomVanillaItemBundleRule(int durabilityCost, string[] itemCodes, int minCount, int maxCount)
    {
        int clampedMin = Math.Max(1, minCount);
        int clampedMax = Math.Max(clampedMin, maxCount);

        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.RandomVanillaItem,
            string.Empty,
            itemCodes,
            null,
            null,
            BonusTargets: itemCodes,
            BonusMinCount: Math.Max(0, clampedMin - 1),
            BonusMaxCount: Math.Max(0, clampedMax - 1),
            BonusItemMinCount: 1,
            BonusItemMaxCount: 1,
            PrimaryMinCount: 1,
            PrimaryMaxCount: 1);
    }

    private static RestorationRule TieredJunkItemRule(int durabilityCost, int primaryMinCount = 1, int primaryMaxCount = 1, int bonusMinCount = 0, int bonusMaxCount = 0)
    {
        return new RestorationRule(
            durabilityCost,
            RestorationTargetKind.TieredJunkItem,
            string.Empty,
            BonusMinCount: bonusMinCount,
            BonusMaxCount: bonusMaxCount,
            PrimaryMinCount: primaryMinCount,
            PrimaryMaxCount: primaryMaxCount,
            LootStyle: BonusLootStyle.TieredJunk);
    }

    private static RestorationRule RandomVanillaTableRule(int durabilityCost)
    {
        return new RestorationRule(durabilityCost, RestorationTargetKind.RandomVanillaTable, "game:table-normal");
    }

    private enum RestorationTargetKind
    {
        Block,
        VanillaAttributedBlock,
        RestoredDecoration,
        RandomRestoredCenser,
        RandomRestoredCanopyBed,
        RandomRestoredShortBed,
        RandomVanillaLantern,
        RandomVanillaTable,
        RandomVanillaItem,
        TieredJunkItem,
        RandomizedClutterType,
        ClutterType
    }

    private enum BonusLootStyle
    {
        None,
        ExactListedItems,
        TieredJunk,
        TieredMetalJunk
    }

    private readonly record struct RestorationRule(
        int DurabilityCost,
        RestorationTargetKind TargetKind,
        string Target,
        string[]? Targets = null,
        string? TextureKey = null,
        string[]? TextureOptions = null,
        string[]? BonusTargets = null,
        int BonusMinCount = 0,
        int BonusMaxCount = 0,
        int BonusItemMinCount = 1,
        int BonusItemMaxCount = 1,
        string[]? RareBonusTargets = null,
        int RareBonusChancePercent = 0,
        string[]? RareBonusEntityTargets = null,
        string[][]? RareBonusEntityGroups = null,
        int RareBonusEntityChancePercent = 0,
        int RareBonusEntityMinCount = 1,
        int RareBonusEntityMaxCount = 1,
        string[]? SecondaryRareBonusEntityTargets = null,
        string[][]? SecondaryRareBonusEntityGroups = null,
        int SecondaryRareBonusEntityChancePercent = 0,
        int SecondaryRareBonusEntityMinCount = 1,
        int SecondaryRareBonusEntityMaxCount = 1,
        int RareBonusCount = 1,
        int PrimaryMinCount = 1,
        int PrimaryMaxCount = 1,
        BonusLootStyle LootStyle = BonusLootStyle.None);

    private static void EnsureDebugLogPath()
    {
        if (DebugLogPath != null)
        {
            return;
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string logDir = Path.Combine(appData, "VintagestoryData", "Logs", "VSTemporalReverser");
        Directory.CreateDirectory(logDir);
        DebugLogPath = Path.Combine(logDir, "restore-debug.jsonl");
    }

    private static string DescribeStackForRecord(ItemStack stack)
    {
        string code = stack.Collectible?.Code?.ToString() ?? "<unknown>";
        string? type = stack.Attributes?.GetString("type");
        return string.IsNullOrWhiteSpace(type) ? $"{code} x{stack.StackSize}" : $"{code} [type={type}] x{stack.StackSize}";
    }

    private static string DescribeEntityForRecord(string entityCode)
    {
        return $"{entityCode} x1";
    }

    private static bool IsRaccoonCreatureGroup(string[] entityCodes)
    {
        return entityCodes.Any(code => code.StartsWith("game:raccoon-common", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMouseCreatureGroup(string[] entityCodes)
    {
        return entityCodes.Any(code => code.Equals("vstemporalreverser:mouse", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMothCreatureGroup(string[] entityCodes)
    {
        return entityCodes.Any(code => code.StartsWith("game:butterfly-", StringComparison.OrdinalIgnoreCase));
    }

    private static string[][] FilterEnabledCreatureGroups(string[][] groups)
    {
        VSTemporalReverserConfig config = VSTemporalReverserModSystem.Config;
        List<string[]> enabledGroups = [];

        foreach (string[] group in groups)
        {
            if (IsRaccoonCreatureGroup(group) && !config.EnableRaccoons)
            {
                continue;
            }

            if (IsMouseCreatureGroup(group) && !config.EnableMice)
            {
                continue;
            }

            if (IsMothCreatureGroup(group) && !config.EnableMoths)
            {
                continue;
            }

            enabledGroups.Add(group);
        }

        return enabledGroups.Count > 0 ? [.. enabledGroups] : Array.Empty<string[]>();
    }

    private static bool TrySpawnRestoredEntity(IWorldAccessor world, Vec3d spawnPos, string entityCode)
    {
        EntityProperties? type = world.GetEntityType(ToAssetLocation(entityCode));
        if (type == null)
        {
            return false;
        }

        Entity? entity = world.ClassRegistry.CreateEntity(type);
        if (entity == null)
        {
            return false;
        }

        entity.Pos.SetPos(spawnPos.X, spawnPos.Y + 0.2, spawnPos.Z);
        world.SpawnEntity(entity);
        return true;
    }

    private static IEnumerable<string> CreateSupplementalRestoredEntities(RestorationRule rule)
    {
        foreach (string entityCode in CreateSupplementalRestoredEntities(
                     rule.RareBonusEntityTargets,
                     rule.RareBonusEntityGroups,
                     rule.RareBonusEntityChancePercent,
                     rule.RareBonusEntityMinCount,
                     rule.RareBonusEntityMaxCount))
        {
            yield return entityCode;
        }

        foreach (string entityCode in CreateSupplementalRestoredEntities(
                     rule.SecondaryRareBonusEntityTargets,
                     rule.SecondaryRareBonusEntityGroups,
                     rule.SecondaryRareBonusEntityChancePercent,
                     rule.SecondaryRareBonusEntityMinCount,
                     rule.SecondaryRareBonusEntityMaxCount))
        {
            yield return entityCode;
        }
    }

    private static IEnumerable<string> CreateSupplementalRestoredEntities(
        string[]? entityCodes,
        string[][]? entityGroups,
        int chancePercent,
        int minCount,
        int maxCount)
    {
        entityCodes ??= Array.Empty<string>();
        if (entityGroups != null && entityGroups.Length > 0)
        {
            entityGroups = FilterEnabledCreatureGroups(entityGroups);
        }
        if ((entityCodes.Length == 0 && (entityGroups == null || entityGroups.Length == 0)) || chancePercent <= 0)
        {
            yield break;
        }

        if (Random.Shared.Next(100) >= chancePercent)
        {
            yield break;
        }

        if (entityGroups != null && entityGroups.Length > 0)
        {
            entityCodes = entityGroups[Random.Shared.Next(entityGroups.Length)];
        }

        int clampedMin = Math.Max(1, minCount);
        int clampedMax = Math.Max(clampedMin, maxCount);
        int spawnCount = clampedMax > clampedMin ? Random.Shared.Next(clampedMin, clampedMax + 1) : clampedMin;

        for (int index = 0; index < spawnCount; index++)
        {
            yield return entityCodes[Random.Shared.Next(entityCodes.Length)];
        }
    }

    private static string[] BuildItemCodes(string prefix, string[] finishes)
    {
        return finishes.Select(finish => $"{prefix}{finish}").ToArray();
    }

    private static string[] BuildCategoryWeightedPool(params (string[]? pool, int weight)[] weightedPools)
    {
        List<(string[] pool, int weight)> usablePools = new();

        foreach ((string[]? pool, int weight) in weightedPools)
        {
            if (pool == null || pool.Length == 0 || weight <= 0)
            {
                continue;
            }

            usablePools.Add((pool, weight));
        }

        if (usablePools.Count == 0)
        {
            return [];
        }

        int commonMultiple = usablePools[0].pool.Length;
        for (int index = 1; index < usablePools.Count; index++)
        {
            commonMultiple = LeastCommonMultiple(commonMultiple, usablePools[index].pool.Length);
        }

        List<string> combined = new();

        foreach ((string[] pool, int weight) in usablePools)
        {
            int repeats = (commonMultiple / pool.Length) * weight;
            for (int repeat = 0; repeat < repeats; repeat++)
            {
                combined.AddRange(pool);
            }
        }

        return combined.ToArray();
    }

    private static int GreatestCommonDivisor(int left, int right)
    {
        left = Math.Abs(left);
        right = Math.Abs(right);

        while (right != 0)
        {
            (left, right) = (right, left % right);
        }

        return Math.Max(1, left);
    }

    private static int LeastCommonMultiple(int left, int right)
    {
        if (left == 0 || right == 0)
        {
            return 0;
        }

        return Math.Abs(left / GreatestCommonDivisor(left, right) * right);
    }

    private static void ApplyToyTextureAttributes(ItemStack stack, string[] enabledRestoredWoodTypes)
    {
        if (enabledRestoredWoodTypes.Length == 0)
        {
            return;
        }

        string ceramicTexture = RandomToyCeramicTextures[Random.Shared.Next(RandomToyCeramicTextures.Length)];
        string woodType = enabledRestoredWoodTypes[Random.Shared.Next(enabledRestoredWoodTypes.Length)];
        string variant = $"{ceramicTexture}-{woodType}";

        stack.Attributes.SetString("type", variant);
        stack.Attributes.SetString("ceramic", ceramicTexture);
        stack.Attributes.SetString("wood", woodType);
    }
}
