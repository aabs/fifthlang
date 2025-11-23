# GitHub Actions Workflow Contract

## Overview

This document defines the interface contract for the GitHub Actions release workflow (`release.yml`). The workflow orchestrates building, testing, and publishing release packages across all platform/framework combinations.

---

## Workflow Trigger Contract

### Trigger Events

**Primary Trigger**: Push to `master` branch with version tags
```yaml
on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+'        # e.g., v1.2.3
      - 'v[0-9]+.[0-9]+.[0-9]+-*'      # e.g., v1.2.3-beta.1
```

**Secondary Trigger**: Manual workflow dispatch (for testing)
```yaml
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to build (e.g., 1.2.3)'
        required: true
        type: string
      dry_run:
        description: 'Dry run (do not publish)'
        required: false
        type: boolean
        default: false
```

### Trigger Behavior

1. **Automatic Release**: Tag push triggers full build+test+publish pipeline
2. **Manual Release**: Workflow dispatch allows testing without publishing
3. **Pre-release Detection**: Tags with suffixes (-alpha, -beta, -rc) marked as pre-release
4. **Version Validation**: Only tags matching SemVer pattern trigger workflow

---

## Concurrency Control Contract

**Policy (FR-032)**: When multiple commits trigger builds concurrently, the system cancels older pending/in-progress builds in favor of the most recent commit.

**Implementation**:
```yaml
concurrency:
  group: release-${{ github.ref }}
  cancel-in-progress: true
```

**Behavior**:
- When new commit pushed to master → cancel any in-progress builds for same ref
- Ensures CI resources focus on latest code
- Prevents queue buildup from rapid commits
- Tag-based releases are isolated by unique ref (no cancellation)
- Manual workflow dispatches use unique run ID (no cancellation between manual runs)

**Rationale**: Prevents wasted CI minutes and storage on stale commits while master is actively developed. Aligns with modern CI best practices for trunk-based development.

---

## Build Matrix Configuration

### Matrix Dimensions

```yaml
strategy:
  fail-fast: false  # Allow all jobs to complete for diagnostics
  # Note: All-or-nothing policy (FR-030) enforced in publish job
  matrix:
    include:
      # Linux x64
      - os: ubuntu-latest
        platform: linux
        arch: x64
        runtime: linux-x64
        framework: net8.0
        archive_format: tar.gz
      
      - os: ubuntu-latest
        platform: linux
        arch: x64
        runtime: linux-x64
        framework: net10.0
        archive_format: tar.gz
      
      # Linux ARM64
      - os: ubuntu-latest
        platform: linux
        arch: arm64
        runtime: linux-arm64
        framework: net8.0
        archive_format: tar.gz
      
      - os: ubuntu-latest
        platform: linux
        arch: arm64
        runtime: linux-arm64
        framework: net10.0
        archive_format: tar.gz
      
      # macOS Intel
      - os: macos-latest
        platform: macos
        arch: x64
        runtime: osx-x64
        framework: net8.0
        archive_format: tar.gz
      
      - os: macos-latest
        platform: macos
        arch: x64
        runtime: osx-x64
        framework: net10.0
        archive_format: tar.gz
      
      # macOS Apple Silicon
      - os: macos-latest
        platform: macos
        arch: arm64
        runtime: osx-arm64
        framework: net8.0
        archive_format: tar.gz
      
      - os: macos-latest
        platform: macos
        arch: arm64
        runtime: osx-arm64
        framework: net10.0
        archive_format: tar.gz
      
      # Windows x64
      - os: windows-latest
        platform: windows
        arch: x64
        runtime: win-x64
        framework: net8.0
        archive_format: zip
      
      - os: windows-latest
        platform: windows
        arch: x64
        runtime: win-x64
        framework: net10.0
        archive_format: zip
      
      # Windows ARM64
      - os: windows-latest
        platform: windows
        arch: arm64
        runtime: win-arm64
        framework: net8.0
        archive_format: zip
      
      - os: windows-latest
        platform: windows
        arch: arm64
        runtime: win-arm64
        framework: net10.0
        archive_format: zip
```

### Matrix Variables

- `os`: GitHub Actions runner OS (ubuntu-latest, macos-latest, windows-latest)
- `platform`: User-facing platform name (linux, macos, windows)
- `arch`: Architecture (x64, arm64)
- `runtime`: .NET runtime identifier (linux-x64, osx-arm64, win-x64, etc.)
- `framework`: Target framework (net8.0, net10.0)
- `archive_format`: Archive format (tar.gz for Unix, zip for Windows)

