# Feature Specification: Guarded Function Overload Completeness & Destructuring Bonus Fix

**Feature Branch**: `002-guard-clause-overload-completeness`  
**Created**: 2025-09-17  
**Status**: Draft  
**Input**: User description: "There is missing validation logic around overloaded functions using guard clauses and destructuring. It should never be possible to compile a program that has no base case for an overloaded function with guard clauses. We require a language phase in the compiler that will check for incomplete overloading of guard clauses. While solving this we should fix the failing test destructuring_example_ShouldReturn6000." 

---

## Problem Overview (WHAT & WHY)
Guarded function overloading allows multiple function bodies distinguished by guard predicates (arbitrary boolean expressions / pattern matches / destructuring predicates). Guard expressions may contain full boolean logic (&&, ||, !, parenthesised expressions, comparisons, equality tests, calls to pure boolean-returning functions, etc.). Today the compiler apparently accepts sets of guarded overloads that do not include an unconditional ("base") case nor a total set of mutually covering guards. This leads to:
- Runtime selection ambiguity or falling through to undefined behavior
- Silent acceptance of logically partial definitions
- Downstream transformation / IL generation producing incorrect logic (e.g., wrong exit code in `destructuring_example_ShouldReturn6000`)

The failing test indicates that the chosen overload / destructuring path yields an incorrect computed value (expected 6000, got 184). This likely stems from either:
1. Incorrect match ordering / missing final catch-all branch
2. Mis-bound destructured fields causing arithmetic on wrong values
3. Guard evaluation semantics not aligned with intended short‑circuit ordering

We need:
1. A compile-time validation phase ensuring guarded overload sets are complete (i.e., at least one base/unconditional variant OR collectively exhaustive mutually exclusive guards that cover all admissible input patterns for the declared parameter types).
2. Diagnostics that precisely identify missing coverage, ambiguous overlapping guards, or unreachable branches.
3. A fix (or exposure) of the destructuring + guard resolution path such that the test passes once validation is in place.

---

## User Value
Language users gain earlier feedback (compile-time) when defining partial or ambiguous guarded overload sets, reducing latent runtime misbehavior and logical bugs. This elevates safety and predictability of pattern/guard-based APIs.

---

## Scope
IN SCOPE:
- Analysis of each function name + arity group using guards / destructuring.
- Detection of: (a) No base case, (b) Non-exhaustive guard coverage, (c) Overlapping (ambiguous) guard predicates, (d) Unreachable overloads (subsumed by prior broader guards), (e) Missing destructured shape members.
- Emission of structured diagnostics with source spans.
- Integration as a new compiler phase (after AST construction, before type inference finalization or lowering—exact placement to be confirmed to ensure type info for guard expressions is available).
- Minimal correction to destructuring / guard selection so that `destructuring_example.5th` returns 6000.

OUT OF SCOPE (for this feature):
- Full formal proof system for guard exhaustiveness over algebraic data types (not yet defined in language).
- Performance optimizations for guard dispatch (jump tables etc.)—only correctness & validation.
- Introducing new pattern syntax beyond what exists now.

---

## Definitions
- Guarded Overload: A function definition variant distinguished by a boolean guard expression (arbitrary boolean logic allowed) or destructuring pattern preceding its body. A destructuring pattern alone (without an explicit boolean condition) does NOT make the overload guarded for completeness purposes if no guard expression is supplied.
- Base Case: An overload for a function with the same name + arity that has no guard expression. It MAY include a destructuring pattern; absence of an explicit guard expression makes it the catch-all candidate. Exactly one base (unguarded) overload is permitted per group AND it MUST be the final declared overload in the group; any declarations after a base overload are a compile-time error (see diagnostics TBD) or ignored by later phases (preferred: emit error).
- Coverage Set: Union of input domains for which at least one guard (or base) succeeds.
- Exhaustive: Coverage Set equals the full cartesian domain of parameter types.
- Ambiguous Overlaps: Two or more guards may succeed on the same input (absent defined precedence) where semantics require uniqueness.

---

