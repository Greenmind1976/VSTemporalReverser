# Dialog Reference

This file tracks the current player-facing dialogs, prompts, and notification text used by `VSTemporalReverser`, along with where each one appears.

## Full GUI Dialogs

### Temporal Reconstruction Device

- Title: `Temporal Reconstruction Device`
- Trigger: Right click a placed reconstruction device
- Files:
  - [BlockTemporalReconstructionDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockTemporalReconstructionDevice.cs)
  - [BlockEntityTemporalReconstructionDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockEntityTemporalReconstructionDevice.cs)
  - [GuiDialogBlockEntityTemporalReconstructionDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/GuiDialogBlockEntityTemporalReconstructionDevice.cs)
  - [en.json](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/assets/vstemporalreverser/lang/en.json)

Sections shown in the dialog:

- `Repair Queue`
- `Fuel`
- `Temporal Reading`
- `Reconstructed Output`

Current status text shown in `Temporal Reading`:

- `Place the damaged item within and add 10 temporal dust to begin reconstruction.`
  - Shown when the dialog is open, the machine is idle, and there is no stronger machine status to show.
- `The chamber is aligned. Reconstruction can begin.`
  - Shown when the machine reports `Ready for reconstruction.`
- `Reconstruction in progress...`
  - Shown while an active repair cycle is running.
- `Reconstructor unavailable.`
  - Fallback if the block entity cannot be found on the client.
- `Using one temporal device to repair another would be wildly irresponsible.`
  - Shown when a reverser is inserted.
- `This item cannot be reconstructed here.`
  - Shown for unsupported inputs.
- `Output inventory is full. Remove reconstructed items to continue.`
  - Shown when output space blocks the next result.
- `Add 10 temporal dust to power reconstruction.`
  - Shown when fuel is missing.
- `No items need repair.`
  - Shown when a supported item is present but already fully repaired.
- `Ready for reconstruction.`
  - Internal machine status used before the dialog swaps it for the more natural player-facing line above.

### Temporal Deconstructor

- Title: `Temporal Deconstructor`
- Trigger: Right click a placed deconstructor
- Files:
  - [BlockTemporalDeconstructorDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockTemporalDeconstructorDevice.cs)
  - [BlockEntityTemporalDeconstructorDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockEntityTemporalDeconstructorDevice.cs)
  - [GuiDialogBlockEntityTemporalDeconstructorDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/GuiDialogBlockEntityTemporalDeconstructorDevice.cs)
  - [en.json](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/assets/vstemporalreverser/lang/en.json)

Sections shown in the dialog:

- `Deconstruction Queue`
- `Fuel`
- `Temporal Reading`
- `Reclaimed Output`

Current status text shown in `Temporal Reading`:

- `Place the item within and add 10 temporal dust to begin deconstruction.`
  - Shown when the dialog is open, the machine is idle, and there is no stronger machine status to show.
- `The chamber is aligned. Deconstruction can begin.`
  - Shown when the machine reports `Ready for deconstruction.`
- `Deconstruction in progress...`
  - Shown while an active deconstruction cycle is running.
- `Deconstructor unavailable.`
  - Fallback if the block entity cannot be found on the client.
- `The device cannot find a stable point in this item's timeline.`
  - Shown when the inserted item is not a supported deconstruction candidate.
- `Its past is too tangled for the device to unwind safely.`
  - Shown when the item category is allowed but no valid deconstruction job can be resolved.
- `Preparing next item...`
  - Shown during the between-item handoff pause.
- `Output inventory is full for the next job. Remove reclaimed items to continue.`
  - Shown when output space blocks the next result.
- `Add 10 temporal dust to power deconstruction.`
  - Shown when fuel is missing.
- `Ready for deconstruction.`
  - Internal machine status used before the dialog swaps it for the more natural player-facing line above.

## Interaction Hints

These are not full dialogs, but they are player-facing interaction text tied to the placed blocks.

- `Open reconstruction device`
  - Shown in block interaction help for the reconstruction device.
  - Files:
    - [BlockTemporalReconstructionDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockTemporalReconstructionDevice.cs)
    - [en.json](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/assets/vstemporalreverser/lang/en.json)
- `Open deconstructor`
  - Shown in block interaction help for the deconstructor.
  - Files:
    - [BlockTemporalDeconstructorDevice.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockTemporalDeconstructorDevice.cs)
    - [en.json](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/assets/vstemporalreverser/lang/en.json)

## In-World Prompt And Error Messages

### Restored Book Surface

- Message: `Hold Shift and right click to place a book on this surface.`
- Trigger: Player right clicks the restored book surface with a placeable book in hand, but without holding Shift
- File:
  - [BlockEntityRestoredBookSurface.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockEntityRestoredBookSurface.cs)

### Closed Canopy Bed During Temporal Storm Restriction

- Message source: `Lang.Get("cantsleep-tempstorm")`
- Trigger: Player tries to use a restored closed-canopy bed when storm sleeping is blocked
- File:
  - [BlockRestoredCanopyBed.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/BlockRestoredCanopyBed.cs)

Note:

- This text comes from the base game language key rather than a mod-local hardcoded string.

## Handheld Temporal Reverser Notifications

These are notification messages sent through the general notification chat channel when the handheld reverser is used.

File:

- [ItemTemporalReverser.cs](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/ItemTemporalReverser.cs)

Messages and trigger conditions:

- `The reverser's field is spent. It needs fresh temporal dust.`
  - Shown when the player tries to use a depleted reverser.
- `This object no longer remembers its former state. It may be salvagable.`
  - Shown when no restoration rule is found, but fallback salvage would still produce something useful.
- `The object's former state was lost in time.`
  - Shown when no restoration rule is found and no salvage fallback is possible.
- `The reverser hums, but finds no restorable pattern.`
  - Shown when a non-supported target has no matching rule and no clutter salvage fallback path.
- `The reverser needs a moment to recharge.`
  - Shown when the player uses the reverser again before its restore cooldown has expired.
- `The reverser finds the pattern, but cannot draw it fully back into the present.`
  - Shown when a restoration rule matches, but the restored stack cannot be created.
- `The reverser cannot salvage anything useful from this pattern.`
  - Shown when salvage resolves to no output stacks.
- `Salvaged materials spill free from the unraveling pattern.`
  - Success notification for salvage mode.
- `The restored item drops free in a usable shape.`
  - Success notification for normal restore mode.

## Scope Note

This file focuses on player-facing dialogs and prompt-like text currently implemented in C#.

It does not try to catalog:

- every item or block name in [en.json](/Users/garretcoffman/Documents/VSMods/VSTemporalReverser/VSTemporalReverser/assets/vstemporalreverser/lang/en.json)
- block hover info from `GetBlockInfo()` unless that text is functioning like a dialog prompt
- sound cues, particle effects, or animation states unless they are paired with user-facing text
