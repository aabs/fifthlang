# Feature Specification: Graph Assertion Block

**Feature Branch**: `001-extend-the-ast`  
**Created**: 2025-09-14  
**Status**: Draft  
**Input**: User description: "Extend the AST to support a new kind of block called a Graph Assertion Block. It behaves like a regular block (a container of statements affecting variables and object references) but additionally asserts all mutations to assertable objects as truths in the default knowledge graph while the block executes. The block is delimited by `<{` and `}>` (already defined in the lexer). If execution of the block completes, the assertions made during the block scope are persisted either into the default graph scope or to a target graph when the block is used as an r-value in an assignment to a graph l-value. Example syntax: `graph ericKnowledge in <x:people> = <{ d: datetime = new datetime(1926, 5, 14); eric.dob = d; eric.age = calculate_age(d); }>; home += ericKnowledge;` and also allowing unnamed inline blocks `<{ ... }>` in expression contexts.".  Graph Assertion Blocks can appear anywhere that an assignment statement or regular block can appear.

## Execution Flow (main)
```
1. Parse user description from Input
	→ If empty: ERROR "No feature description provided"
2. Extract key concepts from description
	→ Identify: actors, actions, data, constraints
3. For each unclear aspect:
	→ Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
	→ If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
	→ Each requirement must be testable
	→ Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
	→ If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
	→ If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
	- User types and permissions
	- Data retention/deletion policies  
	- Performance targets and scale
	- Error handling behaviors
	- Integration requirements
	- Security/compliance needs

---

## User Scenarios & Testing (mandatory)

### Primary User Story
As a Fifth language developer, I want a scoped block syntax that behaves like a normal code block for program logic but also captures and persists truth assertions derived from object mutations to a knowledge graph, so that domain facts updated in code are recorded reliably without extra boilerplate.

### Acceptance Scenarios
1. Given a program with a configured default knowledge graph, When code inside `<{ ... }>` mutates properties/relationships of assertable objects, Then corresponding assertions are accumulated during execution and are persisted to the default graph upon successful scope exit when the block is used as a standalone statement; otherwise, no auto-persist occurs.
2. Given a graph l-value (e.g., a graph variable with a named graph scope), When `<{ ... }>` is used as an r-value in an assignment to a variable, Then a graph value is produced without persisting; persistence occurs later only via explicit graph operations (e.g., `graphVar += value`, `store += value`).
3. Given the example `graph ericKnowledge in <x:people> = <{ ... }>;` followed by `home += ericKnowledge;`, When the block completes, Then the block produces a graph value representing its assertions scoped to `x:people`, and adding it to `home` results in those assertions being persisted to the `x:people` named graph in the backing store.
4. Given a Graph Assertion Block that executes statements that only manipulate local variables or non-assertable objects, When the block completes, Then no assertions are created or persisted and the program behavior matches a regular block.
5. Given nested Graph Assertion Blocks, When inner and outer blocks both complete successfully, Then assertions from inner blocks merge into the enclosing block’s assertion set and commit at the outer boundary unless an explicit graph operation is invoked inside the inner block.
6. Given a Graph Assertion Block that exits via an early return, break, or continue without an unhandled exception, Then the assertions accumulated up to the exit are retained: for l-value targets, commit occurs at the outer transactional boundary; for default graph, persistence occurs only when the block is used as a standalone statement.
7. Given a Graph Assertion Block wherein an unhandled exception is thrown, When the stack unwinds past the block, Then assertions for l-value targets MUST NOT be committed; assertions already persisted to the default graph via explicit operations MUST NOT be rolled back.
8. Given an inline `<{ ... }>` used in an expression context without explicit assignment to a graph l-value, When it completes without raising an exception, Then it produces a graph value without auto-persist; if used as a standalone statement, it persists to the default graph.
9. Given a graph assertion block that is nested within another graph assertion block, When it completes without an exception, Then its assertions are merged into and assigned at the parent block’s commit boundary.
10. Given no mutations occur inside the block, When the block completes, Then the resulting assertion set is empty and assignment or persistence should be a no-op.
11. Given prefix/alias declarations (e.g., `alias x as <...#>;`), When IRIs or terms are referenced within or by effects of the block, Then resolution uses the in-scope aliases consistently with the rest of the language.

