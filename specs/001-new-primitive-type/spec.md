# Feature Specification: New Primitive Type: `triple`

**Feature Branch**: `001-new-primitive-type`  
**Created**: 2025-09-26  
**Status**: Draft  
**Input**: User description (abridged): "Introduce new primitive data type 'triple' representing `VDS.RDF.Triple`, add triple literal syntax `<s, p, o>`, make `triple` a reserved keyword, allow declaration `t1: triple = <a:x, b:y, c:z>;`, define composition rules with `graph` using `+`/`-` producing new immutable graphs."

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

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a Fifth language developer working with RDF / knowledge graphs, I want a concise literal syntax for constructing an RDF triple so that I can express graph mutations and test data inline without verbose object construction or graph assertion blocks.

### Acceptance Scenarios
1. **Given** a file containing `t1: triple = <ex:s, ex:p, ex:o>;`, **When** it is parsed and type-checked, **Then** `t1` is bound to a value of primitive type `triple` whose underlying components map to a `VDS.RDF.Triple(subject=ex:s, predicate=ex:p, object=ex:o)`.
2. **Given** an expression `g1 + <ex:s, ex:p, ex:o>` where `g1` is a `graph`, **When** evaluated, **Then** the result is a new `graph` value containing all triples of `g1` plus the additional triple (if not already present); `g1` itself is not mutated.
3. **Given** an expression `g1 - <ex:s, ex:p, ex:o>` and `g1` contains that triple, **When** evaluated, **Then** the resulting `graph` excludes that triple while `g1` remains unchanged (comparison uses structural equality of subject, predicate, object node values including literal datatype/value).
4. **Given** a triple literal whose object position is a primitive value (e.g. `<ex:s, ex:age, 42>`), **When** parsed, **Then** the object is coerced/boxed according to the existing rules for constructing a `VDS.RDF.Triple` with a literal node.
5. **Given** a triple declaration `t2: triple = <ex:s, ex:p, someVar>;` where `someVar` is in scope and its value is a supported triple object kind, **When** executed, **Then** the object component references the runtime value of `someVar` at construction time.

### Edge Cases
- Triple literal using a variable in subject or predicate position where the variable's static type is `iri` (or equivalent IRI type alias) MUST be allowed; rejection occurs if the variable is not typed as an IRI.
- Object position containing a complex expression (e.g. `<ex:s, ex:val, f(1+2)>`) is accepted if the expression yields a supported node value (IRI or primitive literal) OR (if the expression is a list literal) each element of the list yields a supported node value (list expansion produces multiple triples).
- Empty list object `<s, p, []>` produces zero triples (no-op expansion) and SHOULD emit a compile-time warning indicating the triple literal expands to nothing.
- Duplicate triple addition `g1 + <ex:s, ex:p, ex:o>` when `g1` already contains that triple: no change (set semantics; duplicates suppressed).
- Removal `g1 - <ex:s, ex:p, ex:o>` when triple not present: result should equal `g1` (no error). (Assumed)
- Parsing ambiguity with existing `<{ ... }>` graph assertion blocks and IRIREF tokens: ensure grammar distinguishes `<{` (graph block) vs `<s, p, o>` (triple literal) vs `<http://...>` (IRIREF).

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: Language MUST reserve the keyword `triple` so it cannot be used as an identifier.
- **FR-002**: Language MUST allow variable declarations of the form `<name>: triple = <subject, predicate, object>;`.
- **FR-003**: Grammar MUST introduce a triple literal construct syntactically distinct from IRIREF and graph assertion blocks.
- **FR-004**: A triple literal MUST consist of exactly three components separated by commas and enclosed in `<` and `>` with no leading `{` after `<`.
- **FR-005**: Subject and predicate components MUST be IRIs (full IRIREF or prefixed names) per existing IRI grammar; alternatively they MAY be identifiers referencing in-scope variables whose static type is IRI (fails type check otherwise). [NEEDS CLARIFICATION: Are prefixed names already supported or only IRIREF tokens?]
- **FR-006**: Object component MUST accept: (a) an IRI, (b) a primitive literal (string, number, boolean), (c) an identifier/expression evaluating to those, or (d) a list literal whose elements each satisfy (a) or (b); a list literal triggers expansion into one triple per element.
- **FR-007**: A triple literal MUST evaluate to a value of primitive type `triple` whose underlying representation maps 1:1 onto `VDS.RDF.Triple`.
- **FR-008**: The `+` operator applied to `graph + triple` MUST return a new `graph` containing the set union of existing triples and the added triple (ignoring the addition if an identical triple already exists; original graph not mutated).
- **FR-009**: The `+` operator applied to `triple + triple` MUST return a `graph` containing both triples; if they are identical only one instance appears (set semantics). Relative ordering is implementation-defined and not part of the semantic contract.
- **FR-010**: The `+` operator applied as `triple + graph` MUST produce a `graph` whose triple set is the union; iteration/serialization order is implementation-defined and MUST NOT be relied upon.
- **FR-011**: The `-` operator applied to `graph - triple` MUST return a new `graph` excluding any triple(s) structurally equal in subject, predicate, and object node value (including literal value+datatype). (Structural equality)
- **FR-012**: Attempting `triple - graph` MUST be rejected by the type system.
- **FR-013**: Attempting unsupported arithmetic or logical operations (e.g. `triple * 2`, `!triple`) MUST produce type errors.
- **FR-014**: Triple literals MUST be allowed anywhere an expression is permitted (e.g. inside list literals, function arguments).
- **FR-015**: Error diagnostics MUST clearly distinguish triple literal syntax errors (wrong arity, invalid component) from IRI parsing errors.
- **FR-016**: Removing a triple not present in a graph via `graph - triple` MUST succeed and yield the original graph (immutability preserved).
- **FR-017**: Evaluation MUST not mutate existing graph instances when performing `+` or `-` with a triple.
- **FR-018**: Triple literals MUST be serializable/printable in a canonical textual form `<subject, predicate, object>`.
- **FR-019**: A triple literal whose object is a list literal (e.g. `<s, p, [o1, o2, o3]>`) MUST expand into multiple triples `<s, p, o1>`, `<s, p, o2>`, `<s, p, o3>` in any context where an expression list of triples/graphs is acceptable; expansion order is implementation-defined.
- **FR-020**: A triple literal with an empty list object (e.g. `<s, p, []>`) MUST result in zero emitted triples and SHOULD generate a non-fatal compiler warning describing the no-op expansion.

