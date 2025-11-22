
# Implementation Plan: New Primitive Type `triple`

**Branch**: `001-new-primitive-type` | **Date**: 2025-09-26 | **Spec**: `specs/001-new-primitive-type/spec.md`
**Input**: Feature specification from `/specs/001-new-primitive-type/spec.md`

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
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
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
Add a new primitive type `triple` with literal syntax `<subject, predicate, object>` mapping directly to `VDS.RDF.Triple`, supporting composition with existing `graph` type via `+`/`-` (immutability, structural set semantics). Literal supports: IRIs (full or prefixed), variables of IRI type in subject/predicate, object forms (IRI, primitive literal, expression resolving to those, single-level list expansion). Nested lists disallowed. Triple literals can appear as standalone statements inside graph assertion blocks, asserting their triple(s). No new implicit prefix resolution. Performance budget: ≤5% average parse time regression with variance guard (mean ≤5% AND mean ≤ 2σ). Canonical serialization adds escape handling for `>` and `,` in string object components.

## Technical Context
**Language/Version**: C# / .NET 8.0 (pinned 8.0.118)
**Primary Dependencies**: ANTLR 4.8 runtime, `VDS.RDF` (triple backing), existing Fifth KG helpers
**Storage**: N/A (in-memory AST & graphs)
**Testing**: xUnit + FluentAssertions (ast-tests, syntax-parser-tests, runtime-integration-tests)
**Target Platform**: Cross-platform .NET 8 CLI
**Project Type**: Single multi-project compiler toolchain (.NET solution)
**Performance Goals**: ≤5% average parse time regression (mean ≤5% and ≤2σ) per NFR-002; no measurable impact on non-triple files beyond threshold
**Constraints**: Immutability of graphs under `+`/`-`; no implicit prefix resolution (FR-023); ordering implementation-defined; nested list expansion prohibited
**Scale/Scope**: Feature localized to lexer, parser, AST visitor, transformations, type inference adjustments, tests

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Notes |
|------------------|--------|-------|
| Library-First | PASS | Reuses existing parser/ast-model; no new library needed |
| Generator-as-Source-of-Truth | PASS | No manual edits to `ast-generated/`; potential metamodel update if new AST node required for triple literal (if not reusing existing expression node) |
| Test-First | PENDING | Will add parser + transformation + integration tests before implementation |
| Multi-Pass Separation | PASS | Triple literal lowering handled in dedicated transformation (list expansion + graph operator lowering) |
| Parser Integrity | PASS | Adds new literal production; ambiguity with `<{` resolved by lexical lookahead: `<{` token sequence routed to graph assertion; `<` followed by IRI/prefixedName/computed expr plus two commas and closing `>` recognized as triple literal via gated predicate (comma-count + no `{`); fallback remains IRIREF otherwise. |
| Observability/Diagnostics | PENDING | New diagnostic codes to be added (TRPL00x) |
| Performance Budget | PENDING | Benchmark diff to enforce ≤5% mean parse time regression with variance guard (≤2σ) |
| No Implicit Prefix Policy | PASS | Explicit in FR-023 |
| Immutability Enforcement | PASS | Graph operations produce new instances |

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

**Structure Decision**: Utilize existing compiler structure; changes confined to:
```
src/parser/grammar/FifthLexer.g4 (add keyword)
src/parser/grammar/FifthParser.g4 (add tripleLiteral rule)
src/parser/AstBuilderVisitor.cs (construct TripleLiteralExp)
src/ast-model/AstMetamodel.cs (add TripleLiteralExp node if absent)
src/compiler/LanguageTransformations/TripleLiteralExpansionVisitor.cs (new)
src/compiler/LanguageTransformations/GraphTripleOperatorLoweringVisitor.cs (new or extend existing lowering pass)
test/syntax-parser-tests/* (new samples)
test/ast-tests/* (AST builder tests)
test/runtime-integration-tests/* (graph composition + assertion block tests)
docs/quickstart snippets (triple examples)
```

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

**Output**: research.md with decisions: prefixed name support, structural equality, set semantics, list expansion (single-level only), performance threshold, no implicit prefix resolution.

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate language contracts** (replace web API notion):
   - Grammar contract: production rules, precedence, ambiguity notes
   - Transformation contract: lowering steps (list expansion, graph op desugaring)
   - Type inference contract adjustments
   - Diagnostic contract: TRPL001..TRPL00N definitions

3. **Generate contract tests**:
   - Parser samples (valid & invalid forms: nested list invalid, trailing comma invalid, ambiguous `<{` separation) → expect parse or error
   - AST shape tests: TripleLiteralExp nodes with correct children
   - Transformation tests: list expansion & operator lowering yields expected intermediate graph/triple constructs

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally**: Run `.specify/scripts/bash/update-agent-context.sh copilot` after adding new transformation/grammar contracts.

**Output**: data-model.md (TripleLiteralExp), /contracts/grammar-contract.md, /contracts/transformation-contract.md, /contracts/diagnostics.md, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Derive tasks from: grammar contract (lexer/parser changes), data model (AST node generation), transformation contract (new visitors), diagnostics contract (error/warning codes), performance target (benchmark update), quickstart examples (integration tests).
- Tag parallelizable tasks: grammar tests, transformation tests, diagnostics tests can proceed once node exists.

**Ordering Strategy**:
- TDD: Add failing grammar & AST tests first
- Then implement lexer + parser + visitor
- Add failing transformation tests → implement expansion & lowering
- Add diagnostics tests → implement error codes
- Add integration/runtime tests (graph ops, assertion block)
- Performance benchmark update last (validate ≤5%)

**Estimated Output**: 30-35 numbered tasks (granularity for grammar, transformations, diagnostics, performance verification)

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
