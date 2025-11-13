# Quickstart: TriG Literal Expression

## Build & Test

```fish
# From repository root
 dotnet restore fifthlang.sln
 dotnet build fifthlang.sln
 dotnet test test/ast-tests/ast_tests.csproj
```

## Example

```fifth
age: int = 21;
s: Store = @<
  @prefix ex: <http://example.org/> .

  ex:graph1 {
    ex:Andrew rdf:type ex:Person;
    ex:name "Andrew";
    ex:age {{ age }} .
  }
>;
```

Expected behavior:
- `s` contains a dataset with the prefixes and triples declared.
- Interpolations are serialized per rules: numbers bare, strings quoted.

## Notes
- Use `{{{` and `}}}` to render literal `{{` and `}}` inside the TriG payload.
- Absolute IRIs: `<http://example.org/Thing>`; prefixed names: `ex:Thing`.
