---
id: steering-grammar-and-parser
title: Grammar And Parser Rules
inclusion: fileMatch
fileMatchPattern: "src/parser/**,docs/**/*.5th,test/**/*.5th,src/parser/grammar/**"
---

# Grammar and Parser Rules

## Split Grammar Architecture

:::rule id="GRAM-001" severity="warning" category="architecture" domain="parser"
The parser surface is divided across three primary assets:

- `src/parser/grammar/FifthLexer.g4` for tokens, keywords, literals, and lexical structure
- `src/parser/grammar/FifthParser.g4` for syntactic rules and grammar structure
- `src/parser/AstBuilderVisitor.cs` for parse-tree to high-level AST transformation
:::

## Grammar Change Workflow

:::rule id="GRAM-002" severity="error" category="workflow" domain="parser"
When grammar behavior changes:

1. Edit `FifthLexer.g4` for tokens and keywords and `FifthParser.g4` for syntax rules as needed
2. Update `AstBuilderVisitor.cs` for the new syntax constructs
3. Add test samples under `src/parser/grammar/test_samples/*.5th`
4. Rely on the normal build to run ANTLR compilation automatically
5. Run parser tests with `dotnet test test/syntax-parser-tests/ -v minimal`
6. Run the full regression suite with `dotnet test fifthlang.sln`
:::

## Grammar Compliance for Examples and Tests

:::rule id="GRAM-003" severity="error" category="validation" domain="parser"
All `.5th` files in `docs/`, `specs/`, `test/`, and `src/parser/grammar/test_samples/` must parse with the current grammar. CI enforces this with the `Validate .5th samples (parser-check)` step.

Run `just validate-examples` locally before committing.
:::

## Common Non-Fifth Patterns to Avoid

:::rule id="GRAM-004" severity="error" category="syntax" domain="parser"
Do not use `var <name> =` in examples or tests. Use `name: type =` or the appropriate canonical Fifth form.
:::

:::rule id="GRAM-005" severity="error" category="syntax" domain="parser"
Do not use declarations such as `graph g =` or `triple t =`. Use `g: graph =` or `t: triple =`.
:::

:::rule id="GRAM-006" severity="error" category="syntax" domain="parser"
Do not use the legacy `when` guard shorthand. Use the parameter constraint form `param: Type | <expr>` together with block bodies.
:::

## Canonical Guard Syntax

:::rule id="GRAM-007" severity="error" category="guards" domain="parser"
The canonical contrast for guard syntax is:

```fifth
// INVALID
myprint(int x) when x == 0 => std.print(x);

// VALID
myprint(int x | x == 0) { std.print(x); }
```
:::

## Negative Tests

:::rule id="GRAM-008" severity="warning" category="validation" domain="parser"
Intentionally invalid files are excluded from example validation by these heuristics:

- directory matches under `*/Invalid/*`
- filenames containing `invalid`
- an explicit negative-test comment marker in the file

For debugging, force validation of negative examples with:

```bash
dotnet run --project src/tools/validate-examples/validate-examples.csproj -- --include-negatives
```
:::

## Knowledge Graph Syntax

:::rule id="GRAM-009" severity="warning" category="knowledge-graph" domain="parser"
Use these canonical knowledge-graph forms in examples and tests:

- `name: store = sparql_store(<iri>);`
- `store default = sparql_store(<iri>);`
- `KG.CreateGraph()` to create graphs
- `graph += triple` to add triples

Validate these flows with `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj`.
:::
