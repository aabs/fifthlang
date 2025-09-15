# fifthlang

![CI](https://github.com/aabs/fifthlang/actions/workflows/ci.yml/badge.svg)

CI runs on Linux, macOS, and Windows for pushes/PRs to `master`, plus a nightly schedule. TRX test results are uploaded as workflow artifacts.

Coverage: CI collects cross-platform coverage via XPlat Code Coverage; HTML and Cobertura reports are uploaded as artifacts per-OS.

A compiler and language tooling for the Fifth programming language.

## Quickstart
```fifth
// Declare a default SPARQL store (canonical syntax)
store default = sparql_store(<http://example.org/store>);

main(): int {
  // Statement-form graph block: lowers to SaveGraph(default, graph)
  <{
    <http://example.org/s> <http://example.org/p> "o";
  }>;
  return 0;
}
```

## Prerequisites
- .NET SDK 8.0+
- Java 17+ (ANTLR toolchain; 11 may work but 17 is recommended)
- `ilasm` on PATH or configured via env vars (see `src/compiler/README.md`)

## Build
```fish
# From repo root
 dotnet build fifthlang.sln
 # Or with Make (runs restore + build fast)
 make build-all
```

## Tests
```fish
# List and run tests for the whole solution
 dotnet test fifthlang.sln --list-tests
 dotnet test fifthlang.sln

# Run a specific test project
 dotnet test test/ast-tests/ast_tests.csproj
 dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj

## Coverage
- Local run with Cobertura output (aligned with CI):
```fish
dotnet test fifthlang.sln --no-build --collect "XPlat Code Coverage" --logger "trx;LogFileName=results.trx" --settings fifth.runsettings
# Locate outputs
find test -type f -name '*.trx' | sed -n '1,20p'
find . -type f -name 'coverage.cobertura.xml' | sed -n '1,20p'
```
- Optional HTML report (requires ReportGenerator):
```fish
dotnet tool install -g dotnet-reportgenerator-globaltool
set -gx PATH $HOME/.dotnet/tools $PATH
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:CoverageReport -reporttypes:Html;TextSummary
open CoverageReport/index.html 2>/dev/null; and echo "Opened report in browser"
```
```

## VS Code
- Recommended extensions are defined in `.vscode/extensions.json`.
- Dev Kit Testing UI with TUnit:
  - See `docs/vscode-devkit-tests.md` for setup and discovery steps.

## Knowledge Graphs (Overview)
- Canonical store declaration: `name : store = sparql_store(<iri>);` or `store default = sparql_store(<iri>);`
- Graph assertion blocks: `<{ ... }>`
  - Expression-form yields an `IGraph` value.
  - Statement-form requires a default store and saves the graph to it.
- Built-ins are provided via `Fifth.System.KG` (e.g., `CreateGraph`, `CreateUri`, `CreateLiteral`, `CreateTriple`, `Assert`, `SaveGraph`).
- Supported object literals include strings, booleans, chars, all signed/unsigned integers, float, double, and precise `decimal`.

## Repo layout
- `src/` – language, parser, code generator, compiler
- `test/` – unit and integration tests
- `docs/` – guides and notes (Dev Kit testing setup, etc.)

## License
See `src/LICENSE`.
