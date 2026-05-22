# Temporal Reverser Reference

This guide is generated from the live restoration rules in `VSTemporalReverser/ItemTemporalReverser.cs`.

Use it for two things:

- player-facing guidance on what the reverser can restore
- a maintainer checklist for spotting missing ruined or aged variants

## How To Read This

The reverser uses a small number of shared durability rules:

| Source state | Durability cost |
| --- | ---: |
| `aged`, `old`, normal decorative clutter | `1` |
| `ruined`, `evaporating`, `broken` | `2` |

Some restores are fixed IDs. Others are randomized families and use template tokens in their output IDs:

- `{wood}`: one enabled restored bed wood
- `{tablewood}`: one enabled restored table wood
- `{librarymaterial}`: one enabled library wood or aged wood
- `{cratewood}`: one enabled crate wood
- `{lecternmetal}`: one random censer-style metal finish
- `{bedtopmetal}`: one random metal bed top finish
- `{tablemetal}`: one random metal table finish
- `{tableclothcolor}`: one random metal table cloth color
- `{chaircolor}`: one random chair cloth color

## Rust Ward

The reverser also works against rust creatures, but only when:

- the reverser is in the main hand
- a real light source is in the offhand

It only affects these creature families:

| Target family | Notes |
| --- | --- |
| `drifter*` | Rust ward target |
| `shiver*` | Rust ward target |
| `bowtorn*` | Rust ward target |

Everything else is ignored by the ward.

## Reverser Tiers

| Tier | Item ID | Durability | Held light | Rust ward | Bonus restore loot | Metal restriction |
| --- | --- | ---: | --- | --- | --- | --- |
| Unstable | `vstemporalreverser:temporal-reverser-unstable` | `20` | `lightHsv [32, 5, 9]` | No | No | Copper-only for randomized metal outputs |
| Stabilized | `vstemporalreverser:temporal-reverser-stabilized` | `50` | `lightHsv [32, 5, 19]` | No | Yes | Full metal pool |
| Perfected | `vstemporalreverser:temporal-reverser` | `100` | `lightHsv [32, 5, 31]` | Yes | Yes | Full metal pool, plus `Restore` and `Salvage` modes |

Tier notes:

- `Unstable` uses a copper housing and cannot push back or damage rust creatures.
- `Stabilized` uses an iron housing and keeps the safer non-ward behavior.
- `Perfected` uses the cupronickel housing and is the only tier that can use the rust ward when paired with an offhand light source.
- `Unstable` still restores the same target families, but when the restored result is chosen from a random metal pool it is restricted to copper, and it cannot pull bonus loot or bonus temporal gears out of restored furniture or crates.
- `Perfected` can switch tool mode with `F`. `Restore` works as before. `Salvage` breaks a supported target down into materials and costs double the normal durability.

## Salvage Mode

`Salvage` is currently available on the perfected reverser only.

General salvage patterns:

- book furniture such as bookshelves, bookstands, lecterns, and scrollracks drop planks, nails, and parchment when they clearly contain books or scrolls
- wooden crates, tables, and chairs drop planks and nails
- beds drop planks plus random cloth, and metal bed variants can also return an ingot
- tool and weapon clutter drops a random ingot and a stick
- lantern clutter drops `glass-plain` and a random metal ingot

Salvage costs `2x` the durability of the normal restore cost for the same source object.

