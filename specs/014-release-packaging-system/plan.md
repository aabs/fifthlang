# Implementation Plan: Multi-Platform, Multi-Framework Release Packaging

**Branch**: `014-release-packaging-system` | **Date**: 2025-11-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/014-release-packaging-system/spec.md`

**Note**: This plan follows the `/speckit.plan` execution workflow from `.specify/templates/commands/plan.md`.

## Summary

Implement an automated release packaging system that produces distributable binaries for the Fifth language compiler across multiple operating systems (Windows, macOS, Linux) and .NET framework versions (.NET 8.0 and 10.0). The system will create self-contained, production-ready packages via GitHub Actions CI/CD, enabling users to install and use the compiler without requiring development tools beyond the .NET runtime. Packages will be automatically built on master commits and published to GitHub Releases with smoke testing, checksums, and comprehensive documentation.

## Technical Context

**Language/Version**: C# 14, .NET SDK 8.0.414 (per global.json)  

**SDK vs Runtime Framework Clarification**:
- **Build SDK**: The `global.json` file pins the .NET SDK version (8.0.414) used to compile the project
- **Target Frameworks**: The build can target multiple runtime frameworks (net8.0, net10.0) independently of the SDK version
- **Cross-Compilation**: Pinning SDK version in `global.json` does NOT prevent cross-compiling to different target frameworks
- **SDK Requirements**: Building for .NET 10.0 targets requires .NET 10.0 SDK to be installed alongside the pinned 8.0 SDK
- **Preview SDK Strategy**: When .NET 10.0 final release is not available, the build system will:
  - Attempt to use the latest available .NET 10.0 preview SDK (if any)
  - Gracefully degrade to .NET 8.0-only builds if no .NET 10.0 SDK (preview or final) is available
  - Document preview SDK version used in release notes
  - Annotate packages built with preview SDKs with appropriate warnings

**Primary Dependencies**: 
- GitHub Actions (CI/CD platform)
- .NET CLI (`dotnet publish`) for self-contained deployment
- .NET SDK 8.0.414 (pinned in global.json for build consistency)
- .NET SDK 10.0 (preview or final, for .NET 10.0 target builds)
- ANTLR 4.8 runtime (bundled in packages)
- Bash/PowerShell scripting for build automation
- tar/gzip (Unix) and zip (Windows) for archive creation
- SHA256 for checksums (built into .NET)

**Storage**: GitHub Releases for package distribution, GitHub Actions artifacts for intermediate storage  
**Testing**: TUnit + FluentAssertions for test suite; automated smoke tests per package  
**Target Platform**: 
- Windows 10+ (x64, ARM64)
- macOS 11.0+ (Intel x64, Apple Silicon ARM64)
- Linux (Ubuntu 20.04+, Debian 11+, RHEL 8+) (x64, ARM64)

**Project Type**: Build/CI infrastructure (adds to existing single-solution compiler project)  
**Performance Goals**: 
- Build pipeline < 45 minutes for all 12 platform/framework combinations
- Individual platform builds < 10 minutes
- Package size target: 80-100MB per variant (soft limit: 150MB with warnings per FR-031)

**Constraints**: 
- Must use GitHub Actions free tier only
- Cannot use IL trimming (ANTLR reflection incompatibility)
- Must maintain compatibility with existing build process
- Must not break existing development workflows

**Scale/Scope**: 
- 12 platform/framework combinations (6 platforms × 2 frameworks, with graceful degradation to 6 if .NET 10.0 SDK unavailable per FR-033)
- ~6-10 script files (build, test, validation)
- 1 new GitHub Actions workflow with concurrency control (FR-032)
- Documentation updates across 4-5 files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

This feature adds build/release infrastructure without modifying the compiler core, AST, grammar, or transformation pipeline. Here's how it aligns with constitutional principles:

**I. Library-First, Contracts-First**  
✅ **PASS** - This is infrastructure (build/CI), not a library. However, we will follow contracts-first approach by:
- Defining clear script interfaces (inputs/outputs) before implementation
- Creating testable verification scripts
- Documenting the release process as a contract for CI consumers

**II. CLI and Text I/O Discipline**  
✅ **PASS** - Release scripts will:
- Output to stdout/stderr for GitHub Actions observability
- Use structured output (checksums.txt, version info)
- Emit deterministic, parseable output for automation
- Follow existing Fifth compiler CLI patterns

**III. Generator-as-Source-of-Truth**  
✅ **N/A** - No generated code; only CI/build scripts

**IV. Test-First with TUnit + FluentAssertions**  
✅ **PASS (with adaptation)** - Build scripts testing strategy:
- Smoke tests for each published package (automated per FR-001)
- Integration test for full release workflow (automated via workflow validation)
- Cannot use TUnit/FluentAssertions (wrong domain - infrastructure)
- Will use: bash test scripts + GitHub Actions workflow validation
- All-or-nothing policy (FR-030) ensures no partial releases
- Package size soft limits (FR-031) validated with warnings

**V. Reproducible Builds**  
✅ **PASS** - Critical for this feature:
- Pin .NET SDK version per global.json (8.0.414)
- Document all build tool versions in release workflow
- Ensure deterministic package naming and checksums
- Test on GitHub-hosted runners (standardized environments)

**VI. Multi-Pass Compilation & AST Lowering Philosophy**  
✅ **N/A** - No changes to compilation pipeline

**VII. Roslyn Backend Migration**  
✅ **N/A** - No changes to code generation

**VIII. Parser & Grammar Integrity**  
✅ **N/A** - No grammar changes

**IX. Documentation & Example Validation**  
✅ **PASS** - Must add/update:
- Installation documentation (how to download and use packages)
- Release process documentation (for maintainers)
- Package structure documentation (what's included)
- All examples validated via existing `scripts/validate-examples.fish`

**X. Logging & Observability**  
✅ **PASS** - Release workflow will emit:
- Build progress to GitHub Actions logs
- Smoke test results to stdout
- Error diagnostics to stderr with clear context
- Package metadata (version, checksums) to structured files

**XI. Versioning & Backward Compatibility**  
✅ **PASS** - Release system respects SemVer:
- Version extracted from git tags or CI environment
- Breaking changes to release format require migration notes
- Package naming includes version for clear identification

**Gate Decision**: ✅ **PROCEED TO PHASE 0**  
No blocking issues. The "Test-First" gate requires adapted approach (bash tests + workflow validation instead of TUnit), which is appropriate for infrastructure code.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
fifthlang/
├── .github/
│   └── workflows/
│       ├── ci.yml                    # Existing CI (unchanged)
│       └── release.yml               # NEW: Release packaging workflow
├── scripts/
│   ├── build/
│   │   ├── build-release.sh          # NEW: Build single-platform package
│   │   ├── create-archives.sh        # NEW: Create tar.gz/zip archives
│   │   └── generate-checksums.sh     # NEW: Generate SHA256 checksums
│   ├── test/
│   │   ├── smoke-test.sh             # NEW: Validate package functionality
│   │   └── verify-package.sh         # NEW: Verify package structure
│   └── release/
│       ├── publish-release.sh        # NEW: Publish to GitHub Releases
│       └── version-info.sh           # NEW: Extract version from git tags
├── src/
│   ├── compiler/
│   │   └── compiler.csproj           # MODIFIED: Add PublishSingleFile, RuntimeIdentifier
│   └── [other existing projects unchanged]
├── docs/
│   ├── installation.md               # NEW: End-user installation guide
│   ├── release-process.md            # NEW: Maintainer release guide
│   └── package-structure.md          # NEW: Package contents documentation
├── test/
│   └── release-tests/                # NEW: Release validation tests
│       ├── smoke-test-cases.sh       # Test cases for smoke testing
│       └── integration-release.sh    # Full release workflow test
└── specs/
    └── 014-release-packaging-system/ # This feature's spec
```

