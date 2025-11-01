# Data Model: Exception Handling (AST)

## Entities

### TryStatement
- TryBlock: Block (required)
- CatchClauses: List<CatchClause> (ordered)
- FinallyBlock: Block? (optional)

### CatchClause
- ExceptionType: Type? (optional)
- ExceptionIdentifier: Identifier? (optional)
- Filter: Expression? (optional)
- Body: Block (required)

### ThrowExp (throw expression)
- Exception: Expression (required; must be or convert to System.Exception)

### ThrowStatement (existing)
- Exception: Expression? (optional; absent means rethrow `throw;`)

## Relationships & Rules
- Catch selection: first assignment-compatible catch whose filter (if any) is true
- Exception identifier scope: visible in filter and catch body
- Re-throw: `throw;` preserves stack; `throw ex;` resets per C#
- Iterator/async-iterator contexts: deferred in v1 (diagnostic if used)

## Validation
- Catch type must derive from System.Exception
- Filter must be boolean-convertible
- ThrowExp.Exception type must derive from System.Exception
- Unreachable catch due to ordering/type shadowing: compile-time error
