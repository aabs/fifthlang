# Feature Specification: Constructor Functions

**Feature Branch**: `001-constructor-functions`  
**Created**: 2025-11-19  
**Status**: Draft  
**Input**: "Introducing Constructor Functions: class-named non-generic constructors with overloads, base chaining, synthesis, assignment guarantees."

## Overview

This feature introduces Constructor Functions: explicit class-named, non-generic methods with no return type used to reliably initialize new instances. They provide predictable object creation, enforce required field assignment, enable overloads and inheritance base chaining, and surface clear diagnostics when initialization is incomplete or ambiguous. This improves developer confidence, reduces hidden initialization bugs, and aligns Fifth with familiar patterns from mainstream languages while keeping semantics explicit and safe.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Explicit Construction (Priority: P1)
Developers define a constructor to initialize mandatory fields so object creation is explicit and safe.

**Why this priority**: Foundational; enables clear object instantiation for any non-trivial class.

**Independent Test**: Create a class with two required fields, define one constructor assigning both, instantiate, verify no diagnostics and values set.

**Acceptance Scenarios**:
1. **Given** a class with required fields and a matching constructor, **When** an instance is created with correct arguments, **Then** the instance holds those values and compilation succeeds.
2. **Given** a class with required fields but no constructor, **When** attempting instantiation, **Then** a diagnostic indicates an explicit constructor is required.

---

### User Story 2 - Field Safety & Definite Assignment (Priority: P1)
Developers rely on diagnostics to ensure all required fields are assigned before construction completes.

**Why this priority**: Prevents runtime errors and inconsistent state.

**Independent Test**: Define a constructor omitting one required field assignment; expect a definite assignment diagnostic listing missing fields.

**Acceptance Scenarios**:
1. **Given** a constructor path missing an assignment, **When** compiling, **Then** a diagnostic names each unassigned field.
2. **Given** all paths assign required fields, **When** compiling, **Then** no assignment diagnostics appear.

---

### User Story 3 - Overload Resolution (Priority: P2)
Developers provide multiple constructor signatures and expect precise, unambiguous selection on `new` calls.

**Why this priority**: Enhances expressiveness and reduces boilerplate factory patterns.

**Independent Test**: Define two overloads differing by parameter types; call each and verify correct resolution; add a third causing ambiguity to trigger an error.

**Acceptance Scenarios**:
1. **Given** distinct overloads, **When** calling with matching argument types, **Then** the correct overload is selected.
2. **Given** two equally specific overloads, **When** calling with ambiguous arguments, **Then** an ambiguity diagnostic is produced.

---

### User Story 4 - Inheritance Base Chaining (Priority: P2)
Developers in derived classes initialize base state explicitly when required.

**Why this priority**: Ensures complete initialization across hierarchy.

**Independent Test**: Base class with parameterized constructor; derived class without base call should fail; add base call; succeeds.

**Acceptance Scenarios**:
1. **Given** a base lacking a parameterless constructor, **When** a derived constructor omits `: base(...)`, **Then** a diagnostic reports missing base invocation.
2. **Given** a proper base call with correct arguments, **When** compiling, **Then** construction succeeds.

---

### User Story 5 - Generic Class Construction (Priority: P3)
Developers instantiate generic classes and rely on constructors referencing class type parameters only.

**Why this priority**: Extends constructor safety to generic types without added complexity.

**Independent Test**: Generic class `Box<T>` with constructor using `T`; instantiate with different concrete types; verify type-correct initialization.

**Acceptance Scenarios**:
1. **Given** a generic class and constructor referencing its type parameter, **When** instantiating with `int`, **Then** field types resolve to `int`.
2. **Given** same class instantiated with `string`, **When** assigning via constructor, **Then** types resolve to `string` with no confusion.

---

