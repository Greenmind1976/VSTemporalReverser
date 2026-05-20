# VSTemporalReverser

Vintage Story mod focused on temporal restoration, repair, and salvage.

The mod currently includes:

- three handheld `Temporal Reverser` tiers for restoring selected ruined or aged vanilla clutter into usable forms
- a `Temporal Reconstruction Device` for repairing supported worn gear
- a `Temporal Deconstructor Device` for reclaiming materials from selected crafted items and armor

For a full player-facing reference of handheld reverser targets, durability costs, rust ward targets, and restored output IDs, see [docs/temporal-reverser-reference.md](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/docs/temporal-reverser-reference.md).

## Current Restoration Rules

- Aged or old targets cost `1` durability.
- Ruined targets cost `2` durability.
- Restorations currently produce a mix of vanilla usable blocks and modded restored blocks depending on the furniture family.

## Crafting And Recharge

- `Unstable Temporal Reverser` can be crafted from `2` clear quartz, `1` copper plate, and `1` temporal gear.
- `Stabilized Temporal Reverser` can be crafted from `1` plain glass block, `1` iron plate, and `5` temporal gears.
- `Perfected Temporal Reverser` can be crafted from a `Reverser Casing`, `1` temporal alignment node, `1` flux gap connector, `1` finely balanced oscillator, and `5` temporal gears.
- When a reverser runs out of durability, it becomes a depleted version instead of being destroyed.
- Depleted reversers can be recharged in the crafting grid using the depleted device plus their original temporal gear cost.
- The mod raises vanilla `temporal gears` to a stack size of `64` while it is loaded.

## Temporal Machines

Both placed machines consume `temporal gears` as fuel and now provide live status feedback in their dialogs.

### Temporal Reconstruction Device

- Repairs supported damaged gear over time.
- Displays clear status text for unsupported items, missing fuel, items that do not need repair, and active reconstruction.
- Resumes more cleanly after reloads than the earlier prototype builds.

### Temporal Deconstructor Device

- Processes queued input items over time and stores reclaimed materials in `12` internal output slots.
- Uses `8` input slots, `1` fuel slot, and `12` output slots.
- Takes `20` seconds per deconstruction cycle.
- Stops cleanly when outputs do not have room instead of ejecting reclaimed items into the world.
- Shows dedicated status text for unsupported items, missing fuel, active processing, queue handoff, and blocked output capacity.

### Deconstructor Support Rules

The deconstructor is intentionally curated rather than supporting every reversible vanilla craft.

Currently supported categories include:

- selected vanilla grid-crafted furnishings, fixtures, storage, and mechanical assemblies
- armor, including chain, scale, plate, brigandine, and metal lamellar
- explicit non-grid exceptions such as anvils

The deconstructor does not attempt to support every basic conversion recipe such as planks, firewood, clay, ingots, bandages, or similar low-value crafts.

### Armor Salvage Notes

- Worn armor is supported.
- Repair recipes are filtered out so armor does not refund only its repair ingredients.
- Layered armor refunds are flattened so scale and plate do not kick back intermediate armor pieces.
- Metal armor salvage is normalized to `ingot-*` and `metalbit-*` outputs instead of chain, scale, plate, or lamella sub-components.

## Current Supported Families

- Canopy beds
- Short beds
- Wooden beds
- Tables
- Braziers
- Censers
- Lanterns
- Chandeliers
- Torch holders
- Book stands
- Lecterns

The mod has moved past the original canopy-bed-only spike. It now covers a small curated set of ruined or aged furnishings while still using the same basic interaction flow: match a target, consume durability, remove the original block, and drop the restored result.

## Bonus Loot Table

These restored targets currently drop extra loot in addition to the restored block:

| Restored target | Bonus loot |
|---|---|
| `crate-large-tools1` | `2` random tools |
| `crate/large-clothing1` | `2-4` clothing items, plus a `1%` `backpack-sturdy` roll |
| `crate/crate-large-pottery`, `crate/large-pottery1`, `crate/large-pottery2`, `crate/large-pottery3` | `1-2` fired pottery pieces |
| `crate/crate-medium-books` | `2-4` normal books |
| `crate/crate-medium-pottery`, `crate/crate-medium-pottery-alt` | `1` fired pottery piece |
| `crate/crate-small-pottery` | `1-2` fired pottery pieces |
| `crate/crate-large-rot` | `4-6` rot items |
| `crate/crate-small-rot` | `2-4` rot items |
| `crate/crate-large-junk`, `crate/crate-medium-junk`, `crate/crate-small-junk`, `crate/large-generic-junk1` | Tiered junk loot |
| `crate/large-metaljunk1` | Tiered metal loot, with metal bits at `10` per stack and nails at `4` per stack |

