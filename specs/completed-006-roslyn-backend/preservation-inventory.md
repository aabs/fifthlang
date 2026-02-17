# Preservation Inventory — Roslyn Backend Migration

This inventory is the initial pass required by FR-009. It lists candidate cases where exact IL layout
or emission details may be important to downstream consumers and therefore require explicit
disposition (shim | keep-legacy | convert-test).

Top-level summary (initial pass)

1. Interop / Reflection-sensitive public APIs
   - Representative-Sample-Path: `test/fifth-runtime-tests/Interop/ExampleInteropTest.5th`
   - Reason: Public APIs that may be reflected over at runtime; exact method signatures and custom
     attributes might be relied upon by consumers.
   - Recommended disposition: Survey & convert to behavioral integration tests; consider runtime shim
   - Owner: @aabs
   - Required-Acceptance-Test: Add an integration test that validates reflection-based lookups for the sample API.

2. Custom IL patterns used by hand-crafted tests
   - Representative-Sample-Path: `test/ast-tests/IL-Expectations/*` (ad-hoc)
   - Reason: Tests that assert textual `.il` output or exact PE layout
   - Recommended disposition: Convert to behavior-focused tests where possible; mark a small subset
     as preservation candidates and create narrow shims if necessary.
   - Owner: @aabs
   - Required-Acceptance-Test: Each converted test must pass under both legacy and Roslyn backends.

Next steps

- Expand the inventory by scanning `test/` for `.il` or `IL`-focused tests and produce a per-test
  disposition during T013 (survey). At least one top-priority candidate must have an acceptance test
  or a shim implemented before any deletion PR is merged.
```markdown
# Preservation Inventory — Roslyn Backend Migration

Path: `specs/006-roslyn-backend/preservation-inventory.md`

Purpose: catalog reflection/interop-sensitive members, tests, and other artifacts that require precise IL-preservation or specialized handling. Each entry must be actionable and include an acceptance test or a shim implementation plan.

Format (table):

| ID | SourcePath | Why | Current-IL-Expectation | Representative-Sample-Path | Proposed-Disposition | ShimPath / Note | Required-Acceptance-Test | Owner | Status |
|----|------------|-----|------------------------|---------------------------|----------------------|-----------------|--------------------------|-------|--------|
| PRES-001 | `test/il-sensitive/some_test.5th` | Reflection over generated method tokens | Specific method token sequence expected by downstream consumer | `test/runtime-integration-tests/preservation/PRES-001.cs` | shim | `src/fifthlang.system/shims/Pres001Shim.cs` | `test/runtime-integration-tests/preservation/PRES-001.cs` | @owner | open |

Guidance:
- For each preservation candidate include a Representative-Sample-Path with a runnable test that reproduces the behavior under both backends (legacy and roslyn) whenever possible.
- Dispositions: `shim` (implement runtime helper and emit calls), `keep-legacy` (keep legacy emitter for that narrow case), or `convert-test` (rewrite test to be behavioral rather than IL-textual).
- Top priority candidates must include a passing acceptance test or an implemented shim prior to deletion PR (see FR-009).

```
