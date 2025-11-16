# API Contract: Query Application Operator (`<-`)

**Syntax**: `result: Result = query <- store`  
**Operator Symbol**: `<-`  
**Introduced**: Feature 011-query-application-result-type  
**Version**: 1.0.0

## Overview

The query application operator (`<-`) applies a SPARQL query (left operand) to an RDF store (right operand), returning a `Result` discriminated union. This operator provides syntactic sugar for querying knowledge graphs with minimal noise.

## Syntax

### Basic Form

```fifth
result: Result = query <- store;
```

**Components**:
- **Left Operand (query)**: Expression of type `Query` (created via `?<...>` SPARQL literal syntax)
- **Operator**: `<-` (query application operator)
- **Right Operand (store)**: Expression of type `Store` (or SPARQL-queryable interface)
- **Result**: Value of type `Result` (discriminated union: TabularResult | GraphResult | BooleanResult)

### With Explicit Query Literals

```fifth
// Inline SPARQL query literal
result = ?<SELECT ?name WHERE { ?person <name> ?name }> <- myStore;

// Pre-defined query variable
selectQuery: Query = ?<SELECT ?x ?y WHERE { ?x <related> ?y }>;
result = selectQuery <- myStore;
```

### With Optional Cancellation

```fifth
// Using cancellation token (future feature)
token: CancellationToken = getCancellationToken();
result = query <- store withCancel: token;
// (Syntax TBD; may lower to Execute(query, store, token) call)
```

---

## Semantics

### Type Constraints

**Compile-Time Requirements**:
1. Left operand MUST be assignable to `Query` type
2. Right operand MUST be assignable to `Store` type (or implement SPARQL-queryable interface)
3. Result type is always `Result` (inferred by compiler)

**Type Checking**:
```csharp
// Valid
query: Query = ?<ASK WHERE { ... }>;
store: Store = getStore();
result: Result = query <- store;  // OK

// Invalid (compile-time errors)
result = "not a query" <- store;     // Error: LHS not Query type
result = query <- "not a store";     // Error: RHS not Store type
result = query <- 42;                // Error: RHS not Store type
```

### Evaluation Order

1. **Left operand evaluated first**: Query expression computed
2. **Right operand evaluated second**: Store expression computed
3. **Operator application**: Query applied to Store via runtime executor
4. **Result construction**: Based on SPARQL query form (SELECT → TabularResult, etc.)

### Query Form Determination

Result discriminated union case is determined by SPARQL query form:

| SPARQL Form | Result Case | Example |
|-------------|-------------|---------|
| SELECT | TabularResult | `?<SELECT ?x WHERE { ... }> <- store` |
| CONSTRUCT | GraphResult | `?<CONSTRUCT { ?s ?p ?o } WHERE { ... }> <- store` |
| DESCRIBE | GraphResult | `?<DESCRIBE <http://example.org/resource> > <- store` |
| ASK | BooleanResult | `?<ASK WHERE { ?s a <Person> }> <- store` |

**Runtime Behavior**: dotNetRDF's `SparqlResultSet.ResultsType` property determines which Result case to construct.

---

## Lowering Strategy

The `<-` operator is syntactic sugar that lowers to a Fifth.System API call during compiler transformation:

### AST Representation

**High-Level AST Node**:
```csharp
public sealed class QueryApplicationExp : Expression
{
    public Expression Query { get; init; }
    public Expression Store { get; init; }
}
```

**Lowering Pass** (`QueryApplicationLoweringRewriter`):
```csharp
// Input AST:
QueryApplicationExp {
    Query = varRef("myQuery"),
    Store = varRef("myStore")
}

// Lowered to:
FuncCallExp {
    FunctionName = "Fifth.System.QueryApplicationExecutor.Execute",
    Arguments = [varRef("myQuery"), varRef("myStore"), null /* cancellation token */]
}
```

### Generated C# Code (via Roslyn Backend)

```csharp
// Fifth source:
result = query <- store;

// Generated C# equivalent:
var result = Fifth.System.QueryApplicationExecutor.Execute(query, store, null);
```

