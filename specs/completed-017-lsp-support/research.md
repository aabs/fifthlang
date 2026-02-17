# Research: LSP Server for Fifth

## Decision 1: LSP library choice
- **Decision**: Use OmniSharp.Extensions.LanguageServer for LSP protocol handling.
- **Rationale**: Mature .NET LSP implementation with stdio support and request/notification handlers aligned to the required capabilities.
- **Alternatives considered**: Build a custom JSON-RPC implementation; use other .NET LSP libraries with less community adoption.

## Decision 2: Transport and lifecycle
- **Decision**: Support stdio transport only for MVP with standard initialize/initialized/shutdown lifecycle.
- **Rationale**: Stdio is the most compatible transport across editors and aligns with repository CLI conventions.
- **Alternatives considered**: TCP transport or editor-specific IPC.

## Decision 3: Document state and parsing
- **Decision**: Maintain in-memory document text and parse results for open documents only, using full-text sync updates.
- **Rationale**: Aligns with MVP scope and simplifies change handling while supporting unsaved buffers.
- **Alternatives considered**: Incremental range updates; workspace-wide background parsing.

## Decision 4: Diagnostic sources
- **Decision**: Use existing parser/compiler diagnostics for syntax and semantic errors, scoped to open documents only.
- **Rationale**: Ensures consistency with compiler behavior and avoids duplicating error logic.
- **Alternatives considered**: Implement a separate lightweight diagnostic pass.

## Decision 5: Observability baseline
- **Decision**: Emit structured, deterministic logs to stderr only.
- **Rationale**: Matches CLI observability guidance and keeps MVP lean.
- **Alternatives considered**: Add metrics or tracing.

## Decision 6: Security posture
- **Decision**: Local-only server with no authentication for MVP.
- **Rationale**: Typical for stdio LSP servers launched by editors.
- **Alternatives considered**: Startup enablement flags or auth tokens.