### Edge Cases
- Empty block `<{ }>` persists nothing and must not error.
- Semantics of assertions should favour the Open-World assumption.
- Multiple assertions about a property can be asserted into a graph. 
    - If the user wishes not to have intermediate values asserted, they should use temporary non-assertable variables to accumulate value changes.  
    - That is, normal programming control constructs.
	- Contradictory triples are allowed due to the AAA principle of Semantic Web standards.
- Multiple mutations to the same fact within the block accumulate, and do not overwrite each other.
- Conflicting mutations (e.g., setting differing values for the same property) are permitted in graphs.
- Identical assertions (same subject, predicate, object, and graph context) are deduplicated by set semantics.
- Objects that are assertable at the start of a block are not permitted to stop being assertable during the block.
- Default graph not configured or unreachable store results in an 'Unknown Default Graph or Store' exception.
- Use in conditional/loop contexts: assertions should reflect only executed paths and iterations.
- Cross-thread or re-entrant execution requires thread-safe assignment, but should opt for a permissive model where concurrent assertions should succeed.

## Requirements (mandatory)

### Functional Requirements
- FR-001: The language MUST introduce a Graph Assertion Block construct delimited by `<{` and `}>`.
- FR-002: Statements inside a Graph Assertion Block MUST execute with the same semantics as an ordinary code block for variables, control flow, and object state changes.
- FR-003: The system MUST identify "assertable objects" and, for each mutation to such objects within the block, MUST accumulate corresponding truth assertions representing the change (e.g., subject, predicate, object, and optional graph context) without requiring additional user code.
- FR-004: When used as a standalone statement (i.e., not producing a value consumed by an expression), the accumulated assertions MUST be persisted to the default knowledge graph upon scope completion.
- FR-005: When the block appears as an r-value and is later used in an explicit graph operation (e.g., `graphVar += blockValue`, `store += blockValue`), the accumulated assertions MUST be persisted to that operation’s target graph.
- FR-006: The construct MUST support use in expression contexts that permit blocks, including inline usage without an explicit graph variable name.
- FR-007: Mutations to non-assertable objects and to local-only variables MUST NOT produce assertions.
- FR-008: The set of assertions produced MUST be deterministically scoped to statements actually executed within the block's lexical boundaries.
- FR-009: The feature MUST support nested Graph Assertion Blocks. Assertions are committed into outer scope graphs according to lexical rules defined above. 
- FR-010: The system MUST define "successful completion" of a block as completion without encountering an exception that should unwind assertions made during the block.
- FR-011: On unhandled exceptions during block execution, assertions for l-value targets MUST NOT be committed; assertions already persisted to the default graph via explicit operations MUST NOT be rolled back.
- FR-012: The feature MUST allow references to variables, functions, and aliases from the surrounding scope within the block.
- FR-013: When zero assertions are produced, persistence MUST be a no-op and MUST NOT cause errors.
- FR-014: The feature MUST provide clear diagnostics for illegal usage contexts (e.g., type/context mismatches when assigning a block result to a graph l-value).
- FR-015: The system MUST use the current alias/prefix resolution rules consistently within assertions derived from the block.
- FR-016: The feature SHOULD offer predictable performance characteristics suitable for typical program usage.  
- FR-017: An assertion block that is not assigned as an r-value to some previously defined l-value, should be considered as an implicit assignment to the enclosing scope.  That is, it should be transformed into an assignment to whatever enclosing scope it is in.  
- FR-018: If an assertion block is not in an enclosing scope, it should be considered an implicit assignment to the default graph store.  
- FR-019: If a standalone assertion block attempts to persist to the default store or graph but no such graph store has been declared and connected to, the operation MUST fail with an 'Unknown Default Graph or Store' error and MUST NOT silently no-op.
- FR-020: All syntactic variations on graph assertions should resolve to the same AST model with either explicit or implicit l-value graphs to receive the asserted graph.  Lowering AST transformation steps should be used to ensure consistency.
- FR-021: Graph Assertion Blocks always yield a graph value, which is asserted and/or persisted depending on context.
- FR-022: Persistence of a set of assertions is triggered only by explicit store operations (e.g., `store += graphValue`) to a connected default or explicitly declared store.
- FR-023: When attempting to persist assertions to a store, if failures exist, the result should be to throw an exception describing what has failed, containing the graph r-value that was attempting to be written.  It is up to the user to decide what they wish to do in the event of connection failure.  Partial writes to the store cannot be un-asserted if it fails, mid way through the process, unless the store can somehow support transactional support.  This is not currently supported.
- FR-024:  Where concurrent writes are being performed on a graph store, the semantics for ordering, idempotency and conflict should be taken from the SPARQL 1.2 protocol
- FR-025: Rules for type-checking should come from the Fifth language, not from the knowledge graph ontology.  If a program passes type checking, then any assertions it makes should be permissible.
- FR-026: Assignment to a variable yields an in-memory graph value; persistence occurs only via explicit graph operations (e.g., +=, store += value) or when a block is used as a standalone statement targeting the default graph.
- FR-027: L-value targets are transactional (all-or-nothing at commit boundary). Default graph is non-transactional: any assertions already persisted before the exception remain; no rollback.
- FR-028: Set semantics (identical triples dedup); multiple distinct values for the same predicate are allowed under open-world; no last-write-wins unless property metadata later defines functional properties.
- FR-029: Graph Assertion Block is both a statement and a primary expression; as a standalone statement, it targets default graph; as an expression, it produces a graph value with no auto-persist.


