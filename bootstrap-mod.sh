#!/usr/bin/env bash
set -euo pipefail

if (return 0 2>/dev/null); then
  echo "Run this script, do not source it:"
  echo "  ./bootstrap-mod.sh <project-dir>"
  return 1
fi

if [ "${1:-}" = "" ]; then
  echo "Usage: $0 <project-dir>"
  exit 1
fi

PROJECT_DIR="$1"

if [ ! -d "$PROJECT_DIR" ]; then
  echo "Project directory not found: $PROJECT_DIR"
  exit 1
fi

MODINFO_PATH="$PROJECT_DIR/modinfo.json"
if [ ! -f "$MODINFO_PATH" ]; then
  echo "modinfo.json not found in: $PROJECT_DIR"
  exit 1
fi

MOD_NAME="$(basename "$PROJECT_DIR")"
VERSION="$(sed -n 's/.*"version":[[:space:]]*"\([^"]*\)".*/\1/p' "$MODINFO_PATH" | head -n 1)"
if [ -z "$VERSION" ]; then
  VERSION="1.0.0"
fi

MOD_ID="$(sed -n 's/.*"modid":[[:space:]]*"\([^"]*\)".*/\1/p' "$MODINFO_PATH" | head -n 1)"
if [ -z "$MOD_ID" ]; then
  MOD_ID="$(printf '%s' "$MOD_NAME" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9._-')"
fi

CSPROJ_PATH="$(find "$PROJECT_DIR" -maxdepth 1 -name '*.csproj' | head -n 1)"
CSPROJ_NAME="$(basename "${CSPROJ_PATH:-$MOD_NAME.csproj}")"
ASSEMBLY_NAME="${CSPROJ_NAME%.csproj}"

if [ ! -f "$PROJECT_DIR/VERSION" ]; then
  printf '%s\n' "$VERSION" > "$PROJECT_DIR/VERSION"
fi

if [ ! -f "$PROJECT_DIR/RELEASE_NOTES.md" ]; then
  cat > "$PROJECT_DIR/RELEASE_NOTES.md" <<EOF
# ${MOD_NAME} ${VERSION}

## Highlights
- Initial release.

## Notes
- Add release notes here before publishing.
EOF
fi

if [ ! -f "$PROJECT_DIR/TODO.md" ]; then
  cat > "$PROJECT_DIR/TODO.md" <<'EOF'
# TODO

- Add feature ideas and follow-up tasks here.
EOF
fi

if [ ! -f "$PROJECT_DIR/README.md" ]; then
  cat > "$PROJECT_DIR/README.md" <<EOF
# ${MOD_NAME}

Description goes here.

## Development

\`\`\`bash
dotnet build
\`\`\`

## Release

\`\`\`bash
./release.sh
\`\`\`
EOF
fi

if [ ! -f "$PROJECT_DIR/.gitignore" ]; then
  cat > "$PROJECT_DIR/.gitignore" <<'EOF'
bin/
obj/
dist/
.DS_Store
EOF
fi

if [ ! -f "$PROJECT_DIR/release.sh" ]; then
  cat > "$PROJECT_DIR/release.sh" <<EOF
#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="\$(cd "\$(dirname "\${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="\$ROOT_DIR/${CSPROJ_NAME}"
MOD_OUTPUT_DIR="\$ROOT_DIR/bin/Release/Mods/mod"
VERSION_FILE="\$ROOT_DIR/VERSION"
DIST_DIR="\$ROOT_DIR/dist"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is not installed or not on PATH." >&2
  exit 1
fi

if ! command -v zip >/dev/null 2>&1; then
  echo "zip is not installed or not on PATH." >&2
  exit 1
fi

if [[ ! -f "\$VERSION_FILE" ]]; then
  echo "VERSION file not found at \$VERSION_FILE" >&2
  exit 1
fi

VERSION="\$(tr -d '[:space:]' < "\$VERSION_FILE")"
if [[ -z "\$VERSION" ]]; then
  echo "Could not determine mod version from VERSION file" >&2
  exit 1
fi

echo "Building ${MOD_NAME} \$VERSION"
dotnet build "\$PROJECT_PATH" -c Release -p:NuGetAudit=false

if [[ ! -f "\$MOD_OUTPUT_DIR/${ASSEMBLY_NAME}.dll" ]]; then
  echo "Expected built DLL not found in \$MOD_OUTPUT_DIR" >&2
  exit 1
fi

if [[ ! -f "\$MOD_OUTPUT_DIR/modinfo.json" ]]; then
  echo "Expected modinfo.json not found in \$MOD_OUTPUT_DIR" >&2
  exit 1
fi

mkdir -p "\$DIST_DIR"
ZIP_PATH="\$DIST_DIR/${MOD_ID}-\$VERSION.zip"
rm -f "\$ZIP_PATH"

(
  cd "\$MOD_OUTPUT_DIR"
  zip -r "\$ZIP_PATH" .
)

echo "Created release package:"
echo "  \$ZIP_PATH"
EOF
  chmod +x "$PROJECT_DIR/release.sh"
fi

echo "Bootstrapped project files in: $PROJECT_DIR"
