# Tasks: System KG Types (Option B Wrapper Implementation)

## Phase 0 – Decision Finalization
- [x] Select interop pattern (Option B wrappers) and record in research/spec.
- [x] Operator mapping strategy: operator-first (`operator +`); no special lowering.
- [x] Decide global binding mechanism: predeclared global type names (no imports/alias table).
- [x] Decide duplicate triple policy: set semantics (adds are idempotent; merges de-duplicate).
- [x] Decide thread safety stance: v1 non-thread-safe; document NFR.

## Phase 1 – Compiler & Binding Changes
- [x] Remove primitive registrations for `graph|triple|store` from TypeRegistry.
- [x] Bind lowercase type names as predeclared global types to `Fifth.System` (`graph/triple/store`).
- [x] Adjust symbol resolution to treat these like predeclared types (no import required).
- [x] Ensure operator overload resolution works for `graph += triple` (verify symbol binding flows through standard operator resolution code path).
- [ ] Ensure Roslyn translator emits references to `Fifth.System` assembly (fully-qualified or using directive).
- [ ] Update `LoweredAstToRoslynTranslator` to emit `Fifth.System` APIs for KG ops; remove primitive-specific code paths.

## Phase 2 – Fifth.System Library Surface
- [x] Implement `Graph` wrapper (Add, Triples, Count, bridges).
- [x] Implement non-mutating binary operators on Graph: `+/-` with (Graph, Graph) & (Graph, Triple) returning new Graph.
- [x] Implement mutating compound operators on Graph: `+= / -=` with (Graph, Graph) & (Graph, Triple).
- [x] Implement `Triple` wrapper with node storage and bridges; implement `public static Graph operator +(Triple, Triple)`; no `Triple - Triple` operator.
- [x] Implement `store += graph` and `store -= graph` mutating operators.
- [x] Implement `sparql_store(iri)` factory.
- [x] Decide & document copy strategy for non-mutating graph ops (deep copy vs structural sharing) and implement accordingly.

## Phase 3 – Tests & Validation
- [x] Interop tests verifying wrapper bridges (`ToVds*`, `FromVds*`).
- [x] Operator matrix tests covering every bullet in FR-011 (type inference + mutability assertions).
- [x] Mutation tests: ensure `+=` / `-=` modify LHS instance identity.
- [x] Non-mutation tests: ensure binary `+` / `-` leave operands unchanged and produce distinct instances.
- [x] Type inference tests: confirm resulting static type matches matrix.
- [x] Test that TypeRegistry no longer lists primitives.
- [ ] Run `scripts/validate-examples.fish` and fix regressions if any.
- [x] Run KG smoke tests and runtime integration tests.
- [ ] Perf comparison (≤5% delta) using existing perf scripts (include operator benchmarks).
- [ ] FQN usage tests: use `Fifth.System.Graph` explicitly in type positions.
- [ ] Name shadowing tests: local identifier named `graph` does not break type resolution; FQN escape works.
- [ ] Translator tests: verify KG ops lower to `Fifth.System` APIs.
- [x] Duplicate semantics tests: adding same triple twice results in single triple; graph merges eliminate duplicates.
- [ ] Negative test: `Triple - Triple` is not available (compile-time diagnostic asserted).
- [ ] Ensure emitted C# does not inject alias/imports for basic programs.

## Phase 4 – Documentation & Cleanup
- [ ] Update `docs/knowledge-graphs.md` to reflect wrapper design.
- [ ] Update any spec references from subclasses to wrappers.
- [ ] Add rationale summary to constitution if required.
- [ ] Remove any obsolete TODO markers linked to primitive implementation.
- [ ] Document thread-safety stance (v1 non-thread-safe) and duplicate policy (set semantics, idempotent adds) in user docs. (Spec updated; user docs pending)
- [x] Update contracts to reflect triple operator semantics and duplicate/thread-safety notes.

## Stretch / Future Considerations (Not in scope for initial delivery)
- [ ] Concurrency wrappers or thread-safe graph variant.
- [ ] Configurable duplicate triple policy.
- [ ] Extended query abstraction beyond raw SPARQL string.

## Gate Criteria Mapping
- SC-001, SC-002 validation commands pass before merging.
- SC-003 grep shows absence of primitive registration code.
- SC-004 perf check run and documented in perf report.
- SC-005 interop test asserts bridging.

