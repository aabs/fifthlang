# Feature Specification: Full MSBuild Support

**Feature Branch**: `001-full-msbuild-support`  
**Created**: 24 January 2026  
**Status**: Draft  
**Input**: User description: "full msbuild support (library projects, references, package dependencies, incremental builds, build events, multi-targeting, and design-time build data)"

## Clarifications

### Session 2026-01-24

- Q: How should build events behave on failure? → A: Build fails if any pre/post build event fails.
- Q: Which target frameworks are supported? → A: Support a defined allowlist of target frameworks.
- Q: How should circular project references be handled? → A: Build fails on circular project references.
- Q: How should package version conflicts be handled? → A: Fail the build on version conflicts.
- Q: What design-time output should be produced? → A: Emit a lightweight manifest only.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build projects with the right outputs (Priority: P1)

As a project owner, I want my build to produce either a runnable executable or a reusable library based on project settings, so I can ship the correct artifact without manual steps.

**Why this priority**: This is the core value of MSBuild support and unblocks most users.

**Independent Test**: Can be fully tested by building a single-project solution and verifying the output type matches the project setting.

**Acceptance Scenarios**:

1. **Given** a project configured for a library output, **When** the project is built, **Then** a library artifact is produced at the configured output path.
2. **Given** a project configured for an executable output, **When** the project is built, **Then** an executable artifact is produced at the configured output path.

---

### User Story 2 - Build with project and package references (Priority: P2)

As a developer, I want builds to resolve project and package references automatically, so dependent code compiles without manual reference wiring.

**Why this priority**: Most real projects have dependencies; without this, builds are brittle and manual.

**Independent Test**: Can be tested by building a two-project solution with a dependency and a package reference and confirming the build succeeds.

**Acceptance Scenarios**:

1. **Given** a project that depends on another local project, **When** the solution is built, **Then** dependencies are built in the correct order and the build succeeds.
2. **Given** a project with a package dependency, **When** the solution is built, **Then** the dependency is resolved and the build succeeds without manual configuration.

---

### User Story 3 - Fast, repeatable builds across targets (Priority: P3)

As a developer, I want incremental builds and multi-targeting to work, so I can iterate quickly and produce artifacts for multiple targets consistently.

**Why this priority**: Productivity and distribution rely on fast rebuilds and consistent multi-target outputs.

**Independent Test**: Can be tested by building a multi-target project twice and verifying unchanged builds skip work while producing all target outputs.

**Acceptance Scenarios**:

1. **Given** an unchanged project, **When** a rebuild is requested, **Then** the build completes without reprocessing unchanged sources.
2. **Given** a project configured to build for multiple targets, **When** the build runs, **Then** artifacts are produced for each target in separate output locations.

---

### Edge Cases

- What happens when a referenced project is missing or cannot be built?
- How does the build handle circular project references? (Build fails with a clear error.)
- What happens when package dependencies resolve to incompatible versions?
- How does the system behave when a build event fails?
- How does the build handle a target framework that is unsupported?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support building projects as either reusable libraries or runnable executables based on project settings.
- **FR-002**: System MUST produce artifacts in deterministic, predictable output locations per project configuration.
- **FR-003**: System MUST resolve and build project-to-project dependencies in a correct and deterministic order.
- **FR-004**: System MUST resolve package dependencies declared by the project and include them in compilation.
- **FR-005**: System MUST fail the build with a clear error when a required dependency cannot be resolved.
- **FR-006**: System MUST support incremental builds by skipping compilation work when inputs are unchanged.
- **FR-007**: System MUST execute configured pre-build and post-build events and fail the build if any event fails.
- **FR-008**: System MUST support building the same project for multiple target frameworks in a single build request.
- **FR-009**: System MUST provide design-time build outputs suitable for editor tooling to consume project structure and references.
- **FR-009a**: System MUST emit a lightweight design-time manifest without running a full compilation, containing sources, resolved references, target framework, and defines.
- **FR-010**: System MUST validate target frameworks against an approved allowlist and fail with a clear error for unsupported targets.
- **FR-011**: System MUST detect circular project references and fail the build with a clear error.
- **FR-012**: System MUST fail the build with a clear error when package version conflicts cannot be resolved.

### Key Entities *(include if feature involves data)*

- **Project**: A buildable unit with sources, configuration settings, and output definition.
- **Project Reference**: A dependency on another project in the same workspace or solution.
- **Package Reference**: A dependency on an external package that provides reusable components.
- **Build Output**: The produced artifact and associated metadata for a project build.
- **Target Framework**: A declared build target that produces a distinct set of outputs.
- **Build Event**: A configured pre-build or post-build action with a success/failure result.

## Assumptions

- A typical solution contains up to 10 projects with fewer than 50 source files each.
- Build configurations are defined in project metadata and are available at build time.
- Dependency metadata is declared explicitly in project files.
- The supported target framework list is documented and versioned with the SDK.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A single-project build produces the configured output type with 100% accuracy across 20 consecutive builds.
- **SC-002**: A solution with at least two project references and one package dependency builds successfully without manual intervention.
- **SC-003**: For unchanged inputs, a second build completes in under 10 seconds for the typical solution described in Assumptions.
- **SC-004**: A multi-target project produces all target outputs in a single build with no missing artifacts.
