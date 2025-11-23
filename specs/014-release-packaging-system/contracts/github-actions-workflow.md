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

## Build Matrix Configuration

### Matrix Dimensions

```yaml
strategy:
  fail-fast: false
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

2. **Setup .NET SDK**: Install required .NET SDK version
   ```yaml
   - uses: actions/setup-dotnet@v4
     with:
       global-json-file: global.json  # Respects pinned version
   ```

3. **Setup Java**: Install Java 17+ for ANTLR (if grammar regeneration needed)
   ```yaml
   - uses: actions/setup-java@v4
     with:
       java-version: '17'
       distribution: 'temurin'
   ```

4. **Extract Version**: Get version from git tag or input
   ```yaml
   - name: Extract version
     id: version
     run: |
       VERSION=$(./scripts/release/version-info.sh --format text)
       echo "version=$VERSION" >> $GITHUB_OUTPUT
   ```

5. **Build Package**: Run build-release.sh script
   ```yaml
   - name: Build release package
     run: |
       ./scripts/build/build-release.sh \
         --version ${{ steps.version.outputs.version }} \
         --runtime ${{ matrix.runtime }} \
         --framework ${{ matrix.framework }} \
         --output-dir ./dist
   ```

6. **Verify Package**: Run verification before tests
   ```yaml
   - name: Verify package structure
     run: |
       ./scripts/test/verify-package.sh \
         --package-path ./dist/fifth-*.tar.gz
   ```

7. **Smoke Test**: Run functional tests on package
   ```yaml
   - name: Run smoke tests
     run: |
       ./scripts/test/smoke-test.sh \
         --package-path ./dist/fifth-*.tar.gz \
         --test-dir /tmp/smoke-test
   ```

8. **Upload Artifact**: Save package for publish job
   ```yaml
   - uses: actions/upload-artifact@v4
     with:
       name: package-${{ matrix.runtime }}-${{ matrix.framework }}
       path: ./dist/fifth-*.tar.gz
       retention-days: 7
   ```

### Failure Handling

- **Build Failure**: Job fails, other matrix jobs continue (fail-fast: false)
- **Test Failure**: Job fails, package not uploaded
- **Verification Failure**: Job fails, no smoke tests run

---

## Job Contract: publish

**Purpose**: Aggregate all packages and publish to GitHub Releases.

### Dependencies
- Requires: All `build` jobs complete successfully
- Runs: Only on success of all builds (unless dry run)

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

2. **Generate Checksums**: Create SHA256SUMS manifest
   ```yaml
   - name: Generate checksums
     run: |
       ./scripts/build/generate-checksums.sh \
         --package-dir ./dist \
         --output-file ./dist/SHA256SUMS
   ```

3. **Generate Release Notes**: Extract changelog from commits
   ```yaml
   - name: Generate release notes
     id: release_notes
     run: |
       NOTES=$(./scripts/release/generate-release-notes.sh \
         --version ${{ steps.version.outputs.version }} \
         --format markdown)
       echo "notes<<EOF" >> $GITHUB_OUTPUT
       echo "$NOTES" >> $GITHUB_OUTPUT
       echo "EOF" >> $GITHUB_OUTPUT
   ```

4. **Create GitHub Release**: Publish release with packages
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

5. **Dry Run Output**: Log what would be published (if dry run)
   ```yaml
   - name: Dry run output
     if: ${{ inputs.dry_run }}
     run: |
       echo "DRY RUN: Would publish the following:"
       ls -lh ./dist
       cat ./dist/SHA256SUMS
   ```

### Success Criteria

- All 12 packages present in artifacts
- Checksums generated successfully
- Release created on GitHub
- All packages attached to release
- Release marked as pre-release if version contains suffix

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
- **Behavior**: Publish job skipped (needs all 12 builds)
- **Impact**: No release created
- **Resolution**: Investigate failing jobs, fix, re-run

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
