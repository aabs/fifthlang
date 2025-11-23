# Implementation Tasks: Multi-Platform Release Packaging

**Branch**: `014-release-packaging-system` | **Date**: 2025-11-23  
**Feature**: Multi-platform, multi-framework release packaging system  
**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Task Format

Tasks use the following format: `- [ ] [T###] [P] [US#] Description with file path`

- **T###**: Task number (sequential)
- **[P]**: Parallel execution marker (can be done concurrently with other [P] tasks)
- **US#**: User story reference (US1-US5, or FOUND for foundational, SETUP for project setup)
- **Description**: Clear, actionable description with specific file path when applicable

## Summary Statistics

- **Total Tasks**: 64
- **User Story 1 (P1)**: 18 tasks - Automated release publishing
- **User Story 2 (P1)**: 8 tasks - End user installation
- **User Story 3 (P2)**: 5 tasks - Manual release creation
- **User Story 4 (P3)**: 6 tasks - Build monitoring
- **User Story 5 (P3)**: 4 tasks - Installation verification
- **Setup**: 4 tasks
- **Foundational**: 5 tasks
- **Polish**: 14 tasks
- **Parallel Opportunities**: 38 tasks can be executed in parallel

## MVP Scope

**Minimum Viable Product**: User Story 1 only (18 tasks)
- Automated build, test, and publish pipeline
- All 12 platform/framework combinations
- Smoke testing per package
- GitHub Releases publishing with checksums

This provides core value: users can download and use Fifth compiler packages automatically built from master commits.

---

## Phase 1: Setup (4 tasks)

**Purpose**: Initialize project structure, dependencies, and development environment.

- [x] [T001] [P] [SETUP] Create `scripts/build/` directory for build automation scripts
- [x] [T002] [P] [SETUP] Create `scripts/test/` directory for testing scripts  
- [x] [T003] [P] [SETUP] Create `scripts/release/` directory for release management scripts
- [x] [T004] [SETUP] Create `test/release-tests/` directory and sample Fifth programs (hello.5th, kg-test.5th, parser-test.5th) - depends on T001-T003

---

## Phase 2: Foundational (5 tasks)

**Purpose**: Build blocking prerequisites required by all user stories.

- [x] [T005] [P] [FOUND] Create `scripts/release/version-info.sh` implementing version extraction from git tags with pre-release format support (FR-021: `{base}-pre.{YYYYMMDD}.{commit}`)
- [x] [T006] [P] [FOUND] Modify `src/compiler/compiler.csproj` to add PublishSingleFile and RuntimeIdentifier properties for self-contained deployment
- [x] [T007] [FOUND] Create test sample `test/release-tests/test-samples/hello.5th` with basic main() function printing "Hello, World!" - depends on T004
- [x] [T008] [FOUND] Create test sample `test/release-tests/test-samples/kg-test.5th` with TriG/SPARQL literals and KG.CreateGraph() usage - depends on T004
- [x] [T009] [FOUND] Create test sample `test/release-tests/test-samples/parser-test.5th` with class declarations, functions, and control flow - depends on T004

---

## Phase 3: User Story 1 - Automated Release Publishing (P1) (18 tasks)

**Purpose**: Enable automated multi-platform release package creation from master commits.

**Acceptance Criteria**:
- ✅ Pushing tag to master triggers automated build
- ✅ All 12 platform/framework combinations built in parallel
- ✅ Each package smoke tested before publication
- ✅ GitHub Release created with all packages and checksums
- ✅ Build completes within 45 minutes
- ✅ All-or-nothing policy enforced (FR-030)

**Independent Test Criteria**:
- Mock build script inputs (version, runtime, framework) and verify archive creation
- Test version-info.sh with various git states (tagged, untagged, dirty) and verify pre-release format
- Validate checksum generation produces correct SHA256 hashes
- Verify workflow matrix definition includes all 12 combinations
- Test SDK availability detection and graceful degradation (FR-033)

### Build Script Implementation (5 tasks)

