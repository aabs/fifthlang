# Data Model and Type System Additions

## AST Nodes
- `GraphAssertionBlockExp` (expression):
  - Properties: `Statements: List<Statement>`, `SourceSpan`, `Annotations`
  - Yields: `graph`
- `GraphAssertionBlockStmt` (statement):
  - Same shape as expression; context determines auto-persistence to default graph

## Types
- `store`: persistent store handle (maps to `IUpdateableStorage`)
- `graph`: a collection of triples or a named graph (maps to `IGraph`)
- `triple`: a single fact (maps to `Triple`)
- `iri`: an absolute IRI (maps to `Uri`)

## Type Rules
- `graph += graph`: OK → merges assertions; result `graph`
- `store += graph`: OK → persists assertions; result `store`
- `graphAssertionBlock` as expression: type `graph`
- Standalone `graphAssertionBlock` statement: implicit persistence to default store (compile-time desugaring)

## Inference
- Within `graphAssertionBlock`, mutations to assertable objects contribute to the block’s resulting `graph` value.
- Nested `graphAssertionBlock` values merge into the enclosing block graph unless explicitly persisted inside.

## Lowering Contract
- Expression form: Lower to builder creating an `IGraph`, then emit calls to `KG` functions for assertions; return `IGraph`.
- Statement form: Same as expression, then add explicit `store += graph` operation targeting default store.
