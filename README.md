# VSMod Template

Bootstrap helper for Vintage Story mod projects with a reusable local toolchain for texture/script workflows.

This repo is not itself a buildable mod skeleton. It is a helper repo that:

- installs shared local tooling once
- creates a new mod from the official Vintage Story templates
- bootstraps the extra files you usually want in a real repo

## Included

- `setup-image-tools.sh`
  - Installs shared tools outside project repos:
    - Homebrew: `python`, `imagemagick`, `ffmpeg`
    - Python venv at `~/Documents/VSMods/.image-tools/venv`
    - Python packages: `pillow`, `numpy`
- `activate-tools.sh`
  - Activates the shared venv in the current shell.
- `bootstrap-mod.sh`
  - Adds starter repo files after `dotnet new`
  - Creates `VERSION`, `RELEASE_NOTES.md`, `TODO.md`, `.gitignore`, `README.md`, and `release.sh` when missing.
- `new-mod.sh`
  - Wraps `dotnet new vsmod` / `vsmoddll`
  - Runs the bootstrap step automatically

## Why this layout

You asked to keep tooling out of each mod repo. This helper installs tools once in a shared folder:

- `~/Documents/VSMods/.image-tools`

Any mod repo can reuse the same environment.

## First-time setup (tools)

From this repo:

```bash
chmod +x setup-image-tools.sh activate-tools.sh
./setup-image-tools.sh
```

## Official VS project bootstrap (NuGet template)

Vintage Story’s current recommended bootstrap is the official template package.

Install template package:

```bash
dotnet new install VintageStory.Mod.Templates
```

Set your game path for local API references:

```bash
export VINTAGE_STORY="/Applications/Vintage Story.app/Contents/Resources"
```

Create a new mod project:

```bash
dotnet new vsmod -n MyMod
```

Or DLL-only variant:

```bash
dotnet new vsmoddll -n MyMod
```

Then build:

```bash
dotnet build
```

## Quick project creation helper

Use the included wrapper script:

```bash
chmod +x new-mod.sh
./new-mod.sh MyMod
```

Options:

```bash
./new-mod.sh MyMod vsmoddll
./new-mod.sh MyMod vsmod ~/Documents/VSMods
```

## Daily usage

In any mod repo shell:

```bash
source ~/Documents/VSMods/VSMod-Template/activate-tools.sh
```

Then tools are available:

- `python` with `Pillow` + `numpy`
- `magick` (ImageMagick)
- `ffmpeg`

## Suggested workflow for a new mod

1. Run `./new-mod.sh MyMod`.
2. Activate shared tools.
3. Update the generated `README.md`, `modinfo.json`, `VERSION`, and `RELEASE_NOTES.md`.
4. Add texture scripts under `tools/textures/` in the mod repo if needed.
5. Run build/test loop (`dotnet build`, in-game validation).

## Optional shell helper

To auto-load tools when entering any VSMods folder, add this to `~/.zshrc`:

```bash
vsmod-tools() {
  source ~/Documents/VSMods/VSMod-Template/activate-tools.sh
}
```

Then run:

```bash
vsmod-tools
```

## Notes

- If `brew` is missing, install from <https://brew.sh/>.
- `setup-image-tools.sh` is idempotent; re-running is safe.
- If Python packages break after system updates, rerun setup.
- You can keep your existing direct DLL-reference `.csproj` workflow; the NuGet template is just the cleaner bootstrap.