- [x] [T010] [US1] Create `scripts/build/build-release.sh` - build single-platform package with dotnet publish, accept --version, --runtime, --framework, --output-dir, emit JSON output per contract in `contracts/build-scripts.md`
- [x] [T011] [US1] Create `scripts/build/create-archives.sh` - package artifacts into tar.gz/zip, accept --source-dir, --output-file, --format, --version, preserve executable permissions, emit JSON output per contract in `contracts/build-scripts.md`
- [x] [T012] [US1] Create `scripts/build/generate-checksums.sh` - generate SHA256SUMS file, accept --package-dir, --output-file, follow GNU format, emit JSON output per contract in `contracts/build-scripts.md`
- [x] [T013] [US1] Add executable permissions to build scripts: `chmod +x scripts/build/*.sh`
- [x] [T014] [US1] Test build-release.sh locally for linux-x64 net8.0 variant, verify archive created and version-info.sh integration works - depends on T010, T005

### Test Script Implementation (4 tasks)

- [x] [T015] [US1] Create `scripts/test/smoke-test.sh` - implement 5 test cases (version_check, compile_hello_world, execute_hello_world, compile_with_kg_features, parser_grammar_loaded), accept --package-path, --test-dir, emit JSON output per contract in `contracts/test-scripts.md` - depends on T007, T008, T009
- [x] [T016] [US1] Create `scripts/test/verify-package.sh` - implement 8 verification checks (archive extraction, executable presence, permissions, LICENSE, README, VERSION.txt, directory structure, size limits), accept --package-path, emit JSON output with soft limit handling (FR-031) per contract in `contracts/test-scripts.md`
- [x] [T017] [US1] Add executable permissions to test scripts: `chmod +x scripts/test/*.sh`
- [x] [T018] [US1] Test smoke-test.sh with locally built linux-x64 package, verify all 5 test cases execute and report correctly - depends on T015, T014

### GitHub Actions Workflow Implementation (9 tasks)

- [x] [T019] [US1] Create `.github/workflows/release.yml` with trigger configuration (tag push pattern `v[0-9]+.[0-9]+.[0-9]+*` and workflow_dispatch with dry_run input) per contract in `contracts/github-actions-workflow.md`
- [x] [T020] [US1] Add concurrency control to `.github/workflows/release.yml` with group `release-${{ github.ref }}` and `cancel-in-progress: true` (FR-032) per contract in `contracts/github-actions-workflow.md` - depends on T019
- [x] [T021] [US1] Define build matrix in `.github/workflows/release.yml` with all 12 platform/framework combinations (6 platforms × 2 frameworks), set `fail-fast: false` for diagnostics per contract in `contracts/github-actions-workflow.md` - depends on T019
- [x] [T022] [US1] Implement build job steps in `.github/workflows/release.yml`: checkout (fetch-depth: 0), setup-dotnet (global.json for pinned 8.0 SDK), setup-dotnet (10.0.x for preview/final .NET 10 SDK with include-prerelease: true), setup-java (17, temurin), detect available framework SDKs with `dotnet --list-sdks`, conditionally skip net10.0 builds if SDK unavailable (FR-033), extract version, build package for target framework, verify package, smoke test, upload artifact with framework metadata - depends on T021, T010, T011, T015, T016, T005
- [x] [T023] [US1] Implement publish job in `.github/workflows/release.yml` with `needs: [build]` and `if: ${{ success() }}` to enforce all-or-nothing policy (FR-030), download artifacts, check framework coverage (12 packages expected, 6 acceptable if .NET 10 SDK unavailable), detect SDK versions used (including preview versions), check package sizes with soft limit warnings (FR-031), generate checksums, generate release notes with framework/SDK availability warnings (including preview SDK usage), create GitHub Release - depends on T022, T012
- [x] [T024] [US1] Add pre-release detection logic to `.github/workflows/release.yml` publish job (check if version contains suffix like -alpha, -beta, -rc) per contract in `contracts/github-actions-workflow.md` - depends on T023
- [x] [T025] [US1] Test workflow locally using `act` (`act workflow_dispatch -W .github/workflows/release.yml -n -j build --matrix '{"os":"ubuntu-22.04","framework":"net8.0"}'`) - depends on T023
- [ ] [T026] [US1] Create test tag `v0.9.0-test` and push to test branch to validate workflow triggers, matrix parallelism, and all-or-nothing policy - depends on T024
- [ ] [T027] [US1] Validate workflow run: verify all 12 jobs execute, smoke tests pass, publish job creates release with checksums, concurrency control works, framework coverage detected - depends on T026

---

## Phase 4: User Story 2 - End User Installation (P1) (8 tasks)

**Purpose**: Enable users to easily download and install Fifth compiler from GitHub Releases.

