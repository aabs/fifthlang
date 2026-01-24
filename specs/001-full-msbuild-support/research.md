# Phase 0 Research: Full MSBuild Support

## Decision 1: Align SDK outputs with standard MSBuild properties
- **Decision**: Derive `FifthOutputPath` from MSBuild `TargetPath`, using `OutputType` to set `TargetExt` and output artifacts for Exe vs Library.
- **Rationale**: Matches MSBuild conventions, ensures predictable output locations, and avoids hardcoding `.exe` in targets.
- **Alternatives considered**:
  - Keep a Fifth-specific output path property only (rejected: diverges from MSBuild and complicates tooling).

## Decision 2: Use MSBuild reference resolution items
- **Decision**: Rely on standard `@(ReferencePath)` and `@(ProjectReference)` resolution and pass resolved assemblies to the compiler.
- **Rationale**: Leverages NuGet restore and MSBuild resolution, avoiding custom dependency graphs.
- **Alternatives considered**:
  - Implement custom dependency resolver in the SDK (rejected: duplicates MSBuild and increases risk).

## Decision 3: Incremental builds via Inputs/Outputs
- **Decision**: Add `Inputs`/`Outputs` on `FifthCompile`, plus up-to-date check inputs/outputs for IDEs.
- **Rationale**: Uses MSBuild’s incremental engine and keeps rebuild times low.
- **Alternatives considered**:
  - Custom timestamp caching inside the compiler (rejected: MSBuild already provides this).

## Decision 4: Design-time build manifest
- **Decision**: Emit a design-time manifest during `DesignTimeBuild=true` in `$(IntermediateOutputPath)` including sources, references, target framework, and defines.
- **Rationale**: IDEs need project structure without full compilation; a stable manifest is easy to consume by the language server.
- **Alternatives considered**:
  - Run the full compiler during design-time builds (rejected: slow and unnecessary).

## Decision 5: Compiler output type and references
- **Decision**: Add CLI options for output type (Exe/Library) and reference paths, mapping to Roslyn `OutputKind` and `MetadataReference` inputs.
- **Rationale**: Ensures compiler can be invoked by MSBuild with resolved references and proper output type.
- **Alternatives considered**:
  - Keep compiler output fixed to Exe and wrap library support in post-processing (rejected: fragile and nonstandard).

## Decision 6: Failure policies for build graph and dependencies
- **Decision**: Fail builds on circular project references, package version conflicts, and build event failures.
- **Rationale**: Deterministic, reproducible builds and clear diagnostics reduce ambiguity in CI and tooling.
- **Alternatives considered**:
  - Best-effort resolution with warnings (rejected: nondeterministic results).

## Decision 7: Target framework support policy
- **Decision**: Enforce a documented allowlist of supported target frameworks.
- **Rationale**: Ensures consistent outputs and predictable compatibility.
- **Alternatives considered**:
  - Accept any declared framework (rejected: unsupported targets would fail later and unpredictably).
