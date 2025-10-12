```markdown
# Clarifications — Roslyn Backend Migration

This document captures resolved [NEEDS CLARIFICATION] items for the feature and records decision, rationale, owner and date. Remove corresponding [NEEDS CLARIFICATION] markers from `spec.md` after the decision is recorded.

Entries:

- Q: Inventory of legacy IL patterns that require special handling
  - Decision: Run a focused scan of `src/**` and `test/**` for IL emission sites discovered in `inventory-il.md` and flag top N samples as preservation candidates (see `preservation-inventory.md`).
  - Owner: @aabs
  - Date: TBD

- Q: Signing/signature requirements
  - Decision: Discover current signing usages (assemblies and NuGet) and document CI verification steps in `signing-policy.md`.
  - Owner: @aabs
  - Date: TBD

- Q: Migration owner/approver
  - Decision: Project lead or designated maintainer must be named in `constitutional-deviation.md` and must sign-off T019 before deletion.
  - Owner: @aabs
  - Date: TBD

- Q: User requested immediate removal of legacy emitter
  - Decision: Option A adopted — the user's preference for immediate deletion is noted but NOT actioned. The project will retain the legacy emitter during migration and only remove it via the gated deletion process defined in FR-009 and tasks T019/T020. Immediate deletion requires an explicit constitutional amendment and recorded owner approvals.
  - Owner: @aabs
  - Date: 2025-10-12

```