### All-or-Nothing Build Policy (FR-030)

**Critical Policy**: The workflow implements an all-or-nothing build policy. If ANY of the 12 platform/framework builds fail, the entire release is aborted and NO packages are published.

**Enforcement Mechanism**:
- `fail-fast: false` allows all builds to run for complete diagnostics
- Publish job uses `needs: [build]` with `if: ${{ success() }}` condition
- Success condition requires ALL build jobs to succeed
- Partial success (e.g., 11/12 builds) triggers complete abort

**Rationale**: Ensures users never receive incomplete releases. A missing platform could indicate systemic issues that affect all platforms.

### .NET 10.0 SDK Graceful Degradation (FR-033)

**Policy**: When .NET 10.0 SDK is unavailable in the GitHub Actions environment, the system builds only .NET 8.0 packages (6 total), logs warnings, and annotates the unavailability in release notes.

**Detection Mechanism**: Each build job checks for framework SDK availability before proceeding. If .NET 10.0 SDK is missing, those jobs fail gracefully, and the publish job detects partial framework coverage.

**User Impact**: Users still receive functional .NET 8.0 packages while .NET 10.0 support is pending SDK availability.

---

## Job Contract: build

**Purpose**: Build release package for one platform/framework combination.

### Inputs (from matrix)
- `matrix.os`: Runner OS
- `matrix.runtime`: Runtime identifier
- `matrix.framework`: Framework version
- `matrix.archive_format`: Archive format

### Outputs
- `package_path`: Artifact path for generated package
- `package_name`: Package filename
- `package_size`: Package size in bytes

### Steps

1. **Checkout**: Clone repository with full git history (for version extraction)
   ```yaml
   - uses: actions/checkout@v4
     with:
       fetch-depth: 0  # Need tags for version
   ```

2. **Setup .NET SDK**: Install required .NET SDK versions
   ```yaml
   # Install pinned SDK from global.json (8.0.414)
   - uses: actions/setup-dotnet@v4
     with:
       global-json-file: global.json  # Respects pinned version for build consistency
   
   # Install .NET 10.0 SDK (preview or final) for net10.0 target builds
   # This allows targeting net10.0 framework even though global.json pins 8.0
   - name: Setup .NET 10.0 SDK
     if: matrix.framework == 'net10.0'
     uses: actions/setup-dotnet@v4
     with:
       dotnet-version: '10.0.x'  # Uses latest available (preview or final)
       dotnet-quality: 'preview'  # Allows preview releases if final not available
   ```
   
   **Clarification**: The `global.json` file pins the SDK used for project operations (8.0.414), but we can install additional SDKs (like .NET 10.0) to support multi-targeting. The `dotnet publish --framework net10.0` command will use the 10.0 SDK even though 8.0 is pinned. This is standard .NET multi-targeting behavior.

3. **Setup Java**: Install Java 17+ for ANTLR (if grammar regeneration needed)
   ```yaml
   - uses: actions/setup-java@v4
     with:
       java-version: '17'
       distribution: 'temurin'
   ```

4. **Check Framework SDK Availability**: Detect if target framework SDK is available (FR-033)
   ```yaml
   - name: Check framework SDK availability
     id: check_framework
     continue-on-error: true
     run: |
       if [ "${{ matrix.framework }}" == "net10.0" ]; then
         SDK_VERSION=$(dotnet --list-sdks | grep "^10\." | head -n1 | awk '{print $1}')
         if [ -z "$SDK_VERSION" ]; then
           echo "⚠️  .NET 10.0 SDK not available - this job will fail gracefully" >&2
           echo "SDK_AVAILABLE=false" >> $GITHUB_OUTPUT
           echo "SDK_VERSION=unavailable" >> $GITHUB_OUTPUT
           exit 1  # Fail this job gracefully
         fi
         echo "✓ .NET 10.0 SDK available: $SDK_VERSION" >&2
         if [[ "$SDK_VERSION" == *"preview"* ]] || [[ "$SDK_VERSION" == *"rc"* ]]; then
           echo "ℹ️  Using preview SDK: $SDK_VERSION" >&2
           echo "IS_PREVIEW=true" >> $GITHUB_OUTPUT
         else
           echo "IS_PREVIEW=false" >> $GITHUB_OUTPUT
         fi
         echo "SDK_AVAILABLE=true" >> $GITHUB_OUTPUT
         echo "SDK_VERSION=$SDK_VERSION" >> $GITHUB_OUTPUT
       else
         echo "SDK_AVAILABLE=true" >> $GITHUB_OUTPUT
         echo "IS_PREVIEW=false" >> $GITHUB_OUTPUT
       fi
   ```
   
   **Notes**: 
   - Detects both preview and final .NET 10.0 SDKs
   - Outputs `IS_PREVIEW=true` if using preview SDK (for release notes annotation)
   - Captures SDK version for transparency in release notes
   - Gracefully fails net10.0 jobs if no SDK available (degradation per FR-033)

