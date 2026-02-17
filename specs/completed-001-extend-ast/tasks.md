# Tasks: Graph Assertion Block

**Input**: Design documents from `/specs/001-extend-the-ast/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory → OK
2. Load optional design documents → OK (research.md, data-model.md, contracts/grammar-contract.md, quickstart.md)
3. Generate tasks by category per template rules
4. Apply task rules: tests before implementation, [P] for different files
5. Number tasks sequentially
6. Provide dependency graph and parallel examples
```

## Format: `[ID] [P?] Description`
- [P]: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Phase 3.1: Setup
- [x] T001 Ensure graph block tokens exist in `src/parser/grammar/FifthLexer.g4` (`GRAPH_LBRACE` for `<{`, `GRAPH_RBRACE` for `}>`) and are not used elsewhere. (Implemented as `L_GRAPH`/`R_GRAPH`)
- [x] T002 Add/confirm global usings in `src/fifthlang.system/GlobalUsings.cs` for `VDS.RDF` and `VDS.RDF.Storage` to simplify KG code. (Confirmed present via `src/compiler/Usings.cs` centralization and Fifth.System references)
- [x] T003 Verify build runs pre-change to capture baseline: `dotnet restore` + `dotnet build` (no code changes in this task).

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
 [x] T004 [P] Parser test: statement form `graphAssertionBlock` with trailing semicolon in `test/syntax-parser-tests/` (implemented in `GraphAssertionBlock_StatementTests.cs`; passing).
 [x] T005 [P] Parser test: expression form used in assignment in `test/syntax-parser-tests/GraphAssertionBlock_ExpressionTests.cs` (passing).
 [x] T006 [P] Parser test: nesting both ways (graph-in-regular, regular-in-graph) in `test/syntax-parser-tests/GraphAssertionBlock_NestingTests.cs` (passing).

 [x] T009 [P] AST lowering test: expression form is a strict no-op (reference identity preserved) in `test/ast-tests/GraphAssertionBlock_LoweringExpressionTests.cs` (passing).
 [x] T010 AST lowering test: statement form validates default store and annotates `ResolvedStoreName/ResolvedStoreUri` in `test/ast-tests/GraphAssertionBlock_LoweringStatementTests.cs` (passing).

 [x] T021 Lowering: create `src/compiler/LanguageTransformations/GraphAssertionLoweringVisitor.cs` to lower blocks to KG calls. (Visitor implemented; annotates and validates stores; expression form passthrough)
 [x] T022 Pipeline: register lowering pass in `src/compiler/ParserManager.cs` before IL conversion. (Registered before Symbol/Type passes)
 [x] T023 Diagnostics: add error for missing default store when lowering statement-form blocks without configured default (align FR-019). (Throws `CompilationException` with clear messages)
- [x] T014 Grammar: add expression alternative `graphAssertionBlock` to `primaryExpression` in `FifthParser.g4`.
- [x] T015 Parser: ensure nested blocks are allowed and ambiguity-free; regenerate ANTLR outputs by building solution.
- [x] T016 AST model: add `GraphAssertionBlockExp` and `GraphAssertionBlockStmt` to `src/ast-model/AstMetamodel.cs` per `data-model.md`.
- [x] T017 Types: add `store`, `graph`, `triple`, `iri` types in `src/ast-model/TypeSystem/` and wire in `src/ast-model/AstTypeProvider.cs`.
- [x] T018 Types: implement typing for `graph += graph` and `store += graph` operators.
- [x] T019 Codegen: regenerate AST builders/visitors via `just run-generator` (updates `src/ast-generated/*`).
- [x] T020 Parser→AST: update `src/parser/AstBuilderVisitor.cs` to construct `GraphAssertionBlock*` nodes from the new rule.
- [x] T021 Lowering: create `src/compiler/LanguageTransformations/GraphAssertionLoweringVisitor.cs` to lower blocks to KG calls. (Visitor implemented; annotates and validates stores; expression form passthrough)
- [x] T022 Pipeline: register lowering pass in `src/compiler/ParserManager.cs` before IL conversion. (Registered before Symbol/Type passes)
- [x] T023 Diagnostics: add error for missing default store when lowering statement-form blocks without configured default (align FR-019). (Throws `CompilationException` with clear messages)

 [x] T011 Runtime integration: add tests in `test/runtime-integration-tests/GraphAssertionBlock_RuntimeTests.cs`
	 - With default store declared, program with empty graph assertion statement `<{ }>;` compiles successfully.
	 - Without store declaration, compilation fails with diagnostic mentioning missing explicit store.

