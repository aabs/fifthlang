# Implementation Plan: Implementation of try/catch/finally control flow

**Branch**: `005-implementation-of-try` | **Date**: 2025-11-02 | **Spec**: specs/005-implementation-of-try/spec.md
**Input**: Feature specification from `/specs/005-implementation-of-try/spec.md`

## Summary

Implement C#-equivalent exception handling in Fifth: try/catch/finally with filters and rethrow semantics, plus C#-style throw expressions. Use Fifth-style exception bindings in catch parameters. Async IL equality is deferred until async support exists; validate structural equivalence for non-async. Reserve keywords `try`, `catch`, `finally`, `when`. Defer iterator/async-iterator contexts in v1. Pin IL golden tests to the .NET SDK in `global.json`. Enforce AST first-class modeling (no ad‑hoc annotations).

## Technical Context

**Language/Version**: C# (compiler implementation), Fifth language surface; .NET SDK 8.0.x (global.json pins 8.0.118)
**Primary Dependencies**: Antlr4.Runtime.Standard, RazorLight, System.CommandLine, TUnit, FluentAssertions, dunet, Vogen; Roslyn (for IL or backend equivalence tests)
**Storage**: N/A
**Testing**: TUnit + FluentAssertions; solution tests under `test/*` per constitution
**Target Platform**: .NET runtime on macOS/Linux/Windows; CLR exception model
**Project Type**: Single multi-project .NET solution (compiler, parser, generator, tests)
**Performance Goals**: No measurable regression on macrobenchmarks (p ≥ 0.05)
**Constraints**: SDK pinning for IL tests; Async IL equality (byte-for-byte) is deferred until async support exists
**Scale/Scope**: Parser + AST + semantics + IL emission + tests + docs for this feature only; iterators explicitly out-of-scope in v1

## Constitution Check

Must comply with:
- Library-first/contracts-first: Changes in `src/ast-model/*`, regenerate `src/ast-generated/*` (no manual edits), update parser grammar, and compiler passes. Contracts surfaced via metamodel additions (`TryStatement`, `CatchClause`, `FinallyBlock`, `ThrowExp`).
- Generator-as-source-of-truth: Update metamodels, regenerate via `just run-generator`.
- Test-first: Add parser, AST, semantic, IL structural/equality, and runtime tests first where practical.
- Toolchain discipline: Use .NET SDK from `global.json`; Java 17+ for ANTLR.
- Parser/grammar integrity: Update `FifthLexer.g4` and `FifthParser.g4`, add samples under `src/parser/grammar/test_samples/`.
- Transformation strategy: Prefer `DefaultAstRewriter` for lowering; maintain pass ordering in `ParserManager.cs`.
- Observability & diagnostics: Deterministic, actionable diagnostics; no broad catch masking failures.

Gate result: PASS (no constitution violations planned). Re-checked after Phase 1 design artifacts were generated (research/data-model/contracts/quickstart): still PASS. Next gate at implementation PR time will verify generator usage, grammar samples, and tests.

## Project Structure

### Documentation (this feature)

```text
specs/005-implementation-of-try/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A for external APIs; see README)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ast-model/                 # Add TryStatement, CatchClause, FinallyBlock, ThrowExp
├── ast-generated/             # REGENERATED (no manual edits)
├── parser/
│   ├── grammar/               # FifthLexer.g4, FifthParser.g4 (+ try/catch/finally, throwExpression)
│   └── AstBuilderVisitor.cs   # Build TryStatement, CatchClause, Finally, ThrowExp
├── compiler/
│   └── LanguageTransformations/   # Add/adjust passes if lowering is needed
└── code_generator/            # Ensure IL emission paths (or Roslyn backend equivalence tests)

test/
├── syntax-parser-tests/       # try/catch/finally + throw expression parsing
├── ast-tests/                 # AST shapes: TryStatement, CatchClause, FinallyBlock, ThrowExp
└── runtime-integration-tests/ # runtime semantics + IL structural/equality tests
```

**Structure Decision**: Extend existing compiler projects; no new top-level projects required.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
