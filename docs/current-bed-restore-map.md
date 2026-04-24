# Current Bed Restore Map

This file reflects the current bed restore behavior implemented in the mod.

Durability costs:

- Aged input: `1`
- Ruined input: `2`

## Canopy Beds

### Fixed aged canopy restores

- `fancy-bed-green`
  - restores to `restored-canopy-bed-greenplaidopen-{wood}`
- `fancy-bed-green-drapes-opened`
  - restores to `restored-canopy-bed-greenplaidopened-{wood}`
- `fancy-bed-green-drapes-closed`
  - restores to `restored-canopy-bed-greenplaidclosed-{wood}`

### Family-matched aged canopy restores

- `fancy-bed-old`
  - restores to a random non-green open canopy bed:
  - `morningstaropen`
  - `blueplaidopen`
  - `redplaidopen`
  - `honeycombopen`

- `fancy-bed-old-drapes-opened`
  - restores to a random non-green opened-drapes canopy bed:
  - `honeycombopened`
  - `morningstaropened`
  - `blueplaidopened`
  - `redplaidopened`

- `fancy-bed-old-drapes-closed`
  - restores to a random non-green closed-drapes canopy bed:
  - `morningstarclosed`
  - `blueplaidclosed`
  - `redplaidclosed`
  - `honeycombclosed`

### Fully ruined canopy restores

These ignore the original family and can restore into any canopy-bed family:

- `fancy-bed-stitched-ruined`
- `bed/bed-fancy-ruined1`
- `bed/bed-fancy-ruined2`
- `bed/bed-fancy-ruined3`
- `bed/bed-fancy-ruined4`
- `bed/bed-fancy-ruined5`
- `bed/bed-fancy-ruined6`

Possible restored canopy outputs:

- `morningstaropen`
- `blueplaidopen`
- `greenplaidopen`
- `redplaidopen`
- `honeycombopen`
- `morningstaropened`
- `blueplaidopened`
- `greenplaidopened`
- `redplaidopened`
- `honeycombopened`
- `morningstarclosed`
- `blueplaidclosed`
- `greenplaidclosed`
- `redplaidclosed`
- `honeycombclosed`

All restored canopy beds also roll a random wood:

- `mahogany`
- `walnut`
- `oak`
- `maple`
- `pine`
- `redwood`

## Short Beds

### Aged short-bed restores

- `bed-short-green`
  - restores to `restored-short-bed-greenplaid-{wood}`
- `bed-short-old`
  - restores to a random non-green restored short bed:
  - `morningstar`
  - `blueplaid`
  - `redplaid`
  - `honeycomb`

### Ruined short-bed restores

- `bed-short-stitched-ruined`
  - restores to a random short-bed style:
  - `morningstar`
  - `blueplaid`
  - `greenplaid`
  - `redplaid`
  - `honeycomb`

All restored short beds also roll a random wood:

- `mahogany`
- `walnut`
- `oak`
- `maple`
- `pine`
- `redwood`

## Plain Ruined Beds

- `bed/bed-ruined1`
  - restores to a random restored short bed:
  - `morningstar`
  - `blueplaid`
  - `greenplaid`
  - `redplaid`
  - `honeycomb`
- `bed/bed-ruined2`
  - restores to a random restored short bed:
  - `morningstar`
  - `blueplaid`
  - `greenplaid`
  - `redplaid`
  - `honeycomb`
- `bed/bed-ruined3`
  - restores to `game:bed-woodaged-head-north`
- `bed/bed-ruined4`
  - restores to `game:bed-woodaged-head-north`
- `bed/bed-ruined5`
  - restores to `game:bed-woodaged-head-north`
- `bed/bed-ruined6`
  - restores to `game:bed-woodaged-head-north`

## Intentionally Not Supported Right Now

- `bed/bed-metal-ruined*`
- `bed/metal2-ruined*`
- `bed/bed-straw-ruined*`

Notes:

- The game has clean metal bed clutter, but it is intentionally not part of the current mod scope.
- That keeps the mod focused on the bed families that already feel useful and visually distinct.
