# Data Model: Query Application and Result Type

**Feature**: Query Application and Result Type  
**Date**: 2025-11-15  
**Status**: Complete

## Purpose

This document defines all entities, types, and relationships introduced by the query application feature. Each entity includes field specifications, validation rules, and state transitions where applicable.

## Entity Catalog

### 1. Result (Discriminated Union)

**Location**: `src/fifthlang.system/ResultType.cs`  
**Purpose**: Represents the outcome of applying a SPARQL query to a store. Discriminated union with three cases based on query form.

**Type Definition**:
```csharp
[Union]
public abstract partial class Result
{
    // Case 1: SELECT query results (tabular data)
    partial record TabularResult(SparqlResultSet ResultSet) : Result;
    
    // Case 2: CONSTRUCT/DESCRIBE query results (RDF graph)
    partial record GraphResult(Store GraphStore) : Result;
    
    // Case 3: ASK query results (boolean truth value)
    partial record BooleanResult(bool Value) : Result;
}
```

**Fields**:

#### TabularResult Case
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| ResultSet | SparqlResultSet | dotNetRDF result set containing rows and variable bindings | Non-null |

**Accessors**:
- `IEnumerable<SparqlResult> Rows { get; }` - Lazy iteration over result rows
- `IReadOnlyList<string> Variables { get; }` - SPARQL variable names (e.g., "x", "y")
- `int RowCount { get; }` - Total row count (may force materialization)
- `T GetValue<T>(int rowIndex, string varName)` - Type-safe value access

#### GraphResult Case
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| GraphStore | Store | Fifth.System Store wrapping the constructed/described RDF graph | Non-null, graph count ≥ 0 |

**Accessors**:
- `Store GetStore()` - Access to underlying Store for further querying
- `string ToTriG()` - Serialize graph as TriG format
- `int TripleCount { get; }` - Total triples across all graphs

#### BooleanResult Case
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Value | bool | Truth value from ASK query evaluation | N/A (primitive) |

**Relationships**:
- Result wraps dotNetRDF's `SparqlResultSet` (TabularResult) or Fifth.System's `Store` (GraphResult)
- Result is returned by `QueryApplicationExp` AST node after lowering to Fifth.System execution
- Result instances are immutable after creation

**State Transitions**: None (immutable value object)

**Validation Rules**:
- TabularResult: `ResultSet` must be non-null
- GraphResult: `GraphStore` must be non-null and contain valid RDF
- BooleanResult: No validation (boolean primitive)
- Discriminated union ensures exactly one case is active at runtime

---

### 2. QueryError

**Location**: `src/fifthlang.system/QueryError.cs`  
**Purpose**: Structured diagnostics object for failed query applications. Provides actionable error information with categorized failure modes.

**Type Definition**:
```csharp
public sealed class QueryError
{
    public ErrorKind Kind { get; init; }
    public string Message { get; init; }
    public SourceSpan? SourceSpan { get; init; }
    public string? UnderlyingExceptionType { get; init; }
    public string? Suggestion { get; init; }
}

public enum ErrorKind
{
    SyntaxError,          // Malformed SPARQL query text
    ValidationError,      // Semantic/constraint violations
    ExecutionError,       // Runtime query processing failure
    Timeout,              // Exceeded execution time limit
    Cancellation,         // User-requested cancellation
    SecurityWarning,      // Unsafe query composition detected
    ResourceLimit,        // Memory/result size exceeded
    ConcurrencyConflict   // Conflicting concurrent operation
}
```

**Fields**:
| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| Kind | ErrorKind | Yes | Error category from 8-value enumeration | Must be valid enum value |
| Message | string | Yes | Human-readable error description | Non-empty, ≤ 500 chars |
| SourceSpan | SourceSpan? | No | Location in query text (line, column, length) | If present, line > 0, column > 0 |
| UnderlyingExceptionType | string? | No | Original dotNetRDF exception type name | If present, non-empty |
| Suggestion | string? | No | Actionable fix hint (target: ≥80% coverage per SC-010) | If present, ≤ 200 chars |

