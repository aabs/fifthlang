# Phase 1 Design: Data Model (AST + Runtime Shapes)

## Scope

This document defines the concrete AST/data shapes needed to implement:
- General list comprehensions with updated keywords (`from` / `where`)
- SPARQL comprehensions over tabular SELECT results

It is intentionally focused on contracts between parser → AST → transformations → codegen/runtime.

## AST Entities

### `ListComprehension` (existing; to be revised)

**Current location**: `src/ast-model/AstMetamodel.cs`

**Current issues**:
- `SourceName: string` prevents typechecking and lowering.
- Single `MembershipConstraint` does not support multiple `where` constraints.
- No explicit `Projection` node; assumes only variable projection.

**Proposed shape (language-level)**

- `Projection: Expression`
  - Variable projection: `VarRefExp` / `VarName`
  - Object projection: `ObjectInstantiationExp` (existing object instantiation expression)
- `Source: Expression`
  - General comprehension: any enumerable/list-like source
  - SPARQL comprehension: expression whose static type is a tabular SELECT result
- `Constraints: List<Expression>`
  - Each constraint must typecheck to boolean

**Notes**:
- Keep `ListComprehension` as a subtype of `List : Expression` so it remains an expression returning a list.
- Add location/source info consistently (existing AST uses `Location` and `Parent` on most nodes).

### `LCompProjection` (optional helper)

If the existing AST patterns make it hard to distinguish projection cases purely by `Expression`, introduce a small tagged union:
- `VarProjection(VarName: string)`
- `ExpressionProjection(Expression)`

This is optional; simplest approach is `Projection: Expression` with validation based on shape.

## Semantic Entities

### `TabularResult`

**Runtime shape**: `Result.TabularResult(SparqlResultSet ResultSet)` in `src/fifthlang.system/Result.cs`.

### `Row Binding`

A SPARQL result row maps variable names (e.g. `name`) to RDF node values.

**Proposed runtime helper surface** (likely in `src/fifthlang.system/`):
- `TryGetBinding(row, "name") -> (bool found, object value)`
- `GetBindingOrThrow(row, "name") -> object` (throws a clear runtime error for FR-011a)
- `ConvertBinding(value, targetType) -> <typed>`

(Exact API depends on how Fifth runtime represents values and how codegen emits conversions.)

## State / Transitions

- `ListComprehension` is an expression node and does not require explicit state transitions.
- Lowering pass transforms it into:
  - List allocation
  - Loop over source items/rows
  - Constraint checks
  - Append projected value
  - Return list

## Validation Rules (from spec)

- Generator must typecheck to tabular SELECT result for SPARQL comprehensions (FR-005).
- Constraints must be boolean (FR-007).
- Object projection property RHS must be `?varName` for SPARQL object projections (FR-008).
- Referencing unknown SPARQL variables is compile-time error (FR-009/FR-009a).
- Referencing unknown projected properties is compile-time error (FR-010).
- Missing/unbound bindings at runtime throw a clear error when accessed (FR-011a).

## Open Design Point

**Constraint scoping for object projections**: propose an implicit `it` variable (or similar) bound to the projected object, so constraints use `it.Prop` and standard member access typing.
