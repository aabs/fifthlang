# Lowering Contract: Constructor Functions

**Goal**: Convert high-level `new Class(args)` with resolved constructor into explicit allocation + initialization + validation sequence.

## Input
- InstantiationExpression with `ResolvedConstructor` non-null.
- ConstructorDef (may be synthesized) including Parameters, BaseCall, Body.
- RequiredFieldSet (analysis artifact).

## Output
Lowered statement sequence:
1. Allocation temp: `tmp = alloc(ClassType)` (internal representation).
2. Inline defaults: For each field with initializer → emit assignment.
3. Base call (if BaseCall exists): `invoke baseCtor(tmp, baseArgs...)`.
4. User body statements in order.
5. Definite assignment check (generated validation statement or omitted if satisfied).
6. Replace original expression with `tmp` reference.

## Rewrite API
`ConstructorLoweringRewriter : DefaultAstRewriter`
- `VisitInstantiationExpression`:
  - If unresolved → leave (error already recorded).
  - Else produce prologue statements (steps 1–5) and final expression `tmp`.

## Validation Insertion
- If RequiredFieldSet missing non-empty after body → Emit CTOR003 diagnostic prior to finalization; lowering still produces sequence (object may be considered invalid; compile fails).

## Ordering Guarantees
- Inline defaults precede base call.
- Base call precedes body.
- Assignment validation follows body.

## Non-Goals
- Optimization (e.g., skipping temp variable) in this phase.
- Inlining trivial constructors—deferred until performance tuning.

## Future Extension Points
- Statement hoisting for constructor chaining (`this(...)`).
- Instrumentation hooks around allocation for profiling.