**Acceptance Criteria**:
- ✅ Users can find appropriate package for their platform
- ✅ Packages extract with correct permissions
- ✅ Compiler executable runs without additional setup
- ✅ Installation verified with `fifth --version`
- ✅ Installation takes < 5 minutes

**Independent Test Criteria**:
- Download each platform package and verify extraction
- Test executable permissions on Unix platforms
- Verify `fifth --version` works without configuration
- Test installation on clean VM for each platform
- Validate documentation clarity with non-technical users

### Documentation (6 tasks)

- [ ] [T028] [P] [US2] Create `docs/installation.md` with download instructions, platform-specific extraction steps (tar xzf for Unix, Expand-Archive for Windows), PATH setup, verification with `fifth --version`, troubleshooting section (NFR-018: 8th-grade readability)
- [ ] [T029] [P] [US2] Create `docs/package-structure.md` documenting archive directory layout (`bin/`, `lib/`, LICENSE, README.md, VERSION.txt), file descriptions, size expectations
- [ ] [T030] [P] [US2] Update `README.md` to add "Installation" section linking to `docs/installation.md`, highlight supported platforms, add quick start example
- [ ] [T031] [P] [US2] Create `docs/debugging.md` with common installation issues (permission errors, missing runtime, PATH configuration), platform-specific troubleshooting
- [ ] [T032] [US2] Update `docs/installation.md` to include SHA256 checksum verification steps for security-conscious users (NFR-014) - depends on T028
- [ ] [T033] [US2] Add release notes template guidance to `docs/installation.md` explaining framework availability warnings and package size annotations - depends on T028

### Package Validation (2 tasks)

- [ ] [T034] [US2] Test installation flow on Windows 10 x64: download zip, extract, add to PATH, run `fifth --version`, compile hello.5th - depends on T027
- [ ] [T035] [US2] Test installation flow on macOS 11+ ARM64: download tar.gz, extract, verify permissions, add to PATH, run `fifth --version`, compile hello.5th - depends on T027

---

## Phase 5: User Story 3 - Manual Release Creation (P2) (5 tasks)

**Purpose**: Allow developers to manually trigger releases for milestones and hotfixes.

**Acceptance Criteria**:
- ✅ Workflow can be manually triggered from Actions UI
- ✅ Dry run mode validates build without publishing
- ✅ Version input overrides git tag detection
- ✅ Manual release follows same quality gates as automatic

**Independent Test Criteria**:
- Trigger workflow_dispatch with custom version and dry_run=true
- Verify no release created but artifacts generated
- Trigger workflow_dispatch with dry_run=false and verify release published
- Test version override (non-tag version) produces correct package names

### Manual Trigger Implementation (5 tasks)

- [ ] [T036] [US3] Verify workflow_dispatch trigger configuration in `.github/workflows/release.yml` includes `version` (string, required) and `dry_run` (boolean, default false) inputs - should already be present from T019
- [ ] [T037] [US3] Add dry run handling to publish job in `.github/workflows/release.yml`: skip release creation if `inputs.dry_run == true`, log "DRY RUN: Would publish..." with package list and checksums - depends on T023
- [ ] [T038] [US3] Add version override logic to build job in `.github/workflows/release.yml`: use `inputs.version` if workflow_dispatch triggered, else use version-info.sh output - depends on T022
- [ ] [T039] [US3] Document manual release process in `docs/release-process.md`: when to use manual releases (milestones, hotfixes), workflow_dispatch steps, dry run testing, version naming conventions
- [ ] [T040] [US3] Test manual workflow dispatch with version "0.9.1-rc.1" and dry_run=true, verify build matrix executes, no release created, artifacts available for 7 days - depends on T037, T038

---

## Phase 6: User Story 4 - Build Monitoring (P3) (6 tasks)

**Purpose**: Enable CI administrators to monitor build health and diagnose failures.

**Acceptance Criteria**:
- ✅ Build logs include structured information (platform, framework, duration, status)
- ✅ Failed builds provide actionable error messages
- ✅ Metrics available for build times and package sizes
- ✅ Administrators can identify patterns in failures

**Independent Test Criteria**:
- Parse build logs for structured data (JSON output from scripts)
- Verify error messages include platform, framework, failure reason
- Extract build time and package size metrics from logs
- Test log aggregation for trend analysis

### Logging and Observability (6 tasks)

