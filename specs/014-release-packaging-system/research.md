# Phase 0: Research - Release Packaging System

**Status**: In Progress  
**Updated**: 2025-11-23

## Overview

This document captures unknowns extracted from the Technical Context and feature requirements that must be resolved before design and implementation. Research focuses on .NET publishing best practices, GitHub Actions optimization, cross-platform packaging, and verification strategies.

## Research Questions

### 1. .NET Self-Contained Publishing

**Question**: What are the optimal `dotnet publish` flags for creating self-contained, production-ready compiler packages that include ANTLR runtime without IL trimming?

**Why Critical**: Package size and startup performance depend on publish settings. ANTLR uses reflection, so IL trimming must be disabled, but we need to understand size/performance tradeoffs.

**Research Tasks**:
- [ ] Document required MSBuild properties for self-contained deployment
  - `PublishSingleFile` implications for ANTLR jar bundling
  - `IncludeNativeLibrariesForSelfExtract` requirements
  - `RuntimeIdentifier` patterns for all 6 platform/arch combinations
- [ ] Measure baseline package sizes with/without compression
  - .NET 8.0 vs 10.0 runtime bundle sizes
  - Impact of excluding development-only dependencies
- [ ] Test ANTLR functionality in single-file deployments
  - Verify grammar compilation works in published packages
  - Confirm parser generation tools bundled correctly
- [ ] Document any platform-specific publish quirks
  - macOS code signing requirements (if any for CLI tools)
  - Linux shared library dependencies (glibc versions)
  - Windows ARM64 compatibility

**Success Criteria**: Documented MSBuild properties and verified package sizes < 150MB per variant.

---

### 2. GitHub Actions Matrix Build Optimization

**Question**: How should we structure the build matrix to complete all 12 platform/framework combinations in < 45 minutes on GitHub free tier?

**Why Critical**: Build time directly impacts release velocity. Need to balance parallelism with runner availability and resource usage.

**Research Tasks**:
- [ ] Analyze GitHub Actions free tier runner limits
  - Concurrent job limits
  - Per-job timeout limits (currently defaults to 360 min)
  - Storage limits for artifacts
- [ ] Design optimal matrix strategy
  - Single workflow with 12x matrix vs multiple workflows
  - Job dependencies (e.g., build before test)
  - Artifact passing between jobs
- [ ] Research GitHub Actions concurrency groups (FR-032)
  - How to cancel older pending builds when new commit arrives
  - Interaction with matrix builds (cancel all matrix jobs or individual?)
  - Testing cancellation behavior in workflow_dispatch scenarios
- [ ] Investigate caching strategies
  - NuGet package caching (already exists in ci.yml)
  - Intermediate build output caching
  - ANTLR generated parser caching
- [ ] Test incremental builds
  - Does `dotnet publish` reuse `dotnet build` outputs?
  - Can we cache restore/build and only publish in release workflow?
- [ ] Design all-or-nothing enforcement mechanism (FR-030)
  - How to ensure publish job only runs if ALL 12 builds succeed
  - Testing partial failure scenarios (11/12 succeed)
  - Validating `needs: [build]` with `if: ${{ success() }}` condition

**Success Criteria**: Workflow design that completes in < 45 min with documented caching strategy and verified all-or-nothing enforcement.

---

### 3. Package Verification & Smoke Testing

**Question**: What constitutes a sufficient smoke test for each package to ensure it's production-ready before publication?

**Why Critical**: Cannot publish broken packages. Need automated tests that run quickly but catch critical failures.

**Research Tasks**:
- [ ] Define minimal smoke test scope
  - Version check (`fifth --version`)
  - Simple compilation test (hello-world.5th)
  - Runtime execution test (compiled program runs)
  - Parser test (grammar loads correctly)
- [ ] Design smoke test execution strategy
  - Run on same OS as build vs cross-platform testing
  - Container-based testing for Linux variants
  - Permissions/PATH setup for automated testing
- [ ] Document expected failure modes
  - Missing native dependencies
  - Incorrect runtime identifiers
  - Broken file permissions in archives
