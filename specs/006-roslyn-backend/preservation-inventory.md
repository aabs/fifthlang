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