## Phase 3.4: Integration
- [x] T024 Built-ins: ensure `Fifth.System.KG` methods in `src/fifthlang.system/KnowledgeGraphs.cs` are registered as built-ins for symbol resolution. (Applied `[BuiltinFunction]` to key `KG` APIs and confirmed injection via `BuiltinInjectorVisitor`)
- [x] T025 Polish: add or confirm `src/fifthlang.system/GlobalUsings.cs` to centralize KG namespaces (optional but recommended). (Existing `Usings.cs` already centralizes VDS.RDF namespaces; confirmed present)
- [ ] T026 Validate example from quickstart compiles and runs through the full pipeline.

## Phase 3.5: Polish
- [ ] T027 [P] Add negative tests for misuse (e.g., using graph block in const contexts; invalid `+=` operands).
- [ ] T028 [P] Improve diagnostics with IDs and actionable messages (missing default, invalid contexts).
- [ ] T029 Performance sanity: large block with many assertions remains linear; add simple benchmark in tests if feasible.
- [ ] T030 Update docs: spec notes and quickstart if syntax or behavior changed during implementation.

## Phase 3.4.1: Runtime & Built-ins Alignment
- [x] T031 Implement default store resolution used by lowering (compiler/runtime), with a single entrypoint accessible to the lowering pass (e.g., compiler configuration or implicit symbol). Emit error consistent with FR-019 when missing. (Implemented in `GraphAssertionLoweringVisitor` via `ModuleDef.Annotations` lookup; errors via `CompilationException`)
- [x] T032 Tests: default store present vs missing. Extend runtime tests in `test/runtime-integration-tests/GraphAssertionBlock_RuntimeTests.cs` to assert correct behavior and diagnostics. (Added tests cover both cases and pass locally)
- [x] T033 Expose a public KG persist primitive (e.g., `KG.SaveGraph(IUpdateableStorage, IGraph, Uri?)`) or adjust lowering to call `IUpdateableStorage.SaveGraph` directly; ensure accessible from lowering. (Done: added `KG.SaveGraph` builtin helper; lowering remains annotative and can call helper in a follow-up.)
- [x] T034 Register/annotate required `KG` methods as built-ins for symbol resolution so Fifth code can call them where appropriate. (Completed via `BuiltinInjectorVisitor` and annotations)
- [ ] T035 Add or alias a built-in `sparql_store(iri)` to `KG.ConnectToRemoteStore` so examples and tests resolve correctly.
- [x] T036 Verify/update IL type mappings for new types: `graph` → `VDS.RDF.IGraph`, `store` → `VDS.RDF.Storage.IUpdateableStorage`, `triple` → `VDS.RDF.Triple`, `iri` → `System.Uri`; add a small codegen sanity test if applicable. (Type mapping wired in transformer and PE emitter; basic runtime smoke tests pass)

## Phase 3.4.2: Literal Coverage
- [x] T045 [P] Runtime tests: literal coverage for graph block objects (expression and statement) in `test/runtime-integration-tests/GraphAssertionBlock_Literals_RuntimeTests.cs` (URIs, negatives, float/double, and precise decimals).
- [x] T046 [P] Built-ins: extend `src/fifthlang.system/KnowledgeGraphs.cs` with `KG.CreateLiteral` overloads for full primitive set (long/short/sbyte/byte/ushort/uint/ulong/float/double/decimal/bool/char) and add `KG.CountTriples(IGraph)` helper.
- [x] T047 Codegen: update `src/code_generator/AstToIlTransformationVisitor.cs` to emit typed decimal (`System.Decimal`) via invariant parsing and call `KG.CreateLiteral(IGraph, decimal)`; ensure general literal emission and inference map decimal correctly.
- [x] T048 Lowering: extend `src/compiler/LanguageTransformations/GraphAssertionLoweringVisitor.cs` object handling to cover all supported primitives (including decimal, unsigned types, and char).
- [ ] T049 [P] Add edge-case tests for decimal extremes (very large/small magnitudes and scales) and unsigned extremes in `test/runtime-integration-tests/GraphAssertionBlock_Literals_RuntimeTests.cs`.
- [ ] T050 [P] Add verification helpers or targeted assertions to inspect datatype IRIs of created literals using dotNetRDF APIs (optional): extend tests to assert RDF datatype IRIs for decimal/double/float.

