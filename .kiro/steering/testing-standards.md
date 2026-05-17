---
description: testing-standards
inclusion: always
---
## Framework
- TEST-001: The standard test stack is:

- `xUnit` as the test framework
- `FluentAssertions` for assertions
- `test/ast-tests/`, `test/syntax-parser-tests/`, and `test/runtime-integration-tests/` as the primary test projects
## Process
- TEST-002: Practice TDD by writing tests, seeing them fail, and then implementing the change. Never mask failing tests with broad `try` or `catch` blocks. Let failures surface so CI reflects the true repository state.
## Completion
- TEST-003: A feature is not complete until end-to-end tests prove that it:

1. Uses actual Fifth language syntax including constructs such as TriG literals, SPARQL literals, and operators
2. Executes successfully at runtime rather than merely compiling
3. Produces results that are accessible and correct
4. Exercises the major code paths and result types involved

Features with only compilation tests or with failing runtime tests are incomplete.
## Design
- TEST-004: Prefer property-based testing over single-point scenarios, and aim to verify corner cases rather than only happy paths.
- TEST-005: Avoid testing internal implementation details and avoid depending on concrete implementations where looser behavioral validation is possible.
## Fixtures
- TEST-006: Tests that reference `.5th` sample files must declare `CopyToOutputDirectory` metadata in the owning test `.csproj`.
## Commands
- TEST-007: The default regression command is:

```bash
dotnet test fifthlang.sln
```
- TEST-008: Use this quick smoke command while iterating:

```bash
dotnet test test/ast-tests/ast_tests.csproj
```
- TEST-009: Use this focused parser command when grammar behavior changes:

```bash
dotnet test test/syntax-parser-tests/ -v minimal
```
- TEST-010: Use filtered runtime integration tests for focused investigation:

```bash
dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~YourTestName" -v minimal
```
- TEST-011: Validate knowledge-graph changes with:

```bash
dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj
```
## Ast
- TEST-012: Use this quick smoke test after AST builder changes:

```csharp
using ast;
using ast_generated;

var intLiteral = new Int32LiteralExp { Value = 42 };
var builder = new Int32LiteralExpBuilder();
var result = builder.Build();
```

The builder construction should complete without errors.