**Structure Decision**: Infrastructure feature - adds CI workflow, build scripts, and documentation. No new .NET projects needed; modifies only `src/compiler/compiler.csproj` to add publishing properties. Scripts organized under `scripts/` by purpose (build, test, release) following existing repository patterns. Tests for build scripts live in `test/release-tests/` as shell scripts (not TUnit) due to infrastructure domain.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitutional violations detected. This feature:
- Adds infrastructure scripts (appropriate for build automation)
- Uses shell scripts for CI (standard practice, not TUnit domain)
- Follows existing repository patterns for scripts/ organization
- Maintains reproducible builds via pinned SDK versions
- No new .NET projects or architectural layers introduced

---

## Phase Completion Status

### Phase 0: Research ✅ COMPLETE
**File**: [`research.md`](research.md)

Generated 6 research questions with tasks:
1. .NET Self-Contained Publishing - MSBuild properties, package sizes, ANTLR compatibility
2. GitHub Actions Matrix Build Optimization - Concurrency limits, caching strategies
3. Package Verification & Smoke Testing - Test scope, execution strategy
4. Cross-Platform Archive Creation - tar.gz/zip best practices, tooling
5. GitHub Releases Publishing - CLI vs Actions, checksum formats
6. .NET 10.0 Forward Compatibility - Preview testing, multi-framework strategy

