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
- Base Case: An overload for a function with the same name + arity that either (a) has no guard expression or (b) has a guard expression that is a provable tautology. A tautology for this version is strictly one of: the literal token `true`, a parenthesized `true`, or a constant-folded compile-time boolean constant evaluated to true with no side effects. (No multi-step algebraic simplification is attempted.) It MAY include a destructuring pattern. Exactly one base overload (unguarded or tautology-guard) is permitted per group AND it MUST be the final declared base-like overload; any declarations after the first base-like overload are subject to diagnostics (see FR-053, FR-055+). Tautology base and unguarded base are treated identically for ordering and completeness.
- Coverage Set: Union of input domains for which at least one guard (or base) succeeds.
- Exhaustive: Coverage Set equals the full cartesian domain of parameter types.
- Ambiguous Overlaps: Two or more guards may succeed on the same input (absent defined precedence) where semantics require uniqueness.
- Overload Grouping: Overloads are grouped strictly by (function name, ordered list of parameter types). Parameter identifiers (names) are irrelevant for grouping; two overloads whose parameter name differs but whose parameter type sequence matches are part of the same group. Default values, attributes, or parameter names DO NOT distinguish overload groups.

---

## Functional Requirements
FR-001: The compiler MUST introduce a Guarded Overload Completeness Validation phase.
FR-002: For every function group (same identifier + arity), if any overload uses a guard, the group MUST contain either (a) an unconditional/base overload OR (b) a set of guards proven (by syntactic + heuristic checks) to be exhaustive.
FR-003: If neither condition in FR-002 is met, compilation MUST fail with a diagnostic: "INCOMPLETE_GUARDED_OVERLOAD_SET" referencing the function group and first missing coverage example (heuristic message).
FR-004: Overlapping guards are permissible; runtime dispatch selects the first (declaration order). No overlap error is emitted solely for overlap.
FR-005: The validator MUST flag any guarded overload whose guard is fully subsumed by all prior guards as: "UNREACHABLE_GUARD_OVERLOAD" (preferred over emitting an ambiguity diagnostic). If subsumed, no separate overlap diagnostic is produced.
FR-006: Destructuring patterns MUST NOT require listing all fields of the underlying type; omission of fields is permitted and MUST NOT produce an error. Completeness of destructuring is not validated in this phase.
FR-007: Destructuring bindings MUST map to correct underlying fields; mismatched or unknown member names are assumed already rejected by prior symbol/type resolution. This validator MUST NOT emit a separate unknown-member diagnostic.
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
FR-019: Guard expressions referencing unresolved identifiers SHOULD NOT occur (earlier phases guarantee resolution). If encountered exceptionally, classify the guard as UNKNOWN with no new diagnostic.
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
FR-032: Reserve a future compiler flag (e.g., `--strict-guards`) that escalates GUARD_UNREACHABLE (W1002) to an error and optionally treats UNKNOWN analysis cases as errors. The current implementation MUST NOT yet implement the flag but MUST structure code to allow this escalation with minimal change.
FR-033: Overload grouping MUST be determined solely by function identifier plus the ordered sequence of parameter types (arity + types). Parameter names, default values, or annotations MUST NOT create separate groups; guards across such syntactically different but type-equivalent signatures are validated together.
FR-034: Emitting more than one unguarded (base) overload in a single overload group MUST produce error GUARD_MULTIPLE_BASE (E1005).
FR-035: Declaring any further overload after the first base-like (unguarded or tautology) overload MUST produce error GUARD_BASE_NOT_LAST (E1004) and those subsequent overloads MUST NOT participate in dispatch. Dispatch evaluation for a group HALTS logically at the first base-like overload (later base-like overloads are never considered for execution).
FR-036: Secondary diagnostics MUST follow a consistent wording template: "note: <reason> due to <primary overload ref>" where the reference includes function name, arity, and source span (file:line:col). Each secondary attaches to the related overload's signature span.
FR-037: All diagnostics MUST provide dual highlighting: (a) primary location highlighting the guard expression (or entire signature if no guard) and (b) related location(s) for every other overload participating in the diagnostic (case (c) combined style). Related spans use secondary notes per the format section.
FR-038: A guard is classified ANALYZABLE only if it can be normalized to a conjunction (AND-only) of atomic predicates, each atomic predicate matching one of: (a) equality `Ident == Literal`, (b) unary comparison `Ident <|<=|>|>= Literal`, (c) bounded interval `Literal <|<= Ident <|<= Literal` or `Ident <|<= Literal && Ident >|>= Literal` forms on the same identifier, (d) recognized destructuring field binding with no additional boolean operators, (e) a single identifier presence check produced by destructuring. All other forms are UNKNOWN.
FR-039: UNKNOWN guards MUST: (a) still participate in order for reachability (only base/Always can render them unreachable directly), (b) never contribute positive coverage for completeness, (c) be candidates for escalation under `--strict-guards` (future) converting GUARD_INCOMPLETE from absence of analyzable coverage into an immediate error explanation tagging the first UNKNOWN guard.
FR-040: Mixed inclusive/exclusive numeric interval bounds that invert (produce an empty set) MUST collapse to EMPTY and render that overload unreachable at definition time (eligible for GUARD_UNREACHABLE if referenced) e.g., `x > 5 && x <= 5`.
FR-041: Equality predicates MUST normalize to degenerate closed intervals `[v,v]` for interval reasoning and subsumption.
FR-042: Multiple interval predicates for the same identifier in one normalized conjunction MUST intersect cumulatively; EMPTY intersection yields an unreachable overload (GUARD_UNREACHABLE) without affecting completeness positively.
FR-043: Cross-identifier relational coupling (e.g., `x < y`, `a == b + 1`, `x > y && x < z`) MUST classify the entire guard as UNKNOWN.
FR-044: Destructuring in guards MUST be considered side-effect free for static analysis even if it could throw at runtime due to shape mismatch; potential runtime exceptions MUST NOT influence completeness reasoning.
FR-045: Overload ordering across compilation units MUST follow global parsing order (file processing order determined by the compiler) and within each file lexical order.
FR-046: Generic type parameters in guard predicates MUST be treated as TOP domain (no interval narrowing or value enumeration permitted).
FR-047: Diagnostic codes E1001–E1005 are STABLE; their meanings MUST remain backward compatible across versions.
FR-048: Reserve a future contiguous diagnostic range for `@partial` related semantics (codes not yet assigned; do not allocate in this spec).
FR-049: Diagnostic English message templates in this spec are FINAL; no localization or structural token changes in this version.
FR-050: Emit advisory warning W1101 (GUARD_OVERLOAD_COUNT) when an overload group contains more than 32 overloads (threshold T=32; triggers at size >=33).
FR-051: Emit advisory warning W1102 (GUARD_UNKNOWN_EXPLOSION) when group size >=8 and >50% of overloads are UNKNOWN-classified.
FR-052: A base overload whose guard expression is a tautology (`true`, or reducible to always true) MUST be treated as an unguarded base for ordering/completeness.
FR-053: Conflict resolution priority: report GUARD_MULTIPLE_BASE (E1005) before GUARD_BASE_NOT_LAST (E1004); only emit E1004 for overloads trailing after the FIRST recognized base when no additional base conflicts supersede.
FR-054: The validator MUST NOT emit any GUARD_UNKNOWN_MEMBER diagnostic; unknown members should have been resolved earlier; absence indicates prior phase failure, not a validator concern.
FR-055: Tautology detection is limited to the literal `true`, parenthesized `true`, or a compile-time constant boolean symbol resolved to true; no deeper expression simplification (e.g., `!(false)`) is performed.
FR-056: Duplicate guard expressions (after normalization that strips ASCII whitespace and redundant outer parentheses) MUST mark the later overload as unreachable (GUARD_UNREACHABLE) independent of interval logic. Identifier case sensitivity follows language identifier rules; no semantic simplification beyond parenthesis stripping is applied.
FR-057: Cross-identifier equality (`x == y`) MUST classify the guard as UNKNOWN (aligning with FR-043) even without arithmetic offset.
FR-058: Equality or comparison involving a generic type parameter (e.g., `t == 5`, `t > 0`) MUST classify the guard as UNKNOWN (FR-046 extension) unless the generic is already concretely bound to a fully enumerated domain (future capability, not in this version).
FR-059: Interval representation MUST track inclusivity flags explicitly: Interval {lower?, lowerInclusive, upper?, upperInclusive}. An interval is EMPTY if (a) lower > upper, or (b) lower == upper and not (lowerInclusive && upperInclusive).
FR-060: Empty (inverted) interval detection executes before general subsumption; such guards are immediately treated as unreachable without contributing to coverage or later altering union state.
FR-061: Overload count warning (W1101) MUST emit exactly once per overload group, attached to the first overload's signature span.
FR-062: UNKNOWN explosion warning (W1102) MUST emit exactly once per group, attached to the first overload's signature span, and only if no base overload (unguarded or tautology) is present.
FR-063: UNKNOWN explosion percentage computation: unknownPercent = floor((unknownCount * 100.0) / totalCount); trigger when unknownPercent > 50 (strict greater than 50 as integer) AND totalCount >= 8.
FR-064: Presence of a base overload (unguarded or tautology) suppresses computation and emission of GUARD_UNKNOWN_EXPLOSION entirely; explosion analysis only executes for groups with NO base-like overload.
FR-065: Complexity target refined: overall algorithm MUST operate within O(n^2 + n*k) where n = overload count and k = average atomic predicates per analyzable guard; normalization per guard MUST be O(k).
FR-066: Multiple base-like overloads (unguarded or tautology) produce a single primary GUARD_MULTIPLE_BASE diagnostic on the first duplicate; additional duplicates receive only secondary notes and MUST NOT trigger GUARD_BASE_NOT_LAST (E1004) separately.
FR-067: If conditions for both GUARD_INCOMPLETE (E1001) and GUARD_UNKNOWN_EXPLOSION (W1102) are satisfied (no base, >50% UNKNOWN, size >=8), BOTH diagnostics MUST be emitted (E1001 as primary for completeness failure, W1102 as advisory) unless a base-like overload is later introduced (which would negate E1001 beforehand).
FR-068: GUARD_OVERLOAD_COUNT (W1101) emission is independent of base presence; it MAY appear alongside any other diagnostics including E1001, E1004, E1005, or W1102.

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
- GUARD_INCOMPLETE (E1001): "Function '{name}/{arity}' has guarded overloads but no base case and guards are not exhaustive." (+ uncovered example hint if derivable)
- GUARD_UNREACHABLE (W1002): "Overload #{i} for function '{name}/{arity}' is unreachable (covered by previous guards)."
- GUARD_BASE_NOT_LAST (E1004): "Base (unguarded) overload for function '{name}/{arity}' must be the final overload; subsequent overload at #{i} is invalid."
- GUARD_MULTIPLE_BASE (E1005): "Multiple unguarded base overloads detected for function '{name}/{arity}'. Only one final base overload is permitted."
- GUARD_OVERLOAD_COUNT (W1101): "Overload group for '{name}/{arity}' exceeds recommended maximum of 32 (found {count}). Analysis precision may degrade."
- GUARD_UNKNOWN_EXPLOSION (W1102): "Overload group for '{name}/{arity}' has excessive UNKNOWN guards ({unknownPercent}%). Refactor or add base case for clarity."
- (Removed) INCOMPLETE_DESTRUCTURE: No longer emitted; destructuring omissions are permitted.
- (Removed) GUARD_UNKNOWN_MEMBER (former E1003) per FR-054.

