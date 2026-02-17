# Contract: TriG Literal Expression

This contract describes the observable behavior of the TriG Literal Expression feature for consumers of the Fifth language and its toolchain.

## Surface Syntax
- Start delimiter: `@<`
- End delimiter: matching top-level `>` corresponding to the opening `@<`
- Interpolations: `{{ expression }}`
- Literal braces: `{{{` → `{{`; `}}}` → `}}`

## Type
- Expression evaluates to: `Store`
- Assignability: May be assigned to variables of type `Store`

## Interpolation Serialization
- Strings → quoted with escaping per Turtle
- Numbers (int, float, decimal) → unquoted numeric
- Booleans → `true`/`false`
- Date/time → quoted lexical form + `^^xsd:dateTime`
- IRIs → absolute as `<...>`; prefixed names (e.g., `ex:Thing`) unquoted
- Unsupported values → compile-time diagnostic at interpolation site

## Diagnostics
- Unbalanced delimiters (`@< ... >`) → error with span at literal start
- Bad interpolation delimiter (`{{`/`}}`) → error with span at occurrence
- TriG parsing failure → error with line/column within the literal payload

## Lowering Behavior (observable)
- The literal is lowered into code that constructs a `Store`, builds an interpolated raw string, and loads it via a TriG parser.
- Whitespace and newlines are preserved exactly as written in the source literal.

## Backward/Forward Compatibility
- This construct is new; no legacy syntax is consumed.
- Future versions may add explicit helpers (`iri()`, `typed()`) without breaking existing code.
