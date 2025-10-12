#!/usr/bin/env bash
set -euo pipefail

# Compare generator output with committed files under src/ast-generated/
# Usage: scripts/check-generated.sh

tmp_dir=$(mktemp -d)
cleanup() { rm -rf "$tmp_dir"; }
trap cleanup EXIT

echo "Running AST generator into temporary folder: $tmp_dir"

if command -v just >/dev/null 2>&1; then
  echo "Using just run-generator"
  just run-generator --folder "$tmp_dir"
else
  echo "Running generator via dotnet"
  dotnet run --project src/ast_generator/ast_generator.csproj -- --folder "$tmp_dir"
fi

echo "Comparing generated outputs..."
if diff -ru src/ast-generated "$tmp_dir" > "$tmp_dir/diff.txt"; then
  echo "No differences detected between committed generated files and generator output." >&2
  exit 0
else
  echo "Generated code differs from committed files. Please do not hand-edit generated files." >&2
  echo "Run 'just run-generator' to update generated files after changing metamodel/templates." >&2
  echo "Diff (first 200 lines):" >&2
  head -n 200 "$tmp_dir/diff.txt" >&2 || true
  exit 1
fi
