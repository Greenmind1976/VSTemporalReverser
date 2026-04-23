#!/usr/bin/env bash
set -euo pipefail

###############################################################################
# Build + Install VSTemporalReverser into Vintage Story 1.22
###############################################################################

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

cd "$ROOT_DIR"

MOD_ID="vstemporalreverser"
PROJECT_DIR="VSTemporalReverser"
PROJECT_PATH="$PROJECT_DIR/VSTemporalReverser.csproj"
MOD_BUILD_DIR="$PROJECT_DIR/bin/Debug/Mods/mod"
VS_APP_DIR="/Applications/Vintage Story 1.22.app"
VS_MODS_DIR="$VS_APP_DIR/Mods"
VS_LAUNCHER="${VS_LAUNCHER:-$HOME/bin/vs-1.22}"
VS_EXECUTABLE="$VS_APP_DIR/Vintagestory"

rm -rf "$PROJECT_DIR/bin" "$PROJECT_DIR/obj"

echo "Deleting installed mod dir: $VS_MODS_DIR/$MOD_ID"
rm -rf "$VS_MODS_DIR/$MOD_ID"

if [[ -e "$VS_MODS_DIR/$MOD_ID" ]]; then
  echo "ERROR: Mod dir still exists: $VS_MODS_DIR/$MOD_ID" >&2
  exit 1
fi

if [[ ! -d "$VS_APP_DIR" ]]; then
  echo "ERROR: Vintage Story app not found: $VS_APP_DIR" >&2
  exit 1
fi

VINTAGE_STORY="$VS_APP_DIR" dotnet build "$PROJECT_PATH" -p:NuGetAudit=false

if [[ ! -d "$MOD_BUILD_DIR" ]]; then
  echo "ERROR: Expected build output folder not found: $MOD_BUILD_DIR" >&2
  exit 1
fi

if [[ ! -w "$VS_MODS_DIR" ]]; then
  echo "Mods folder not writable, using sudo..."
  sudo mkdir -p "$VS_MODS_DIR"
  sudo rm -rf "$VS_MODS_DIR/$MOD_ID"
  sudo cp -R "$MOD_BUILD_DIR" "$VS_MODS_DIR/$MOD_ID"
else
  mkdir -p "$VS_MODS_DIR"
  rm -rf "$VS_MODS_DIR/$MOD_ID"
  cp -R "$MOD_BUILD_DIR" "$VS_MODS_DIR/$MOD_ID"
fi

echo "Installed '$MOD_ID' to:"
echo "  $VS_MODS_DIR/$MOD_ID"

if [[ -x "$VS_LAUNCHER" ]]; then
  echo
  echo "Launching Vintage Story 1.22 via:"
  echo "  $VS_LAUNCHER"
  "$VS_LAUNCHER" >/dev/null 2>&1 &
elif [[ -x "$VS_EXECUTABLE" ]]; then
  echo
  echo "Launching Vintage Story 1.22 via:"
  echo "  $VS_EXECUTABLE"
  "$VS_EXECUTABLE" >/dev/null 2>&1 &
else
  echo
  echo "Vintage Story 1.22 launcher not found at: $VS_LAUNCHER"
  echo "Vintage Story 1.22 executable not found at: $VS_EXECUTABLE"
  echo "Install is complete; start the game manually to test the mod."
fi
