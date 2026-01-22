# Implementation Plan: Language Server Protocol (LSP) for IDE Integration

**Branch**: `001-lsp-server` | **Date**: 2026-01-23 | **Spec**: [specs/001-lsp-server/spec.md](specs/001-lsp-server/spec.md)
**Input**: Feature specification from `/specs/001-lsp-server/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Deliver a Fifth language server executable over stdio. Current implementation includes full-text document sync, syntax diagnostics, basic hover, and keyword completions; definition and semantic features remain to be implemented. The server maintains in-memory document state for open files only and responds with deterministic, structured logs to stderr.

## Technical Context

**Language/Version**: C# .NET 8.0  
**Primary Dependencies**: OmniSharp.Extensions.LanguageServer (LSP), existing parser/compiler libraries  
**Storage**: In-memory document/AST cache (no persistent store)  
**Testing**: xUnit + FluentAssertions (existing `test/language-server-smoke`)  
**Target Platform**: Cross-platform CLI (macOS/Linux/Windows)  
**Project Type**: Single service executable under `src/`  
**Performance Goals**: Diagnostics <1s; hover/definition <500ms for typical workspaces  
**Constraints**: stdio transport only; full-text sync; open documents only; local-only/no auth  
**Scale/Scope**: Single-root workspace; ≤200 files, ≤2,000 lines per open file for performance targets

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Library-first, contracts-first: PASS (new focused library under `src/` with public CLI entry)
- CLI and text I/O discipline: PASS (stdio transport with deterministic text outputs)
- Test-first: PASS (xUnit tests required for server behaviors)
- Generator-as-source-of-truth: PASS (no changes to generated code expected)

**Post-Design Re-check**: PASS (no violations introduced by design artifacts)

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
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
src/
└── language-server/
  ├── Fifth.LanguageServer.csproj
  ├── Program.cs
  ├── DocumentService.cs
  ├── DocumentStore.cs
  ├── SymbolService.cs
  ├── Parsing/
  │   └── ParsingService.cs
  └── Handlers/
      ├── DocumentSyncHandler.cs
      ├── HoverHandler.cs
      ├── CompletionHandler.cs
      └── DefinitionHandler.cs

test/
└── language-server-smoke/
  ├── LanguageServerSmoke.csproj
  ├── ParsingServiceTests.cs
  └── SmokeTests.cs
```

**Structure Decision**: Single project under `src/language-server/` with smoke tests under `test/language-server-smoke/`.
