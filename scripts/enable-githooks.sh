#!/usr/bin/env bash
set -euo pipefail
repo_root="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )/.." &> /dev/null && pwd )"
cd "$repo_root"
git config --local core.hooksPath .githooks
echo "✓ Enabled .githooks for this repo"
git config --get core.hooksPath