## Beds, Tables, Lighting, And Heat

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
| --- | ---: | --- | --- |
| `fancy-bed-green` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidopen-{wood}-feet-north` | Can spawn moths and mice |
| `fancy-bed-stitched-ruined` | 2 | random `vstemporalreverser:restored-canopy-bed-{style}-{wood}-head/feet` | Any canopy bed style |
| `bed/bed-fancy-ruined1` to `bed/bed-fancy-ruined6` | 2 | random `vstemporalreverser:restored-canopy-bed-{style}-{wood}-head/feet` | Any canopy bed style |
| `fancy-bed-old` | 1 | random `vstemporalreverser:restored-canopy-bed-{style}-{wood}-head/feet` | Non-green open canopy styles |
| `fancy-bed-old-drapes-opened` | 1 | random `vstemporalreverser:restored-canopy-bed-{style}-{wood}-head/feet` | Non-green opened canopy styles |
| `fancy-bed-old-drapes-closed` | 1 | random `vstemporalreverser:restored-canopy-bed-{style}-{wood}-head/feet` | Non-green closed canopy styles |
| `fancy-bed-green-drapes-opened` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidopened-{wood}-feet-north` | Can spawn moths and mice |
| `fancy-bed-green-drapes-closed` | 1 | `vstemporalreverser:restored-canopy-bed-greenplaidclosed-{wood}-feet-north` | Can spawn moths and mice |
| `bed-short-green` | 1 | `vstemporalreverser:restored-short-bed-greenplaid-{wood}-feet-north` | Can spawn moths and mice |
| `bed-short-old` | 1 | random `vstemporalreverser:restored-short-bed-{style}-{wood}-head/feet` | Non-green short bed styles |
| `bed-short-stitched-ruined` | 2 | random `vstemporalreverser:restored-short-bed-{style}-{wood}-head/feet` | Any short bed style |
| `bed/bed-ruined1` to `bed/bed-ruined2` | 2 | random `vstemporalreverser:restored-short-bed-{style}-{wood}-head/feet` | Any short bed style |
| `bed/bed-ruined3` to `bed/bed-ruined6` | 2 | `game:bed-woodaged-head-north` | Vanilla aged wooden bed |
| `bed/bed-metal` | 1 | `vstemporalreverser:restored-metal-table-{lecternmetal}-{bedtopmetal}` | Restores the metal table form |
| `bed/bed-metal-ruined1` to `bed/bed-metal-ruined3` | 2 | `vstemporalreverser:restored-metal-table-{lecternmetal}-{bedtopmetal}` | Restores the metal table form |
| `bed/metal2` | 1 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
| `bed/metal2-mattress` | 1 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
| `bed/metal2-pillow` | 1 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
| `bed/metal2-ruined1` to `bed/metal2-ruined3` | 2 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
| `bed/metal1-evaporating` | 2 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
| `bed/metal2-evaporating` | 2 | `vstemporalreverser:restored-metal-bed-{lecternmetal}-{chaircolor}-head-north` | Two-block metal bed |
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
| `table-ruined1` to `table-ruined6` | 2 | random `vstemporalreverser:restored-table-{tablestyle}-{tablewood}-north` | Aged color table family |
| `brazier3`, `brazier4`, `brazier-evaporating` | 2 | `vstemporalreverser:restored-brazier-{material}` | Random metal |
| `lantern/ground1` to `lantern/ground6` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/wall1` to `lantern/wall3` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/ceiling1` to `lantern/ceiling2` | 1 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/ground7`, `lantern/ground8` | 2 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/wall5` | 2 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `lantern/ceiling3` | 2 | randomized `game:lantern-large-up` | Random metal, lining, quartz glass |
| `chandelier-ruined1` to `chandelier-ruined3` | 2 | `vstemporalreverser:restored-chandelier-{material}-candle0` | Random metal |
| `bellowsagedcrude*` | 1 | `game:bellows-crude-north` | Clutter family |
| `bellowsagedsmall*` | 1 | `game:bellows-small-north` | Clutter family |
| `bellowsagedlarge*` | 1 | `game:bellows-large-north` | Clutter family |
| `bellows`, `bellows-north`, `bellows-east`, `bellows-south`, `bellows-west` | 2 | matching `game:bellows-{dir}` | Direct block restore |

## Functional Decor And Fixtures

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
| --- | ---: | --- | --- |
| `torchholder-aged-empty-{north,east,south,west}` | 1 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Direct block restore |
| `torchholder-aged-filled-{north,east,south,west}` | 1 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Filled holder restores empty |
| `torchholder-ruined-empty-{north,east,south,west}` | 2 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Direct block restore |
| `torchholder-ruined-filled-{north,east,south,west}` | 2 | `vstemporalreverser:torchholder-{material}-empty-{dir}` | Filled holder restores empty |
| `anvil-broken1` | 2 | `game:anvil-copper` | Clutter family |
| `anvil-broken2` | 2 | `game:anvil-bismuthbronze` | Clutter family |
| `anvil-broken3` | 2 | `game:anvil-iron` | Clutter family |
| `censer/ceramic1*`, `censer/ceramic2*`, `censer/ceramic3*` | 1 or 2 | random `vstemporalreverser:restored-censer-{ceramic-style}-{finish}` | Finish from ceramic pool |
| `censer/metal1*`, `censer/metal2*`, `censer/metal3*`, `censer/metal4*` | 1 or 2 | random `vstemporalreverser:restored-censer-{metal-style}-{finish}` | Finish from metal pool |
| `censer/...-ceiling`, `censer/...-wall` | 1 or 2 | random matching restored censer family | Ceiling and wall forms included |
| `shelf/ceramic1*`, `shelf/ceramic2*`, `shelf/ceramic3*` | 1 or 2 | random `vstemporalreverser:restored-censer-{ceramic-style}-{finish}` | Currently normalized through censer logic |
| `shelf/metal1*`, `shelf/metal2*`, `shelf/metal3*`, `shelf/metal4*` | 1 or 2 | random `vstemporalreverser:restored-censer-{metal-style}-{finish}` | Currently normalized through censer logic |

