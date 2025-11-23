# Build Script Contracts

## Overview

This document defines the interface contracts for build scripts that create release packages. All scripts follow CLI discipline: accept inputs via arguments/environment, emit structured output to stdout, log progress to stderr, exit with status codes.

---

## Contract: build-release.sh

**Purpose**: Build a single-platform release package from source.

### Input Interface

**Required Arguments**:
- `--version <string>`: Semantic version (e.g., "1.2.3")
- `--runtime <string>`: Runtime identifier (e.g., "linux-x64", "win-arm64")
- `--framework <string>`: Target framework (e.g., "net8.0", "net10.0")
- `--output-dir <path>`: Directory for output artifacts

**Optional Arguments**:
- `--configuration <string>`: Build configuration (default: "Release")
- `--verbose`: Enable detailed logging
- `--skip-tests`: Skip running tests before publishing

**Environment Variables**:
- `DOTNET_CLI_HOME`: .NET CLI home directory
- `NUGET_PACKAGES`: NuGet package cache location

### Output Interface

**Exit Codes**:
- `0`: Success
- `1`: General error (build/publish failed)
- `2`: Invalid arguments
- `3`: Missing dependencies (dotnet, java)

**stdout** (JSON format):
```json
{
  "success": true,
  "package_path": "/path/to/fifth-1.2.3-linux-x64-net8.0.tar.gz",
  "package_size_bytes": 89432064,
  "artifacts": [
    {
      "type": "executable",
      "path": "bin/fifth",
      "size_bytes": 65432100
    },
    {
      "type": "library",
      "path": "lib/Antlr4.Runtime.Standard.dll",
      "size_bytes": 2340567
    }
  ],
  "build_time_seconds": 123.45
}
```

**stderr** (human-readable progress):
```
[INFO] Building Fifth compiler v1.2.3 for linux-x64 (net8.0)
[INFO] Restoring NuGet packages...
[INFO] Building compiler project...
[INFO] Publishing to /tmp/fifth-build/linux-x64-net8.0...
[INFO] Creating archive fifth-1.2.3-linux-x64-net8.0.tar.gz...
[INFO] Build complete in 123.45s
```

### Behavior Contracts

1. **Idempotency**: Running with same inputs produces same output (deterministic builds)
2. **Clean Environment**: Must work in fresh environment without cached artifacts
3. **Error Reporting**: All errors logged to stderr with context (file, line, reason)
4. **Resource Cleanup**: Temporary build directories cleaned on exit (success or failure)
5. **Atomic Output**: Package file written atomically (temp file + rename)

### Example Usage

```bash
./scripts/build/build-release.sh \
  --version 1.2.3 \
  --runtime linux-x64 \
  --framework net8.0 \
  --output-dir ./dist

echo $?  # Check exit code
```

---

## Contract: create-archives.sh

**Purpose**: Package build artifacts into platform-appropriate archives.

### Input Interface

**Required Arguments**:
- `--source-dir <path>`: Directory containing build artifacts
- `--output-file <path>`: Target archive file path
- `--format <tar.gz|zip>`: Archive format
- `--version <string>`: Version for directory naming

**Optional Arguments**:
- `--include-readme`: Include README.md in archive
- `--include-license`: Include LICENSE in archive
- `--verbose`: Enable detailed logging

### Output Interface

**Exit Codes**:
- `0`: Success
- `1`: General error
- `2`: Invalid arguments
- `4`: Source directory not found
- `5`: Archive creation failed

**stdout** (JSON format):
```json
{
  "success": true,
  "archive_path": "/dist/fifth-1.2.3-linux-x64-net8.0.tar.gz",
  "archive_size_bytes": 89432064,
  "file_count": 47,
  "compression_ratio": 0.68,
  "creation_time_seconds": 8.2
}
```

**stderr** (human-readable progress):
```
[INFO] Creating tar.gz archive from /tmp/build/linux-x64-net8.0
[INFO] Adding 47 files to archive...
[INFO] Setting executable permissions for bin/fifth
[INFO] Compressing with gzip (level 9)...
[INFO] Archive created: fifth-1.2.3-linux-x64-net8.0.tar.gz (89.4 MB)
```

### Behavior Contracts

1. **Permission Preservation**: Unix archives preserve executable bits
2. **Deterministic Creation**: Same inputs → same archive (reproducible)
3. **Nested Structure**: Archive contains `fifth-{version}/` top-level directory
4. **Validation**: Verify archive integrity before reporting success
5. **Error Recovery**: Clean up partial archives on failure

### Example Usage

```bash
./scripts/build/create-archives.sh \
  --source-dir /tmp/build/linux-x64-net8.0 \
  --output-file ./dist/fifth-1.2.3-linux-x64-net8.0.tar.gz \
  --format tar.gz \
  --version 1.2.3 \
  --include-readme \
  --include-license
```

---

## Contract: generate-checksums.sh

**Purpose**: Generate SHA256 checksums for release packages.

### Input Interface

**Required Arguments**:
- `--package-dir <path>`: Directory containing package files
- `--output-file <path>`: Checksum manifest file path

**Optional Arguments**:
- `--format <bsd|gnu>`: Checksum file format (default: gnu)
- `--verify`: Verify existing checksums instead of generating

### Output Interface

**Exit Codes**:
- `0`: Success
- `1`: General error
- `2`: Invalid arguments
- `6`: Checksum verification failed