- [ ] [T041] [P] [US4] Add structured logging to `scripts/build/build-release.sh`: emit JSON events to stdout for key stages (restore, build, publish, archive), include timestamps, platform, framework per NFR-022 - depends on T010
- [ ] [T042] [P] [US4] Add structured logging to `scripts/test/smoke-test.sh`: emit JSON events for each test case (start, pass/fail, duration), include test output and error details per NFR-022 - depends on T015
- [ ] [T043] [P] [US4] Add error diagnostic logging to all build scripts: emit structured error messages to stderr with error type, affected platform, remediation steps per NFR-024 - depends on T010, T011, T012, T005
- [ ] [T044] [US4] Add build metrics collection to `.github/workflows/release.yml` publish job: parse JSON outputs from build jobs, aggregate build times, package sizes, test results, emit summary to $GITHUB_STEP_SUMMARY per NFR-023 - depends on T023
- [ ] [T045] [US4] Create `scripts/release/analyze-build-logs.sh` to parse GitHub Actions logs and extract metrics (build times per platform, package sizes, test pass rates, failure patterns) for offline analysis
- [ ] [T046] [US4] Document monitoring workflow in `docs/release-process.md`: how to access build logs, interpret structured output, diagnose common failures, extract metrics for trend analysis - depends on T039

---

## Phase 7: User Story 5 - Installation Verification (P3) (4 tasks)

**Purpose**: Allow users to verify successful installation and package integrity.

**Acceptance Criteria**:
- ✅ Users can verify installation with `fifth --version`
- ✅ Users can verify package integrity with checksums
- ✅ Verification takes < 1 minute
- ✅ Clear success/failure indicators

**Independent Test Criteria**:
- Test `fifth --version` on fresh installation
- Download SHA256SUMS and verify each package checksum
- Test verification on corrupted package (expect failure)
- Validate verification instructions are clear

### Verification Tools and Documentation (4 tasks)

- [ ] [T047] [P] [US5] Add checksum verification example to `docs/installation.md`: download SHA256SUMS, use `sha256sum -c` (Unix) or `Get-FileHash` (Windows), interpret results - depends on T028
- [ ] [T048] [P] [US5] Create `scripts/test/verify-installation.sh` user-facing script: check `fifth` executable exists, verify `fifth --version` works, optionally verify package checksum, emit pass/fail status
- [ ] [T049] [US5] Document installation verification workflow in `docs/installation.md`: run verify-installation.sh, interpret output, common issues (wrong PATH, corrupted download) - depends on T047, T048
- [ ] [T050] [US5] Test complete user workflow: download package, extract, verify checksums, run verify-installation.sh, compile hello.5th, execute compiled program - depends on T048, T034, T035

---

## Phase 8: Polish & Cross-Cutting Concerns (14 tasks)

**Purpose**: Final quality improvements, documentation refinement, and comprehensive validation.

### Testing & Validation (6 tasks)

- [ ] [T051] [P] [POLISH] Create unit tests for version-info.sh: test tagged commits, untagged commits, pre-release format generation, dirty state detection, CI environment variable handling in `test/release-tests/test-version-info.sh`
- [ ] [T052] [P] [POLISH] Create integration test for full release workflow in `test/release-tests/integration-release.sh`: mock git repo with tag, run build-release.sh, verify-package.sh, smoke-test.sh sequence, validate outputs
- [ ] [T053] [P] [POLISH] Add negative tests for build scripts: invalid arguments, missing dependencies, corrupt source files, insufficient disk space - verify correct exit codes and error messages
- [ ] [T054] [POLISH] Test all-or-nothing policy (FR-030): force failure in one matrix job, verify publish job is skipped, no partial releases created - depends on T027
- [ ] [T055] [POLISH] Test SDK graceful degradation (FR-033): mock .NET 10.0 SDK unavailability, verify only .NET 8.0 packages built (6 total), framework warning added to release notes - depends on T027
- [ ] [T056] [POLISH] Run full regression: push test tag, verify all 12 packages built, all smoke tests passed, checksums correct, release published, concurrency control works, size warnings logged - depends on T027

### Documentation Finalization (5 tasks)

