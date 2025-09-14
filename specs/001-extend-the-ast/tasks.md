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
- [ ] T001 Ensure graph block tokens exist in `src/parser/grammar/FifthLexer.g4` (`GRAPH_LBRACE` for `<{`, `GRAPH_RBRACE` for `}>`) and are not used elsewhere.
- [ ] T002 Add/confirm global usings in `src/fifthlang.system/GlobalUsings.cs` for `VDS.RDF` and `VDS.RDF.Storage` to simplify KG code.
- [ ] T003 Verify build runs pre-change to capture baseline: `dotnet restore` + `dotnet build` (no code changes in this task).

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [ ] T004 [P] Parser test: statement form `graphAssertionBlock` with trailing semicolon in `test/syntax-parser-tests/` (new test file `GraphAssertionBlock_StatementTests.cs`).
- [ ] T005 [P] Parser test: expression form used in assignment in `test/syntax-parser-tests/GraphAssertionBlock_ExpressionTests.cs`.
- [ ] T006 [P] Parser test: nesting both ways (graph-in-regular, regular-in-graph) in `test/syntax-parser-tests/GraphAssertionBlock_NestingTests.cs`.
- [ ] T007 AST/type test: block expression yields `graph` type in `test/ast-tests/GraphAssertionBlock_TypeTests.cs`.
- [ ] T008 AST/type test: `graph += graph` and `store += graph` typing rules in `test/ast-tests/GraphAndStoreOperatorTests.cs`.
- [ ] T009 Lowering test: expression block lowers to KG calls (no persistence) in `test/ast-tests/GraphAssertionBlock_LoweringExpressionTests.cs`.
- [ ] T010 Lowering test: statement block lowers to default `store += graph` (error if missing) in `test/ast-tests/GraphAssertionBlock_LoweringStatementTests.cs`.
- [ ] T011 [P] Runtime integration: use dotNetRdf `TripleStore` to verify `store += graph` persists assertions in `test/runtime-integration-tests/GraphAssertionBlock_RuntimeTests.cs`.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T012 Grammar: add rule `graphAssertionBlock: GRAPH_LBRACE statements? GRAPH_RBRACE;` to `src/parser/grammar/FifthParser.g4`.
- [ ] T013 Grammar: add statement alternative `graphAssertionBlock ';'` to `statement` in `FifthParser.g4`.
- [ ] T014 Grammar: add expression alternative `graphAssertionBlock` to `primaryExpression` in `FifthParser.g4`.
- [ ] T015 Parser: ensure nested blocks are allowed and ambiguity-free; regenerate ANTLR outputs by building solution.
- [ ] T016 AST model: add `GraphAssertionBlockExp` and `GraphAssertionBlockStmt` to `src/ast-model/AstMetamodel.cs` per `data-model.md`.
- [ ] T017 Types: add `store`, `graph`, `triple`, `iri` types in `src/ast-model/TypeSystem/` and wire in `src/ast-model/AstTypeProvider.cs`.
- [ ] T018 Types: implement typing for `graph += graph` and `store += graph` operators.
- [ ] T019 Codegen: regenerate AST builders/visitors via `make run-generator` (updates `src/ast-generated/*`).
- [ ] T020 Parser→AST: update `src/parser/AstBuilderVisitor.cs` to construct `GraphAssertionBlock*` nodes from the new rule.
- [ ] T021 Lowering: create `src/compiler/LanguageTransformations/GraphAssertionLoweringVisitor.cs` to lower blocks to KG calls.
- [ ] T022 Pipeline: register lowering pass in `src/compiler/ParserManager.cs` before IL conversion.
- [ ] T023 Diagnostics: add error for missing default store when lowering statement-form blocks without configured default (align FR-019).

## Phase 3.4: Integration
- [ ] T024 Built-ins: ensure `Fifth.System.KG` methods in `src/fifthlang.system/KnowledgeGraphs.cs` are registered as built-ins for symbol resolution.
- [ ] T025 Polish: add or confirm `src/fifthlang.system/GlobalUsings.cs` to centralize KG namespaces (optional but recommended).
- [ ] T026 Validate example from quickstart compiles and runs through the full pipeline.

## Phase 3.5: Polish
- [ ] T027 [P] Add negative tests for misuse (e.g., using graph block in const contexts; invalid `+=` operands).
- [ ] T028 [P] Improve diagnostics with IDs and actionable messages (missing default, invalid contexts).
- [ ] T029 Performance sanity: large block with many assertions remains linear; add simple benchmark in tests if feasible.
- [ ] T030 Update docs: spec notes and quickstart if syntax or behavior changed during implementation.

## Phase 3.4.1: Runtime & Built-ins Alignment
- [ ] T031 Implement default store resolution used by lowering (compiler/runtime), with a single entrypoint accessible to the lowering pass (e.g., compiler configuration or implicit symbol). Emit error consistent with FR-019 when missing.
- [ ] T032 Tests: default store present vs missing. Extend runtime tests in `test/runtime-integration-tests/GraphAssertionBlock_RuntimeTests.cs` to assert correct behavior and diagnostics.
- [ ] T033 Expose a public KG persist primitive (e.g., `KG.SaveGraph(IUpdateableStorage, IGraph, Uri?)`) or adjust lowering to call `IUpdateableStorage.SaveGraph` directly; ensure accessible from lowering.
- [ ] T034 Register/annotate required `KG` methods as built-ins for symbol resolution so Fifth code can call them where appropriate.
- [ ] T035 Add or alias a built-in `sparql_store(iri)` to `KG.ConnectToRemoteStore` so examples and tests resolve correctly.
- [ ] T036 Verify/update IL type mappings for new types: `graph` → `VDS.RDF.IGraph`, `store` → `VDS.RDF.Storage.IUpdateableStorage`, `triple` → `VDS.RDF.Triple`, `iri` → `System.Uri`; add a small codegen sanity test if applicable.

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