**stdout** (JSON format):
```json
{
  "success": true,
  "manifest_path": "/dist/SHA256SUMS",
  "package_count": 12,
  "checksums": [
    {
      "filename": "fifth-1.2.3-linux-x64-net8.0.tar.gz",
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
      "size_bytes": 89432064
    }
  ],
  "generation_time_seconds": 2.3
}
```

**stderr** (human-readable progress):
```
[INFO] Generating SHA256 checksums for 12 packages
[INFO] Processing fifth-1.2.3-linux-x64-net8.0.tar.gz...
[INFO] Checksum: e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
[INFO] Manifest written to /dist/SHA256SUMS
```

### Behavior Contracts

1. **Standard Format**: Output follows GNU coreutils sha256sum format
2. **Verification Support**: Can verify existing manifest with `--verify`
3. **Atomic Write**: Manifest file written atomically
4. **Sorted Output**: Files listed alphabetically for determinism
5. **Relative Paths**: Filenames in manifest are relative to package directory

### Example Usage

```bash
# Generate checksums
./scripts/build/generate-checksums.sh \
  --package-dir ./dist \
  --output-file ./dist/SHA256SUMS

# Verify checksums
./scripts/build/generate-checksums.sh \
  --package-dir ./dist \
  --output-file ./dist/SHA256SUMS \
  --verify
```

---

## Contract: version-info.sh

**Purpose**: Extract version information from git repository.

### Input Interface

**Optional Arguments**:
- `--format <json|text|semver>`: Output format (default: json)
- `--allow-dirty`: Allow uncommitted changes (for testing)

**Environment Variables**:
- `CI`: Set by CI systems, affects tag resolution
- `GITHUB_REF`: GitHub Actions ref (refs/tags/v1.2.3)

### Output Interface

**Exit Codes**:
- `0`: Success
- `1`: General error
- `7`: Not a git repository
- `8`: No version tag found
- `9`: Dirty working tree (without --allow-dirty)

**stdout** (JSON format):
```json
{
  "version": "1.2.3",
  "version_tag": "v1.2.3",
  "commit_sha": "a1b2c3d4e5f6",
  "commit_short": "a1b2c3d",
  "is_prerelease": false,
  "is_dirty": false,
  "branch": "main",
  "build_timestamp": "2025-11-23T19:00:00Z"
}
```

**Text format**:
```
1.2.3
```

**SemVer format**:
```
1.2.3+a1b2c3d.20251123190000
```

### Pre-release Version Format (FR-021)

For non-tagged commits on master branch, the script generates pre-release versions using the format:

**Format**: `{base_version}-pre.{YYYYMMDD}.{short_commit}`

**Examples**:
- `0.1.0-pre.20251123.a1b2c3d` - Pre-release from master
- `1.2.0-pre.20251215.b4c5d6e` - Pre-release after v1.1.0 tag

**Components**:
- `base_version`: Derived from most recent git tag (e.g., `1.2.0`), or defaults to `0.1.0` if no tags exist
- `pre`: Literal string indicating pre-release
- `YYYYMMDD`: Build date in UTC
- `short_commit`: First 7 characters of commit SHA

**JSON output for pre-release**:
```json
{
  "version": "0.1.0-pre.20251123.a1b2c3d",
  "version_tag": null,
  "commit_sha": "a1b2c3d4e5f6",
  "commit_short": "a1b2c3d",
  "is_prerelease": true,
  "is_dirty": false,
  "branch": "master",
  "build_timestamp": "2025-11-23T19:00:00Z"
}
```

### Behavior Contracts

1. **Tag Parsing**: Strips "v" prefix from tags (v1.2.3 → 1.2.3)
2. **Prerelease Detection**: Recognizes -alpha, -beta, -rc suffixes in tags, AND generates pre-release format for non-tagged commits
3. **Pre-release Naming**: Non-tagged commits use `{base}-pre.{date}.{commit}` format (FR-021)
4. **Dirty Detection**: Fails if uncommitted changes (unless --allow-dirty)
5. **CI Integration**: Uses CI environment variables when available
6. **Fallback**: Uses describe output if no exact tag match, generates pre-release version from base

### Example Usage

```bash
# Get version JSON
./scripts/release/version-info.sh --format json

# Get plain version for scripting
VERSION=$(./scripts/release/version-info.sh --format text)
echo "Building version: $VERSION"
```

---

## Common Error Codes

All scripts use consistent exit codes for common failure scenarios:

| Code | Meaning | Common Causes |
|------|---------|---------------|
| 0 | Success | - |
| 1 | General error | Unexpected failures, unhandled exceptions |
| 2 | Invalid arguments | Missing required args, invalid formats |
| 3 | Missing dependencies | dotnet, java, tar, gzip not found |
| 4 | File not found | Source directory, config file missing |
| 5 | Operation failed | Archive creation, compression failed |
| 6 | Verification failed | Checksum mismatch, smoke test failure |
| 7 | Git error | Not a repository, no remotes |
| 8 | Version error | No tags, invalid version format |
| 9 | Dirty state | Uncommitted changes when clean required |

## Testing Contracts

Each script must have corresponding test coverage:

1. **Happy Path**: Valid inputs produce expected outputs
2. **Error Handling**: Invalid inputs produce correct exit codes and messages
3. **Edge Cases**: Empty directories, large files, special characters in paths
4. **Idempotency**: Multiple runs with same inputs produce same results
5. **Resource Cleanup**: Temporary files cleaned on both success and failure

Test files located at: `test/release-tests/test-{script-name}.sh`
