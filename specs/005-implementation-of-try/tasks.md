# Tasks: Implementation of try/catch/finally control flow

Feature: Implementation of try/catch/finally control flow
Branch: 005-implementation-of-try
Spec: specs/005-implementation-of-try/spec.md
Plan: specs/005-implementation-of-try/plan.md

## Phase 1 — Setup

- [ ] T001 Ensure SDK pin for IL tests is enforced in CI workflows (use SDK from `global.json`). For example, `.github/workflows/ci.yml` uses `actions/setup-dotnet` honoring `global.json`. Document the exact workflow file and step that enforces this.
- [ ] T002 Add parser sample placeholders under `src/parser/grammar/test_samples/` (directory exists)
- [ ] T003 Add performance macrobench gate placeholder test under `test/perf/` (skeleton; will be wired later)
- [ ] T004 Verify ANTLR jar presence at `src/parser/tools/antlr-4.8-complete.jar`

## Phase 2 — Foundational (blocking prerequisites)

- [ ] T005 Update `src/ast-model/AstMetamodel.cs` to add nodes: `TryStatement`, `CatchClause`, `ThrowExp`, and `FinallyBlock`
- [ ] T006 Run generator to populate `src/ast-generated/*` (no manual edits)
- [ ] T007 [P] Reserve keywords (`try`, `catch`, `finally`, `when`) in `src/parser/grammar/FifthLexer.g4`
- [ ] T008 Define `throwExpression` production in `src/parser/grammar/FifthParser.g4` (no integration yet)

## Phase 3 — [US1] Parsing & AST construction (P1)

Goal: Parse try/catch/finally (with filters) and throw expressions; build correct AST nodes.
Independent test criteria: All new grammar samples parse; AST shape tests pass for TryStatement, CatchClause, FinallyBlock, ThrowExp.

- [ ] T009 [US1] Add `tryStatement` production in `src/parser/grammar/FifthParser.g4` (try/catch/finally with `when` filter)
- [ ] T010 [US1] Add `catchClause` alternatives: `catch {}`, `catch (Type)`, `catch (id: Type)`, optional `when (expr)` in `src/parser/grammar/FifthParser.g4`
- [ ] T011 [US1] Integrate `throwExpression` into null-coalescing and conditional operator rules in `src/parser/grammar/FifthParser.g4`
- [ ] T012 [US1] Implement AST construction in `src/parser/AstBuilderVisitor.cs` for `TryStatement` with ordered `CatchClause` list and optional `Finally`
- [ ] T013 [US1] Implement `CatchClause` population (ExceptionIdentifier before ExceptionType for `id: Type`, etc.) in `src/parser/AstBuilderVisitor.cs`
- [ ] T014 [US1] Implement `ThrowExp` node construction in `src/parser/AstBuilderVisitor.cs` (keep `throw;` as `ThrowStatement`)
- [ ] T015 [P] [US1] Add grammar sample `src/parser/grammar/test_samples/TryCatchFinally_Syntax_01.5th`
- [ ] T016 [P] [US1] Add grammar sample `src/parser/grammar/test_samples/TryCatchFinally_Syntax_02.5th`
- [ ] T017 [P] [US1] Add grammar sample `src/parser/grammar/test_samples/TryCatchFinally_Syntax_03.5th`
- [ ] T018 [P] [US1] Add grammar sample `src/parser/grammar/test_samples/ThrowExpression_Syntax_01.5th`
- [ ] T019 [P] [US1] Add grammar sample `src/parser/grammar/test_samples/ThrowExpression_Syntax_02.5th`
- [ ] T020 [US1] Add parser tests in `test/syntax-parser-tests/` to validate all samples parse
- [ ] T021 [US1] Add AST tests in `test/ast-tests/` to validate shapes for Try/Catch/Finally/ThrowExp
- [ ] T022 [US1] Add AST contract test `NoAdHocAnnotations_Contract` to assert first-class nodes only (no annotation bags)

## Phase 4 — [US2] Semantic analysis & diagnostics (P1)

Goal: Enforce catch type constraints, filter boolean-convertible, unreachable catch as error, throw expression operand type.
Independent test criteria: Semantic tests fail before implementation and pass after; diagnostics messages are specific.

