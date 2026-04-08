---
id: steering-testing-standards
title: Testing Standards
inclusion: fileMatch
fileMatchPattern: "test/**"
---

# Testing Standards

## Framework and Tools

:::rule id="TEST-001" severity="warning" category="framework" domain="testing"
The standard test stack is:

- `xUnit` as the test framework
- `FluentAssertions` for assertions
- `test/ast-tests/`, `test/syntax-parser-tests/`, and `test/runtime-integration-tests/` as the primary test projects
:::

## Test-First (Non-Negotiable)

:::rule id="TEST-002" severity="error" category="process" domain="testing"
Practice TDD by writing tests, seeing them fail, and then implementing the change. Never mask failing tests with broad `try` or `catch` blocks. Let failures surface so CI reflects the true repository state.
:::

## Compilation is NOT Sufficient

:::rule id="TEST-003" severity="error" category="completion" domain="testing"
A feature is not complete until end-to-end tests prove that it:

1. Uses actual Fifth language syntax including constructs such as TriG literals, SPARQL literals, and operators
2. Executes successfully at runtime rather than merely compiling
3. Produces results that are accessible and correct
4. Exercises the major code paths and result types involved

Features with only compilation tests or with failing runtime tests are incomplete.
:::

## Test Design Principles

:::rule id="TEST-004" severity="warning" category="design" domain="testing"
Prefer property-based testing over single-point scenarios, and aim to verify corner cases rather than only happy paths.
:::

:::rule id="TEST-005" severity="warning" category="design" domain="testing"
Avoid testing internal implementation details and avoid depending on concrete implementations where looser behavioral validation is possible.
:::

:::rule id="TEST-006" severity="error" category="fixtures" domain="testing"
Tests that reference `.5th` sample files must declare `CopyToOutputDirectory` metadata in the owning test `.csproj`.
:::

## Running Tests

:::rule id="TEST-007" severity="warning" category="commands" domain="testing"
The default regression command is:

```bash
dotnet test fifthlang.sln
```
:::

:::rule id="TEST-008" severity="warning" category="commands" domain="testing"
Use this quick smoke command while iterating:

```bash
dotnet test test/ast-tests/ast_tests.csproj
```
:::

:::rule id="TEST-009" severity="warning" category="commands" domain="testing"
Use this focused parser command when grammar behavior changes:

```bash
dotnet test test/syntax-parser-tests/ -v minimal
```
:::

:::rule id="TEST-010" severity="warning" category="commands" domain="testing"
Use filtered runtime integration tests for focused investigation:

```bash
dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~YourTestName" -v minimal
```
:::

:::rule id="TEST-011" severity="warning" category="commands" domain="testing"
Validate knowledge-graph changes with:

```bash
dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj
```
:::

## AST Smoke Test

:::rule id="TEST-012" severity="warning" category="ast" domain="testing"
Use this quick smoke test after AST builder changes:

```csharp
using ast;
using ast_generated;

var intLiteral = new Int32LiteralExp { Value = 42 };
var builder = new Int32LiteralExpBuilder();
var result = builder.Build();
```

The builder construction should complete without errors.
:::
