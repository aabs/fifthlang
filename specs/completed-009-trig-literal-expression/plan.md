# Implementation Plan: TriG Literal Expression Type

**Branch**: `001-trig-literal-expression` | **Date**: 2025-11-11 | **Spec**: specs/001-trig-literal-expression/spec.md
**Input**: Feature specification from `/specs/001-trig-literal-expression/spec.md`

## Summary

Introduce a new surface syntax literal, the TriG Literal Expression, delimited by `@< ... >`, which evaluates to a `Store`. Content is TriG with `{{ expression }}` interpolations. Compiler work spans parser additions, AST metamodel update (`TriGLiteralExpression`), AST building, a lowering pass to construct a `Store` and load TriG using platform libraries, and tests. Interpolations use default type mappings (strings quoted/escaped; numbers/booleans bare; datetime → `xsd:dateTime`; IRIs via `<...>` or prefixed names). Literal braces are escaped with triple braces (`{{{` → `{{`, `}}}` → `}}`).

## Technical Context

**Language/Version**: C# 14 on .NET 8.0 SDK (global.json pins 8.0.118)  
**Primary Dependencies**: Antlr4 runtime; existing compiler/AST generator; dotNetRDF types for runtime store parsing (via Fifth.System integration)  
**Storage**: In-memory RDF dataset (`Store`)  
**Testing**: TUnit + FluentAssertions across parser, AST, and runtime integration test projects  
**Target Platform**: Cross-platform .NET 8 (macOS/Linux/Windows)
**Project Type**: Multi-project .NET solution with generator and compiler pipeline  
**Performance Goals**: Accept TriG literals up to ~100KB; full solution build time delta ≤ 5% versus baseline without the literal  
**Constraints**: No hand edits to generated code; grammar integrity; deterministic diagnostics; adhere to lowering patterns  
**Scale/Scope**: Single feature spanning AST, grammar, transformations, and tests; no new top-level projects

Open items: None (feature clarifications resolved in spec and research).

## Constitution Check

Gate evaluation before design:
- Library-First, Contracts-First: Changes limited to existing libraries (`ast-model`, `parser`, `compiler`, tests). PASS
- Generator-as-Source-of-Truth: Metamodel updated; regenerate `src/ast-generated/` via generator. PASS
- Test-First: Add parser samples, AST tests, and runtime integration tests before implementation. PASS
- Parser & Grammar Integrity: Update both `FifthLexer.g4` and `FifthParser.g4` plus `AstBuilderVisitor.cs`. PASS
- CLI/Text I/O Discipline: No new executables; existing CLIs unchanged. PASS
- Reproducible Builds: Toolchain unchanged; follow documented commands. PASS
- Simplicity & Safety: Introduce one AST node and one lowering pass; minimal surface change. PASS

Re-check after Phase 1: expected PASS (no complexity increases or generated-code edits outside policy).

Post-design verification:
- Metamodel change planned only (add AST node) → aligns with Generator-as-Source-of-Truth. PASS
- Tests enumerated for parser/AST/runtime integration → satisfies Test-First. PASS
- No new executables or external dependencies introduced. PASS
- Diagnostic granularity plan consistent with Observability & Diagnostics section. PASS
- No complexity gate violations (no new project, no architectural divergence). PASS

## Project Structure

### Documentation (this feature)

```text
specs/001-trig-literal-expression/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
```

### Source Code (repository root)

```text
src/
├── ast-model/
│   └── AstMetamodel.cs                 # Add TriGLiteralExpression
├── ast-generated/                      # Regenerated (do not hand-edit)
├── parser/
│   ├── grammar/
│   │   ├── FifthLexer.g4               # Add token(s) for @< ... > literal
│   │   └── FifthParser.g4              # Add rule for TriG literal expression
│   └── AstBuilderVisitor.cs            # Build TriGLiteralExpression AST node
├── compiler/
│   ├── LanguageTransformations/
│   │   └── TriGLiteralLoweringRewriter.cs   # New lowering pass
│   └── ParserManager.cs                # Insert pass into pipeline
└── fifthlang.system/                   # Existing Store type

test/
├── syntax-parser-tests/                # Positive/negative samples for literal
├── ast-tests/                          # AST node + builder tests
└── runtime-integration-tests/          # End-to-end Store initialization
```

**Structure Decision**: Extend existing AST/Parser/Compiler projects; no new projects introduced. All generated code continues under `src/ast-generated/` per policy.

## Complexity Tracking

No violations anticipated; no additional complexity justified.