Current junk tiers:

- `-junk` common tier: paintings, arrows, bombs, seeds, fabric, nails, resin, and leather
- `-junk` uncommon tier: copper/bronze ingots and plates, windmill and helve hammer parts, and lower-tier armor, with armor taking only 10% of the uncommon junk roll
- `-junk` rare tier: iron and meteoric iron ingots/plates, plus iron and meteoric iron armor
- `-junk` ultra-rare tier: `backpack-sturdy`, plus gold, silver, and steel armor
- `-metaljunk1` common tier: nails, 4 at a time, from a random metal; and random metal bits, 10 at a time
- `-metaljunk1` uncommon tier: copper/bronze ingots and plates
- `-metaljunk1` rare tier: iron, meteoric iron, silver, and gold ingots and plates, plus iron and meteoric iron armor
- `-metaljunk1` ultra-rare tier: silver, gold, and steel armor and plates

## Critter Spawns

Some restored crates, beds, chairs, and tables can also spawn bonus critters.

### Crates

| Restored target | Critter chance | Critter spawn |
|---|---|---|
| `crate/crate-medium-books` | `40%` | `1` moth or mouse group |
| `crate/large-clothing1` | `40%` | `1` moth group |
| `crate/crate-large-junk`, `crate/crate-medium-junk`, `crate/crate-small-junk`, `crate/large-generic-junk1` | `25%` | `1-5` mice or raccoons |
| `crate/crate-large-rot` | `50%` | `1-5` mice or raccoons |
| `crate/crate-small-rot` | `40%` | `1-5` mice or raccoons |
| `crate/medium-toybox1`, `crate/medium-toybox2` | `60%` | `1` mouse or raccoon |
| large crates and medium book/pottery crates | `10%` | `1` mouse or raccoon |

### Furniture

| Restored target | Critter chance | Critter spawn |
|---|---|---|
| beds | `50%` moths, `20%` mice | `1-5` each |
| chairs | `50%` moths, `20%` mice | `1` each |
| tables | `50%` moths, `20%` mice | `1` each |

### Critter Toggles

These config settings control the critter families independently:

- `EnableMoths`
- `EnableMice`
- `EnableRaccoons`

## Crate Restorations

These crate clutter targets restore into crate blocks and may also drop bonus loot:

| Crate target | Restored output |
|---|---|
| `crate/crate-large-ore1`, `crate/crate-large-ore2`, `crate/crate-large-ore3`, `crate/crate-large-oldore` | `20-40` ore nuggets of one random ore type |
| contaminated ore crate variants | `20-40` ore nuggets of one random ore type, plus rare `nugget-pentlandite` and `nugget-uranium` rolls |
| `crate-large-tools1` | `2` random tools |
| `crate/large-clothing1` | `2-4` clothing items, plus a `1%` sturdy backpack roll |
| `crate/crate-large-pottery`, `crate/large-pottery1`, `crate/large-pottery2`, `crate/large-pottery3` | `1-2` fired pottery pieces |
| `crate/crate-medium-books` | `2-4` books |
| `crate/crate-medium-pottery`, `crate/crate-medium-pottery-alt` | `1` fired pottery piece |
| `crate/crate-small-pottery` | `1-2` fired pottery pieces |
| `crate/crate-large-rot` | `4-6` rot items |
| `crate/crate-small-rot` | `2-4` rot items |
| `crate/crate-large-junk`, `crate/crate-medium-junk`, `crate/crate-small-junk`, `crate/large-generic-junk1` | Tiered junk loot |
| `crate/large-metaljunk1` | Tiered metal loot, with metal bits at `10` per stack and nails at `4` per stack |
| `crate/crate-large-empty` | Empty large crate |
| `crate/crate-medium-empty` | Empty medium crate |
| `crate/crate-small-empty` | Empty small crate |
| `crate/crate-small-stacked` | `2` small crates |
| `crate/crate-large-cobweb`, `crate/crate-large-evaporating`, ruined large crates | Empty large crate |
| `crate/crate-small-evaporating`, ruined small crates | Empty small crate |
| `crate/crate-medium-evaporating`, ruined medium crates | Empty medium crate |

### Crate Item Pools

The randomized loot pools behind the crate families are:

- `crate/crate-large-ore1`, `crate/crate-large-ore2`, `crate/crate-large-ore3`, `crate/crate-large-oldore`
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
- contaminated ore crate variants
  - same nugget pool as above
  - rare bonus rolls for `nugget-pentlandite` and `nugget-uranium`
- `crate-large-tools1`
  - Random restored tool codes across axes, hammers, hoes, knives, pickaxes, saws, scythes, shovels, spears, chisels, wrenches, prospecting picks, shears, and tongs
