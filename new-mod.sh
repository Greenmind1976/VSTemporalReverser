#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Prevent leaking shell options when accidentally sourced.
if (return 0 2>/dev/null); then
  echo "Run this script, do not source it:"
  echo "  ./new-mod.sh <ModName> [vsmod|vsmoddll] [target-dir]"
  return 1
fi

# Create a new Vintage Story mod using official templates.
# Usage:
#   ./new-mod.sh <ModName> [vsmod|vsmoddll] [target-dir]
# Examples:
#   ./new-mod.sh MyMod
#   ./new-mod.sh MyLibrary vsmoddll
#   ./new-mod.sh MyMod vsmod ~/Documents/VSMods

if [ "${1:-}" = "" ]; then
  echo "Usage: $0 <ModName> [vsmod|vsmoddll] [target-dir]"
  exit 1
fi

MOD_NAME="$1"
TEMPLATE="${2:-vsmod}"
TARGET_DIR="${3:-$PWD}"

if [ "$TEMPLATE" != "vsmod" ] && [ "$TEMPLATE" != "vsmoddll" ]; then
  echo "Template must be 'vsmod' or 'vsmoddll'"
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is required. Install .NET SDK first."
  exit 1
fi

if ! dotnet new list | grep -Eq "vsmod|vsmoddll"; then
  echo "VintageStory templates not found. Installing..."
  dotnet new install VintageStory.Mod.Templates
fi

if [ -z "${VINTAGE_STORY:-}" ]; then
  DEFAULT_VS_PATH="/Applications/Vintage Story.app/Contents/Resources"
  if [ -d "$DEFAULT_VS_PATH" ]; then
    export VINTAGE_STORY="$DEFAULT_VS_PATH"
    echo "VINTAGE_STORY not set. Using: $VINTAGE_STORY"
  else
    cat <<MSG
VINTAGE_STORY is not set.
Set it before building, for example:
  export VINTAGE_STORY="/Applications/Vintage Story.app/Contents/Resources"
MSG
  fi
fi

mkdir -p "$TARGET_DIR"
cd "$TARGET_DIR"

if [ -e "$MOD_NAME" ]; then
  echo "Target already exists: $TARGET_DIR/$MOD_NAME"
  exit 1
fi

dotnet new "$TEMPLATE" -n "$MOD_NAME"
/bin/bash "$SCRIPT_DIR/bootstrap-mod.sh" "$TARGET_DIR/$MOD_NAME"

echo

echo "Created: $TARGET_DIR/$MOD_NAME"
echo "Next steps:"
echo "  cd \"$TARGET_DIR/$MOD_NAME\""
echo "  dotnet build"
echo "  ./release.sh"
