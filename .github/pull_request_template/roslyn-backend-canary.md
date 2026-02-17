## Roslyn Backend / Toolchain Change PR Guidance

Use this template when your Pull Request touches toolchain configuration (for example `global.json`, pinned Roslyn package, `Directory.Build.props`, or generator templates) or when you request a canary/overlay run that exercises legacy emitters in CI.

IMPORTANT: Changes to the canonical pinned SDK (`global.json`) are a constitution-level change. The repository enforces a guard in CI that will fail PRs modifying `global.json` unless explicit approval has been recorded (see steps below).

Checklist
- [ ] I have described the change and why it is necessary in the PR body.
- [ ] I have verified this change does not hand-edit generated outputs under `src/ast-generated/`. If metamodel/templates changed, I have included the generator run output and logs in this PR.
- [ ] If this PR modifies `global.json` or the canonical SDK: I have included one of the following:
  - A link to a separate constitution amendment PR/issue that documents the rationale and includes the required migration plan; OR
  - Confirmation that the `toolchain-change-approved` label has been added to this PR by an authorized approver (project lead or designated owner). **CI will block merges of PRs that change `global.json` unless this label is present.**
- [ ] If you want a CI overlay (canary) that overlays legacy emitter code for side-by-side validation in CI, add the `canary` label to this PR. The overlay will be applied in CI from the authoritative legacy ref and will not commit any files to this branch.
- [ ] I have updated `specs/006-roslyn-backend/constitutional-deviation.md` or linked the relevant deviation checklist if this PR is part of the removal/cutover workflow (T019/T020). The checkboxes in that checklist are current.

- [ ] If this PR proposes deletion of the legacy emitter files, I confirm: (a) this PR references FR-009 and updates `specs/006-roslyn-backend/constitutional-deviation.md` with evidence for each precondition; and (b) I will obtain the `legacy-removal-approved` label from the designated approver (project lead or named owner) before merging. CI will block merges of deletion PRs that lack the `legacy-removal-approved` label or an approved constitutional amendment.

How to request approval for `global.json` changes
1. Open an issue or a constitution-amendment PR that explains the proposed toolchain change (rationale, roll-forward/rollback steps, CI impact, and downstream compatibility concerns). Link that issue/PR in this PR's description.
2. Ask the project lead or the designated approver(s) to review the amendment. Once the approver is satisfied, they should add the `toolchain-change-approved` label to the amendment PR and the related PR(s) that must modify `global.json`.
3. CI will detect the presence of `global.json` modifications and require the `toolchain-change-approved` label before allowing merging.

How to request approval for legacy emitter deletion
1. Open a constitutional-deviation PR or issue documenting the rationale, preservation inventory links, CI evidence and the proposed deletion PR(s).
2. Request review from the project lead and designated approver(s). Once satisfied, the approver should add the `legacy-removal-approved` label to the constitutional-deviation PR and to the deletion PR(s) that implement the removal.
3. CI will block merging of deletion PRs that do not have the `legacy-removal-approved` label or the explicit constitutional amendment referenced in the PR description.

Notes for reviewers
- If you see a PR modifying `global.json` without an amendment link or the required label, do not approve the PR. Request a constitution amendment or add the `toolchain-change-approved` label only after the amendment is recorded and approved.
- For canary validations: adding the `canary` label will request the CI overlay run. Reviewers should examine the uploaded artifacts and PDB mappings in the CI job artifacts.

Contact
- If you are unsure who should sign off, add a comment on the PR and tag `@project-lead` (or the relevant maintainer) to request guidance.