## Chairs

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
| --- | ---: | --- | --- |
| `chair-aged` | 1 | `vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}` | Can spawn moths and mice |
| `chair-ebony` | 1 | `vstemporalreverser:restored-chair-ebony` | Can spawn moths and mice |
| `chair-back` | 1 | `vstemporalreverser:restored-chair-back` | Can spawn moths and mice |
| `chair-crude` | 1 | `vstemporalreverser:restored-chair-crude` | Can spawn moths and mice |
| `chair-long` | 1 | `vstemporalreverser:restored-chair-long-{librarymaterial}` | Can spawn moths and mice |
| `chair-metal1`, `chair-metal1-pillow` | 1 | `vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}` | Can spawn moths and mice |
| `chair-metal1-ruined1` to `chair-metal1-ruined3` | 2 | `vstemporalreverser:restored-chair-metal-{lecternmetal}-{chaircolor}` | Can spawn moths and mice |
| `chair-ruined*` | 2 | `vstemporalreverser:restored-chair-colored-{chaircolor}-{librarymaterial}` | Can spawn moths and mice |

## Tools, Weapons, Toys, And Utility Clutter

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
| --- | ---: | --- | --- |
| `tool-axe` | 2 | random restored axe item | Metal picked from supported tool pool |
| `tool-hammer` | 2 | random restored hammer item | Metal picked from supported tool pool |
| `tool-hoe` | 2 | random restored hoe item | Metal picked from supported tool pool |
| `tool-knife` | 2 | random restored knife item | Metal picked from supported tool pool |
| `tool-pickaxe` | 2 | random restored pickaxe item | Metal picked from supported tool pool |
| `tool-saw` | 2 | random restored saw item | Metal picked from supported tool pool |
| `tool-scythe` | 2 | random restored scythe item | Metal picked from supported tool pool |
| `tool-shovel` | 2 | random restored shovel item | Metal picked from supported tool pool |
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
| `shelf-toys1` | 1 | `game:bookshelf` with toy shelf contents | Exact toy shelf payload |
| `shelf-toys2` | 1 | `game:bookshelf` with toy shelf contents | Exact toy shelf payload |
| `shelf-toys3` | 1 | `game:bookshelf` with toy shelf contents | Exact toy shelf payload |
| `crate/large-metaljunk1`, `large-metaljunk1` | 1 | `game:crate` with metal junk loot | Special-case junk clutter helper |

## Crates And Storage

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
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
| any clutter containing both `contamin` and `ore` | 1 | `game:crate` | Drops ore nuggets plus rare uranium or pentlandite |
| `crate/large-clothing1` | 1 | `game:crate` | Drops 2 to 4 clothing items, rare sturdy backpack |
| `crate/crate-large-junk` | 1 | `game:crate` | Tiered junk loot |
| `crate/crate-medium-junk` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Tiered junk loot |
| `crate/crate-small-junk` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Tiered junk loot |
| `crate/large-generic-junk1` | 1 | `game:crate` | Tiered junk loot |
| `crate/large-metaljunk1` | 1 | `game:crate` | Tiered metal junk loot |
| `crate/crate-small-rot` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Drops 2 to 4 rot-family items |
| `crate/crate-large-rot` | 1 | `game:crate` | Drops 4 to 6 rot-family items |
| `crate/medium-toybox1` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Exact toybox contents |
| `crate/medium-toybox2` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Exact toybox contents |
| `crate/crate-large-empty` | 1 | `game:crate` | Empty labeled crate |
| `crate/crate-medium-empty` | 1 | `vstemporalreverser:restored-crate-medium-{cratewood}` | Empty crate |
| `crate/crate-small-empty` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Empty crate |
| `crate/crate-small-stacked` | 1 | `vstemporalreverser:restored-crate-small-{cratewood}` | Drops 2 restored crate blocks |
| `crate/crate-large-cobweb` | 2 | `game:crate` | Empty labeled crate |
| `crate/crate-large-evaporating` | 2 | `game:crate` | Empty labeled crate |
| `crate/crate-small-evaporating` | 2 | `vstemporalreverser:restored-crate-small-{cratewood}` | Empty crate |
| `crate/crate-large-ruined*` | 2 | `game:crate` | Empty labeled crate |
| `crate/crate-small-ruined*` | 2 | `vstemporalreverser:restored-crate-small-{cratewood}` | Empty crate |

## Books, Bookcases, Lecterns, And Scrollracks

