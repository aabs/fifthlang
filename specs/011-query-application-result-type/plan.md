# Implementation Plan: Query Application and Result Type

**Branch**: `011-query-application-result-type` | **Date**: 2025-11-15 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/011-query-application-result-type/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Introduce SPARQL query application to stores using the `<-` operator, returning results in a discriminated union `Result` type that handles tabular (SELECT), graph (CONSTRUCT/DESCRIBE), and boolean (ASK) outcomes. This enables developers to query RDF knowledge graphs with minimal syntactic noise while maintaining type safety. The implementation requires grammar extension, new runtime types in Fifth.System, AST transformation passes for type checking and lowering, and comprehensive error handling with structured diagnostics.

Technical approach: Add `<-` token to lexer grammar, create Result discriminated union wrapping dotNetRDF's SparqlResultSet, implement query application as a compiler-recognized operation that lowers to Fifth.System API calls, provide streaming/iterator access for large SELECT results, add runtime validator for SPARQL injection detection, support optional cancellation tokens, and ensure concurrent read query isolation.

## Technical Context

**Language/Version**: C# 14, .NET SDK 8.0.118 (per global.json)  
**Primary Dependencies**: ANTLR 4.8 runtime (`Antlr4.Runtime.Standard`), dotNetRDF (`VDS.RDF.*`), RazorLight (code generation), TUnit + FluentAssertions (testing)  
**Storage**: In-memory RDF triple stores via dotNetRDF (`ITripleStore`, `TripleStore`)  
**Testing**: TUnit with FluentAssertions; test projects: `test/syntax-parser-tests/`, `test/ast-tests/`, `test/runtime-integration-tests/`  
**Target Platform**: Cross-platform .NET 8.0 (Linux, macOS, Windows)  
**Project Type**: Single project (compiler + runtime library)  
**Performance Goals**: 
- SELECT queries returning 100k rows: < 1.5× baseline memory, < 10% throughput degradation vs direct dotNetRDF
- Cancellation response: < 200ms termination latency (95th percentile)
- Parallel read queries: ≤ 5% per-query latency variance vs isolated execution  
**Constraints**: 
- Must not break existing Query and Store type implementations
- Must maintain grammar token precedence (no conflicts with existing operators)
- Transformation passes must preserve AST validity and type safety
- Generated code must follow constitution's "do not hand-edit" discipline  
**Scale/Scope**: 
- 4 SPARQL query forms (SELECT, CONSTRUCT, DESCRIBE, ASK)
- 8 error Kind values (comprehensive diagnostics taxonomy)
- Target: 30+ unsafe query composition test cases (SC-008)
- Integration: ~6 new AST node types, ~4 transformation passes, 1 grammar token

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Library-First, Contracts-First
**Status**: PASS  
**Analysis**: Feature introduces runtime types (Result, QueryError) in existing `fifthlang.system` library with clear contracts. Grammar extension to `FifthLexer.g4` and AST nodes in `AstMetamodel.cs` follow established patterns. No new organizational libraries required.

### ✅ CLI and Text I/O Discipline
**Status**: PASS  
**Analysis**: Compiler remains primary CLI surface; query application lowers to runtime library calls. No new CLI tools introduced. Diagnostics emit to stderr as structured messages per constitution.

### ✅ Generator-as-Source-of-Truth
**Status**: PASS  
**Analysis**: All new AST nodes (QueryApplicationExp, ResultType, etc.) will be defined in `src/ast-model/AstMetamodel.cs`. Generated builders/visitors will be produced via `just run-generator`. No hand-edits to `src/ast-generated/` permitted.

### ✅ Test-First (Non-Negotiable)
**Status**: PASS  
**Analysis**: TDD workflow mandated: grammar tests in `test/syntax-parser-tests/`, AST transformation tests in `test/ast-tests/`, end-to-end integration tests in `test/runtime-integration-tests/`. All 13 success criteria (SC-001 through SC-013) require measurable test coverage before implementation.

### ✅ Reproducible Builds & Toolchain Discipline
**Status**: PASS  
**Analysis**: No new external dependencies beyond existing dotNetRDF. ANTLR grammar changes follow standard build process. All timeouts honored per constitution (restore: 120s, build: 120s, test: 300s).

### ✅ Simplicity, Minimal Surface, and Safety
**Status**: PASS with JUSTIFICATION  
**Analysis**: Feature adds 1 operator (`<-`), 1 discriminated union type (Result with 3 cases), 1 error type (QueryError with 8 Kind values). Complexity justified by:
- Query application is fundamental to semantic web workflows (FR-001 through FR-011)
- Discriminated union is simplest design vs separate types per query form
- 8 error Kind values cover distinct failure modes without over-fragmentation
- Streaming/cancellation features prevent memory issues and improve UX (FR-013, FR-015)

