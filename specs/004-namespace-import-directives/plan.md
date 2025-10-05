# Implementation Plan: Namespace Import Directives# Implementation Plan: Namespace Import Directives



**Branch**: `004-namespace-import-directives` | **Date**: 2025-10-05 | **Spec**: specs/004-namespace-import-directives/spec.md**Branch**: `004-namespace-import-directives` | **Date**: 2025-10-05 | **Spec**: specs/004-namespace-import-directives/spec.md

**Input**: Feature specification from `/specs/004-namespace-import-directives/spec.md`**Input**: Feature specification from `/specs/004-namespace-import-directives/spec.md`



## Execution Flow (/plan command scope)## Execution Flow (/plan command scope)

``````

1. Load feature spec from Input path1. Load feature spec from Input path

   → If not found: ERROR "No feature spec at {path}"   → If not found: ERROR "No feature spec at {path}"

2. Fill Technical Context (scan for NEEDS CLARIFICATION)2. Fill Technical Context (scan for NEEDS CLARIFICATION)

   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)

   → Set Structure Decision based on project type   → Set Structure Decision based on project type

3. Fill the Constitution Check section based on the content of the constitution document.3. Fill the Constitution Check section based on the content of the constitution document.

4. Evaluate Constitution Check section below4. Evaluate Constitution Check section below

   → If violations exist: Document in Complexity Tracking   → If violations exist: Document in Complexity Tracking

   → If no justification possible: ERROR "Simplify approach first"   → If no justification possible: ERROR "Simplify approach first"

   → Update Progress Tracking: Initial Constitution Check   → Update Progress Tracking: Initial Constitution Check

5. Execute Phase 0 → research.md5. Execute Phase 0 → research.md

   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"

6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).

7. Re-evaluate Constitution Check section7. Re-evaluate Constitution Check section

   → If new violations: Refactor design, return to Phase 1   → If new violations: Refactor design, return to Phase 1

   → Update Progress Tracking: Post-Design Constitution Check   → Update Progress Tracking: Post-Design Constitution Check

8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)

9. STOP - Ready for /tasks command9. STOP - Ready for /tasks command

``````



**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:

- Phase 2: /tasks command creates tasks.md- Phase 2: /tasks command creates tasks.md

- Phase 3-4: Implementation execution (manual or via tools)- Phase 3-4: Implementation execution (manual or via tools)



## Summary## Summary

Introduce file-scoped namespace declarations and explicit namespace import directives to the Fifth compiler pipeline. The parser must recognize `namespace` and `import`, the compiler must merge namespace symbol tables across modules, imports must be idempotent and scoped per module, and diagnostics must warn on undeclared imports while honoring local symbol shadowing. Legacy `use` syntax and the `module_import` grammar rule will be removed. The build must keep namespace resolution under 2 seconds for projects with up to 100 modules and emit structured diagnostics containing module and namespace identifiers.Introduce file-scoped namespace declarations and explicit namespace import directives to the Fifth compiler pipeline. The parser must recognize `namespace` and `import`, the compiler must merge namespace symbol tables across modules, imports must be idempotent and scoped per module, and diagnostics must warn on undeclared imports while honoring local symbol shadowing. Legacy `use` syntax and the `module_import` grammar rule will be removed. The build must keep namespace resolution under 2 seconds for projects with up to 100 modules and emit structured diagnostics containing module and namespace identifiers.

## Technical Context

## Technical Context**Language/Version**: C# (.NET 8.0.118 per `global.json`)  

**Language/Version**: C# (.NET 8.0.118 per `global.json`)  **Primary Dependencies**: .NET BCL, Antlr4.Runtime.Standard, MSBuild project enumeration, RazorLight code generator templates  

**Primary Dependencies**: .NET BCL, Antlr4.Runtime.Standard, MSBuild project enumeration, RazorLight code generator templates  **Storage**: N/A (in-memory symbol tables)  

**Storage**: N/A (in-memory symbol tables)  **Testing**: TUnit + FluentAssertions suites (`test/ast-tests`, `test/syntax-parser-tests`, `test/runtime-integration-tests`), `scripts/validate-examples.fish` parser sweep  

