# VSTemporalReverser Test Checklist

Use this as the in-game validation list before calling the mod done.

Mark each item as you verify it. For randomized restores, the goal is to confirm:

- the target is recognized
- the reverser consumes the correct durability
- the source block/clutter is removed
- the restored block or item drops correctly
- bonus items and critters only appear when expected
- unstable/stabilized/perfected tier restrictions behave correctly

## Global Device Tests

- [ ] `Unstable Temporal Reverser` shows in handbook and crafts correctly
- [ ] `Stabilized Temporal Reverser` shows in handbook and crafts correctly
- [ ] `Temporal Reverser` perfected version shows in handbook and crafts correctly
- [ ] `Reverser Casing` shows in handbook and crafts correctly
- [ ] `Depleted Unstable Temporal Reverser` exists and looks visually depleted
- [ ] `Depleted Stabilized Temporal Reverser` exists and looks visually depleted
- [ ] `Depleted Temporal Reverser` exists and looks visually depleted
- [ ] Unstable hotbar icon is framed correctly and spins around the right point
- [ ] Stabilized hotbar icon is framed correctly and spins around the right point
- [ ] Perfected hotbar icon is framed correctly and spins around the right point
- [ ] Reverser Casing icon is framed correctly and uses the device-style icon setup
- [ ] Temporal gears stack to `64` while the mod is loaded

## Tier Behavior

- [ ] Unstable restores valid targets
- [ ] Stabilized restores valid targets
- [ ] Perfected restores valid targets
- [ ] Only perfected shows `Restore` / `Salvage` mode switching
- [ ] Stabilized does not show salvage mode
- [ ] Unstable does not show salvage mode
- [ ] Rust ward only functions on perfected
- [ ] Tooltip text matches actual behavior for all three tiers

## Depletion And Recharge

- [ ] Unstable depletes into its depleted item instead of disappearing
- [ ] Stabilized depletes into its depleted item instead of disappearing
- [ ] Perfected depletes into its depleted item instead of disappearing
- [ ] Unstable recharge recipe works
- [ ] Stabilized recharge recipe works
- [ ] Perfected recharge recipe works
- [ ] Recharged item preserves the correct tier and no wrong item is returned

## Invalid Target Behavior

- [ ] Invalid clutter gives a sensible failure message
- [ ] Invalid block gives a sensible failure message
- [ ] Cooldown message appears when trying to spam-use the device
- [ ] Failed restores do not consume durability incorrectly
- [ ] Debug mode logs successful restores
- [ ] Debug mode logs failed restores

## Salvage Mode

- [ ] Perfected can toggle into salvage mode
- [ ] Salvage mode consumes double durability
- [ ] Salvage mode gives salvage outputs on supported salvage targets
- [ ] Salvage mode gives the correct failure message on unsupported targets

## Beds

- [ ] `fancy-bed-green`
- [ ] `fancy-bed-green-drapes-opened`
- [ ] `fancy-bed-green-drapes-closed`
- [ ] `fancy-bed-old`
- [ ] `fancy-bed-old-drapes-opened`
- [ ] `fancy-bed-old-drapes-closed`
- [ ] `fancy-bed-stitched-ruined`
- [ ] `bed/bed-fancy-ruined1`
- [ ] `bed/bed-fancy-ruined2`
- [ ] `bed/bed-fancy-ruined3`
- [ ] `bed/bed-fancy-ruined4`
- [ ] `bed/bed-fancy-ruined5`
- [ ] `bed/bed-fancy-ruined6`
- [ ] `bed-short-green`
- [ ] `bed-short-old`
- [ ] `bed-short-stitched-ruined`
- [ ] `bed/bed-ruined1`
- [ ] `bed/bed-ruined2`
- [ ] `bed/bed-ruined3`
- [ ] `bed/bed-ruined4`
- [ ] `bed/bed-ruined5`
- [ ] `bed/bed-ruined6`
- [ ] `bed/bed-metal`
- [ ] `bed/bed-metal-ruined1`
- [ ] `bed/bed-metal-ruined2`
- [ ] `bed/bed-metal-ruined3`
- [ ] `bed/metal2`
- [ ] `bed/metal2-mattress`
- [ ] `bed/metal2-pillow`
- [ ] `bed/metal2-ruined1`
- [ ] `bed/metal2-ruined2`
- [ ] `bed/metal2-ruined3`
- [ ] `bed/metal1-evaporating`
- [ ] `bed/metal2-evaporating`
- [ ] Bed restores can spawn moths when expected
- [ ] Bed restores can spawn mice when expected

