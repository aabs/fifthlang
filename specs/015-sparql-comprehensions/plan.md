# Implementation Plan: SPARQL Comprehensions

**Branch**: `[015-sparql-comprehensions]` | **Date**: 2025-12-13 | **Spec**: specs/015-sparql-comprehensions/spec.md
**Input**: Feature specification from `specs/015-sparql-comprehensions/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement the new list-comprehension syntax (`from` / `where`) and extend it to support SPARQL comprehensions that map a tabular SPARQL SELECT result into a typed list.

Key technical approach:
- Update grammar + AST builder to parse the new comprehension syntax.
- Redesign `ListComprehension` AST to be fully typed (source/projection are AST expressions, constraints are a list).
- Add compile-time SPARQL introspection using the generated ANTLR SPARQL parser to validate SELECT form and extract projected variables.
- Lower comprehensions via an AST rewrite pass into list allocation + iteration + filtering + append.

Phase 0/1 artifacts:
- specs/015-sparql-comprehensions/research.md
- specs/015-sparql-comprehensions/data-model.md
- specs/015-sparql-comprehensions/quickstart.md
- specs/015-sparql-comprehensions/contracts/diagnostics.md

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# 14 on .NET SDK 8.0.118 (global.json)  
**Primary Dependencies**: ANTLR4.Runtime, generated ANTLR parsers, dotNetRDF (`VDS.RDF.*`)  
**Storage**: N/A (in-memory AST + in-memory/runtime RDF store abstractions)  
**Testing**: TUnit + FluentAssertions (parser + runtime integration tests)  
**Target Platform**: Cross-platform .NET (validated in Linux CI)  
**Project Type**: Compiler toolchain (parser + compiler + system library)  
**Performance Goals**: Keep compilation deterministic; no new asymptotic hot paths (SPARQL parse only for literals used in comprehensions)  
**Constraints**: Never hand-edit `src/ast-generated/`; breaking syntax change must be rejected clearly (FR-013)  
**Scale/Scope**: Medium feature spanning grammar, AST, compiler validations, lowering, and tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Gates (from `.specify/memory/constitution.md`):

- **Generator-as-source-of-truth**: any AST contract change must be done via `src/ast-model/AstMetamodel.cs` and regenerated; no manual edits under `src/ast-generated/`.
- **Test-first / end-to-end required**: add parser + runtime integration tests that compile and execute Fifth code using the new syntax.
- **Grammar integrity**: update `FifthLexer.g4` / `FifthParser.g4` and `AstBuilderVisitor.cs` together.
- **Prefer lowering passes**: implement comprehensions via AST transformations (not translator-only special cases).
- **Breaking change**: rejecting legacy `in`/`#` comprehension syntax must be covered by updated tests, a migration note, and a minor/major SemVer bump decision (constitution XI). Note: the repo release workflow generates GitHub release notes automatically.

Status: PASS for planning.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
```text
src/
├── ast-model/                      # Update AST contracts (ListComprehension redesign)
├── ast-generated/                  # Regenerated output (do not hand-edit)
├── parser/
│   ├── grammar/
│   │   ├── FifthLexer.g4           # Add/adjust FROM token; legacy rejection strategy
│   │   ├── FifthParser.g4          # Update list_comprehension rule and constraint list
│   │   └── SparqlParser.g4         # Used for compile-time SELECT/vars extraction
│   └── AstBuilderVisitor.cs        # Build the revised comprehension AST
├── compiler/
│   ├── ParserManager.cs            # Integrate new validation/lowering passes
│   └── LanguageTransformations/    # New/updated visitors/rewriters for comprehensions
└── fifthlang.system/               # Runtime helpers for tabular result row access/conversion

test/
├── syntax-parser-tests/            # Grammar-level tests for new syntax + legacy rejection
└── runtime-integration-tests/      # End-to-end tests exercising SPARQL comprehensions
```

**Structure Decision**: Extend existing compiler toolchain projects; no new projects or packages.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitution violations anticipated for this feature.

## Phase 0: Research (completed)

- See specs/015-sparql-comprehensions/research.md

## Phase 1: Design & Contracts (completed)

- Data model: specs/015-sparql-comprehensions/data-model.md
- Diagnostics contract: specs/015-sparql-comprehensions/contracts/diagnostics.md
- User quickstart: specs/015-sparql-comprehensions/quickstart.md

Post-design constitution check: still PASS (no generator edits; plan uses AST lowering + tests).

## Phase 2: Implementation Plan

### 2.1 Grammar & AST builder

- Update `FifthLexer.g4`:
  - introduce `FROM: 'from'` token (taking care of interactions with existing `WHERE` token)
  - reject legacy comprehension forms (`in` and `#`) with a clear parse/compile error (FR-013)
- Update `FifthParser.g4`:
  - implement the new comprehension grammar (projection + `from` generator + optional `where` constraint list)
  - reuse existing `object_instantiation_expression` for object projections
- Update `AstBuilderVisitor.cs`:
  - build the redesigned `ListComprehension` AST
  - preserve list literals `[a, b, c]` behavior

### 2.2 AST contract + regeneration

- Update `src/ast-model/AstMetamodel.cs`:
  - redesign `ListComprehension` fields to support:
    - `Projection: Expression`
    - `Source: Expression`
    - `Constraints: List<Expression>`
- Run generator (`just run-generator`) and build the full solution to validate.

### 2.3 Compile-time SPARQL introspection + comprehension validation

- Add a compiler validation pass for comprehensions that:
  - enforces boolean constraints (FR-007)
  - detects SPARQL object projections and requires RHS bindings are `?varName` (FR-008)
  - parses the SPARQL query text at compile time (FR-009a) to:
    - verify SELECT form (FR-005)
    - extract projected vars (for FR-009)
  - validates property names exist on projected type (FR-010)

### 2.4 Lowering / execution semantics

- Implement a lowering pass that desugars `ListComprehension` into:
  - list allocation
  - loop over source elements/rows
  - constraint checks (AND)
  - append projected value
  - return list

For SPARQL tabular results, lowering will require a stable way to enumerate rows and read variable bindings; add minimal helper APIs in `src/fifthlang.system/` as needed.

### 2.5 Tests

- Parser tests (`test/syntax-parser-tests/`):
  - new syntax parses
  - legacy `in`/`#` is rejected
  - multiple constraints in `where a, b, c` parse correctly
- Runtime integration (`test/runtime-integration-tests/`):
  - end-to-end: SELECT → comprehension → list of typed objects
  - empty result returns empty list
  - missing binding throws runtime error when accessed
  - unknown SPARQL var in projection fails compile-time

### 2.6 Docs / release note

- Add an in-repo migration note describing `in/#` → `from/where` with examples.
- Ensure the release tag uses an appropriate SemVer bump (minor/major) per `docs/Development/release-process.md` and constitution XI.
