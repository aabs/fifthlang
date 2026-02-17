# Implementation Plan: Remove Graph Assertion Block (GAB)

**Branch**: `[001-gab-removal]` | **Date**: 2025-11-09 | **Spec**: `specs/001-gab-removal/spec.md`
**Input**: Feature specification from `/specs/001-gab-removal/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Remove the Graph Assertion Block (GAB) language construct entirely while preserving all RDF-related functionality (triple literals, RDF datatypes, valid store declarations). Eliminate grammar rules and AST nodes specific to GAB. Ensure the compiler produces standard syntax errors for any former GAB constructs (no GAB-specific messaging). Update docs/examples to remove references. Maintain RDF feature regressions green.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# 14 (per constitution)  
**Primary Dependencies**: ANTLR 4.8 runtime; internal AST generator; xUnit + FluentAssertions  
**Storage**: N/A  
**Testing**: xUnit test projects (`test/syntax-parser-tests`, `test/runtime-integration-tests`, `test/kg-smoke-tests`)  
**Target Platform**: .NET 8 CLI tools on macOS/Linux  
**Project Type**: Multi-project .NET solution (compiler, parser, generator, tests)  
**Performance Goals**: No change from baseline; parse/build times remain within documented guidance  
**Constraints**: Follow generator-as-source-of-truth; no edits in `src/ast-generated/`; never cancel builds/tests  
**Scale/Scope**: Repo-wide grammar/AST surface cleanup; contained to parser, ast-model, docs, and tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Library-first, contracts-first: Changes impact AST metamodel (`src/ast-model/AstMetamodel.cs`) and grammar; generators used to update outputs. PASS
- Generator-as-source-of-truth: No hand edits under `src/ast-generated/`. PASS
- Test-first: Ensure example validation and existing suites pass; do not add GAB-specific tests. PASS
- Parser & Grammar Integrity: Remove rules; update visitor; remove any GAB samples. PASS
- Observability: Standard diagnostics only (no special GAB messages). PASS
- Simplicity & minimal surface: Eliminates unused feature; reduces complexity. PASS

## Project Structure

### Documentation (this feature)

```text
specs/001-gab-removal/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output (/speckit.plan)
├── data-model.md        # Phase 1 output (/speckit.plan)
├── quickstart.md        # Phase 1 output (/speckit.plan)
├── contracts/           # Phase 1 output (/speckit.plan)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: Use existing repository structure. Touch points: `src/ast-model/`, `src/parser/grammar/`, `src/parser/AstBuilderVisitor.cs`, `test/syntax-parser-tests/`, `test/runtime-integration-tests/`, `test/kg-smoke-tests/`, `docs/`, and validator `scripts/validate-examples.fish`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
