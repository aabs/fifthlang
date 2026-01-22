# Feature Specification: Language Server Protocol (LSP) for IDE Integration

**Feature Branch**: `001-lsp-server`  
**Created**: 2026-01-23  
**Status**: Draft  
**Input**: User description: "Implement Language Server Protocol (LSP) for IDE Integration"

## Clarifications

### Session 2026-01-23

- Q: Workspace scope for MVP? → A: Single-root workspace only.
- Q: Diagnostics scope for MVP? → A: Open documents only.
- Q: Document change handling for MVP? → A: Full-text sync only.
- Q: Observability baseline for MVP? → A: Structured logs to stderr only.
- Q: Security posture for MVP? → A: Local-only server, no authentication.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Live Diagnostics While Editing (Priority: P1)

As a developer, I want to see syntax and semantic errors as I type so I can fix issues immediately without running full builds.

**Why this priority**: Real-time diagnostics are the minimum expectation for modern IDE support and provide the biggest usability impact.

**Independent Test**: Can be tested by opening a file, introducing an error, and confirming the error appears and updates without saving.

**Acceptance Scenarios**:

1. **Given** an open Fifth file, **When** I introduce a syntax error, **Then** an error is shown for the correct line and updates as I edit.
2. **Given** an open Fifth file, **When** I fix a previously reported error, **Then** the error is cleared without requiring a full build.

---

### User Story 2 - Understand Symbols in Context (Priority: P2)

As a developer, I want hover and definition navigation so I can understand types, signatures, and where symbols are defined.

**Why this priority**: Navigation and hover reduce friction when reading unfamiliar code and speed up onboarding.

**Independent Test**: Can be tested by hovering a symbol and jumping to its definition within the workspace.

**Acceptance Scenarios**:

1. **Given** a symbol in a file, **When** I hover over it, **Then** I see its type and signature information.
2. **Given** a symbol reference, **When** I request “go to definition,” **Then** I am navigated to the correct definition even if it is in another file.

---

### User Story 3 - Productive Editing with Completions (Priority: P3)

As a developer, I want context-aware completions so I can write code faster and discover available symbols.

**Why this priority**: Completion improves productivity and helps new contributors learn the language and APIs.

**Independent Test**: Can be tested by requesting completion in an open document and verifying relevant items appear.

**Acceptance Scenarios**:

1. **Given** a partially typed identifier, **When** I request completion, **Then** I receive relevant keywords and symbols in scope.
2. **Given** a function name, **When** I request completion, **Then** I see the function signature in the completion detail.

### Edge Cases

- Edits are made in an unsaved buffer and must still update diagnostics and navigation results.
- A file contains multiple errors; all relevant errors should be reported without suppressing later issues.
- The workspace contains multiple files with the same symbol name; navigation resolves the correct definition by scope.
- Very large files should still provide diagnostics and completion without the editor freezing.
- A file is closed; diagnostics and symbols for that file should be cleared from the editor view.
- Diagnostics are not computed for unopened files; unopened files show no diagnostics until opened.
- Rapid successive edits should not queue partial updates; the latest full text replaces prior pending changes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide an IDE integration endpoint using the standard language server protocol so common editors can connect.
- **FR-002**: The system MUST track open documents and apply changes from editors without requiring files to be saved.
- **FR-003**: The system MUST provide real-time diagnostics for syntax and semantic errors in open documents.
- **FR-004**: The system MUST provide hover information that includes symbol type and signature where available.
- **FR-005**: The system MUST provide context-aware completion suggestions for keywords and in-scope symbols.
- **FR-006**: The system MUST support navigation to symbol definitions across files within the workspace.
- **FR-007**: The system MUST return graceful empty results (no errors) when a requested feature has no applicable data.
- **FR-008**: The system MUST keep results up to date as documents change, without requiring a full project rebuild for each edit.
- **FR-009**: The system MUST support a single-root workspace in the MVP.
- **FR-010**: The system MUST publish diagnostics for open documents only in the MVP.
- **FR-011**: The system MUST accept full-text document updates only (no incremental range edits) in the MVP.
- **FR-012**: The system MUST emit structured, deterministic logs to stderr for diagnostics and requests in the MVP.
- **FR-013**: The system MUST operate as a local-only server for the MVP with no authentication requirements.

### Key Entities *(include if feature involves data)*

- **Document**: A single Fifth source file opened in the editor, including its current unsaved content.
- **Workspace**: The set of Fifth files the editor considers part of the project.
- **Diagnostic**: A message that identifies an error or warning with location and severity.
- **Symbol**: A named definition or reference within the codebase (functions, classes, variables, types).
- **Completion Item**: A suggested keyword or symbol that can be inserted at the cursor.
- **Location**: A reference to where a symbol is defined within the workspace.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Diagnostics update within 1 second of an edit for files up to 2,000 lines.
- **SC-002**: Hover and definition requests return results in under 500 ms for typical project sizes (≤ 200 files).
- **SC-003**: At least 90% of completion requests in common coding positions return relevant suggestions on the first attempt.
- **SC-004**: A new contributor can connect an editor and receive diagnostics/hover/definition in under 5 minutes using documented steps.

## Scope & Assumptions

- The MVP includes diagnostics, hover, completion, and go-to-definition.
- The MVP supports single-root workspaces only.
- The MVP emits structured logs to stderr only; metrics and tracing are out of scope.
- The MVP is local-only with no authentication requirements.
- Standard IDEs are expected to use the language server protocol without custom extensions.
- Advanced features (references, rename, code actions, semantic tokens, signature help, formatting) are deferred to a later phase.

## Current Implementation Status

**In place**:
- Stdio language server entry point with OmniSharp LSP integration.
- Full-text document sync (open/change/save/close) with in-memory storage.
- Syntax diagnostics from the parser (open documents only).
- Basic hover handler (line-echo placeholder).
- Basic keyword completions.
- Smoke tests in `test/language-server-smoke/` (ParsingServiceTests, SmokeTests).

**Not yet implemented**:
- Semantic diagnostics.
- Go-to-definition handler.
- Symbol-aware hover content.
- Context-aware completion beyond keywords.
