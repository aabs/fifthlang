---

description: "Task list for full MSBuild support"
---

# Tasks: Full MSBuild Support

**Input**: Design documents from `/specs/001-full-msbuild-support/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are required by constitution. Include test tasks per user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Minimal scaffolding needed before shared work

- [x] T001 Create allowlist file in src/Fifth.Sdk/Sdk/SupportedTargetFrameworks.props

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

- [x] T002 Update output path properties in src/Fifth.Sdk/Sdk/Sdk.props
- [x] T003 [P] Add output-type and reference arguments in src/compiler/Program.cs
- [x] T004 [P] Store output type and references in src/compiler/CompilerOptions.cs
- [x] T005 Update Roslyn output type and references in src/compiler/Compiler.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Build projects with the right outputs (Priority: P1) 🎯 MVP

**Goal**: Produce deterministic executable or library artifacts based on project settings.

**Independent Test**: Build a single project configured as Exe and Library and verify output type and output path.

### Tests for User Story 1

- [x] T006 [P] [US1] Add output type build tests in test/fifth-sdk-tests/OutputTypeTests.cs

### Implementation for User Story 1

- [x] T007 [US1] Wire OutputType and TargetPath into FifthCompile in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T008 [US1] Ensure output extension mapping matches OutputType in src/Fifth.Sdk/Sdk/Sdk.targets

**Checkpoint**: User Story 1 is fully functional and independently testable

---

## Phase 4: User Story 2 - Build with project and package references (Priority: P2)

**Goal**: Resolve project and package references deterministically and fail on conflicts.

**Independent Test**: Build a solution with a project reference and a package reference; validate correct build order and resolved dependencies.

### Tests for User Story 2

- [x] T009 [P] [US2] Add reference resolution tests in test/fifth-sdk-tests/ReferenceResolutionTests.cs

### Implementation for User Story 2

- [x] T010 [US2] Pass resolved reference paths to compiler in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T011 [US2] Enforce project dependency ordering in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T012 [US2] Fail build on missing project or package dependencies in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T013 [US2] Detect circular project references and fail build in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T014 [US2] Fail build on package version conflicts in src/Fifth.Sdk/Sdk/Sdk.targets

**Checkpoint**: User Stories 1 and 2 are independently functional

---

## Phase 5: User Story 3 - Fast, repeatable builds across targets (Priority: P3)

**Goal**: Support incremental builds, multi-targeting, build events, and design-time manifests.

**Independent Test**: Build a multi-target project twice; ensure unchanged inputs skip work and outputs exist for each target.

### Tests for User Story 3

- [x] T015 [P] [US3] Add incremental and multi-target build tests in test/fifth-sdk-tests/IncrementalBuildTests.cs

### Implementation for User Story 3

- [x] T016 [US3] Add incremental build Inputs/Outputs and up-to-date checks in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T017 [US3] Execute PreBuildEvent/PostBuildEvent and fail on error in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T018 [US3] Enforce target framework allowlist and multi-targeting in src/Fifth.Sdk/Sdk/Sdk.targets
- [x] T019 [US3] Emit design-time manifest with sources, references, target framework, and defines in src/Fifth.Sdk/Sdk/Sdk.targets

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and cleanup across stories

- [x] T020 [P] Update capability documentation in src/Fifth.Sdk/README.md
- [x] T021 [P] Update design summary in docs/Designs/5thproj-implementation-summary.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2)
- **User Story 2 (P2)**: Can start after Foundational (Phase 2)
- **User Story 3 (P3)**: Can start after Foundational (Phase 2)

### Parallel Opportunities

- T003 and T004 can run in parallel (different files)
- T006, T009, and T015 can run in parallel (different files)
- T020 and T021 can run in parallel (different files)

---

## Parallel Example: User Story 2

```bash
Task: "Pass resolved reference paths to compiler in src/Fifth.Sdk/Sdk/Sdk.targets"
Task: "Detect circular project references and fail build in src/Fifth.Sdk/Sdk/Sdk.targets"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate User Story 1 independently

### Incremental Delivery

1. Setup + Foundational
2. User Story 1 → validate
3. User Story 2 → validate
4. User Story 3 → validate
5. Polish & documentation
