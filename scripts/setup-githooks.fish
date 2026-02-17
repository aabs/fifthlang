#!/usr/bin/env fish
set -e

echo "Setting up Git hooks..."
set repo_root (git rev-parse --show-toplevel)
mkdir -p "$repo_root/.git/hooks"

cp "$repo_root/.githooks/pre-commit" "$repo_root/.git/hooks/pre-commit"
chmod +x "$repo_root/.git/hooks/pre-commit"
echo "Git pre-commit hook installed."

cp "$repo_root/.githooks/pre-push" "$repo_root/.git/hooks/pre-push"
chmod +x "$repo_root/.git/hooks/pre-push"
echo "Git pre-push hook installed."

echo "Done."