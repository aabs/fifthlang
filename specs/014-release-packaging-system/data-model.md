# Data Model: Release Packaging System

**Status**: Phase 1 Design  
**Updated**: 2025-11-23

## Overview

This document defines the key entities, their relationships, and data structures for the release packaging system. Since this is an infrastructure/build feature rather than a traditional application, entities represent artifacts, configurations, and metadata rather than persistent domain models.

## Core Entities

### 1. Release Package

**Description**: A distributable archive containing the compiled Fifth compiler and all required runtime dependencies for a specific platform and framework version.

**Attributes**:
- `version`: string (semantic version, e.g., "1.2.3")
- `platform`: enum (Windows, macOS, Linux)
- `architecture`: enum (x64, ARM64)
- `framework`: enum (net8.0, net10.0)
- `filename`: string (e.g., "fifth-1.2.3-linux-x64-net8.0.tar.gz")
- `filepath`: string (local path during build)
- `size_bytes`: integer
- `checksum_sha256`: string
- `build_timestamp`: ISO8601 datetime
- `archive_format`: enum (tar.gz, zip)

**Relationships**:
- Contains 1+ **Build Artifacts**
- Validated by 1 **Smoke Test Result**
- Published as part of 1 **Release Version**
- Created by 1 **Build Matrix Configuration**

**File Representation**: Archive file (tar.gz or zip)

**Example**:
```json
{
  "version": "0.9.0",
  "platform": "linux",
  "architecture": "x64",
  "framework": "net8.0",
  "filename": "fifth-0.9.0-linux-x64-net8.0.tar.gz",
  "size_bytes": 89432064,
  "checksum_sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "build_timestamp": "2025-11-23T18:45:32Z",
  "archive_format": "tar.gz"
}
```

---

### 2. Build Artifact

**Description**: A compiled binary or resource file included in a Release Package.

**Attributes**:
- `artifact_type`: enum (Executable, Library, Resource, Documentation)
- `filename`: string (e.g., "fifth", "fifth.dll", "Antlr4.Runtime.Standard.dll")
- `relative_path`: string (path within archive, e.g., "bin/fifth")
- `size_bytes`: integer
- `is_executable`: boolean
- `framework_version`: string (e.g., "net8.0")
- `runtime_identifier`: string (e.g., "linux-x64")

**Relationships**:
- Included in 1+ **Release Packages**
- Produced by .NET publish process

**File Representation**: Binary file within archive

**Example**:
```json
{
  "artifact_type": "Executable",
  "filename": "fifth",
  "relative_path": "bin/fifth",
  "size_bytes": 65432100,
  "is_executable": true,
  "framework_version": "net8.0",
  "runtime_identifier": "linux-x64"
}
```

---

### 3. Release Version

**Description**: A specific tagged release of the Fifth compiler, containing all platform/framework package variants.

**Attributes**:
- `version_tag`: string (git tag, e.g., "v1.2.3")
- `version_number`: string (semantic version, e.g., "1.2.3")
- `release_date`: ISO8601 datetime
- `is_prerelease`: boolean
- `git_commit_sha`: string
- `changelog`: string (markdown-formatted)
- `release_notes`: string (user-facing description)
- `github_release_id`: integer (GitHub API identifier)
- `github_release_url`: string

**Relationships**:
- Contains 12 **Release Packages** (6 platforms × 2 frameworks)
- References 1 git commit
- Has 1 **Checksum Manifest**

**File Representation**: GitHub Release + metadata JSON

**Example**:
```json
{
  "version_tag": "v0.9.0",
  "version_number": "0.9.0",
  "release_date": "2025-11-23T19:00:00Z",
  "is_prerelease": false,
  "git_commit_sha": "a1b2c3d4e5f6",
  "changelog": "### Added\n- Multi-platform packaging\n### Fixed\n- Parser error recovery",
  "release_notes": "First official release with automated packaging",
  "github_release_id": 123456,
  "github_release_url": "https://github.com/aabs/fifthlang/releases/tag/v0.9.0"
}
```

---

### 4. Smoke Test Result

**Description**: The outcome of automated verification tests run against a Release Package to ensure it's functional.

**Attributes**:
- `package_filename`: string (links to Release Package)
- `test_timestamp`: ISO8601 datetime
- `test_duration_seconds`: float
- `overall_status`: enum (Pass, Fail, Skipped)
- `test_cases`: array of TestCase
- `error_messages`: array of string (if failed)
- `test_environment`: object (OS version, runner info)

**Nested Type: TestCase**:
- `test_name`: string (e.g., "version_check", "compile_hello_world")
- `status`: enum (Pass, Fail, Skipped)
- `duration_seconds`: float
- `output`: string (stdout/stderr capture)
- `error_message`: string (if failed)

**Relationships**:
- Validates 1 **Release Package**
- Run by CI workflow

**File Representation**: JSON log file (smoke-test-results.json)

**Example**:
```json
{
  "package_filename": "fifth-0.9.0-linux-x64-net8.0.tar.gz",
  "test_timestamp": "2025-11-23T18:50:00Z",
  "test_duration_seconds": 12.3,
  "overall_status": "Pass",
  "test_cases": [
    {
      "test_name": "version_check",
      "status": "Pass",
      "duration_seconds": 1.2,
      "output": "Fifth Language Compiler v0.9.0",
      "error_message": null
    },
    {
      "test_name": "compile_hello_world",
      "status": "Pass",
      "duration_seconds": 8.5,
      "output": "Compilation successful: hello.exe",
      "error_message": null
    }
  ],
  "error_messages": [],
  "test_environment": {
    "os": "ubuntu-20.04",
    "runner": "GitHub-Hosted"
  }
}
```

---