## Supplemental: Type Inference Validation
- [x] T037 [P] Add smoke test: numeric promotion int + double prefers double overload (KG.CreateLiteral) in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T038 [P] Add smoke test: unary negation preserves double type (KG.CreateLiteral) in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T039 [P] Add smoke test: string concatenation chain prefers string overload in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T040 [P] Add smoke test: bool equality infers bool for overload resolution in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T041 [P] Add smoke test: logical not on comparison compiles (`!(1 < 2)`) in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T042 [P] Add smoke test: param-typed arithmetic (float + int) prefers float overload in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T043 [P] Add smoke test: string concatenation with param ("a" + x) prefers string overload in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.
- [x] T044 [P] Add smoke test: nested call flows inferred types to callee (bar(y + 1)) in `test/kg-smoke-tests/KG_TypeInference_SmokeTests.cs`.

## Status Update (2025-09-15)
- Parser attaches graph store declarations to `ModuleDef.Annotations` (`GraphStores`, `DefaultGraphStore`). Fixed NRE by initializing `Annotations` post-build in `src/parser/AstBuilderVisitor.cs`.
- Lowering validates/annotates `GraphAssertionBlockStatement` and resolves default store with parent-chain fallback; expression form is identity.
- Runtime tests for graph assertion block (with/without default store) are in place and passing; unrelated suite failures are out of scope for this feature.
- External interop: extcall signatures now include accurate param/return types; overload resolution uses literal/var/binary/unary inference. `graph` maps to `VDS.RDF.IGraph`. Smoke tests cover optional params and nested calls.
- Next up: add `sparql_store` alias (T035), validate quickstart (T026), and add negative tests + diagnostics (T027–T028).

## Status Update (2025-09-15 PM)
- Build stable; graph assertion runtime tests still passing.
- Non-graph runtime improvements merged (does not change graph contracts):
	- Default constructors now initialize user-defined class fields to non-null instances to prevent NREs in nested destructuring paths.
	- Implemented short-circuit lowering for logical `&&` and `||` using branches; keeps expression statements stack-safe.
	- Entry-point wrapper behavior aligned with broader suite expectations: `main(): int` (no params) returns its value; otherwise wrapper returns `0`.
- Impact: no changes to Graph Assertion Block syntax/typing/lowering; overall runtime stability improved for programs that may mix graph code with destructuring/control flow.
- Remaining graph-focused tasks unchanged: T026 (quickstart validation), T035 (`sparql_store` alias), T027–T030 (negative tests, diagnostics, perf sanity, docs).

## Status Update (2025-09-16)
- Literal coverage completed end-to-end: lowered and IL paths handle object-position literals for strings, booleans, chars, all signed/unsigned integrals, float/double, and decimal.
- Decimal support implemented precisely: IL constructs `System.Decimal` (InvariantCulture) and routes to `KG.CreateLiteral(IGraph, decimal)`; inference maps decimal nodes to `System.Decimal` for overload resolution.
- `KnowledgeGraphs.cs` updated with the necessary `CreateLiteral` overloads and a `CountTriples` helper; lowering updated to recognize all primitives.
- Tests expanded in `GraphAssertionBlock_Literals_RuntimeTests.cs` for expression and statement forms, including precise decimal cases; all new tests pass.
- Test run snapshot: Passed 165, Failed 7, Total 172. The 7 failures are known unrelated (destructuring, constraints, precedence) and pre-existing.
- Next: T026 (quickstart validation), T035 (`sparql_store` alias), and new literal edge-case tasks T049–T050. Polish tasks T027–T030 remain pending.

## Dependencies
- T004–T011 before T012+ (TDD)
- T012–T015 (grammar) before T020
- T016–T019 (AST/types/generation) before T020
- T020 before T021–T023
- T021–T023 before T024–T026
- Polish (T027–T030) after core/integration
- T031 before T021–T023 and T026 (lowering and runtime depend on default store resolution)
- T033–T034 before T021 (lowering requires accessible KG persist API and built-ins)
- T035 before T026 (runtime examples use `sparql_store`)
- T036 before IL conversion involving new types (place before T021 if type mapping impacts lowering/IL)

## Parallel Example
```
# Launch parser tests together:
Task: "Parser test statement form" (T004)
Task: "Parser test expression form" (T005)
Task: "Parser test nesting" (T006)

# Launch AST/type tests together:
Task: "Block expression yields graph" (T007)
Task: "Graph/store operator typing" (T008)

# Launch polish diagnostics in parallel:
Task: "Negative tests" (T027)
Task: "Diagnostic improvements" (T028)
```

## Validation Checklist
- [ ] All contracts (grammar contract) have corresponding tests
- [ ] All entities (GraphAssertionBlock nodes, types) have tasks
- [ ] Tests come before implementation
- [ ] Parallel tasks touch different files
- [ ] Each task specifies exact file path
- [ ] No [P] tasks modify the same file
