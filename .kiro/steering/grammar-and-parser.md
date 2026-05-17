---
description: grammar-and-parser
inclusion: always
---
## Architecture
- GRAM-001: The parser surface is divided across three primary assets:

- `src/parser/grammar/FifthLexer.g4` for tokens, keywords, literals, and lexical structure
- `src/parser/grammar/FifthParser.g4` for syntactic rules and grammar structure
- `src/parser/AstBuilderVisitor.cs` for parse-tree to high-level AST transformation
## Workflow
- GRAM-002: When grammar behavior changes:

1. Edit `FifthLexer.g4` for tokens and keywords and `FifthParser.g4` for syntax rules as needed
2. Update `AstBuilderVisitor.cs` for the new syntax constructs
3. Add test samples under `src/parser/grammar/test_samples/*.5th`
4. Rely on the normal build to run ANTLR compilation automatically
5. Run parser tests with `dotnet test test/syntax-parser-tests/ -v minimal`
6. Run the full regression suite with `dotnet test fifthlang.sln`
## Validation
- GRAM-003: All `.5th` files in `docs/`, `specs/`, `test/`, and `src/parser/grammar/test_samples/` must parse with the current grammar. CI enforces this with the `Validate .5th samples (parser-check)` step.

Run `just validate-examples` locally before committing.
- GRAM-008: Intentionally invalid files are excluded from example validation by these heuristics:

- directory matches under `*/Invalid/*`
- filenames containing `invalid`
- an explicit negative-test comment marker in the file

For debugging, force validation of negative examples with:

```bash
dotnet run --project src/tools/validate-examples/validate-examples.csproj -- --include-negatives
```
## Syntax
- GRAM-004: Do not use `var <name> =` in examples or tests. Use `name: type =` or the appropriate canonical Fifth form.
- GRAM-005: Do not use declarations such as `graph g =` or `triple t =`. Use `g: graph =` or `t: triple =`.
- GRAM-006: Do not use the legacy `when` guard shorthand. Use the parameter constraint form `param: Type | <expr>` together with block bodies.
## Guards
- GRAM-007: The canonical contrast for guard syntax is:

```fifth
// INVALID
myprint(int x) when x == 0 => std.print(x);

// VALID
myprint(int x | x == 0) { std.print(x); }
```
## Knowledge Graph
- GRAM-009: Use these canonical knowledge-graph forms in examples and tests:

- `name: store = sparql_store(<iri>);`
- `store default = sparql_store(<iri>);`
- `KG.CreateGraph()` to create graphs
- `graph += triple` to add triples

Validate these flows with `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj`.
