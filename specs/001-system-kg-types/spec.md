# Feature Specification: System KG Types in Fifth.System

**Feature Branch**: `001-system-kg-types`  
**Created**: 2025-11-09  
**Status**: Draft  
**Input**: Change graph/triple/store from language primitives to thin wrappers in `Fifth.System` (Option B: composition over dotNetRDF), with no source-breaking changes. Operators are implemented on these classes so the compiler need not lower KG operators specially. Types are globally available (no imports, no alias tables required).

## User Scenarios & Testing (mandatory)

### User Story 1 - Backward-Compatible Usage (Priority: P1)

As a Fifth developer with existing KG code, I can continue to write and run code like `g: graph; t: triple; s: store = sparql_store(<iri>); g += t;` without any changes, and it compiles and runs against the new `Fifth.System` implementations.

**Why this priority**: Preserves current user workflows and avoids ecosystem breakage while enabling the architectural refactor.

**Independent Test**: Run KG-focused tests and sample programs without modifying `.5th` files; they must pass unchanged.

**Acceptance Scenarios**:

1. Given an existing program `test/runtime-integration-tests/TestPrograms/KnowledgeManagement/declare-a-graph.5th`, When building and running the runtime integration tests, Then all KG-related tests pass without code changes.
2. Given the KG smoke tests project, When running `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj -v minimal`, Then the suite passes with zero regressions.

---

### User Story 2 - System-Library Extensibility (Priority: P2)

As a library author, I can add helpers (methods, extensions) to `Fifth.System.Graph`, `Fifth.System.Triple`, or `Fifth.System.Store` to expose richer behavior, and Fifth programs can call them normally.

**Why this priority**: Moves capabilities from compiler primitives into an evolvable library surface that can be extended without grammar changes.

**Independent Test**: Add a new helper in `Fifth.System` (e.g., `Graph.CountTriplesBySubject(iri)`) and consume it from a `.5th` sample compiled in tests.

**Acceptance Scenarios**:

1. Given a new helper method in `fifthlang.system` assembly, When a `.5th` test program calls it on a `graph`-typed value, Then it compiles and executes successfully.

---

### User Story 3 - Interop with dotNetRDF (Priority: P2)

As a developer bridging to .NET, I can pass/receive the underlying RDF objects through interop boundaries without custom compiler hooks.

**Why this priority**: Ensures the new design remains compatible with the underlying RDF ecosystem and C# utilities.

**Independent Test**: A thin C# fixture method receives a `Fifth.System.Graph` and asserts it wraps/subclasses the expected dotNetRDF type.

**Acceptance Scenarios**:

1. Given an interop test, When passing a `graph` value from Fifth to a C# helper, Then the helper sees a compatible type (e.g., `IGraph` or wrapper exposing `IGraph`).

---

### User Story 4 - Remove Primitive Registrations (Priority: P3)

As a maintainer, I can remove primitive registrations for `graph`, `triple`, `store` from the compiler type registry, and the system still builds and tests pass due to system types.

**Why this priority**: Reduces compiler surface area; moves semantics to libraries.

**Independent Test**: A unit test or static assertion that the `TypeRegistry` no longer lists these as primitives; build + tests still succeed.

**Acceptance Scenarios**:

1. Given the compiler TypeRegistry, When inspected, Then `graph`, `triple`, `store` are not present as primitives (replaced by system type binding).

---

### Edge Cases

- Name shadowing: If a user declares a local `graph` identifier (variable/function), ensure type name resolution still succeeds via prelude binding or fully-qualified `Fifth.System.Graph` reference.
- Global availability: `graph`, `triple`, `store` are globally available predeclared type names; programs compile without importing `Fifth.System`.
- Operator semantics: `graph += triple` remains supported by mapping to system-type method/operator without grammar changes.
- `store default` declaration: Remains valid and not impacted by library move; factory `sparql_store(<iri>)` returns `Fifth.System.Store`.
- Casing: Fifth surface uses lowercase `graph`, `triple`, `store`; system classes use PascalCase. A binding/alias preserves lowercase type annotations without exposing new keywords.
- Versioning: dotNetRDF dependency evolution must not break binary compatibility; prefer wrapping interfaces (`IGraph`, `Triple`) to insulate changes.

## Requirements (mandatory)

### Functional Requirements

