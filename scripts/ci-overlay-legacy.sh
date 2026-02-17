#!/usr/bin/env bash
set -euo pipefail

# CI overlay helper for legacy emitter files
# Usage (CI):
#   1) Checkout PR branch in default workspace (actions/checkout)
#   2) Checkout authoritative legacy ref into 'legacy-src':
#        - uses: actions/checkout@v4
#          with:
#            ref: 'master'
#            path: 'legacy-src'
#   3) Run this script:
#        ./scripts/ci-overlay-legacy.sh legacy-src src/code_generator src/legacy-emitters
# The script copies the subtree from the legacy checkout into the target overlay path.

SRC_CHECKOUT_DIR=${1:-legacy-src}
SUBTREE_PATH=${2:-src/code_generator}
TARGET_DIR=${3:-src/legacy-emitters}

if [ ! -d "$SRC_CHECKOUT_DIR" ]; then
  echo "Error: source checkout directory '$SRC_CHECKOUT_DIR' not found."
  echo "CI must checkout the authoritative legacy ref into '$SRC_CHECKOUT_DIR' before running this script."
  exit 1
fi

if [ ! -d "$SRC_CHECKOUT_DIR/$SUBTREE_PATH" ]; then
  echo "Error: subtree path '$SRC_CHECKOUT_DIR/$SUBTREE_PATH' not present in legacy checkout."
  exit 1
fi

echo "Overlaying '$SRC_CHECKOUT_DIR/$SUBTREE_PATH' -> '$TARGET_DIR'"
mkdir -p "$TARGET_DIR"
rsync -a --delete "$SRC_CHECKOUT_DIR/$SUBTREE_PATH/" "$TARGET_DIR/"

echo "Overlay complete. Files now available at: $TARGET_DIR"
