---
title: A New Release Pipeline for Fifth
summary: This post describes Fifth's new GitHub Actions release pipeline that builds self-contained compiler binaries across Linux, macOS, and Windows for both .NET 8.0 and .NET 10.0, complete with checksums and smoke tests to ensure every download works out of the box.
authors:
    - Andrew Matthews
date: 2025-11-25
---
# A Practical Release Pipeline for Fifth

We recently finished the Release Packaging workflow for the Fifth compiler. This post walks through what the pipeline does, which platforms it targets, the guarantees it makes, and how to install the new artifacts without surprises.

## What the Workflow Does

- Builds the compiler on Linux, macOS, and Windows runners for both .NET 8.0 and .NET 10.0 targets (12 archives total when all succeed).
- Publishes self-contained binaries (no additional runtime required) plus a `lib/` folder with the reference assemblies that power IDE integration and SDK workflows.
- Runs layout checks and smoke tests for every successful build to ensure the packaged compiler can parse and compile a small Fifth sample before we ship it.
- Aggregates metadata for every build, enforces that we produced six `net8.0` and six `net10.0` archives, and emits a consolidated `SHA256SUMS` manifest so users can verify downloads.
- Creates a GitHub release with the archives, checksum file, and release notes drawn from the pipeline metadata.

## What It Does Not Do (Yet)

- No installers or package-manager feeds. Deliverables are `.tar.gz` (Linux/macOS) and `.zip` (Windows) archives only.
- No delta updates; every release is a full compiler distribution.
- No container images or VS Code extensions are published from this pipeline.
- The `net10.0` builds currently rely on preview SDKs. They exist so early adopters can validate future runtime behavior, but the `net8.0` packages remain the supported baseline.

## Multi-Target Coverage

| Platform | Architectures | Runtimes |
| --- | --- | --- |
| Linux | x64, arm64 | net8.0, net10.0 |
| macOS | x64, arm64 | net8.0, net10.0 |
| Windows | x64, arm64 | net8.0, net10.0 |

Every archive follows the naming pattern `fifth-<version>-<runtime>-<framework>.<tar.gz|zip>`, for example `fifth-0.9.0-test-linux-x64-net8.0.tar.gz`.

## Contents of Each Archive

```
fifth-<version>/
  bin/
    fifth (or fifth.exe on Windows)
  lib/
    *.dll support libraries for IDE tooling and the SDK
  LICENSE
  README.md
  VERSION.txt
```

`bin/fifth` exposes the same CLI that the repository refers to as `fifthc`; the binary has been renamed only to distinguish it from the dotnet project name inside the source tree.

## Download, Verify, and Install

1. **Download an archive and the checksum manifest** (replace variables as needed):

```bash
VERSION=0.9.0-test
RUNTIME=linux-x64
FRAMEWORK=net8.0
curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz"
curl -LO "https://github.com/aabs/fifthlang/releases/download/v${VERSION}/SHA256SUMS"
```

-2. **Verify the checksum**:

- Linux/macOS (GNU coreutils installed):

```bash
sha256sum --ignore-missing -c SHA256SUMS
```

- macOS (BSD `shasum`):

```bash
grep "fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz" SHA256SUMS | shasum -a 256 -c -
```

- Windows PowerShell:

```powershell
$version = "0.9.0-test"
$runtime = "win-x64"
$framework = "net8.0"
Get-FileHash ".\fifth-$version-$runtime-$framework.zip" -Algorithm SHA256
Get-Content .\SHA256SUMS | Select-String "fifth-$version-$runtime-$framework.zip"
```
Compare the computed hash to the line from `SHA256SUMS`. Only proceed when they match.

3. **Extract**:

- Linux/macOS:

```bash
mkdir -p ~/opt/fifth/${VERSION}
tar -xzf fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.tar.gz -C ~/opt/fifth/${VERSION}
```

- Windows:

```powershell
$version = "0.9.0-test"
$runtime = "win-x64"
$framework = "net8.0"
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\fifth\$version" | Out-Null
Expand-Archive -Force -Path ".\fifth-$version-$runtime-$framework.zip" -DestinationPath "$env:USERPROFILE\fifth\$version"
```

4. **Update `PATH` (optional but recommended)**:

```bash
export PATH=~/opt/fifth/${VERSION}/fifth-${VERSION}/bin:$PATH
```

On Windows, add `%USERPROFILE%\fifth\<version>\fifth-<version>\bin` to the user PATH via the System Properties dialog or `setx`.

## Using the Packaged Compiler

Once extracted, invoke the compiler directly from the `bin` directory:

```bash
fifth --source hello.5th --output hello.exe
fifth --command run --source hello.5th --output hello.exe --args "--name Ada"
fifth --command lint --source src/
```

Because the archives are self-contained, no additional .NET runtime is required to run the binary. For larger projects you may still point IDE tooling at the `lib/` folder by referencing its path in `FifthCompilerPath` entries inside your `csproj` files (see `src/Fifth.Sdk/README.md`).

## Expectations for Early Releases

- The smoke tests exercise small programs only; they do not cover every runtime combination or large project scenario. Treat `net10.0` builds as preview-quality until the upstream SDK stabilizes.
- If a package fails to build for a given platform, the publish job will halt. In that situation the release will not appear; we re-tag only after fixing the underlying issue.
- Releases currently target desktop/server environments. Mobile or WASM outputs are out of scope for this workflow.

## Feedback

If you hit issues downloading, verifying, or running each target, please open a GitHub issue with the platform, archive name, and the relevant log output. The Release Packaging workflow surfaces its metadata in the GitHub Actions logs, so referencing the failing run ID helps us reproduce problems quickly.

This incremental pipeline gives us predictable, verifiable deliverables without overstating their scope. As we gather feedback, we will evaluate package-manager feeds, container images, and additional automation around tagging to make releases even smoother.
