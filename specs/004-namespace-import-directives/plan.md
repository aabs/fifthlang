# Implementation Plan: Namespace Import Directives

**Branch**: `004-namespace-import-directives` | **Date**: 2025-10-05 | **Spec**: `specs/004-namespace-import-directives/spec.md`
**Input**: Feature specification for namespace declarations and import directives

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If missing, stop with "No feature spec at {path}"
2. Populate Technical Context (ensure no NEEDS CLARIFICATION markers)
   → Detect project type from repository layout to drive structure decision
3. Complete Constitution Check section using constitution requirements
4. Evaluate Constitution Check
   → Document violations in Complexity Tracking or halt if unjustifiable
   → Mark Initial Constitution Check in Progress Tracking
5. Execute Phase 0 (produce `research.md`)
   → If unknowns remain, stop with "Resolve unknowns"
6. Execute Phase 1 (produce `data-model.md`, `quickstart.md`, `contracts/…`, update agent instructions)
7. Re-run Constitution Check after Phase 1 outputs
   → Address new violations or return to Phase 1 design
   → Mark Post-Design Constitution Check in Progress Tracking
8. Describe Phase 2 task-generation strategy (do not create `tasks.md`)
9. STOP – hand off to `/tasks`
```

## Summary
Implement file-scoped namespaces and module-local `import` directives for Fifth projects. The parser must recognize new syntax, the compiler must aggregate symbols across modules sharing a namespace, and imported namespaces must be resolved idempotently per module. Diagnostics warn about undeclared namespaces while preserving local symbol shadowing. Both MSBuild manifests and CLI-supplied file lists define the module set, and namespace resolution must complete within 2 seconds for 100-module projects.

## Technical Context
**Language/Version**: C# targeting .NET 8.0.118 (`global.json`)
**Primary Dependencies**: Antlr4.Runtime.Standard, RazorLight, System.CommandLine, MSBuild project evaluation
**Storage**: In-memory symbol tables and metadata structures
**Testing**: TUnit + FluentAssertions suites (`test/ast-tests`, `test/syntax-parser-tests`, `test/runtime-integration-tests`), `scripts/validate-examples.fish`
**Target Platform**: Cross-platform .NET CLI
**Project Type**: Single compiler toolchain repository serving language, parser, compiler, and runtime components
**Performance Goals**: Namespace resolution ≤ 2 seconds for 100-module projects
**Constraints**: TDD-first workflow, no manual edits under `src/ast-generated/`, diagnostics must include module + namespace identifiers, no cancelled `dotnet restore/build/test`
**Scale/Scope**: Projects with dozens to low hundreds of `.5th` modules, potentially cyclic namespace imports, mixed MSBuild and CLI entry points

## Constitution Check
- Uphold Section IV Test-First: add parser, compiler, and runtime tests before implementation; observe failing state.
- Respect Section III generator rule: modify `src/ast-model/AstMetamodel.cs` (or IL equivalent) and regenerate rather than editing `src/ast-generated/`.
- Maintain CLI diagnostic discipline per Sections II & X: structured stderr with module + namespace identifiers.
- Enforce build discipline: no cancelled `dotnet restore/build/test`, run `scripts/validate-examples.fish` when touching samples.
- Coordinate MSBuild + CLI enumeration without diverging code paths to satisfy Library-First principle.

**Status**: Initial Constitution Check – PASS | Post-Design Constitution Check – PASS

## Project Structure
```
specs/004-namespace-import-directives/
├── plan.md              # This plan (clean copy)
├── research.md          # Phase 0 outputs
├── data-model.md        # Phase 1 entity definitions
├── quickstart.md        # Phase 1 reproducible scenario
└── contracts/
    └── import-resolution.md  # Phase 1 contract

src/
├── ast-model/
├── parser/
│   ├── grammar/
│   └── AstBuilderVisitor.cs
├── compiler/
│   ├── ParserManager.cs
│   └── LanguageTransformations/
└── fifthlang.system/

test/
├── syntax-parser-tests/
├── runtime-integration-tests/
└── ast-tests/
```

**Structure Decision**: Single-toolchain repository; feature touches parser grammar/visitor, compiler symbol aggregation pipeline, AST model (if required), and associated TUnit suites plus CLI entry points.

## Phase 0: Outline & Research
- **Objectives**
  - Verify MSBuild project enumeration API for module discovery and namespace metadata exposure.
  - Identify symbol-table aggregation strategy for shared namespaces without duplicating scope entries.
  - Align diagnostic formatting with structured output requirements (module + namespace identifiers).
  - Establish baseline measurement approach for namespace resolution latency.
- **Outputs**: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/004-namespace-import-directives/research.md` (2025-10-05) documenting decisions, rationale, alternatives.
- **Status**: Completed.

## Phase 1: Design & Contracts
1. Extracted entities (`Module`, `NamespaceScope`, `ImportDirective`, `SymbolEntry`, `NamespaceImportGraph`) with attributes/state transitions in `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/004-namespace-import-directives/data-model.md`.
2. Authored provider/consumer contract for namespace import resolution in `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/004-namespace-import-directives/contracts/import-resolution.md`.
3. Captured quick verification scenario in `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/004-namespace-import-directives/quickstart.md`.
4. Agent instructions already reflect current tech stack (no new updates required).
- **Status**: Completed (2025-10-05).

## Phase 2: Task Planning Approach (for `/tasks`)
- **Task Generation Strategy**
  - Seed from `.specify/templates/tasks-template.md` into `tasks.md` (created by `/tasks`).
  - Derive contract-test tasks covering undeclared namespace warning, shadowing precedence, cyclic imports, and CLI multi-file enumeration scenarios.
  - Map entities to implementation tasks for namespace aggregation, import graph traversal, and CLI entry handling.
  - Translate quickstart and user stories into runtime and parser integration tests, including legacy `use` rejection.
  - Add implementation tasks strictly after corresponding failing tests to maintain TDD ordering.
- **Ordering**
  - TDD-first: parser and runtime tests before implementation.
  - Dependency chain: AST/model adjustments → parser grammar + visitor → compiler symbol loader → diagnostics/performance instrumentation.
  - Mark `[P]` where tests touch independent suites (e.g., parser vs runtime) to enable parallel work.
- **Expected Output**: 25–30 numbered tasks in `tasks.md` when `/tasks` executes.

## Phase 3+: Future Implementation (beyond /plan)
- **Phase 3**: `/tasks` command generates `tasks.md` from the above strategy.
- **Phase 4**: Execute tasks, implementing namespace import functionality per constitution.
- **Phase 5**: Validate via `dotnet build`, targeted test suites, quickstart scenario, and performance measurement.

## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|
| _None_    | _N/A_       | _N/A_                                 |

## Progress Tracking
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented (none required)
