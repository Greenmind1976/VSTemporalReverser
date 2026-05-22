# QA Checklist

This file tracks the recommended in-game regression pass for `VSTemporalReverser`.

Use it as a practical checklist after behavior, loot, dialog, audio, or block-id changes.

## Global Setup

- [ ] Confirm the mod loads without startup errors.
- [ ] Confirm placed reconstruction and deconstruction devices render correctly.
- [ ] Confirm device progress bars render correctly when idle and while running.
- [ ] Confirm the raw-material stack-size config is set to the value you want to test.
- [ ] Confirm `Deconstruct To Ingots` is tested in both states:
  - [ ] `On`
  - [ ] `Off`

## Handheld Reverser: Restore Mode

Test one representative of each family unless a special case is called out.

### Furniture And Fixtures

- [ ] Ruined metal chair clutter
- [ ] Ruined wood chair clutter
- [ ] Ruined table clutter
- [ ] Ruined brazier
- [ ] Ruined chandelier
- [ ] Ruined lectern
- [ ] Ruined bookstand
- [ ] Ruined censer
- [ ] Ruined torch holder

### Beds

- [ ] Ruined short bed clutter
- [ ] Ruined canopy / fancy bed clutter
- [ ] `bed/bed-metal*` source restores into `restored-metal-table-*`
- [ ] `bed/metal2*` source restores into `restored-metal-bed-*`

### Crates

- [ ] Books crate
- [ ] Pottery crate
- [ ] Junk crate
- [ ] Clothing crate

### Other Targets

- [ ] Lantern-style ruin / clutter target
- [ ] Tool or weapon ruin target
- [ ] Toy clutter source
- [ ] Toy shelf source

### Restore Mode Expectations

- [ ] The restored item appears correctly.
- [ ] Bonus dust drops still appear where expected.
- [ ] The `1%` temporal gear roll can still occur on successful handheld use.
- [ ] Restore-mode toy targets still restore normally.
- [ ] Unsupported non-restorable targets still show the normal restore failure message.

## Handheld Reverser: Salvage Mode

### Metal / Mixed Targets

- [ ] Ruined metal chair clutter
- [ ] Ruined metal bed clutter
- [ ] Ruined metal table clutter
- [ ] Ruined chandelier
- [ ] Ruined brazier
- [ ] Ruined metal censer
- [ ] Ruined lantern-style target

### Wood / Cloth Targets

- [ ] Ruined wood chair clutter
- [ ] Ruined wood table clutter
- [ ] Ruined short bed
- [ ] Ruined canopy bed
- [ ] Ruined lectern / bookstand

### Crates / Clutter

- [ ] Books crate
- [ ] Pottery crate
- [ ] Junk crate
- [ ] Clothing crate
- [ ] Tool / weapon clutter
- [ ] Pottery or ceramic clutter target

### Toys

- [ ] Toy clutter source
- [ ] Toy shelf source
- [ ] Toybox-style source if available

### Salvage Mode Expectations

- [ ] Toy salvage now uses the toy-specific message:
  - `The reverser hums at the toy, then thinks better of it. Some little histories are better left unbroken.`
- [ ] Pottery / ceramic salvage still yields clay-related outputs.
- [ ] Salvage still produces dust and the occasional temporal gear roll.
- [ ] Unsupported non-toy targets still use the normal salvage failure text.

## Deconstructor: Machine Flow And Audio

- [ ] Insert one valid item plus dust and confirm:
  - [ ] `switch-items` plays first
  - [ ] the motor / running loop does not start until after the extra startup gap
  - [ ] running textures and particles begin with the loop sound
- [ ] Queue a second valid item and confirm:
  - [ ] `switch-items` plays between items
  - [ ] the next cycle does not start too early
- [ ] Remove the active item mid-process and confirm:
  - [ ] shutdown sound plays
  - [ ] machine shuts down cleanly
- [ ] Let a job complete and confirm:
  - [ ] `100%` progress is visible briefly before completion

## Deconstructor: Vanilla / General

- [ ] Armor piece with `Deconstruct To Ingots = On`
- [ ] Same armor piece with `Deconstruct To Ingots = Off`
- [ ] Lantern with `Deconstruct To Ingots = On`
- [ ] Same lantern with `Deconstruct To Ingots = Off`
- [ ] Item that should return plates / chains / rods / sheets when the setting is off
- [ ] Unsupported non-toy item
- [ ] Restored toy item

