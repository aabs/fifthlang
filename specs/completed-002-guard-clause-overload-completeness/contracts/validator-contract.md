# Contract: Guard Overload Validation Phase

## Purpose
Defines the minimal public surface and invariants for the Guarded Overload Completeness Validation phase. Ensures isolation, determinism, and stable diagnostic production.

## Public API (Proposed)
Only one public entry type is permitted (enforced by reflection test):
```
public sealed class GuardOverloadValidationPhase {
  public GuardOverloadValidationPhase(IDiagnosticSink sink);
  public void Run(CompilationUnit rootAst, CancellationToken ct = default);
}
```
- `IDiagnosticSink` is existing compiler infra.
- Returns no value; diagnostics emitted through sink.

Optional (future):
```
public interface IGuardOverloadValidator {
  void Run(CompilationUnit rootAst, CancellationToken ct = default);
}
```
Facade may be introduced only if injection/refactoring needed.

## Invariants
1. Deterministic output: same AST → identical sequence of diagnostics (ordering + messages).
2. No modification of passed AST nodes; analysis is read-only.
3. No reflection over non-compiler assemblies; pure in-memory traversal.
4. Zero allocations of large transient collections beyond pooling policy thresholds.
5. Phase may be executed exactly once per compilation; repeat produces identical diagnostics (idempotent).

## Layering (Enforced)
```
Infrastructure -> Normalization -> Analysis -> Diagnostics -> Phase Entry
```
No inverse dependencies permitted. Internal namespaces mirror folder structure.

## Diagnostic Gating
- E1005 suppresses E1001 completeness emission (but W1101/W1102 still allowed).
- E1004 does not suppress E1001.
- W1102 may co-emit with E1001.

## Extension Policy
- Adding new diagnostic requires precedence table & spec update.
- Adding new UnknownReason requires enum + spec + test update.
- Adding new AtomicConstraintType requires normalization + formatter + tests.

## Testing Obligations
- Reflection boundary test validates only approved public types.
- Traceability test ensures every FR/AC mapped to at least one test.
- Determinism test runs validator twice and hashes ordered diagnostics.

## Performance Obligations
- Median overhead ≤5% vs baseline.
- Allocation share ≤1% or pooling justification committed.

## Instrumentation
Enabled via env var `FIFTH_GUARD_VALIDATION_PROFILE=1`; outputs JSON lines (one per overload group) to stderr; never alters diagnostics.

## Failure Modes
- Catastrophic internal exception surfaces as compiler error (no swallow). Partial diagnostics already produced remain valid.
- Unsupported guard shapes classified UNKNOWN, never throw.

## Security / Safety Considerations
- No untrusted input beyond existing AST; no file I/O; pure analysis.

## Rationale
Surface minimization fosters long-term stability and test predictability while enabling internal refactors without cross-assembly impact.
