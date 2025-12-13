# Feature Specification: SPARQL Comprehensions

**Feature Branch**: `[015-sparql-comprehensions]`  
**Created**: 2025-12-13  
**Status**: Draft  
**Input**: User description: "SPARQL Comprehensions"

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Project objects from SPARQL results (Priority: P1)

As a Fifth developer, I want a concise comprehension syntax that turns a tabular SPARQL query result into a typed list of objects, so I can build domain objects without writing repetitive “row mapping” boilerplate.

**Why this priority**: This is the core value proposition—making SPARQL query results easy and idiomatic to consume.

**Independent Test**: Can be fully tested by running a program that executes a SELECT query over known sample data and evaluates a comprehension into a list of typed objects.

**Acceptance Scenarios**:

1. **Given** a tabular query result containing bindings for variables referenced by the comprehension projection, **When** the comprehension is evaluated, **Then** it returns a list of objects whose properties match the corresponding bindings for each result row.
2. **Given** a tabular query result with zero rows, **When** the comprehension is evaluated, **Then** it returns an empty list of the projected element type.

---

### User Story 2 - Filter rows with constraints (Priority: P2)

As a Fifth developer, I want to optionally add zero or more constraint expressions to the comprehension, so I can filter the result rows before projecting them into output values (or omit constraints entirely when I want all rows).

**Why this priority**: Filtering is a common need; it keeps queries and projections readable and avoids “post-filtering” boilerplate.

**Independent Test**: Can be tested independently by using the same query result and comparing the output list size/content with and without constraints.

**Acceptance Scenarios**:

1. **Given** a comprehension with multiple constraints, **When** it is evaluated, **Then** only rows that satisfy all constraints are included in the output list.
2. **Given** constraints that exclude all rows, **When** it is evaluated, **Then** it returns an empty list.
3. **Given** a comprehension with zero constraints, **When** it is evaluated, **Then** all rows from the source result are eligible for projection into the output list.

---

### User Story 3 - Clear errors for invalid comprehensions (Priority: P3)

As a Fifth developer, I want clear, actionable compilation errors when a SPARQL comprehension is invalid (e.g., wrong source type, missing variables, invalid constraints), so I can fix mistakes quickly.

**Why this priority**: A concise syntax needs strong diagnostics; otherwise users lose time debugging ambiguous failures.

**Independent Test**: Can be tested independently using negative samples that intentionally violate one rule at a time and asserting that compilation fails with the expected error category.

**Acceptance Scenarios**:

1. **Given** a comprehension whose generator is not a SPARQL tabular result, **When** compiling, **Then** compilation fails with an error that explains the required generator type.
2. **Given** a projection referencing a variable that is not present in the query result, **When** compiling, **Then** compilation fails with an error identifying the unknown variable.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- Empty query result produces an empty list (not null).
- Unbound/missing values in a row cause a clear runtime error when referenced by the projection or a `where` constraint.
- Multiple constraints are treated as a logical AND and may short-circuit when earlier constraints evaluate to false.
- A comprehension cannot have multiple generators in the same expression.
- Using a SPARQL comprehension with a non-tabular query result fails with a clear compilation error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The language MUST support a list comprehension form (“SPARQL Comprehension”) that produces a list from a tabular SPARQL SELECT query result.
- **FR-002**: The SPARQL Comprehension MUST support projecting each result row into a typed element value.
- **FR-003**: The projection MUST follow the list comprehension grammar defined in this spec (see Grammar) and MUST support either (a) projecting a single variable name or (b) projecting a typed object instance built from SPARQL variables.
- **FR-004**: A SPARQL Comprehension MUST have exactly one generator source.
- **FR-005**: The generator source expression MUST typecheck to a tabular (SELECT) SPARQL query result; using results of other SPARQL query forms MUST be rejected at compile time.
- **FR-006**: A SPARQL Comprehension MAY include zero or more constraints; when present, all constraints MUST be satisfied for a row to be included.
- **FR-007**: Each constraint expression MUST evaluate to a boolean outcome; otherwise compilation MUST fail with an error.
- **FR-008**: The language MUST provide a way to reference SPARQL result bindings inside the object-instantiation projection via `?varName` (see `SPARQL_VARNAME` in Grammar).
- **FR-009**: If a projection references a SPARQL variable that is not available in the generator result, compilation MUST fail with an error identifying the unknown variable.
- **FR-009a**: The compiler MUST use the SPARQL grammar (see Assumptions & Dependencies) to extract the set of projected SPARQL variables for SELECT queries and MUST validate variable usage in comprehensions at compile time (not runtime).
- **FR-010**: If a projection or constraint references a target property that does not exist on the projected type, compilation MUST fail with an error identifying the unknown property.
- **FR-011**: If the source result contains no matching rows, the comprehension MUST evaluate to an empty list of the projected type.
- **FR-011a**: If a projection or constraint references an unbound/missing SPARQL variable value in a row, evaluation MUST fail with a clear runtime error identifying the missing variable.
- **FR-012**: Any breaking change MUST follow the constitution's breaking-change process: it MUST include (a) a migration note describing what changed and how to update code, (b) updated tests reflecting the new behavior, and (c) a minor/major SemVer bump decision as appropriate.
- **FR-013**: The existing (general) list comprehension capability MUST be preserved with equivalent behavior, but with keyword updates: `in` is replaced by `from`, and the existing “such-that” filter marker `#` is replaced by `where`. The legacy `in` and `#` forms MUST be rejected with a clear parse/compile error.
- **FR-014**: The general list comprehension form and the SPARQL Comprehension form MUST be supported as alternate forms of the list comprehension feature.

