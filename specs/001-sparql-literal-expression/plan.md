# Implementation Plan: Embedded SPARQL Queries

**Branch**: `001-sparql-literal-expression` | **Date**: 2025-11-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-sparql-literal-expression/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a new AST type `SparqlLiteralExpression` for inline SPARQL queries using `?< ... >` syntax. The literal maps to a new system type `Fifth.System.Query` (user-facing as `Query`), supports safe variable binding via parameter insertion (dotNetRDF `SparqlParameterizedString`), and optionally supports `{{expr}}` interpolation for computed values. The implementation follows Fifth's multi-pass compilation model: grammar extension → AST building → type checking → lowering/transformation passes → IL/Roslyn emission.

## Technical Context

**Language/Version**: C# 14, .NET 8.0 SDK (global.json pins 8.0.118)  
**Primary Dependencies**: ANTLR 4.8 runtime (`Antlr4.Runtime.Standard`), dotNetRDF (`VDS.RDF.*`), RazorLight (code generation), TUnit + FluentAssertions (testing)  
**Storage**: N/A (in-memory AST and query objects)  
**Testing**: TUnit (unit tests under `test/ast-tests/`, `test/syntax-parser-tests/`), FluentAssertions for assertions, integration tests under `test/runtime-integration-tests/`  
**Target Platform**: .NET 8.0 cross-platform (macOS, Linux, Windows)  
**Project Type**: Compiler/language infrastructure (single solution with multiple projects)  
**Performance Goals**: Parse/compile SPARQL literals with <50ms overhead per literal for typical queries (<5KB), validate syntax at compile-time  
**Constraints**: No runtime query execution in compiler; maintain deterministic build times (<2 minutes for full solution); zero injection vulnerabilities (100% parameterized binding)  
**Scale/Scope**: Support SPARQL 1.1 query/update syntax; handle literals up to 1MB (with diagnostic for larger); expected usage: 1-20 literals per source file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Core Principles Compliance

✅ **I. Library-First, Contracts-First**
- New AST node `SparqlLiteralExpression` added to `src/ast-model/AstMetamodel.cs`
- Grammar extended in `src/parser/grammar/FifthLexer.g4` and `src/parser/grammar/FifthParser.g4`
- New system type `Fifth.System.Query` in `src/fifthlang.system/`
- Builders/visitors auto-generated via `ast_generator`

✅ **II. CLI and Text I/O Discipline**
- Compiler diagnostics via stderr (syntax errors in SPARQL literals)
- Standard compilation flow; no new CLI surface

✅ **III. Generator-as-Source-of-Truth**
- AST metamodel changes → regenerate builders/visitors
- No hand-edits to `src/ast-generated/`

✅ **IV. Test-First**
- TDD workflow: add parser tests (`test/syntax-parser-tests/`), AST tests (`test/ast-tests/`), integration tests (`test/runtime-integration-tests/`)
- Grammar samples in `src/parser/grammar/test_samples/*.5th`

✅ **V. Reproducible Builds & Toolchain Discipline**
- .NET 8.0.118, ANTLR 4.8, Java 17+
- Build order: ast-model → ast_generator → ast-generated → parser → compiler → tests

✅ **VI. Simplicity, Minimal Surface**
- Reuses existing literal pattern (primitive, TriG)
- Leverages dotNetRDF for SPARQL parsing/parameterization
- No new complex abstractions; follows existing AST model conventions

✅ **VII. Multi-Pass Compilation**
- Parse → AST build → type annotation → lowering (if needed) → Roslyn emission
- SPARQL literal parsing integrated into AstBuilderVisitor
- Type checking maps `SparqlLiteralExpression` → `Query`

✅ **VIII. AST Design & Transformation Strategy**
- High-level AST: `SparqlLiteralExpression` with variable bindings
- Lowering pass (if needed): resolve variable references, construct parameterized query object
- Follows transformation pattern: prefer multiple simple passes

✅ **IX. Parser & Grammar Integrity**
- Grammar changes: add `SPARQL_START`, `SPARQL_CLOSE_ANGLE` tokens to lexer
- Parser rule: `sparqlLiteral: SPARQL_START sparqlLiteralContent* SPARQL_CLOSE_ANGLE`
- Update `AstBuilderVisitor.cs` to construct `SparqlLiteralExpression`
- Test samples validate parsing

✅ **X. Observability & Diagnostics**
- Clear compile-time diagnostics for malformed SPARQL, unknown variables
- Line/column reporting within literal body

