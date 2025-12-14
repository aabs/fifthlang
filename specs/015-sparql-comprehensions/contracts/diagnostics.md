# Contracts: Diagnostics (SPARQL Comprehensions)

This feature introduces new compile-time diagnostics for comprehension validation. Codes and exact messages should be stable.

## Proposed Diagnostic Codes

### `LCOMP001` — Invalid generator type

- **When**: generator does not typecheck to a tabular SELECT result (for SPARQL comprehensions).
- **Example**: `from (?<ASK {...}> <- store)` or `from someNonTabularValue`

### `LCOMP002` — Non-SELECT SPARQL query in generator

- **When**: generator expression is (or contains) a SPARQL literal/query whose parsed form is not SELECT.
- **Note**: This is a compile-time check using the ANTLR SPARQL grammar (`SparqlParser.g4`).

### `LCOMP003` — Unknown SPARQL variable

- **When**: object projection uses `?varName` that is not projected by the SELECT clause.
- **Example**: query projects `?name` but projection uses `?age`.

### `LCOMP004` — Non-boolean constraint

- **When**: a `where` constraint expression does not typecheck to boolean.

### `LCOMP005` — Invalid object projection binding

- **When**: in a SPARQL object projection, a property initializer RHS is not a SPARQL variable token `?varName`.
- **Example**: `new Person() { Name = "bob" }` inside a SPARQL comprehension.

### `LCOMP006` — Unknown property in projection

- **When**: projection references a property that does not exist on the projected type.

### `LCOMP007` — Legacy comprehension syntax rejected

- **When**: parser encounters `in`/`#` comprehension syntax.
- **Note**: This may surface as a parse error rather than a compiler diagnostic depending on implementation.

## Source Location

Diagnostics should include:
- file, line, column (when available)
- the relevant source span (optional)

## Relationship to Existing `SPARQL00X`

Existing `SPARQL00X` diagnostics are currently oriented around SPARQL literal interpolation/size and token-level checks.
This feature’s diagnostics are comprehension-specific and should not overload `SPARQL00X` unless there is a strong reason.
