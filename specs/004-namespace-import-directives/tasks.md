# Tasks: Namespace Import Directives

**Input**: Design documents from `/specs/004-namespace-import-directives/`
**Prerequisites**: `plan.md`, `research.md`, `data-model.md`, `contracts/import-resolution.md`, `quickstart.md`

## Phase 3.1: Setup
- [ ] T001 Seed namespace import smoke-program assets in `test/runtime-integration-tests/TestPrograms/NamespaceImports/{math.5th,consumer.5th}` matching the quickstart scenario and ensure the files copy to `bin/Debug` via `runtime-integration-tests.csproj` metadata.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [ ] T002 [P] Create `test/syntax-parser-tests/NamespaceImportSyntaxTests.cs` covering namespace declaration/import acceptance and asserting legacy `use` syntax now fails.
- [ ] T003 [P] Add `test/runtime-integration-tests/NamespaceImportRuntimeTests.cs` that compiles the NamespaceImports program set and asserts `add(2,3)` returns 5 with no diagnostics.
- [ ] T004 [P] Add `test/runtime-integration-tests/Validation/NamespaceImportDiagnosticsTests.cs` verifying warning `WNS0001` is emitted (module path + namespace) when an import targets an undeclared namespace.
- [ ] T005 [P] Add `test/ast-tests/NamespaceImportGraphTests.cs` to exercise `NamespaceImportGraph` cycle detection and idempotent traversal.
- [ ] T006 [P] Add `test/runtime-integration-tests/NamespaceImportCliTests.cs` covering CLI enumeration of multiple `.5th` files (no MSBuild manifest).

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T007 [P] Introduce `ModuleMetadata` record in `src/compiler/NamespaceResolution/ModuleMetadata.cs` capturing module path, declared namespace, imports, and declarations.
- [ ] T008 [P] Introduce `NamespaceScopeIndex` aggregate in `src/compiler/NamespaceResolution/NamespaceScopeIndex.cs` to merge symbols from modules sharing a namespace.
- [ ] T009 [P] Add `ImportDirectiveBinding` record in `src/compiler/NamespaceResolution/ImportDirectiveBinding.cs` tracking module owner, namespace name, and resolved scope.
- [ ] T010 [P] Extend `src/ast-model/Symbols/SymbolTableEntry.cs` to expose fully-qualified names and local shadowing metadata needed for namespace imports.
- [ ] T011 [P] Implement `NamespaceImportGraph` in `src/compiler/NamespaceResolution/NamespaceImportGraph.cs` including cycle-safe traversal and memoization for imports.
- [ ] T012 Update `src/parser/grammar/FifthParser.g4` and `src/parser/grammar/FifthLexer.g4` to support file-scoped `namespace` declarations and `import` directives, removing `module_import` legacy syntax.
- [ ] T013 Update `src/parser/AstBuilderVisitor.cs` to populate `ModuleMetadata` and `ImportDirectiveBinding` nodes from the parse tree.
- [ ] T014 Add `src/compiler/LanguageTransformations/NamespaceImportResolverVisitor.cs` to aggregate namespace scopes, resolve imports, and enforce idempotency.
- [ ] T015 Wire the resolver into `src/compiler/ParserManager.cs` immediately after `SymbolTableBuilderVisitor` and before overload/type inference passes, ensuring namespace scopes exist before later transformations.
- [ ] T016 Update `src/compiler/LanguageTransformations/SymbolTableBuilderVisitor.cs` to emit namespace-aware symbol entries into `NamespaceScopeIndex`.
- [ ] T017 Update `src/compiler/CompilerOptions.cs` and `src/compiler/Program.cs` to accept multiple `.5th` file arguments while preserving existing options/help text.
- [ ] T018 Add MSBuild manifest support by updating `src/compiler/compiler.csproj` and `Directory.Build.props` to emit an item list of `.5th` sources for consumption by the resolver.
- [ ] T019 Implement unified module loading in `src/compiler/Compiler.cs` using a new helper `src/compiler/NamespaceResolution/ModuleResolver.cs` to combine CLI enumerations and MSBuild manifests into `ModuleMetadata` instances.
- [ ] T020 Create `src/compiler/NamespaceResolution/NamespaceDiagnosticEmitter.cs` and integrate with `Compiler.cs` so warning `WNS0001` and related diagnostics include module + namespace identifiers per contract.
- [ ] T021 Instrument namespace resolution timing in `src/compiler/Compiler.cs`, emitting elapsed milliseconds under the existing diagnostics flag to guard the 2-second SLA.

## Phase 3.4: Integration
- [ ] T022 Amend `test/runtime-integration-tests/runtime-integration-tests.csproj` to copy the NamespaceImports program set to the output directory for all configurations.
- [ ] T023 Extend `scripts/validate-examples.fish` to validate the new NamespaceImports samples so CI parser checks cover them.

## Phase 3.5: Polish
- [ ] T024 [P] Update `docs/syntax-samples-readme.md` to document file-scoped namespaces and `import` directives with canonical examples.
- [ ] T025 [P] Add `test/runtime-integration-tests/Performance/NamespaceImportPerformanceTests.cs` exercising large module sets and asserting the logged resolution time stays under 2 seconds.
- [ ] T026 [P] Refresh CLI help text in `src/compiler/CompilerCommand.cs` to describe multi-file enumeration and MSBuild manifest behavior.
- [ ] T027 [P] Update `docs/5thproj-implementation-summary.md` with the new namespace import workflow and diagnostics guidance.
- [ ] T028 Run `dotnet build fifthlang.sln`, `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj`, `dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj`, and `scripts/validate-examples.fish` to confirm the feature passes all gates.

## Dependencies
- T001 → T002–T006 (samples required before tests).
- T002–T006 → T007–T021 (tests must fail before implementation begins).
- T007 → T008–T011 (Module metadata required before other namespace structures).
- T008–T011 → T014–T016 (namespace structures required before resolver integration).
- T012 → T013 (grammar updates before AST builder changes).
- T014 → T015 → T016 (resolver creation, then pipeline wiring, then symbol table integration).
- T017 → T019 (CLI option parsing must exist before resolver consumes arguments).
- T018 → T019 (manifest target must be available before resolver reads it).
- T019 → T020–T021 (resolver must populate modules before diagnostics/performance instrumentation).
- T001 → T022 (namespace samples must exist before csproj copy metadata is added).
- T023 → T028 (CI validation script must include new samples before final verification).
- T024–T027 depend on T020–T021 (docs should reflect final behavior).

## Parallel Execution Example
```
# After T001 completes, launch parser/runtime test authoring in parallel:
specify tasks run T002
specify tasks run T003
specify tasks run T004
specify tasks run T005
specify tasks run T006
```

## Notes
- `[P]` tasks touch unique files and can run concurrently once their dependencies are satisfied.
- Always observe TDD: ensure T002–T006 are committed in failing state before tackling implementation tasks.
- Regenerate ANTLR outputs implicitly via `dotnet build`; do not commit generated parser artifacts.
- Avoid manual edits under `src/ast-generated/`; regenerate via the documented commands when metamodels change.