**ErrorKind Enumeration Semantics**:
| Kind | Trigger Condition | Typical Message | Example Suggestion |
|------|-------------------|-----------------|-------------------|
| SyntaxError | RdfParseException from dotNetRDF | "SPARQL syntax error at line 3, column 15" | "Check for missing semicolon after triple pattern" |
| ValidationError | SparqlQueryException (semantic) | "Undefined variable ?z in SELECT clause" | "Ensure all projected variables appear in graph patterns" |
| ExecutionError | SparqlQueryException (runtime) | "Function evaluation failed in FILTER clause" | "Verify function arguments match expected types" |
| Timeout | RdfQueryTimeoutException | "Query exceeded 30 second timeout" | "Simplify graph pattern or increase timeout limit" |
| Cancellation | OperationCanceledException | "Query cancelled by user request" | N/A (user-initiated) |
| SecurityWarning | SparqlSecurityValidator | "Unbalanced braces detected, possible injection" | "Review query construction, consider bind() API" |
| ResourceLimit | OutOfMemoryException | "Result set exceeded 500MB memory limit" | "Add LIMIT clause or stream results iteratively" |
| ConcurrencyConflict | Future coordinator exception | "Write conflict with concurrent transaction" | "Retry operation or use pessimistic locking" |

**Relationships**:
- QueryError is produced by `QueryApplicationExecutor.Execute()` on failure path
- Mapped from dotNetRDF exceptions via switch-based handler (see research.md)
- SecurityWarning generated independently by `SparqlSecurityValidator` pre-execution

**State Transitions**: None (immutable value object)

**Validation Rules**:
- Kind must be one of 8 defined values
- Message required, non-empty
- SourceSpan coordinates must be positive if present
- Suggestion recommended (≥80% coverage) but not required

---

### 3. QueryApplicationExp (AST Node)

**Location**: `src/ast-model/AstMetamodel.cs`  
**Purpose**: High-level AST node representing query application operation (`query <- store`). Lowers to Fifth.System API calls during transformation.

**Type Definition**:
```csharp
public sealed class QueryApplicationExp : Expression
{
    public Expression Query { get; init; }      // Left operand (must be Query type)
    public Expression Store { get; init; }      // Right operand (must be Store type)
    public TypeInfo? InferredType { get; set; } // Set to Result type by type inference pass
}
```

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Query | Expression | Query expression (LHS of `<-`) | Must type-check to Query type |
| Store | Expression | Store expression (RHS of `<-`) | Must type-check to Store type |
| InferredType | TypeInfo? | Result type (set by TypeAnnotationVisitor) | Must be Result after type checking |

**Relationships**:
- Parent: Expression (inherits from AST base class)
- Children: Query (Expression), Store (Expression)
- Lowered by: `QueryApplicationLoweringRewriter` transforms to function call expression invoking Fifth.System API
- Type-checked by: `QueryApplicationTypeCheckVisitor` validates operand types

**State Transitions**:
1. **Created**: Parsed by AstBuilderVisitor from grammar rule
2. **Type-Checked**: QueryApplicationTypeCheckVisitor validates Query/Store types
3. **Security-Validated**: SparqlSecurityValidator inspects Query expression for unsafe patterns
4. **Lowered**: QueryApplicationLoweringRewriter transforms to `Result Fifth.System.QueryApplicationExecutor.Execute(Query, Store, CancellationToken?)`

**Validation Rules**:
- Query operand must have type assignable to `Query`
- Store operand must have type assignable to `Store` (or SPARQL-queryable interface)
- Both operands must be non-null expressions
- Type inference must successfully resolve to `Result` type

---

### 4. SourceSpan (Value Object)

