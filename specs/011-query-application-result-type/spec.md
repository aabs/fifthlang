# Feature Specification: Query Application and Result Type

**Feature Branch**: `011-query-application-result-type`  
**Created**: 2025-11-15  
**Status**: Draft  
**Input**: User description: "Allow the presentation of a SPARQL query to a store, to get results in either tabular or graph format. Introduce new Result type and <- operator for query application."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Apply SELECT Query to Store (Priority: P1)

A developer needs to query RDF data stored in a knowledge graph using SPARQL SELECT queries and process the tabular results within their Fifth program. They want to use a clean, intuitive syntax that minimizes syntactic noise.

**Why this priority**: This is the most common SPARQL use case - querying data and iterating over results. It provides the foundation for all query operations and delivers immediate value for data access patterns.

**Independent Test**: Can be fully tested by creating a store with sample data, applying a SELECT query using the `<-` operator, and verifying the Result contains the expected tabular data with the projected SPARQL variables.

**Acceptance Scenarios**:

1. **Given** a Store containing RDF triples and a Query containing a SPARQL SELECT statement, **When** the developer applies the query to the store using `result: Result = query <- store`, **Then** the Result contains tabular data with columns matching the SPARQL variables projected in the SELECT clause
2. **Given** a Result from a SELECT query, **When** the developer accesses the data, **Then** they can retrieve values by SPARQL variable name
3. **Given** a Result from a SELECT query with multiple result rows, **When** the developer iterates over the results, **Then** each row provides access to all projected variables

---

### User Story 2 - Apply CONSTRUCT/DESCRIBE Query to Store (Priority: P2)

A developer needs to query RDF data using SPARQL CONSTRUCT or DESCRIBE queries that return graph-structured results (RDF datasets) rather than tabular data. They want the result to be a Store that can be further queried or manipulated.

**Why this priority**: CONSTRUCT and DESCRIBE queries are essential for graph transformations and data extraction patterns. This enables graph-to-graph operations which are fundamental to semantic web workflows.

**Independent Test**: Can be fully tested by creating a store with sample data, applying a CONSTRUCT query using the `<-` operator, and verifying the Result provides access to the returned Store containing the constructed triples.

**Acceptance Scenarios**:

1. **Given** a Store containing RDF triples and a Query containing a SPARQL CONSTRUCT statement, **When** the developer applies the query to the store using `result: Result = query <- store`, **Then** the Result provides access to a Store containing the constructed RDF graph
2. **Given** a Result from a CONSTRUCT query, **When** the developer accesses the Store, **Then** they can query it or export it as TriG format
3. **Given** a Result from a DESCRIBE query, **When** the developer accesses the Store, **Then** it contains all triples describing the specified resources

---

### User Story 3 - Handle Query Execution Errors (Priority: P3)

A developer applies a query to a store, but the query execution fails due to syntax errors, connection issues, or other runtime problems. They need clear error information to diagnose and fix the issue.

**Why this priority**: Error handling is essential for production systems but can be implemented after the happy path is working. It improves developer experience and system reliability.

**Independent Test**: Can be fully tested by attempting to apply malformed queries or queries to disconnected stores, and verifying that appropriate error messages are provided.

**Acceptance Scenarios**:

1. **Given** a syntactically invalid SPARQL query, **When** the developer attempts to apply it to a Store, **Then** a compile-time error is raised indicating the syntax issue
2. **Given** a valid Query and a Store, **When** the query execution fails at runtime (e.g., connection timeout), **Then** a runtime exception is raised with details about the failure
3. **Given** an attempt to apply a query to a non-queryable object, **When** compilation occurs, **Then** a type error is raised indicating the RHS must be a SPARQL-queryable store

---

### User Story 4 - Type Safety for Result Access (Priority: P2)

A developer wants to access result data with type safety, understanding at development time whether they're working with tabular results (SELECT) or graph results (CONSTRUCT/DESCRIBE).

**Why this priority**: Type safety prevents runtime errors and improves developer experience. It's important for catching errors early but doesn't block basic functionality.

**Independent Test**: Can be fully tested by attempting to access tabular data from a CONSTRUCT result or graph data from a SELECT result, and verifying appropriate compile-time errors or runtime behavior.

**Acceptance Scenarios**:

