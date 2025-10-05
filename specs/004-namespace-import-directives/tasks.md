# Tasks: Namespace Import Directives

**Input**: Design documents from `/specs/004-namespace-import-directives/`
**Prerequisites**: `plan.md`, `research.md`, `data-model.md`, `contracts/import-resolution.md`, `quickstart.md`

## Phase 3.1: Setup
- [ ] T001 Seed namespace import smoke-program assets in `test/runtime-integration-tests/TestPrograms/NamespaceImports/{math.5th,consumer.5th}` matching the quickstart scenario and ensure the files copy to `bin/Debug` via `runtime-integration-tests.csproj` metadata.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [ ] T002 [P] Create `test/syntax-parser-tests/NamespaceImportSyntaxTests.cs` covering namespace declaration/import acceptance and asserting legacy `use` syntax now fails.
- [ ] T003 [P] Add `test/runtime-integration-tests/Validation/NamespaceDuplicateSymbolTests.cs` that compiles two modules in the same namespace declaring `export add(int a, int b): int { return a + b; }` and expects a duplicate-symbol diagnostic naming both files.
- [ ] T004 [P] Add `test/runtime-integration-tests/NamespaceImportRuntimeTests.cs` that compiles the NamespaceImports program set and asserts `main(): int { return add(2, 3); }` evaluates to `5` with no diagnostics.
- [ ] T005 [P] Add `test/runtime-integration-tests/Validation/NamespaceImportDiagnosticsTests.cs` verifying warning `WNS0001` is emitted (module path + namespace) when an import targets an undeclared namespace.
- [ ] T006 [P] Add `test/ast-tests/NamespaceImportGraphTests.cs` to exercise `NamespaceImportGraph` cycle detection and idempotent traversal.
- [ ] T007 [P] Add `test/runtime-integration-tests/NamespaceImportCliTests.cs` covering CLI enumeration of multiple `.5th` files (no MSBuild manifest).
- [ ] T008 [P] Add `test/runtime-integration-tests/Validation/NamespaceEntryPointDiagnosticsTests.cs` ensuring builds fail when zero or multiple `main` functions exist after namespace aggregation, with diagnostics naming offending modules.
- [ ] T009 [P] Add `test/runtime-integration-tests/NamespaceImportShadowingTests.cs` proving a module-local symbol (e.g., `export add(int a, int b): int { return a - b; }`) shadows an imported one for that file only.
- [ ] T010 [P] Add `test/runtime-integration-tests/NamespaceImportGlobalNamespaceTests.cs` covering a module without a `namespace` declaration importing a named namespace and confirming its declarations remain in the global scope while imports are file-local.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T011 [P] Introduce `ModuleMetadata` record in `src/compiler/NamespaceResolution/ModuleMetadata.cs` capturing module path, declared namespace (or global), imports, and declarations.
- [ ] T012 [P] Introduce `NamespaceScopeIndex` aggregate in `src/compiler/NamespaceResolution/NamespaceScopeIndex.cs` to merge symbols from modules sharing a namespace and emit duplicate symbol diagnostics across contributing modules.
- [ ] T013 [P] Add `ImportDirectiveBinding` record in `src/compiler/NamespaceResolution/ImportDirectiveBinding.cs` tracking module owner, namespace name, and resolved scope.
- [ ] T014 [P] Extend `src/ast-model/Symbols/SymbolTableEntry.cs` to expose fully-qualified names and local shadowing metadata needed for namespace imports.
- [ ] T015 [P] Implement `NamespaceImportGraph` in `src/compiler/NamespaceResolution/NamespaceImportGraph.cs` including cycle-safe traversal and memoization for imports.
- [ ] T016 Update `src/parser/grammar/FifthParser.g4` and `src/parser/grammar/FifthLexer.g4` to support file-scoped `namespace` declarations and `import` directives, removing `module_import` legacy syntax.
- [ ] T017 Update `src/parser/AstBuilderVisitor.cs` to populate `ModuleMetadata` and `ImportDirectiveBinding` nodes from the parse tree.
- [ ] T018 Add `src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs` to aggregate namespace scopes, resolve imports, enforce idempotency, and respect module-local shadowing precedence.
- [ ] T019 Wire the resolver into `src/compiler/ParserManager.cs` immediately after `SymbolTableBuilderVisitor` and before overload/type inference passes, ensuring namespace scopes exist before later transformations.
- [ ] T020 Update `src/compiler/LanguageTransformations/SymbolTableBuilderVisitor.cs` to emit namespace-aware symbol entries into `NamespaceScopeIndex`, flagging duplicates and preserving local shadow indicators.
- [ ] T021 Update `src/compiler/CompilerOptions.cs` and `src/compiler/Program.cs` to accept multiple `.5th` file arguments while preserving existing options/help text.
- [ ] T022 Add MSBuild manifest support by updating `src/compiler/compiler.csproj` and `Directory.Build.props` to emit an item list of `.5th` sources for consumption by the resolver.
- [ ] T023 Implement unified module loading in `src/compiler/Compiler.cs` using a new helper `src/compiler/NamespaceResolution/ModuleResolver.cs` to combine CLI enumerations and MSBuild manifests into `ModuleMetadata` instances, including modules without explicit namespaces and enforcing the single-entry-point rule.
- [ ] T024 Create `src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs` and integrate with `Compiler.cs` so duplicate-symbol errors, warning `WNS0001`, and related diagnostics include module + namespace identifiers per contract.
- [ ] T025 Instrument namespace resolution timing in `src/compiler/Compiler.cs`, emitting elapsed milliseconds under the existing diagnostics flag to guard the 2-second SLA.

