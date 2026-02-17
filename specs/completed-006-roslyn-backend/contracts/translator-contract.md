# Translator API Contract — LoweredAstToRoslynTranslator

This document defines the public contract for the Roslyn translator component that translates Lowered AST modules into Roslyn C# sources and produces translation metadata used by tests and CI verification.

## Translator surface

- Class: `LoweredAstToRoslynTranslator`
- Primary method:
  - `TranslationResult Translate(LoweredAstModule module, TranslatorOptions? options = null)`

## Inputs

- `LoweredAstModule`
  - ModuleName: string
  - Types: IReadOnlyList<LoweredType>
  - Methods: IReadOnlyList<LoweredMethod>
  - OriginSourceFiles: IReadOnlyList<string> (paths to original .5th sources)

- `TranslatorOptions` (optional)
  - EmitDebugInfo: bool (default: true)
  - BackendCompatibilityMode: enum { LegacyShim, Strict } (default: LegacyShim)
  - AdditionalReferences: IReadOnlyList<string>

## Outputs (`TranslationResult`)

- `IReadOnlyList<string> Sources` — Generated C# source file contents (or file paths when saved to disk)
- `MappingTable Mapping` — Mapping of lowered AST NodeId -> source file index + start/end line/column
- `IReadOnlyList<DiagnosticRecord> Diagnostics` — Any diagnostics produced during translation
- `EmitHints` (optional) — Suggested EmitOptions (e.g., target frameworks, Roslyn options)

## MappingTable / MappingEntry

Each `MappingEntry` must include:
- `NodeId` (unique identifier of the Lowered AST node)
- `SourceIndex` (index into `TranslationResult.Sources`)
- `StartLine`, `StartColumn`, `EndLine`, `EndColumn` (1-based coordinates in the generated C# file that map back to original .5th source via `#line` or EmbeddedText)

## Diagnostics

- `DiagnosticRecord` shape:
  - `Id` (string)
  - `Severity` (enum: Error | Warning | Info)
  - `Message` (string)
  - `SourceFile` (string?)
  - `StartLine`, `StartColumn`, `EndLine`, `EndColumn` (optional coordinates)

## Behavioral contracts & acceptance tests

- For PDB verification (FR-003) the `Translate` call must result in an emitted Portable PDB where at least one `MappingEntry` for a known node from the POC sample corresponds to an exact SequencePoint in the PDB with matching start-line/start-column and end-line/end-column.
- For test-suite equivalence (FR-002) all samples in the Baseline Test Suite must compile and run with equivalent observable behavior when translated and emitted by Roslyn.

## Stability guarantees

- The translator API is internal to `src/compiler/` for now; only the contract in this document is considered stable across incremental implementations in this feature branch.
- The `MappingTable` schema must remain backward compatible for mapping/test harness consumers until the POC and mapping tests are stabilized.

## Notes

- This contract is intentionally minimal and focused on the needs of the POC and test harness. Implementation details (e.g., how #line pragmas, EmbeddedText or Roslyn SequencePoint emission are used) are implementation choices; the important contract is that `MappingTable` plus emitted PDBs provide the required mapping fidelity.