### Vanilla / General Expectations

- [ ] `On` gives ingot-style salvage for supported metal outputs.
- [ ] `Off` gives vanilla-style recipe intermediates for supported metal outputs.
- [ ] Unsupported non-toy items show:
  - `The device cannot find a stable point in this item's timeline.`
- [ ] Restored toys show:
  - `The device hums at the toy, then thinks better of it. Some little histories are better left unbroken.`

## Deconstructor: Restored Metal Families

- [ ] `restored-brazier-*`
- [ ] `restored-normal-brazier-*`
- [ ] `restored-dim-brazier-*`
- [ ] `restored-chandelier-*`
- [ ] `restored-censer-*`
- [ ] `restored-lectern-metal-*`
- [ ] `restored-chair-metal-*`
- [ ] `restored-metal-table-*`
- [ ] `restored-metal-table-*` cloth variant
- [ ] `restored-metal-bed-*`
- [ ] `restored-metal-table-*` variant restored from `bed/bed-metal*`

### Restored Metal Expectations

- [ ] Metal chair returns `3` ingots plus `3` `cloth-mordant`.
- [ ] Metal bed returns `4` ingots plus `3` `cloth-white`.
- [ ] Metal table variants return their expected metal + cloth combination.
- [ ] Metal table variant restored from `bed/bed-metal*` returns metal for each detected metal in the variant code.
- [ ] Metal restored families feel at least as good as, and usually better than, handheld salvage.

## Deconstructor: Restored Wood / Library Families

- [ ] `restored-bookstand-*`
- [ ] `restored-lectern-agedwood-*`
- [ ] `restored-lectern-largewood-*`
- [ ] `restored-lectern-ornatewood-*`
- [ ] `restored-lectern-ruinedwood-*`
- [ ] `restored-table-*` plain
- [ ] `restored-table-*` cloth
- [ ] `restored-table-*` `scribeaccessories`
- [ ] `restored-chair-back`
- [ ] `restored-chair-colored-*`
- [ ] `restored-chair-crude`
- [ ] `restored-chair-ebony`
- [ ] `restored-chair-long-*`

### Restored Wood / Library Expectations

- [ ] Library pieces return planks plus parchment.
- [ ] Cloth table / chair variants return cloth in addition to planks / nails.
- [ ] `scribeaccessories` tables return candles.

## Deconstructor: Restored Beds / Crates / Decoration

- [ ] `restored-canopy-bed-*`
- [ ] `restored-short-bed-*`
- [ ] `restored-crate-large-*`
- [ ] `restored-crate-medium-*`
- [ ] `restored-crate-small-*`
- [ ] `restored-decoration-*`

### Restored Beds / Crates / Decoration Expectations

- [ ] Canopy bed returns planks, nails, and white cloth.
- [ ] Short bed returns planks, nails, and white cloth.
- [ ] Crates return planks plus nails / strips.
- [ ] Decoration returns parchment.

## Raw Material Stack Size Coverage

Confirm the custom raw-material stack-size setting affects the material families the mod can now output.

- [ ] `ingot-*`
- [ ] `metalbit-*`
- [ ] `nugget-*`
- [ ] `metalplate-*`
- [ ] `metalchain-*`
- [ ] `metalnailsandstrips-*`
- [ ] `metalsheet-*`
- [ ] `metalscale-*`
- [ ] `metallamellae-*`
- [ ] `rod-*`
- [ ] `plank-*`
- [ ] `clay-*`
- [ ] `seed-*`
- [ ] `seeds-*`
- [ ] `cloth-*`
- [ ] `leather-*`
- [ ] `hide-*`
- [ ] `glass-*`
- [ ] `paper-parchment`
- [ ] `candle`
- [ ] `beeswax`
- [ ] `stick`
- [ ] `firewood`
- [ ] `firewood-aged`
- [ ] `flaxfibers`
- [ ] `flaxtwine`
- [ ] `drygrass`
- [ ] `papyrustops`
- [ ] `papyrusroot`
- [ ] `resin`
- [ ] `gear-temporal`
- [ ] `temporal-dust`

## Notes

- Toys are intentionally excluded from restored-item deconstruction.
- Ceramic / pottery clutter is supported on the handheld salvage side, but restored ceramic families are not yet a broad deconstructor family.
- The renamed beta ids now use:
  - `restored-metal-bed-*`
  - `restored-metal-table-*`