## Functional Requirements
FR-001: The compiler MUST introduce a Guarded Overload Completeness Validation phase.
FR-002: For every function group (same identifier + arity), if any overload uses a guard, the group MUST contain either (a) an unconditional/base overload OR (b) a set of guards proven (by syntactic + heuristic checks) to be exhaustive.
FR-003: If neither condition in FR-002 is met, compilation MUST fail with a diagnostic: "INCOMPLETE_GUARDED_OVERLOAD_SET" referencing the function group and first missing coverage example (heuristic message).
FR-004: Overlapping guards are permissible; runtime dispatch selects the first (declaration order). No overlap error is emitted solely for overlap.
FR-005: The validator MUST flag any guarded overload whose guard is fully subsumed by all prior guards as: "UNREACHABLE_GUARD_OVERLOAD" (preferred over emitting an ambiguity diagnostic). If subsumed, no separate overlap diagnostic is produced.
FR-006: Destructuring patterns MUST NOT require listing all fields of the underlying type; omission of fields is permitted and MUST NOT produce an error. Completeness of destructuring is not validated in this phase.
FR-007: Destructuring bindings MUST map to correct underlying fields; mismatched or unknown member names produce: "UNKNOWN_DESTRUCTURED_MEMBER".
FR-008: Validation MUST occur prior to code generation so no invalid set reaches IL emission.
FR-009: The phase MUST not produce false positives for functions without any guards.
FR-010: The phase MUST allow explicitly intentional partiality only if an `@partial` (future) attribute is present (not implemented now—by default partiality is an error). (Mark for future extension.)
FR-011: Error diagnostics MUST include file path, line/col span for each implicated guard/base overload.
FR-012: On failure, subsequent phases MUST still be able to show aggregated errors (do not abort after first unless catastrophic parse error).
FR-013: Guard evaluation order semantics MUST remain deterministic (likely definition order); validation MUST respect that order when reasoning about reachability.
FR-014: The existing failing test `destructuring_example_ShouldReturn6000` MUST pass after fixes.
FR-015: The validator SHOULD gracefully degrade (emit a generic warning) when it cannot prove exhaustiveness but a base overload exists (no error in that case).
FR-016: If both a base overload and guarded overloads exist, no completeness error; unreachable checks still run.
FR-017: The validator MUST integrate with existing diagnostic reporting infrastructure used by other language phases.
FR-018: Performance: Validation SHOULD be O(n^2) worst-case in number of overloads per group (acceptable given typical small arity counts).
FR-019: Guard expressions referencing unresolved identifiers MUST rely on earlier binding/type phases; if unavailable, emit a deferred diagnostic and skip overlap analysis for that guard.
FR-020: Provide extension points in code (internal well-factored methods) to later plug in richer pattern coverage logic.
FR-021: Guard expressions MAY use arbitrary boolean logic; the validator MUST treat any expression it cannot structurally analyse as UNKNOWN for completeness, while still using declaration order for reachability assessment.
FR-022: Accessing a missing (null) field/property within a guard expression follows normal runtime null reference semantics; the validator MUST NOT attempt to statically force presence.
FR-023: Analysis is purely compile-time and MUST NOT depend on dynamic presence/absence of fields; missing fields at runtime are outside the scope of this phase.
FR-024: Calls invoked inside guard expressions are ASSUMED PURE for analysis simplification; side effects are allowed but the validator treats them as opaque and does not reorder or eliminate them.
FR-025: Primitive domains: boolean is fully enumerable (true/false). Numeric types (integral & floating) are considered infinite for value-based equality guards; a set of discrete numeric value guards is NEVER treated as exhaustive. Numeric ranges (e.g., `<`, `<=`, `>`, `>=`, interval comparisons) MAY participate in subsumption and partial coverage analysis but are not considered fully exhaustive unless a final base overload exists.
FR-030: A guard comprised purely of a numeric range that wholly lies within an earlier numeric range guard is considered subsumed (UNREACHABLE). Subsumption detection for numeric ranges is limited to simple interval comparisons on the same parameter (e.g., `x > 0 && x < 10` within earlier `x > -100 && x < 100`).
FR-031: If a chain of numeric range guards cumulatively covers all numeric values (e.g., `x < 0`, `x == 0`, `x > 0`), this is NOT considered exhaustive without a trailing base overload; coverage still requires explicit base for numeric domains.
FR-026: Prefer emitting only UNREACHABLE_GUARD_OVERLOAD when a later guard is fully covered by earlier guards; do not emit an additional ambiguity/overlap diagnostic for the same case.
FR-027: Diagnostic naming convention MUST adopt prefix GUARD_ with numeric codes (see Diagnostics section) and stable message formats.
FR-028: Destructuring creates a stable binding context for the duration of guard evaluation so repeated field access within the same guard need not re-evaluate or re-destructure.
FR-029: When a diagnostic concerns multiple overloads, emit one primary diagnostic attached to the first implicated overload and secondary notes referencing the others.