- [ ] Investigate GitHub Actions testing matrix
  - Test on actual target platforms (ubuntu, macos, windows runners)
  - Verify packages install in clean environments
  - Test package extraction and PATH setup

**Success Criteria**: Smoke test script that runs in < 5 min per package and catches 90%+ of deployment issues.

---

### 4. Cross-Platform Archive Creation

**Question**: What tooling should we use to create platform-appropriate archives (tar.gz for Unix, zip for Windows) with correct file permissions and structure?

**Why Critical**: Archive format affects user experience. Wrong permissions or structure breaks installation.

**Research Tasks**:
- [ ] Research tar.gz best practices for Unix
  - Preserving executable permissions (+x for `fifth`)
  - Consistent directory structure (e.g., `fifth-{version}/bin/fifth`)
  - Symlink handling (if any)
  - Deterministic archive creation (for reproducible builds)
- [ ] Research zip best practices for Windows
  - Executable flagging (.exe extensions sufficient?)
  - Path separator normalization
  - Deterministic zip creation
- [ ] Tool selection for GitHub Actions
  - Native tar/gzip availability on runners
  - PowerShell Compress-Archive for Windows
  - Cross-platform alternatives (e.g., 7zip, GNU tar on all platforms)
- [ ] Investigate archive structure conventions
  - Flat structure vs nested directories
  - README/LICENSE placement
  - Version info file inclusion

**Success Criteria**: Documented archive creation commands that work on all GitHub-hosted runners and produce < 80MB archives.

---

### 5. GitHub Releases Publishing

**Question**: What's the recommended approach for publishing to GitHub Releases from Actions workflows, including checksums and release notes?

**Why Critical**: Release publication is the final step; must be reliable and include all metadata.

**Research Tasks**:
- [ ] Research GitHub CLI (`gh release create`) vs Actions
  - `actions/upload-release-asset` (deprecated?)
  - `softprops/action-gh-release` (community action)
  - Native `gh` CLI availability on runners
- [ ] Design release note generation
  - Extract version from git tags
  - Auto-generate changelog from commits since last tag
  - Include installation instructions in release body
  - Add warnings for framework availability (FR-033) and package size (FR-031)
- [ ] Implement pre-release version naming (FR-021)
  - Parse git tags to determine base version
  - Generate `{base}-pre.{YYYYMMDD}.{commit}` format for non-tagged builds
  - Test version extraction from various git states (tagged, untagged, no tags)
- [ ] Checksum file format
  - SHA256SUMS standard format (BSD vs GNU)
  - Per-package checksum files vs single aggregate
  - Naming conventions (SHA256.txt, checksums.txt)
- [ ] Version tagging strategy
  - Semantic versioning enforcement
  - Tag naming (v1.2.3 vs 1.2.3)
  - Pre-release tag handling (alpha, beta, rc)

**Success Criteria**: Documented publishing workflow that creates a release with all artifacts, checksums, and formatted release notes.

---

### 6. .NET 10.0 Forward Compatibility & Graceful Degradation

**Question**: What considerations are needed to ensure the build system works for both .NET 8.0 and .NET 10.0, with graceful degradation when .NET 10.0 SDK is unavailable?

**Why Critical**: Requirement specifies dual-framework support with graceful degradation (FR-033). Need to handle .NET 10.0 preview/unavailability scenarios.

**Key Clarification**: The `global.json` file pins the SDK used for building (currently 8.0.414) but does NOT prevent targeting multiple runtime frameworks. The build system can target both net8.0 and net10.0 frameworks from the same SDK, as long as the target framework SDKs are installed. Building for .NET 10.0 requires .NET 10.0 SDK (preview or final) to be available alongside the pinned 8.0 SDK.

**Preview SDK Strategy**: When .NET 10.0 final release is not available, the system will use whatever preview SDK is available for .NET 10.0. This allows early testing and package creation before GA release.