Severity:
- Errors: GUARD_INCOMPLETE, GUARD_BASE_NOT_LAST, GUARD_MULTIPLE_BASE
- Warnings: GUARD_UNREACHABLE, GUARD_OVERLOAD_COUNT, GUARD_UNKNOWN_EXPLOSION (all eligible for escalation in future strict mode except count warning)

### Secondary Diagnostics Format
Primary diagnostics list the core issue. Each related overload gets a secondary note entry using these templates (with single-emission rules FR-061/062/066):
 - Unreachable: "note: overload #{i} unreachable due to earlier coverage by overload #{j} at {file}:{line}:{col}"
 - Multiple base: "note: extra base overload at {file}:{line}:{col} ignored; base already declared at {baseFile}:{baseLine}:{baseCol}"
 - Base not last: "note: overload #{i} invalid because base overload terminates overloading at #{baseIndex}"
Placeholders {file}:{line}:{col} refer to 1-based coordinates of the overload signature token. Tooling consuming diagnostics can rely on a machine-readable structure (IDs E1001-E1005, primary=true/false flag) in the future, but textual form MUST match templates.

### Source Span Policy
Each emitted diagnostic includes:
 - Primary span: exact text range of the guard expression; if unguarded base overload, the span is the function signature identifier token range.
 - Related spans: one per implicated overload (unreachable, duplicate base, trailing after base). These are attached as secondary diagnostics (notes) with the templates above.
