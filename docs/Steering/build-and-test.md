---
id: steering-build-and-test
title: Build And Test Commands
inclusion: auto
---

# Build and Test Commands

## Prerequisites

:::rule id="BUILD-001" severity="error" category="prerequisites" domain="build"
The local environment must include:

- .NET 10.0 SDK pinned by `global.json` to `10.0.100`
- Java 17 or newer for ANTLR grammar compilation
- The ANTLR runtime package and the jar at `src/parser/tools/antlr-4.13.2-complete.jar`
:::

## Essential Commands

:::rule id="BUILD-002" severity="error" category="commands" domain="build"
Use the solution restore command as the standard restore entry point:

```bash
dotnet restore fifthlang.sln
```

This operation typically takes about 70 seconds. Do not cancel it. Use a timeout of at least 120 seconds when automation controls execution time.
:::

:::rule id="BUILD-003" severity="error" category="commands" domain="build"
Use the solution build command as the standard build entry point:

```bash
dotnet build fifthlang.sln
```

This operation typically takes about 60 seconds. Do not cancel it. Use a timeout of at least 120 seconds when automation controls execution time.
:::

:::rule id="BUILD-004" severity="error" category="testing" domain="build"
Use the solution test command as the default regression gate:

```bash
dotnet test fifthlang.sln
```

Do not cancel this run. Use a timeout of at least 5 minutes when automation controls execution time.
:::

:::rule id="BUILD-005" severity="warning" category="testing" domain="build"
Use this command for a quick smoke-test subset while iterating locally:

```bash
dotnet test test/ast-tests/ast_tests.csproj
```
:::

:::rule id="BUILD-006" severity="error" category="generation" domain="build"
After metamodel changes, regenerate AST output with:

```bash
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```
:::

## Verification

:::rule id="BUILD-007" severity="warning" category="verification" domain="build"
Confirm the toolchain before debugging restore or build failures:

```bash
dotnet --version
java -version
```

The .NET command should report `10.0.x`, and Java should report version 17 or newer.
:::

## Build Order Dependencies

:::rule id="BUILD-008" severity="error" category="dependency" domain="build"
The effective build order is:

```text
ast-model -> ast_generator -> ast-generated -> parser -> compiler -> tests
```

Always build the full solution rather than individual projects so dependency ordering is resolved correctly.
:::

## Critical Rules

:::rule id="BUILD-009" severity="error" category="workflow" domain="build"
Do not cancel restore, build, test, or generation operations. The documented timings in this repository are normal and expected.
:::

:::rule id="BUILD-010" severity="warning" category="parser" domain="build"
ANTLR grammar compilation happens automatically during the parser project build. Do not add redundant manual generation steps to the normal workflow.
:::

:::rule id="BUILD-011" severity="warning" category="generation" domain="build"
AST code generation runs automatically before compilation via MSBuild targets. Manual generation is primarily for focused regeneration workflows.
:::

:::rule id="BUILD-012" severity="warning" category="diagnostics" domain="build"
The following warnings are expected and safe to ignore unless they change unexpectedly:

- ANTLR `assoc` option warnings
- C# nullable reference warnings
- Switch exhaustiveness warnings
:::

## Validation Protocol (after any change)

:::rule id="BUILD-013" severity="error" category="validation" domain="build"
After any change, validate in this order:

1. `dotnet build fifthlang.sln`
2. `dotnet test fifthlang.sln`
3. Verify runtime behavior

Compilation alone is not sufficient validation.
:::

## Granular Test Targets

:::rule id="BUILD-014" severity="warning" category="testing" domain="build"
These targeted commands are available for narrower local validation:

```bash
just test-ast
just test-runtime
just test-syntax
just test-all-roslyn
```

Use them to iterate locally, but retain the full solution test run as the regression gate.
:::
