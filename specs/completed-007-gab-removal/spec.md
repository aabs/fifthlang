# Feature Specification: Remove Graph Assertion Block (GAB)

**Feature Branch**: `[001-gab-removal]`  
**Created**: 2025-11-09  
**Status**: Draft  
**Input**: User description: "Complete removal of the language feature graph assertion block (GAB).  Leaving all of the RDF related functionality in place. For example triple literals and related data types. This language feature was experimental, and has no end users. Therefore, it is safe to completely remove without any process of deprecation. We will therefore tackle the entire process in one step. Related syntax definitions from the grammar files will also be removed, as will AST classes involved in delivery of the functionality such as the blocks, statements and expressions involved in transparent assertions on graphs."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Compiler rejects GAB syntax (Priority: P1)

As a compiler maintainer, I want the language to no longer accept Graph Assertion Block (GAB) syntax so that the grammar, AST, and documentation remain consistent and simpler, with RDF features retained.

**Why this priority**: Eliminates an unused experimental surface that increases complexity and maintenance costs; ensures immediate consistency across grammar and docs.

**Independent Test**: Verify grammar and AST no longer include GAB constructs; repository builds and example validation passes without GAB references.

**Acceptance Scenarios**:

1. Given a Fifth file containing a GAB-like construct (formerly graph assertion block), When parsed, Then the parser fails with a standard syntax error at/near the block start (no GAB-specific wording).
2. Given a Fifth file without GAB constructs, When parsed and compiled, Then the pipeline succeeds unchanged.

---

### User Story 2 - Docs and examples contain no GAB (Priority: P2)

As a documentation reader, I need examples and guides to reflect only supported syntax so I can learn the language without encountering deprecated constructs.

**Why this priority**: Prevents confusion and support debt; aligns learning materials with the language.

**Independent Test**: Run the repo’s example validator across docs/specs/tests and confirm zero references to GAB and zero parsing failures due to GAB.

**Acceptance Scenarios**:

1. Given the documentation set, When scanned for GAB references, Then zero instances remain in user-facing materials.
2. Given parsing of all examples, When executed by the example validator, Then no parsing failures relate to GAB.

---

### User Story 3 - RDF features remain intact (Priority: P3)

As a developer using RDF features, I want triple literals and RDF data types to continue working so my existing RDF-related code remains functional.

**Why this priority**: Preserves current value while removing only the unused construct.

**Independent Test**: Execute the repository's knowledge-graph smoke and runtime subsets and require zero failures:

- `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj -v minimal`
- `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter "FullyQualifiedName~KG_"`

Both commands MUST complete with 0 Failed and 0 Skipped tests.

**Acceptance Scenarios**:

1. Given the `kg-smoke-tests` project, When executed via the command above, Then it reports 0 Failed and 0 Skipped.
2. Given runtime integration tests filtered by `FullyQualifiedName~KG_`, When executed via the command above, Then they report 0 Failed and 0 Skipped.
3. Given a program using triple literals and supported store declarations (e.g., `name: store = sparql_store(<iri>);` or `store default = sparql_store(<iri>);`), When parsed and compiled, Then the pipeline succeeds unchanged.

---

### Edge Cases

- Files containing commented-out GAB syntax do not affect parsing or validation.
- Legacy internal samples referencing GAB are removed from the repository (no dedicated negative tests required).
- Error messages for former GAB constructs follow normal syntax error patterns; they do not reference removed constructs or suggest alternatives.

## Requirements *(mandatory)*

**Assumptions and Scope**: GAB has no external users; its complete removal is safe. All RDF-related functionality (triple literals, RDF types, permissible store declarations) remains unchanged. No deprecation period or feature flag is required.

### Functional Requirements

- **FR-001**: The language MUST reject Graph Assertion Block (GAB) syntax as invalid using standard syntax error diagnostics (no explicit mention of GAB in messages).
- **FR-002**: The language surface MUST remove GAB-related syntax from the public grammar; developers cannot write GAB in valid Fifth code.
- **FR-003**: The AST model MUST remove nodes specific to GAB (blocks/statements/expressions for transparent graph assertions) without affecting RDF features.
- **FR-004**: Documentation and examples MUST remove references to GAB and show only supported RDF features (e.g., triple literals, RDF datatypes, valid store declarations).
- **FR-005**: The example validation process MUST pass with zero GAB-related failures across docs/specs/tests.
- **FR-006**: The test suite MUST not include GAB-specific tests; remove any unit or parser tests that depend on GAB constructs.
- **FR-007**: RDF features (triple literals, RDF datatypes, permissible store declarations) MUST continue to parse and behave as before.
- **FR-008**: No user-visible configuration/flags MUST be required to enable/disable GAB; removal is unconditional.

### Key Entities *(include if feature involves data)*

- **Triple Literal**: Represents RDF statements in literal form; continues to be supported.
- **Store Declaration**: Declares RDF stores in supported canonical forms; continues to be supported.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Repository-wide example validation completes with 0 GAB references and 0 parsing failures due to GAB; no GAB-specific tests remain.
- **SC-002**: Documentation review finds 0 user-facing mentions of GAB post-change.
- **SC-003**: RDF regression verification passes with 0 Failed and 0 Skipped for both:
	- `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj -v minimal`
	- `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter "FullyQualifiedName~KG_"`

## Clarifications

### Session 2025-11-09

- Q: Should error messages explicitly reference GAB when rejecting these constructs? → A: No; leave no trace. Use standard syntax errors with no GAB mention.
- Q: Must we add dedicated negative tests for GAB rejection? → A: No; remove all GAB-related unit/parser tests.
