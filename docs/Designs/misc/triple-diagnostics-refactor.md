# Deferred Triple Diagnostics Refactor

We temporarily disabled structured TRPL00x diagnostics (TRPL001, TRPL004, TRPL006) and one disambiguation test.

## Rationale
- Left-recursive legacy expression grammar made capturing malformed triple literal shapes brittle.
- Valid and malformed forms currently produce generic `SYNTAX` diagnostics; this is acceptable short-term because malformed shapes do not silently compile.
- Unblocking downstream work (expansion, graph features) took priority.

## Follow-up Plan
1. Introduce precedence-based expression grammar (primary -> postfix -> unary -> power -> mult -> add -> rel -> eq -> and -> or -> assign).
2. Move `tripleLiteral` into `primary` (non-left-recursive) so malformed variants parse as a single node.
3. Ensure unified permissive rule still accepts: missing object, trailing comma, extra components.
4. Re-enable `VisitTripleLiteral` to produce `MalformedTripleExp` and restore `TripleDiagnosticsVisitor` emission of TRPL codes.
5. Re-enable tests:
   - `TripleDiagnosticsTests` (all methods)
   - `DISAMBIG_01_Simple_Triple_Token_Sequence_Contains_Commas`
6. Remove `DISABLE_TRIPLE_DIAGNOSTIC_TESTS` symbol and commented test blocks.

## Risks if Deferred Too Long
- Users get low-quality syntax-only errors for common authoring mistakes.
- Harder to differentiate malformed vs semantic errors in later phases.
- Potential future expansion logic may need to re-implement malformed detection heuristics (duplication).

## Acceptance Criteria
- All previously disabled tests green without altering their assertions.
- No generic `SYNTAX` diagnostics for the malformed triple cases; they carry specific TRPL00x Codes.
- Precedence grammar passes existing non-triple expression tests.

## Owner
Assign in upcoming milestone after current feature branch merges.

---
Tracking file created automatically.
