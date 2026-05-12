# Temporal Reverser Loot Table

Generated from the live restore rules in [VSTemporalReverser/ItemTemporalReverser.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/ItemTemporalReverser.cs).

This file is the root-level maintainer reference for:

- what clutter and ruined blocks the reverser supports
- what each source restores into
- which outputs are fixed vs randomized
- which restores also add bonus loot

For player-facing background and feature notes, see [README.md](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/README.md). For the earlier generated reference, see [docs/temporal-reverser-reference.md](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/docs/temporal-reverser-reference.md).

## Durability Rules

| Source state | Cost |
| --- | ---: |
| `aged`, `old`, ordinary decorative clutter | `1` |
| `ruined`, `evaporating`, `broken` | `2` |

## Notes On Random Tokens

Some restore IDs use shared tokens:

- `{wood}`: one enabled bed wood
- `{tablewood}`: one enabled table wood
- `{librarymaterial}`: one enabled bookshelf/lectern/chair wood or aged wood
- `{cratewood}`: one enabled crate wood
- `{lecternmetal}`: one censer-style metal finish
- `{bedtopmetal}`: one random metal bed-top finish
- `{tablemetal}`: one random metal table finish
- `{tableclothcolor}`: one random metal table cloth color
- `{chaircolor}`: one random chair cloth color

## Beds, Tables, Lighting, And Heat

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `fancy-bed-green` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidopen-{wood}-feet-north` | Can spawn moths and mice |
| `fancy-bed-green-drapes-opened` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidopened-{wood}-feet-north` | Can spawn moths and mice |
| `fancy-bed-green-drapes-closed` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidclosed-{wood}-feet-north` | Can spawn moths and mice |
| `fancy-bed-old` | 1 | random canopy bed, non-green open styles | Any matching restored canopy bed style |
| `fancy-bed-old-drapes-opened` | 1 | random canopy bed, non-green opened styles | Any matching restored canopy bed style |
| `fancy-bed-old-drapes-closed` | 1 | random canopy bed, non-green closed styles | Any matching restored canopy bed style |
| `fancy-bed-stitched-ruined` | 2 | random canopy bed | Any canopy bed style |
| `bed/bed-fancy-ruined1` to `bed/bed-fancy-ruined6` | 2 | random canopy bed | Any canopy bed style |
| `bed-short-green` | 1 | `vstemporalreverser:restored-short-bed-greenplaid-{wood}-feet-north` | Can spawn moths and mice |
| `bed-short-old` | 1 | random restored short bed | Non-green short-bed styles |
| `bed-short-stitched-ruined` | 2 | random restored short bed | Any short-bed style |
| `bed/bed-ruined1` to `bed/bed-ruined2` | 2 | random restored short bed | Any short-bed style |
| `bed/bed-ruined3` to `bed/bed-ruined6` | 2 | `game:bed-woodaged-head-north` | Vanilla aged wooden bed |
| `bed/bed-metal` | 1 | `vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}` | Low metal table form |
| `bed/bed-metal-ruined1` to `bed/bed-metal-ruined3` | 2 | `vstemporalreverser:restored-metal-table-low-{lecternmetal}-{bedtopmetal}` | Low metal table form |
| `bed/metal2`, `bed/metal2-mattress`, `bed/metal2-pillow` | 1 | `vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north` | Two-block high metal bed |
| `bed/metal2-ruined1` to `bed/metal2-ruined3` | 2 | `vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north` | Two-block high metal bed |
| `bed/metal1-evaporating`, `bed/metal2-evaporating` | 2 | `vstemporalreverser:restored-metal-bed-high-{lecternmetal}-{chaircolor}-head-north` | Two-block high metal bed |
| `table-aged` | 1 | `vstemporalreverser:restored-table-agedwhite-{tablewood}-north` | Can spawn moths and mice |
| `table-long` | 1 | `vstemporalreverser:restored-table-scribe-{tablewood}-north` | Can spawn moths and mice |
| `table-long-with-accessories` | 1 | `vstemporalreverser:restored-table-scribeaccessories-{tablewood}-north` | Can spawn moths and mice |
| `table-long-with-cloth-blue` | 1 | `vstemporalreverser:restored-table-scribeblue-{tablewood}-north` | Can spawn moths and mice |
| `table-long-with-cloth-green` | 1 | `vstemporalreverser:restored-table-scribegreen-{tablewood}-north` | Can spawn moths and mice |
| `table-long-with-cloth-purple` | 1 | `vstemporalreverser:restored-table-scribepurple-{tablewood}-north` | Can spawn moths and mice |
| `table-long-with-cloth-red` | 1 | `vstemporalreverser:restored-table-scribered-{tablewood}-north` | Can spawn moths and mice |
| `table/metal1` | 1 | `vstemporalreverser:restored-metal-table-{tableclothcolor}-{tablemetal}-north` | Cloth color is random |
| `table/metal1-cloth` | 1 | `vstemporalreverser:restored-metal-table-green-{tablemetal}-north` | Fixed green cloth |
| `table/metal1-ruined1` to `table/metal1-ruined3` | 2 | `vstemporalreverser:restored-metal-table-{tableclothcolor}-{tablemetal}-north` | Cloth color is random |
| `table-ruined1` to `table-ruined6` | 2 | random restored table family | Aged color table family |
| `brazier3`, `brazier4`, `brazier-evaporating` | 2 | `vstemporalreverser:restored-brazier-{material}` | Random metal |
| `lantern/ground1` to `lantern/ground6` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/wall1` to `lantern/wall3` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/ceiling1` to `lantern/ceiling2` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/ground7`, `lantern/ground8`, `lantern/wall5`, `lantern/ceiling3` | 2 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `chandelier-ruined1` to `chandelier-ruined3` | 2 | `vstemporalreverser:restored-chandelier-{material}-candle0` | Random metal |
| `bellowsagedcrude*` | 1 | `game:bellows-crude-north` | Clutter family |
| `bellowsagedsmall*` | 1 | `game:bellows-small-north` | Clutter family |
| `bellowsagedlarge*` | 1 | `game:bellows-large-north` | Clutter family |
| `bellows`, `bellows-north`, `bellows-east`, `bellows-south`, `bellows-west` | 2 | matching `game:bellows-{dir}` | Direct block restore |