## Tables

- [ ] `table-aged`
- [ ] `table-long`
- [ ] `table-long-with-accessories`
- [ ] `table-long-with-cloth-blue`
- [ ] `table-long-with-cloth-green`
- [ ] `table-long-with-cloth-purple`
- [ ] `table-long-with-cloth-red`
- [ ] `table/metal1`
- [ ] `table/metal1-cloth`
- [ ] `table/metal1-ruined1`
- [ ] `table/metal1-ruined2`
- [ ] `table/metal1-ruined3`
- [ ] `table-ruined1`
- [ ] `table-ruined2`
- [ ] `table-ruined3`
- [ ] `table-ruined4`
- [ ] `table-ruined5`
- [ ] `table-ruined6`
- [ ] Table restores can spawn moths when expected
- [ ] Table restores can spawn mice when expected

## Braziers, Lanterns, Chandeliers, Bellows

- [ ] `brazier3`
- [ ] `brazier4`
- [ ] `brazier-evaporating`
- [ ] `lantern/ground1`
- [ ] `lantern/ground2`
- [ ] `lantern/ground3`
- [ ] `lantern/ground4`
- [ ] `lantern/ground5`
- [ ] `lantern/ground6`
- [ ] `lantern/ground7`
- [ ] `lantern/ground8`
- [ ] `lantern/wall1`
- [ ] `lantern/wall2`
- [ ] `lantern/wall3`
- [ ] `lantern/wall5`
- [ ] `lantern/ceiling1`
- [ ] `lantern/ceiling2`
- [ ] `lantern/ceiling3`
- [ ] `chandelier-ruined1`
- [ ] `chandelier-ruined2`
- [ ] `chandelier-ruined3`
- [ ] `bellowsagedcrude*`
- [ ] `bellowsagedsmall*`
- [ ] `bellowsagedlarge*`
- [ ] `bellows`
- [ ] `bellows-north`
- [ ] `bellows-east`
- [ ] `bellows-south`
- [ ] `bellows-west`

## Torchholders, Anvils, Censers, Shelves

- [ ] `candlestub-single`
- [ ] `candlestubs-bunch1`
- [ ] `candlestubs-bunch2`
- [ ] `candlestubs-bunch3`
- [ ] `candlestubs-bunch4`
- [ ] `torchholder-aged-empty-{north,east,south,west}`
- [ ] `torchholder-aged-filled-{north,east,south,west}`
- [ ] `torchholder-ruined-empty-{north,east,south,west}`
- [ ] `torchholder-ruined-filled-{north,east,south,west}`
- [ ] `anvil-broken1`
- [ ] `anvil-broken2`
- [ ] `anvil-broken3`
- [ ] `censer/ceramic1*`
- [ ] `censer/ceramic2*`
- [ ] `censer/ceramic3*`
- [ ] `censer/metal1*`
- [ ] `censer/metal2*`
- [ ] `censer/metal3*`
- [ ] `censer/metal4*`
- [ ] `censer/...-ceiling`
- [ ] `censer/...-wall`
- [ ] `shelf/ceramic1*`
- [ ] `shelf/ceramic2*`
- [ ] `shelf/ceramic3*`
- [ ] `shelf/metal1*`
- [ ] `shelf/metal2*`
- [ ] `shelf/metal3*`
- [ ] `shelf/metal4*`

## Chairs

- [ ] `chair-back`
- [ ] `chair-crude`
- [ ] `chair-ebony`
- [ ] `chair-normal`
- [ ] `chair-ruined*`
- [ ] `chair-long-acacia`
- [ ] `chair-long-aged`
- [ ] `chair-long-baldcypress`
- [ ] `chair-long-birch`
- [ ] `chair-long-ebony`
- [ ] `chair-long-kapok`
- [ ] `chair-long-larch`
- [ ] `chair-long-maple`
- [ ] `chair-long-oak`
- [ ] `chair-long-pine`
- [ ] `chair-long-purpleheart`
- [ ] `chair-long-redwood`
- [ ] `chair-long-veryaged`
- [ ] `chair-long-walnut`
- [ ] `chair-metal1-pillow`

