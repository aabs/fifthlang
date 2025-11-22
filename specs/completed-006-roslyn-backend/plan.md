
# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
[Extract from feature spec: primary requirement + technical approach from research]
The feature replaces the repository's legacy IL/PEEmitter code-generation pipeline with a Roslyn-driven backend that
translates the Lowered AST into C# syntax trees and uses Microsoft.CodeAnalysis to emit assemblies and Portable PDBs.
Primary goals: preserve developer debugging fidelity (full line/column SequencePoints), achieve test-suite behavioral
equivalence for a curated baseline, and provide a gated, auditable cut-over plan that keeps the legacy emitter available
until the FR-009 deletion gates are satisfied.

## Technical Context
**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  
**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  
**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  
**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  
**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]
**Project Type**: [single/web/mobile - determines source structure]  
**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  
**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  
**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]
## Technical Context
Language/Version: C# targeting C# 14 for generated sources (LangVersion=14 when `UsePinnedRoslyn=true` in CI/release; local devs may use SDK-provided Roslyn and 'preview' language mode).
Primary Dependencies: Microsoft.CodeAnalysis.CSharp (Roslyn) pinned to 4.14.0 for CI/release, System.Reflection.Metadata pinned to 9.0.0, Antlr4.Runtime.Standard (parser), xUnit & FluentAssertions (tests), RazorLight (codegen templates) and existing runtime dependencies referenced by the compiler.
Storage: N/A (compiler/tooling work — no persistent data stores required)
Testing: xUnit-based unit & integration tests; dotnet test harnesses under `test/` (parser tests, ast-tests, runtime-integration-tests, kg-smoke-tests, perf scenarios).
Target Platform: Validate and produce artifacts on .NET 8 (canonical pinned SDK) and .NET 10-rc; CI matrix must run both SDKs.
Project Type: Library-first compiler monorepo (existing layout). New translator and supporting types will be added under `src/compiler/LoweredToRoslyn/` and tests under `test/`.
Performance Goals: Performance validation is deferred during initial CI gating (FR-004). Compile-time and runtime benchmarks will be executed during stabilization and optimization phases; regressions will be documented and addressed post-cutover.
Constraints:
- Preserve developer debugging experience (Portable PDBs with precise SequencePoints mapping back to original `.5th` source).
- Do NOT hand-edit generated code in `src/ast-generated/` (follow Constitution). Regeneration must be used for any metamodel changes.
- Keep `global.json` canonical SDK pinned to .NET 8; CI must additionally validate .NET 10-rc artifacts.
Scale/Scope: Target a full replacement of the downstream IL emission pipeline for all constructs currently emitted by the legacy emitter, with a gated deletion plan for the legacy code paths.

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitutional gates evaluated (quick summary):

- Generator discipline: The plan does not require hand-editing `src/ast-generated/` and preserves the generator-as-source-of-truth rule. PASS
- Canonical SDK: The plan keeps `global.json` pinned to .NET 8 and treats .NET 10-rc as a validation target in CI. PASS
- Build & Test ordering: The plan preserves the required build order and tooling constraints (ast-model → ast_generator → ast-generated → parser → code_generator/translator → compiler → tests). PASS
- Gated deletion: The plan implements FR-009 gated deletion steps (preservation inventory, PDB mapping tests, CI matrix, regeneration proof, constitutional deviation checklist). PASS

Conclusion: No constitution violations detected at the plan level. Any future step that would change the canonical SDK, hand-edit generated files, or remove legacy emitters must follow the documented constitutional-deviation process (see FR-009 and `constitutional-deviation.md`).

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->
```
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

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]
**Structure Decision**: Use the existing repository, library-first layout. Add the Roslyn translator under a new compiler subfolder: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/` for `LoweredAstToRoslynTranslator`, mapping/table types and any small runtime shims. Tests and validation harnesses will live under `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/` (runtime-integration-tests and ast-tests). No large structural refactor expected.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [ ] All NEEDS CLARIFICATION resolved (some operational clarifications are recorded in `clarifications.md` and T027 - owner: @aabs)
- [ ] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