## Functional Decor And Fixtures

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `torchholder-aged-empty-{north,east,south,west}` | 1 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Direct block restore |
| `torchholder-aged-filled-{north,east,south,west}` | 1 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Filled holder restores empty |
| `torchholder-ruined-empty-{north,east,south,west}` | 2 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Direct block restore |
| `torchholder-ruined-filled-{north,east,south,west}` | 2 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Filled holder restores empty |
| `anvil-broken1` | 2 | `game:anvil-copper` | Unstable downgrades higher anvils to copper where needed |
| `anvil-broken2` | 2 | `game:anvil-bismuthbronze` | Unstable clamps to copper at restore time |
| `anvil-broken3` | 2 | `game:anvil-iron` | Unstable clamps to copper at restore time |
| `censer/ceramic1*`, `censer/ceramic2*`, `censer/ceramic3*` | 1 or 2 | random `vstemporalreverser:restored-censer-{ceramic-style}-{finish}` | Finish from ceramic pool |
| `censer/metal1*`, `censer/metal2*`, `censer/metal3*`, `censer/metal4*` | 1 or 2 | random `vstemporalreverser:restored-censer-{metal-style}-{finish}` | Finish from metal pool |
| `censer/...-ceiling`, `censer/...-wall` | 1 or 2 | matching restored censer family | Ceiling and wall forms included |
| `shelf/ceramic1*`, `shelf/ceramic2*`, `shelf/ceramic3*` | 1 or 2 | random restored censer family | Normalized through censer logic |
| `shelf/metal1*`, `shelf/metal2*`, `shelf/metal3*`, `shelf/metal4*` | 1 or 2 | random restored censer family | Normalized through censer logic |
| clutter candle groups with recognized counts | 1 or 2 | `candle` x count | Count comes from clutter subtype |