### 5. Build Matrix Configuration

**Description**: A specification for one build job in the CI/CD matrix, defining target platform, architecture, and framework.

**Attributes**:
- `job_id`: string (unique identifier, e.g., "linux-x64-net8.0")
- `os_runner`: string (GitHub Actions runner, e.g., "ubuntu-latest")
- `platform`: enum (Windows, macOS, Linux)
- `architecture`: enum (x64, ARM64)
- `framework`: enum (net8.0, net10.0)
- `runtime_identifier`: string (e.g., "linux-x64", "win-arm64")
- `publish_flags`: array of string (dotnet publish arguments)
- `archive_format`: enum (tar.gz, zip)
- `depends_on`: array of string (job dependencies)

**Relationships**:
- Produces 1 **Release Package**
- Part of GitHub Actions workflow matrix

**File Representation**: YAML workflow configuration

**Example**:
```yaml
job_id: linux-x64-net8.0
os_runner: ubuntu-latest
platform: Linux
architecture: x64
framework: net8.0
runtime_identifier: linux-x64
publish_flags:
  - --configuration Release
  - --self-contained true
  - --runtime linux-x64
  - --framework net8.0
  - -p:PublishSingleFile=true
  - -p:PublishTrimmed=false
archive_format: tar.gz
depends_on: []
```

---

### 6. Checksum Manifest

**Description**: A file containing SHA256 checksums for all packages in a Release Version, used for integrity verification.

**Attributes**:
- `version`: string (release version)
- `generated_at`: ISO8601 datetime
- `entries`: array of ChecksumEntry

**Nested Type: ChecksumEntry**:
- `filename`: string
- `sha256`: string
- `size_bytes`: integer

**Relationships**:
- Belongs to 1 **Release Version**
- References 12 **Release Packages**

**File Representation**: Text file (SHA256SUMS)

**Example**:
```text
# Fifth Language Compiler v0.9.0 - SHA256 Checksums
# Generated: 2025-11-23T19:00:00Z

e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855  fifth-0.9.0-linux-x64-net8.0.tar.gz
5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8  fifth-0.9.0-win-x64-net8.0.zip
# ... (10 more entries)
```

---

## Entity Relationships Diagram

```
┌─────────────────┐
│ Release Version │
│  (GitHub Tag)   │
└────────┬────────┘
         │ 1
         │
         │ 12 (6 platforms × 2 frameworks)
         ▼
┌─────────────────────┐         ┌──────────────────────┐
│  Release Package    │◄────────│ Build Matrix Config  │
│   (Archive File)    │ creates │  (CI Job Config)     │
└─────────┬───────────┘         └──────────────────────┘
          │ contains
          │ 1..*
          ▼
┌─────────────────────┐
│  Build Artifact     │
│ (Binary/Resource)   │
└─────────────────────┘

┌─────────────────────┐         ┌──────────────────────┐
│  Release Package    │───────►│  Smoke Test Result   │
│                     │ tested  │   (Test Report)      │
└─────────────────────┘  by 1   └──────────────────────┘

┌─────────────────────┐
│  Release Version    │───────►│  Checksum Manifest   │
│                     │  has 1  │   (SHA256SUMS)       │
└─────────────────────┘         └──────────────────────┘
```

## Data Flow

### Build Phase
1. **Build Matrix Configuration** → triggers CI job
2. CI job runs `dotnet publish` → produces **Build Artifacts**
3. Build script packages artifacts → creates **Release Package**
4. Generate checksum → updates **Release Package** metadata

### Test Phase
1. Extract **Release Package** in clean environment
2. Run smoke tests → produces **Smoke Test Result**
3. If tests pass → mark package ready for publication
4. If tests fail → abort release, log errors

### Publish Phase
1. Aggregate all 12 **Release Packages**
2. Generate **Checksum Manifest** from package checksums
3. Create **Release Version** with git tag metadata
4. Upload to GitHub Releases → set `github_release_id`
5. Update release notes with installation instructions

## File Formats

### Package Archive Structure (Linux/macOS)
```
fifth-{version}-{platform}-{arch}-{framework}.tar.gz
└── fifth-{version}/
    ├── bin/
    │   └── fifth              # Executable (+x permissions)
    ├── lib/
    │   ├── fifth.dll
    │   ├── Antlr4.Runtime.Standard.dll
    │   └── [other dependencies]
    ├── LICENSE
    ├── README.md
    └── VERSION.txt
```

### Package Archive Structure (Windows)
```
fifth-{version}-win-{arch}-{framework}.zip
└── fifth-{version}/
    ├── bin/
    │   └── fifth.exe
    ├── lib/
    │   └── [dependencies]
    ├── LICENSE
    ├── README.md
    └── VERSION.txt
```

### Metadata Files
- `VERSION.txt`: Plain text version number
- `SHA256SUMS`: Checksum manifest (standard format)
- `release-metadata.json`: Structured release information
- `smoke-test-results.json`: Test execution logs

## Design Notes

- **No persistent storage**: All entities are ephemeral (created during CI, published to GitHub)
- **Immutable releases**: Once published, Release Packages and Versions are not modified
- **Deterministic builds**: Same source + config → same checksums (reproducible builds principle)
- **Self-describing packages**: VERSION.txt and README.md embedded in every archive
- **Platform-specific formats**: tar.gz for Unix (preserves permissions), zip for Windows

## Future Considerations

- **Package signing**: Code signing for Windows, notarization for macOS (not in MVP)
- **Installation metadata**: For future package manager integration (Homebrew, Chocolatey)
- **Update manifests**: JSON feed for version checking and auto-updates (post-MVP)
- **Build provenance**: SLSA attestations for supply chain security (future enhancement)