1. **Given** a Result from a SELECT query, **When** the developer attempts to access it as tabular data, **Then** the operation succeeds and provides access to variables and rows
2. **Given** a Result from a CONSTRUCT query, **When** the developer attempts to access it as a Store, **Then** the operation succeeds and provides access to the RDF graph
3. **Given** a Result, **When** the developer checks its type, **Then** they can determine whether it contains tabular or graph data

---

### Edge Cases

- What happens when a SELECT query returns zero results?
- How does the system handle empty CONSTRUCT results (no triples constructed)?
- What happens when applying a query to a store that doesn't implement the required query interface?
- How does the system handle queries with syntax errors detected only at runtime?
- What happens when a SPARQL query times out during execution?
- How does the system handle queries that return extremely large result sets?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST introduce a new `Result` type in the Fifth.System namespace that exposes a discriminated union with three cases: `TabularResult` (SELECT), `GraphResult` (CONSTRUCT/DESCRIBE), and `BooleanResult` (ASK). The `Result` MUST rely on dotNetRDF’s result model for discrimination and data access (SELECT/ASK via `SparqlResultSet` + `SparqlResultsType`; CONSTRUCT/DESCRIBE via dotNetRDF graph outputs adapted to Fifth.System `Store`).
- **FR-002**: System MUST support the `<-` operator for applying queries to stores with syntax `result: Result = query <- store`
- **FR-003**: System MUST support applying queries to any store type that is assignable to or implements a SPARQL-queryable interface
- **FR-004**: System MUST enable Result to act as a discriminated union that can contain either tabular data (SELECT results) or graph data (CONSTRUCT/DESCRIBE results)
- **FR-005**: Result MUST provide access to SPARQL variables and row data when containing tabular results from SELECT queries
- **FR-006**: Result MUST provide access to a Store containing RDF triples when containing graph results from CONSTRUCT or DESCRIBE queries
- **FR-007**: System MUST raise compile-time errors when the right-hand side of `<-` is not a SPARQL-queryable store type
- **FR-008**: System MUST raise runtime errors when query execution fails with details about the failure cause
- **FR-009**: System MUST handle SELECT, CONSTRUCT, DESCRIBE, and ASK query forms appropriately
- **FR-010**: System MUST preserve the semantics of SPARQL result formats as defined by the SPARQL specification
- **FR-011**: For SPARQL ASK queries, Result MUST expose a dedicated boolean union case (e.g. `BooleanResult`) providing direct access to the truth value without tabular row abstraction
- **FR-012**: System MUST allow raw string composition of SPARQL query literals (concatenation/interpolation) while performing a runtime validation pass that rejects unsafe token sequences (e.g. unbalanced braces, injected prefix declarations, rogue `DROP GRAPH`, or variable placeholders outside allowed interpolation spans). Compiler MUST emit a warning (not an error) for unstructured concatenations lacking explicit safe interpolation markers, and MUST provide an opt-in API (`bind(varName, value)`) for future migration to structured binding without forbidding existing patterns.
- **FR-013**: Query execution engine MUST scale to handle SELECT result sets up to 100,000 rows with streaming access (iterator / paging) without exceeding a 2× memory footprint relative to dotNetRDF's native execution for equivalent queries.
- **FR-014**: System MUST expose a structured diagnostics object for failed query applications (`QueryError { Kind, Message, SourceSpan?, UnderlyingExceptionType?, Suggestion? }`) instead of plain exception-only messaging.
- **FR-015**: Query application MUST support cooperative cancellation via a language-level cancellation token (if supplied) propagating to dotNetRDF execution; absence of token keeps current synchronous semantics.
- **FR-016**: Concurrent query applications to the same Store MUST be isolated (no shared mutable state) and MUST serialize destructive operations (if later allowed) while permitting parallel read queries.
- **FR-017**: System MUST define a `QueryError.Kind` enumeration with the following values for structured diagnostics: SyntaxError (malformed SPARQL), ValidationError (semantic/constraint violations), ExecutionError (runtime query processing failure), Timeout (exceeded execution limit), Cancellation (user-requested stop), SecurityWarning (unsafe query composition detected), ResourceLimit (memory/result size exceeded), ConcurrencyConflict (conflicting concurrent operation).
- **FR-018**: SPARQL literal syntax `?<...>` MUST be parsed at compile time by a dedicated, delegated SPARQL parser that is separate from the core Fifth grammar. The Fifth parser MUST only delimit the literal boundaries and delegate the inner content to the SPARQL parser. Any SPARQL syntax errors MUST surface as compile-time diagnostics mapped to `QueryError.Kind = SyntaxError` with precise source spans. No ad-hoc manual tokenization inside the Fifth parser is permitted for SPARQL content.

