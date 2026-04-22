#!/usr/bin/env bash

NEW_PY_VENV="${HOME}/Documents/VSMods/.image-tools/venv"
OLD_PY_VENV="${HOME}/Documents/VSMods/.codex-tools/venv"
PY_VENV="$NEW_PY_VENV"

if [ ! -f "${PY_VENV}/bin/activate" ] && [ -f "${OLD_PY_VENV}/bin/activate" ]; then
  PY_VENV="$OLD_PY_VENV"
fi

if [ ! -f "${PY_VENV}/bin/activate" ]; then
  echo "Tool venv not found at ${PY_VENV}."
  echo "Run ./setup-image-tools.sh first."
  return 1 2>/dev/null || exit 1
fi

# shellcheck disable=SC1090
source "${PY_VENV}/bin/activate"

echo "Shared image tools activated: ${PY_VENV}"
echo "python: $(command -v python)"
echo "pip:    $(command -v pip)"
