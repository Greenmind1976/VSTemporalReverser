# VSTemporalReverser

Vintage Story mod experiment for restoring selected ruined or aged vanilla objects into usable forms.

The first implementation pass adds a creative-only `Temporal Reverser` item. When used on vanilla canopy bed clutter, it removes the clutter block and drops a sleepable vanilla aged wooden bed.

## Current Restoration Rules

- Aged canopy bed clutter costs 1 durability.
- Ruined canopy bed clutter costs 2 durability.
- Current output is `game:bed-woodaged-head-north`, which is sleepable.

This is intentionally a narrow spike. It proves item interaction, vanilla clutter matching, durability cost, block removal, and restored sleepable item drop flow before adding custom functional canopy-bed behavior.

## Layout

- `VSTemporalReverser/` - buildable Vintage Story code mod project
- `VSTemporalReverser/assets/vstemporalreverser/` - mod assets
- `data/` - generated vanilla repair candidate data
- `docs/` - analysis notes for repair candidates and merchant sources
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