## Chairs

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `chair-aged` | 1 | `vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}` | Can spawn moths and mice |
| `chair-ebony` | 1 | `vstemporalreverser:restored-chair-ebony` | Can spawn moths and mice |
| `chair-back` | 1 | `vstemporalreverser:restored-chair-back` | Can spawn moths and mice |
| `chair-crude` | 1 | `vstemporalreverser:restored-chair-crude` | Can spawn moths and mice |
| `chair-long` | 1 | `vstemporalreverser:restored-chair-long-{librarymaterial}` | Can spawn moths and mice |
| `chair-metal1`, `chair-metal1-pillow` | 1 | `vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}` | Can spawn moths and mice |
| `chair-metal1-ruined1` to `chair-metal1-ruined3` | 2 | `vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}` | Can spawn moths and mice |
| `chair-ruined*` | 2 | `vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}` | Can spawn moths and mice |

## Tools, Weapons, Trash, Toys, And Utility Clutter

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `tool-axe` | 2 | random restored axe item | See randomized pools below |
| `tool-hammer` | 2 | random restored hammer item | See randomized pools below |
| `tool-hoe` | 2 | random restored hoe item | See randomized pools below |
| `tool-knife` | 2 | random restored knife item | See randomized pools below |
| `tool-pickaxe` | 2 | random restored pickaxe item | See randomized pools below |
| `tool-saw` | 2 | random restored saw item | See randomized pools below |
| `tool-scythe` | 2 | random restored scythe item | See randomized pools below |
| `tool-shovel` | 2 | random restored shovel item | See randomized pools below |
| `tool-spear` | 2 | random restored spear item | Spear metals use a smaller pool |
| `pile-weapon1` to `pile-weapon8` | 2 | random restored weapon item | Axe, knife, spear, or falx |
| `pile-tools1` | 2 | random restored saw, hammer, or axe | Tool subset |
| `pile-tools2` | 2 | random restored shovel, tongs, or knife | Tool subset |
| `pile-tools3` | 2 | random restored hoe, scythe, or shears | Tool subset |
| `pile-tools4` | 2 | random restored pickaxe or prospecting pick | Tool subset |
| `pile-woodworkingtools` | 2 | random restored saw, axe, or hammer | Tool subset |
| `shelf-tools` | 2 | random restored tool item | Full tool pool |
| any clutter containing `precisiontools` | 2 | random restored precision tool item | Hammer, chisel, or wrench pool |
| any clutter containing `woodworkingtools` | 2 | random restored woodworking tool item | Saw, axe, or hammer pool |
| `pile-trash-pottery` and pottery/shard trash variants | 1 | 1 random pottery item | Includes broad shard/potsherd matching |
| `pile-trash-oldore` and oldore trash variants | 1 | 6 to 12 random ore nuggets | Ore pool is randomized |
| `pile-trash-scrap` and scrap trash variants | 1 | tiered junk item plus 1 to 3 bonus junk drops | Uses same junk logic family as junk crates |
| `toy1`, `toy2`, `toy3` | 1 | `vstemporalreverser:restored-toy-toy10` | Intentional shared mapping |
| `toy4` | 1 | `vstemporalreverser:restored-toy-toy4` | Fixed output |
| `toy5` | 1 | `vstemporalreverser:restored-toy-toy5` | Fixed output |
| `toy6` | 1 | `vstemporalreverser:restored-toy-toy6` | Fixed output |
| `toy7` | 1 | `vstemporalreverser:restored-toy-toy7` | Fixed output |
| `toy8` | 1 | `vstemporalreverser:restored-toy-toy8` | Fixed output |
| `toy9` | 1 | `vstemporalreverser:restored-toy-toy9` | Fixed output |
| `toy10` | 1 | `vstemporalreverser:restored-toy-toy10` | Fixed output |
| `toy11` | 1 | `vstemporalreverser:restored-toy-toy11` | Fixed output |
| `toy12` | 1 | `vstemporalreverser:restored-toy-toy12` | Fixed output |
| `toy13` | 1 | `vstemporalreverser:restored-toy-toy13` | Fixed output |
| `toy14` | 1 | `vstemporalreverser:restored-toy-toy14` | Fixed output |
| `toy15` | 1 | `vstemporalreverser:restored-toy-toy15` | Fixed output |
| `toy16` | 1 | `vstemporalreverser:restored-toy-toy16` | Fixed output |
| `shelf-toys1`, `shelf-toys2`, `shelf-toys3` | 1 | `game:bookshelf` with exact toy shelf contents | Exact bonus payload |