## Phase 3.4: Integration
- [ ] T026 Amend `test/runtime-integration-tests/runtime-integration-tests.csproj` to copy the NamespaceImports program set to the output directory for all configurations.
- [ ] T027 Extend `scripts/validate-examples.fish` to validate the new NamespaceImports samples so CI parser checks cover them.

## Phase 3.5: Polish
- [ ] T028 [P] Update `docs/syntax-samples-readme.md` to document file-scoped namespaces and `import` directives with canonical examples.
- [ ] T029 [P] Add `test/runtime-integration-tests/Performance/NamespaceImportPerformanceTests.cs` exercising large module sets and asserting the logged resolution time stays under 2 seconds.
- [ ] T030 [P] Refresh CLI help text in `src/compiler/CompilerCommand.cs` to describe multi-file enumeration and MSBuild manifest behavior.
- [ ] T031 [P] Update `docs/5thproj-implementation-summary.md` with the new namespace import workflow and diagnostics guidance.
- [ ] T032 Run `dotnet build fifthlang.sln`, `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj`, `dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj`, and `scripts/validate-examples.fish` to confirm the feature passes all gates.

## Dependencies
- T001 → T002–T010 (samples required before tests).
- T002–T010 → T011–T025 (tests must fail before implementation begins).
- T011 → T012–T015 (Module metadata required before other namespace structures).
- T012–T015 → T018–T020 (namespace structures required before resolver integration).
- T016 → T017 (grammar updates before AST builder changes).
- T018 → T019 → T020 (resolver creation, then pipeline wiring, then symbol table integration).
- T021 → T023 (CLI option parsing must exist before resolver consumes arguments).
- T022 → T023 (manifest target must be available before resolver reads it).
- T023 → T024–T025 (resolver must populate modules before diagnostics/performance instrumentation).
- T001 → T026 (namespace samples must exist before csproj copy metadata is added).
- T027 → T032 (CI validation script must include new samples before final verification).
- T028–T031 depend on T024–T025 (docs should reflect final behavior).

## Parallel Execution Example
```
# After T001 completes, launch parser/runtime test authoring in parallel:
specify tasks run T002
specify tasks run T003
specify tasks run T004
specify tasks run T005
specify tasks run T006
specify tasks run T007
specify tasks run T008
specify tasks run T009
specify tasks run T010
```

## Notes
- `[P]` tasks touch unique files and can run concurrently once their dependencies are satisfied.
- Always observe TDD: ensure T002–T010 are committed in failing state before tackling implementation tasks.
- Regenerate ANTLR outputs implicitly via `dotnet build`; do not commit generated parser artifacts.
- Avoid manual edits under `src/ast-generated/`; regenerate via the documented commands when metamodels change.