---

## Runtime Execution

### Executor Signature

```csharp
public static class QueryApplicationExecutor
{
    public static Result Execute(
        Query query, 
        Store store, 
        CancellationToken? cancellationToken = null)
    {
        // 1. Validate query text (SparqlSecurityValidator)
        var validationErrors = SparqlSecurityValidator.Validate(query.Text);
        if (validationErrors.Any(e => e.Kind == ErrorKind.SecurityWarning))
            throw new QueryExecutionException(validationErrors.First());
        
        // 2. Acquire read lock for concurrency isolation (FR-016)
        using var lockHandle = store.AcquireReadLock();
        
        // 3. Execute query via dotNetRDF
        var processor = new LeviathanQueryProcessor(store.TripleStore);
        var options = new SparqlQueryEvaluationOptions { Timeout = cancellationToken };
        
        try
        {
            cancellationToken?.ThrowIfCancellationRequested();
            var results = processor.ProcessQuery(query.SparqlQuery, options);
            
            // 4. Convert dotNetRDF results to Result union
            return results.ResultsType switch
            {
                SparqlResultsType.Boolean => new BooleanResult(results.Result),
                SparqlResultsType.VariableBindings => new TabularResult(results),
                SparqlResultsType.Graph => new GraphResult(new Store(results.Graph)),
                _ => throw new InvalidOperationException($"Unsupported result type: {results.ResultsType}")
            };
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException(QueryErrorFactory.FromException(ex, query.Text));
        }
    }
}
```

### Error Handling

Execution failures throw `QueryExecutionException` containing a `QueryError`:

```csharp
public class QueryExecutionException : Exception
{
    public QueryError Error { get; }
    
    public QueryExecutionException(QueryError error) 
        : base(error.Message) 
    {
        Error = error;
    }
}
```

---

## Performance Characteristics

