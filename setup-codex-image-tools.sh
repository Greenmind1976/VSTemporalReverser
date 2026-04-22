#!/usr/bin/env bash
set -euo pipefail

echo "setup-codex-image-tools.sh is deprecated. Redirecting to setup-image-tools.sh"
"$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/setup-image-tools.sh"