---

## Non-Functional Requirements
NFR-001: Added phase MUST not increase cold full-compilation time by >5% for current test suite.
NFR-002: Memory overhead MUST remain linear in total number of guarded overloads encountered.
NFR-003: Diagnostics format MUST be consistent with existing style (severity, code, message).

---

## Proposed Compiler Phase Placement
Order (tentative):
1. Parse → AST
2. (Existing early validations)
3. Type binding / symbol resolution (enough to know parameter types & destructured members)
4. Guarded Overload Completeness Validation (NEW)
5. Remaining transformations / lowering
6. IL generation

Rationale: We need symbol + type info for destructuring correctness and to reason about potential domain coverage. We do not need full type inference finalization if parameter structural shapes are already known.

---

## Validation Algorithm (Heuristic)
For each function group G (name + arity):
1. Collect overload list in declaration order: O1..On.
2. Partition into: Base (no guard) and Guarded (with guard or destructuring predicate requiring condition).
3. If Base present: mark completeness satisfied (skip exhaustive proof); proceed to overlap + unreachable checks.
4. Else (no Base):
   a. Build abstract domain approximation per parameter: if primitive (int/float/string/bool) treat domain as TOP; if enum-like or union (future) capture discrete set; for destructured record/class treat presence of field constraints as narrowing predicate.
   b. For each guard Gi produce a symbolic predicate descriptor (set of equality tests, type tests, field match constraints). If predicate not structurally parsable, mark UNKNOWN and exclude from completeness proof.
   c. Attempt coverage: iterative union of predicate descriptors; if after all guards union != TOP → emit INCOMPLETE_GUARDED_OVERLOAD_SET.
5. Overlap detection: pairwise intersections MAY exist and are permitted; no diagnostic produced solely for overlap. Numeric range overlaps follow same rule unless one interval is fully contained by earlier intervals (then treated in unreachable detection).
6. Unreachable detection: for each Gi, test if union of all earlier predicates completely covers Gi (including containment of numeric intervals) → if yes emit UNREACHABLE_GUARD_OVERLOAD (primary on first earlier overload, secondary notes on the unreachable one).
7. Destructuring integrity: verify referenced members exist and adjust predicate descriptor.
8. Emit diagnostics.

NOTE: Heuristic domain approximation deliberately conservative: if UNKNOWN elements exist and no Base, treat as potentially incomplete (emit error unless all unknowns collectively cover TOP—cannot prove so treat as incomplete). Future enhancement can refine.

---

## Data Structures
- FunctionGroup { Name, Arity, List<Overload> }
- Overload { Parameters, GuardExpression (nullable), DestructuringPattern (nullable), SourceSpan }
- PredicateDescriptor { Kind=Always | Conjunction(set of atomic constraints) | Unknown }
- AtomicConstraint { Type: Equality | FieldMatch | TypeTest, Operand(s) }

---

