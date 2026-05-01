using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    private const int AgedDurabilityCost = 1;
    private const int RuinedDurabilityCost = 2;
    private static string? DebugLogPath;
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
    private static readonly string[] RandomRestoredCenserMetalFinishes =
    [
        "copper",
        "brass",
        "blackbronze",
        "silver",
        "gold",
        "electrum"
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
    private static readonly string[] RandomRestoredAxeItems = BuildItemCodes("axe-felling-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredHammerItems = BuildItemCodes("hammer-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredHoeItems = BuildItemCodes("hoe-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredKnifeItems = BuildItemCodes("knife-generic-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredPickaxeItems = BuildItemCodes("pickaxe-", RandomRestoredCommonMetals);
    private static readonly string[] RandomRestoredSawItems = BuildItemCodes("saw-", RandomRestoredCommonMetals);
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
        ["bed/bed-metal"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/bed-metal-ruined1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/bed-metal-ruined2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/bed-metal-ruined3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-mattress"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-pillow"] = VanillaBlockRule(AgedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-ruined1"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-ruined2"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-ruined3"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal1-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
        ["bed/metal2-evaporating"] = VanillaBlockRule(RuinedDurabilityCost, "vstemporalreverser:restored-bed-metal-{lecternmetal}-{chaircolor}"),
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
        ["bellows"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-north"),
        ["bellows-north"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-north"),
        ["bellows-east"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-east"),
        ["bellows-south"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-south"),
        ["bellows-west"] = VanillaBlockRule(RuinedDurabilityCost, "game:bellows-west"),
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
        dsc.AppendLine("Aged targets cost 1 durability. Ruined targets cost 2 durability.");
        dsc.AppendLine("Current targets include beds, tables, braziers, censers, lanterns, chandeliers, bellows, torch holders, toys, and some tools/weapons.");
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
        string? clutterType = null;
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
        List<string> spawnedEntries = [DescribeStackForRecord(restoredStack)];

        world.BlockAccessor.SetBlock(0, pos);
        world.SpawnItemEntity(restoredStack, dropPos);
        foreach (ItemStack extraStack in CreateSupplementalRestoredStacks(world, rule))
        {
            spawnedEntries.Add(DescribeStackForRecord(extraStack));
            world.SpawnItemEntity(extraStack, dropPos);
        }
        foreach (string entityCode in CreateSupplementalRestoredEntities(rule))
        {
            if (TrySpawnRestoredEntity(world, dropPos, entityCode))
            {
                spawnedEntries.Add(DescribeEntityForRecord(entityCode));
            }
        }
        DamageItem(world, byEntity, slot, rule.DurabilityCost);

        WriteRestoreDebugRecord(
            clutterType ?? block.Code.Path,
            rule,
            spawnedEntries);

        world.PlaySoundAt(new AssetLocation("game", "sounds/effect/translocate"), pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5);
        SendNotification(byEntity, "The restored item drops free in a usable shape.");

        handling = EnumHandHandling.PreventDefault;
    }

    private static ItemStack? CreateRestoredStack(IWorldAccessor world, RestorationRule rule)
    {
        string[] enabledRestoredWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredWoodTypes);
        string[] enabledRestoredTableWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredTableWoodTypes);
        string[] enabledLibraryMaterials = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomRestoredLibraryMaterials);
        string[] enabledCrateWoodTypes = VSTemporalReverserModSystem.GetEnabledWoodTypes(RandomCrateWoodTypes);

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
            string[] finishes = rule.Targets ?? Array.Empty<string>();
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
            string[] itemCodes = rule.Targets ?? Array.Empty<string>();
            if (itemCodes.Length == 0)
            {
                return null;
            }

            string itemCode = itemCodes[Random.Shared.Next(itemCodes.Length)];
            Item? item = world.GetItem(ToAssetLocation(itemCode));
            if (item == null)
            {
                return null;
            }

            int minCount = Math.Max(1, rule.PrimaryMinCount);
            int maxCount = Math.Max(minCount, rule.PrimaryMaxCount);
            int stackSize = maxCount > minCount ? Random.Shared.Next(minCount, maxCount + 1) : minCount;

            ItemStack itemStack = new(item, stackSize);
            itemStack.ResolveBlockOrItem(world);
            return itemStack;
        }

        if (rule.TargetKind == RestorationTargetKind.Block)
        {
            string wood = enabledRestoredWoodTypes[Random.Shared.Next(enabledRestoredWoodTypes.Length)];
            string tableWood = enabledRestoredTableWoodTypes[Random.Shared.Next(enabledRestoredTableWoodTypes.Length)];
            string tableStyle = rule.Targets != null && rule.Targets.Length > 0
                ? rule.Targets[Random.Shared.Next(rule.Targets.Length)]
                : string.Empty;
            string material = RandomLanternMaterials[Random.Shared.Next(RandomLanternMaterials.Length)];
            string lecternMetalFinish = RandomRestoredCenserMetalFinishes[Random.Shared.Next(RandomRestoredCenserMetalFinishes.Length)];
            string torchholderMaterial = RandomTorchholderMaterials[Random.Shared.Next(RandomTorchholderMaterials.Length)];
            string libraryMaterial = enabledLibraryMaterials[Random.Shared.Next(enabledLibraryMaterials.Length)];
            string crateWood = enabledCrateWoodTypes[Random.Shared.Next(enabledCrateWoodTypes.Length)];
            string chairColor = RandomVanillaChairColors[Random.Shared.Next(RandomVanillaChairColors.Length)];
            string targetCode = rule.Target
                .Replace("{wood}", wood, StringComparison.Ordinal)
                .Replace("{tablestyle}", tableStyle, StringComparison.Ordinal)
                .Replace("{tablewood}", tableWood, StringComparison.Ordinal)
                .Replace("{material}", material, StringComparison.Ordinal)
                .Replace("{lecternmetal}", lecternMetalFinish, StringComparison.Ordinal)
                .Replace("{librarymaterial}", libraryMaterial, StringComparison.Ordinal)
                .Replace("{cratewood}", crateWood, StringComparison.Ordinal)
                .Replace("{chaircolor}", chairColor, StringComparison.Ordinal);
            targetCode = targetCode.Replace("{torchholdermaterial}", torchholderMaterial, StringComparison.Ordinal);
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

    private static IEnumerable<ItemStack> CreateSupplementalRestoredStacks(IWorldAccessor world, RestorationRule rule)
    {
        if (rule.LootStyle == BonusLootStyle.TieredJunk)
        {
            foreach (ItemStack itemStack in CreateTieredLootStacks(world, rule, PickTieredJunkItemCode))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (rule.LootStyle == BonusLootStyle.TieredMetalJunk)
        {
            foreach (ItemStack itemStack in CreateTieredLootStacks(world, rule, PickWeightedMetalJunkItemCode))
            {
                yield return itemStack;
            }

            yield break;
        }

        if (rule.LootStyle == BonusLootStyle.ExactListedItems)
        {
            string[] exactItemCodes = rule.BonusTargets ?? Array.Empty<string>();
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

        string[] itemCodes = rule.BonusTargets ?? Array.Empty<string>();
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

        string[] rareItemCodes = rule.RareBonusTargets ?? Array.Empty<string>();
        if (rareItemCodes.Length > 0 && rule.RareBonusChancePercent > 0 && Random.Shared.Next(100) < rule.RareBonusChancePercent)
        {
            string rareItemCode = rareItemCodes[Random.Shared.Next(rareItemCodes.Length)];
            AssetLocation rareCode = ToAssetLocation(rareItemCode);
            Item? rareItem = world.GetItem(rareCode);
            if (rareItem != null)
            {
                yield return new ItemStack(rareItem, Math.Max(1, rule.RareBonusCount));
                yield break;
            }

            Block? rareBlock = world.GetBlock(rareCode);
            if (rareBlock != null)
            {
                yield return new ItemStack(rareBlock, Math.Max(1, rule.RareBonusCount));
            }
        }
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

    private static string PickTieredJunkItemCode()
    {
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

    private static string PickWeightedMetalJunkItemCode()
    {
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
            "shelf-tools" => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredToolItems),
            _ when normalized.Contains("precisiontools", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredPrecisionToolItems),
            _ when normalized.Contains("woodworkingtools", StringComparison.OrdinalIgnoreCase) => RandomVanillaItemRule(RuinedDurabilityCost, RandomRestoredWoodworkingToolItems),
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
                ? VanillaAttributedBlockWithBonusAndRareItemsRule(durabilityCost, "game:crate", bonusItems, minCount, maxCount, bonusItemMinCount, bonusItemMaxCount, rareBonusItems, rareBonusChancePercent, rareBonusEntities, rareBonusEntityGroups, rareBonusEntityChancePercent, rareBonusEntityMinCount, rareBonusEntityMaxCount, lootStyle, "type", "wood-{cratewood}", "lidState", "closed")
                : VanillaAttributedBlockWithBonusAndRareItemsRule(durabilityCost, "game:crate", bonusItems, minCount, maxCount, bonusItemMinCount, bonusItemMaxCount, rareBonusItems, rareBonusChancePercent, rareBonusEntities, rareBonusEntityGroups, rareBonusEntityChancePercent, rareBonusEntityMinCount, rareBonusEntityMaxCount, lootStyle, "type", "wood-{cratewood}", "lidState", "closed", "label", label);
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
            "crate/large-clothing1" => LargeCrateWithBonusRule(AgedDurabilityCost, RandomClothingItems, 2, 4, "paper-decoration", 1, 1, RandomRareClothingItems, 1, rareBonusEntityGroups: new[] { RandomMothCreatureEntities }, rareBonusEntityChancePercent: 40),
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
        if (!VSTemporalReverserModSystem.Config.EnableDebugLogging)
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
            $"vstemporalreverser:restored-canopy-bed-{style}-{{wood}}-head-north",
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
            $"vstemporalreverser:restored-short-bed-{style}-{{wood}}-head-north",
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
            if (IsRaccoonCreatureGroup(group) && !config.EnableRaccoonCritterSpawns)
            {
                continue;
            }

            if (IsMouseCreatureGroup(group) && !config.EnableMouseCritterSpawns)
            {
                continue;
            }

            if (IsMothCreatureGroup(group) && !config.EnableMothCritterSpawns)
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