## Toys And Toy Shelves

- [ ] `toy1`
- [ ] `toy2`
- [ ] `toy3`
- [ ] `toy4`
- [ ] `toy5`
- [ ] `toy6`
- [ ] `toy7`
- [ ] `toy8`
- [ ] `toy9`
- [ ] `toy10`
- [ ] `toy11`
- [ ] `toy12`
- [ ] `toy13`
- [ ] `toy14`
- [ ] `toy15`
- [ ] `toy16`
- [ ] `shelf-toys1`
- [ ] `shelf-toys2`
- [ ] `shelf-toys3`
- [ ] Restored toys use the correct model and textures
- [ ] Restored toy icon item swaps correctly in GUI

## Trash Piles

- [ ] `pile-trash-pottery`
- [ ] pottery shard / potsherd trash variants
- [ ] `pile-trash-oldore`
- [ ] oldore trash variants
- [ ] `pile-trash-scrap`
- [ ] scrap trash variants

## Crates And Storage

- [ ] `crate-large-tools1`
- [ ] `crate/crate-medium-books`
- [ ] `crate/crate-medium-pottery`
- [ ] `crate/crate-medium-pottery-alt`
- [ ] `crate/crate-small-pottery`
- [ ] `crate/crate-large-pottery`
- [ ] `crate/large-pottery1`
- [ ] `crate/large-pottery2`
- [ ] `crate/large-pottery3`
- [ ] `crate/crate-large-ore1`
- [ ] `crate/crate-large-ore2`
- [ ] `crate/crate-large-ore3`
- [ ] `crate/crate-large-oldore`
- [ ] contaminated ore crate variants
- [ ] `crate/large-clothing1`
- [ ] `crate/crate-large-junk`
- [ ] `crate/crate-medium-junk`
- [ ] `crate/crate-small-junk`
- [ ] `crate/large-generic-junk1`
- [ ] `crate/large-metaljunk1`
- [ ] `crate/crate-small-rot`
- [ ] `crate/crate-large-rot`
- [ ] `crate/medium-toybox1`
- [ ] `crate/medium-toybox2`
- [ ] `crate/crate-large-empty`
- [ ] `crate/crate-medium-empty`
- [ ] `crate/crate-small-empty`
- [ ] `crate/crate-small-stacked`
- [ ] `crate/crate-large-cobweb`
- [ ] `crate/crate-large-evaporating`
- [ ] ruined large crate variants
- [ ] `crate/crate-small-evaporating`
- [ ] ruined small crate variants
- [ ] `crate/crate-medium-evaporating`
- [ ] ruined medium crate variants

## Books, Bookshelves, Scrollracks, Bookstands, Lecterns

- [ ] `bookshelves/bookstand-*`
- [ ] `bookshelves/lectern-large-book-*`
- [ ] `bookshelves/lecturn-aged-*`
- [ ] `bookshelves/lecturn-ruined`
- [ ] `bookshelves/lecturn-*`
- [ ] `lecturn-ruined`
- [ ] `lecturn-*`
- [ ] `full`
- [ ] `doublesidednew`
- [ ] `bookshelves/bookshelf-full*`
- [ ] `bookshelves/bookshelf-standard*`
- [ ] `doublesidedold`
- [ ] `bookshelves/bookshelf-ruined-full*`
- [ ] `doublesidedoldempty`
- [ ] `half`
- [ ] `half-front`
- [ ] other `bookshelves/bookshelf-*` empties
- [ ] `bookshelves/scrollrack-full*`
- [ ] `bookshelves/scrollrack-*`
- [ ] other clutter containing `scrollrack`
- [ ] `bookshelves/bookpile-aged*`
- [ ] `bookshelves/bookpile*`
- [ ] `bookshelves/bookstack*`
- [ ] `bookshelves/large-book*`
- [ ] `bookshelves/cartography-book-open*`

## Tools, Weapons, Ore, Pottery, Clothing

