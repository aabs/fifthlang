# Quickstart: Release Packaging System

**Audience**: Developers testing the release packaging system locally  
**Time**: 15-30 minutes  
**Prerequisites**: .NET 8.0 SDK, Java 17+, bash/PowerShell

---

## Overview

This quickstart guide walks you through building and testing Fifth compiler release packages on your local machine before running the full CI/CD pipeline.

## Quick Navigation

- [Build a Single Package](#build-a-single-package) - Create one platform-specific package
- [Run Smoke Tests](#run-smoke-tests) - Validate package functionality
- [Test Full Release Workflow](#test-full-release-workflow) - Simulate CI pipeline locally
- [Verify Package Contents](#verify-package-contents) - Inspect package structure
- [Troubleshooting](#troubleshooting) - Common issues and solutions

---

## Prerequisites

### Install Dependencies

**macOS**:
```bash
# .NET SDK 8.0 (check global.json for exact version)
brew install --cask dotnet-sdk

# .NET 10.0 SDK (optional, for testing multi-framework builds)
# Install preview if final not available: https://dotnet.microsoft.com/download/dotnet/10.0
# Note: Installing 10.0 SDK alongside 8.0 does NOT interfere with global.json pinning

# Java 17+ (for ANTLR)
brew install openjdk@17

# Verify versions
dotnet --version     # Should be 8.0.x (from global.json)
dotnet --list-sdks   # Should show both 8.0 and 10.0 SDKs if installed
java -version        # Should be 17+
```

**Linux (Ubuntu/Debian)**:
```bash
# .NET SDK 8.0 (pinned in global.json)
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.414

# .NET 10.0 SDK (optional, for testing multi-framework builds)
# Use --quality preview for preview releases if final not available
./dotnet-install.sh --channel 10.0 --quality preview
# Note: Multiple SDKs can coexist; global.json pins which one is used for project operations

# Java 17+
sudo apt install openjdk-17-jdk

# Verify versions
dotnet --version     # Should be 8.0.414
dotnet --list-sdks   # Should show both 8.0 and 10.0 SDKs if installed
java -version
```

**Windows (PowerShell)**:
```powershell
# Install .NET SDK 8.0 from https://dotnet.microsoft.com/download
# Install .NET SDK 10.0 (optional) - can install preview version if final not available
# Both SDKs can coexist; global.json determines which is used for project operations

# Install Java 17+ from https://adoptium.net/

# Verify versions
dotnet --version     # Should be 8.0.x
dotnet --list-sdks   # Shows all installed SDKs
java -version
```

**SDK Version Clarification**:
- The `global.json` file pins SDK 8.0.414 for build consistency
- You can install .NET 10.0 SDK (preview or final) alongside 8.0 without conflict
- When building for `net10.0` target framework, the 10.0 SDK is automatically used
- When building for `net8.0` target framework, the 8.0 SDK (from global.json) is used
- This multi-targeting approach allows creating packages for both frameworks from the same build environment

### Clone Repository

```bash
git clone https://github.com/aabs/fifthlang.git
cd fifthlang
git checkout 014-release-packaging-system
```

---

## Build a Single Package

### Step 1: Extract Version

```bash
# Get current version from git tags
./scripts/release/version-info.sh --format json

# If no tags exist (development), use development version
VERSION="0.9.0-dev"
```

### Step 2: Build for Your Platform

**Linux x64**:
```bash
./scripts/build/build-release.sh \
  --version "$VERSION" \
  --runtime linux-x64 \
  --framework net8.0 \
  --output-dir ./dist

# Output: ./dist/fifth-0.9.0-dev-linux-x64-net8.0.tar.gz
```

**macOS Apple Silicon**:
```bash
./scripts/build/build-release.sh \
  --version "$VERSION" \
  --runtime osx-arm64 \
  --framework net8.0 \
  --output-dir ./dist

# Output: ./dist/fifth-0.9.0-dev-osx-arm64-net8.0.tar.gz
```

**Windows x64** (PowerShell):
```powershell
./scripts/build/build-release.ps1 `
  -Version "$VERSION" `
  -Runtime "win-x64" `
  -Framework "net8.0" `
  -OutputDir "./dist"

# Output: ./dist/fifth-0.9.0-dev-win-x64-net8.0.zip
```

### Step 3: Verify Build Output

```bash
# Check package was created
ls -lh ./dist/fifth-*.tar.gz  # or *.zip on Windows

# Verify package size (should be < 150MB)
du -h ./dist/fifth-*.tar.gz
```

**Expected Output**:
```
-rw-r--r--  1 user  staff   89M Nov 23 18:45 fifth-0.9.0-dev-linux-x64-net8.0.tar.gz
```

---

## Run Smoke Tests

### Step 1: Prepare Test Environment

```bash
# Create temporary test directory
mkdir -p /tmp/fifth-smoke-test
cd /tmp/fifth-smoke-test

# Copy package to test directory
cp ~/fifthlang/dist/fifth-*.tar.gz ./
```

### Step 2: Run Smoke Test Script

```bash
# Run full smoke test suite
~/fifthlang/scripts/test/smoke-test.sh \
  --package-path ./fifth-*.tar.gz \
  --test-dir ./smoke-test-run \
  --verbose

# Exit code 0 = all tests passed
echo $?
```

**Expected Output**:
```
[INFO] Extracting package: fifth-0.9.0-dev-linux-x64-net8.0.tar.gz
[INFO] Package extracted to /tmp/fifth-smoke-test/smoke-test-run/
[INFO] Running test: version_check
[PASS] version_check (1.2s)
[INFO] Running test: compile_hello_world
[PASS] compile_hello_world (8.5s)
[INFO] Running test: execute_hello_world
[PASS] execute_hello_world (1.5s)
[INFO] Running test: compile_with_kg_features
[PASS] compile_with_kg_features (0.8s)
[INFO] Running test: parser_grammar_loaded
[PASS] parser_grammar_loaded (0.3s)
[INFO] All tests passed (5/5) in 12.3s
```

### Step 3: Inspect Test Results

```bash
# View JSON output (if saved to file)
cat smoke-test-results.json | jq .

# Check extracted package contents
ls -la ./smoke-test-run/fifth-0.9.0-dev/
```

---

## Test Full Release Workflow

Simulate the complete CI/CD pipeline locally (builds multiple platforms):

### Step 1: Build All Platforms (Your OS Only)

**Linux**:
```bash
# Build for all Linux architectures
for ARCH in x64 arm64; do
  for FRAMEWORK in net8.0 net10.0; do
    ./scripts/build/build-release.sh \
      --version "$VERSION" \
      --runtime "linux-$ARCH" \
      --framework "$FRAMEWORK" \
      --output-dir ./dist
  done
done

# Output: 4 packages (2 arch × 2 frameworks)
ls ./dist/fifth-*.tar.gz
```

**macOS**:
```bash
# Build for both Intel and Apple Silicon
for ARCH in x64 arm64; do
  for FRAMEWORK in net8.0 net10.0; do
    ./scripts/build/build-release.sh \
      --version "$VERSION" \
      --runtime "osx-$ARCH" \
      --framework "$FRAMEWORK" \
      --output-dir ./dist
  done
done

# Output: 4 packages
ls ./dist/fifth-*.tar.gz
```

**Windows** (PowerShell):
```powershell
# Build for both x64 and ARM64
@("x64", "arm64") | ForEach-Object {
    $arch = $_
    @("net8.0", "net10.0") | ForEach-Object {
        $framework = $_
        ./scripts/build/build-release.ps1 `
            -Version $VERSION `
            -Runtime "win-$arch" `
            -Framework $framework `
            -OutputDir "./dist"
    }
}

# Output: 4 packages
Get-ChildItem ./dist/fifth-*.zip
```

### Step 2: Generate Checksums

```bash
# Generate SHA256SUMS manifest
./scripts/build/generate-checksums.sh \
  --package-dir ./dist \
  --output-file ./dist/SHA256SUMS

# View checksums
cat ./dist/SHA256SUMS
```

**Expected Output**:
```
# Fifth Language Compiler v0.9.0-dev - SHA256 Checksums
# Generated: 2025-11-23T19:00:00Z

e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855  fifth-0.9.0-dev-linux-x64-net8.0.tar.gz
5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8  fifth-0.9.0-dev-linux-x64-net10.0.tar.gz
...
```

### Step 3: Verify All Packages

```bash
# Run verification on all packages
for PKG in ./dist/fifth-*.tar.gz ./dist/fifth-*.zip; do
  echo "Verifying $PKG..."
  ./scripts/test/verify-package.sh --package-path "$PKG"
done
```

### Step 4: Test Package Installation

```bash
# Extract package to temporary location
INSTALL_DIR="/tmp/fifth-install-test"
mkdir -p "$INSTALL_DIR"
tar -xzf ./dist/fifth-*.tar.gz -C "$INSTALL_DIR"

# Test compiler execution
"$INSTALL_DIR"/fifth-*/bin/fifth --version

# Test simple compilation
cd "$INSTALL_DIR"
cat > hello.5th <<EOF
main(): int {
    std.print("Hello from Fifth!");
    return 0;
}
EOF

./fifth-*/bin/fifth compile hello.5th -o hello.exe
dotnet hello.exe
```

**Expected Output**:
```
Fifth Language Compiler v0.9.0-dev
Compilation successful: hello.exe
Hello from Fifth!
```

---

## Verify Package Contents

### Extract and Inspect

**Linux/macOS**:
```bash
# Extract package
tar -tzf ./dist/fifth-*.tar.gz | head -n 20

# Extract to directory
mkdir -p /tmp/inspect
tar -xzf ./dist/fifth-*.tar.gz -C /tmp/inspect

# List contents
tree /tmp/inspect/fifth-*
# or
find /tmp/inspect/fifth-* -type f | sort
```

**Windows** (PowerShell):
```powershell
# List archive contents
Expand-Archive -Path ./dist/fifth-*.zip -DestinationPath C:\temp\inspect

# List contents
Get-ChildItem C:\temp\inspect\fifth-* -Recurse
```

### Expected Structure

```
fifth-0.9.0-dev/
├── bin/
│   └── fifth                 # Executable (or fifth.exe on Windows)
├── lib/
│   ├── fifth.dll
│   ├── Antlr4.Runtime.Standard.dll
│   ├── ast-model.dll
│   ├── parser.dll
│   ├── compiler.dll
│   └── [other dependencies]
├── LICENSE
├── README.md
└── VERSION.txt
```

### Verify File Permissions (Linux/macOS)

```bash
# Check executable has +x permission
ls -l /tmp/inspect/fifth-*/bin/fifth

# Should show: -rwxr-xr-x (executable bit set)
```

### Verify Package Size

```bash
# Check size is within limits (< 150MB)
du -sh ./dist/fifth-*.tar.gz

# Breakdown by directory
du -h /tmp/inspect/fifth-*/*
```

**Expected Sizes**:
- Total package: 80-100 MB (compressed)
- Extracted: 200-250 MB
- Largest: `lib/` directory (~180 MB, .NET runtime)

---

## Troubleshooting

### Build Failures

**Error**: `dotnet: command not found`
```bash
# Verify .NET SDK installed
which dotnet

# If missing, install via package manager or from https://dot.net
```

**Error**: `java: command not found`
```bash
# Verify Java installed
which java
java -version

# If missing, install Java 17+ from package manager
```

**Error**: `ANTLR grammar compilation failed`
```bash
# Check ANTLR jar exists
ls src/parser/tools/antlr-4.8-complete.jar

# Verify Java can run jar
java -jar src/parser/tools/antlr-4.8-complete.jar
```

**Error**: `Package size exceeds 150MB`
```bash
# Check what's taking space
tar -tzf ./dist/fifth-*.tar.gz | xargs -I {} sh -c 'echo $(tar -xzOf ./dist/fifth-*.tar.gz {} | wc -c) {}'

# Common causes:
# - Debug symbols included (check --configuration Release)
# - Test assemblies included (check publish filters)
# - Duplicate dependencies (check dependency tree)
```

### Smoke Test Failures

**Error**: `Executable not found`
```bash
# Check package structure
tar -tzf ./dist/fifth-*.tar.gz | grep bin/fifth

# Verify extraction successful
ls /tmp/smoke-test/smoke-test-run/fifth-*/bin/
```

**Error**: `Permission denied` (Linux/macOS)
```bash
# Check executable permissions
ls -l /tmp/smoke-test/smoke-test-run/fifth-*/bin/fifth

# Fix if needed
chmod +x /tmp/smoke-test/smoke-test-run/fifth-*/bin/fifth
```

**Error**: `Compilation failed: ANTLR runtime not found`
```bash
# Check Antlr4.Runtime.Standard.dll included
tar -tzf ./dist/fifth-*.tar.gz | grep Antlr4.Runtime.Standard.dll

# Verify in extracted package
ls /tmp/smoke-test/smoke-test-run/fifth-*/lib/Antlr4.Runtime.Standard.dll
```

**Error**: `Test timeout exceeded`
```bash
# Run with increased timeout
./scripts/test/smoke-test.sh \
  --package-path ./fifth-*.tar.gz \
  --test-dir ./smoke-test-run \
  --timeout 600  # 10 minutes

# Check for performance issues in build
dotnet --info  # Verify .NET runtime version
```

### Package Verification Failures

**Error**: `Archive extractable check failed`
```bash
# Test extraction manually
tar -xzf ./dist/fifth-*.tar.gz -C /tmp/extract-test

# Check for corruption
tar -tzf ./dist/fifth-*.tar.gz > /dev/null
echo $?  # Should be 0
```

**Error**: `Missing LICENSE file`
```bash
# Check LICENSE in archive
tar -tzf ./dist/fifth-*.tar.gz | grep LICENSE

# Verify source has LICENSE
ls -l LICENSE

# Rebuild package if missing
```

### Windows-Specific Issues

**Error**: `Execution policy prevents running scripts`
```powershell
# Check current policy
Get-ExecutionPolicy

# Allow scripts (for current user)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or run with bypass
PowerShell -ExecutionPolicy Bypass -File ./scripts/build/build-release.ps1
```

**Error**: `Path too long` (Windows 260 character limit)
```powershell
# Enable long paths in Windows 10+
# Run as Administrator:
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force

# Or use shorter output directory
./scripts/build/build-release.ps1 -OutputDir "C:\tmp"
```

---

## Next Steps

After successful local testing:

1. **Commit Changes**: Push scripts to feature branch
   ```bash
   git add scripts/ .github/workflows/
   git commit -m "Add release packaging scripts"
   git push origin 014-release-packaging-system
   ```

2. **Test in CI**: Create PR to trigger CI workflow
   ```bash
   gh pr create --title "Release Packaging System" --body "Test CI integration"
   ```

3. **Manual Workflow Test**: Run workflow dispatch with dry_run=true
   - Navigate to Actions tab in GitHub
   - Select "Release Packaging" workflow
   - Click "Run workflow", set dry_run=true

4. **Create Test Release**: Tag and push to test full pipeline
   ```bash
   git tag v0.9.0-test
   git push origin v0.9.0-test
   # Monitor workflow at https://github.com/aabs/fifthlang/actions
   ```

5. **Verify Release**: Check GitHub Releases page
   - All 12 packages present
   - Checksums file attached
   - Release notes formatted correctly

---

## Additional Resources

- **Full Specification**: `specs/014-release-packaging-system/spec.md`
- **Build Scripts**: `specs/014-release-packaging-system/contracts/build-scripts.md`
- **Test Contracts**: `specs/014-release-packaging-system/contracts/test-scripts.md`
- **CI Workflow**: `specs/014-release-packaging-system/contracts/github-actions-workflow.md`
- **Data Model**: `specs/014-release-packaging-system/data-model.md`

## Feedback

If you encounter issues not covered in this guide:

1. Check existing issues: https://github.com/aabs/fifthlang/issues
2. Open new issue with "release-packaging" label
3. Include: OS, .NET version, error messages, build logs
