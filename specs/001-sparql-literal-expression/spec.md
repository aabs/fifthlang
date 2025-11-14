# Feature Specification: Embedded SPARQL Queries

**Feature Branch**: `001-sparql-literal-expression`  
**Created**: 2025-11-13  
**Status**: Draft  
**Input**: User description: "Literal Expression AST type for SPARQL Queries"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Author SPARQL as a literal (Priority: P1)

As a developer, I can write an inline SPARQL literal using `?< ... >` that produces a value assignable to variables of type `Query` so I can author queries concisely in Fifth code.

**Why this priority**: Enables the core value of embedding SPARQL natively; all other functionality builds on this.

**Independent Test**: Create a minimal program declaring `q: Query = ?<SELECT * WHERE { }>;` and verify compilation succeeds and the AST node is of the new literal type.

**Acceptance Scenarios**:

1. Given a valid SPARQL SELECT inside `?< ... >`, When compiling, Then the literal parses and type-checks to `Query`.
2. Given an empty literal `?<>`, When compiling, Then it type-checks to `Query` and yields a valid empty query object.

---

### User Story 2 - Bind variables via parameters (Priority: P1)

As a developer, I can reference in-scope Fifth variables directly (e.g., `age`) within a SPARQL literal, and they are safely bound as typed parameters to the underlying query so I don’t have to build strings manually.

**Why this priority**: Provides safe variable binding and prevents injection; matches dotNetRDF capability.

**Independent Test**: Declare `age: int = 42; q: Query = ?<SELECT * WHERE { ?s ex:age age }>;` and verify the bound parameter exists and compiles without diagnostic errors.

**Acceptance Scenarios**:

1. Given an in-scope variable `age: int`, When referenced in a SPARQL literal body, Then the compiler binds it as a typed parameter to the query.
2. Given an out-of-scope identifier inside the SPARQL, When compiling, Then a compile-time diagnostic reports unknown variable.

---

### User Story 3 - Interpolation placeholders (Priority: P1)

As a developer, I can use `{{expr}}` inside a SPARQL literal for value interpolation of expressions, with correct typing and escaping, to compose queries when parameter binding is insufficient.

**Why this priority**: Essential for computed values and dynamic query construction; complements parameter binding for full expressiveness.

**Independent Test**: Declare `age: int = 42; q: Query = ?<SELECT * WHERE { ?s ex:age {{age}} }>;` and verify the resulting query is valid and safe.

**Acceptance Scenarios**:

1. Given a valid expression inside `{{ ... }}`, When compiling, Then the value is injected as a typed literal or IRI safely into the query text.
2. Given a malformed or non-constant expression inside `{{ ... }}`, When compiling, Then a diagnostic points to the interpolation site.

---

### Edge Cases

- Empty literal body `?<>` compiles and yields a minimal query instance.
- Malformed SPARQL text produces a compile-time diagnostic with location within the literal.
- Unknown variable names in the SPARQL body (not via `{{ }}`) produce compile-time diagnostics.
- Excessive literal size (e.g., >1MB) produces a friendly diagnostic suggesting external files.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The grammar MUST recognize `sparqlLiteral` via tokens `SPARQL_START` (`?<`) and `SPARQL_CLOSE_ANGLE` (`>`), and include it in `literal` alongside existing primitives and TriG.
- **FR-002**: The parser MUST construct a new AST node type `SparqlLiteralExpression` representing the literal body and any discovered variable references and interpolation placeholders.
- **FR-003**: The type system MUST map `SparqlLiteralExpression` to compile-time type `Fifth.System.Query` (user surface `Query`).
- **FR-004**: Variable identifiers present in the SPARQL body that match in-scope Fifth variables MUST be bound as parameters on a prepared query object (no textual splicing) using a safe parameterization model.
- **FR-005**: Interpolation placeholders `{{expr}}` inside the literal MUST be supported for value-only insertion (expressions evaluating to IRIs or literals), serialized safely as typed values.
- **FR-006**: The compiler MUST validate SPARQL content (query or update) at compile-time by parsing via the embedded SPARQL grammar, allowing variable tokens/placeholders, and emit precise diagnostics for syntax errors.
- **FR-007**: Unknown variables referenced in the SPARQL body (non-interpolated) MUST produce compile-time diagnostics identifying the missing binding name.
- **FR-008**: The system MUST prevent injection by never concatenating raw user-controlled text into structural SPARQL; all values MUST be passed as typed parameters or safe-serialized interpolation values.
- **FR-009**: The resulting literal value MUST be usable anywhere a `Query` is expected, including assignment, function parameters, and return values.
- **FR-010**: Tooling and tests MUST cover parse success for valid examples, failures for malformed examples, and variable binding resolution in both direct-reference and interpolation forms.

### Key Entities *(include if feature involves data)*

- **SparqlLiteralExpression**: AST node capturing raw SPARQL text segments, variable reference table, interpolation segments, and source spans.
- **Query**: Surface type mapped to system `System.Fifth.Query`, representing a compiled SPARQL query or update.
- **Binding**: A name-to-value association for in-scope variables referenced inside the literal (sourced from Fifth variables or `{{expr}}`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of valid sample literals compile to `Query` with zero diagnostics.
- **SC-002**: 100% of malformed SPARQL samples produce at least one diagnostic pointing within the literal body.
- **SC-003**: Variable binding works in 95%+ of representative cases (identifiers, IRIs, typed literals) verified by unit tests.
- **SC-004**: No security regressions: zero instances of raw concatenation of untrusted values into SPARQL verified by code scan and tests.