- [ ] T023 [US2] Implement catch type validation (must derive from `System.Exception`) in semantic analyzer (file: `src/compiler/LanguageTransformations/*` or validation pass)
- [ ] T024 [US2] Implement filter expression boolean-convertible check in semantic analyzer
- [ ] T025 [US2] Implement unreachable catch detection (ordering/type shadowing) → error
- [ ] T026 [US2] Implement throw expression operand type check (must be Exception)
- [ ] T027 [US2] Add semantic tests in `test/ast-tests/` or `test/runtime-integration-tests/` (as appropriate): `CatchNonExceptionType_Error`, `FilterMustBeBoolean_Error`, `UnreachableCatch_Error`, `ThrowExpression_OperandType_Error`
 - [ ] T048 [US2] Implement iterator/async-iterator deferral diagnostic for try/catch/finally in iterator methods (per FR-009) and add tests (`Iterator_TryCatchFinally_DeferredDiagnostic`)

## Phase 5 — [US3] IL emission (structural equivalence, non-async) (P2)

Goal: Emit handler regions and correct opcodes matching Roslyn structurally.
Independent test criteria: Structural IL tests pass comparing handler layout and critical opcodes.

- [ ] T028 [US3] Implement try/finally emission in `src/code_generator/PEEmitter.cs` (regions, `leave`, `endfinally`)
- [ ] T029 [US3] Implement catch and filter emission (`catch` vs `filter` metadata) in `src/code_generator/PEEmitter.cs`
- [ ] T030 [US3] Implement throw expression emission (evaluate then `throw`) in `src/code_generator/PEEmitter.cs`
- [ ] T031 [US3] Add IL structural tests under `test/runtime-integration-tests/` for `TryFinally_ILLayout`
- [ ] T032 [US3] Add IL structural tests under `test/runtime-integration-tests/` for `TryCatch_Filter_IL`
- [ ] T033 [US3] Add IL structural tests under `test/runtime-integration-tests/` for `ThrowExpression_IL`

 

## Phase 7 — [US5] Runtime semantics & integration (P2)

Goal: Validate observable behavior matches C# semantics for catch selection, rethrow, finally execution, and throw expressions.
Independent test criteria: Integration tests pass verifying behavior; stack traces preserved for `throw;`.

- [ ] T038 [US5] Add `FinallyAlwaysExecutes` test in `test/runtime-integration-tests/`
- [ ] T039 [US5] Add `RethrowPreservesStackTrace` test in `test/runtime-integration-tests/`
- [ ] T040 [US5] Add `ExceptionMapping_Emission` test in `test/runtime-integration-tests/`
- [ ] T041 [US5] Add `ThrowExpression_Runtime` test in `test/runtime-integration-tests/`

## Phase 8 — Polish & Cross-Cutting

- [ ] T042 Update docs `docs/syntax-samples-readme.md` with try/catch/finally + throw expression examples
- [ ] T043 Update `learn5thInYMinutes.md` with brief examples
- [ ] T044 Ensure `scripts/validate-examples.fish` covers new examples (if needed, add sample paths)
- [ ] T045 Add migration note for reserved keywords in release notes (docs/ or CHANGELOG)
- [ ] T046 Add macrobench gate test wiring and CI documentation in `docs/perf/baselines.md`
- [ ] T047 Ensure regression validation uses the full test suite (`dotnet test fifthlang.sln`) by default in local workflows and CI; keep AST-only as optional fast smoke
 - [ ] T049 [DEFERRED] Add CI job to run macrobench suite and enforce “no measurable regression” (Mann–Whitney U p ≥ 0.05) vs baseline; pin SDK from `global.json` and surface metrics in PR checks
	 - One-liner: Manual macrobench runs are still recommended locally; CI enforcement deferred until perf pipeline is stabilized.

## Dependencies (story completion order)

- US1 (Parsing & AST) → US2 (Semantics) → US3 (IL Structural)
- US5 (Runtime Semantics) depends on US3
- Docs/Polish can begin after US1 (parallel), but finalize after US3

Note: US4 (Async IL Equality) is DEFERRED/blocked until async support exists and is not required for this feature’s completion.

## Parallel execution examples

- Parsing samples (T015–T019) can run in parallel after grammar rules are added
- IL structural tests (T031–T033) can be authored in parallel once T028–T030 are complete
 

## Implementation strategy (MVP first)

- MVP scope: Complete US1 (Parsing & AST) and US2 (Semantics) with tests. This unlocks early validation and internal consumption.
- Next: US3 (IL structural) to enable end-to-end compile/run. Then US5 for runtime semantics validation.
- Finally: Polish.
