```markdown
# Constitutional Deviation Checklist — Roslyn Backend Removal

Path: `specs/006-roslyn-backend/constitutional-deviation.md`

Purpose
-------
This checklist documents the preconditions and approvals required before merging any Pull Request that deletes the legacy IL emitter code (AstToIlTransformationVisitor, ILEmissionVisitor, PEEmitter, and closely coupled IL metamodel artifacts). It formalizes the FR-009 gating steps and provides a single place for approvals and evidence references.

How to use
----------
1. For any PR that proposes deletion of legacy emitter files, update this checklist with links to evidence (tests, CI artifacts, generator logs) and request sign-off from the designated approver(s).
2. The PR must reference this checklist and include the `legacy-removal-approved` label (applied only by the designated approver) or link to an explicit constitutional amendment PR that grants deletion authority.

Preconditions (all MUST be checked before merging deletion PR)

- [ ] Preservation inventory completed: `specs/006-roslyn-backend/preservation-inventory.md` contains dispositions and Representative-Sample-Path entries for each high-priority candidate.
- [ ] PDB mapping verification: `test/runtime-integration-tests/RoslynPdbVerificationTests.cs` and mapping unit tests (e.g., `LoweredToRoslynMappingTests`) pass on the migration branch for both .NET 8 and .NET 10-rc. Link to test runs/CI artifacts:
  - .NET 8 artifacts: ____
  - .NET 10-rc artifacts: ____
- [ ] CI matrix validation: `/.github/workflows/roslyn-backend-validation.yml` has run across OS × SDK matrix and uploaded inspection artifacts (assemblies + PDBs). Links to uploaded artifacts: ____
- [ ] Regeneration proof: any `src/ast-model/*` or `src/ast_generator/*` changes included in the PR are accompanied by a generator run; commit includes the generator log, and `scripts/check-generated.sh` passes in CI. Link to generator log: ____
- [ ] Observability & artifact identity: CI produces an `artifact-manifest.json` (or equivalent) that records backend used, Roslyn version, git sha, and build metadata for each artifact. Link to manifest in artifacts: ____
- [ ] Signing verification: Signing policy (`specs/006-roslyn-backend/signing-policy.md`) enumerates required signing steps; CI includes a signing verification step for release artifacts (or equivalent proof). Link to signing verification logs: ____
- [ ] Downstream compatibility tests: consumer-style tests (reflection/interop usage) pass against Roslyn-produced artifacts or documented shims exist. Link to test runs: ____

Approvals (signatures)

| Role | Approver (GitHub handle) | Date | Notes / PR reference |
|------|---------------------------|------|----------------------|
| Project lead / designated owner | @___ | ____ | ____ |
| Security / Signing owner | @___ | ____ | ____ |
| QA / Release lead | @___ | ____ | ____ |

Audit trail
-----------
Record here the PR numbers and links for each deletion-related PR and the constitutional amendment PR (if one was required):

- Deletion PR: #____
- Constitutional amendment PR (if any): #____

Notes
-----
- Any deviation from this checklist must be documented and approved by the project lead and recorded as a constitution amendment.

```
