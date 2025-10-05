# Research Notes: Namespace Import Directives

## MSBuild Module Enumeration
- **Decision**: Extend the existing `Compiler.ParsePhase` workflow so that MSBuild supplies an explicit manifest of `.5th` modules (via an item list emitted by the new project SDK target) rather than scanning directories or selecting the first file.
- **Rationale**: The current implementation only parses the first `.5th` file discovered, which breaks multi-module builds. Having MSBuild enumerate modules keeps build inputs deterministic and enables namespace resolution across the entire project.
- **Alternatives Considered**:
  - *Directory globbing at runtime*: rejected because it cannot respect MSBuild include/exclude semantics and hinders incremental builds.
  - *Manual manifest files*: rejected to avoid duplicated configuration—the build already knows all sources.

## Namespace Symbol Aggregation
- **Decision**: Introduce a namespace aggregation pass that merges symbol tables across all modules declaring the same namespace before standard language analysis phases run (right after `SymbolTableBuilderVisitor`).
- **Rationale**: Import directives must observe a union view of namespace members independent of their defining module. Injecting the aggregation pass early ensures later phases (overload gathering, type inference) see a coherent scope.
- **Alternatives Considered**:
  - *Module-local symbol tables only*: rejected because imports would not expose declarations from sibling modules.
  - *Late merge during IL generation*: rejected since semantic analysis relies on namespace visibility much earlier.

## Diagnostic Formatting
- **Decision**: Route namespace-resolution warnings and errors through the existing `Diagnostic` pipeline but extend messages to include `{module}:{namespace}` qualifiers and emit them on `stderr` for errors, `stdout` for warnings when diagnostics verbosity is enabled.
- **Rationale**: The compiler already centralizes diagnostics in `Compiler.CompileAsync`; augmenting message payloads maintains consistency with constitution Section X (structured text I/O) and satisfies the spec requirement for contextual metadata.
- **Alternatives Considered**:
  - *Custom logger*: rejected to avoid bypassing the standardized diagnostic surface.
  - *Silent handling of undeclared imports*: rejected per spec, which mandates warnings.

## Performance Baseline & Measurement
- **Decision**: Add targeted benchmarking hooks (behind the existing `--diagnostics` flag) that record namespace resolution elapsed time in the diagnostics list, enabling automated checks against the 2-second SLA for 100 modules.
- **Rationale**: Measuring within the compiler keeps instrumentation lightweight and aligned with constitution observability guidance while avoiding external tooling dependencies.
- **Alternatives Considered**:
  - *Standalone benchmarking harness*: deferred; heavier weight than needed for continuous validation.
  - *Relying solely on CI timings*: rejected because they provide coarse-grained feedback and mix unrelated costs.
