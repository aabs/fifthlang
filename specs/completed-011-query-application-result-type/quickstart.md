# Quickstart: Query Application and Result Type

**Feature**: Query Application and Result Type  
**Audience**: Fifth language developers  
**Time to Complete**: 10 minutes

## What You'll Learn

- How to apply SPARQL queries to RDF stores using the `<-` operator
- How to handle different result types (tabular, graph, boolean)
- How to process query results with pattern matching
- How to handle errors with structured diagnostics

## Prerequisites

- Fifth language environment installed (compiler + runtime)
- Basic familiarity with SPARQL query syntax
- Understanding of Fifth's type system and pattern matching

## Step 1: Create an RDF Store

First, create a store with sample data:

```fifth
// Create store from TriG literal (feature 009)
store: Store = @<{
    <http://example.org/person1> <http://example.org/name> "Alice" ;
                                 <http://example.org/age> 30 ;
                                 <http://example.org/city> "Boston" .
    
    <http://example.org/person2> <http://example.org/name> "Bob" ;
                                 <http://example.org/age> 25 ;
                                 <http://example.org/city> "Seattle" .
    
    <http://example.org/person3> <http://example.org/name> "Charlie" ;
                                 <http://example.org/age> 35 ;
                                 <http://example.org/city> "Boston" .
}>;
```

## Step 2: Execute a SELECT Query

Query for all person names and ages:

```fifth
// Define SPARQL query using ?<...> syntax
selectQuery: Query = ?<
    PREFIX ex: <http://example.org/>
    
    SELECT ?name ?age
    WHERE {
        ?person ex:name ?name ;
                ex:age ?age .
    }
    ORDER BY DESC(?age)
>;

// Apply query to store using <- operator
result: Result = selectQuery <- store;

// Process tabular results
result switch {
    TabularResult t => {
        std.print("Found " + t.RowCount.toString() + " people:");
        
        foreach (row in t.Rows) {
            name: string = row["name"].toString();
            age: int = row["age"].asInteger();
            std.print("  " + name + " is " + age.toString() + " years old");
        }
    },
    _ => std.print("Unexpected result type")
}
```

**Output**:
```
Found 3 people:
  Charlie is 35 years old
  Alice is 30 years old
  Bob is 25 years old
```

## Step 3: Execute a CONSTRUCT Query

Build a new graph with derived data:

```fifth
// Construct graph with person summaries
constructQuery: Query = ?<
    PREFIX ex: <http://example.org/>
    
    CONSTRUCT {
        ?person ex:summary ?summary .
    }
    WHERE {
        ?person ex:name ?name ;
                ex:age ?age ;
                ex:city ?city .
        
        BIND(CONCAT(?name, " (", STR(?age), ", ", ?city, ")") AS ?summary)
    }
>;

// Apply query and get graph result
graphResult: Result = constructQuery <- store;

graphResult switch {
    GraphResult g => {
        std.print("Constructed graph with " + g.TripleCount.toString() + " triples");
        std.print(g.ToTriG());
        
        // Can query the constructed graph
        verifyQuery: Query = ?<SELECT ?s ?summary WHERE { ?s <http://example.org/summary> ?summary }>;
        verification: Result = verifyQuery <- g.GraphStore;
    },
    _ => std.print("Expected graph result")
}
```

**Output**:
```
Constructed graph with 3 triples
@prefix ex: <http://example.org/> .

ex:person1 ex:summary "Alice (30, Boston)" .
ex:person2 ex:summary "Bob (25, Seattle)" .
ex:person3 ex:summary "Charlie (35, Boston)" .
```

## Step 4: Execute an ASK Query

Check for existence of data:

```fifth
// Check if any person lives in Boston
askQuery: Query = ?<
    PREFIX ex: <http://example.org/>
    
    ASK WHERE {
        ?person ex:city "Boston" .
    }
>;

boolResult: Result = askQuery <- store;

boolResult switch {
    BooleanResult b => {
        message: string = if b.Value 
            "Found people in Boston" 
            else "No people in Boston";
        std.print(message);
    },
    _ => std.print("Expected boolean result")
}
```

**Output**:
```
Found people in Boston
```

## Step 5: Handle Errors

Catch and process query execution errors:

```fifth
// Intentionally malformed query
malformedQuery: Query = ?<
    SELECT ?x WHERE { ?x <invalid>
    // Missing closing brace
>;

try {
    badResult: Result = malformedQuery <- store;
} catch (QueryExecutionException ex) {
    std.print("Query failed!");
    
    // Access structured error details
    error: QueryError = ex.Error;
    
    error.Kind switch {
        ErrorKind.SyntaxError => {
            std.print("Syntax error: " + error.Message);
            if (error.SourceSpan != null) {
                span: SourceSpan = error.SourceSpan;
                std.print("  at line " + span.Line.toString() + 
                         ", column " + span.Column.toString());
            }
            if (error.Suggestion != null) {
                std.print("Suggestion: " + error.Suggestion);
            }
        },
        ErrorKind.Timeout => {
            std.print("Query timed out: " + error.Message);
            std.print("Try simplifying the graph pattern");
        },
        ErrorKind.SecurityWarning => {
            std.logWarning("SECURITY ALERT: " + error.Message);
            std.print("Review query construction");
        },
        _ => std.print("Error: " + error.Message)
    }
}
```

**Output**:
```
Query failed!
Syntax error: SPARQL syntax error at line 2, column 30: unexpected end of input
  at line 2, column 30
Suggestion: Check for missing closing brace in graph pattern
```

## Step 6: Filter Results

Use SPARQL FILTER to constrain results:

