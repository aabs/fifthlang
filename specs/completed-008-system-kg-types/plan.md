# Implementation Plan: System KG Types in Fifth.System

**Branch**: `001-system-kg-types` | **Date**: 2025-11-09 | **Spec**: `specs/001-system-kg-types/spec.md`
**Input**: Feature specification from `/specs/001-system-kg-types/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Transition `graph`, `triple`, and `store` from compiler primitives to `Fifth.System` types while keeping source compatibility. Bind lowercase type names as globally available predeclared types to `Fifth.System.Graph`, `Fifth.System.Triple`, and `Fifth.System.Store`. Preserve operator semantics (e.g., `graph += triple`) via library methods/overloads and update lowering/translation to target the new system APIs. Remove primitive registrations from the compiler.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# 14, .NET SDK 8.0.118 (per global.json)  
**Primary Dependencies**: dotNetRDF (`VDS.RDF.*`), Fifth.System library  
**Storage**: N/A (in-memory objects and SPARQL stores via library)  
**Testing**: xUnit + FluentAssertions; existing suites: ast-tests, syntax-parser-tests, runtime-integration-tests, kg-smoke-tests  
**Target Platform**: .NET 8 (macOS/Linux/Windows)  
**Project Type**: Multi-project .NET solution (compiler + system library)  
**Performance Goals**: KG operations within 5% of current baseline (SC-004)  
**Constraints**: No grammar changes; no source-breaking changes; globally available type names  
**Scale/Scope**: Limited surface change; wide test blast radius (parser, lowering, runtime)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Library-first: Implement behavior in `src/fifthlang.system/` with clear contracts → PASS
- Generator as source of truth: No manual edits under `src/ast-generated/` → PASS (no generator changes anticipated)
- Grammar integrity: No new tokens/keywords; no grammar edits → PASS
- Test-first: Add/update tests in existing projects; do not hide failures → PASS
- Toolchain discipline: Use .NET 8.0.118, Java 17+; do not cancel builds → PASS

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

**Structure Decision**: Use existing solution layout. Changes localized to `src/fifthlang.system/` (new/updated types and helpers), `src/compiler/*` (TypeRegistry removal, lowering/translator bindings), and tests under `test/*` only where necessary for interop.

---

## Phase 0: Outline & Research

Unknowns to resolve (convert to research tasks):
- dotNetRDF binding choice: subclass vs wrapper vs interface exposure (NEEDS CLARIFICATION → Research best interop pattern for `IGraph`, `Triple`, `IInMemoryQueryableStore`).
- Operator mapping: How to expose `graph += triple` cleanly in C# surface (operator overload vs method `Add`), considering Fifth lowering (NEEDS CLARIFICATION → Evaluate ergonomics + Roslyn translation simplicity).
- Global predeclared type names: Implementation strategy in compiler (prelude table vs special-case binding) while avoiding “primitive registration” (NEEDS CLARIFICATION → Identify hook point in BuiltinInjector or symbol table initialization).

Artifacts to produce:
- `specs/001-system-kg-types/research.md` capturing decisions, rationale, alternatives.

---

## Phase 1: Design & Contracts

Deliverables:
- `data-model.md`: Describe `Fifth.System.Graph/Triple/Store` public surface used by compiler lowering/translator (methods, properties, operators).
- `contracts/`: Add a concise C#-ish contract doc (no OpenAPI needed) outlining method signatures used by compiler (e.g., `Graph.Add(Triple t)`, optional operator `+`/`+=`).
- `quickstart.md`: Minimal examples showing usage in `.5th` staying unchanged; interop snippet calling into C# with `Fifth.System.Graph`.
- Agent context update: `.specify/scripts/bash/update-agent-context.sh copilot`.

Design notes:
- No grammar changes. Lowering should target `Fifth.System` API. Remove primitive registrations and ensure type resolution binds global names to system types.

---

## Phase 2: Implementation Planning (High-Level Tasks)

1) System Types
- Add `Graph`, `Triple`, `Store` to `src/fifthlang.system/` wrapping/subclassing dotNetRDF.
- Provide factory `sparql_store(iri)` returning `Store`.
- Expose `Graph.Add(Triple)` and support `+=` semantics (operator or method).

2) Compiler Bindings
- Remove primitive registrations for `graph/triple/store` from `TypeRegistry` or equivalent.
- Introduce global predeclared type names binding to `Fifth.System` types (prelude symbol injection).
- Update lowering/translator to call `Fifth.System` APIs.

3) Tests & Validation
- Ensure existing `.5th` samples compile unchanged; fix any translator calls.
- Add one interop test asserting type compatibility with dotNetRDF interfaces.
- Validate performance within 5% on KG scenarios.

---

## Phase Outputs

- Phase 0: `research.md`
- Phase 1: `data-model.md`, `contracts/` docs, `quickstart.md`, agent context updated
- Phase 2: Implementation tasks enumerated (handed to `/speckit.tasks`)

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
