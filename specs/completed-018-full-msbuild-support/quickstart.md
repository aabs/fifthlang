# Quickstart: Full MSBuild Support

## Goal
Build Fifth projects as executables or libraries with dependencies, incremental builds, multi-targeting, and design-time metadata.

## Steps
1. Create or open a Fifth project file and declare source files.
2. Set `OutputType` to `Exe` or `Library` and configure output paths.
3. Add `ProjectReference` entries for local project dependencies.
4. Add `PackageReference` entries for external dependencies.
5. Choose target frameworks from the supported allowlist; set multiple `TargetFrameworks` if needed.
6. Run a build; verify outputs appear in the configured output locations and that repeated builds skip unchanged work.
7. Verify builds fail with clear errors for circular project references, package version conflicts, or failed build events.
8. For IDE tooling, trigger a design-time build and verify the manifest is produced in the intermediate output folder.

## Expected Results
- Output artifacts match the configured `OutputType`.
- Dependencies resolve automatically without manual wiring.
- Rebuilds are fast when inputs are unchanged.
- Design-time metadata is available for editor tooling.