## Crates And Storage

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `crate-large-tools1` | 2 | `game:crate` with tool loot | Drops 2 restored tools |
| `crate/crate-medium-books` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Drops 2 to 4 books |
| `crate/crate-medium-pottery` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Drops 1 pottery item |
| `crate/crate-medium-pottery-alt` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Drops 1 pottery item |
| `crate/crate-small-pottery` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Drops 1 to 2 pottery items |
| `crate/crate-large-pottery` | 1 | `game:crate` | Drops 1 to 2 pottery items |
| `crate/large-pottery1` to `crate/large-pottery3` | 1 | `game:crate` | Drops 1 to 2 pottery items |
| `crate/crate-large-ore1` to `crate/crate-large-ore3` | 1 | `game:crate` | Drops 20 to 40 ore nuggets |
| `crate/crate-large-oldore` | 1 | `game:crate` | Drops 20 to 40 ore nuggets |
| clutter containing both `contamin` and `ore` | 1 | `game:crate` | Ore nuggets plus rare uranium or pentlandite |
| `crate/large-clothing1` | 1 | `game:crate` | Drops 2 to 4 clothing items, rare sturdy backpack |
| `crate/crate-large-junk` | 1 | `game:crate` | Tiered junk loot |
| `crate/crate-medium-junk` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Tiered junk loot |
| `crate/crate-small-junk` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Tiered junk loot |
| `crate/large-generic-junk1` | 1 | `game:crate` | Tiered junk loot |
| `crate/large-metaljunk1` | 1 | `game:crate` | Tiered metal junk loot |
| `crate/crate-small-rot` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Drops 2 to 4 rot-family items |
| `crate/crate-large-rot` | 1 | `game:crate` | Drops 4 to 6 rot-family items |
| `crate/medium-toybox1`, `crate/medium-toybox2` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Exact toybox contents |
| `crate/crate-large-empty` | 1 | `game:crate` | Empty labeled crate |
| `crate/crate-medium-empty` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Empty crate |
| `crate/crate-small-empty` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Empty crate |
| `crate/crate-small-stacked` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Drops 2 restored crate blocks |
| `crate/crate-large-cobweb`, `crate/crate-large-evaporating`, `crate/crate-large-ruined*` | 2 | `game:crate` | Empty labeled crate |
| `crate/crate-small-evaporating`, `crate/crate-small-ruined*` | 2 | `vstemporalreverser:restored-crate-small-{cratewood}` | Empty crate |
| `crate/crate-medium-evaporating`, `crate/crate-medium-ruined*` | 2 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Empty crate |

## Books, Bookcases, Lecterns, Bookstands, And Scrollracks

| Source ID or pattern | Cost | Restores to | Notes |
| --- | ---: | --- | --- |
| `bookshelves/bookstand-*` | 1 or 2 | `vstemporalreverser:restored-bookstand-{librarymaterial}` | Includes 1 random book |
| `bookshelves/lectern-large-book-*` | 1 or 2 | `vstemporalreverser:restored-lectern-largewood-{librarymaterial}` | Includes 1 random book |
| `bookshelves/lecturn-aged-*` | 1 or 2 | `vstemporalreverser:restored-lectern-agedwood-{librarymaterial}` | Includes a book if source contains `book` |
| `bookshelves/lecturn-ruined` | 2 | `vstemporalreverser:restored-lectern-ruinedwood-{librarymaterial}` | No bonus book |
| `bookshelves/lecturn-*` | 1 or 2 | `vstemporalreverser:restored-lectern-ornatewood-{librarymaterial}` | Includes a book if source contains `book` |
| `lecturn-ruined` | 2 | `vstemporalreverser:restored-lectern-metal-{lecternmetal}` | No bonus book |
| `lecturn-*` | 1 or 2 | `vstemporalreverser:restored-lectern-metal-{lecternmetal}` | Includes a book if source contains `book` |
| `full`, `doublesidednew`, `bookshelves/bookshelf-full*`, `bookshelves/bookshelf-standard*` | 1 | `game:bookshelf` | Drops 4 to 12 books |
| `doublesidedold`, `bookshelves/bookshelf-ruined-full*` | 2 | `game:bookshelf` | Drops 2 to 8 books |
| `doublesidedoldempty`, `half`, `half-front`, other `bookshelves/bookshelf-*` | 1 or 2 | `game:bookshelf` | Empty shelf restore |
| `bookshelves/scrollrack-full*` | 1 | `game:scrollrack` | Drops 3 to 12 scrolls |
| `bookshelves/scrollrack-*`, other clutter containing `scrollrack` | 1 or 2 | `game:scrollrack` | Empty rack unless marked full |
| `bookshelves/bookpile-aged*`, `bookshelves/bookpile*`, `bookshelves/bookstack*`, `bookshelves/large-book*`, `bookshelves/cartography-book-open*` | 1 or 2 | random normal book bundle | Returns 3 to 6 books total |