### Key Entities (include if feature involves data)
- Graph Assertion Block: A scoped construct that executes code and accumulates assertions derived from mutations to assertable objects; on completion, it yields/persists an assertion set to a graph target.
- Assertion: A fact produced by observing a mutation; conceptually a triple/quad with optional graph context and metadata.
- Assertable Object: A domain object designated by the language/runtime as contributing to knowledge graph assertions when mutated.
- Default Knowledge Graph: The implicit target for assertions when no explicit graph l-value is provided.
- Graph l-value (Graph Variable): A program variable representing a graph target (optionally with a named graph scope) that can receive assertions from a block used as an r-value.
- Graph Store: The configured persistence layer where graphs are stored and updated.

---

## Review & Acceptance Checklist
GATE: Automated checks run during main() execution

### Content Quality
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous  
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified

---

## Execution Status
Updated by main() during processing

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---

## Engineering Notes

- Memory consumption should be proportional to the number of assertions made. 
- Once an assertion block has been exited (and assigned or persisted to an explicit or implicit l-value), any memory footprint it took up should be immediately cleared.
- A graph l-value is any variable assignable to a `graph` type. A graph variable may be an l-value or an r-value within an assignment statement. A graph value can be the result of a graph assertion block, and can be the r-value for an assignment.

### Terminology Usage

- **triple**: a single truth statement consisting of three parts: subject, predicate and object.  See RDF specification for more details.
- **graph**: A collection of triples, potentially scoped to an explicit IRI.
- **store**: A server supporting the SPARQL 1.2 protocol, allowing the storage and querying of graphs and triples.
- **Assertion**: Adding truth statements (triples) to a graph.
- **Persist** or **Persistence**: Submitting a graph or triple to some store supporting persistence.
- **Atomic**:  Submitting multiple values to a container, such as a graph or store, in a single 'transaction' operation.  The operation either saves all asserted triples and graphs, or none.  No partial success is allowed.  Any assignment to a store should be considered an atomic transaction.
- **Assignment**:  Assignment means asserting all assertions in an r-value graph into an l-value graph.  Persistence means the assignment of a graph to a store.