- [ ] [T057] [P] [POLISH] Add release workflow diagram to `docs/release-process.md`: visual representation of trigger → build matrix → smoke test → publish pipeline
- [ ] [T058] [P] [POLISH] Create `docs/release-troubleshooting.md`: common CI failures (quota exceeded, artifact upload failed, network timeout), diagnostic steps, remediation procedures
- [ ] [T059] [P] [POLISH] Add FAQ section to `docs/installation.md`: which package to download, how to verify checksums, what if package too large, what if framework unavailable, how to uninstall
- [ ] [T060] [POLISH] Review all documentation for NFR-018 compliance (8th-grade readability): run through readability checker, simplify technical jargon, add glossary if needed - depends on T028, T029, T030, T031, T039, T057, T058, T059
- [ ] [T061] [POLISH] Update constitution (`.specify/memory/constitution.md`) to document release packaging patterns: build script conventions, test script contracts, artifact naming standards, release process governance

### Security & Compliance (3 tasks)

- [ ] [T062] [P] [POLISH] Audit bundled dependencies for license compliance: verify ANTLR, dotNetRDF, and all transitive dependencies have compatible licenses, add attribution to LICENSE file per regulatory constraints
- [ ] [T063] [P] [POLISH] Verify GitHub token permissions in `.github/workflows/release.yml`: ensure minimal required permissions (contents: write for releases), document in workflow comments per NFR-013
- [ ] [T064] [POLISH] Security scan of release packages: run `dotnet list package --vulnerable` on published artifacts, verify no critical vulnerabilities, document remediation process in `docs/release-process.md` - depends on T027

---

## Dependency Graph

### Critical Path (longest sequential chain)
T001 → T004 → T007 → T015 → T018 → T022 → T023 → T024 → T026 → T027 → T034 → T056
**Estimated Duration**: ~40-50 hours (assumes dependencies complete before dependents start)

### Parallelization Opportunities

**Phase 1 (Setup)**: T001, T002, T003 can run in parallel (3 tasks)  
**Phase 2 (Foundational)**: T005, T006 can run in parallel (2 tasks); T007, T008, T009 can run in parallel after T004 (3 tasks)  
**Phase 3 (US1)**: T010, T011, T012 can run in parallel (3 tasks); T015, T016 can run in parallel after T010-T012 (2 tasks)  
**Phase 4 (US2)**: T028, T029, T030, T031 can run in parallel (4 tasks); T034, T035 can run in parallel after T027 (2 tasks)  
**Phase 6 (US4)**: T041, T042, T043 can run in parallel (3 tasks)  
**Phase 7 (US5)**: T047, T048 can run in parallel (2 tasks)  
**Phase 8 (Polish)**: T051, T052, T053 can run in parallel (3 tasks); T057, T058, T059 can run in parallel (3 tasks); T062, T063 can run in parallel (2 tasks)

**Total Parallel Tasks**: 38 of 64 tasks (59%) can be executed concurrently in their respective phases

---

## Implementation Order Recommendation

For maximum efficiency, implement in this order:

1. **Setup & Foundational** (T001-T009): Establish project structure and core dependencies
2. **Build Scripts** (T010-T014): Core build automation
3. **Test Scripts** (T015-T018): Validation infrastructure  
4. **GitHub Actions** (T019-T027): CI/CD pipeline
5. **Documentation** (T028-T033): User-facing guides
6. **Manual Triggers** (T036-T040): Flexibility for releases
7. **Monitoring** (T041-T046): Observability
8. **Verification** (T047-T050): User confidence
9. **Polish** (T051-T064): Quality and compliance

## Task Status Tracking

To track progress:
1. Copy this file to `tasks.md` in the spec directory
2. Check off tasks as completed: `- [x] [T###] ...`
3. Update commit messages with task references: `[T015] Implement smoke-test.sh`
4. Link PRs to tasks in PR description

## Estimated Effort

**By Phase**:
- Phase 1 (Setup): 2 hours
- Phase 2 (Foundational): 6 hours
- Phase 3 (US1): 35 hours (critical path)
- Phase 4 (US2): 12 hours
- Phase 5 (US3): 8 hours
- Phase 6 (US4): 10 hours
- Phase 7 (US5): 6 hours
- Phase 8 (Polish): 25 hours

**Total Estimated Effort**: 104 hours (~13 days at 8 hours/day)

**With Parallelization**: ~70 hours wall time (~9 days) assuming 2-3 developers working concurrently

---

## Notes

- Tasks marked with [P] can be implemented in parallel within their phase
- Each task includes file path for implementation work
- Dependencies explicitly noted where tasks must be sequential
- Independent test criteria specified for each user story phase
- MVP scope (US1 only) provides deployable value in 35 hours
