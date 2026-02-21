# Publish Fifth compiler tool and SDK

Publishing and consuming the Fifth compiler as a global .NET tool and the MSBuild SDK package.

## Overview

The Fifth compiler is published as two NuGet packages:
- **Fifth.Compiler.Tool** — A global dotnet CLI tool (`fifthc` command)
- **Fifth.Sdk** — An MSBuild SDK for building `.5thproj` projects

The release workflow automatically publishes both packages to [nuget.org](https://www.nuget.org) when you push a semantic version tag (e.g., `v0.2.0`).

## Prerequisites

- .NET SDK 8.0.x (per global.json)
- For automatic publication: `NUGET_PUBLISH` secret configured in GitHub (NuGet API key)

## Versioning

- Use semantic versioning for both packages (e.g., `0.2.0`, `1.0.0-beta`)
- Keep `Fifth.Sdk` and `Fifth.Compiler.Tool` versions synchronized
- Update version in:
  - `src/Fifth.Sdk/Fifth.Sdk.csproj` (`<Version>`)
  - `src/compiler/compiler.csproj` (`<Version>`)
  - any sample `global.json` that pins the SDK version

## Publishing (automated)

**Trigger:** Push a tag matching `v[0-9]+.[0-9]+.[0-9]+` or `v[0-9]+.[0-9]+.[0-9]+-*`

```bash
git tag v0.2.0
git push origin v0.2.0
```

The GitHub Actions workflow (`.github/workflows/release.yml`) will:
1. Checkout the repository
2. Build and test the solution
3. Pack both SDK and compiler tool
4. Publish to nuget.org (skipped if NUGET_PUBLISH secret is missing)
5. Create a GitHub release with installation instructions

## Publishing (manual)

If you need to publish manually:

```bash
# Build and restore (if not done already)
dotnet restore fifthlang.sln
dotnet build fifthlang.sln -c Release

# Pack both projects
VERSION="0.2.0"
dotnet pack src/Fifth.Sdk/Fifth.Sdk.csproj -c Release /p:Version="$VERSION"
dotnet pack src/compiler/compiler.csproj -c Release /p:Version="$VERSION"

# Push to nuget.org
NUGET_API_KEY="<your-api-key>"
dotnet nuget push "src/Fifth.Sdk/bin/Release/Fifth.Sdk.$VERSION.nupkg" \
  --api-key "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json
dotnet nuget push "src/compiler/bin/Release/Fifth.Compiler.Tool.$VERSION.nupkg" \
  --api-key "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json
```

## Consumer: Install the compiler tool

```bash
# Install globally (requires .NET runtime on the system)
dotnet tool install -g Fifth.Compiler.Tool

# Or install a specific version
dotnet tool install -g Fifth.Compiler.Tool --version 0.2.0

# Update to latest version
dotnet tool update -g Fifth.Compiler.Tool

# Use the tool
fifthc --help
```

## Consumer: Use the MSBuild SDK

### In a `.5thproj` file

```xml
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <FifthSource Include="src/**/*.5th" />
  </ItemGroup>
</Project>
```

### Pin the SDK version in `global.json`

```json
{
  "msbuild-sdks": {
    "Fifth.Sdk": "0.2.0"
  }
}
```

## Dry-run testing

To test the release workflow without publishing:

```bash
# Trigger workflow_dispatch with dry_run=true
gh workflow run release.yml -f dry_run=true
```

This will:
- Build and test the solution
- Pack both projects
- Skip publishing to NuGet
- Display what would have been published
