# Implementation Plan: Graph Assertion Block

**Branch**: `001-extend-the-ast` | **Date**: 2025-09-14 | **Spec**: /Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/001-extend-the-ast/spec.md
**Input**: Feature specification from `/specs/001-extend-the-ast/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → OK
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → No outstanding NEEDS CLARIFICATION in spec; decisions encoded in FR-026..029
3. Evaluate Constitution Check section below
   → No violations; using existing projects and direct dependencies
   → Update Progress Tracking: Initial Constitution Check
4. Execute Phase 0 → research.md
   → Completed in this commit
5. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file
   → Completed in this commit
6. Re-evaluate Constitution Check section
   → No new violations (post-design)
   → Update Progress Tracking: Post-Design Constitution Check
7. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
   → Described below
8. STOP - Ready for /tasks command
```

## Summary
Introduce a new Graph Assertion Block delimited by `<{` and `}>` that behaves like a normal block while accumulating assertions from mutations to assertable objects. As a statement, it auto-persists to the default graph on completion; as an expression, it yields a graph value that is persisted only via explicit graph operations (e.g., `graphVar += value`, `store += value`). Implementation proceeds in stages: grammar (new `graphAssertionBlock` rule, nested blocks supported), AST model (new node and types: `store`, `graph`, `triple`, `iri`), type system rules, symbol resolution for built-ins in `Fifth.System` (`KG`), AST builder mapping, and lowering to explicit calls to knowledge graph primitives. Tests validate parsing, AST shape, type checking, lowering, and runtime with dotNetRdf TripleStore.

## Technical Context
- Language/Version: C#/.NET 8.0 (per `global.json`), ANTLR 4 (C# runtime)
- Primary Dependencies: Antlr4.Runtime.Standard, dotNetRdf 3.4.0, RazorLight (AST gen), xUnit + FluentAssertions
- Storage: dotNetRdf `IUpdateableStorage` via `SparqlHttpProtocolConnector` and `InMemoryManager`
- Testing: xUnit suites in `test/ast-tests`, `test/syntax-parser-tests`, `test/runtime-integration-tests`
- Target Platform: .NET 8 (macOS dev verified); CLI compiler
- Project Type: Single solution with multiple libraries (use Option 1 structure)
- Performance Goals: Linear with number of assertions; no extra passes beyond existing pipeline
- Constraints: Keep grammar unambiguous and support nesting; persistence explicit; default graph non-transactional; l-values transactional per spec
- Scale/Scope: Typical program scale for Fifth; graphs sized to unit/integration tests

## Constitution Check
**Simplicity**:
- Projects: 6 (existing solution; no new projects)
- Using framework directly: Yes (ANTLR, dotNetRdf)
- Single data model: Yes (extend existing AST model; no extra DTOs)
- Avoiding patterns: Yes (prefer direct visitors/transformations already used)

**Architecture**:
- Feature via libraries: Yes (parser, ast-model, compiler, fifthlang.system)
- Libraries listed: parser (grammar), ast-model (nodes/types), ast-generated (builders/visitors), compiler (transformations), code_generator (IL), fifthlang.system (KG built-ins)
- CLI per library: N/A (compiler CLI exists)
- Library docs: Spec + plan + quickstart added under `/specs/001-extend-the-ast/`

**Testing (NON-NEGOTIABLE)**:
- TDD: Add parser tests first (RED), then implement grammar (GREEN), follow with AST/type/lowering tests sequentially
- Commits: Ensure failing tests precede implementation where practical
- Order: Parser contracts → AST/type → Lowering → Runtime integration
- Real dependencies: Use actual dotNetRdf `TripleStore` in runtime tests; no mocks
- Integration tests: yes, for end-to-end parse→lower→execute

**Observability**:
- Compiler errors/diagnostics expanded for illegal contexts
- Log/traces not in scope (CLI), but test assertions validate outcomes

**Versioning**:
- Feature branch only; no package version bump in this plan

## Project Structure
Structure Decision: Option 1 (single repo with libraries and tests)

## Phase 0: Outline & Research
Decisions and rationale captured in `research.md`:
- Grammar: Introduce `graphAssertionBlock` production; admit in statement and primary-expression positions; support arbitrary nesting alongside existing blocks; avoid ambiguity by distinct delimiters `<{` `}>` already tokenized.
- Semantics: Explicit persistence via graph/store ops; standalone statement auto-persists to default; transactional boundary at l-value commit; default graph non-transactional (no rollback); dedup identical triples; open-world for conflicting values.
- Types: Add `store`, `graph`, `triple`, `iri` to type system; define assignment compatibility and inference rules; ensure `graph += graph` and `store += graph` typing.
- Built-ins: Expose `Fifth.System.KG` functions to symbol table; consider thin built-ins such as `kg.create_store()`, `kg.create_graph()`, `kg.assert(triple)`, `kg.save(store, graph, iri?)` aligned with existing `KnowledgeGraphs.cs`.
- Lowering: Map assertion blocks to sequences of calls to KG built-ins; enforce explicit operations for persistence.
- Testing: Use dotNetRdf `TripleStore` to assert persisted triples; parser samples from `test/.../KnowledgeManagement` and new cases.