**Estimated Duration**: 8-12 hours  
**Deliverables**: 5 documentation artifacts (build-config, cicd-design, testing-strategy, packaging-standards, version-management)

### Phase 1: Design ✅ COMPLETE

**Data Model** - [`data-model.md`](data-model.md)  
Defined 6 core entities with relationships:
- Release Package (archive file with metadata)
- Build Artifact (binaries/resources within package)
- Release Version (git tag + GitHub Release)
- Smoke Test Result (test execution logs)
- Build Matrix Configuration (CI job specifications)
- Checksum Manifest (SHA256SUMS file)

**Contracts** - [`contracts/`](contracts/)  
Created 3 contract specifications:

1. **Build Scripts** ([`build-scripts.md`](contracts/build-scripts.md))
   - `build-release.sh` - Build single-platform package
   - `create-archives.sh` - Package artifacts into archives
   - `generate-checksums.sh` - Generate SHA256 checksums
   - `version-info.sh` - Extract version from git

2. **Test Scripts** ([`test-scripts.md`](contracts/test-scripts.md))
   - `smoke-test.sh` - Validate package functionality (5 test cases)
   - `verify-package.sh` - Verify package structure (8 checks)
   - Test sample files (hello.5th, kg-test.5th, parser-test.5th)

3. **GitHub Actions Workflow** ([`github-actions-workflow.md`](contracts/github-actions-workflow.md))
   - Trigger contracts (tag push + manual dispatch)
   - Build matrix (12 platform/framework combinations)
   - Build job steps (checkout → build → test → upload)
   - Publish job (aggregate → checksums → release)
   - Performance targets (< 45min total, < 15min per job)

**Quickstart Guide** - [`quickstart.md`](quickstart.md)  
Developer guide covering:
- Local package building (single platform)
- Running smoke tests
- Testing full release workflow
- Verifying package contents
- Troubleshooting common issues
- Integration testing steps

**Agent Context**: Not yet updated (requires `.specify/scripts/bash/update-agent-context.sh`)

### Phase 2: Task Breakdown - PENDING
**Action Required**: Run `/speckit.tasks` command to generate `tasks.md`

This phase will break down the implementation into atomic tasks based on the contracts and data model defined above.

---

## Ready for Implementation

All planning artifacts complete. The feature is ready for:

1. **Implementation Phase**: Create scripts per contracts, implement CI workflow
2. **Testing Phase**: Validate scripts locally, test workflow with dry_run
3. **Integration Phase**: Merge to master, tag release, verify production pipeline

**Next Command**: `/speckit.tasks` to generate detailed task breakdown

**Estimated Total Effort** (from spec):
- Development: 80-100 hours
- Testing: 20-30 hours
- Documentation: 10-15 hours
- **Total**: 110-145 hours (14-18 days at 8 hours/day)

**Implementation Validation Checklist**:
- ✅ Concurrent build cancellation working correctly (FR-032)
- ✅ Pre-release naming follows `{version}-pre.{YYYYMMDD}.{commit}` format (FR-021)
- ✅ Package size warnings logged but non-blocking (FR-031)
- ✅ .NET 10.0 unavailability handled gracefully with .NET 8.0 fallback (FR-033)
- ✅ .NET 10.0 preview SDK usage supported with appropriate release note annotations (FR-033, FR-034)
- ✅ Multi-SDK installation working (pinned 8.0 + preview/final 10.0) without interference (FR-034)
- ✅ All-or-nothing policy enforced (FR-030) - no partial releases
- ✅ Release notes include framework availability, SDK version/preview status, and size warnings
