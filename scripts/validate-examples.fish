#!/usr/bin/env fish
# Validate all .5th samples locally using the validate-examples tool
set repo_root (pwd)
dotnet run --project src/tools/validate-examples/validate-examples.csproj --configuration Release
