# Quickstart: Embedded SPARQL Queries

**Feature**: 001-sparql-literal-expression  
**Target Audience**: Fifth language developers  
**Prerequisites**: Fifth compiler with AST generator support

## Overview

SPARQL literal expressions allow you to embed SPARQL queries directly in Fifth code using `?< ... >` syntax. Queries compile to the `Query` type with safe variable binding.

## Basic Usage

### Simple Query

```fifth
main(): int {
    // Empty query (valid but not useful)
    emptyQuery: Query = ?<>;
    
    // Basic SELECT query
    selectAll: Query = ?<
        SELECT * WHERE {
            ?subject ?predicate ?object .
        }
    >;
    
    return 0;
}
```

### Query with Variable Binding (P1)

The most powerful feature: reference Fifth variables directly in SPARQL.

```fifth
main(): int {
    // Fifth variables
    age: int = 42;
    name: string = "Alice";
    minSalary: decimal = 50000.0;
    
    // Query with bound variables
    query: Query = ?<
        PREFIX foaf: <http://xmlns.com/foaf/0.1/>
        PREFIX ex: <http://example.org/ns#>
        
        SELECT ?person ?salary WHERE {
            ?person foaf:name name ;
                    foaf:age age ;
                    ex:salary ?salary .
            FILTER (?salary > minSalary)
        }
    >;
    
    // Variables 'name', 'age', 'minSalary' are safely bound as parameters
    // No string concatenation or injection risk
    
    return 0;
}
```

**How it works**:
- The compiler scans the SPARQL text for identifiers
- Matches them against in-scope Fifth variables
- Generates safe parameterized query using dotNetRDF's `SparqlParameterizedString`
- Type conversions handled automatically (int→xsd:integer, string→xsd:string, etc.)

### Supported Types for Binding

| Fifth Type | RDF Representation | Example |
|------------|-------------------|---------|
| `int`, `long` | `xsd:integer` | `42` → `"42"^^xsd:integer` |
| `float`, `double` | `xsd:double` | `3.14` → `"3.14"^^xsd:double` |
| `decimal` | `xsd:decimal` | `99.99` → `"99.99"^^xsd:decimal` |
| `string` | `xsd:string` | `"Alice"` → `"Alice"^^xsd:string` |
| `bool` | `xsd:boolean` | `true` → `"true"^^xsd:boolean` |

### Query Types

SPARQL supports multiple query forms, all accessible via literals:

```fifth
main(): int {
    // SELECT: returns result set
    selectQuery: Query = ?<
        SELECT ?name ?email WHERE {
            ?person foaf:name ?name ;
                    foaf:mbox ?email .
        }
    >;
    
    // CONSTRUCT: builds new graph
    constructQuery: Query = ?<
        PREFIX ex: <http://example.org/>
        CONSTRUCT {
            ?person ex:hasEmail ?email .
        } WHERE {
            ?person foaf:mbox ?email .
        }
    >;
    
    // ASK: returns boolean
    askQuery: Query = ?<
        ASK WHERE {
            ?person foaf:name "Alice" .
        }
    >;
    
    // DESCRIBE: returns RDF description
    describeQuery: Query = ?<
        DESCRIBE <http://example.org/person/alice>
    >;
    
    return 0;
}
```

## Advanced Features

### Multi-line Queries

Whitespace is preserved; format for readability:

```fifth
complexQuery: Query = ?<
    PREFIX foaf: <http://xmlns.com/foaf/0.1/>
    PREFIX dc: <http://purl.org/dc/elements/1.1/>
    PREFIX ex: <http://example.org/ns#>
    
    SELECT DISTINCT ?author ?bookTitle
    WHERE {
        ?book dc:creator ?author ;
              dc:title ?bookTitle ;
              ex:publishedYear ?year .
        
        ?author foaf:name ?authorName .
        
        FILTER (?year > 2000)
        FILTER (LANG(?bookTitle) = "en")
    }
    ORDER BY DESC(?year)
    LIMIT 10
>;
```

### Namespaces and Prefixes

Use standard SPARQL PREFIX declarations:

```fifth
query: Query = ?<
    PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
    PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
    PREFIX owl: <http://www.w3.org/2002/07/owl#>
    PREFIX foaf: <http://xmlns.com/foaf/0.1/>
    PREFIX ex: <http://example.org/ns#>
    
    SELECT ?class WHERE {
        ?class rdf:type owl:Class .
    }
>;
```

### Interpolation (P2 - Optional in MVP)

For computed values, use `{{expr}}` syntax:

```fifth
main(): int {
    baseUri: string = "http://example.org/";
    resourceId: int = 123;
    
    // Interpolation for computed IRI
    query: Query = ?<
        SELECT * WHERE {
            <{{baseUri}}resource/{{resourceId}}> ?p ?o .
        }
    >;
    
    // Results in: <http://example.org/resource/123> ?p ?o
    
    return 0;
}
```

**Warning**: Interpolation bypasses parameter binding. Use only for:
- IRI construction
- Constant values
- Non-injectable expressions

Prefer variable binding (no `{{}}`) when possible for better type safety.

## Error Handling

### Compile-Time Errors

The compiler catches errors early:

```fifth
// Error: Unknown variable
query1: Query = ?<
    SELECT * WHERE {
        ?s ex:prop unknownVar .  // Error: 'unknownVar' not in scope
    }
>;

// Error: Invalid SPARQL syntax
query2: Query = ?<
    SELCT * WHERE { ?s ?p ?o }  // Error: typo in 'SELECT'
>;

// Error: Type mismatch
graph: Graph = ...;
query3: Query = ?<
    SELECT * WHERE {
        ?s ex:hasGraph graph .  // Error: Graph not bindable as SPARQL parameter
    }
>;
```

