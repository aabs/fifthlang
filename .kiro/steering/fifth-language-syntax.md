---
description: fifth-language-syntax
inclusion: always
---
## Syntax
- SYN-001: Basic syntax looks like this:

```fifth
class Person {
    Name: string;
    Height: float;
}

main() => myprint(5 + 6);
myprint(int x) => std.print(x);
```
- SYN-002: Use `name: type = value` form. Never use `var name =` in C# or JavaScript style, and never use type-first forms such as `type name =`.

```fifth
x: int = 42;
g: graph = KG.CreateGraph();
```
## Functions
- SYN-003: Function definitions can use either expression bodies or block bodies:

```fifth
add(int a, int b) => a + b;

greet(string name) {
    std.print("Hello " + name);
}
```
## Guards
- SYN-004: Use the parameter constraint form with block bodies:

```fifth
myprint(int x | x == 0) { std.print(x); }
```

Do not use the legacy `when` shorthand.

```fifth
// INVALID
// myprint(int x) when x == 0 => std.print(x);
```
## Knowledge Graph
- SYN-005: Use the canonical knowledge-graph forms:

```fifth
myStore: store = sparql_store(<http://example.org/store>);
store default = sparql_store(<http://example.org/default>);

g: graph = KG.CreateGraph();
// Add triples with += operator
```
- SYN-006: Use these literal forms in syntax and examples:

- TriG literals use `<{...}>`
- SPARQL literals use `?<...>`
- Object-position literal values may be strings, booleans, chars, signed integers, unsigned integers, `float`, `double`, or `decimal`
## Reference
- SYN-007: Use these locations when looking for canonical syntax examples:

- `test/ast-tests/CodeSamples/*.5th`
- `src/parser/grammar/test_samples/*.5th`
- `docs/Getting-Started/`