### ✅ Multi-Pass Compilation & AST Lowering Philosophy
**Status**: PASS  
**Analysis**: Implementation follows standard multi-pass pipeline:
1. Lexer adds `<-` token (Phase 1: Lexical Analysis)
2. Parser recognizes query application syntax (Phase 1: Parsing)
3. AstBuilderVisitor creates QueryApplicationExp nodes (Phase 1: AST Building)
4. Type inference pass validates Query and Store types (Phase 2: Analysis)
5. Security validator checks for injection patterns (Phase 2: Transformation)
6. Lowering pass transforms QueryApplicationExp to Fifth.System API calls (Phase 2: Transformation)
7. Roslyn backend emits C# code calling dotNetRDF (Phase 4: Code Generation)

### ✅ AST Design & Transformation Strategy
**Status**: PASS  
**Analysis**: Design follows two-level AST approach:
- **Main AST**: QueryApplicationExp (high-level), ResultType (discriminated union)
- **Lowering Strategy**: Use `DefaultAstRewriter` (preferred pattern per constitution) for query application lowering, enabling statement hoisting for error handling and streaming setup
- **Transformation Passes**:
  - QueryApplicationTypeCheckVisitor (validates operand types)
  - SparqlSecurityValidator (FR-012 injection detection)
  - QueryApplicationLoweringRewriter (transforms to System API calls)
- Each pass has single responsibility; order-dependent execution managed by ParserManager

### ✅ Parser & Grammar Integrity
**Status**: PASS  
**Analysis**: 
- Grammar changes isolated to `FifthLexer.g4` (add `<-` token) and `FifthParser.g4` (add query application expression rule)
- AstBuilderVisitor updated to handle new syntax
- Test samples added to `src/parser/grammar/test_samples/*.5th`
- All `.5th` examples will be validated via CI `validate-examples` step per constitution

### ✅ Observability & Diagnostics
**Status**: PASS  
**Analysis**: Structured QueryError type (FR-014, FR-017) with Kind enumeration provides actionable diagnostics. Messages include SourceSpan, UnderlyingExceptionType, and Suggestion fields. Emits to stderr with file/line/column info.

### ✅ Versioning & Backward Compatibility
**Status**: PASS  
**Analysis**: New feature (no breaking changes to existing APIs). Query and Store types remain unchanged. Additive grammar change (`<-` operator). Version bump: MINOR increment per semver.

### Summary
**Overall Status**: ✅ ALL GATES PASS

No constitution violations. Feature aligns with Library-First principle (extends Fifth.System), follows AST transformation strategy (multi-pass lowering), maintains test discipline (TDD with TUnit), and adheres to grammar integrity (split lexer/parser with samples). Complexity justified by user value and semantic web domain requirements.

## Project Structure

### Documentation (this feature)

```text
specs/011-query-application-result-type/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (generated below)
├── data-model.md        # Phase 1 output (generated below)
├── quickstart.md        # Phase 1 output (generated below)
├── contracts/           # Phase 1 output (generated below)
│   ├── Result.api.md    # Result discriminated union contract
│   ├── QueryError.api.md # QueryError diagnostics contract
│   └── QueryApplicationOp.api.md # Query application operator semantics
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── ast-model/
│   ├── AstMetamodel.cs           # [MODIFIED] Add QueryApplicationExp, ResultType nodes
│   └── TypeSystem/               # [MODIFIED] Update type registry for Result type
├── ast-generated/                # [REGENERATED] All builders/visitors updated
│   ├── builders.generated.cs
│   ├── visitors.generated.cs
│   ├── rewriter.generated.cs
│   └── typeinference.generated.cs
├── parser/
│   ├── grammar/
│   │   ├── FifthLexer.g4         # [MODIFIED] Add QUERY_APPLICATION_OP token '<-'
│   │   ├── FifthParser.g4        # [MODIFIED] Add queryApplicationExpr rule
│   │   └── test_samples/         # [NEW] query_application_*.5th samples
│   │       ├── query_application_select.5th
│   │       ├── query_application_construct.5th
│   │       ├── query_application_ask.5th
│   │       └── query_application_errors.5th
│   └── AstBuilderVisitor.cs      # [MODIFIED] Handle VisitQueryApplicationExpr
├── compiler/
│   ├── ParserManager.cs          # [MODIFIED] Register new transformation passes
│   └── LanguageTransformations/
│       ├── QueryApplicationTypeCheckVisitor.cs  # [NEW] Validate Query/Store types
│       ├── SparqlSecurityValidator.cs           # [NEW] FR-012 injection detection
│       └── QueryApplicationLoweringRewriter.cs  # [NEW] Lower to System API calls
└── fifthlang.system/
    ├── ResultType.cs             # [NEW] Discriminated union: TabularResult|GraphResult|BooleanResult
    ├── QueryError.cs             # [NEW] Structured diagnostics with Kind enumeration
    ├── QueryApplicationExecutor.cs  # [NEW] Wraps dotNetRDF execution with streaming
    └── SparqlValidationRules.cs  # [NEW] FR-012 unsafe pattern detection

test/
├── syntax-parser-tests/
│   └── QueryApplicationGrammarTests.cs  # [NEW] Grammar correctness tests
├── ast-tests/
│   ├── QueryApplicationTypeCheckTests.cs # [NEW] Type validation tests
│   ├── QueryApplicationLoweringTests.cs  # [NEW] AST transformation tests
│   └── SparqlSecurityValidatorTests.cs   # [NEW] SC-008 injection tests
└── runtime-integration-tests/
    ├── QueryApplicationSelectTests.cs    # [NEW] SC-001, SC-006, SC-009 tests
    ├── QueryApplicationConstructTests.cs # [NEW] SC-002 CONSTRUCT tests
    ├── QueryApplicationAskTests.cs       # [NEW] SC-007 ASK boolean tests
    ├── QueryApplicationErrorTests.cs     # [NEW] SC-004, SC-010, SC-013 error tests
    ├── QueryCancellationTests.cs         # [NEW] SC-011 cancellation tests
    └── QueryConcurrencyTests.cs          # [NEW] SC-012 parallel query tests
```