Expected diagnostics:
```
error FTH-SPARQL-002: Unknown variable 'unknownVar' referenced in SPARQL literal at line 3, column 20
error FTH-SPARQL-001: Invalid SPARQL syntax: Expected 'SELECT' but found 'SELCT' at line 2, column 5
error FTH-SPARQL-003: Cannot bind variable 'graph' of type 'Graph' as SPARQL parameter at line 3, column 24
```

## Best Practices

### 1. Use Variable Binding (Not Interpolation)

✅ **Good** (type-safe, injection-proof):
```fifth
age: int = 42;
query: Query = ?<SELECT * WHERE { ?s ex:age age }>;
```

❌ **Avoid** (bypasses type checking):
```fifth
age: int = 42;
query: Query = ?<SELECT * WHERE { ?s ex:age {{age}} }>;
```

### 2. Define Variables Before Literal

✅ **Good**:
```fifth
name: string = "Alice";
age: int = 30;
query: Query = ?<SELECT * WHERE { ?p foaf:name name; foaf:age age }>;
```

❌ **Bad** (variables undefined):
```fifth
query: Query = ?<SELECT * WHERE { ?p foaf:name name; foaf:age age }>;
name: string = "Alice";  // Too late!
age: int = 30;
```

### 3. Use PREFIX Declarations

✅ **Good**:
```fifth
query: Query = ?<
    PREFIX foaf: <http://xmlns.com/foaf/0.1/>
    SELECT * WHERE { ?p foaf:name ?n }
>;
```

❌ **Avoid** (verbose):
```fifth
query: Query = ?<
    SELECT * WHERE { ?p <http://xmlns.com/foaf/0.1/name> ?n }
>;
```

### 4. Format for Readability

✅ **Good**:
```fifth
query: Query = ?<
    PREFIX ex: <http://example.org/>
    
    SELECT ?person ?email WHERE {
        ?person ex:email ?email ;
                ex:verified true .
    }
    ORDER BY ?email
>;
```

❌ **Avoid** (hard to read):
```fifth
query: Query = ?<PREFIX ex: <http://example.org/> SELECT ?person ?email WHERE { ?person ex:email ?email ; ex:verified true . } ORDER BY ?email>;
```

## Common Patterns

### Pattern 1: Parameterized Filtering

```fifth
filterPeople(minAge: int, country: string): Query {
    return ?<
        PREFIX foaf: <http://xmlns.com/foaf/0.1/>
        PREFIX ex: <http://example.org/ns#>
        
        SELECT ?person ?name WHERE {
            ?person foaf:name ?name ;
                    foaf:age ?age ;
                    ex:country country .
            FILTER (?age >= minAge)
        }
    >;
}

main(): int {
    adults: Query = filterPeople(18, "USA");
    return 0;
}
```

### Pattern 2: Query Builder

```fifth
class QueryBuilder {
    baseQuery: Query;
    
    init(entityType: string) {
        baseQuery = ?<
            SELECT * WHERE {
                ?entity rdf:type entityType .
            }
        >;
    }
}

main(): int {
    personQuery: QueryBuilder = QueryBuilder("foaf:Person");
    return 0;
}
```

### Pattern 3: Dynamic Query Selection

```fifth
getQuery(queryType: string): Query {
    return queryType match {
        "people" => ?<SELECT ?p WHERE { ?p rdf:type foaf:Person }>,
        "orgs" => ?<SELECT ?o WHERE { ?o rdf:type foaf:Organization }>,
        _ => ?<SELECT ?s WHERE { ?s ?p ?o }>
    };
}
```

## Integration with Knowledge Graphs

SPARQL queries work seamlessly with Fifth's knowledge graph features:

```fifth
main(): int {
    // Create store
    store: Store = sparql_store(<http://dbpedia.org/sparql>);
    
    // Define query
    capitalQuery: Query = ?<
        PREFIX dbo: <http://dbpedia.org/ontology/>
        
        SELECT ?capital ?country WHERE {
            ?country dbo:capital ?capital .
        }
        LIMIT 10
    >;
    
    // Execute (future operator)
    // results: ResultSet = capitalQuery.execute(store);
    
    return 0;
}
```

## Testing Your Queries

### Unit Test Example

```fifth
// test/SparqlLiteralTests.5th

testSimpleQuery() {
    query: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
    assert(query.Type == QueryType.Select);
}

testVariableBinding() {
    name: string = "TestUser";
    query: Query = ?<SELECT * WHERE { ?s foaf:name name }>;
    
    // Verify parameter was bound
    params: Dictionary = query.Parameters;
    assert(params.containsKey("name"));
    assert(params["name"].FifthType == typeof(string));
}

testInvalidQuery() {
    // Should fail at compile time
    // query: Query = ?<INVALID SPARQL>;
    // Uncomment to verify error: FTH-SPARQL-001
}
```

## Next Steps

- **Execution**: Future operators for running queries against stores
- **Results**: Working with `ResultSet`, `Graph` return values
- **Optimization**: Query caching, federated queries
- **Debugging**: IDE support for SPARQL syntax highlighting

## See Also

- **Specification**: `specs/001-sparql-literal-expression/spec.md`
- **Data Model**: `specs/001-sparql-literal-expression/data-model.md`
- **Knowledge Graphs Guide**: `docs/knowledge-graphs.md`
- **SPARQL 1.1 Spec**: https://www.w3.org/TR/sparql11-query/