### Key Entities

- **Result**: Represents the outcome of applying a SPARQL query to a store. Acts as a discriminated union containing either: (1) tabular data (SELECT) with SPARQL variable + row access, (2) graph data (CONSTRUCT/DESCRIBE) as a Store, or (3) a boolean value (ASK) via a dedicated boolean case (e.g. `BooleanResult { Value: bool }`). Internally leverages dotNetRDF’s unified results: SELECT/ASK use `SparqlResultSet` and its `ResultsType` for discrimination; CONSTRUCT/DESCRIBE consume dotNetRDF graph outputs adapted to Fifth.System `Store` without duplicating graph semantics.
- **Query**: Represents a SPARQL query (already exists in the system, created using `?<...>` syntax). Can be applied to stores using the `<-` operator.
- **Store**: Represents an RDF dataset (already exists in the system). Must be SPARQL-queryable to be used as the right-hand side of `<-` operator.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can apply SPARQL queries to stores using the `<-` operator and receive results with no more than 2 lines of code
- **SC-002**: Result type correctly distinguishes between tabular (SELECT) and graph (CONSTRUCT/DESCRIBE) results 100% of the time
- **SC-003**: Compile-time type checking prevents applying queries to non-queryable objects in 100% of cases
- **SC-004**: Runtime error messages for query execution failures include sufficient detail for developers to diagnose the issue in under 2 minutes
- **SC-005**: Query application syntax reduces syntactic noise by at least 50% compared to equivalent C# dotNetRDF code
- **SC-006**: System successfully handles result sets containing up to 10,000 rows without performance degradation
- **SC-007**: ASK queries yield the correct boolean value 100% of the time across at least 20 test scenarios (true, false, empty pattern, graph clause variants)
- **SC-008**: Runtime validation rejects 100% of a curated set (≥ 30) of unsafe SPARQL concatenation/injection attempts (e.g. prefix smuggling, trailing `#` comment escapes, mid-token insertions) while allowing ≥ 95% of legitimate composed queries without false positives.
- **SC-009**: SELECT queries returning 100k rows complete with < 1.5× baseline memory usage and < 10% throughput degradation vs direct dotNetRDF benchmark on identical dataset.
- **SC-010**: 100% of forced error scenarios produce a structured `QueryError` object populated with Kind + Message; ≥ 80% also provide a Suggestion.
- **SC-011**: Cancellation requests issued before 50% of execution time result in termination within 200ms 95% of the time.
- **SC-012**: Parallel execution test (≥ 25 simultaneous read queries) shows no cross-query data corruption and ≤ 5% variance in per-query latency vs isolated execution.
- **SC-013**: All error scenarios produce a `QueryError` with a `Kind` value from the defined enumeration (SyntaxError, ValidationError, ExecutionError, Timeout, Cancellation, SecurityWarning, ResourceLimit, ConcurrencyConflict); 100% coverage of each kind in test suite.

## Assumptions

- The `Query` type already exists in the Fifth.System namespace (created via SPARQL literal syntax `?<...>`)
- The `Store` type already exists and wraps dotNetRDF's triple store functionality
- dotNetRDF's `SparqlResultSet` type provides sufficient functionality for both tabular and graph results
- The Fifth language supports discriminated unions or a similar type mechanism for Result
- Type inference can determine when Result contains tabular vs graph data based on the query type
- The `<-` operator can be added to the grammar without conflicts with existing operators

## Dependencies

- dotNetRDF library (`VDS.RDF.*` packages) for SPARQL query execution
- Fifth.System library providing Query and Store types
- ANTLR grammar modifications to support the `<-` operator
- Delegated SPARQL parser (reuse/adapt the existing SPARQL grammar under `src/parser/grammar/` or equivalent) invoked only for `?<...>` literals
- Type system support for discriminated unions or variant types

## Technical Constraints

