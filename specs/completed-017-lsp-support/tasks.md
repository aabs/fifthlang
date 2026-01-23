---

description: "Task list for LSP server implementation"
---

# Tasks: Language Server Protocol (LSP) for IDE Integration

**Input**: Design documents from /specs/001-lsp-server/
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Follow Test-First per constitution; use existing smoke tests where applicable.

## Format: [ID] [P?] [Story] Description

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm structure and ensure foundational wiring is in place.

**Covers**: FR-001, FR-002, FR-008, FR-009, FR-011, FR-013

- [X] T001 Verify language server project references and package versions in src/language-server/Fifth.LanguageServer.csproj
- [X] T002 [P] Confirm stdio server entry point and handler registration in src/language-server/Program.cs
- [X] T003 [P] Review smoke test coverage in test/language-server-smoke/SmokeTests.cs and ParsingServiceTests.cs
- [X] T004 [P] Verify document tracking + full-text sync behavior in src/language-server/DocumentService.cs and src/language-server/Handlers/DocumentSyncHandler.cs
- [X] T005 [P] Verify single-root + local-only assumptions in src/language-server/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story work.

- [X] T006 Add structured stderr logging baseline in src/language-server/Program.cs
- [X] T007 [P] Register SymbolService in DI container in src/language-server/Program.cs
- [X] T008 [P] Add DefinitionHandler skeleton in src/language-server/Handlers/DefinitionHandler.cs
- [X] T009 Wire DefinitionHandler into server registration in src/language-server/Program.cs
- [X] T010 [P] Add defensive empty-result handling helpers in src/language-server/Handlers/

**Checkpoint**: Foundational wiring complete (logging + definition handler registration)

---

## Phase 3: User Story 1 - Live Diagnostics While Editing (Priority: P1) 🎯 MVP

**Goal**: Real-time syntax + semantic diagnostics for open documents.

**Independent Test**: Open a .5th file, introduce syntax/semantic errors, and verify diagnostics update without saving.

### Implementation for User Story 1

- [X] T011 [P] [US1] Add smoke test for diagnostics updates in test/language-server-smoke/SmokeTests.cs
- [X] T012 [US1] Extend ParsedDocument to track semantic diagnostics in src/language-server/Parsing/ParsingService.cs
- [X] T013 [US1] Add semantic diagnostic extraction using compiler pipeline in src/language-server/Parsing/ParsingService.cs
- [X] T014 [US1] Update DocumentService to preserve semantic diagnostics in src/language-server/DocumentService.cs
- [X] T015 [US1] Publish combined diagnostics (syntax + semantic) in src/language-server/Handlers/DocumentSyncHandler.cs
- [X] T016 [US1] Verify open-document-only diagnostics behavior in src/language-server/Handlers/DocumentSyncHandler.cs
- [X] T017 [US1] Verify diagnostics update without rebuild in src/language-server/Handlers/DocumentSyncHandler.cs

**Checkpoint**: Diagnostics include semantic errors for open documents only.

---

## Phase 4: User Story 2 - Understand Symbols in Context (Priority: P2)

**Goal**: Symbol-aware hover and go-to-definition across workspace documents (including unopened files).

**Independent Test**: Hover a symbol for type/signature and jump to its definition in the same file or another workspace file, including unopened files.

### Implementation for User Story 2

- [X] T018 [P] [US2] Add smoke test for hover and definition across unopened workspace files in test/language-server-smoke/SmokeTests.cs
- [X] T019 [US2] Upgrade SymbolService to resolve symbols using AST/symbol tables in src/language-server/SymbolService.cs
- [X] T020 [US2] Add workspace-wide definition index (open + on-disk files) in src/language-server/SymbolService.cs
- [X] T021 [US2] Implement DefinitionHandler using SymbolService in src/language-server/Handlers/DefinitionHandler.cs
- [X] T022 [US2] Replace placeholder hover with symbol-aware hover content in src/language-server/Handlers/HoverHandler.cs

**Checkpoint**: Hover shows symbol info and go-to-definition resolves definitions.

---

## Phase 5: User Story 3 - Productive Editing with Completions (Priority: P3)

**Goal**: Context-aware completions for keywords and in-scope symbols.

**Independent Test**: Request completion at an identifier position and see keywords + relevant symbols.

### Implementation for User Story 3

- [X] T023 [P] [US3] Add smoke test for completion results in test/language-server-smoke/SmokeTests.cs
- [X] T024 [US3] Add symbol-based completion items in src/language-server/Handlers/CompletionHandler.cs
- [X] T025 [US3] Use SymbolService to filter completions by scope in src/language-server/Handlers/CompletionHandler.cs
- [X] T026 [US3] Add signature detail to function completions in src/language-server/Handlers/CompletionHandler.cs

**Checkpoint**: Completion returns relevant symbols and signatures.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Refinements that affect multiple stories.

**Covers**: FR-007

- [X] T027 [P] Update quickstart guidance if behaviors changed in specs/001-lsp-server/quickstart.md
- [X] T028 [P] Verify graceful empty-result handling across handlers in src/language-server/Handlers/
- [X] T029 Run local smoke validation with existing tests in test/language-server-smoke/LanguageServerSmoke.csproj

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup → Foundational → US1 → US2 → US3 → Polish

### User Story Dependencies

- US1 (P1): Depends on Foundational
- US2 (P2): Depends on Foundational and benefits from US1 diagnostics context
- US3 (P3): Depends on Foundational and SymbolService upgrades from US2

### Parallel Opportunities

- Phase 1: T002, T003, T004, T005 can run in parallel
- Phase 2: T007 and T008 can run in parallel
- Phase 6: T027 and T028 can run in parallel

---

## Parallel Example: User Story 2

- Task: "Upgrade SymbolService in src/language-server/SymbolService.cs"
- Task: "Implement DefinitionHandler in src/language-server/Handlers/DefinitionHandler.cs"

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational
2. Implement US1 diagnostics pipeline
3. Validate diagnostics against open documents

### Incremental Delivery

1. Add US2 hover/definition
2. Add US3 completions
3. Polish and update smoke tests

---

## Notes

- [P] tasks = different files, no dependencies
- [US#] labels map to user stories in spec.md
- Keep tasks independently completable per story