### Non-Functional / Constraints (optional but relevant)
- **NFR-001**: Introducing the `triple` literal MUST NOT break existing parsing of IRIREF or graph assertion blocks (backward compatibility).
- **NFR-002**: Parsing performance regression for existing files (no triple literals) SHOULD be negligible (<5% parse time increase). [NEEDS CLARIFICATION: performance target acceptance threshold]
- **NFR-003**: Clear, localized grammar changes SHOULD minimize ambiguity; no increase in global ANTLR ambiguity warnings beyond baseline.
- **NFR-004**: Triple iteration/serialization ordering is implementation-defined; tests and user programs MUST NOT depend on order stability.

### Open Questions / Clarifications Needed
1. What exact forms of object expressions are valid beyond list expansion (e.g., nested lists, graphs, other triples)?
5. Should there be implicit prefix resolution or namespace aliasing rules applied specifically to triple literals beyond existing IRI handling?
6. Must triple literals support whitespace/newline flexibility (e.g. `<ex:s,\n ex:p,\n 42>`)? (Assumed yes following general tokenization.)
7. Should object numeric literals infer typed literal datatypes (e.g. xsd:int) automatically? (Assumed existing literal mapping.)
8. Should a trailing comma be disallowed explicitly (e.g. `<a:b, c:d, e:f,>`)? (Assumed disallowed.)

## Clarifications

-### Session 2025-09-26
- Q: Are variables permitted in subject or predicate positions? → A: Yes, if their static type is IRI.
- Q: Graph duplicate semantics for `+`? → A: Set semantics (no duplicates).
- Q: Triple equality definition? → A: Structural equality on S,P,O node values (literals include value+datatype).
- Q: Allowed object forms in triple literal? → A: IRI | primitive literal | expression resolving to those | list literal expands to multiple triples.
- Q: Semantics of empty list object? → A: Produces zero triples with warning.
- Q: Is graph triple ordering observable? → A: Implementation-defined (not guaranteed across runs).

### Key Entities *(include if feature involves data)*
- **Triple (primitive)**: Represents exactly three RDF nodes: subject (IRI), predicate (IRI), object (IRI or literal/value). Immutable.
- **Graph (existing)**: Collection of zero or more triples. Treated as immutable under triple composition operations producing new graph instances.
- **IRI / Prefixed Name (existing)**: Identifiers used for subject and predicate (and optionally object).

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

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
*Updated by main() during processing*

- [ ] User description parsed  
- [ ] Key concepts extracted  
- [ ] Ambiguities marked  
- [ ] User scenarios defined  
- [ ] Requirements generated  
- [ ] Entities identified  
- [ ] Review checklist passed  

---
