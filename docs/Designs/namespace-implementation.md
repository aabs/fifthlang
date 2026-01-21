# Namespace declarations and imports: implementation details

## Purpose
This document explains how file-scoped `namespace` declarations and `import` directives work in Fifth and how they are implemented in the compiler. It is authoritative for the current implementation and ties each behavior back to the requirements in [specs/004-namespace-import-directives/spec.md](specs/004-namespace-import-directives/spec.md).

## Language surface
- **File-scoped namespace**: A module may declare at most one file-scoped namespace using the `namespace` keyword (FR-001). Modules without a declaration belong to the global namespace (FR-002).
- **Import directive**: `import <qualified.name>;` brings a namespace’s symbols into the importing module’s scope only (FR-003).
- **Legacy syntax**: `use` and the old `module_import` rule are not accepted (FR-008).
- **Symbol visibility**: Local declarations shadow imported ones in that module (FR-010). Symbols remain public by default; `export` is treated as a no-op for visibility (spec “Symbol Visibility”).

## Parsing and AST capture
1. **Grammar acceptance**
   - The parser recognizes file-scoped `namespace` and `import` directives at module scope, rejecting multiple namespace declarations per module (FR-001).
   - See [src/parser/grammar/FifthParser.g4](src/parser/grammar/FifthParser.g4) and [src/parser/grammar/FifthLexer.g4](src/parser/grammar/FifthLexer.g4).

2. **AST population**
   - `AstBuilderVisitor` records each `import` into module annotations as a list of `NamespaceImportDirective` values.
   - Each directive captures the imported namespace string and source location (line/column).
   - See [src/parser/AstBuilderVisitor.cs](src/parser/AstBuilderVisitor.cs) and [src/ast-model/NamespaceImportDirective.cs](src/ast-model/NamespaceImportDirective.cs).

## Module discovery and assembly construction
The compiler resolves the module set before semantic analysis:
- **CLI multi-file input**: Multiple `--source` values are accepted and used as the module set (FR-012). See [src/compiler/Program.cs](src/compiler/Program.cs) and [src/compiler/CompilerOptions.cs](src/compiler/CompilerOptions.cs).
- **Directory input**: When `--source` is a directory, only top-level `.5th` files are included to avoid unintentionally pulling nested fixtures. See [src/compiler/NamespaceResolution/ModuleResolver.cs](src/compiler/NamespaceResolution/ModuleResolver.cs).
- **Module metadata**: Each module is parsed into a single `ModuleDef` and wrapped in `ModuleMetadata`, which stores path, declared namespace (or global), imports, and symbol declarations. See [src/compiler/NamespaceResolution/ModuleMetadata.cs](src/compiler/NamespaceResolution/ModuleMetadata.cs).

The combined `AssemblyDef` is created from the collected modules and annotated with module metadata for later phases (FR-003, FR-004).

## Namespace aggregation and import resolution
Resolution occurs in `NamespaceImportResolverVisitor` and is wired immediately after initial symbol-table construction (FR-019).

### Step 1: Namespace scopes
- A `NamespaceScopeIndex` aggregates modules by declared namespace and merges their declarations into a single namespace scope (FR-004).
- Duplicate symbol names within a namespace are detected and reported (FR-005) using `NamespaceDiagnosticEmitter` (FR-011).
- See [src/compiler/NamespaceResolution/NamespaceScopeIndex.cs](src/compiler/NamespaceResolution/NamespaceScopeIndex.cs) and [src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs](src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs).

### Step 2: Import graph
- A `NamespaceImportGraph` records namespace-to-namespace edges derived from module import directives.
- Traversal is cycle-safe and short-circuits repeated work (FR-006).
- See [src/compiler/NamespaceResolution/NamespaceImportGraph.cs](src/compiler/NamespaceResolution/NamespaceImportGraph.cs).

### Step 3: Apply imports
For each module:
1. Imports are de-duplicated per module (idempotent behavior).
2. The resolver traverses the import graph to include transitive imports.
3. Resolved namespaces have their symbols imported into the module’s symbol table.
4. Local symbols shadow imported symbols: if a local symbol exists, the imported entry is ignored and the local entry is flagged as a shadow boundary (FR-010).

Implementation: [src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs](src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs).

### Step 4: Resolved import annotation
- The resolver records the list of successfully resolved namespaces in module annotations under `ModuleAnnotationKeys.ResolvedImports`.
- This is used later during code generation to emit `using` directives only for valid namespaces and avoid build failures when imports are missing (FR-009).
- See [src/ast-model/NamespaceImportDirective.cs](src/ast-model/NamespaceImportDirective.cs) and [src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs](src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs).

## Diagnostics
Diagnostics follow the schema in [specs/004-namespace-import-directives/spec.md](specs/004-namespace-import-directives/spec.md) and the contract in [specs/004-namespace-import-directives/contracts/import-resolution.md](specs/004-namespace-import-directives/contracts/import-resolution.md).

- **Undeclared import warning** (FR-009):
  - Code: WNS0001
  - Severity: Warning
  - Fields: `file`, `namespace`, `line`, `column`, `message`
  - Emitted when an import references a namespace with no declaring modules.
  - The compiler continues and treats the namespace as empty.
