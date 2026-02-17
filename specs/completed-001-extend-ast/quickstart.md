# Quickstart: Graph Assertion Block

## Prerequisites
- .NET 8 SDK and Java 17+

## Build & Generate
```fish
# From repo root
just build-all
# or
 dotnet restore fifthlang.sln
 dotnet build fifthlang.sln
```

## Run generator (if ast-model changes)
```fish
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

## Test
```fish
# Parser syntax tests
 dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj
# AST tests
 dotnet test test/ast-tests/ast_tests.csproj
# Runtime integration tests
 dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj
```

## Sample
```fifth
alias x as <http://example.com/blah#>;

store home = sparql_store(<http://localhost:8080/graphdb>);

class Person in <x:>
{
    dob : datetime ;
    age : int ;
}

let eric = new Person();
let g = <{ eric.dob = new datetime(1926, 5, 14); eric.age = calculate_age(eric.dob); }>;
home += g;  # explicit persist
```