```fifth
// Find people over 30 in Boston
filteredQuery: Query = ?<
    PREFIX ex: <http://example.org/>
    
    SELECT ?name ?age
    WHERE {
        ?person ex:name ?name ;
                ex:age ?age ;
                ex:city "Boston" .
        
        FILTER (?age > 30)
    }
>;

filteredResult: Result = filteredQuery <- store;

filteredResult switch {
    TabularResult t => {
        if (t.RowCount == 0) {
            std.print("No results found");
        } else {
            foreach (row in t.Rows) {
                std.print(row["name"].toString() + ": " + row["age"].toString());
            }
        }
    },
    _ => {}
}
```

**Output**:
```
Charlie: 35
```

## Complete Example

Here's a complete Fifth program using query application:

```fifth
main(): int {
    // Create sample store
    store: Store = @<{
        <http://example.org/person1> <http://example.org/name> "Alice" ;
                                     <http://example.org/age> 30 .
        <http://example.org/person2> <http://example.org/name> "Bob" ;
                                     <http://example.org/age> 25 .
    }>;
    
    // Query for adults (age >= 18)
    adultsQuery: Query = ?<
        PREFIX ex: <http://example.org/>
        SELECT ?name ?age
        WHERE {
            ?person ex:name ?name ; ex:age ?age .
            FILTER (?age >= 18)
        }
    >;
    
    try {
        result: Result = adultsQuery <- store;
        
        result switch {
            TabularResult t => {
                std.print("Found " + t.RowCount.toString() + " adults:");
                foreach (row in t.Rows) {
                    std.print("  " + row["name"].toString());
                }
            },
            _ => std.print("Unexpected result type")
        }
        
        return 0;  // Success
        
    } catch (QueryExecutionException ex) {
        std.print("Query failed: " + ex.Error.Message);
        return 1;  // Error
    }
}
```

## Common Patterns

### Pattern 1: Inline Query Literals

```fifth
// No need to pre-define query variable
result = ?<SELECT ?x WHERE { ?x <rdf:type> <Person> }> <- store;
```

### Pattern 2: Reusable Queries

```fifth
// Define once, apply to multiple stores
personQuery: Query = ?<SELECT ?name WHERE { ?person <name> ?name }>;

result1 = personQuery <- store1;
result2 = personQuery <- store2;
result3 = personQuery <- store3;
```

### Pattern 3: Chaining Queries

```fifth
// First query constructs intermediate graph
intermediateGraph: Result = ?<CONSTRUCT { ?s ?p ?o } WHERE { ... }> <- store;

// Second query applies to constructed graph
intermediateGraph switch {
    GraphResult g => {
        finalResult = ?<SELECT ?x WHERE { ... }> <- g.GraphStore;
    },
    _ => {}
}
```

### Pattern 4: Exhaustive Pattern Matching

```fifth
// Compiler enforces handling all cases
processResult(result: Result): string {
    result switch {
        TabularResult t => formatTable(t),
        GraphResult g => g.ToTriG(),
        BooleanResult b => b.Value.toString()
    }
    // Missing case = compile error
}
```

## Performance Tips

1. **Use LIMIT for large result sets**:
   ```fifth
   query = ?<SELECT ?x WHERE { ... } LIMIT 1000> <- store;
   ```

2. **Stream results lazily**:
   ```fifth
   result switch {
       TabularResult t => {
           // Iterate without materializing full result
           foreach (row in t.Rows) {
               processRow(row);  // Process one at a time
           }
       },
       _ => {}
   }
   ```

3. **Avoid RowCount if not needed** (may force materialization):
   ```fifth
   // BAD: Forces full result materialization
   if (t.RowCount > 100) { ... }
   
   // GOOD: Iterate lazily
   foreach (row in t.Rows) { ... }
   ```

4. **Use ASK instead of SELECT COUNT**:
   ```fifth
   // Efficient existence check
   exists = ?<ASK WHERE { ?s <rdf:type> <Person> }> <- store;
   ```

## Security Best Practices

1. **Avoid raw string concatenation**:
   ```fifth
   // BAD: Injection risk
   userInput: string = getUserInput();
   query = ?<SELECT ?x WHERE { ?x <name> "${userInput}" }> <- store;
   
   // BETTER: Use bind() API (future feature)
   query = ?<SELECT ?x WHERE { ?x <name> ?userName }>.bind("userName", userInput) <- store;
   ```

2. **Catch SecurityWarning errors**:
   ```fifth
   catch (QueryExecutionException ex) {
       ex.Error.Kind switch {
           ErrorKind.SecurityWarning => {
               std.logWarning("Potential injection detected");
               // Reject or sanitize query
           },
           _ => {}
       }
   }
   ```

## Next Steps

- Read the [Result API Contract](./contracts/Result.api.md) for detailed API documentation
- Read the [QueryError API Contract](./contracts/QueryError.api.md) for error handling details
- Explore the [Query Application Operator Contract](./contracts/QueryApplicationOp.api.md) for advanced usage
- Review [test/runtime-integration-tests/](../../test/runtime-integration-tests/) for more examples

## Troubleshooting

### "Type error: LHS not assignable to Query"
- Ensure left operand is created via `?<...>` SPARQL literal syntax
- Check that variable is explicitly typed: `query: Query = ?<...>;`

### "Query execution timeout"
- Add `LIMIT` clause to constrain results
- Simplify graph patterns (reduce joins, use specific predicates)
- Increase timeout (future feature)

### "Memory limit exceeded"
- Use streaming iteration (avoid `RowCount`)
- Add `LIMIT` clause
- Process results incrementally (don't accumulate in memory)

### "SecurityWarning: Unbalanced braces"
- Review query text for missing `{` or `}`
- Check for injection in dynamically constructed queries
- Consider using structured binding (future `bind()` API)
