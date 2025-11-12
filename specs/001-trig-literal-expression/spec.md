# Feature Specification: TriG Literal Expression Type

**Feature Branch**: `001-trig-literal-expression`  
**Created**: 2025-11-11  
**Status**: Draft  
**Input**: User description: "Implement New Literal Expression Type for TriG Documents... (see PR context)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Initialize Store from TriG literal (Priority: P1)

As a Fifth developer, I can declare and initialize a `Store` variable directly from an inline TriG literal delimited by `@< ... >`, so that I can embed RDF datasets alongside code without external files.

**Why this priority**: Enables core value of embedding RDF datasets in code and is the minimal viable slice for the feature.

**Independent Test**: A .5th program with a `Store` initialized from a TriG literal compiles and runs; no interpolation used.

**Acceptance Scenarios**:

1. Given a valid TriG dataset inside `@< ... >`, When compiling, Then the program type-checks with the variable typed as `Store` and executes to produce a populated store.
2. Given malformed TriG inside `@< ... >`, When compiling, Then the compiler reports a diagnostic pinpointing the offending span inside the literal.

---

### User Story 2 - Interpolate expressions into TriG (Priority: P2)

As a developer, I can embed `{{ expression }}` anywhere inside the TriG literal to inject computed values (IRIs, literals, numbers, strings, dates), so the dataset can depend on program state.

**Why this priority**: Enables dynamic datasets and real-world scenarios; secondary to basic literal parsing.

**Independent Test**: A .5th program with variables (`int`, `string`, `bool`, `datetime`) interpolated into a TriG literal compiles and produces the corresponding serialized RDF terms.

**Acceptance Scenarios**:

1. Given `age: int = 21;` and a TriG literal containing `ex:age {{ age }}`, When running, Then the resulting RDF term is the integer literal 21 in the dataset.
2. Given `name: string = "Andrew";` and a TriG literal containing `ex:name {{ name }}`, When running, Then the resulting RDF term is a quoted string literal with proper escaping.
3. Given an expression that evaluates to an IRI wrapper (see FR-006), When running, Then the resulting term is inserted as an IRI without quotes.

---

### User Story 3 - Robust delimiter handling (Priority: P3)

As a developer, I can include nested `<...>` IRIs and TriG blocks within the literal without prematurely terminating at the first `>`, so that standards-compliant TriG parses correctly.

**Why this priority**: Prevents false terminations; essential for standards-compliant TriG but after P1/P2.

**Independent Test**: TriG literal containing multiple IRI `<http://...>` segments and nested graph blocks compiles and executes without delimiter errors.

**Acceptance Scenarios**:

1. Given IRIs enclosed in `<...>` inside the literal, When compiling, Then the literal only terminates at the matching top-level `>` for the opening `@<`.
2. Given unbalanced `<`/`>` in the literal, When compiling, Then a clear diagnostic indicates the imbalance and expected termination.

### Edge Cases

- Unbalanced `@< ... >` delimiters: missing closing `>` produces a targeted diagnostic with the start location of the literal.
- Literal containing the sequence `{{` or `}}` intended as text (not interpolation): use `{{{` to render `{{` and `}}}` to render `}}`.
- Interpolated expressions evaluating to `null` or unsupported types: results in a compile-time error with guidance.
- Very large TriG literals (e.g., 100KB–1MB): should not degrade compilation or editor responsiveness beyond normal expectations.

## Requirements *(mandatory)*

### Functional Requirements

- FR-001: The language MUST support a new literal expression form, TriG Literal Expression, starting with `@<` and ending with the matching top-level `>`; content between is treated as TriG text with optional interpolations.
- FR-002: A TriG Literal Expression MUST be assignable to variables of type `Store` (from the Fifth.System namespace) and type-check as producing a `Store` value.
- FR-003: The TriG literal parser MUST correctly handle nested `<...>` IRIs without prematurely terminating; termination occurs only at the top-level matching `>` corresponding to the initial `@<`.
- FR-004: The literal MUST support expression interpolation using `{{ expression }}` anywhere inside the TriG content, including within graph blocks and after predicates/objects.
- FR-005: Interpolation expressions MUST be evaluated in the surrounding lexical scope at runtime when the literal expression executes, preserving normal scoping and visibility rules.
- FR-006: Interpolated values MUST be serialized into valid TriG/Turtle terms:
  - Strings → quoted string literals with proper escaping
  - Numbers (int/float/decimal) → unquoted numeric literals
  - Booleans → `true`/`false`
   - Date/time and other value types → typed literals with appropriate datatype IRIs (e.g., `xsd:dateTime`) using sensible defaults
   - IRIs → inserted as IRIs using TriG syntax: absolute IRIs enclosed in `< ... >`; prefixed names written directly without quotes
- FR-007: If an interpolation yields an unsupported value or cannot be serialized to a valid TriG term, the compiler MUST emit a diagnostic at the interpolation site.
- FR-008: Whitespace and newlines inside the literal MUST be preserved as-is; the language MUST NOT implicitly trim or re-indent content.
- FR-009: Diagnostics MUST precisely reference spans within the TriG literal for parsing errors, unbalanced delimiters, or invalid interpolations, including line/column within the literal block.
- FR-010: The feature MUST coexist unambiguously with other graph constructs that use single `{}`; the `@< ... >` form is distinct and MUST NOT be confused with graph blocks.
- FR-011: The language MUST allow literal braces: `{{{` renders `{{` and `}}}` renders `}}` without triggering interpolation.

### Key Entities *(include if feature involves data)*

- TriGLiteralExpression: A language surface construct representing a TriG dataset literal with optional interpolations; evaluates to a `Store` value.
- Store: A language-visible type representing an RDF dataset store (backed by the platform runtime); target of assignment.
- Interpolation: An embedded expression inside `{{ ... }}` whose value is serialized into a TriG/Turtle term.

## Assumptions & Dependencies

- Assumption: Interpolations evaluate at runtime when the literal expression executes; constant folding may occur when expressions are compile-time constants, but it is not required.
- Assumption: Standard datatype IRIs (e.g., XML Schema datatypes) are used for typed literal serialization when applicable.
- Assumption: The literal content is included verbatim; no implicit trimming or indentation normalization is performed by the language.
- Dependency: Existing TriG grammar/lexer components are available within the parser project to validate TriG content where applicable.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- SC-001: Developers can declare a `Store` from a TriG literal without helper code and run a program to observe expected triples/quads present.
- SC-002: At least five distinct base types (`string`, `int`, `float/decimal`, `bool`, `datetime`) interpolate correctly with valid TriG serialization in acceptance tests.
- SC-003: Parser/diagnostics identify and locate errors within the literal with line/column precision in 100% of negative tests for this feature.
- SC-004: Accepts TriG literals up to 100KB; full solution build time delta for a project including such a literal is ≤ 5% versus the baseline project without the literal.