**Research Tasks**:
- [ ] Investigate .NET 10.0 preview availability
  - Check GitHub Actions runners for pre-installed .NET 10.0 preview SDKs
  - Determine if setup-dotnet action supports installing preview versions
  - Test preview SDK installation using recommended approach: `dotnet-version: '10.0.x'` with `include-prerelease: true`
  - Alternative: specific preview version like `dotnet-version: '10.0.100-preview.7'`
  - Recommended: Use `10.0.x` + `include-prerelease: true` to get latest available (preview or final)
  - Verify preview SDK stability and compatibility with existing code
  - Document breaking changes from 8.0 to 10.0 (if any)
- [ ] Clarify SDK detection and multi-framework setup
  - Confirm `global.json` pinned SDK (8.0.414) is used for project building
  - Verify additional SDKs (like 10.0 preview) can be installed alongside pinned SDK
  - Document how `dotnet publish --framework net10.0` uses 10.0 SDK even with pinned 8.0 SDK
  - Test building net8.0 and net10.0 targets from same build environment
- [ ] Design SDK detection and fallback mechanism (FR-033)
  - Implement `dotnet --list-sdks | grep "^10\\.0\\."` check to detect available .NET 10.0 SDK (preview or final)
  - Use regex pattern `^10\\.0\\.` (escaped for shell) to match only .NET 10.0.x versions (not future 11.0, 12.0, etc.)
  - Gracefully skip .NET 10.0 builds without failing entire workflow when SDK unavailable
  - Detect partial framework coverage in publish job (6 packages vs 12)
  - Generate appropriate warnings in release notes about SDK availability
  - Annotate packages built with preview SDKs (include preview version in release notes)
- [ ] Design multi-framework build strategy
  - Verify single csproj with `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` approach
  - Confirm `dotnet publish --framework net8.0` vs `--framework net10.0` targets correctly
  - Verify package naming distinguishes frameworks (net8.0 vs net10.0 in filename)
  - Test runtime identifier (RID) compatibility across frameworks
- [ ] Test compatibility with existing code
  - Run existing test suite against .NET 10.0 preview SDK
  - Check for API deprecations or breaking changes in compiler code
  - Verify ANTLR 4.8 runtime compatibility with .NET 10.0
  - Test self-contained publishing with .NET 10.0 runtime
- [ ] Plan transition strategy and SDK version management
  - Use preview SDKs immediately when available (don't wait for GA)
  - Document preview SDK version in release notes for transparency
  - Support policy: maintain .NET 8.0 packages for LTS support
  - Migration from preview to stable: no code changes needed, workflow auto-detects final SDK
  - Communication: release notes must clearly indicate which packages use preview vs final SDKs

**Success Criteria**: Documented strategy for adding .NET 10.0 support with SDK detection (preview or final), graceful degradation plan, verified preview SDK usage, and tested behavior when SDK is unavailable.

---

## Research Deliverables

Upon completion, this research phase will produce:

1. **Build Configuration Template** (`docs/build-config.md`)
   - MSBuild properties for all platform/framework combinations
   - Package size benchmarks
   - Publishing command reference

2. **CI/CD Design Document** (`docs/cicd-design.md`)
   - Workflow structure (matrix configuration)
   - Caching strategy
   - Artifact management
   - Estimated build times

3. **Testing Strategy** (`docs/testing-strategy.md`)
   - Smoke test specification
   - Verification checklist
   - Test execution plan
   - Expected failure modes

4. **Packaging Standards** (`docs/packaging-standards.md`)
   - Archive structure specification
   - Naming conventions
   - Checksum format
   - Release note template

5. **Version Management Plan** (`docs/version-management.md`)
   - Tagging conventions
   - Multi-framework support timeline
   - Deprecation policy

## Timeline

**Estimated Duration**: 8-12 hours  
**Blockers**: Access to .NET 10.0 preview (if not yet available)  
**Dependencies**: None (can proceed immediately)

## Notes

- Research should prioritize hands-on experimentation over documentation reading
- Create proof-of-concept scripts for each research area
- Test on actual GitHub Actions runners, not just local environment
- Document any surprises or non-obvious behaviors discovered
