---
description: pr-checklist
inclusion: always
---
## Validation
- PR-001: Before opening or updating a pull request:

1. Build the full solution with `dotnet build fifthlang.sln` and do not cancel the run
2. Run the full test suite with `dotnet test fifthlang.sln`
3. Validate grammar examples with `just validate-examples`
## Testing
- PR-002: Pull requests that change behavior must add or update tests, and all relevant suites must pass locally.
## Generation
- PR-003: Do not hand-edit `src/ast-generated/`. If the metamodel changes, include the required regeneration steps in the pull request.
- PR-015: Generated code changes must follow metamodel versioning rules.
## Parser
- PR-004: Any grammar change must have corresponding updates in both the parser grammar and the AST builder visitor.
- PR-017: When adding or updating `.5th` examples or test programs:

1. Validate parsing locally with parser or syntax tests
2. Use grammar-supported forms only and avoid legacy shorthand
3. Add `CopyToOutputDirectory` metadata in the test `.csproj` when an integration test consumes the sample
4. Run the relevant integration tests before committing
5. Update the grammar files and constitution if the change intentionally introduces new surface syntax
## Pipeline
- PR-005: Transformation changes must be integrated correctly into the pipeline in `ParserManager.cs`.
## Contracts
- PR-006: When behavior changes affect public contracts or CLI behavior, update the relevant contracts and CLI help text.
## Maintainability
- PR-007: When a change increases complexity, document the rationale in the pull request.
## Quality
- PR-008: Favor the smallest viable change and keep diffs focused.
- PR-009: Confirm reproducibility by re-running the documented commands rather than relying on assumptions.
- PR-010: Verify that generated outputs and diagnostics remain deterministic.
## Lowering
- PR-011: Validate that AST transformations maintain correct lowering semantics through the pipeline.
## Breaking Change
- PR-012: Every breaking change must include a migration note in the pull request.
- PR-013: Breaking changes must include updated tests that reflect the new behavior.
## Versioning
- PR-014: Apply a minor or major version bump when the change warrants it.
## Deprecation
- PR-016: Every deprecation must be documented and covered by tests.