### Assumptions & Dependencies

- A “tabular result” is defined as the output of a SPARQL SELECT query.
- Users will provide projected types whose properties can be assigned from the available result bindings.
- SPARQL variables referenced in object projections use `?varName` syntax.
- Comprehensions will continue to exist for non-SPARQL sources; SPARQL Comprehensions extend (rather than replace) comprehension usage.

- **Compile-time SPARQL introspection**: The type checker and language transformation phases MAY parse SPARQL queries at compile time using the ANTLR grammar in `src/parser/grammar/SparqlParser.g4`.
  - Determines whether a SPARQL query is a SELECT query (required for tabular results).
  - Extracts the list of projected SPARQL variables for validating comprehension projections.
  - Surfaces query-type and variable-name validation failures as compile-time compiler errors (not runtime errors).

### Grammar

The existing list comprehension syntax:

```antlr
list_comprehension:
  varname = var_name IN source = expression (
    SUCH_THAT constraint = expression
  );
```

Is replaced by the following breaking change:

```antlr
list_comprehension:
  lcomp_projection FROM source = lcomp_generator (
    WHERE constraints = lcomp_constraint_list
  )?;

// a projection can be either a simple variable taken from an enumerable,
// or it can be an object instantiation projection
lcomp_projection:
    varname = var_name    #lcomp_var
  | lcomp_proj_obj        #lcomp_objinst
  ;

// an object instantiation projection builds an object instance out of the variables
// available in a row of a SPARQL tabular result set.
lcomp_proj_obj:
  object_instantiation_expression;

// Semantic constraint for SPARQL object-instantiation projections:
// - Each property assignment RHS MUST be a SPARQL variable token `?varName` (SPARQL_VARNAME)
//   that is present in the tabular result set.

// an enumerator that contains the elements from which projection is taken.
// for object instantiation, this can only be a single `Result` type (from which elements are taken)
lcomp_generator:
  source = expression
  ;

lcomp_constraint_list:
  lcomp_constraint (COMMA lcomp_constraint)*
  ;

// a boolean type expression referencing the elements in the projection.
// See the section "Referencing Projected Properties within Constraints" for details on the rules of varrefs in constraints
lcomp_constraint:
  constraint = expression
  ;

// Lexer tokens

FROM: 'from' ;
SPARQL_VARNAME: '?' IDENTIFIER ;
```

### Key Entities *(include if feature involves data)*

- **Tabular Result**: A sequence of rows produced by a SPARQL SELECT query, where each row provides bindings for named variables.
- **Row Binding**: The value associated with a single SPARQL variable in a given row.
- **Comprehension Generator**: The single source value that provides rows to iterate over.
- **Projection**: The rule that turns a single row into one output element.
- **Constraint**: A predicate that determines whether a given row is included.

### Referencing Projected Properties within Constraints

- For variable projections, constraints reference the projected variable name (e.g., `[x from xs where x > 0]`).
- For object-instantiation projections, constraints reference projected properties via an implicit `it` value of the projected type (e.g., `where it.Age < 21, it.Age > 12`).
- In object-instantiation projections, `it.PropName` refers to the value that would be assigned to `PropName` for the current row. If the referenced value is unbound/missing for a row, evaluation MUST fail with the same clear runtime error described in **FR-011a**.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A representative program can project a non-empty tabular SELECT result into a non-empty typed list where every projected property value matches the corresponding result binding.
- **SC-002**: A representative program with constraints produces an output list that contains only items that satisfy all constraints.
- **SC-003**: A representative program where the query matches zero rows produces an empty list (and completes successfully).
- **SC-004**: Invalid comprehensions fail compilation with clear, actionable error messages covering at least: non-SELECT results, unknown variables, unknown properties, and non-boolean constraints.

## Clarifications

### Session 2025-12-13

- Q: For the general list-comprehension syntax change (currently `[x in xs # constraint]`), what should the compiler accept? → A: Breaking change now: only allow `from` and `where` (reject `in` and `#`).
- Q: Inside a SPARQL comprehension, how should SPARQL SELECT variables be referenced in the projection/`where` constraints? → A: In object-instantiation projections, use `?varName` tokens (e.g., `Name = ?name`). Constraint expressions reference projected values/properties (details to be clarified).
- Q: What should the projection part of a SPARQL comprehension look like? → A: Follow the new grammar: either a simple variable projection or a typed object projection using existing object instantiation syntax (e.g., `new TypeName() { Prop = ?var, ... }`).
- Q: For `lcomp_proj_obj` (object projection), should the comprehension grammar reuse Fifth’s existing object instantiation expression syntax? → A: Yes, reuse `object_instantiation_expression`.
- Q: When a SPARQL result row has an unbound/missing value for a variable referenced by the projection/`where`, what should happen? → A: Runtime error (clear error identifying the unbound variable).
- Q: What should be allowed as the generator source expression in a SPARQL comprehension? → A: Any tabular result expression (`from <expr>` where `<expr>` typechecks to a tabular SELECT result).
- Q: Should projected SPARQL variable discovery and SELECT/non-SELECT validation be performed using the SPARQL grammar at compile time? → A: Yes; the compiler may parse SPARQL using `src/parser/grammar/SparqlParser.g4` and must surface these as compile-time errors, not runtime errors.