### Edge Cases
- Instantiation with `null` for unconstrained generic parameter.
- Duplicate constructor signature definitions in same class.
- Shadowed parameter names matching field names require explicit qualification.
- Missing base initializer when base has no parameterless constructor.
- Recursive or cyclic inheritance leading to potential base call cycles.
- Constructor declared but returning a value expression (invalid form).
- Parameterless constructor synthesis when all fields have defaults versus required field presence.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-CTOR-001**: A class MAY declare zero or more constructors all named exactly as the class and without a return type.
- **FR-CTOR-002**: Constructors MAY include zero or more parameters whose types can reference class type parameters.
- **FR-CTOR-003**: Constructors MUST NOT declare independent type parameters; only class-level type parameters are permitted.
- **FR-CTOR-004**: If no constructors are declared and all required fields are defaulted or nullable, a parameterless constructor MUST be synthesized.
- **FR-CTOR-005**: If no constructors are declared and any required field lacks initialization, compilation MUST emit a diagnostic requiring an explicit constructor.
- **FR-CTOR-006**: Each constructor overload MUST have a unique ordered parameter type list.
- **FR-CTOR-007**: Object creation MUST select a single best overload or produce an ambiguity diagnostic.
- **FR-CTOR-008**: Constructor bodies MUST NOT return a value; `return;` (without expression) is allowed for early exit. Any `return <expr>;` form MUST produce diagnostic CTOR009 with the location and constructor signature.
- **FR-CTOR-009**: All required fields MUST be definitely assigned along all execution paths prior to constructor completion.
- **FR-CTOR-010**: Derived class constructors MUST invoke a base constructor if the base lacks a parameterless constructor.
- **FR-CTOR-011**: Base constructor invocation MUST appear in a leading initializer `: base(...)` preceding the body.
- **FR-CTOR-012**: Parameters may shadow field names; qualification via `this.` MUST be supported and required for disambiguation. When a parameter name matches a field name, unqualified assignment assigns to the parameter (no-op); field assignment requires `this.fieldName = value;`. Missing qualification does not produce a diagnostic but results in ineffective assignment (the field remains uninitialized). Static analysis SHOULD detect this pattern and emit a warning (future enhancement).

**Example**:
```fifth
class Person {
    Name: string;
    
    Person(string Name) {
        Name = Name;           // No-op: assigns parameter to itself
        this.Name = Name;      // Correct: assigns parameter to field
    }
}
```

- **FR-CTOR-013**: Object creation MUST validate argument arity and type compatibility with the resolved constructor.
- **FR-CTOR-014**: In generic classes, constructor parameter types referencing class type parameters MUST reflect concrete arguments at instantiation.
- **FR-CTOR-015**: Constructors MUST reject disallowed modifiers with diagnostic CTOR010. Forbidden modifiers: `async`, `static`, `abstract`, `virtual`, `override`, `sealed`. Only visibility modifiers (`public`, `private`, `protected`, `internal`) are permitted.
- **FR-CTOR-016**: Cyclic base constructor requirements MUST be detected and reported.
- **FR-CTOR-017**: User-declared parameterless constructor MUST suppress synthesis of an implicit one.
- **FR-CTOR-018**: `null` arguments for unconstrained generic parameters MUST follow standard type compatibility rules without special inference.
- **FR-CTOR-019**: Diagnostics MUST include code, location, constructor signature, class name, and actionable hint.
- **FR-CTOR-020**: Successful constructor resolution MUST bind class type parameters before field type usage.

### Key Entities
- **Class**: Defines fields and optional constructors governing instance initialization.
- **Constructor**: Class-named method without return type establishing initialization sequence.
- **Base Constructor**: Parent class constructor potentially required for proper inherited state setup.
- **Field**: Named piece of data; may be required (non-nullable, no default) or optional.
- **Generic Class**: Class parameterized by one or more type parameters whose concrete types influence constructor parameter types and field types.

## Diagnostic Codes *(mandatory)*

All constructor-related diagnostics use structured codes CTOR001–CTOR010 with JSON-formatted output.