**Testing**: TUnit + FluentAssertions suites (`test/ast-tests`, `test/syntax-parser-tests`, `test/runtime-integration-tests`), `scripts/validate-examples.fish` parser sweep  **Target Platform**: Cross-platform .NET CLI (macOS/Linux/Windows)  

**Target Platform**: Cross-platform .NET CLI (macOS/Linux/Windows)  **Project Type**: Single compiler toolchain repository  

**Project Type**: Single compiler toolchain repository  ## Constitution Check

**Performance Goals**: Namespace resolution completes within 2 seconds for projects containing up to 100 modules  *GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constraints**: TDD-first workflow, no manual edits under `src/ast-generated/`, CLI text I/O diagnostics must include file/module context, builds/tests must run unhindered  

**Scale/Scope**: Dozens to low hundreds of `.5th` modules per project with overlapping namespace declarations and cyclic imports**Initial Compliance Notes**

- Uphold Section IV Test-First: expand parser, compiler, and runtime tests before implementation; observe failing state.

## Constitution Check- Respect Section III generator rule: adjust `AstMetamodel.cs` if AST updates are required and regenerate builders via documented commands.

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*- Maintain CLI text diagnostics per Sections II & X with structured stderr output.

- Enforce build discipline (no cancelled `dotnet restore/build/test`, run `scripts/validate-examples.fish`).

**Initial Compliance Notes**- Avoid editing `src/ast-generated/` manually; regenerate after metamodel changes.

- Uphold Section IV Test-First: expand parser, compiler, and runtime tests before implementation; observe failing state.

- Respect Section III generator rule: adjust `AstMetamodel.cs` if AST updates are required and regenerate builders via documented commands.**Status**: Initial Constitution Check – PASS

- Maintain CLI text diagnostics per Sections II & X with structured stderr output.specs/004-namespace-import-directives/

- Enforce build discipline (no cancelled `dotnet restore/build/test`, run `scripts/validate-examples.fish`).```

- Avoid editing `src/ast-generated/` manually; regenerate after metamodel changes.src/

├── ast-model/

**Status**: Initial Constitution Check – PASS├── ast_generator/

├── parser/

## Project Structure│   ├── AstBuilderVisitor.cs

│   └── grammar/

### Documentation (this feature)├── compiler/

```│   ├── ParserManager.cs

specs/004-namespace-import-directives/│   └── LanguageTransformations/

├── plan.md              # This file (/plan command output)└── fifthlang.system/

├── research.md          # Phase 0 output (/plan command)```

├── data-model.md        # Phase 1 output (/plan command)

├── quickstart.md        # Phase 1 output (/plan command)**Structure Decision**: Single-toolchain repository; this feature touches `src/parser/grammar`, `src/parser/AstBuilderVisitor.cs`, `src/compiler/ParserManager.cs`, `src/compiler/LanguageTransformations/`, `src/ast-model/`, and validation suites under `test/` with supporting scripts.

├── contracts/           # Phase 1 output (/plan command)1. **Extract unknowns from Technical Context** above:

└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)   - Confirm MSBuild project enumeration API used by Fifth tooling to gather module list and how namespace metadata will be surfaced.

```   - Determine symbol table aggregation strategy for namespaces spanning multiple modules without duplicating existing scopes.

   - Assess current diagnostic infrastructure for including module/namespace identifiers.

### Source Code (repository root)   - Benchmark current namespace/import handling (legacy `use`) to establish before/after performance measurements aligned with the 2-second SLA.

```

src/2. **Generate and dispatch research agents**:

├── ast-model/   ```

├── ast_generator/   Research MSBuild module enumeration integration for namespace metadata.

├── parser/   Research symbol table union patterns within current compiler passes.

│   ├── AstBuilderVisitor.cs   Research compiler diagnostic formatting for structured namespace/module identifiers.

│   └── grammar/   Research baseline performance tooling to measure namespace resolution timing.

├── compiler/   ```

│   ├── ParserManager.cs1. **Extract entities from feature spec** → `data-model.md`:

│   └── LanguageTransformations/   - Document `Module`, `NamespaceScope`, `ImportDirective`, `SymbolEntry` with attributes, relationships, and shadowing rules.