5. **Extract Version**: Get version from git tag or input
   ```yaml
   - name: Extract version
     id: version
     run: |
       VERSION=$(./scripts/release/version-info.sh --format text)
       echo "version=$VERSION" >> $GITHUB_OUTPUT
   ```

6. **Build Package**: Run build-release.sh script
   ```yaml
   - name: Build release package
     run: |
       ./scripts/build/build-release.sh \
         --version ${{ steps.version.outputs.version }} \
         --runtime ${{ matrix.runtime }} \
         --framework ${{ matrix.framework }} \
         --output-dir ./dist
   ```

7. **Verify Package**: Run verification before tests (size check is soft limit per FR-031)
   ```yaml
   - name: Verify package structure
     run: |
       ./scripts/test/verify-package.sh \
         --package-path ./dist/fifth-*.tar.gz
   ```

8. **Smoke Test**: Run functional tests on package
   ```yaml
   - name: Run smoke tests
     run: |
       ./scripts/test/smoke-test.sh \
         --package-path ./dist/fifth-*.tar.gz \
         --test-dir /tmp/smoke-test
   ```

9. **Upload Artifact**: Save package for publish job
   ```yaml
   - uses: actions/upload-artifact@v4
     with:
       name: package-${{ matrix.runtime }}-${{ matrix.framework }}
       path: ./dist/fifth-*.tar.gz
       retention-days: 7
   ```

### Failure Handling

- **Build Failure**: Job fails, other matrix jobs continue (fail-fast: false). All-or-nothing policy (FR-030) ensures entire release aborts if any job fails.
- **Test Failure**: Job fails, package not uploaded. Triggers release abort per all-or-nothing policy.
- **Verification Failure**: Job fails, no smoke tests run. Note: Package size warnings (>150MB) are non-blocking per FR-031.
- **Framework SDK Unavailable**: Job fails gracefully, allowing .NET 8.0-only release per FR-033.

---

## Job Contract: publish

**Purpose**: Aggregate all packages and publish to GitHub Releases.

### Dependencies
- Requires: All `build` jobs complete successfully (enforces all-or-nothing policy per FR-030)
- Runs: Only on success of all builds (unless dry run)
- Note: If .NET 10.0 SDK unavailable, expects only 6 packages (.NET 8.0) per FR-033

### Inputs
- Artifacts from all build jobs (12 packages)
- `github.ref`: Git tag reference
- `workflow_dispatch.inputs.dry_run`: Skip actual publishing (optional)

### Outputs
- `release_id`: GitHub Release ID
- `release_url`: GitHub Release URL

### Steps

1. **Download All Artifacts**: Retrieve packages from build jobs
   ```yaml
   - uses: actions/download-artifact@v4
     with:
       path: ./dist
   ```

2. **Check Framework Coverage & SDK Versions**: Detect partial framework coverage and preview SDK usage (FR-033)
   ```yaml
   - name: Check framework coverage and SDK versions
     id: check_coverage
     run: |
       NET8_COUNT=$(find ./dist -name "*-net8.0.*" | wc -l)
       NET10_COUNT=$(find ./dist -name "*-net10.0.*" | wc -l)
       
       echo "net8_packages=$NET8_COUNT" >> $GITHUB_OUTPUT
       echo "net10_packages=$NET10_COUNT" >> $GITHUB_OUTPUT
       
       # Check if .NET 10.0 packages are missing
       if [ "$NET10_COUNT" -eq 0 ]; then
         echo "⚠️  .NET 10.0 packages unavailable - publishing .NET 8.0 only" | tee -a $GITHUB_STEP_SUMMARY
         echo "framework_warning=.NET 10.0 SDK was unavailable; only .NET 8.0 packages included" >> $GITHUB_OUTPUT
       else
         # Check if any net10.0 packages were built with preview SDK
         # This info should be in build job outputs or artifact metadata
         NET10_SDK_VERSION=$(dotnet --list-sdks | grep "^10\." | head -n1 | awk '{print $1}')
         if [[ "$NET10_SDK_VERSION" == *"preview"* ]] || [[ "$NET10_SDK_VERSION" == *"rc"* ]]; then
           echo "ℹ️  .NET 10.0 packages built with preview SDK: $NET10_SDK_VERSION" | tee -a $GITHUB_STEP_SUMMARY
           echo "preview_warning=⚠️ .NET 10.0 packages built with preview SDK $NET10_SDK_VERSION (not final release)" >> $GITHUB_OUTPUT
         else
           echo "✓ .NET 10.0 packages built with final SDK: $NET10_SDK_VERSION" | tee -a $GITHUB_STEP_SUMMARY
           echo "preview_warning=" >> $GITHUB_OUTPUT
         fi
       fi
   ```
   
   **Notes**:
   - Detects whether .NET 10.0 packages are present (FR-033 graceful degradation)
   - Identifies preview SDK usage for transparency
   - Outputs warnings for both missing frameworks and preview SDK usage
   - These warnings will be included in release notes

