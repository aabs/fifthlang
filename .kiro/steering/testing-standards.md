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
- TEST-002 [MANDATORY]: Practice TDD by writing tests, seeing them fail, and then implementing the change. Never mask failing tests with broad `try` or `catch` blocks. Let failures surface so CI reflects the true repository state.
## Completion
- TEST-003: A feature is not complete until end-to-end tests prove that it:

1. Uses actual Fifth language syntax including constructs such as TriG literals, SPARQL literals, and operators
2. Executes successfully at runtime rather than merely compiling
3. Produces results that are accessible and correct
4. Exercises the major code paths and result types involved

Features with only compilation tests or with failing runtime tests are incomplete.
## Testing
- TEST-004 [MANDATORY]: Test code should use property-based testing (PBT) using FsCheck.xunit in preference to unit tests.
- TEST-004a [MANDATORY]: Never just test single-point scenarios and  happy paths, instead use a property-based test that will test all positive, negative and edge cases.
- TEST-005: Avoid testing internal implementation details and avoid depending on concrete implementations where looser behavioral validation is possible.
- TEST-013 [MANDATORY]: Define properties as universal rules that must hold for all valid inputs, rather than relying on specific example cases.
- TEST-014 [MANDATORY]: Design generators to produce diverse, realistic, and edge-case inputs across the full input space.
- TEST-015 [MANDATORY]: Ensure failing cases can be minimized automatically through shrinking to aid debugging.
- TEST-016 [MANDATORY]: Specify preconditions clearly or constrain generators so properties are only evaluated in valid domains.
- TEST-017 [MANDATORY]: Keep tests deterministic and reproducible by controlling randomness and eliminating hidden state or side effects.
- TEST-018 [MANDATORY]: Use strong oracles, models, or metamorphic relationships to validate correctness beyond simple assertions.

- Every property must have an oracle: a mechanical way to decide pass/fail that is stronger than “doesn’t throw” or “looks plausible”.
- Prefer a reference (spec) model oracle when you can: compute expected behaviour using a simpler, obviously-correct implementation and compare.
- If you can’t compute the exact expected output, use a metamorphic oracle: apply a transformation to inputs and assert a predictable relationship between outputs.
- Use multiple weak oracles together (invariants + metamorphic + cross-check) rather than one weak check.
- Fail with evidence: when a property fails, ensure the counterexample is informative (shrinks well; includes classification/labels).
- TEST-019 [MANDATORY]: When making significant changes to a pre-existing unit test, convert it to a Property based test that tests a whole class of invariants and pre and post conditions. 
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
