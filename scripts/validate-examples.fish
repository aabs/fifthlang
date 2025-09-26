#!/usr/bin/env fish
# Validate all .5th samples locally using the validate-examples tool
set repo_root (pwd)
# Forward all args to the dotnet tool; use --include-negatives to include intentionally-invalid samples for debugging
dotnet run --project src/tools/validate-examples/validate-examples.csproj --configuration Release $argv