## Diagnostics (codes & messages)
- GUARD_INCOMPLETE (E1001): "Function '{name}/{arity}' has guarded overloads but no base case and guards are not exhaustive." (+ first uncovered example hint if derivable)
- GUARD_UNREACHABLE (W1002): "Overload #{i} for function '{name}/{arity}' is unreachable (covered by previous guards)." (Primary attached to first earlier overload; secondary note on unreachable overload.)
- GUARD_UNKNOWN_MEMBER (E1003): "Member '{m}' in destructuring for overload #{i} does not exist on type '{T}'."
- (Removed) INCOMPLETE_DESTRUCTURE: No longer emitted; destructuring omissions are permitted.

Severity:
- Errors: GUARD_INCOMPLETE, GUARD_UNKNOWN_MEMBER
- Warnings: GUARD_UNREACHABLE (could be escalated in strict mode future)

---

## Test Plan
Categories:
1. Success scenarios:
   - Single base overload + additional guarded overloads (no errors)
   - Only guarded overloads with provably exhaustive simple domain (e.g., boolean parameter: guard `x == true` and `x == false`)
   - Destructuring with base case fallback
2. Failure / warning scenarios:
   - Guarded overloads with no base and missing branch (GUARD_INCOMPLETE)
   - Unreachable second guard identical to first (GUARD_UNREACHABLE warning)
   - Destructuring referencing nonexistent member (GUARD_UNKNOWN_MEMBER)
   - Guard expression causing runtime null reference (runtime exception; not a validator diagnostic)
3. Real test fix:
   - `destructuring_example.5th` after corrections returns 6000; verify exit code.
4. Regression integration:
   - Ensure unrelated tests still pass (spot-check existing 165 passing tests)
5. Diagnostic formatting correctness.

Test Artifacts:
- New `.5th` test files under `test/runtime-integration-tests/TestPrograms/Functions/Guards/`
- Unit tests for validator (if unit-level project exists) or integration runtime tests.

---

## Risks & Mitigations
- Risk: Overly aggressive completeness errors on complex guards → Mitigation: Use conservative UNKNOWN classification; allow base case to bypass.
- Risk: Performance regression on large guard sets → Mitigation: Early break when base case found; O(n^2) acceptable.
- Risk: Unfixable heuristic false positives → Provide internal escape hatch (not user-facing yet) to disable via compiler option for debugging.

---

## Open Questions / Clarifications
There is currently NO attribute system to mark intentional partiality; any partial guarded overload set without a base case is an error (future enhancement may introduce an `@partial` attribute to relax this).

Marking with [NEEDS CLARIFICATION] if additional answers are required before implementation.

---

## Acceptance Criteria
AC-001: Incomplete guard set program yields GUARD_INCOMPLETE (E1001).
AC-002: Overlapping (but not subsuming) guards compile without diagnostic; runtime picks first matching guard.
AC-003: Fully subsumed guard (including numeric range containment) yields single GUARD_UNREACHABLE (W1002) with secondary note on the later overload.
AC-009: Declaring any overload after the base (unguarded) overload produces an error (code to be defined, e.g., GUARD_BASE_NOT_LAST) or prevents compilation.
AC-010: Discrete numeric value guards alone are never reported as exhaustive; a missing base results in GUARD_INCOMPLETE.
AC-004: Failing test `destructuring_example_ShouldReturn6000` passes (ExitCode 6000) after changes.
AC-005: No new failures introduced in existing passing tests.
AC-006: Compilation time increase <5% (manual measurement acceptable initially).
AC-007: Diagnostics display correct line/column spans.
AC-008: No INCOMPLETE_DESTRUCTURE diagnostic appears anywhere (removed behavior).

---

## Engineering Notes
Implementation will add a visitor over the AST collecting function overload groups. It will construct predicate descriptors from guard expressions (initially parse equality chains of the form `x == literal` and destructuring field existence). The validator will attach diagnostics to a shared DiagnosticSink. Placement after symbol/type binding ensures parameter types are known. The test fix likely involves either introducing a missing base overload or correcting destructuring binding order; root cause to be identified during implementation.

---

## Execution Status
- [ ] Spec reviewed
- [ ] Implementation started
- [ ] Validator integrated
- [ ] Tests added
- [ ] Failing test passes
- [ ] Regression green