- `crate/large-clothing1`
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
- `crate/crate-large-pottery`, `crate/large-pottery1`, `crate/large-pottery2`, `crate/large-pottery3`, `crate/crate-medium-pottery`, `crate/crate-medium-pottery-alt`, `crate/crate-small-pottery`
  - Fired bowls, clay planters, clay pots, crocks, crucibles, flowerpots, storage vessels, jugs, and watering cans in the four color families
- `crate/crate-medium-books`
  - Normal books in the common color set
- `crate/crate-large-rot`, `crate/crate-small-rot`
  - `leather-normal-plain`
  - `seeds-amaranth`
  - `seeds-bellpepper`
  - `seeds-cabbage`
  - `seeds-carrot`
  - `seeds-cassava`
  - `seeds-fennel`
  - `seeds-flax`
  - `seeds-onion`
  - `seeds-parsnip`
  - `seeds-peanut`
  - `seeds-pumpkin`
  - `seeds-rice`
  - `seeds-rye`
  - `seeds-soybean`
  - `seeds-spelt`
  - `seeds-sunflower`
  - `seeds-turnip`
- `crate/crate-large-junk`, `crate/crate-medium-junk`, `crate/crate-small-junk`, `crate/large-generic-junk1`
  - Common tier: paintings, arrows, bombs, fabric, nails, resin, leather, and seed items
  - Uncommon tier: copper and bronze ingots/plates, windmill parts, helve hammer parts, and lower-tier armor at 10% of the uncommon roll
  - Rare tier: iron and meteoric iron ingots/plates, plus iron and meteoric iron armor
  - Ultra-rare tier: `backpack-sturdy`, plus gold, silver, and steel armor
- `crate/large-metaljunk1`
  - Common tier: nails, 4 at a time, from a random metal; and metal bits, 10 at a time
  - Uncommon tier: copper/bronze ingots and plates
  - Rare tier: iron, meteoric iron, silver, and gold ingots and plates, plus iron and meteoric iron armor
  - Ultra-rare tier: silver, gold, and steel armor and plates

## Current Limitations

- Restored book stands and lecterns currently restore as decorative blocks plus a random book.
- Placing books onto restored book stands or lecterns is not supported yet.
- The deconstructor support list is intentionally curated and does not yet cover every worthwhile non-grid vanilla assembly.

## Configuration

- `VSTemporalReverserConfig.json` stores the mod's settings for restored wood outputs, critter spawns, machine-related options, and debug mode.
- `EnableDebugMode` is off by default and writes restore events to `~/Library/Application Support/VintagestoryData/Logs/VSTemporalReverser/restore-debug.jsonl` on macOS, or the equivalent `VintagestoryData/Logs/VSTemporalReverser/restore-debug.jsonl` path on other platforms.
- When `EnableDebugMode` is on, you can spawn a durable test item with exact remaining durability using:
  - `/trspawntool game:shovel-steel 300`
  - `/trspawntool game:pickaxe-steel 750`
  - Format: `/trspawntool <itemcode> <remainingdurability>`
- `EnableCustomRawMaterialStackSizes` is off by default. This is an opt-in compatibility setting for raising stack sizes on configurable raw materials and common crafting supplies.
- `RawMaterialStackSize` supports `64`, `128`, and `256`. Any other number snaps to the nearest supported value.
- Changing custom stack sizes requires a reload to fully apply.
- The custom stack-size pass can cover raw metals, wood basics, raw clay, seeds, flax materials, cloth, leather, hides, dry grass, papyrus, and resin when the toggle is enabled.
- If `configlib` and your GUI config mod are installed, the config screen is grouped into `Global`, `Rust Ward`, `Wood Restore`, and `Critter Spawns` sections.

## Layout

- `VSTemporalReverser/` - buildable Vintage Story code mod project
- `VSTemporalReverser/assets/vstemporalreverser/` - mod assets
- `data/` - generated vanilla repair candidate data
- `docs/` - analysis notes, reference docs, and generated support lists
- `VERSION` - release version
- `release.sh` - local release package builder

The older helper scripts from the template repo are still present for local tooling workflows.

## Build

```bash
VINTAGE_STORY="/Applications/Vintage Story 1.22.app" dotnet build VSTemporalReverser/VSTemporalReverser.csproj -c Debug -p:NuGetAudit=false
```

## Install + Launch 1.22

```bash
./build-122-install.sh
```

This builds the debug mod, installs it into `/Applications/Vintage Story 1.22.app/Mods/vstemporalreverser`, then launches Vintage Story 1.22 via `~/bin/vs-1.22` when that launcher exists.

## Release Package

```bash
./release.sh
```

The release zip is written to `dist/`.
