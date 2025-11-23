# SDK Version and Preview Release Clarifications

**Date**: 2025-11-23  
**Status**: Approved

## Summary

This document clarifies the .NET SDK versioning strategy for the release packaging system, addressing two key concerns:

1. Whether pinning SDK version in `global.json` interferes with cross-compilation
2. How to handle .NET 10 preview releases when final release is unavailable

## Key Clarifications

### 1. global.json Pinning vs Cross-Compilation

**Question**: Does pinning the .NET SDK version in `global.json` interfere with cross-compiling to different .NET runtime frameworks?

**Answer**: No, there is no interference.

**Explanation**:
- The `global.json` file pins the SDK used for **project operations** (restore, build, etc.)
- It does NOT prevent targeting multiple runtime frameworks (net8.0, net10.0)
- Multiple SDKs can be installed side-by-side without conflict
- When building for a specific target framework (e.g., `dotnet publish --framework net10.0`), the appropriate SDK (10.0) is automatically used for that target
- The pinned SDK (8.0.414) ensures build consistency while allowing multi-targeting

**Implementation**:
```yaml
# Install pinned SDK from global.json (8.0.414)
- uses: actions/setup-dotnet@v4
  with:
    global-json-file: global.json

# Install additional SDK for .NET 10.0 targeting
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
    dotnet-quality: 'preview'  # Allows preview if final unavailable
```

### 2. Preview SDK Usage Strategy

**Question**: If .NET 10.0 final release is not yet available, should the system use preview releases?

**Answer**: Yes, use whatever .NET 10.0 SDK is available (preview or final).

**Rationale**:
- Allows early testing and package creation before GA release
- Enables continuous integration without waiting for final releases
- Provides transparency to users about preview usage

**Requirements**:
- Detect SDK version (preview vs final)
- Annotate release notes with SDK version used
- Include warnings when packages are built with preview SDKs
- Clearly communicate preview status to users

**Implementation**:
```bash
# Detect SDK version and preview status
SDK_VERSION=$(dotnet --list-sdks | grep "^10\." | head -n1 | awk '{print $1}')
if [[ "$SDK_VERSION" == *"preview"* ]] || [[ "$SDK_VERSION" == *"rc"* ]]; then
  echo "IS_PREVIEW=true" >> $GITHUB_OUTPUT
  echo "SDK_VERSION=$SDK_VERSION" >> $GITHUB_OUTPUT
fi
```

**Release Notes Example**:
```markdown
## Release v0.9.0

### Packages
- Windows (x64, ARM64) - .NET 8.0, .NET 10.0
- macOS (Intel, Apple Silicon) - .NET 8.0, .NET 10.0  
- Linux (x64, ARM64) - .NET 8.0, .NET 10.0

⚠️ **.NET 10.0 SDK Notice**: Packages built with preview SDK 10.0.100-preview.7 (not final release). Users should be aware that preview SDKs may have different behavior than final releases.

### Downloads
...
```

## Graceful Degradation

When .NET 10.0 SDK is unavailable (neither preview nor final):
- Build only .NET 8.0 packages (6 platforms)
- Log warning in build output
- Annotate release notes with framework unavailability message
- Enforce all-or-nothing policy within each framework (if any .NET 8.0 build fails, abort)

**Release Notes Example (SDK Unavailable)**:
```markdown
## Release v0.8.5

### Packages
- Windows (x64, ARM64) - .NET 8.0
- macOS (Intel, Apple Silicon) - .NET 8.0
- Linux (x64, ARM64) - .NET 8.0

⚠️ **Framework Availability**: .NET 10.0 SDK was unavailable; only .NET 8.0 packages included in this release.

### Downloads
...
```

## Updated Requirements

These clarifications resulted in the following specification updates:

- **FR-033**: Enhanced to specify preview SDK usage when final unavailable
- **FR-034**: NEW - System MUST support installing multiple .NET SDKs (8.0 pinned + 10.0 preview/final)
- **plan.md**: Added "SDK vs Runtime Framework Clarification" section
- **research.md**: Updated .NET 10.0 research tasks with preview SDK investigation
- **tasks.md**: Updated T022 and T023 to include preview SDK detection and annotation
- **contracts/github-actions-workflow.md**: Added multi-SDK setup and preview detection steps
- **data-model.md**: Added SDK metadata fields to Release Version entity

## Benefits

1. **Build Consistency**: Pinned SDK ensures reproducible builds
2. **Multi-Targeting Flexibility**: Can target multiple frameworks without SDK conflicts
3. **Early Adoption**: Can use preview SDKs for testing and package creation
4. **Transparency**: Users know exactly which SDK version was used
5. **Graceful Degradation**: System works even when newer SDKs unavailable

## References

- Spec: `specs/014-release-packaging-system/spec.md` (Clarifications section)
- Plan: `specs/014-release-packaging-system/plan.md` (Technical Context section)
- Research: `specs/014-release-packaging-system/research.md` (Question 6)
- Contract: `specs/014-release-packaging-system/contracts/github-actions-workflow.md` (Build job steps)