**Structure Decision**: Single project structure (Option 1) is appropriate. Feature extends existing compiler (`src/compiler/`) and runtime library (`src/fifthlang.system/`) without requiring separate frontend/backend or mobile components. Grammar modifications follow established split lexer/parser pattern in `src/parser/grammar/`. All new AST nodes reside in `src/ast-model/` per constitution's Generator-as-Source-of-Truth principle.

## Complexity Tracking

> **No violations requiring justification**

This feature introduces moderate complexity (1 new operator, discriminated union with 3 cases, 8 error kinds, 3 transformation passes) but does not violate constitution constraints. All additions follow established patterns:

- New operator `<-` is standard grammar extension (precedent: existing operators)
- Result discriminated union is simplest design vs alternative separate types
- 8 QueryError.Kind values provide semantic precision without over-fragmentation
- Transformation passes (type check, security validation, lowering) each have single responsibility
- No new project/repository/framework introductions

Complexity is proportional to feature scope (supporting 4 SPARQL query forms with comprehensive error handling) and aligns with semantic web domain requirements documented in specification.

---

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design completion (2025-11-15)*

### ✅ All Gates Still Pass

**Generator-as-Source-of-Truth**: Confirmed - all new AST nodes defined in AstMetamodel.cs; data-model.md specifies QueryApplicationExp with proper field definitions. No hand-edits to ast-generated/ permitted.

**AST Design & Transformation Strategy**: Validated - research.md documents use of DefaultAstRewriter (preferred pattern) for QueryApplicationLoweringRewriter. Three distinct transformation passes (type check, security validation, lowering) each have single responsibility. Two-level AST maintained (QueryApplicationExp in main AST, lowers to function calls).

**Parser & Grammar Integrity**: Confirmed - research.md specifies exact lexer token (`QUERY_APPLICATION_OP : '<-'`) and parser rule (`queryApplicationExpr`). Contracts document full operator semantics. Test samples will be added to `src/parser/grammar/test_samples/*.5th`.

**Test-First Discipline**: Design artifacts include comprehensive test strategies:
- Grammar tests in syntax-parser-tests/
- AST transformation tests in ast-tests/
- Integration tests in runtime-integration-tests/
- All 13 success criteria (SC-001 through SC-013) mapped to test requirements in contracts/

**Simplicity & Minimal Surface**: Design review confirms:
- Result type uses standard dunet union pattern (existing in codebase)
- QueryError follows Fifth.System conventions (matches existing Query, Store types)
- Operator precedence documented; no conflicts with existing tokens
- Security validator is additive validation layer (doesn't modify core pipeline)

### No New Risks Identified

Phase 0 research and Phase 1 design validated all technical decisions:
- Discriminated union implementation (dunet library, already in use)
- Grammar token precedence (ANTLR longest-match rule, no ambiguities)
- Exception mapping strategy (explicit switch-based handler, testable)
- Streaming approach (leverages dotNetRDF native support, no custom complexity)
- Concurrency model (ReaderWriterLockSlim, standard .NET synchronization)

### Design Quality Indicators

- **Contracts Complete**: 3 API contracts (Result, QueryError, QueryApplicationOp) with full signatures, examples, security considerations
- **Data Model Complete**: 5 entities defined with fields, validation rules, relationships diagram
- **Research Complete**: 7 technical decisions documented with alternatives considered and rationales
- **Quickstart Complete**: End-to-end examples for all 4 SPARQL query forms, error handling, performance tips
- **Agent Context Updated**: GitHub Copilot context includes new technologies

### Conclusion

✅ **APPROVED for Phase 2 (Task Breakdown)**

Design adheres to all constitution principles. Ready to proceed to `/speckit.tasks` for implementation task decomposition.