### Time Complexity
- **Operator evaluation**: O(1) (AST node creation)
- **Query execution**: Depends on SPARQL query complexity and dataset size (dotNetRDF's responsibility)
- **Result construction**: O(1) for BooleanResult, O(1) for GraphResult wrapper, O(1) for TabularResult wrapper (iteration is lazy)

### Space Complexity
- **SELECT (TabularResult)**: O(1) wrapper + streaming iteration (target: <1.5× baseline memory per FR-013)
- **CONSTRUCT/DESCRIBE (GraphResult)**: O(n) where n = number of constructed triples
- **ASK (BooleanResult)**: O(1) (single boolean)

### Concurrency
- **Read/read**: Parallel query applications allowed (FR-016)
- **Read/write**: Future write operations will acquire write lock, blocking reads
- **Overhead**: ReaderWriterLockSlim adds ~microseconds per lock acquire (negligible for query latencies)

---

## Examples

### SELECT Query

```fifth
store: Store = loadStore("data.trig");
selectQuery: Query = ?<
    PREFIX ex: <http://example.org/>
    SELECT ?name ?age
    WHERE {
        ?person ex:name ?name ;
                ex:age ?age .
        FILTER (?age > 18)
    }
>;

result = selectQuery <- store;

result switch {
    TabularResult t => {
        std.print("Found " + t.RowCount.toString() + " adults:");
        foreach (row in t.Rows) {
            std.print(row["name"].toString() + " is " + row["age"].toString());
        }
    },
    _ => std.print("Unexpected result type")
}
```

### CONSTRUCT Query

```fifth
constructQuery: Query = ?<
    CONSTRUCT {
        ?person <summary> ?info
    }
    WHERE {
        ?person <name> ?name ;
                <age> ?age .
        BIND(CONCAT(?name, " (", STR(?age), ")") AS ?info)
    }
>;

result = constructQuery <- store;

result switch {
    GraphResult g => {
        std.writeFile("summary.trig", g.ToTriG());
    },
    _ => std.print("Expected graph result")
}
```

### ASK Query

```fifth
askQuery: Query = ?<
    ASK WHERE { 
        ?person <age> ?age .
        FILTER (?age < 18)
    }
>;

result = askQuery <- store;

result switch {
    BooleanResult b => std.print(if b.Value "Has minors" else "No minors"),
    _ => std.print("Expected boolean result")
}
```

### Error Handling

```fifth
try {
    // Potentially malformed query
    result = ?<SELECT ?x WHERE { ?x <invalid> > <- store;
} catch (QueryExecutionException ex) {
    ex.Error.Kind switch {
        ErrorKind.SyntaxError => std.print("Syntax error: " + ex.Error.Message),
        ErrorKind.Timeout => std.print("Query timed out, try simpler pattern"),
        ErrorKind.SecurityWarning => {
            std.logWarning("SECURITY ALERT: " + ex.Error.Message);
            std.print("Review query construction: " + ex.Error.Suggestion);
        },
        _ => std.print("Query failed: " + ex.Error.Message)
    }
}
```

---

## Operator Precedence

`<-` has **lower precedence than function calls** and **higher precedence than assignment**:

```fifth
// Parentheses recommended for clarity
result = (getQuery()) <- (getStore());

// Implicit precedence:
result = getQuery() <- getStore();  // OK: function calls evaluated first

// Assignment has lowest precedence:
x = query <- store;  // OK: query application evaluated, then assigned to x
```

---

## Comparison with Alternative Syntaxes

| Syntax | Pros | Cons |
|--------|------|------|
| `query <- store` (chosen) | Concise, reads "query from store" | Uncommon operator in C-family languages |
| `store.execute(query)` | Familiar method call | Verbose, 3 extra chars + dot notation |
| `store -> query` | Consistent with functional arrow | Reads backward ("store to query"?) |
| `query @ store` | Shortest | `@` overloaded in many contexts |
| `query.applyTo(store)` | Most explicit | Verbose, breaks fluent style |

**Decision Rationale**: `<-` balances conciseness with semantic clarity (query "drawn from" store). Precedent: Haskell's `<-` for monadic binding.

---

## Constraints & Limitations

### MVP Constraints
- No query composition (e.g., `(query1 <- store1) <- store2` not supported)
- No operator overloading for custom store types (must implement SPARQL-queryable interface)
- No short-circuit evaluation (both operands always evaluated)

### Future Enhancements
- Async/await support: `result = await query <-async store;`
- Query caching: `result = query <- store withCache;`
- Distributed query federation: `result = query <- [store1, store2, store3];`

---

## Testing Strategy

### Grammar Tests (`test/syntax-parser-tests/`)
- Verify `<-` token recognized by lexer
- Confirm precedence relative to other operators
- Test edge cases (whitespace around operator, comments)

### Type Checking Tests (`test/ast-tests/`)
- Validate compile-time rejection of non-Query LHS
- Validate compile-time rejection of non-Store RHS
- Confirm Result type inference

### Integration Tests (`test/runtime-integration-tests/`)
- Execute all 4 SPARQL forms (SELECT, CONSTRUCT, DESCRIBE, ASK)
- Verify correct Result case construction
- Test error handling (syntax errors, timeouts, cancellation)

---

## Success Criteria Validation

| Criterion | Validation Approach |
|-----------|---------------------|
| SC-001 (2 lines max) | Code review: `query <- store` is 1 line |
| SC-002 (100% correct discrimination) | Integration tests for all 4 forms |
| SC-003 (100% compile-time type checking) | Unit tests with invalid operands |
| SC-005 (50% noise reduction) | Measure token count vs equivalent C# dotNetRDF code |

---

## Security Considerations

- **Injection risks**: SparqlSecurityValidator runs pre-execution (FR-012)
- **Resource exhaustion**: Timeout and ResourceLimit errors prevent DoS
- **Data leakage**: Result may expose sensitive RDF data; apply access control
- **Audit logging**: Log all query applications with timestamps and user context

---

## Compatibility

- **Minimum Language Version**: Fifth 1.0 (this feature introduces operator)
- **Breaking Changes**: None (new operator, no conflicts with existing syntax)
- **Backward Compatibility**: Existing code unaffected (no `<-` usage prior to this feature)
