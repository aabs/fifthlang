# Quickstart: Implementation of try/catch/finally

## Prerequisites
- .NET 8.0 SDK (global.json pins 8.0.118)
- Java 17+ (ANTLR)

## Build & Test
```fish
# From repo root
dotnet restore fifthlang.sln
dotnet build fifthlang.sln

# Regenerate AST after metamodel updates
just run-generator  # or: dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

# Run tests
# Default for regression checks: full suite
dotnet test fifthlang.sln -v minimal

# Optional focused runs while iterating locally
dotnet test test/syntax-parser-tests/ -v minimal
dotnet test test/ast-tests/ -v minimal
```

## Validation for This Feature
```fish
# Parser samples for try/catch/finally and throw expressions
# (samples to be added under src/parser/grammar/test_samples/)

# Macrobench performance gate (no measurable regression)
# See project docs for macrobench harness; ensure p >= 0.05 vs baseline

# IL baseline comparisons
# Ensure SDK used matches global.json; update baselines if SDK pin changes
```

## Notes
- Never edit files under `src/ast-generated/` manually; change `src/ast-model/*` and regenerate.
- Reserve keywords `try`, `catch`, `finally`, `when`.
- Iterators/async-iterators are deferred in v1 (diagnostic if used with try/catch/finally).
- Throw expressions are supported (e.g., `x ?? throw new E()`).