| Code | Severity | Triggered By | Message Pattern | Related FR |
|------|----------|--------------|-----------------|------------|
| CTOR001 | Error | No matching constructor found | "No constructor found for class '{ClassName}' matching arguments ({ArgTypes})" | FR-CTOR-007, FR-CTOR-013 |
| CTOR002 | Error | Ambiguous overload | "Ambiguous constructor call for class '{ClassName}'. Candidates: {CandidateSignatures}" | FR-CTOR-007 |
| CTOR003 | Error | Unassigned required field | "Constructor for '{ClassName}' does not assign required fields: {FieldList}" | FR-CTOR-009 |
| CTOR004 | Error | Missing base constructor call | "Constructor for '{ClassName}' must invoke base constructor; base class '{BaseClassName}' has no parameterless constructor" | FR-CTOR-010, FR-CTOR-011 |
| CTOR005 | Error | Cannot synthesize parameterless | "Cannot synthesize parameterless constructor for '{ClassName}'; required fields lack defaults: {FieldList}" | FR-CTOR-004, FR-CTOR-005 |
| CTOR006 | Error | Duplicate constructor signature | "Duplicate constructor signature for '{ClassName}': {Signature}" | FR-CTOR-006 |
| CTOR007 | Error | Invalid constructor type parameter | "Constructor '{ClassName}' cannot declare independent type parameters; only class-level type parameters allowed" | FR-CTOR-003 |
| CTOR008 | Error | Cyclic base constructor | "Cyclic base constructor dependency detected: {CyclePath}" | FR-CTOR-016 |
| CTOR009 | Error | Value return in constructor | "Constructor '{ClassName}' cannot return a value; use 'return;' without expression" | FR-CTOR-008 |
| CTOR010 | Error | Forbidden modifier | "Constructor '{ClassName}' has forbidden modifier '{Modifier}'; constructors cannot be {ModifierList}" | FR-CTOR-015 |

### Diagnostic JSON Schema
```json
{
  "code": "CTOR00X",
  "severity": "Error",
  "message": "...",
  "class": "ClassName",
  "signature": "ClassName(ParamTypes)",
  "location": {
    "file": "path/to/file.5th",
    "line": 42,
    "column": 10
  },
  "hint": "Actionable suggestion..."
}
```

## Success Criteria *(mandatory)*

### Measurable Outcomes
- **SC-001**: Developers can define a working constructor for a class with two required fields in ≤5 lines and instantiate without diagnostics.
- **SC-002**: 100% of required field omissions produce a single clear diagnostic listing all missing fields.
- **SC-003**: Ambiguous overload cases always produce an ambiguity diagnostic; no silent mis-selection.
- **SC-004**: Parameterless synthesis occurs only when all required fields are defaulted/nullable (verified across test suite).
- **SC-005**: Generic class constructors correctly reflect concrete type arguments across ≥5 distinct instantiation tests.
- **SC-006**: Inheritance tests show mandatory base invocation enforced with 0 false positives.
- **SC-007**: Compile-time performance increase for representative projects using constructors remains <5% versus baseline (business acceptable responsiveness).
- **SC-008**: All eight diagnostic codes are triggered in negative tests with structured formatting.
- **SC-009**: No existing non-constructor code requires modification (0 regressions in prior test corpus).
- **SC-010**: Documentation includes ≥5 runnable examples demonstrating constructor forms (basic, overload, inheritance, generic, error).

## Edge Cases (Detailed)
- Instantiating a generic class with `null` where type parameter is unconstrained.
- Multiple constructors differing only by parameter name (treated as duplicate signature).
- Base class parameterless removal causing derived class diagnostics until updated.
- Return with expression inside constructor (invalid form) triggers modifier/value diagnostic.
- Shadowed parameter identical to field; missing `this.` leads to assigning parameter to itself (prevent via rule enforcement).
- Cyclic inheritance mis-configuration causing repeated base invocation attempt.

## Assumptions
- Language already supports class definitions, fields, inheritance, generics (reified) and `new` instantiation syntax.
- Destructuring and parameter constraints behave consistently inside constructor parameter lists where allowed.
- No asynchronous construction needed for current use cases.
- Constructors do not introduce new visibility semantics beyond existing method rules.

## Dependencies
- Existing generics feature for type parameter binding.
- Existing diagnostics framework for structured messages.
- Inheritance model ensuring clear identification of base constructors.

## Out of Scope
- Independent generic parameters per constructor.
- Constructor chaining within same class (`this(...)`).
- Default parameter values, copy constructors, destructors/finalizers.
- Async constructors and custom allocation strategies.
- Memory arenas and pooling policies.

## No Clarifications Needed
All requirements are concrete; no [NEEDS CLARIFICATION] markers remain.