## Randomized Output Pools

### Tool Pools

- Restored axe pool: `axe-felling-{metal}`
- Restored hammer pool: `hammer-{metal}`
- Restored hoe pool: `hoe-{metal}`
- Restored knife pool: `knife-generic-{metal}`
- Restored pickaxe pool: `pickaxe-{metal}`
- Restored saw pool: `saw-{metal}`
- Restored scythe pool: `scythe-{metal}`
- Restored shovel pool: `shovel-{metal}`
- Restored spear pool: `spear-generic-{metal}` using the smaller spear metal set
- Precision tools: hammers, chisels, and wrenches
- Weapon pile pool: restored axes, knives, spears, and `blade-falx-{metal}`

### Pottery Pool

Random pottery restores pull from fired pottery families including bowls, clay planters, clay pots, crocks, crucibles, flowerpots, storage vessels, jugs, and watering cans in the common color families.

### Ore Nugget Pool

Random ore nugget restores pull from:

- `nugget-bismuthinite`
- `nugget-cassiterite`
- `nugget-chromite`
- `nugget-galena`
- `nugget-hematite`
- `nugget-ilmenite`
- `nugget-limonite`
- `nugget-magnetite`
- `nugget-malachite`
- `nugget-nativecopper`
- `nugget-nativegold`
- `nugget-nativesilver`
- `nugget-pentlandite`
- `nugget-rhodochrosite`
- `nugget-sphalerite`
- `nugget-uranium`
- `nugget-wolframite`

### Clothing Pool

Clothing crate restores pull from:

- `cloth-black`
- `cloth-blue`
- `cloth-brown`
- `cloth-gray`
- `cloth-green`
- `cloth-orange`
- `cloth-pink`
- `cloth-plain`
- `cloth-purple`
- `cloth-red`
- `cloth-white`
- `cloth-yellow`
- `linen-normal-down`
- `linen-offset-down`
- `linen-diamond-down`
- `linen-square-down`

### Rot Pool

Rot-family restores pull from:

- `leather-normal-plain`
- common crop seed items including amaranth, bell pepper, cabbage, carrot, cassava, fennel, flax, onion, parsnip, peanut, pumpkin, rice, rye, soybean, spelt, sunflower, and turnip

### Tiered Junk Pool

- Common: paintings, arrows, bombs, seeds, fabric, nails, resin, leather
- Uncommon: copper and bronze ingots/plates, windmill parts, helve hammer parts, lower-tier armor
- Rare: iron and meteoric iron ingots/plates, iron and meteoric iron armor
- Ultra-rare: `backpack-sturdy`, plus gold, silver, and steel armor

### Tiered Metal Junk Pool

- Common: nails, 4 at a time, from a random metal; and random metal bits, 10 at a time
- Uncommon: copper and bronze ingots and plates
- Rare: iron, meteoric iron, silver, and gold ingots and plates, plus iron and meteoric iron armor
- Ultra-rare: silver, gold, and steel armor and plates

## Bonus Temporal Gears

Supported furniture and crate restores can also roll bonus temporal gears when bonus drops are enabled on the reverser tier.

- Current chance: `5%`
- Current stack size: `1` to `5`
- Unstable uses the same restore targets, but its bonus-drop and metal restrictions still apply where configured