- **Duplicate symbol error** (FR-005):
  - Emitted when two modules in the same namespace declare the same symbol name.
- **Entry-point errors** (FR-007):
  - Emitted when there is zero or multiple `main` functions across the module set.

Emitter implementation: [src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs](src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs).

## Entry-point validation
After module aggregation, `ModuleResolver` verifies the entry point:
- Exactly one `main` across all modules is required (FR-007).
- When there is none or more than one, an error diagnostic is emitted, referencing the participating modules.
- See [src/compiler/NamespaceResolution/ModuleResolver.cs](src/compiler/NamespaceResolution/ModuleResolver.cs).

## Code generation: Roslyn output
Namespace imports affect generated C# in two ways:

1. **Namespace placement**
   - Modules with a declared namespace emit a C# namespace. Modules without a namespace are emitted at the top level so they remain in the global namespace (FR-002).
   - See [src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs](src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs).

2. **Using directives for resolved imports**
   - The translator reads `ModuleAnnotationKeys.ResolvedImports` and emits:
     - `using <Namespace>;` for symbol availability
     - `using static <Namespace>.Program;` for top-level function access
   - Missing imports do not emit C# `using` directives, avoiding Roslyn failures while preserving the warning-only behavior (FR-009).

3. **Program class strategy**
   - Each module emits a `public static partial class Program` to host top-level functions.
   - This enables cross-module static imports without clashes.
   - A stub `Main` is only generated for single-module builds that do not declare `main`, preventing multi-module entry-point collisions.

Implementation: [src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs](src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs).

## Performance
- Namespace resolution is timed and reported under diagnostics (FR-012, NFR-001).
- Timing is recorded in `NamespaceImportResolverVisitor` and surfaced by the compiler when diagnostics are enabled.
- See [src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs](src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs) and [src/compiler/Compiler.cs](src/compiler/Compiler.cs).

## Edge cases (spec alignment)
- **Import cycles**: Supported; traversal short-circuits already-visited namespaces (FR-006).
- **Undeclared imports**: Warn only; treated as empty (FR-009).
- **Global namespace imports**: Modules without a namespace remain global, but can import named namespaces for local use (Acceptance Scenario #2 in the spec).
- **Shadowing**: Local symbols always take precedence over imported symbols in the same module (FR-010).

## Validation mapping
These tests assert the contract behaviors:
- Namespace parsing and legacy `use` rejection: [test/syntax-parser-tests/NamespaceImportSyntaxTests.cs](test/syntax-parser-tests/NamespaceImportSyntaxTests.cs).
- Runtime behavior for the quickstart scenario: [test/runtime-integration-tests/NamespaceImportRuntimeTests.cs](test/runtime-integration-tests/NamespaceImportRuntimeTests.cs).
- Warning schema for undeclared imports: [test/runtime-integration-tests/Validation/NamespaceImportDiagnosticsTests.cs](test/runtime-integration-tests/Validation/NamespaceImportDiagnosticsTests.cs).
- Duplicate symbol detection: [test/runtime-integration-tests/Validation/NamespaceDuplicateSymbolTests.cs](test/runtime-integration-tests/Validation/NamespaceDuplicateSymbolTests.cs).
- Entry-point validation: [test/runtime-integration-tests/Validation/NamespaceEntryPointDiagnosticsTests.cs](test/runtime-integration-tests/Validation/NamespaceEntryPointDiagnosticsTests.cs).
- Shadowing and global namespace behavior: [test/runtime-integration-tests/NamespaceImportShadowingTests.cs](test/runtime-integration-tests/NamespaceImportShadowingTests.cs), [test/runtime-integration-tests/NamespaceImportGlobalNamespaceTests.cs](test/runtime-integration-tests/NamespaceImportGlobalNamespaceTests.cs).
- CLI multi-file enumeration: [test/runtime-integration-tests/NamespaceImportCliTests.cs](test/runtime-integration-tests/NamespaceImportCliTests.cs).
- Import graph behavior and cycles: [test/ast-tests/NamespaceImportGraphTests.cs](test/ast-tests/NamespaceImportGraphTests.cs).

## Notes on MSBuild manifests
Spec FR-012 and task T022 describe MSBuild manifest support. The module resolver already accepts a source manifest file when provided. Any MSBuild-generated manifest should list `.5th` sources and be passed via `--source-manifest` so resolution uses the same pipeline as CLI enumeration.

## Summary
Namespace declarations and imports are implemented as a pipeline that:
1. Parses file-scoped `namespace` and `import` directives.
2. Aggregates symbols per namespace with duplicate detection.
3. Resolves imports idempotently with cycle-safe traversal.
4. Emits structured diagnostics with module and namespace metadata.
5. Generates C# with correct namespace placement and import-based `using` directives, while preserving global modules and entry-point correctness.

This matches the requirements in [specs/004-namespace-import-directives/spec.md](specs/004-namespace-import-directives/spec.md) and the contract in [specs/004-namespace-import-directives/contracts/import-resolution.md](specs/004-namespace-import-directives/contracts/import-resolution.md).
