# Constitutional Deviation: Immediate Removal of Legacy IL Emitter

Created: 2025-10-12
Feature branch: `006-roslyn-backend`

Purpose
-------
This document defines the mandatory sign-off checklist, gating, and PR templates required to authorize a constitutional deviation that would permit the immediate removal of the legacy IL AST metamodel and the IL emission pipeline (AstToIlTransformationVisitor, ILEmissionVisitor, PEEmitter and related artifacts).

Context & Constraints
---------------------
- The project constitution (`.specify/memory/constitution.md`) requires a documented canary and acceptance plan before removing legacy emitters. Deviation from that rule is only allowed after explicit owner approval recorded here.
- Removal of code or deletion of tests is a destructive change: this deviation requires demonstrable, reproducible evidence (tests, CI, signing, preservation inventory) and explicit sign-offs.

Non-negotiable Preconditions (all MUST be satisfied before deletion)
-----------------------------------------------------------------
The following items are mandatory and must be attached to any *Move* or *Deletion* PR prior to merging.

1. Preservation inventory (`specs/006-roslyn-backend/preservation-inventory.md`) — Task: T022
   - Complete inventory of reflection/interop-sensitive members, tests that assert IL layout, and any downstream consumers that require exact IL layout.
   - For each item: file/test path, reason, proposed disposition (shim | keep legacy emitter for case | convert test), and priority.

2. PDB fidelity verification harness and green results — Task: T020
   - Portable PDB SequencePoint tests implemented and passing in CI for representative samples.
   - Artifacts: test output, failing-to-passing trace, and CI job link.

3. Roslyn POC translator & mapping tests (POC level) — Tasks: T031, T032
   - Translator can handle core constructs used in preservation candidates.
   - `TranslationResult` and `MappingTable` tests pass.

4. CI matrix coverage and green runs — Task: T003 / T040
   - The Roslyn backend validation workflow must run and pass across the required SDKs and OS matrix (.NET 8 and .NET 10-rc, Linux/Windows/macOS).
   - CI artifacts (logs, produced assemblies, PDBs) must be attached to the PR.

5. Signing & release verification policy implemented — Task: T041
   - Release build signing policy documented in `specs/006-roslyn-backend/signing.md` and signing steps in `Directory.Build.props` or CI pipeline.
   - CI verification must be able to validate signatures (public key token, verifiable signature) for produced artifacts.

6. Observability & artifact identity manifest — Task: T042
   - Each built artifact must include a small manifest `backend.json` with backend id, Roslyn version, build id, and commit sha for traceability.

7. Documentation & maintainers guide — Task: T052
   - Maintainer guide describing how to run the translator locally, reproduce PDB validation, and how to revert the removal.

8. Canary plan defined and accepted (below)

Required Approvals (sign-off roles)
----------------------------------
All approvals must be recorded in the PR body (the PR checklist below) and the approvals must come from the following roles (GitHub handles and dates required):

- Project Lead / Feature Owner — required
- Release Manager — required
- QA / Test Lead — required
- Security / Signing Owner — required
- Architecture Owner (compiler core) — required
- Downstream Consumer Representative (if known) — strongly recommended

Canary & Rollout Plan (proposed default, owner-editable)
-----------------------------------------------------
This plan is the recommended minimum canary sequence. Owners may change duration/metrics but must include equivalent safeguards.

1. Non-destructive staging PR — Move legacy emitter sources to `src/legacy-emitters/` and update build to exclude that folder from the normal compilation (no deletion yet). This ensures an immediate and reversible test of removability.
   - Run full CI matrix with this PR.
2. Canary interval — Keep staging PR merged to `feature/canary` or equivalent and monitor for at least 14 calendar days OR at least 5 successful CI runs by active integration (whichever is longer).
3. Monitoring during canary — track failures in: runtime-integration-tests, kg-smoke-tests, downstream integration tests, and any telemetry signals. PDB mapping tests must remain green.
4. Acceptance during canary — zero critical/blocked regressions and fewer than X (configurable) medium regressions with remediation tasks open and assigned.
5. Final deletion PR — After canary acceptance, prepare a dedicated deletion PR that removes the legacy sources. Include full checklist and sign-offs before merge.

