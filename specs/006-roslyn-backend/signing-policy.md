```markdown
# Signing Policy — Roslyn Backend Migration

Purpose: capture current artifact signing requirements, secrets ownership, and CI signing/verification steps for release artifacts produced by the Roslyn backend.

Checklist:

- Inventory existing signed artifacts:
  - `src/...` assemblies that are currently strong-named or signed: (list results)
  - NuGet packages produced by releases: (list results)

- Key management proposals:
  - Use GitHub Actions Secrets for signing key storage (or a centralized key vault) with restricted approvers.

- CI signing steps (sketch):
  1. Build artifact in CI job (deterministic build inputs, pinned Roslyn)
  2. Run signing step using `sn` or `dotnet sign` or a platform-specific signer; sign as required
  3. Run verification step `sn -v` or `dotnet verify` and publish verification logs as artifacts

- Acceptance: `signing-policy.md` enumerates existing signed artifacts, a recommended key-storage approach, and exact CI steps to produce and verify signed artifacts.

Owner: TBD

```