## Phase 1: Design & Contracts
Artifacts generated in this plan:
- `data-model.md`: AST additions (GraphAssertionBlock node), new primitive/reference types, typing and inference constraints, lowering contract.
- `contracts/grammar-contract.md`: Grammar rule signatures and admissible contexts; nesting rules.
- `quickstart.md`: Build, regenerate, and test commands; sample program using Graph Assertion Block.

## Phase 2: Task Planning Approach
Task Generation Strategy:
- Derive tasks from grammar contracts, data model, and quickstart scenarios.
- Prioritize parser tests → grammar → AST model → generators → builder visitor → type rules → lowering → integration tests.

Ordering Strategy:
- TDD and dependency order strictly enforced.

Estimated Output:
- ~25 tasks across parser, AST, type system, lowering, tests.

## Guardrails & File Pointers (Do This, Not That)

### Parser & Grammar
- Files:
   - `src/parser/grammar/FifthLexer.g4` (verify tokens for `<{` as `GRAPH_LBRACE` and `}>` as `GRAPH_RBRACE` or add if missing)
   - `src/parser/grammar/FifthParser.g4`
- Changes:
   - Add rule: `graphAssertionBlock: GRAPH_LBRACE statements? GRAPH_RBRACE;`
   - Allow as statement: add alternative in `statement`: `graphAssertionBlock ';'`
   - Allow as expression: add alternative in `primaryExpression`: `graphAssertionBlock`
   - Ensure any block type can nest any other (graph-in-regular and regular-in-graph) — add tests.
- DO: Require a trailing `;` for statement form to match existing statement termination.
- DON’T: Reuse `{`/`}` for graph blocks; avoid token overlap and ambiguity.

### AST Model & Generation
- Files:
   - `src/ast-model/AstMetamodel.cs` (add `GraphAssertionBlockExp`, `GraphAssertionBlockStmt`)
   - `src/ast-model/TypeSystem/*` and `src/ast-model/AstTypeProvider.cs` (types)
   - Regenerate: `just run-generator` (updates `src/ast-generated/*`)
- DO: Only edit `ast-model`; never hand-edit `src/ast-generated/*`.
- Node shape: block has `Statements`, yields `graph` in expression contexts.

### Type System
- Files:
   - `src/ast-model/TypeSystem/` (introduce `store`, `graph`, `triple`, `iri`)
   - `src/ast-model/AstTypeProvider.cs` (wire new types)
- Rules:
   - `graph += graph` valid; `store += graph` valid.
   - `graphAssertionBlock` expression type is `graph`.
   - Standalone `graphAssertionBlock` statement lowers to default store persist; error if default missing (align FR-019).
- DON’T: Add transactional semantics for default store; non-goal per spec.

### Parser → AST Builder
- File: `src/parser/AstBuilderVisitor.cs`
- Change: Map `graphAssertionBlock` context to new AST node(s); preserve nested content.

### Lowering (Desugaring)
- Files:
   - `src/compiler/LanguageTransformations/` → new `GraphAssertionLoweringVisitor.cs`
   - `src/compiler/ParserManager.cs` → register lowering pass before IL conversion
- Behavior:
   - Expression blocks: build an `IGraph` via `KG` and populate via assertions; yield the graph value.
   - Statement blocks: same, then emit explicit `store += graph` to default; error if no default configured.
- DON’T: Bypass lowering by special-casing IL generation.

### Knowledge Graph Built-ins
- Project: `src/fifthlang.system/` (`Fifth.System`)
- Ensure `KG` surface from `KnowledgeGraphs.cs` is resolvable as built-ins in symbol table.
- Optional cleanup: add `GlobalUsings.cs` with `using VDS.RDF; using VDS.RDF.Storage;` to reduce boilerplate.

### Default Store Handling
- Define how “default store” is provided (config/symbol). Tests must cover both configured and missing default.
- Diagnostics: Provide specific error ID/message for missing default store.

### Tests (TDD Sequence)
- Parser tests (first):
   - Statement form with `}>;`
   - Expression form assigned to variable
   - Nesting both directions
- AST/type tests:
   - Node shape and `graph` typing; `+=` rules
- Lowering tests:
   - Verify lowered calls to `KG` APIs
- Runtime integration:
   - Use dotNetRdf `TripleStore` to verify persisted triples via `store += graph`.

### Do/Don’t Quick List
- DO: Regenerate AST with `just run-generator` after model changes.
- DO: Keep persistence explicit (no auto-persist in expression form).
- DO: Keep default store non-transactional (no rollback).
- DON’T: Edit generated files.
- DON’T: Introduce hidden globals; pass/store default explicitly.

## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|
| None | — | — |

## Progress Tracking
**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [x] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/specs/.specify/memory/constitution.md`*