| Source ID or pattern | Cost | Restored result ID or pool | Notes |
| --- | ---: | --- | --- |
| `bookshelves/bookstand-*` | 1 or 2 | `vstemporalreverser:restored-bookstand-{librarymaterial}` | Includes 1 random book |
| `bookshelves/lectern-large-book-*` | 1 or 2 | `vstemporalreverser:restored-lectern-largewood-{librarymaterial}` | Includes 1 random book |
| `bookshelves/lecturn-aged-*` | 1 or 2 | `vstemporalreverser:restored-lectern-agedwood-{librarymaterial}` | Includes a book if source name contains `book` |
| `bookshelves/lecturn-ruined` | 2 | `vstemporalreverser:restored-lectern-ruinedwood-{librarymaterial}` | No bonus book |
| `bookshelves/lecturn-*` | 1 or 2 | `vstemporalreverser:restored-lectern-ornatewood-{librarymaterial}` | Includes a book if source name contains `book` |
| `lecturn-ruined` | 2 | `vstemporalreverser:restored-lectern-metal-{lecternmetal}` | No bonus book |
| `lecturn-*` | 1 or 2 | `vstemporalreverser:restored-lectern-metal-{lecternmetal}` | Includes a book if source name contains `book` |
| `full` | 1 | `game:bookshelf` | Drops 4 to 12 books |
| `doublesidednew` | 1 | `game:bookshelf` | Drops 4 to 12 books |
| `doublesidedold` | 2 | `game:bookshelf` | Drops 2 to 8 books |
| `doublesidedoldempty` | 2 | `game:bookshelf` | Empty shelf |
| `half`, `half-front` | 1 | `game:bookshelf` | Empty shelf |
| `bookshelves/bookshelf-full*` | 1 | `game:bookshelf` | Drops 4 to 12 books |
| `bookshelves/bookshelf-standard*` | 1 | `game:bookshelf` | Drops 4 to 12 books |
| `bookshelves/bookshelf-ruined-full*` | 2 | `game:bookshelf` | Drops 2 to 8 books |
| `bookshelves/bookshelf-*` | 1 or 2 | `game:bookshelf` | Empty shelf restore |
| `bookshelves/scrollrack-full*` | 1 | `game:scrollrack` | Drops 3 to 12 scrolls |
| `bookshelves/scrollrack-*` | 1 or 2 | `game:scrollrack` | Empty scroll rack restore |
| `bookshelves/bookpile-aged*` | 1 | random normal book bundle | Returns 3 to 6 books total |
| `bookshelves/bookpile*` | 1 | random normal book bundle | Returns 3 to 6 books total |
| `bookshelves/bookstack*` | 1 | random normal book bundle | Returns 3 to 6 books total |
| `bookshelves/large-book*` | 1 or 2 | random normal book bundle | Returns 3 to 6 books total |
| `bookshelves/cartography-book-open*` | 1 or 2 | random normal book bundle | Returns 3 to 6 books total |
| other `bookshelves/*` clutter | 1 or 2 | `game:bookshelf` or `game:scrollrack` | Chosen by whether the source name contains `scrollrack` |
| any other clutter containing `scrollrack` and `full` | 1 or 2 | `game:scrollrack` | Drops 3 to 12 scrolls |
| any other clutter containing `scrollrack` | 1 or 2 | `game:scrollrack` | Empty scroll rack restore |
| any other clutter containing `bookshelf` and `full` | 1 or 2 | `game:bookshelf` | Drops 2 to 12 books depending on ruined state |
| any other clutter containing `bookshelf` | 1 or 2 | `game:bookshelf` | Empty bookshelf restore |
| `bookrow/bookrow*` | 1 | random normal book bundle | Returns 3 to 6 books total |
| `book-big-*` | 1 | random normal book bundle | Returns 3 to 6 books total |

## Notes On Random Pools

These restore groups do not have a single fixed output ID:

| Group | Output pool |
| --- | --- |
| random canopy beds | restored canopy bed styles across morningstar, blue plaid, green plaid, red plaid, and honeycomb variants |
| random short beds | restored short bed styles across morningstar, blue plaid, green plaid, red plaid, and honeycomb variants |
| random aged ruined tables | restored aged table color styles `agedwhite`, `agedblue`, `agedgreen`, `agedpurple`, `agedred` |
| random lanterns | `game:lantern-large-up` with randomized material and lining attributes |
| random tools | axes, hammers, hoes, knives, pickaxes, saws, scythes, shovels, spears, chisels, wrenches, prospecting picks, shears, and tongs |
| random weapons | axes, knives, spears, and falx blades |
| random books | normal book color pool |
| random scrolls | normal scroll pool |
| random censer finishes | ceramic and metal finish pools |

## Unsupported Clutter Feedback

If a target is recognized as clutter but has no restore rule, the player sees:

`This object no longer remembers its former state.`
