---
id: steering-fifth-language-syntax
title: Fifth Language Syntax Reference
inclusion: manual
---

# Fifth Language Syntax Reference

## Basic Syntax

:::rule id="SYN-001" severity="warning" category="syntax" domain="syntax"
Basic syntax looks like this:

```fifth
class Person {
    Name: string;
    Height: float;
}

main() => myprint(5 + 6);
myprint(int x) => std.print(x);
```
:::

## Variable Declarations

:::rule id="SYN-002" severity="error" category="syntax" domain="syntax"
Use `name: type = value` form. Never use `var name =` in C# or JavaScript style, and never use type-first forms such as `type name =`.

```fifth
x: int = 42;
g: graph = KG.CreateGraph();
```
:::

## Function Definitions

:::rule id="SYN-003" severity="warning" category="functions" domain="syntax"
Function definitions can use either expression bodies or block bodies:

```fifth
add(int a, int b) => a + b;

greet(string name) {
    std.print("Hello " + name);
}
```
:::

## Parameter Constraints (Guards)

:::rule id="SYN-004" severity="error" category="guards" domain="syntax"
Use the parameter constraint form with block bodies:

```fifth
myprint(int x | x == 0) { std.print(x); }
```

Do not use the legacy `when` shorthand.

```fifth
// INVALID
// myprint(int x) when x == 0 => std.print(x);
```
:::

## Knowledge Graph Constructs

:::rule id="SYN-005" severity="warning" category="knowledge-graph" domain="syntax"
Use the canonical knowledge-graph forms:

```fifth
myStore: store = sparql_store(<http://example.org/store>);
store default = sparql_store(<http://example.org/default>);

g: graph = KG.CreateGraph();
// Add triples with += operator
```
:::

## TriG and SPARQL Literals

:::rule id="SYN-006" severity="warning" category="knowledge-graph" domain="syntax"
Use these literal forms in syntax and examples:

- TriG literals use `<{...}>`
- SPARQL literals use `?<...>`
- Object-position literal values may be strings, booleans, chars, signed integers, unsigned integers, `float`, `double`, or `decimal`
:::

## Sample Files

:::rule id="SYN-007" severity="warning" category="reference" domain="syntax"
Use these locations when looking for canonical syntax examples:

- `test/ast-tests/CodeSamples/*.5th`
- `src/parser/grammar/test_samples/*.5th`
- `docs/Getting-Started/`
:::
