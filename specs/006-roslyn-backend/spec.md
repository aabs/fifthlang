# Feature Specification: Roslyn Backend Migration

**Feature Branch**: `006-roslyn-backend`  
**Created**: 2025-10-12  
**Status**: Draft  
**Input**: User description: "Goal: Completely remove our current custom Common Intermediate Language (CIL/MSIL) code generation logic and replace it with C# Abstract Syntax Tree (AST) construction using the Microsoft.CodeAnalysis (Roslyn) API, leveraging Roslyn for the final CIL emission.

This refactoring will significantly reduce our technical debt in maintaining a custom compiler backend, provide access to platform-optimized CIL generation, and potentially simplify the implementation of modern .NET features (like async/await, new C#/F# language constructs) in the future."

## Clarifications

### Session 2025-10-12

- Q: Which acceptance metric should be used to determine behavioral equivalence between artifacts produced by the Roslyn backend and the legacy backend? → A: Test-suite equivalence only
 - Q: Which .NET SDK and Roslyn versions must be targeted and supported for this migration? → A: NET8+10-rc
 - Q: Which policy should we adopt for legacy IL constructs that the current backend emits? → A: Allow IL to change if test-suite equivalence holds; document deviations
 - Q: Which Roslyn version policy should we adopt? → A: Pin a specific Roslyn version across builds for determinism
 - Q: What performance regression thresholds should the CI gate enforce? → A: Ignore performance testing
 - Q: What debug/source mapping fidelity is required for PDBs produced by the Roslyn backend? → A: Full line-and-column mapping for all statements and expressions (highest fidelity)

## Execution Flow (main)
```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
   - User types and permissions
   - Data retention/deletion policies  
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Security/compliance needs

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a compiler maintainer and consumer of the Fifth language toolchain, I want the compiler to stop producing hand-written CIL and instead express program semantics as C# Abstract Syntax Trees that are compiled by Microsoft.CodeAnalysis (Roslyn), so that generated assemblies are produced by a well-maintained, standard toolchain, maintenance cost is reduced, and future language features can be implemented with less backend-specific work.

### Acceptance Scenarios
1. **Given** the repository checked out at the migration branch (`006-roslyn-backend`) and the current CI baseline, **When** the full test matrix (parser tests, AST tests, runtime-integration-tests, kg-smoke-tests, and runtime regression suites) is executed against the codebase built with the Roslyn backend, **Then** all tests that currently pass on `master` must pass on the migration branch, or failing tests must be explicitly documented and justified with a proposed remediation plan.

2. **Given** a curated baseline of representative sample programs (a subset of `test/ast-tests/CodeSamples`, runtime smoke tests, and any known hand-crafted IL cases), **When** those samples are compiled and executed using the Roslyn-based backend, **Then** their observable behavior (exit code, stdout/stderr, produced files, side effects) must match the baseline from the legacy backend under the selected equivalence strategy: Test-suite equivalence only (compiled artifacts must pass the existing test suite; other emitted-IL differences are allowed).

3. **Given** a debugging session against a compiled sample, **When** a developer sets breakpoints and steps through the program, **Then** generated debug information (PDB/source mapping) must be sufficient to debug at the original source level as currently supported (see FR-003 for required full line-and-column fidelity).

4. **Given** packaging and distribution steps, **When** the CI pipeline produces release artifacts (assemblies, PDBs, NuGet packages if applicable), **Then** those artifacts must be consumable by the same downstream consumers that use artifacts produced by the legacy backend.


### Edge Cases
Code patterns that historically relied on emitting custom IL sequences (e.g., nontrivial modifiers, unverifiable IL, custom tail-call patterns) may not map directly to C# AST. Policy: allow emitted IL to change if test-suite equivalence holds; document every deviation in the migration inventory with rationale. For constructs that must preserve IL layout or behavior (interop/reflection-sensitive), add them to the inventory as preservation candidates requiring explicit handling or approval. [NEEDS CLARIFICATION: inventory of legacy IL patterns that require special handling].
Cases that depend on exact emitted IL layout for interop or reflection must be discovered and validated. [NEEDS CLARIFICATION: list of public surface APIs or tests that depend on IL layout].
Build environments with different .NET SDKs or toolchains may produce different results. Target SDKs for the Roslyn backend: .NET 8 and the .NET 10 release candidate (10-rc). Roslyn versioning should align with the SDK-provided Roslyn when possible; specific pinning decisions will be made during planning. CI must validate builds and run the baseline test suite on both targeted SDKs.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001 (Backend Replacement)**: The compiler MUST replace the legacy custom CIL/MSIL emission pipeline with a Roslyn-driven pipeline that constructs C# AST (or other Roslyn-native inputs) representing the program semantics and uses Roslyn to compile to CIL for supported target frameworks. Acceptance: compilation completes and produces runnable assemblies for the baseline test set.

- **FR-002 (Behavioral Equivalence)**: For a defined baseline of representative programs, the assemblies produced by the Roslyn backend MUST exhibit behavior equivalent to those produced by the legacy backend as defined in the Acceptance Scenarios. Acceptance: the baseline test harness must pass (Test-suite equivalence only — compiled artifacts must pass the existing test suite); any deviations must be documented with test cases and an approved remediation plan.

- **FR-003 (Debugging & Diagnostics)**: The Roslyn-generated artifacts MUST include Portable PDBs with full line-and-column mapping for statements and expressions in the original .5th source so that breakpoints, step-over/into, variable inspection and source-level stack traces map precisely to original source locations. Acceptance: setting breakpoints on any statement or emitted expression resolves to the correct file, start-line/start-column and end-line/end-column as recorded in the baseline tests; PDBs must include SequencePoints or equivalent mapping data to support this fidelity.

**FR-004 (Performance Constraints)**: Performance validation is intentionally deferred during CI gating for this migration. Acceptance: CI will not block migration merges on performance regressions; performance benchmarks in `test/perf/` should be executed during the stabilization phase and documented. Significant regressions discovered post-cutover will trigger a documented remediation plan. (Decision: performance testing ignored for CI gating during migration.)

- **FR-005 (Feature Parity)**: Language constructs currently supported by the legacy backend (including corner cases implemented via direct IL emission) MUST be supported by the Roslyn pipeline, or have documented, approved deviations. Default acceptance: test-suite equivalence is sufficient; IL differences are acceptable if the baseline test harness passes. Preservation candidates (reflection/interop-sensitive members) must be inventoried and addressed explicitly. [NEEDS CLARIFICATION: canonical list of such constructs].

- **FR-006 (Incremental Rollout)**: The project MUST provide a controlled rollout path, allowing the new backend to be enabled/disabled per-build or via CI gating to facilitate staged verification and rollback. Acceptance: a build flag or CI job configuration exists to select backend.

- **FR-007 (CI & Tests Integration)**: The CI pipeline MUST be extended to run the full validation matrix against the Roslyn backend (including parser, AST, runtime, and integration tests) and fail the merge if behavioral regressions are detected without documented exceptions.

- **FR-008 (Backward Compatibility of Artifacts)**: Artifacts produced by the Roslyn backend MUST be compatible with existing deployment flows (loading, reflection, interop) used by downstream consumers unless explicitly documented otherwise.

- **FR-009 (Removal of Legacy Code)**: The legacy CIL generator code paths MUST be retained behind a well-documented deprecation plan and only removed after the Roslyn backend meets the acceptance criteria and a canary/rollout period has completed. Acceptance: removal is gated by passing migration checklist and a release decision.

- **FR-010 (Observability & Telemetry)**: Add test/CI hooks and runtime checks that allow verification of the backend used for each build, artifact identity, and a post-deployment monitor for any runtime regressions.

- **FR-011 (Documentation & Maintainer Guidance)**: Provide migration documentation describing the new backend's architecture at a high level, how to run the Roslyn-based compilation locally for debugging, and steps to reproduce the acceptance test suite.

- **FR-012 (Security & Signing)**: Ensure support for existing assembly signing, strong-name handling, and security-relevant metadata in produced artifacts. [NEEDS CLARIFICATION: current signing/signature requirements].

- **FR-013 (Target SDKs)**: The Roslyn backend MUST be validated on and support .NET 8 and the .NET 10 release candidate (10-rc). Acceptance: CI runs the baseline test suite on both SDKs and produces consumable artifacts for each target.

- **FR-014 (Roslyn Pinning)**: For deterministic builds and artifact reproducibility, the project MUST pin an explicit Roslyn compiler package/version for release and CI builds. Acceptance: CI reproduces identical build outputs given the same pinned Roslyn version; dev workflows may use SDK-provided Roslyn but releases must use the pinned version.

*Example of marking unclear requirements:*
- **FR-006**: System MUST authenticate users via [NEEDS CLARIFICATION: auth method not specified - email/password, SSO, OAuth?]
- **FR-007**: System MUST retain user data for [NEEDS CLARIFICATION: retention period not specified]

### Key Entities *(include if feature involves data)*
- **Legacy CIL Generator**: The existing custom code that emits MSIL/CIL directly. This is the primary artifact targeted for replacement and eventual removal.

- **Roslyn Backend**: The new pipeline component responsible for constructing C# AST or other Roslyn consumable inputs and invoking Roslyn to produce assemblies and PDBs.

- **Baseline Test Suite**: Curated set of unit and integration tests and sample programs used to validate behavioral equivalence and performance (subset of `test/` and selected samples in `test/ast-tests/CodeSamples`).

- **Migration Branch & CI Jobs**: The feature branch (`006-roslyn-backend`) and dedicated CI jobs that validate the new backend during development and rollout.

- **Perf & Regression Benchmarks**: Scripts and datasets under `test/perf/` used to evaluate compile-time and runtime performance impacts.

### Technical background: AST → IL transformation (current mechanism)

The current compiler converts the high-level Fifth AST into an IL-level AST via a dedicated lowering visitor (`AstToIlTransformationVisitor`). That IL AST is then converted into executable artifacts by `ILEmissionVisitor` and the `PEEmitter`. High-level overview of the mechanism:

- Input: Lowered AST (result of Language Transformation passes). This AST is semantically complete and annotated with symbol/type information from earlier passes.
- Lowering: `AstToIlTransformationVisitor` performs a structured traversal and emits IL-level nodes such as `CallInstruction`, `LoadInstruction`, `StoreInstruction`, `BranchInstruction`, `NewObjInstruction`, and `ReturnInstruction`.
   - Responsibilities: map functions to IL methods, classes to type declarations, expression trees to linear instruction sequences, infer local variable types for `.locals init`, and encode external call signatures (`extcall:` format) used by later emission stages.
   - Token resolution: the visitor delegates to token- and metadata-resolution helpers so instruction operands (method tokens, field tokens, type handles) are resolvable by the PE emitter stage.
- IL AST: A compact, instruction-oriented model (`ILMetamodel.cs`) that closely mirrors ECMA-335 constructs and is intended to be directly consumable by textual (`ILEmissionVisitor`) or binary (`PEEmitter`) emitters.
- Emission: `ILEmissionVisitor` can render textual `.il` (used by some test harnesses), while `PEEmitter` performs binary metadata and IL emission using `System.Reflection.Metadata` APIs (MetadataBuilder, InstructionEncoder) to produce PE assemblies and PDBs.

Notes about current responsibilities and coupling:
- The lowering visitor performs both control-flow lowering and metadata-binding helpers that produce canonical IL AST suitable for direct PE metadata emission. This creates a tight coupling between the lowered IL AST shape and the expectations of the `PEEmitter`.
- Debug information generation is implemented at emit time by `PEEmitter` (PDB generation) and by textual `.il` outputs used by tests; source-mapping data is embedded in emitted artifacts at this stage.

Implication: The current cut-over to Roslyn must clearly replace the responsibilities that currently are implemented by the chain: `AstToIlTransformationVisitor` → `IL AST` → `ILEmissionVisitor` / `PEEmitter`.

### Cut‑over design: where Roslyn replaces the downstream pipeline

Cut-over summary
- Cut-over point (explicit): the boundary between the Language Transformations (Lowered AST) phase and the IL Transformation phase. Practically, the new Roslyn backend will be invoked immediately after the last language-lowering visitor completes and before the `AstToIlTransformationVisitor` runs.
- New component: `LoweredAstToRoslynTranslator` (name provisional). Responsibilities:
   - Accept the same Lowered AST used today by `AstToIlTransformationVisitor`.
   - Produce Roslyn C# syntax trees (CSharpSyntaxNodes) representing equivalent semantics of classes, methods, fields and expressions.
   - Emit or supply mapping metadata (via #line directives and/or EmbeddedText and PDB-aware EmitOptions) that allows generated assemblies to be debugged at the original Fifth source level.
   - Translate extcall: signatures and other metadata-defined calls into resolved method invocations using normal Roslyn binding/mechanisms or compile-time generated wrappers.
   - Synthesize any runtime shims required where raw IL semantics cannot be produced directly from C# (for example, unverifiable IL patterns). The plan is to minimize shims and prefer expressible C# patterns; only inventoried preservation candidates will get targeted shims or a narrow legacy-emitter fallback.

Why translate from Lowered AST instead of re-parsing source
- Lowered AST already contains resolved symbols, type annotations and lowered constructs suitable for code emission; this avoids repeating analysis and ensures semantics remain unchanged by the translation.

Roslyn emission approach (high level)
- Construct C# SyntaxNodes using Microsoft.CodeAnalysis.CSharp.SyntaxFactory; create one or more generated C# source files per input module.
- Use `#line` pragmas, Roslyn EmbeddedText and explicit SequencePoint emission so PDBs include full start-line/start-column and end-line/end-column mappings to original .5th source locations. Populate EmitOptions to produce Portable PDBs (DebugInformationFormat.PortablePdb) and ensure SequencePoints are emitted for each statement/expression to satisfy FR-003.
- Build a CSharpCompilation with referenced assemblies (the same runtime/NuGet dependencies as before) and call `Emit(Stream assemblyStream, Stream pdbStream, embeddedTexts: ...)` to produce PE/pdb artifacts.

Fallback / preservation strategy
- For language constructs that cannot be expressed directly in C#, the translator will prefer:
   1. A pure-C# implementation that preserves observable semantics; or
   2. A small runtime shim (helper method) that reproduces the exact semantics while keeping the generated C# readable; or
   3. As a last resort during the migration, invoke the legacy `PEEmitter` for a narrowly scoped set of preservation candidates (i.e., we preserve the current IL pipeline for a handful of tested patterns only). The goal is to eliminate case (3) as soon as feasible.

Cut‑over mechanics (process)
1. Add `LoweredAstToRoslynTranslator` to the compilation pipeline in `ParserManager.cs` immediately after the final language transformation visitor.
2. Introduce a compiler option/feature flag (`--backend=roslyn|legacy`) and CI job variants to build and test both backends in parallel during the rollout.
3. Implement a Roslyn POC that validates core constructs (methods, classes, basic expressions) and produces Portable PDBs with correct #line mapping.
4. Incrementally translate more constructs and run side-by-side comparisons; keep the legacy pipeline available for preserving enumerated cases until the Roslyn backend matures.
5. Once coverage and acceptance criteria are met, remove `AstToIlTransformationVisitor`, `ILEmissionVisitor`, `PEEmitter` and any now-unused IL metamodel elements (after a documented deprecation/canary period).

Acceptance criteria specific to the cut‑over
- The Roslyn backend must compile the baseline test suite successfully (Test-suite equivalence); where observable behavior differs, deviations must be documented and approved.
- Generated PDBs must provide full line-and-column source-level debugging fidelity as defined in FR-003 (SequencePoints with precise start/end line and column information) for worker scenarios validated in the baseline tests.
- The migration must include a deprecation plan and an agreed canary period before deletion of legacy emitters.

Developer guidance (local debugging)
- Devs will be able to run the Roslyn backend locally using `--backend=roslyn` (or an equivalent project configuration) and step through generated artifacts referencing original `.5th` sources via Visual Studio/VSCode with PDB support.
 - Developer validation: provide a small validation harness that compiles a representative `.5th` sample with `--backend=roslyn`, loads the produced assembly and PDB in a debugger, and asserts that breakpoints set at specific source line/column positions hit the expected runtime locations. This harness should be included in `test/runtime-integration-tests` for automated verification on the feature branch.


### Out of Scope / Non-Goals
- Adding new language surface features as part of the migration (the migration focuses on backend implementation, not language semantics). New language features should be proposed and scoped separately.
- Broad runtime or standard library rewrites unrelated to backend emission.

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [ ] The spec's objectives are focused on user value and long-term maintainability
- [ ] Implementation decisions that were included (Roslyn) are driven by the user's request and explicitly called out
- [ ] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain (must resolve before final acceptance)
- [ ] Requirements are testable and have clear acceptance criteria
- [ ] Success criteria are measurable (test-suite pass, performance bounds, debug fidelity)
- [ ] Scope is clearly bounded and non-goals are documented
- [ ] Dependencies and assumptions identified and owned

---

## Execution Status
*Updated by main() during processing*

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked (see [NEEDS CLARIFICATION] items)
- [x] User scenarios defined
- [x] Requirements generated (draft)
- [x] Entities identified
- [ ] Review checklist passed (blocked on clarifications)
 - [x] Inventory of legacy IL emission sites completed (see `inventory-il.md`)

### Migration Phases (high-level)
1. Spike & feasibility: produce a proof-of-concept that compiles a small subset of language features to Roslyn and emits runnable assemblies.
2. Incremental implementation: implement Roslyn backend for core language constructs and add test coverage for each increment.
3. Full coverage: implement remaining constructs and special-case handling for legacy IL patterns.
4. Stabilize & optimize: run perf benchmarks and tune generation; ensure debug fidelity and CI stability.
5. Cutover & removal: gate removal of legacy emitter behind passing canary period and release decision.

### Risks & Mitigations
- Risk: Some legacy IL patterns cannot be expressed by Roslyn C# AST. Mitigation: identify and document these early, and provide fallback emission options or small runtime shims.
- Risk: Performance regression (compile- or runtime-time). Mitigation: benchmark early, keep legacy emitter as a gated fallback during rollout.
- Risk: Breakage of downstream consumers expecting a particular IL layout. Mitigation: catalogue public-facing APIs and run downstream integration tests.

### Open Questions / [NEEDS CLARIFICATION]
- Which Roslyn version should be pinned versus using SDK-provided Roslyn?
- Which legacy IL constructs require special handling (inventory)?
- What are the acceptable performance regression thresholds for compile time and runtime (CI gates)?
- Is a build-time or runtime feature flag required for selecting backend, or should selection be CI-only during rollout?
- Who is the owner of the migration and who approves the final removal of the legacy backend?

---

---
