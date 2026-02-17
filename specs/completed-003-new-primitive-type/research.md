# Phase 0 Research: Triple Primitive & Literal

## Decisions
| Topic | Decision | Rationale | Alternatives Considered |
|-------|----------|-----------|--------------------------|
| Subject/Predicate variables | Allow if statically typed IRI | Flexibility, aligns with existing IRI node creation | Restrict to IRIs only (too limiting) |
| Object forms | IRI, primitive literal, expression, list expansion | Expressive single literal syntax; avoids verbose repetition | Only single value (less ergonomic), function-call factory |
| Empty list object | Expands to zero triples + warning | Prevent silent no-op confusion while remaining non-fatal | Error (too strict), silent ignore (opaque) |
| Duplicate handling | Set semantics (suppress duplicates) | RDF triples naturally treated as set members | Multiset (complex equality semantics) |
| Triple equality | Structural (S,P,O value + literal datatype) | Deterministic, matches RDF graph semantics | Reference equality (not stable), string serialization compare (slower) |
| Ordering | Implementation-defined, not observable | Avoid promises that constrain internal structures | Stable insertion order (implies extra bookkeeping) |
| List expansion lowering | Expand to multiple triple nodes before graph ops | Simplifies later passes; reuses existing KG operations | Runtime loop emission in codegen (more complex) |
| Operator lowering target | `KG.CreateTriple` + `KG.Assert` / union builder helpers | Consistent with existing graph assertion lowering | Custom ad-hoc graph combinators |
| Mutating operators (+=, -=) | Provide; lower to KG Assert/Retract or copy+add/remove | Mirrors common usage patterns; ergonomic | Exclude (less user friendly) |
| Performance guard | Ambiguity test, parse benchmark delta <5% | Mitigate grammar regression risk | Ignore (potential future debt) |
| Warning emission for empty list | Compiler diagnostic (non-fatal) | Developer feedback without blocking | Silent skip (confusing) |
| Prefixed names in subject/predicate/object IRIs | Allowed (existing grammar) | Consistency with other IRI sites; no special casing | Restrict to full IRIs (less ergonomic) |
| Nested list expansion | Disallowed (compile error) | Simplifies grammar & transformation; avoids deep flatten cost | Recursive flatten (higher complexity), ignore inner lists (confusing) |
| Implicit prefix resolution | None added (FR-023) | Predictability; leverages existing prefix table only | Auto-resolve or infer (hidden magic) |

## Unresolved (Deferred to Implementation)
- Prefix/alias expansion specifics in triple literal (use existing IRI resolution path)
- Numeric literal precise datatype mapping confirmation (follow existing literal mapping logic)
- Trailing comma explicit parse error test case
- Whitespace/newline tolerance tests for multiline triple literals

## Risks & Mitigations
| Risk | Impact | Mitigation |
|------|--------|-----------|
| Grammar ambiguity with `<` usage | Parser conflicts / backtracking | Introduce distinct entry rule `triple_literal` requiring comma-separated 3-part pattern not starting with `{` |
| Overlapping IRI vs triple literal tokenization | Misclassification | Ensure lexer keeps IRIREF rule precedence; parser distinguishes by presence of commas & absence of closing '>' directly after IRI tokens |
| Performance regression | Slower parsing | Benchmark parser tests pre/post; rollback pattern if >5% |
| Transformation complexity | Hard-to-maintain pass | Isolate into two focused visitors: Expansion & Operator Lowering |
| Duplicate suppression correctness | Incorrect graph state | Structural equality test suite & property-based dedupe tests |
| Empty list unnoticed | Silent logical bug | Warning diagnostic test asserting emission |

## Benchmarks (Planned)
- Baseline: existing syntax-parser-tests total execution time
- Target: Added triple tests increase total time <5%
- Micro: parse 1K triple literals file vs baseline expression file

## Property-Based Testing Focus
- Object literal domain: random mixture of IRIs and numeric/string/boolean primitives
- List expansion: associativity `graph + <s,p,[o1,o2]> == graph + <s,p,o1> + <s,p,o2>`
- Duplicate invariants: adding same triple N times leaves set size constant

## Tooling / Test Additions
- New grammar samples under `src/parser/grammar/test_samples/`
- New runtime integration tests validating operator lowering output & KG behavior
- Diagnostic expectation tests for empty list warning & invalid triple arity

## Go / No-Go Criteria
| Criteria | Pass Condition |
|----------|----------------|
| Grammar compiles | No new ANTLR errors (warnings allowed) |
| Tests (Phase 1 initial) | All added failing tests fail in expected assertions (TDD start) |
| Performance | Parse benchmark within threshold |
| Diagnostics | Empty list triggers warning; invalid forms produce clear errors |

## Summary
Research confirms approach is localized (lexer+parser+2 passes+tests) with minimal risk to existing systems. Ready to proceed to Phase 1 design artifacts.