- [ ] clutter containing `axe`
- [ ] clutter containing `hammer`
- [ ] clutter containing `hoe`
- [ ] clutter containing `knife`
- [ ] clutter containing `pickaxe`
- [ ] clutter containing `saw`
- [ ] clutter containing `scythe`
- [ ] clutter containing `shovel`
- [ ] clutter containing `spear`
- [ ] `tool-axe`
- [ ] `tool-hammer`
- [ ] `tool-hoe`
- [ ] `tool-knife`
- [ ] `tool-pickaxe`
- [ ] `tool-saw`
- [ ] `tool-scythe`
- [ ] `tool-shovel`
- [ ] `tool-spear`
- [ ] `pile-weapon1`
- [ ] `pile-weapon2`
- [ ] `pile-weapon3`
- [ ] `pile-weapon4`
- [ ] `pile-weapon5`
- [ ] `pile-weapon6`
- [ ] `pile-weapon7`
- [ ] `pile-weapon8`
- [ ] `pile-tools1`
- [ ] `pile-tools2`
- [ ] `pile-tools3`
- [ ] `pile-tools4`
- [ ] `pile-woodworkingtools`
- [ ] `shelf-tools`
- [ ] clutter containing `precisiontools`
- [ ] clutter containing `woodworkingtools`
- [ ] clutter containing weapon pile variants
- [ ] random pottery restores from pottery sources
- [ ] clothing crate restores
- [ ] ore nugget restores from ore sources

## Bonus Loot Validation

- [ ] Large tool crate drops `2` restored tools
- [ ] Medium books crate drops `2-4` books
- [ ] Medium pottery crate drops `1` pottery item
- [ ] Small pottery crate drops `1-2` pottery items
- [ ] Large pottery crate drops `1-2` pottery items
- [ ] Large ore crates drop `20-40` nuggets
- [ ] Contaminated ore crates can rare-roll uranium or pentlandite
- [ ] Large clothing crate drops `2-4` clothing items
- [ ] Large clothing crate can rare-roll `backpack-sturdy`
- [ ] Junk crate families use the intended tiered junk pool
- [ ] Metal junk crate uses the intended tiered metal junk pool
- [ ] Small rot crate drops `2-4` rot items
- [ ] Large rot crate drops `4-6` rot items
- [ ] Toybox crates return exact toybox contents
- [ ] Trash scrap uses the same junk-family logic as junk crates
- [ ] Bonus temporal gear drop chance feels active at the new `5%` rate

## Unstable Restriction Tests

- [ ] Unstable can get bonus items
- [ ] Unstable never restores metal weapon/tool results above copper from randomized pools
- [ ] Unstable never gets armor pieces from junk or metal-junk bonus loot
- [ ] Unstable never gets machine parts from junk or metal-junk bonus loot
- [ ] Unstable still allows `backpack-sturdy`
- [ ] Unstable clamps broken anvils to copper
- [ ] Unstable trash scrap stays within restricted bonus rules
- [ ] Unstable crate bonus loot stays within restricted bonus rules

## Critter Spawn Tests

- [ ] Bed critter spawns
- [ ] Table critter spawns
- [ ] Medium books crate critter spawns
- [ ] Large clothing crate critter spawns
- [ ] Junk crate critter spawns
- [ ] Rot crate critter spawns
- [ ] Toybox crate critter spawns
- [ ] Critter config toggles disable them correctly

## Texture And Asset Regression Tests

- [ ] Restored objects render correctly after the texture-domain migration
- [ ] Bookstands and lecterns still render correctly
- [ ] Bookshelf piles, stacks, and rows still render correctly
- [ ] Chandelier candles still render correctly
- [ ] Torchholders still render correctly
- [ ] Censers still render correctly
- [ ] Reverser and depleted reverser items still render correctly
- [ ] No restored object is missing pink/black fallback textures

## Exhaustive Pattern Families

