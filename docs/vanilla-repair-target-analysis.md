# Vanilla Repair Target Analysis

Scope: vanilla `game` domain only. I used the 1.22 Asset Inspector dumps in the app data folder and checked source assets under `/Applications/Vintage Story 1.22.app/assets/game` and `/Applications/Vintage Story 1.22.app/assets/survival`.

Primary dump:

- `/Users/garretcoffman/Library/Application Support/VintagestoryData-1.22/VSAssetInspector/assetinspect-ids-all-game-20260422-222237-433.json`
- Generated: `2026-04-22T22:22:37.432689Z`
- Counts: 4469 items, 14091 blocks, 820 entities, 19380 total records

Comparison dump:

- `/Users/garretcoffman/Library/Application Support/VintagestoryData/VSAssetInspector/assetinspect-ids-all-game-20260417-213253-208.json`
- Generated: `2026-04-17T21:32:53.207222Z`
- Counts: 4030 items, 13833 blocks, 785 entities, 18648 total records

There is also an item-only dump:

- `/Users/garretcoffman/Library/Application Support/VintagestoryData-1.22/VSAssetInspector/assetinspect-ids-items-game-20260422-222256-235.json`
- Counts: 4469 items, no blocks/entities

## Summary

The feasible vanilla repair model is not one universal `broken -> normal` mapping. It splits into three buckets:

1. Exact replacement mappings:
   - Antique armor has `broken`, `damaged`, and `pristine` variants.
   - Static translocators have `broken` and `normal` variants per facing.

2. Repair-in-place/drop-self targets:
   - `game:clutter` already has `BlockClutter` plus `Reparable`.
   - The source asset marks clutter with `attributes.reparability: 6`.
   - Pitch glue applies `repairgainByType: "*-hot": 0.33334`, so vanilla repairs clutter in roughly three hot-glue uses.
   - For this mod, a temporal reverser can likely set the same repaired state/attribute that glue produces, then break/drop or spawn the collected clutter block.

3. No exact vanilla non-ruined counterpart:
   - Ruined weapons are real item variants such as `game:axe-bearded-ruined`.
   - In the vanilla expanded item list, `game:axe-bearded`, `game:blade-gladius`, etc. do not exist without `-ruined`.
   - These can be repaired as durability restoration on the same item, or mapped by design to a nearest standard weapon such as `axe-felling-iron`, but that would be a mod-authored interpretation rather than a vanilla mapping.

## Strong Exact Mappings

Antique armor:

| Broken | Damaged | Pristine |
| --- | --- | --- |
| `game:armor-head-antique-blackguard-broken` | `game:armor-head-antique-blackguard-damaged` | `game:armor-head-antique-blackguard-pristine` |
| `game:armor-body-antique-blackguard-broken` | `game:armor-body-antique-blackguard-damaged` | `game:armor-body-antique-blackguard-pristine` |
| `game:armor-legs-antique-blackguard-broken` | `game:armor-legs-antique-blackguard-damaged` | `game:armor-legs-antique-blackguard-pristine` |
| `game:armor-head-antique-forlorn-broken` | `game:armor-head-antique-forlorn-damaged` | `game:armor-head-antique-forlorn-pristine` |
| `game:armor-body-antique-forlorn-broken` | `game:armor-body-antique-forlorn-damaged` | `game:armor-body-antique-forlorn-pristine` |
| `game:armor-legs-antique-forlorn-broken` | `game:armor-legs-antique-forlorn-damaged` | `game:armor-legs-antique-forlorn-pristine` |

Static translocator blocks:

| Broken | Normal |
| --- | --- |
| `game:statictranslocator-broken-north` | `game:statictranslocator-normal-north` |
| `game:statictranslocator-broken-east` | `game:statictranslocator-normal-east` |
| `game:statictranslocator-broken-south` | `game:statictranslocator-normal-south` |
| `game:statictranslocator-broken-west` | `game:statictranslocator-normal-west` |

## Ruined Weapons

These are vanilla item records, but I found no exact clean counterpart in the expanded dump:

- `game:axe-bearded-ruined`
- `game:axe-battle-ruined`
- `game:axe-bardiche-ruined`
- `game:axe-double-ruined`
- `game:blade-gladius-ruined`
- `game:blade-arming-ruined`
- `game:blade-claymore-ruined`
- `game:blade-sabre-ruined`
- `game:club-flanged-ruined`
- `game:club-morningstar-ruined`
- `game:club-spiked-ruined`
- `game:club-warhammer-ruined`
- `game:knife-dagger-ruined`
- `game:knife-stiletto-ruined`
- `game:knife-khanjar-ruined`
- `game:knife-baselard-ruined`
- `game:spear-boar-ruined`
- `game:spear-voulge-ruined`
- `game:spear-fork-ruined`
- `game:spear-ranseur-ruined`

Suggested behavior: restore durability on the same ruined weapon item, or leave these out of the first version. If we later want temporal transformation, the mapping should be design-authored by weapon family, not inferred as vanilla truth.

## Ruined/Broken Blocks From Expanded IDs

Potentially useful:

- `game:door-ruined-rough1` through `game:door-ruined-rough3`
- `game:door-ruined-windowed1` through `game:door-ruined-windowed3`
- `game:door-ruined-solid1` through `game:door-ruined-solid3`
- `game:door-ruined-barred1` through `game:door-ruined-barred3`
- `game:door-ruined-sleek1` through `game:door-ruined-sleek3`
- `game:torchholder-ruined-empty-*`
- `game:torchholder-ruined-filled-*`
- `game:statictranslocator-broken-*`

Probably not repair targets:

- `game:brickruin-irregular-*` are aged brick blocks, not broken versions of normal bricks.
- `game:egg-chicken-broken` is a broken egg state, not a repairable item.
- `game:painting-castleruin-*` and `game:painting-sunkenruin-*` contain "ruin" in the painting title only.

## Clutter Repair Bucket

The Asset Inspector dump lists `game:clutter` as one block, while the broken/ruined identity lives in its `type` attribute. Source assets expose many type codes such as:

- `anvil-broken1` through `anvil-broken3`
- `bed/bed-*-ruined*`
- `bookshelves/scrollrack-ruined*`
- `chair-ruined*`
- `chandelier-ruined*`
- `crate/crate-*-ruined*`
- `hazmat-ruined*`
- `mechanics-ruined*`
- `pipe-veryrusted-*-broken*`
- `stove-ruined`
- `table-ruined*`
- `toolrack-ruined*`

There is also `game:clutter-devastation` with `brokengear/*` type values, but its asset has `repairability: 999` and no `Reparable` behavior. I would not include it in version one.

## Dump Comparison Notes

The April 22 all-game dump adds these broken/ruined candidates versus the April 17 all-game dump:

- Item: `game:clutter-fishing/brokensword`
- Blocks: all `door-ruined-barred*`, `door-ruined-solid*`, `door-ruined-sleek*`, and `torchholder-ruined-*`

No broken/ruined candidates disappeared between those two dumps.

## Implementation Direction

For the Temporal Reverser item, start with block interaction:

1. Raycast target block.
2. If block code is `game:clutter`, inspect block entity/tree attributes for `type`.
3. If type contains `broken` or `ruined` and the block has the vanilla reparable state, set it fully repaired, then drop/spawn that collected clutter stack and remove the block.
4. If block code matches an exact replacement mapping, replace it with the mapped block or drop the mapped block stack, depending on desired gameplay.
5. For item targets in inventories or ground entities, apply exact armor mappings first; leave ruined weapons as same-code durability restoration unless we intentionally add design mappings.

