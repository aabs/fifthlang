# Research: Implementation of try/catch/finally in Fifth

## Decisions

- Iterator scope (v1): Defer iterators entirely
  - Rationale: Avoid state-machine complexity and ordering edge cases now; reduces risk and schedule impact
  - Alternatives: Allow try/finally only in iterators (partial support); full support now (high complexity)

- IL baseline pinning: Pin to SDK in global.json
  - Rationale: Stable, reproducible IL comparisons aligned with repo policy; update with SDK pin only
  - Alternatives: Hard-freeze Roslyn/SDK (slows upgrades); float latest (churn, flakiness)

- Performance budget: No measurable regression on macrobenchmarks
  - Rationale: Encourages real-world performance focus; statistically significant test protects users
  - Alternatives: ≤1/3/5% micro targets (hard to sustain across handler metadata changes)

- Throw expressions: Include in v1 (match C# contexts)
  - Rationale: Common modern idioms; integrates cleanly with semantics and IL
  - Alternatives: Defer; allow only in specific contexts

- Unreachable catch severity: Error (compile-time)
  - Rationale: Matches C#; prevents dead code and ordering bugs
  - Alternatives: Warning/Info/No diagnostic

- AST modeling policy: First-class nodes and typed properties; annotations only for marginal non-semantic metadata
  - Rationale: Ensures generator-as-source-of-truth, schema clarity, and testability
  - Alternatives: Ad-hoc annotations (risk inconsistency, weaker contracts)

## Key References
- Spec: specs/005-implementation-of-try/spec.md
- Constitution: .specify/memory/constitution.md

## Open Items
- Observability/metrics: N/A for language semantics; diagnostics covered by spec
- Scalability: Not directly applicable (compiler feature)
- Documentation updates: learn5thInYMinutes.md, docs/syntax-samples-readme.md