└── fifthlang.system/   - Capture state transitions for namespace resolution lifecycle (declared → aggregated → imported).



test/2. **Generate API contracts** from functional requirements:

├── syntax-parser-tests/   - Define CLI contract for `fifthc` compiler namespace resolution (arguments, expected diagnostics, exit codes) in `/contracts/namespace-resolution.cli.md`.

├── runtime-integration-tests/   - Specify diagnostic message schema including module/namespace identifiers.

└── ast-tests/

3. **Generate contract tests** from contracts:

scripts/   - Derive failing tests in `test/runtime-integration-tests` for namespace import scenarios and diagnostics before implementation.

└── validate-examples.fish   - Extend parser tests to cover namespace declaration/import syntax acceptance and legacy rejection.

```4. **Extract test scenarios** from user stories:

   - Map cross-namespace symbol resolution, undeclared namespace warning, cyclic imports, and local shadowing scenarios into integration and parser tests.

**Structure Decision**: Single-toolchain repository; this feature touches `src/parser/grammar`, `src/parser/AstBuilderVisitor.cs`, `src/compiler/ParserManager.cs`, `src/compiler/LanguageTransformations/`, `src/ast-model/`, and validation suites under `test/` with supporting scripts.   - Quickstart walks through authoring modules with namespaces, imports, and verifying diagnostics/output.



## Phase 0: Outline & Research**Ordering Strategy**:

1. **Extract unknowns from Technical Context** above:**Phase Status**:

   - Confirm MSBuild project enumeration API used by Fifth tooling to gather module list and how namespace metadata will be surfaced.- [x] Phase 0: Research complete (/plan command)

   - Determine symbol table aggregation strategy for namespaces spanning multiple modules without duplicating existing scopes.- [x] Phase 1: Design complete (/plan command)

   - Assess current diagnostic infrastructure for including module/namespace identifiers.- [x] Initial Constitution Check: PASS

   - Benchmark current namespace/import handling (legacy `use`) to establish before/after performance measurements aligned with the 2-second SLA.- [x] Post-Design Constitution Check: PASS

- [x] All NEEDS CLARIFICATION resolved

2. **Generate and dispatch research agents**:- [x] Complexity deviations documented

   ```

   Research MSBuild module enumeration integration for namespace metadata.# Implementation Plan: [FEATURE]

   Research symbol table union patterns within current compiler passes.

   Research compiler diagnostic formatting for structured namespace/module identifiers.**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

   Research baseline performance tooling to measure namespace resolution timing.**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

   ```

## Execution Flow (/plan command scope)

3. **Consolidate findings** in `research.md` using format:```

   - Decision: [what was chosen]1. Load feature spec from Input path

   - Rationale: [why chosen]   → If not found: ERROR "No feature spec at {path}"

   - Alternatives considered: [what else evaluated]2. Fill Technical Context (scan for NEEDS CLARIFICATION)

   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)

**Output**: research.md with all NEEDS CLARIFICATION resolved   → Set Structure Decision based on project type

3. Fill the Constitution Check section based on the content of the constitution document.

## Phase 1: Design & Contracts4. Evaluate Constitution Check section below

*Prerequisites: research.md complete*   → If violations exist: Document in Complexity Tracking

   → If no justification possible: ERROR "Simplify approach first"

1. **Extract entities from feature spec** → `data-model.md`:   → Update Progress Tracking: Initial Constitution Check

   - Document `Module`, `NamespaceScope`, `ImportDirective`, `SymbolEntry` with attributes, relationships, and shadowing rules.5. Execute Phase 0 → research.md

   - Capture state transitions for namespace resolution lifecycle (declared → aggregated → imported).   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"

6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).

2. **Generate API contracts** from functional requirements:7. Re-evaluate Constitution Check section

   - Define CLI contract for `fifthc` compiler namespace resolution (arguments, expected diagnostics, exit codes) in `/contracts/namespace-resolution.cli.md`.   → If new violations: Refactor design, return to Phase 1

   - Specify diagnostic message schema including module/namespace identifiers.   → Update Progress Tracking: Post-Design Constitution Check

