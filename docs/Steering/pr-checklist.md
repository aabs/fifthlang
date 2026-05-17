---
id: steering-pr-checklist
title: PR Checklist And Quality Gates
inclusion: manual
---

# PR Checklist and Quality Gates

## Pre-PR Validation

:::rule id="PR-001" category="validation"
Before opening or updating a pull request:

1. Build the full solution with `dotnet build fifthlang.sln` and do not cancel the run
2. Run the full test suite with `dotnet test fifthlang.sln`
3. Validate grammar examples with `just validate-examples`
:::

## PR Requirements

:::rule id="PR-002" category="testing"
Pull requests that change behavior must add or update tests, and all relevant suites must pass locally.
:::

:::rule id="PR-003" category="generation"
Do not hand-edit `src/ast-generated/`. If the metamodel changes, include the required regeneration steps in the pull request.
:::

:::rule id="PR-004" category="parser"
Any grammar change must have corresponding updates in both the parser grammar and the AST builder visitor.
:::

:::rule id="PR-005" category="pipeline"
Transformation changes must be integrated correctly into the pipeline in `ParserManager.cs`.
:::

:::rule id="PR-006" category="contracts"
When behavior changes affect public contracts or CLI behavior, update the relevant contracts and CLI help text.
:::

:::rule id="PR-007" mandatory="false" category="maintainability"
When a change increases complexity, document the rationale in the pull request.
:::

## Review Standards

:::rule id="PR-008" mandatory="false" category="quality"
Favor the smallest viable change and keep diffs focused.
:::

:::rule id="PR-009" mandatory="false" category="quality"
Confirm reproducibility by re-running the documented commands rather than relying on assumptions.
:::

:::rule id="PR-010" mandatory="false" category="quality"
Verify that generated outputs and diagnostics remain deterministic.
:::

:::rule id="PR-011" category="lowering"
Validate that AST transformations maintain correct lowering semantics through the pipeline.
:::

## Breaking Changes

:::rule id="PR-012" category="breaking-change"
Every breaking change must include a migration note in the pull request.
:::

:::rule id="PR-013" category="breaking-change"
Breaking changes must include updated tests that reflect the new behavior.
:::

:::rule id="PR-014" category="versioning"
Apply a minor or major version bump when the change warrants it.
:::

:::rule id="PR-015" category="generation"
Generated code changes must follow metamodel versioning rules.
:::

:::rule id="PR-016" category="deprecation"
Every deprecation must be documented and covered by tests.
:::

## Grammar Compliance Checklist

:::rule id="PR-017" category="parser"
When adding or updating `.5th` examples or test programs:

1. Validate parsing locally with parser or syntax tests
2. Use grammar-supported forms only and avoid legacy shorthand
3. Add `CopyToOutputDirectory` metadata in the test `.csproj` when an integration test consumes the sample
4. Run the relevant integration tests before committing
5. Update the grammar files and constitution if the change intentionally introduces new surface syntax
:::