✅ **XI. Versioning & Backward Compatibility**
- New feature (additive); no breaking changes
- Minor version bump

### Gates Evaluation

**PASS**: No constitution violations. Feature aligns with existing patterns (TriG literals precedent), follows multi-pass architecture, maintains test discipline, and uses generator-driven infrastructure.

## Project Structure

### Documentation (this feature)

```text
specs/001-sparql-literal-expression/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (generated below)
├── data-model.md        # Phase 1 output (generated below)
├── quickstart.md        # Phase 1 output (generated below)
├── contracts/           # Phase 1 output (generated below)
│   └── Query.cs         # Fifth.System.Query type contract
├── checklists/
│   └── requirements.md  # Spec quality checklist (completed)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── ast-model/
│   └── AstMetamodel.cs              # Add SparqlLiteralExpression node
├── ast-generated/                   # Auto-regenerated by ast_generator
│   ├── builders.generated.cs        # Includes SparqlLiteralExpressionBuilder
│   └── visitors.generated.cs        # Includes visitor methods for SparqlLiteralExpression
├── parser/
│   ├── grammar/
│   │   ├── FifthLexer.g4           # Add SPARQL_START, SPARQL_CLOSE_ANGLE tokens
│   │   ├── FifthParser.g4          # Add sparqlLiteral rule
│   │   └── test_samples/
│   │       └── sparql-literal.5th  # Test samples for new syntax
│   └── AstBuilderVisitor.cs        # Add VisitSparqlLiteral method
├── compiler/
│   └── LanguageTransformations/
│       ├── SparqlVariableBindingVisitor.cs  # Resolve in-scope variable references (NEW)
│       └── TypeAnnotationVisitor.cs         # Map SparqlLiteralExpression → Query type
└── fifthlang.system/
    ├── Query.cs                     # Fifth.System.Query wrapper (NEW)
    └── KnowledgeGraphs.cs           # May add query execution helpers (future)

test/
├── syntax-parser-tests/
│   └── SparqlLiteralTests.cs        # Grammar parsing tests (NEW)
├── ast-tests/
│   └── SparqlLiteralAstTests.cs     # AST construction tests (NEW)
└── runtime-integration-tests/
    └── SparqlLiteralIntegrationTests.cs  # End-to-end tests (NEW)
```

**Structure Decision**: Single-project compiler infrastructure. New AST node follows existing literal pattern (primitive, TriG). Grammar extended in split lexer/parser. New system type in `fifthlang.system` library. Testing across three tiers: parser, AST, integration.

## Complexity Tracking

No constitution violations. This feature:
- Reuses existing literal pattern (follows TriG literal precedent)
- Fits within single-project compiler structure
- Adds minimal new abstractions (one AST node, one system type)
- Leverages existing dotNetRDF for SPARQL parsing
- No new projects or repositories required

## Post-Design Constitution Re-Check

*Re-evaluation after Phase 1 design (research, data model, contracts, quickstart)*

### Design Validation

✅ **Contracts Match Constitution Expectations**
- `SparqlLiteralExpression` follows AST metamodel conventions (partial record, required properties)
- `Fifth.System.Query` follows system library pattern (sealed class, internal constructor)
- Grammar extensions use lexer/parser split correctly
- No hand-edited generated files

✅ **Transformation Pipeline Aligns with Multi-Pass Architecture**
- Two new visitors: `SparqlVariableBindingVisitor` (binding resolution), `SparqlLoweringRewriter` (code generation)
- Correct ordering: after symbol table, before IL/Roslyn backend
- Each visitor has single responsibility

✅ **Testing Strategy Meets Test-First Requirements**
- Parser tests: `test/syntax-parser-tests/SparqlLiteralTests.cs`
- AST tests: `test/ast-tests/SparqlLiteralAstTests.cs`
- Integration tests: `test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs`
- Security tests: injection prevention scenarios
- Quickstart provides executable examples

✅ **Simplicity & Minimal Surface Maintained**
- No new complex abstractions beyond one AST node and one system type
- Reuses dotNetRDF (already dependency for TriG/KG)
- Follows TriG literal precedent precisely
- No framework additions or architectural changes

✅ **Reproducible Build Preserved**
- No new build steps; AST regeneration sufficient
- No new external dependencies beyond existing dotNetRDF
- Build order unchanged: ast-model → generator → ast-generated → parser → compiler → tests

### Final Gate: PASS

All constitution principles respected. Feature ready for Phase 2 (task breakdown via `/speckit.tasks`).
