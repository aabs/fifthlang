# Data Model: LSP Server

## Document
- **Represents**: An open Fifth source file and its current content.
- **Fields**:
  - **Uri**: Unique identifier for the document.
  - **Version**: Editor-provided version counter.
  - **Text**: Current full text.
  - **LastParsedAt**: Timestamp of last parse (for freshness).
  - **ParseErrors**: Current syntax errors.
- **Relationships**: Belongs to a Workspace; produces Diagnostics.

## Workspace
- **Represents**: A collection of files participating in the same project.
- **Fields**:
  - **RootUri**: Workspace root.
  - **Documents**: Set of open documents.
  - **IndexState**: Status of symbol/definition indexing.
- **Relationships**: Owns Documents and Symbols.

## Diagnostic
- **Represents**: A syntax or semantic issue in a document.
- **Fields**:
  - **Range**: Location in the document.
  - **Severity**: Error, warning, info.
  - **Message**: Human-readable description.
  - **Source**: Parser or analyzer origin.
- **Validation Rules**: Range must be within document bounds.

## Symbol
- **Represents**: A named definition or reference.
- **Fields**:
  - **Name**: Symbol name.
  - **Kind**: Function, class, variable, type, etc.
  - **Location**: Definition location.
  - **Signature**: Type or function signature.
- **Relationships**: Resolved within Workspace; used by Hover and Definition.

## Completion Item
- **Represents**: A suggested completion entry.
- **Fields**:
  - **Label**: Display text.
  - **Kind**: Keyword, symbol, snippet.
  - **Detail**: Type or signature.
  - **InsertText**: Text to insert.

## Location
- **Represents**: A position in a document.
- **Fields**:
  - **Uri**: Document identifier.
  - **Range**: Start/end positions.

## State Transitions
- **Document**: Open → Updated (on change) → Closed.
- **Workspace Index**: Uninitialized → Building → Ready; rebuild on file changes.
