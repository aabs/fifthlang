# Data Model: TriG Literal Expression

## Entities

### TriGLiteralExpression (AST Node)
- Purpose: Represents an inline TriG dataset literal with optional interpolations.
- Fields (conceptual):
  - `ContentSpan`: source range for the full literal payload (inside `@<` and matching `>`)
  - `RawText`: original TriG text including braces and whitespace
  - `Interpolations`: ordered collection of interpolation entries
- Validation Rules:
  - Must have balanced top-level `@<` ... `>` delimiters
  - Interpolation delimiters must be balanced; triple braces map to literal braces
  - No unsupported interpolation result types (see serialization contract)

### InterpolationEntry
- Purpose: Captures a single `{{ ... }}` embedded expression.
- Fields (conceptual):
  - `Expression`: AST of the embedded expression
  - `Start`, `End`: source positions within the literal
- Validation Rules:
  - Expression must type-check in surrounding lexical scope

### Store (Runtime Value)
- Purpose: The evaluated form of the TriG literal expression; an RDF dataset container.
- Behavior: On evaluation, interpolations are serialized into a TriG string; the TriG is parsed into a `Store`.

## Relationships
- A `TriGLiteralExpression` contains zero or more `InterpolationEntry`.
- Evaluation of `TriGLiteralExpression` produces a `Store` value.

## Serialization Rules (Summary)
- Strings → quoted and escaped
- Numbers (int/float/decimal) → bare
- Booleans → bare `true`/`false`
- Date/time → `"lexical"^^xsd:dateTime`
- IRIs → absolute as `<iri>`; prefixed names as-is
- Literal braces → `{{{` → `{{`, `}}}` → `}}`

## State & Transitions
- Construction: Created during AST build when encountering `@< ... >` in parser.
- Lowering: Rewritten into a sequence that constructs a `Store`, builds an interpolated raw string, and loads it via TriG parser.
- Error: On invalid TriG or serialization failure, compilation emits diagnostics referencing the literal span.
