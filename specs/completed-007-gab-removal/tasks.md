# Tasks: Remove Graph Assertion Block (GAB)

**Input**: Design documents from `/specs/001-gab-removal/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

Tests: Optional. Spec clarifies removal of GAB-specific tests; do not add new negative tests.

## Format: `[ID] [P?] [Story] Description`

- [P]: Can run in parallel (different files, no dependencies)
- [Story]: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Ensure docs align with clarifications and remove obsolete planning artifacts.

- [ ] T001 Update plan summary to remove mention of adding negative tests in `specs/001-gab-removal/plan.md`
- [ ] T002 Remove obsolete contract doc for negative tests in `specs/001-gab-removal/contracts/negative-parser-tests.md`
- [ ] T003 [P] Confirm prerequisites per constitution in `.specify/memory/constitution.md` (toolchain, build order)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Baseline verification before user story work.

- [ ] T004 Run initial build to confirm current green state: `fifthlang.sln`
- [ ] T005 [P] Run quick example validation script `scripts/validate-examples.fish` to capture current baseline

**Checkpoint**: Foundation ready — proceed to user stories.

---

## Phase 3: User Story 1 — Compiler rejects GAB syntax (Priority: P1) 🎯 MVP

**Goal**: Language no longer accepts GAB; grammar/AST/visitor updated; standard errors only.

**Independent Test**: Grammar and AST contain no GAB constructs; repository builds; example validation passes.

### Implementation for User Story 1

- [ ] T006 [P] [US1] Remove GAB tokens/rules from lexer `src/parser/grammar/FifthLexer.g4`
- [ ] T007 [P] [US1] Remove GAB rules from parser `src/parser/grammar/FifthParser.g4`
- [ ] T008 [US1] Remove GAB visit/construct handling from `src/parser/AstBuilderVisitor.cs`
- [ ] T009 [US1] Delete GAB AST node definitions from `src/ast-model/AstMetamodel.cs`
- [ ] T010 [US1] Regenerate AST generated code via `src/ast_generator/` (outputs under `src/ast-generated/`)
- [ ] T011 [P] [US1] Remove any GAB references in compiler passes under `src/compiler/LanguageTransformations/`
- [ ] T012 [US1] Build full solution `fifthlang.sln` and fix compilation issues limited to GAB removal
- [ ] T013 [P] [US1] Run syntax parser tests `test/syntax-parser-tests/`
- [ ] T014 [US1] Remove any unit/parser tests that depend on GAB constructs under `test/` folders

**Checkpoint**: US1 complete — GAB removed; builds pass; examples validate.

---

## Phase 4: User Story 2 — Docs and examples contain no GAB (Priority: P2)

**Goal**: No user-facing references to GAB remain; examples parse.

**Independent Test**: Repo-wide example validator passes with zero GAB references.

### Implementation for User Story 2

- [ ] T015 [P] [US2] Remove GAB references from `docs/knowledge-graphs.md`
- [ ] T016 [P] [US2] Update constitution to remove GAB mentions in `.specify/memory/constitution.md`
- [ ] T017 [P] [US2] Sweep examples and docs for GAB and remove occurrences in `docs/`, `specs/`, and `src/parser/grammar/test_samples/`
- [ ] T018 [US2] Run validator `scripts/validate-examples.fish` and resolve any remaining GAB references in docs/examples

**Checkpoint**: US2 complete — docs/examples clean; validator passes.

---

## Phase 5: User Story 3 — RDF features remain intact (Priority: P3)

**Goal**: Triple literals, RDF datatypes, and store declarations continue to work.

**Independent Test**: RDF smoke and runtime tests pass.

### Implementation for User Story 3

- [ ] T019 [P] [US3] Run RDF smoke tests `test/kg-smoke-tests/`
- [ ] T020 [P] [US3] Run runtime integration tests `test/runtime-integration-tests/`
- [ ] T021 [US3] Address any regressions strictly related to RDF features in `src/parser/`, `src/ast-model/`, and `fifthlang.system/`

**Checkpoint**: US3 complete — RDF passes unchanged.

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Final clean-up and repository hygiene.

- [ ] T022 [P] Update high-level docs to reflect removal in `README.md` and `AGENTS.md`
- [ ] T023 [P] Grep repository for residual tokens like "GAB", "graph assertion block" and remove (focus files under `docs/`, `specs/`, `src/`)
- [ ] T024 Full test sweep `fifthlang.sln` (all test projects) and ensure compliance with constitution
- [ ] T025 [P] Ensure no edits under `src/ast-generated/`; verify regeneration provenance in commit notes
 - [ ] T026 Verify no user flags/configs enable GAB (FR-008); grep for feature flags and update docs if any found

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup → Foundational → User Stories → Polish
- User Stories can proceed in priority order; US2 and US3 may start after Foundational but US1 changes may affect them; prefer US1 first.

### User Story Dependencies

- US1: None (after Foundational)
- US2: Depends on US1 for grammar/AST removal to avoid doc churn
- US3: Can run after Foundational but validates better post-US1

### Parallel Opportunities

- [P] Lexer and Parser removals (T006, T007) can proceed in parallel
- [P] Visitor cleanup (T008) can proceed while AST node deletion is prepared but commit after T009
- [P] Compiler pass reference cleanup (T011), syntax tests (T013) can run in parallel after T010
- [P] Docs sweeps (T015–T017) can be performed concurrently by different contributors
- [P] RDF test runs (T019, T020) in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational (T001–T005)
2. Implement US1 (T006–T014)
3. Validate via build and example validation

### Incremental Delivery

- Deliver US1 → validate
- Deliver US2 → validate
- Deliver US3 → validate