3. **Check Package Sizes**: Log warnings for packages exceeding soft limit (FR-031)
   ```yaml
   - name: Check package sizes
     run: |
       for pkg in ./dist/**/*.{tar.gz,zip}; do
         SIZE_MB=$(du -m "$pkg" | cut -f1)
         if [ "$SIZE_MB" -gt 150 ]; then
           echo "⚠️  Package $pkg exceeds 150MB soft limit: ${SIZE_MB}MB" | tee -a $GITHUB_STEP_SUMMARY
         fi
       done
   ```

4. **Generate Checksums**: Create SHA256SUMS manifest
   ```yaml
   - name: Generate checksums
     run: |
       ./scripts/build/generate-checksums.sh \
         --package-dir ./dist \
         --output-file ./dist/SHA256SUMS
   ```

5. **Generate Release Notes**: Extract changelog from commits and add framework/SDK warnings
   ```yaml
   - name: Generate release notes
     id: release_notes
     run: |
       NOTES=$(./scripts/release/generate-release-notes.sh \
         --version ${{ steps.version.outputs.version }} \
         --format markdown)
       
       # Add framework warning if .NET 10.0 unavailable
       if [ -n "${{ steps.check_coverage.outputs.framework_warning }}" ]; then
         NOTES="$NOTES\n\n⚠️ **Framework Availability**: ${{ steps.check_coverage.outputs.framework_warning }}"
       fi
       
       # Add preview SDK warning if .NET 10.0 packages built with preview
       if [ -n "${{ steps.check_coverage.outputs.preview_warning }}" ]; then
         NOTES="$NOTES\n\n${{ steps.check_coverage.outputs.preview_warning }}"
       fi
       
       # Add package size warnings (if any packages exceed 150MB soft limit)
       SIZE_WARNINGS=$(find ./dist -size +150M -name "*.tar.gz" -o -size +150M -name "*.zip" 2>/dev/null || true)
       if [ -n "$SIZE_WARNINGS" ]; then
         NOTES="$NOTES\n\n⚠️ **Package Size Notice**: Some packages exceed the 150MB target size. This is expected for self-contained deployments with full runtime."
       fi
       
       echo "notes<<EOF" >> $GITHUB_OUTPUT
       echo "$NOTES" >> $GITHUB_OUTPUT
       echo "EOF" >> $GITHUB_OUTPUT
   ```
   
   **Notes**:
   - Includes framework availability warnings (FR-033)
   - Includes preview SDK warnings for transparency
   - Includes package size warnings (FR-031)
   - All warnings are non-blocking but provide important context to users

6. **Create GitHub Release**: Publish release with packages
   ```yaml
   - name: Create GitHub Release
     uses: softprops/action-gh-release@v1
     if: ${{ !inputs.dry_run }}
     with:
       tag_name: ${{ github.ref_name }}
       name: Fifth Compiler ${{ steps.version.outputs.version }}
       body: ${{ steps.release_notes.outputs.notes }}
       draft: false
       prerelease: ${{ contains(github.ref_name, '-') }}
       files: |
         ./dist/**/*.tar.gz
         ./dist/**/*.zip
         ./dist/SHA256SUMS
     env:
       GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
   ```

7. **Dry Run Output**: Log what would be published (if dry run)
   ```yaml
   - name: Dry run output
     if: ${{ inputs.dry_run }}
     run: |
       echo "DRY RUN: Would publish the following:"
       ls -lh ./dist
       cat ./dist/SHA256SUMS
       echo "Framework coverage: ${{ steps.check_coverage.outputs.net8_packages }} net8.0, ${{ steps.check_coverage.outputs.net10_packages }} net10.0"
   ```

### Success Criteria