8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)

3. **Generate contract tests** from contracts:9. STOP - Ready for /tasks command

   - Derive failing tests in `test/runtime-integration-tests` for namespace import scenarios and diagnostics before implementation.```

   - Extend parser tests to cover namespace declaration/import syntax acceptance and legacy rejection.

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:

4. **Extract test scenarios** from user stories:- Phase 2: /tasks command creates tasks.md

   - Map cross-namespace symbol resolution, undeclared namespace warning, cyclic imports, and local shadowing scenarios into integration and parser tests.- Phase 3-4: Implementation execution (manual or via tools)

   - Quickstart walks through authoring modules with namespaces, imports, and verifying diagnostics/output.

## Summary

5. **Update agent file incrementally** (O(1) operation):[Extract from feature spec: primary requirement + technical approach from research]

   - Run `.specify/scripts/bash/update-agent-context.sh copilot`

     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.## Technical Context

   - If exists: Add only NEW tech from current plan**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  

   - Preserve manual additions between markers**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  

   - Update recent changes (keep last 3)**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  

   - Keep under 150 lines for token efficiency**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  

   - Output to repository root**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]

**Project Type**: [single/web/mobile - determines source structure]  

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  

## Phase 2: Task Planning Approach**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

*This section describes what the /tasks command will do - DO NOT execute during /plan*

## Constitution Check

**Task Generation Strategy**:*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Load `.specify/templates/tasks-template.md` as base

- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)[Gates determined based on constitution file]

- Each contract → contract test task [P]

- Each entity → model creation task [P] ## Project Structure

- Each user story → integration test task

- Implementation tasks to make tests pass### Documentation (this feature)

```

**Ordering Strategy**:specs/[###-feature]/

- TDD order: Tests before implementation ├── plan.md              # This file (/plan command output)

- Dependency order: Models before services before CLI├── research.md          # Phase 0 output (/plan command)

- Mark [P] for parallel execution (independent files)├── data-model.md        # Phase 1 output (/plan command)

├── quickstart.md        # Phase 1 output (/plan command)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md├── contracts/           # Phase 1 output (/plan command)

└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan```



## Phase 3+: Future Implementation### Source Code (repository root)

*These phases are beyond the scope of the /plan command*<!--

  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout

**Phase 3**: Task execution (/tasks command creates tasks.md)    for this feature. Delete unused options and expand the chosen structure with

**Phase 4**: Implementation (execute tasks.md following constitutional principles)    real paths (e.g., apps/admin, packages/something). The delivered plan must

**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)  not include Option labels.

-->

## Complexity Tracking```

*Fill ONLY if Constitution Check has violations that must be justified*# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)

src/

| Violation | Why Needed | Simpler Alternative Rejected Because |├── models/

|-----------|------------|-------------------------------------|├── services/

| _None_ | _N/A_ | _N/A_ |├── cli/

└── lib/



## Progress Trackingtests/

*This checklist is updated during execution flow*├── contract/

├── integration/

**Phase Status**:└── unit/

- [x] Phase 0: Research complete (/plan command)

- [x] Phase 1: Design complete (/plan command)# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)

- [ ] Phase 2: Task planning complete (/plan command - describe approach only)backend/

- [ ] Phase 3: Tasks generated (/tasks command)├── src/

- [ ] Phase 4: Implementation complete│   ├── models/

- [ ] Phase 5: Validation passed│   ├── services/

│   ├── pages/

**Gate Status**:│   └── services/

- [x] Initial Constitution Check: PASSapi/

- [x] Post-Design Constitution Check: PASS└── [same as backend above]

- [x] All NEEDS CLARIFICATION resolved

- [x] Complexity deviations documentedios/ or android/

└── [platform-specific structure: feature modules, UI flows, platform tests]

---```

*Based on Constitution v2.1.1 - See `/memory/constitution.md`*

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

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
**Output**: research.md with all NEEDS CLARIFICATION resolved

   - Entity name, fields, relationships
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
- [ ] Phase 0: Research complete (/plan command)
- [ ] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [ ] Initial Constitution Check: PASS
- [ ] Post-Design Constitution Check: PASS
- [ ] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