- Must maintain compatibility with existing Query and Store type implementations
- Must follow Fifth language's type system conventions
- Must integrate with existing error handling and diagnostic systems
- Result type must be efficient for both small and large result sets
- Implementation must not introduce breaking changes to existing query or store functionality
- SPARQL literal parsing MUST be handled by a separate delegated parser; the core Fifth parser MUST NOT inline or partially reimplement SPARQL tokenization/parsing beyond locating the `?<...>` literal bounds

## Clarifications

### Session 2025-11-15

- Q: What granularity should the `QueryError.Kind` enumeration have for structured diagnostics? → A: Option B - Expanded core: SyntaxError, ValidationError, ExecutionError, Timeout, Cancellation, SecurityWarning, ResourceLimit, ConcurrencyConflict

### ASK Query Representation
Decision: Adopt Option A — introduce a distinct boolean discriminated union case for ASK results.

Rationale:
- Reflects true SPARQL semantics (ASK is inherently boolean)
- Eliminates artificial single-row/single-column tabular wrapping
- Simplifies access (`if (result.IsBoolean && result.Value)`) and reduces allocation
- Improves type-directed tooling and code completion clarity

Impacts:
- Added **FR-011** and **SC-007**
- Updated `Result` entity description to include boolean case
- Test plan must add targeted ASK scenarios (positive/negative, empty store, GRAPH clause, FILTER edge cases)

Non-Goals:
- Do not represent ASK as a degenerate SELECT result
- Do not require iteration APIs for boolean extraction

Open Follow-ups: None pending for ASK after this clarification.

### SPARQL Literal Parsing (Delegated Parser)
Decision: Parse the content of `?<...>` using a separate delegated SPARQL parser at compile time. The core Fifth grammar only delimits the literal and forwards its content for SPARQL parsing.

Rationale:
- Ensures authoritative SPARQL compliance and isolates grammar evolution from the Fifth grammar
- Enables precise compile-time diagnostics with line/column spans for malformed SPARQL (fulfills US3.1 and FR-018)
- Avoids fragile hand-rolled tokenization inside the Fifth parser

Impacts:
- Added **FR-018** and updated Dependencies/Constraints to mandate delegation
- Parser integration must wire an error listener that maps SPARQL parser errors to compiler diagnostics with `QueryError.Kind = SyntaxError`
- Syntax tests must include valid/invalid `?<...>` samples; invalid samples fail at compile time

Non-Goals:
- Do not inline SPARQL grammar rules into `FifthParser.g4` beyond literal delimitation
- Do not defer SPARQL literal syntax errors to runtime; these are compile-time diagnostics

Open Follow-ups:
- Select/adapt the existing SPARQL grammar implementation already present under `src/parser/grammar/` and define the adapter surface (`SparqlLiteralParser`)

### dotNetRDF Results Dependency (I2)
Decision: Treat dotNetRDF as the authoritative source for SPARQL result discrimination and shape. `Result` depends on `SparqlResultSet`/`SparqlResultsType` for SELECT/ASK and adapts graph outputs (CONSTRUCT/DESCRIBE) to `Store`.

Rationale:
- Avoids re-implementing result semantics already provided by dotNetRDF
- Ensures compatibility and correctness across all query forms
- Simplifies our `Result` to a thin, typed facade over dotNetRDF

Impacts:
- Use `SparqlResultsType` for SELECT/ASK discrimination; no synthetic tables for ASK
- For graph queries, convert dotNetRDF graph outputs into `Store` while preserving triples
- Tests must assert correct mapping for all result forms

Non-Goals:
- Do not create a parallel in-memory representation duplicating dotNetRDF result structures

### Query Construction Security Posture
Decision: Option D — permit raw string concatenation/interpolation for SPARQL query construction with a runtime safety validation layer (diagnostic + hard failure), rather than enforcing purely structured binding at compile time.

Rationale:
- Maximizes short-term developer flexibility: existing code using ad-hoc concatenation continues to work.
- Introduces incremental hardening via a lightweight validator that inspects final query text before execution.
- Provides migration path toward stricter structured binding (`bind(name, value)`) without blocking current experiments.
- Avoids premature complexity in grammar/type system while still surfacing security concerns (warnings for unstructured patterns).