- All expected packages present in artifacts (12 packages, or 6 if .NET 10.0 SDK unavailable per FR-033)
- Checksums generated successfully
- Release created on GitHub
- All packages attached to release
- Release marked as pre-release if version contains suffix
- Framework availability warnings included in release notes if applicable
- Package size warnings logged but non-blocking (FR-031)

---

## Environment Variables

**Provided by GitHub Actions**:
- `GITHUB_REF`: Full git ref (refs/tags/v1.2.3)
- `GITHUB_REF_NAME`: Tag name (v1.2.3)
- `GITHUB_SHA`: Commit SHA
- `GITHUB_REPOSITORY`: Repository (aabs/fifthlang)
- `GITHUB_WORKSPACE`: Workspace directory
- `RUNNER_OS`: Runner OS (Linux, macOS, Windows)
- `RUNNER_ARCH`: Runner architecture (X64, ARM64)

**Custom Secrets Required**:
- `GITHUB_TOKEN`: Automatically provided for release creation

---

## Performance Targets

### Individual Job
- Checkout + Setup: < 2 minutes
- Build + Publish: < 8 minutes
- Tests: < 5 minutes
- Total per job: < 15 minutes

### Overall Workflow
- Matrix parallelism: 12 jobs
- Total wall time: < 20 minutes (limited by slowest job)
- Aggregate CPU time: ~180 minutes (12 × 15)

### Resource Usage
- Disk space per job: ~5 GB (build outputs + packages)
- Artifact storage: ~1 GB (12 packages @ ~80 MB each)
- Bandwidth: ~1 GB upload (packages to GitHub)

---

## Error Scenarios

### Build Failure in Matrix Job
- **Behavior**: Job marked failed, other jobs continue
- **Impact**: Publish job skipped (requires all builds)
- **Resolution**: Fix build, re-run workflow

### Test Failure in Matrix Job
- **Behavior**: Job marked failed, package not uploaded
- **Impact**: Missing artifact prevents publish
- **Resolution**: Fix test, re-run workflow

### Publish Failure
- **Behavior**: Release creation fails, packages orphaned in artifacts
- **Impact**: No release published, manual cleanup needed
- **Resolution**: Re-run workflow or manually create release

### Partial Build Success (Some Matrix Jobs Fail)
- **Behavior**: Publish job skipped per all-or-nothing policy (FR-030). ALL build jobs must succeed for ANY packages to be published.
- **Impact**: No release created. Users receive no packages (even if 11/12 succeed).
- **Resolution**: Investigate failing jobs, fix root cause, re-run entire workflow.
- **Exception**: If only .NET 10.0 jobs fail due to SDK unavailability, this is handled gracefully per FR-033 (6 .NET 8.0 packages published).

---

## CI/CD Integration

### Integration with Existing CI
- **Existing `ci.yml`**: Runs on PRs and commits (unchanged)
- **New `release.yml`**: Runs only on version tags
- **No Conflicts**: Different trigger conditions, no overlap

### Workflow Dependencies
- **None**: Release workflow is standalone
- **Manual Trigger**: Can be run independently for testing
- **Tag Protection**: Only maintainers can push version tags

### Monitoring
- **GitHub Actions UI**: View workflow runs, job logs
- **Email Notifications**: On workflow failure (configurable)
- **Release Page**: Verify published packages and checksums

---

## Example Workflow Invocation

### Automatic (Tag Push)
```bash
git tag v1.2.3
git push origin v1.2.3
# Workflow triggers automatically
```

### Manual (Testing)
1. Navigate to Actions tab in GitHub
2. Select "Release Packaging" workflow
3. Click "Run workflow"
4. Enter version (e.g., "1.2.3")
5. Check "Dry run" to skip publishing
6. Click "Run workflow"

### Workflow Run URL
`https://github.com/aabs/fifthlang/actions/workflows/release.yml`

---

## Contract Validation

The workflow contract is validated by:
1. **Schema Validation**: GitHub Actions validates YAML syntax
2. **Dry Run Testing**: Manual workflow dispatch with dry_run=true
3. **Build Matrix Completeness**: All 12 combinations defined
4. **Script Interface Compliance**: All referenced scripts follow contracts
5. **Artifact Naming**: Consistent naming across jobs

## Future Enhancements (Out of Scope for MVP)

- **Code Signing**: Sign Windows/macOS binaries
- **Notarization**: macOS notarization for Gatekeeper
- **Docker Images**: Build Docker containers in addition to packages
- **Package Manager Integration**: Publish to Homebrew, Chocolatey, APT
- **Nightly Builds**: Separate workflow for bleeding-edge builds
- **Performance Benchmarks**: Run perf tests before publishing
