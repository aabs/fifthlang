# Research Notes: Namespace Import Directives

## MSBuild Module Enumeration
- **Decision**: Extend the existing `Compiler.ParsePhase` workflow so that MSBuild supplies an explicit manifest of `.5th` modules (via an item list emitted by the new project SDK target) rather than relying on implicit discovery.
- **Rationale**: The current implementation only parses the first `.5th` file discovered, which breaks multi-module builds. Having MSBuild enumerate modules keeps build inputs deterministic and enables incremental builds.
- **Alternatives Considered**:
  - *Directory globbing at runtime*: rejected because it cannot respect MSBuild include/exclude semantics and hinders incremental builds.
  - *Manual manifest files*: rejected to avoid duplicated configuration—the build already knows all sources.

## Namespace Symbol Aggregation
- **Decision**: Introduce a namespace aggregation pass that merges symbol tables across all modules declaring the same namespace before standard language analysis phases run (right after `SymbolTableBuilder`).
- **Rationale**: Import directives must observe a union view of namespace members independent of their defining module. Injecting the aggregation pass early ensures later phases (overload gathering, etc.) operate on a stable view of symbols.
- **Alternatives Considered**:
  - *Module-local symbol tables only*: rejected because imports would not expose declarations from sibling modules.
  - *Late merge during IL generation*: rejected since semantic analysis relies on namespace visibility much earlier.

## Diagnostic Formatting
- **Decision**: Route namespace-resolution warnings and errors through the existing `Diagnostic` pipeline but extend messages to include `{module}:{namespace}` qualifiers and emit them on `stderr`.
- **Rationale**: The compiler already centralizes diagnostics in `Compiler.CompileAsync`; augmenting message payloads maintains consistency with constitution Section X (structured text I/O) and satisfies user expectations.
- **Alternatives Considered**:
  - *Custom logger*: rejected to avoid bypassing the standardized diagnostic surface.
  - *Silent handling of undeclared imports*: rejected per spec, which mandates warnings.

## Performance Baseline & Measurement
- **Decision**: Add targeted benchmarking hooks (behind the existing `--diagnostics` flag) that record namespace resolution elapsed time in the diagnostics list, enabling automated checks against performance regressions.
- **Rationale**: Measuring within the compiler keeps instrumentation lightweight and aligned with constitution observability guidance while avoiding external tooling dependencies.
- **Alternatives Considered**:
  - *Standalone benchmarking harness*: deferred; heavier weight than needed for continuous validation.
  - *Relying solely on CI timings*: rejected because they provide coarse-grained feedback and mix unrelated costs.

## CLI Enhancements for Multi-Module Builds
- **Decision**: Enhance the CLI invocation to allow specifying multiple `.5th` source modules using explicit paths or glob patterns (e.g., `fifthc src/**/*.5th`). This enables users to include multiple modules efficiently while keeping the interface intuitive.
- **Rationale**: Globbing makes it easier to manage large projects with distributed module locations, aligning with user expectations for modern tooling convenience.
- **Alternatives Considered**:
  - *Manual enumeration of files*: rejected to avoid tedious and error-prone specification for large projects.
  - *Implicit discovery only*: rejected because it prevents users from tailoring inputs to their needs.