Impacts:
- Added **FR-012** and **SC-008**.
- Requires a validator component (likely in compiler or execution pipeline) scanning assembled query text for disallowed patterns (unclosed quotes, injected `;` outside production, unauthorized `DROP/LOAD/CREATE` if disallowed, duplicate `PREFIX` collisions, variable name shadow attempts).
- Compiler warning category introduced: `SPARQL.UnstructuredComposition` for concatenations without safe interpolation markers.
- Test matrix must include both allowed and rejected samples (legitimate multi-line concatenation, malicious prefix injection, comment escape attack, unicode homoglyph variable injection).
- Documentation must clearly differentiate: (a) raw concatenation (works, may warn) vs (b) future structured binding (preferred, no warnings).

Non-Goals:
- No immediate compile-time ban of raw concatenation.
- No automatic auto-fix / rewriting of dangerous queries; failure is explicit with diagnostic.
- Not providing full static taint analysis in this phase.

Open Follow-ups:
- Define validator rule list and false-positive thresholds.
- Decide when (future feature) to upgrade warnings to errors for certain patterns.
- Consider adding an opt-in strict mode flag (`#pragma sparql_strict`) turning warnings into errors.

### Error Kind Taxonomy
Decision: Adopt Option B — expanded core enumeration balancing precision with maintainability.

Rationale:
- **SyntaxError, ValidationError, ExecutionError** cover fundamental SPARQL lifecycle stages.
- **Timeout, Cancellation** address operational control (aligned with FR-015).
- **SecurityWarning** surfaces injection risks from FR-012 validation layer.
- **ResourceLimit** enables graceful handling of memory/result size constraints (complements FR-013 streaming).
- **ConcurrencyConflict** supports FR-016 isolation model when destructive operations are introduced.
- Avoids over-fragmentation (Option C's dotNetRDF mirroring) while providing more semantic precision than a single generic kind (Option D).

Impacts:
- Added **FR-017** and **SC-013**.
- Requires mapping dotNetRDF exceptions (`RdfParseException`, `SparqlQueryException`, `RdfQueryTimeoutException`) to internal `Kind` values.
- Test matrix must ensure each `Kind` is triggered and validated at least once.
- IDE/linter tooling can key off `Kind` for targeted suggestions (e.g., SecurityWarning → "Consider using bind() API").

Non-Goals:
- No numeric sub-codes within `Kind` values in this phase.
- No automatic retry logic based on `Kind` (deferred to caller).

Open Follow-ups:
- Document dotNetRDF-to-Kind mapping table (e.g., `RdfParseException` → SyntaxError).
- Define ResourceLimit threshold values (memory budget, max row count).
- Specify ConcurrencyConflict triggering conditions once destructive operations are designed.

### Performance Scaling
Decision: Support streaming/paged access for large SELECT results (target 100k rows) without forcing full materialization.

Rationale:
- Prevents memory spikes for analytical queries.
- Aligns with future pipeline passes (e.g., row filtering) that can be lazy.

Impacts:
- Added **FR-013**, **SC-009**.
- Requires iterative adapter over dotNetRDF results + optional paging API.

### Structured Diagnostics
Decision: Introduce a structured `QueryError` surface instead of ad-hoc exceptions only.

Rationale:
- Enables IDE/linter tooling to give precise feedback (Kind-based actions).
- Facilitates automated retry or fallback strategies.

Impacts:
- Added **FR-014**, **SC-010**.
- Requires mapping dotNetRDF exceptions to internal error taxonomy.

### Cancellation & Concurrency
Decision: Provide optional cancellation token parameter; ensure concurrent read queries are safe while future destructive operations are serialized.

Rationale:
- Improves responsiveness for long-running graph pattern matches.
- Establishes baseline for later timeout and resource budgeting features.

Impacts:
- Added **FR-015**, **FR-016**, **SC-011**, **SC-012**.
- Introduces minimal coordination layer (read/write lock or async semaphore) without penalizing read-heavy workloads.

Non-Goals (for this phase):
- No automatic adaptive batching of result rows.
- No speculative pre-fetch beyond simple buffering.
- No full transaction model for write queries.

Open Follow-ups:
- Define `Kind` enumeration (Syntax, Validation, Execution, Timeout, Cancellation, SecurityWarning).
- Benchmark memory usage with layered iterator vs direct list materialization.
- Evaluate ergonomics of token passing syntax (inline vs function signature augmentation).