- [ ] `bookshelves/bookstand-aged`
- [ ] `bookshelves/bookstand-ruined`
- [ ] `bookshelves/bookstand-evaporating`
- [ ] `bookshelves/lectern-large-book-aged`
- [ ] `bookshelves/lectern-large-book-ruined`
- [ ] `bookshelves/lecturn-aged`
- [ ] `bookshelves/lecturn-aged-book`
- [ ] `bookshelves/lecturn-ruined`
- [ ] `bookshelves/lecturn`
- [ ] `bookshelves/lecturn-book`
- [ ] `lecturn-aged`
- [ ] `lecturn-book`
- [ ] `lecturn-ruined`
- [ ] `full`
- [ ] `doublesidednew`
- [ ] `doublesidedold`
- [ ] `doublesidedoldempty`
- [ ] `half`
- [ ] `half-front`
- [ ] `bookshelves/bookshelf-full`
- [ ] `bookshelves/bookshelf-standard`
- [ ] `bookshelves/bookshelf-ruined-full`
- [ ] `bookshelves/bookshelf-empty`
- [ ] `bookshelves/scrollrack-full`
- [ ] `bookshelves/scrollrack-empty`
- [ ] generic clutter containing `scrollrack`
- [ ] `bookshelves/bookpile-aged1`
- [ ] `bookshelves/bookpile-aged2`
- [ ] `bookshelves/bookpile-aged2-evaporating`
- [ ] `bookshelves/bookpile-aged3`
- [ ] `bookshelves/bookpile-aged4`
- [ ] `bookshelves/bookpile-aged5`
- [ ] `bookshelves/bookpile-aged5-evaporating`
- [ ] `bookshelves/bookpile1`
- [ ] `bookshelves/bookpile2`
- [ ] `bookshelves/bookpile3`
- [ ] `bookshelves/bookpile4`
- [ ] `bookshelves/bookpile5`
- [ ] `bookshelves/bookstack1`
- [ ] `bookshelves/bookstack2`
- [ ] `bookshelves/bookstack3`
- [ ] `bookshelves/bookstack4`
- [ ] `bookshelves/large-book-closed`
- [ ] `bookshelves/large-book-closed-evaporating`
- [ ] `bookshelves/large-book-open`
- [ ] `bookshelves/large-book-open-evaporating`
- [ ] `bookshelves/large-book-pile1`
- [ ] `bookshelves/large-book-pile1-evaporating`
- [ ] `bookshelves/large-book-pile2`
- [ ] `bookshelves/large-book-pile3`
- [ ] `bookshelves/large-book-standing1`
- [ ] `bookshelves/large-book-standing2`
- [ ] `bookshelves/large-book-standing3`
- [ ] `bookshelves/large-book-standing4`
- [ ] `bookshelves/large-book-standing5`
- [ ] `bookshelves/large-book-standing6`
- [ ] `bookshelves/cartography-book-open`
- [ ] `bookshelves/cartography-book-open-evaporating`
- [ ] `bookrow/bookrow1`
- [ ] `bookrow/bookrow2`
- [ ] `bookrow/bookrow3`
- [ ] `bookrow/bookrow4`
- [ ] `bookrow/bookrow5`
- [ ] `bookrow/bookrow6`
- [ ] `bookrow/bookrow7`
- [ ] `bookrow/bookrow8`
- [ ] `bookrow/bookrow9`
- [ ] `bookrow/bookrow10`
- [ ] `bookrow/bookrow11`
- [ ] `bookrow/bookrow12`
- [ ] `bookrow/bookrow13`
- [ ] `bookrow/bookrow14`
- [ ] `bookrow/bookrow15`
- [ ] `book-big-closed`
- [ ] `book-big-open`
- [ ] `book-big-open-trader`
- [ ] `crate/crate-large-ruined1`
- [ ] `crate/crate-large-ruined2`
- [ ] `crate/crate-large-ruined3`
- [ ] `crate/crate-small-ruined1`
- [ ] `crate/crate-small-ruined2`
- [ ] `crate/crate-small-ruined3`
- [ ] `crate/crate-medium-ruined1`
- [ ] `crate/crate-medium-ruined2`
- [ ] `crate/crate-medium-ruined3`
- [ ] any clutter containing both `contamin` and `ore`
- [ ] any clutter containing `precisiontools`
- [ ] any clutter containing `woodworkingtools`
- [ ] any clutter containing pottery shard naming variants

## Final Sanity Checks

- [ ] Save/reload with restored objects already placed
- [ ] MP sanity test if applicable
- [ ] Handbook entries match actual recipes
- [ ] README still matches actual gameplay
- [ ] `loot-table.md` still matches actual gameplay
