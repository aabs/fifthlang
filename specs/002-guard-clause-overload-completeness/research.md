# Research: Guarded Overload Completeness Validation

## Objective
Establish minimal, deterministic heuristics to validate guarded function overload completeness, reachability, and structural quality without implementing a full logical solver.

## Inputs Considered
- Feature spec FR-001..070, AC-001..038
- Constitution (multi-pass pipeline, TDD, simplicity, determinism)
- Existing compiler transformation order: must run post symbol/type binding but pre-lowering.

## Alternative Approaches Evaluated
| Approach | Description | Pros | Cons | Decision |
|----------|-------------|------|------|----------|
| Full Boolean SAT / Interval Solver | Translate guards to CNF, solve for coverage/existence of gaps. | Precise, extensible. | Overkill, higher complexity & perf cost, nondeterministic ordering complexity. | Rejected (YAGNI). |
| Pattern Matching Algebra | Introduce ADT/domain algebra and exhaustiveness via enumerated shapes. | Future-friendly for algebraic types. | Language lacks ADTs yet; speculative complexity. | Rejected (premature). |
| Heuristic Conjunctive Normalization (Selected) | Accept only AND-only atomic constraints; treat others UNKNOWN. | Simple, fast, conservative; easy to extend. | Conservative (may under-report coverage). | Accepted. |
| Per-Parameter Domain Tracking | Track partial domains for primitive types and combine via cartesian coverage. | Balanced precision. | Still complex for large numeric domains; numeric infinity issues. | Partially adopted (boolean only exhaustive). |

## Key Design Decisions
1. UNKNOWN classification collapses non-conjunctive or cross-identifier expressions to avoid unsound assumptions.
2. Tautology detection strictly literal/constant true; no symbolic simplification to keep determinism.
3. Numeric intervals: only detect emptiness, duplicate coverage, and subsumption—not completeness.
4. Boolean domain is the only primitive domain considered exhaustible via guards alone (x==true / x==false pairs).
5. Diagnostic precedence table codified to prevent drift; spec parity test required.
6. Object pooling deferred unless allocation share threshold exceeded (>1% or ≥10% reduction possible).

## Open Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Over-conservative UNKNOWN leads to frequent E1001 | User frustration | Add advisory docs; future strict flag can tune. |
| Performance regression on large groups | Slower compile | Early base detection, short-circuit unreachable, pooling if needed. |
| Drift between spec & implementation | Inconsistent diagnostics | Precedence parity test, traceability matrix. |

## Future Enhancements Log
- CNF expansion for limited OR distributions.
- Interval union compaction for integer domains.
- Generic constraint reasoning.
- Pattern shape coverage once ADTs introduced.

## References
- Internal spec `spec.md`
- Constitution `specs/.specify/memory/constitution.md`
