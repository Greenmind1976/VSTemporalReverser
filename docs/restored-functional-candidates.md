# Restored Functional Candidates

Scope: vanilla `game` domain only.

This pass checks the idea that the Temporal Reverser can work on two input tiers:

- Ruined input: costs 2x reverser durability.
- Aged/old input: costs 1x reverser durability.

The output should be a new modded `temporalreverser:restored-*` block/item. It can reuse a vanilla aged/old/clean visual model, but the mod supplies the missing function.

## Scan Result

From vanilla blocktype assets I found:

- 119 unique ruined variant codes.
- 165 unique aged/old variant codes.

Most of those are not useful for this mod. The good candidates are the families where vanilla has both a broken/ruined visual and a usable aged/old/clean visual we can borrow.

## Best First Targets

| Family | Ruined input, 2x durability | Aged input, 1x durability | Proposed restored output | Function to restore |
| --- | --- | --- | --- | --- |
| Canopy bed | `fancy-bed-stitched-ruined`, `bed/bed-fancy-ruined*` | `fancy-bed-old`, `fancy-bed-old-drapes-opened`, `fancy-bed-old-drapes-closed` | `temporalreverser:restored-canopy-bed` | bed/sleep |
| Short bed | `bed-short-stitched-ruined` | `bed-short-old` | `temporalreverser:restored-short-bed` | bed/sleep |
| Wooden bed | `bed/bed-ruined*` | `game:bed-woodaged-head-*` | `temporalreverser:restored-aged-wooden-bed` | bed/sleep |
| Metal bed | `bed/bed-metal-ruined*`, `bed/metal2-ruined*` | `bed/bed-metal`, `bed/metal2` | `temporalreverser:restored-metal-bed` | bed/sleep |
| Chair | `chair-ruined*` | `chair-aged`, `game:chair-aged` | `temporalreverser:restored-aged-chair` | sitting, if implemented |
| Table | `table-ruined*` | `table-aged`, `game:table-aged` | `temporalreverser:restored-aged-table` | table/collision/display |
| Metal chair | `chair-metal1-ruined*` | `chair-metal1` | `temporalreverser:restored-metal-chair` | sitting, if implemented |
| Metal table | `table/metal1-ruined*` | `table/metal1` | `temporalreverser:restored-metal-table` | table/collision/display |
| Lectern | `bookshelves/lecturn-ruined`, `lecturn-ruined` | `bookshelves/lecturn-aged-*` | `temporalreverser:restored-aged-lectern` | display/readable behavior if added |
| Scroll rack | `bookshelves/scrollrack-ruined*` | `game:scrollrack-aged`, `game:scrollrack-veryaged`, `bookshelves/scrollrack-empty1` | `temporalreverser:restored-aged-scroll-rack` | vanilla scrollrack |
| Tool rack | `toolrack-ruined*` | `toolrack-empty`, `game:toolrack-*` | `temporalreverser:restored-aged-tool-rack` | vanilla toolrack |
| Crate | `crate/crate-small-ruined*`, `crate/crate-large-ruined*` | `game:crate` with `type: wood-aged`, empty crate clutter | `temporalreverser:restored-aged-crate` | crate storage |
| Torch holder | `game:torchholder-ruined-*` | `game:torchholder-aged-*` | `temporalreverser:restored-aged-torch-holder` | vanilla torchholder |
| Metal shelf | `shelf/metal1-ruined1`, `shelf/metal2-ruined1` | `shelf/metal1-empty`, `shelf/metal2-empty` | `temporalreverser:restored-metal-shelf` | display/storage if added |

## Canopy Bed Notes

This is the best proof of concept.

Vanilla has these useful visual sources:

- `fancy-bed-stitched-ruined` is player-facing as "Aged canopy bed" even though the type name says ruined.
- `fancy-bed-old` is also "Aged canopy bed".
- `fancy-bed-old-drapes-opened` and `fancy-bed-old-drapes-closed` are also "Aged canopy bed".
- `bed/bed-fancy-ruined3` through `bed/bed-fancy-ruined6` are player-facing as "Ruined canopy bed".
- `bed/bed-fancy-open` is player-facing as "Canopy bed".

So the mod can do this:

`Ruined canopy bed` or `Aged canopy bed` -> `Restored canopy bed`

The restored block should not be vanilla `game:clutter`. It should be a modded functional bed that borrows the aged canopy bed model and implements bed behavior.

## Implementation Shape

For each family, the Reverser can use the same rule shape:

1. Raycast a block.
2. If it is `game:clutter`, read its `attributes.type`.
3. Match against `ruinedInputs` or `agedInputs`.
4. Consume reverser durability:
   - 2x for ruined input.
   - 1x for aged/old input.
5. Remove the original block.
6. Spawn or place the `temporalreverser:restored-*` output.

For beds, I would start by dropping the restored bed item instead of placing it directly. That avoids immediately solving multiblock placement while we prove that the conversion and functional block are viable.

## Not First-Pass Targets

These exist in vanilla but should wait:

- Devastation machinery: lots of aged pieces, unclear functional target.
- Banners: aged banners exist, but no useful ruined counterpart.
- Ladders: aged ladders already include climbable variants, so restoration does not add much.
- Hazmat/mechanics: ruined and clean clutter exist, but the restored function is unclear.
- Censers/clay lamps: possible light-source candidates, but they need a design choice before implementation.