Rollback Plan
-------------
If regressions are detected during the staging or canary period, perform the following:

1. Revert the staging PR or re-enable the legacy emitter path (fast rollback instructions included in the maintainer guide).
2. Open a post-mortem documenting root cause and required remediation tasks.
3. Re-run CI and do not proceed to deletion until blockers are closed and revalidated.

PR Templates (copy into PR description when preparing staging or deletion PRs)
------------------------------------------------------------------
1) Staging PR (non-destructive move)

Title: [Staging] Move legacy emitters to `src/legacy-emitters/` — feature/canary

Description:
- Purpose: test removal viability without deleting code. This staging PR moves the legacy emitter files to an archive location and configures the build to exclude them from default builds.
- Changes:
  - Files moved: (list)
  - Build change: (what changed)

Preconditions (attach links):
- Preservation inventory: `/specs/006-roslyn-backend/preservation-inventory.md` [link]
- Roslyn POC test results: [CI link]
- Signing policy doc: `/specs/006-roslyn-backend/signing.md` [link]

Checklist (must be checked before merge):
- [ ] `preservation-inventory.md` attached and reviewed
- [ ] PDB mapping tests (T020) passing in CI
- [ ] Roslyn CI matrix green for this PR
- [ ] Artifact manifests attached to CI artifacts
- [ ] Project Lead approval (@username, date)
- [ ] QA Lead approval (@username, date)
- [ ] Security Lead approval (@username, date)

2) Deletion PR (destructive)

Title: [DELETE] Remove legacy IL emitter and IL metamodel (final) — feature/legacy-remove

Description:
- Purpose: remove AstToIlTransformationVisitor, ILEmissionVisitor, PEEmitter, ILMetamodel and associated IL-specific tests that have been converted or inventoried for preservation.
- Summary of preconditions satisfied (links to artifacts and approvals):
  - Preservation inventory: [link]
  - Canary PR: [link], canary interval end date: YYYY-MM-DD
  - CI matrix results: [link]
  - Signing policy implemented: [link]
  - Maintainer guide updated: [link]

Mandatory deletion checklist (all items must be green):
- [ ] Staging PR merged and canary interval completed (dates recorded)
- [ ] No critical regressions triggered during canary
- [ ] Preservation dispositions resolved for all candidates (shims implemented or tests dropped/converted)
- [ ] All preservation shims are unit-tested and integration-tested
- [ ] PDB mapping tests passing for migrated tests
- [ ] Release artifacts reproducible and signed (T041) verified in CI
- [ ] Project Lead: Approved (@username, date)
- [ ] Release Manager: Approved (@username, date)
- [ ] QA Lead: Approved (@username, date)
- [ ] Security Lead: Approved (@username, date)
- [ ] Architecture Owner: Approved (@username, date)
- [ ] Downstream Consumer Owner(s): Notified / Approved (@username, date)

Post-deletion steps
-------------------
- Create a public changelog entry and migration note.
- Monitor post-deletion telemetry and tests for a 30-day period. Open incidents for any regressions.
- Maintain an archived branch/tag with the legacy emitters for emergency re-introduction if required.

Enforcement & Compliance
------------------------
- This document must be referenced in both the staging and deletion PR descriptions. The deletion PR must not be merged without all required signoffs recorded in the PR body.
- Repository maintainers must enforce the checklist via PR reviews and CI checks where possible.

Appendix: references
--------------------
- Spec: `/specs/006-roslyn-backend/spec.md`
- Plan: `/specs/006-roslyn-backend/plan.md`
- Tasks: `/specs/006-roslyn-backend/tasks.md` (T022, T020, T031, T032, T003, T041, T042, T052 referenced)
- Constitution excerpt: `.specify/memory/constitution.md` (see sections on legacy emitters and deprecation)

Document version: 1.0
