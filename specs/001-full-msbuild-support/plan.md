# Implementation Plan: Full MSBuild Support

**Branch**: `001-full-msbuild-support` | **Date**: 24 January 2026 | **Spec**: [specs/001-full-msbuild-support/spec.md](specs/001-full-msbuild-support/spec.md)
**Input**: Feature specification from `/specs/001-full-msbuild-support/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Deliver full MSBuild support for Fifth projects by expanding SDK targets and compiler integration to support library outputs, deterministic dependency resolution, incremental builds, build events (fail on error), multi-targeting with an allowlisted set of target frameworks, and lightweight design-time manifests for tooling.

## Technical Context

**Language/Version**: C# 14 on .NET 8.0 (SDK pinned by global.json)  
**Primary Dependencies**: MSBuild SDK infrastructure, Roslyn compilation, NuGet restore pipeline  
**Storage**: File system outputs (bin/obj, manifests)  
**Testing**: xUnit + FluentAssertions  
**Target Platform**: .NET SDK 8 on Windows/macOS/Linux  
**Project Type**: Multi-project .NET solution  
**Performance Goals**: Incremental rebuilds under 10 seconds for a typical 10-project solution  
**Constraints**: Deterministic outputs; fail on build event errors, circular references, and package version conflicts; allowlisted target frameworks  
**Scale/Scope**: Up to 10 projects, fewer than 50 source files per project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Library-first, contracts-first**: Pass. Work scoped to SDK/CLI contracts with tests.
- **CLI and text I/O discipline**: Pass. Compiler CLI remains deterministic and scriptable.
- **Do not edit generated code**: Pass. No changes under `src/ast-generated/`.
- **Test-first & runtime validation**: Pass. Tests will validate build behavior.
- **Reproducible builds & toolchain discipline**: Pass. Toolchain pinning retained.
- **Simplicity/minimal surface**: Pass. Leverages standard MSBuild resolution patterns.

Re-check after Phase 1 design: Pass.

## Project Structure

### Documentation (this feature)

```text
specs/001-full-msbuild-support/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Fifth.Sdk/
│   └── Sdk/
│       ├── Sdk.props
│       └── Sdk.targets
├── compiler/
│   ├── Compiler.cs
│   ├── CompilerOptions.cs
│   └── Program.cs
├── language-server/
└── vscode-client/

test/
├── fifth-sdk-tests/
├── runtime-integration-tests/
└── syntax-parser-tests/
```

**Structure Decision**: Use the existing multi-project solution structure, focusing on Fifth.Sdk targets/props, compiler CLI/options, and associated tests.

## Complexity Tracking

No constitution violations identified.

## Phase 0: Outline & Research

- Consolidate MSBuild resolution patterns and design-time build conventions.
- Confirm compiler reference handling and output-type mapping strategy.
- Document decisions in [specs/001-full-msbuild-support/research.md](specs/001-full-msbuild-support/research.md).

## Phase 1: Design & Contracts

- Define build-related entities and constraints in [specs/001-full-msbuild-support/data-model.md](specs/001-full-msbuild-support/data-model.md).
- Generate build orchestration contracts in [specs/001-full-msbuild-support/contracts/openapi.yaml](specs/001-full-msbuild-support/contracts/openapi.yaml).
- Document usage guidance in [specs/001-full-msbuild-support/quickstart.md](specs/001-full-msbuild-support/quickstart.md).
- Update agent context via `.specify/scripts/bash/update-agent-context.sh copilot`.

## Phase 2: Planning

- Break work into test-first tasks for SDK targets/props, compiler options, reference resolution, incremental build wiring, and design-time manifests.
- Define validation steps (build + relevant tests).
