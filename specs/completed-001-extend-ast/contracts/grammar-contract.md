# Grammar Contract: Graph Assertion Block

## Lexer Assumptions
- Tokens `<{` and `}>` already exist and are unique to graph assertion blocks.

## Parser Rules
- `graphAssertionBlock` → `GRAPH_LBRACE statements? GRAPH_RBRACE`
  - Where `GRAPH_LBRACE` = `<{` and `GRAPH_RBRACE` = `}>`
- Allowed contexts:
  - As a statement (in any block where a regular block could appear)
  - As a primary expression
- Nesting: Any block type can nest any other (regular blocks, graph assertion blocks, etc.).

## Ambiguity Constraints
- No overlap between `<{` / `}>` and normal `{` / `}`
- `graphAssertionBlock` participates in expression precedence as a primary

## Example Snippets
- Statement form:
```
<{ 
  eric.dob = new datetime(1926, 5, 14);
  eric.age = calculate_age(d);
}>;
```
- Expression form with assignment and persist:
```
let g = <{ eric.dob = d; eric.age = calculate_age(d); }>;
home += g;
```