**Location**: `src/ast-model/TypeSystem/SourceSpan.cs` (likely already exists; define if missing)  
**Purpose**: Represents a location in source code for diagnostic reporting.

**Type Definition**:
```csharp
public readonly record struct SourceSpan(int Line, int Column, int Length, string? FilePath = null);
```

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Line | int | 1-based line number | > 0 |
| Column | int | 1-based column number | > 0 |
| Length | int | Character span length | ≥ 0 |
| FilePath | string? | Optional source file path | If present, non-empty |

**Relationships**:
- Embedded in QueryError for error location reporting
- May be extracted from RdfParseException line/column info

**Validation Rules**:
- Line and Column must be positive
- Length must be non-negative
- FilePath optional (null for dynamic queries)

---

### 5. Supporting Grammar Entities

#### QUERY_APPLICATION_OP Token

**Location**: `src/parser/grammar/FifthLexer.g4`

**Definition**:
```antlr
QUERY_APPLICATION_OP : '<-' ;
```

**Precedence**: Defined before comparison operators (`<`, `>`) to ensure longest-match rule applies.

#### queryApplicationExpr Parser Rule

**Location**: `src/parser/grammar/FifthParser.g4`

**Definition**:
```antlr
queryApplicationExpr
    : expression QUERY_APPLICATION_OP expression  # QueryApplication
    ;
```

**Integration**: Add to `expression` rule alternatives or appropriate precedence level (likely primary expression or infix operation).

---

## Relationships Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      Query Application                      │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ syntax: query <- store
                           ▼
                  ┌──────────────────┐
                  │ Grammar Parser   │
                  │ (FifthParser.g4) │
                  └──────────────────┘
                           │
                           │ creates
                           ▼
              ┌────────────────────────────┐
              │  QueryApplicationExp (AST) │
              │  - Query: Expression       │
              │  - Store: Expression       │
              └────────────────────────────┘
                           │
                           │ validated by
                           ▼
          ┌─────────────────────────────────────┐
          │ QueryApplicationTypeCheckVisitor    │
          │ (ensures Query/Store types correct) │
          └─────────────────────────────────────┘
                           │
                           │ secured by
                           ▼
               ┌────────────────────────┐
               │ SparqlSecurityValidator│
               │ (detects injection)    │
               └────────────────────────┘
                           │
                           │ lowered by
                           ▼
        ┌───────────────────────────────────────┐
        │ QueryApplicationLoweringRewriter      │
        │ (transforms to System API call)       │
        └───────────────────────────────────────┘
                           │
                           │ runtime execution
                           ▼
            ┌──────────────────────────────┐
            │ QueryApplicationExecutor     │
            │ Fifth.System.Execute()       │
            └──────────────────────────────┘
                           │
                    ┌──────┴──────┐
                    │             │
               Success          Failure
                    │             │
                    ▼             ▼
              ┌─────────┐   ┌────────────┐
              │ Result  │   │ QueryError │
              │ (union) │   │ - Kind     │
              │         │   │ - Message  │
              │ Cases:  │   │ - Span     │
              │ - Tabular│  │ - Suggest  │
              │ - Graph  │  └────────────┘
              │ - Boolean│
              └─────────┘
```

---

## Validation Summary

| Entity | Key Validation Rules | Enforcement Point |
|--------|---------------------|------------------|
| Result | Non-null case fields, valid union discriminant | Runtime construction, pattern match exhaustiveness |
| QueryError | Kind in enum, Message non-empty, SourceSpan coords > 0 | Factory methods in QueryApplicationExecutor |
| QueryApplicationExp | Query/Store operands type-correct, non-null | QueryApplicationTypeCheckVisitor compile-time pass |
| SourceSpan | Line/Column > 0, Length ≥ 0 | Construction validation in record struct |

---

## Next Phase

Proceed to Phase 1 (continued): Create `contracts/` directory with detailed API specifications for Result, QueryError, and QueryApplicationExecutor runtime interfaces.