- FR-001: Introduce `Fifth.System.Graph`, `Fifth.System.Triple`, and `Fifth.System.Store` types as thin wrappers over the corresponding dotNetRDF interfaces/types (prefer interface-based interop where available).
- FR-002: Compiler MUST define `graph`, `triple`, and `store` as globally available predeclared type names in the global namespace; no `Fifth.System` import or alias table is required in user code or emission.
- FR-003: Remove primitive type registrations for `graph`, `triple`, `store` from the compiler `TypeRegistry` (no special-case primitive handling remains).
- FR-004: Preserve existing operator semantics with no syntax changes: `graph += triple` is resolved to `Fifth.System.Graph` compound operator overloads (e.g., `operator +=`), and binary `graph + ...` is resolved to the corresponding `operator +`. Emit as operator expressions with no special-case compiler lowering for KG.
- FR-005: Provide/retain factory function `sparql_store(iri)` in `Fifth.System` that constructs and returns a `Store` instance.
- FR-006: Update lowering and translators to target the `Fifth.System` APIs rather than primitive-specific code paths; no hand-coded exceptions in the compiler.
- FR-007: No new keywords or grammar tokens are introduced; parsing of existing `.5th` KG samples remains unchanged.
- FR-008: Performance impact of the refactor is minimal; KG operations remain within 5% wall-clock of baseline on the existing smoke/perf scenarios.
- FR-009: Provide a fully-qualified escape hatch: `Fifth.System.Graph` can be referenced explicitly in type positions and interop code.
- FR-010: Ensure `scripts/validate-examples.fish` passes without modifications to examples; any incompatible example must be fixed in the library, not the grammar.
- FR-011: Implement operator matrix with specified mutability & type inference:
  - `triple + triple -> graph` (non-mutating)
  - `triple + graph -> graph` (non-mutating)
  - `graph + triple -> graph` (non-mutating)
  - `graph - triple -> graph` (non-mutating)
  - `graph + graph -> graph` (non-mutating)
  - `graph - graph -> graph` (non-mutating)
  - `graph += triple -> graph` (mutating)
  - `graph -= triple -> graph` (mutating)
  - `graph += graph -> graph` (mutating)
  - `graph -= graph -> graph` (mutating)
  - `store += graph -> store` (mutating; adds graph)
  - `store -= graph -> store` (mutating; removes graph)
  - All non-mutating binary operators return new wrapper instances (copy-on-write if underlying structure supports set semantics) without altering operands.
  - All mutating compound assignments apply in-place changes and return the LHS.
  - Graphs use set semantics: duplicate triples are suppressed. Adding the same triple is idempotent; binary merges (`+`) and compound merges (`+=`) de-duplicate.

## Non-Functional Requirements

- NF-001 (Thread Safety): `Fifth.System.Graph`, `Triple`, and `Store` are not thread-safe in v1. Concurrent mutation or reads during mutation are undefined behavior; use single-threaded access or external synchronization.

## Clarifications

### Session 2025-11-09

- Q: Do `graph`, `triple`, and `store` require importing `Fifth.System` to be usable? → A: No — they are globally available predeclared type names, usable without any imports.
- Q: What is the dotNetRDF interop pattern? → A: Option B selected — thin wrappers (composition) over dotNetRDF interfaces/types with explicit bridge methods (`ToVds*`/`FromVds*`).
- Q: How are `+=`/`+` supported? → A: Operators are implemented on the wrapper classes. The compiler resolves these as normal operator overloads; no bespoke lowering is used.
- Q: Binding strategy for type names (imports vs alias table vs global)? → A: Types live directly in the global namespace; no imports or alias table are used.
- Q: What are the semantics of `triple + triple` and `triple - triple`? → A: `triple + triple` produces a new `graph` containing both triples; `triple - triple` is undefined and not provided as an operator (no lowering, no overload).

### Key Entities

- Graph: Fifth surface type aliasing/binding to `Fifth.System.Graph` (thin wrapper over dotNetRDF `IGraph`), defines full operator set per FR-011 (both non-mutating binary and mutating compound assignments).
- Triple: Fifth surface type aliasing/binding to `Fifth.System.Triple`.
- Store: Fifth surface type aliasing/binding to `Fifth.System.Store` (e.g., SPARQL-backed store).

## Success Criteria (mandatory)

### Measurable Outcomes

- SC-001: Build + full tests pass without editing `.5th` samples:
  - `dotnet build fifthlang.sln -v minimal`
  - `dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj -v minimal`
  - `dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter FullyQualifiedName~KG_`
- SC-002: Example validation passes with zero new failures:
  - `scripts/validate-examples.fish`
- SC-003: No primitive registrations remain for these types (grep returns no hits in compiler for primitive bindings):
  - `grep -R --line-number --exclude-dir=.git "register.*primitive" src/ | grep -E "graph|triple|store" || true`
- SC-004: Performance remains within 5% of current baseline on KG scenarios:
  - `scripts/perf/compare_benchmarks.py` shows ≤5% delta for KG benchmarks.
- SC-005: An interop test asserts that `graph` values are assignable to or expose the underlying dotNetRDF interfaces via the system wrappers.
