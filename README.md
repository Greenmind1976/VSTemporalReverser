# VSTemporalReverser

Vintage Story mod experiment for restoring selected ruined or aged vanilla objects into usable forms.

The mod currently adds a creative-only `Temporal Reverser` item. When used on supported vanilla clutter or ruined decor, it removes the original block and drops a usable restored block variant.

## Current Restoration Rules

- Aged or old targets cost `1` durability.
- Ruined targets cost `2` durability.
- Restorations currently produce a mix of vanilla usable blocks and modded restored blocks depending on the furniture family.

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

The mod has moved past the original canopy-bed-only spike. It now covers a small curated set of ruined or aged furnishings while still using the same basic interaction flow: match a target, consume durability, remove the original block, and drop the restored result.

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
