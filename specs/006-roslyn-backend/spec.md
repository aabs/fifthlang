# Feature Specification: Roslyn Backend Migration

**Feature Branch**: `006-title-roslyn-backend`  
**Created**: 2025-10-12  
**Status**: Draft  
**Input**: User description: "Goal: Completely remove our current custom Common Intermediate Language (CIL/MSIL) code generation logic and replace it with C# Abstract Syntax Tree (AST) construction using the Microsoft.CodeAnalysis (Roslyn) API, leveraging Roslyn for the final CIL emission.

This refactoring will significantly reduce our technical debt in maintaining a custom compiler backend, provide access to platform-optimized CIL generation, and potentially simplify the implementation of modern .NET features (like async/await, new C#/F# language constructs) in the future."

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
1. **Given** the repository checked out at the migration branch (`006-title-roslyn-backend`) and the current CI baseline, **When** the full test matrix (parser tests, AST tests, runtime-integration-tests, kg-smoke-tests, and runtime regression suites) is executed against the codebase built with the Roslyn backend, **Then** all tests that currently pass on `master` must pass on the migration branch, or failing tests must be explicitly documented and justified with a proposed remediation plan.

2. **Given** a curated baseline of representative sample programs (a subset of `test/ast-tests/CodeSamples`, runtime smoke tests, and any known hand-crafted IL cases), **When** those samples are compiled and executed using the Roslyn-based backend, **Then** their observable behavior (exit code, stdout/stderr, produced files, side effects) must match the baseline from the legacy backend within defined equivalence criteria. [NEEDS CLARIFICATION: define exact equivalence metric — byte-for-byte, observable behaviour, or test-suite pass/fail only].

3. **Given** a debugging session against a compiled sample, **When** a developer sets breakpoints and steps through the program, **Then** generated debug information (PDB/source mapping) must be sufficient to debug at the original source level as currently supported. [NEEDS CLARIFICATION: current expected fidelity of .5th → PDB/source mapping].

4. **Given** packaging and distribution steps, **When** the CI pipeline produces release artifacts (assemblies, PDBs, NuGet packages if applicable), **Then** those artifacts must be consumable by the same downstream consumers that use artifacts produced by the legacy backend.

### Edge Cases
- Code patterns that historically relied on emitting custom IL sequences (e.g., nontrivial modifiers, unverifiable IL, custom tail-call patterns) may not map directly to C# AST. For each such pattern the migration MUST either: provide a Roslyn-expressible equivalent, implement a well-documented fallback, or document why behavior will change. [NEEDS CLARIFICATION: inventory of legacy IL patterns that require special handling].
- Cases that depend on exact emitted IL layout for interop or reflection must be discovered and validated. [NEEDS CLARIFICATION: list of public surface APIs or tests that depend on IL layout].
- Build environments with older .NET SDKs or toolchains may produce different results; Roslyn and SDK versioning must be defined and validated. [NEEDS CLARIFICATION: target SDK/Runtime versions to support].

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001 (Backend Replacement)**: The compiler MUST replace the legacy custom CIL/MSIL emission pipeline with a Roslyn-driven pipeline that constructs C# AST (or other Roslyn-native inputs) representing the program semantics and uses Roslyn to compile to CIL for supported target frameworks. Acceptance: compilation completes and produces runnable assemblies for the baseline test set.

- **FR-002 (Behavioral Equivalence)**: For a defined baseline of representative programs, the assemblies produced by the Roslyn backend MUST exhibit behavior equivalent to those produced by the legacy backend as defined in the Acceptance Scenarios. Acceptance: the baseline test harness passes or all deviations are documented with test cases.

- **FR-003 (Debugging & Diagnostics)**: The Roslyn-generated artifacts MUST include PDBs/source mapping sufficient to support the same developer debug workflows currently supported (breakpoints, step-over/into, source-level stack traces). [NEEDS CLARIFICATION: exact fidelity required for .5th → source mapping].

- **FR-004 (Performance Constraints)**: The migration MUST not introduce unacceptable regressions in compile time or runtime performance. Acceptance: benchmarks defined in `test/perf/` or CI perf jobs must show regressions within acceptable bounds. [NEEDS CLARIFICATION: define acceptable thresholds for compile-time and runtime regressions].

- **FR-005 (Feature Parity)**: Language constructs currently supported by the legacy backend (including corner cases implemented via direct IL emission) MUST be supported by the Roslyn pipeline, or have documented, approved deviations. [NEEDS CLARIFICATION: canonical list of such constructs].

- **FR-006 (Incremental Rollout)**: The project MUST provide a controlled rollout path, allowing the new backend to be enabled/disabled per-build or via CI gating to facilitate staged verification and rollback. Acceptance: a build flag or CI job configuration exists to select backend.

- **FR-007 (CI & Tests Integration)**: The CI pipeline MUST be extended to run the full validation matrix against the Roslyn backend (including parser, AST, runtime, and integration tests) and fail the merge if behavioral regressions are detected without documented exceptions.

- **FR-008 (Backward Compatibility of Artifacts)**: Artifacts produced by the Roslyn backend MUST be compatible with existing deployment flows (loading, reflection, interop) used by downstream consumers unless explicitly documented otherwise.

- **FR-009 (Removal of Legacy Code)**: The legacy CIL generator code paths MUST be retained behind a well-documented deprecation plan and only removed after the Roslyn backend meets the acceptance criteria and a canary/rollout period has completed. Acceptance: removal is gated by passing migration checklist and a release decision.

- **FR-010 (Observability & Telemetry)**: Add test/CI hooks and runtime checks that allow verification of the backend used for each build, artifact identity, and a post-deployment monitor for any runtime regressions.

- **FR-011 (Documentation & Maintainer Guidance)**: Provide migration documentation describing the new backend's architecture at a high level, how to run the Roslyn-based compilation locally for debugging, and steps to reproduce the acceptance test suite.

- **FR-012 (Security & Signing)**: Ensure support for existing assembly signing, strong-name handling, and security-relevant metadata in produced artifacts. [NEEDS CLARIFICATION: current signing/signature requirements].

*Example of marking unclear requirements:*
- **FR-006**: System MUST authenticate users via [NEEDS CLARIFICATION: auth method not specified - email/password, SSO, OAuth?]
- **FR-007**: System MUST retain user data for [NEEDS CLARIFICATION: retention period not specified]

### Key Entities *(include if feature involves data)*
- **Legacy CIL Generator**: The existing custom code that emits MSIL/CIL directly. This is the primary artifact targeted for replacement and eventual removal.

- **Roslyn Backend**: The new pipeline component responsible for constructing C# AST or other Roslyn consumable inputs and invoking Roslyn to produce assemblies and PDBs.

- **Baseline Test Suite**: Curated set of unit and integration tests and sample programs used to validate behavioral equivalence and performance (subset of `test/` and selected samples in `test/ast-tests/CodeSamples`).

- **Migration Branch & CI Jobs**: The feature branch (`006-title-roslyn-backend`) and dedicated CI jobs that validate the new backend during development and rollout.

- **Perf & Regression Benchmarks**: Scripts and datasets under `test/perf/` used to evaluate compile-time and runtime performance impacts.

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
- What exact acceptance metric should be used for behavioral equivalence (byte-for-byte vs observable behavior)?
- Which .NET SDK and Roslyn versions must be targeted and supported for this migration?
- Which legacy IL constructs require special handling (inventory)?
- What are the acceptable performance regression thresholds for compile time and runtime (CI gates)?
- Is a build-time or runtime feature flag required for selecting backend, or should selection be CI-only during rollout?
- Who is the owner of the migration and who approves the final removal of the legacy backend?

---

---
