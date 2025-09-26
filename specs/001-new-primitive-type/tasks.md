# Tasks: Feature 001 New Primitive Type `triple`

## Legend
- [ ] = open
- [P] = parallelizable after dependencies
- (R) = requires regeneration (ast generator)
- (B) = benchmark/performance related
- (D) = diagnostics related

## Phase Ordering Overview
1. Grammar & AST tests (fail first)
2. Lexer/Parser changes + AstBuilderVisitor
3. AST node/metamodel adjustments (if needed) + regenerate
4. Transformation passes (expansion, lowering)
5. Diagnostics & type inference updates
6. Mutating operators support (+=, -=)
7. Integration & property-based tests
8. Performance benchmark & validation (≤5% parse delta)
9. Documentation & quickstart validation

---

### 1. Grammar & Lexer
1. [ ] Add `TRIPLE` keyword to `FifthLexer.g4` (ensure reserved) (D)
2. [ ] Add `MINUS_ASSIGN : '-=';` token to lexer (if not present) (grammar search showed only PLUS_ASSIGN) (D)
3. [ ] Integrate `tripleLiteral` production into `FifthParser.g4` under `literal` or operand rule
4. [ ] Ensure lookahead disambiguation between `<{` graph assertion block vs `<s, p, o>` triple literal (unit tests)
5. [ ] Add parser validation for exactly two commas (arity) or rely on rule; confirm TRPL001 emission path (D)
6. [ ] Add negative grammar samples: nested list `<s,p,[[o]]>`; trailing comma `<s,p,o,>`; wrong arity `<s,p>`; `<s,p,o,x>`

### 2. AST & Metamodel
7. [ ] Confirm `TripleLiteralExp` exists in `AstMetamodel.cs`; if missing add with fields Subject, Predicate, Object and regenerate (R)
8. [ ] Add `IsExpanded` flag or metadata if needed; else record via transformation tracking (decide minimal change) (R)
9. [ ] Regenerate AST builders/visitors (`make run-generator`) (R)

### 3. Parser → AST Integration
10. [ ] Update `AstBuilderVisitor.cs` to construct `TripleLiteralExp`
11. [ ] Add tests in `test/ast-tests/` verifying AST shape for sample triple literals
12. [ ] Add test verifying variables allowed in subject/predicate only when IRI typed; produce TRPL002 otherwise (D)

### 4. Transformation Passes
13. [ ] Implement `TripleLiteralExpansionVisitor` (list object → multiple TripleLiteralExp) (no nested lists) (D)
14. [ ] Emit TRPL004 warning for empty list object (D)
15. [ ] Emit TRPL006 error for nested list detection (scan list elements) (D)
16. [ ] Implement `GraphTripleOperatorLoweringVisitor` handling: graph+triple, triple+graph, triple+triple, graph-triple, plus compound assignments
17. [ ] Ensure structural equality reliance delegates to KG helpers (avoid reimplementing)
18. [ ] Add visitor tests for expansion (list size >1, empty list, nested list error)
19. [ ] Add visitor tests for lowering (each operator form)

### 5. KG Helper Enhancements
20. [ ] Review `KnowledgeGraphs.cs` (or equivalent) for existing methods: CreateTriple, Assert, Retract, CopyGraph / union patterns
21. [ ] Add `Retract` if missing
22. [ ] Add safe copy/clone helper if graph mutation semantics require defensive copy
23. [ ] Add unit tests for new KG helpers

### 6. Type Inference & Diagnostics
24. [ ] Map triple literal to primitive type `triple` in type inference generator or manual logic
25. [ ] Ensure list expansion context yields `graph` when folding via operator; verify in type tests
26. [ ] Introduce diagnostic codes TRPL001–TRPL006 in diagnostic subsystem
27. [ ] Add tests for each diagnostic condition

### 7. Mutating Operators (+=, -=)
28. [ ] Extend parser assignment rule to accept MINUS_ASSIGN if absent
29. [ ] Lower `g += <...>` to `g = g + <...>`; same for `g -= <...>`
30. [ ] Tests for mutating forms (including list expansion in RHS)

### 8. Property-Based & Integration Tests
31. [ ] Property-based test: adding duplicate triple does not change graph size
32. [ ] Property-based test: list expansion equivalence `<s,p,[o1,o2]> == (<s,p,o1> + <s,p,o2>)` folded result
33. [ ] Runtime integration test: triple literal inside graph assertion block asserts content (FR-021)
34. [ ] Integration test: nested list rejected with TRPL006
35. [ ] Integration test: performance of parse unaffected by >5% (baseline harness)

### 9. Performance Benchmark (B)
36. [ ] Add benchmark sample file with many triple literals
37. [ ] Run baseline parse timing pre-change, record
38. [ ] Run post-change parse timing; compute delta; assert ≤5%

### 10. Documentation & Quickstart
39. [ ] Validate quickstart examples parse (scripts/validate-examples.fish)
40. [ ] Add new examples to docs syntax samples
41. [ ] Update spec if any discovered edge-case adjustments required

### 11. Cleanup & Review
42. [ ] Constitution re-check: ensure no manual generated edits, tests present first
43. [ ] Ensure all new diagnostics documented
44. [ ] Final plan status update & mark phases complete

## Parallelization Notes
- Grammar negative/positive samples (1–6) can be written in parallel with metamodel confirmation (7).
- Transformation tests (18–19) depend on expansion/lowering code but can scaffold with expected failure earlier.
- Property-based tests (31–32) can be introduced after lowering passes compile.

## Risk Mitigations Mapping
- Ambiguity risk → tasks 3–4–6 (tests)
- Performance risk → tasks 36–38
- Nested list complexity → tasks 13–15–34
- Duplicate semantics correctness → tasks 31–32

## Acceptance Criteria Mapping
| FR/NFR | Tasks |
|--------|-------|
| FR-003/004 | 3,4,6 |
| FR-005 | 10,12 |
| FR-006 | 13,15,18 |
| FR-008–010 | 16,19,31–32 |
| FR-019–021 | 13,16,18,19,33 |
| FR-023 | 3,12,26 |
| NFR-002 | 36–38 |
| Diagnostics TRPL001–TRPL006 | 5,12–15,26–27 |

## Open Follow-Ups (Post-merge)
- Potential future optimization: caching of structural equality checks for large graphs
- Consider adding graph literal syntax synergy in future