This enforces option (c): highlight both primary and related locations for clarity across tools.

### UNKNOWN Classification Rules
Normalization pipeline:
1. Parse guard expression AST.
2. Distribute AND over parenthesized sub-expressions; abort normalization if any OR (||) or XOR-like construct appears → classify UNKNOWN.
3. Inspect each conjunct:
   - If matches atomic equality or comparison pattern on a single identifier vs literal → keep.
   - If forms an interval via two comparisons on the same identifier with compatible bounds → merge to interval descriptor.
   - If is a destructuring-produced presence predicate (implicit) → accept.
   - Else classify WHOLE guard UNKNOWN (no partial mixing).
4. If all conjuncts valid → guard ANALYZABLE; build PredicateDescriptor.
5. If any step fails → mark UNKNOWN; predicate contributes zero to coverage union but remains for ordering.

Rationale: Prevents unsound assumptions from complex boolean expressions while enabling precise handling of simple, common predicates.

---

## Test Plan
Categories:
1. Success scenarios:
   - Single base overload + additional guarded overloads (no errors)
   - Only guarded overloads with provably exhaustive simple domain (e.g., boolean parameter: guard `x == true` and `x == false`)
   - Destructuring with base case fallback
2. Failure / warning scenarios:
   - Guarded overloads with no base and missing branch (GUARD_INCOMPLETE)
   - Unreachable guard via prior coverage or empty/inverted interval (GUARD_UNREACHABLE)
   - Interval inversion / empty intersection (GUARD_UNREACHABLE)
   - Excessive overload count (>=33) (GUARD_OVERLOAD_COUNT)
   - UNKNOWN explosion (>50% UNKNOWN, size >=8) (GUARD_UNKNOWN_EXPLOSION)
   - Base not last (GUARD_BASE_NOT_LAST)
   - Multiple bases (GUARD_MULTIPLE_BASE with priority over NOT_LAST)
   - Tautology guard recognized as base equivalence
   - Cross-identifier relational predicate classified UNKNOWN
   - Generic parameter predicates remain TOP (no false subsumption)
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
AC-011: Multiple unguarded overloads for the same function produce GUARD_MULTIPLE_BASE (E1005).
AC-004: Failing test `destructuring_example_ShouldReturn6000` passes (ExitCode 6000) after changes.
AC-005: No new failures introduced in existing passing tests.
AC-006: Compilation time increase <5% (manual measurement acceptable initially).
AC-007: Diagnostics display correct line/column spans.
AC-008: No INCOMPLETE_DESTRUCTURE diagnostic appears anywhere (removed behavior).
AC-012: Diagnostics show dual highlighting (primary + all related spans) consistent with Source Span Policy.
AC-013: Guards containing OR, function calls, arithmetic beyond simple literal comparisons, cross-parameter comparisons, or mixed identifiers in one conjunct are classified UNKNOWN per FR-038/039.
AC-014: Inverted/mixed interval (e.g., `x > 5 && x <= 5`) collapses to EMPTY and yields GUARD_UNREACHABLE.
AC-015: Equality guard normalized to `[v,v]` participates in subsumption decisions.
AC-016: Multiple interval constraints intersect; empty intersection => GUARD_UNREACHABLE.
AC-017: Cross-identifier relational guard classified UNKNOWN and counts toward UNKNOWN percentage.
AC-018: Destructuring potential runtime throw does not alter completeness outcome.
AC-019: Generic parameter predicate never considered exhaustive nor narrowed; no incorrect subsumption warnings introduced.
AC-020: Group of 33 overloads produces single GUARD_OVERLOAD_COUNT warning.
AC-021: Group size 12 with 7 UNKNOWN (58%) produces GUARD_UNKNOWN_EXPLOSION; size 12 with 6 UNKNOWN (50%) does not.
AC-022: Tautology guard recognized as base; completeness satisfied provided ordering rules met.
AC-023: Multiple base overloads produce GUARD_MULTIPLE_BASE; only overloads after first base (when unique) may get GUARD_BASE_NOT_LAST.
AC-024: No GUARD_UNKNOWN_MEMBER diagnostic appears anywhere.
AC-025: Identical duplicate guard expression causes later overload to receive GUARD_UNREACHABLE.
AC-026: GUARD_OVERLOAD_COUNT emitted once per group (no duplication across later overloads).
AC-027: Presence of base (unguarded or tautology) suppresses GUARD_UNKNOWN_EXPLOSION even if >50% UNKNOWN.
AC-028: Guard `x == y` (same parameter types) classified UNKNOWN.
AC-029: Guard involving generic `T` equality or comparison classified UNKNOWN.
AC-030: One unguarded base plus one tautology base triggers single GUARD_MULTIPLE_BASE; no separate GUARD_BASE_NOT_LAST for tautology if already covered by multiple-base diagnostic.
AC-031: UNKNOWN explosion percentage strictly greater than 50% (not >=) required to emit W1102.
AC-032: Group with a base and 33 total overloads emits GUARD_OVERLOAD_COUNT (W1101) once (no suppression by base).
AC-033: Group with no base, 40 overloads, 60% UNKNOWN emits both GUARD_INCOMPLETE (E1001) and GUARD_UNKNOWN_EXPLOSION (W1102).
AC-034: Group with base and all other guards UNKNOWN emits neither GUARD_INCOMPLETE nor GUARD_UNKNOWN_EXPLOSION.
AC-035: Normalized duplicate `( ( x == 5 ) )` followed later by `x==5` triggers GUARD_UNREACHABLE on the later overload.

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

