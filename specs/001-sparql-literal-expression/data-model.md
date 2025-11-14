# Data Model: Embedded SPARQL Queries

**Feature**: 001-sparql-literal-expression  
**Date**: 2025-11-13  
**Phase**: Phase 1 - Design & Contracts

## Overview

This document defines the data structures and relationships for SPARQL literal expressions in Fifth. The model spans three layers: AST representation, system type contract, and runtime representation.

## AST Layer: SparqlLiteralExpression

### Node Definition

```csharp
// Added to src/ast-model/AstMetamodel.cs

/// <summary>
/// Represents a SPARQL query literal expression: ?&lt; ... &gt;
/// </summary>
public partial record SparqlLiteralExpression : Expression
{
    /// <summary>
    /// Raw SPARQL text content (SELECT/CONSTRUCT/ASK/DESCRIBE/UPDATE)
    /// </summary>
    public required string SparqlText { get; init; }
    
    /// <summary>
    /// Variable bindings discovered during resolution:
    /// identifiers in SPARQL body that match in-scope Fifth variables
    /// </summary>
    public List<VariableBinding> Bindings { get; init; } = new();
    
    /// <summary>
    /// Interpolation sites ({{expr}}) for computed value injection
    /// </summary>
    public List<Interpolation> Interpolations { get; init; } = new();
    
    /// <summary>
    /// Source location for diagnostics
    /// </summary>
    public required TextSpan SourceSpan { get; init; }
}

/// <summary>
/// Represents a Fifth variable reference within SPARQL literal
/// </summary>
public partial record VariableBinding
{
    /// <summary>
    /// Variable name as it appears in SPARQL text
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Resolved Fifth variable reference (set by SparqlVariableBindingVisitor)
    /// </summary>
    public Expression? ResolvedExpression { get; set; }
    
    /// <summary>
    /// Inferred Fifth type (set by TypeAnnotationVisitor)
    /// </summary>
    public FifthType? Type { get; set; }
    
    /// <summary>
    /// Location within SPARQL text for diagnostics
    /// </summary>
    public required TextSpan SpanInLiteral { get; init; }
}

/// <summary>
/// Represents an interpolation placeholder {{expr}} within SPARQL literal
/// </summary>
public partial record Interpolation
{
    /// <summary>
    /// Character position in SparqlText where interpolation starts
    /// </summary>
    public required int Position { get; init; }
    
    /// <summary>
    /// Length of interpolation region (including {{ }})
    /// </summary>
    public required int Length { get; init; }
    
    /// <summary>
    /// Fifth expression to evaluate and inject
    /// </summary>
    public required Expression Expression { get; init; }
    
    /// <summary>
    /// Result type after evaluation (IRI, string, numeric, boolean)
    /// </summary>
    public FifthType? ResultType { get; set; }
}
```

### Relationships

- **SparqlLiteralExpression** ⊆ **Expression** (is-a relationship)
- **SparqlLiteralExpression** contains 0..N **VariableBinding** (composition)
- **SparqlLiteralExpression** contains 0..N **Interpolation** (composition)
- **VariableBinding** references **Expression** (resolved variable)
- **Interpolation** contains **Expression** (embedded computation)

### Lifecycle States

1. **Parsed**: Created by `AstBuilderVisitor`, has `SparqlText` and source span; bindings/interpolations empty
2. **Bound**: After `SparqlVariableBindingVisitor`, bindings populated with resolved expressions
3. **Typed**: After `TypeAnnotationVisitor`, all binding/interpolation types inferred
4. **Lowered**: After `SparqlLoweringRewriter`, transformed to query construction code

### Validation Rules

| Rule | Phase | Enforced By |
|------|-------|-------------|
| SPARQL syntax is valid per SPARQL 1.1 spec | Lowering | dotNetRDF parser |
| All referenced variables exist in scope | Binding | SparqlVariableBindingVisitor |
| Variable types are compatible with SPARQL literals | Type checking | TypeAnnotationVisitor |
| Interpolation expressions are constant or variable refs | Type checking | TypeAnnotationVisitor |
| No interpolation nesting ({{...{{...}}...}}) | Parsing | FifthParser |
| Literal size ≤ 1MB | Parsing | AstBuilderVisitor |

## System Type Layer: Fifth.System.Query

### Type Contract

```csharp
// New file: src/fifthlang.system/Query.cs

namespace Fifth.System;

/// <summary>
/// Represents a compiled SPARQL query or update operation.
/// Surface syntax: q: Query = ?&lt;SELECT * WHERE { ?s ?p ?o }>;
/// </summary>
public sealed class Query
{
    /// <summary>
    /// Underlying dotNetRDF query representation
    /// </summary>
    internal VDS.RDF.Query.SparqlQuery UnderlyingQuery { get; }
    
    /// <summary>
    /// Query type: SELECT, CONSTRUCT, ASK, DESCRIBE, or UPDATE
    /// </summary>
    public QueryType Type => MapQueryType(UnderlyingQuery.QueryType);
    
    /// <summary>
    /// Bound parameter names and types
    /// </summary>
    public IReadOnlyDictionary<string, ParameterInfo> Parameters { get; }
    
    /// <summary>
    /// Original SPARQL text (for debugging/logging)
    /// </summary>
    public string SourceText { get; }
    
    /// <summary>
    /// Internal constructor (only called by compiler-generated code)
    /// </summary>
    internal Query(
        VDS.RDF.Query.SparqlQuery underlyingQuery,
        Dictionary<string, ParameterInfo> parameters,
        string sourceText)
    {
        UnderlyingQuery = underlyingQuery ?? throw new ArgumentNullException(nameof(underlyingQuery));
        Parameters = parameters;
        SourceText = sourceText;
    }
    
    /// <summary>
    /// Returns SPARQL text representation
    /// </summary>
    public override string ToString() => UnderlyingQuery.ToString();
}

/// <summary>
/// Query type classification
/// </summary>
public enum QueryType
{
    Select,
    Construct,
    Ask,
    Describe,
    Update
}

/// <summary>
/// Parameter binding metadata
/// </summary>
public sealed record ParameterInfo
{
    public required string Name { get; init; }
    public required Type FifthType { get; init; }
    public required NodeType RdfNodeType { get; init; } // IRI, Literal, Variable
}

/// <summary>
/// RDF node type classification
/// </summary>
public enum NodeType
{
    Iri,
    Literal,
    BlankNode,
    Variable
}
```

### Type System Integration

- **Compile-time type**: `Query` (user-visible surface type)
- **Runtime type**: `Fifth.System.Query` (.NET class)
- **Type inference**: `SparqlLiteralExpression` always infers to `Query`
- **Assignability**: `Query` assignable to `object`, not assignable to string/other types
- **Operators**: Future work (query composition, execution)

## Runtime Layer: Query Construction

### Lowering Pattern

The `SparqlLoweringRewriter` transforms `SparqlLiteralExpression` into equivalent Fifth code that constructs a `Query` instance:

**Source Fifth code**:
```fifth
age: int = 42;
name: string = "Alice";
q: Query = ?<
    SELECT ?person WHERE {
        ?person foaf:name name;
                foaf:age age.
    }
>;
```

**Lowered to (conceptual IL/Roslyn)**:
```csharp
int age = 42;
string name = "Alice";

// Query construction
var paramStr = new VDS.RDF.Query.SparqlParameterizedString();
paramStr.CommandText = "SELECT ?person WHERE { ?person foaf:name @name; foaf:age @age. }";
paramStr.SetLiteral("name", new VDS.RDF.LiteralNode(name));
paramStr.SetLiteral("age", new VDS.RDF.LiteralNode(age));

var parser = new VDS.RDF.Query.SparqlQueryParser();
var underlyingQuery = parser.ParseFromString(paramStr);

var parameters = new Dictionary<string, ParameterInfo>
{
    ["name"] = new ParameterInfo { Name = "name", FifthType = typeof(string), RdfNodeType = NodeType.Literal },
    ["age"] = new ParameterInfo { Name = "age", FifthType = typeof(int), RdfNodeType = NodeType.Literal }
};

Query q = new Query(underlyingQuery, parameters, paramStr.CommandText);
```

### Parameter Binding Rules

| Fifth Type | RDF Node Type | Serialization Method |
|------------|---------------|---------------------|
| `string` | Literal | `SetLiteral("name", new LiteralNode(value))` |
| `int`, `long`, `float`, `double`, `decimal` | Literal | `SetLiteral("name", new LiteralNode(value.ToString(), datatype))` |
| `bool` | Literal | `SetLiteral("name", new LiteralNode(value.ToString(), XSD.boolean))` |
| `Iri` (system type) | IRI | `SetUri("name", new UriNode(value.Uri))` |
| Custom class | Literal (JSON) | `SetLiteral("name", new LiteralNode(JsonSerialize(value)))` |
| `Graph`, `Triple` | Error | Diagnostic: "Cannot bind graph/triple as SPARQL parameter" |

### Interpolation Handling (P2)

For interpolation expressions `{{expr}}`, the lowering strategy differs:

**Source**:
```fifth
prefix: string = "http://example.org/";
q: Query = ?<SELECT * WHERE { <{{prefix}}resource1> ?p ?o }>;
```

**Lowered to**:
```csharp
string prefix = "http://example.org/";
string interpolated_0 = prefix;
string finalSparql = $"SELECT * WHERE {{ <{interpolated_0}resource1> ?p ?o }}";

var paramStr = new SparqlParameterizedString();
paramStr.CommandText = finalSparql;
// No parameters for interpolation; already injected

var parser = new SparqlQueryParser();
var underlyingQuery = parser.ParseFromString(paramStr);
Query q = new Query(underlyingQuery, new Dictionary<string, ParameterInfo>(), finalSparql);
```

**Security Note**: Interpolation must validate that injected values are safe (e.g., IRI syntax validation, no control characters).

## Error States & Diagnostics

### Compile-Time Errors

| Error Code | Condition | Example |
|------------|-----------|---------|
| `FTH-SPARQL-001` | Malformed SPARQL syntax | `?<SELCT * WHERE>` (typo in SELECT) |
| `FTH-SPARQL-002` | Unknown variable reference | `?<SELECT * WHERE { ?s ex:prop unknownVar }>` |
| `FTH-SPARQL-003` | Type mismatch in binding | Binding `Graph` variable as literal parameter |
| `FTH-SPARQL-004` | Literal size exceeds limit | SPARQL text > 1MB |
| `FTH-SPARQL-005` | Invalid interpolation expression | `{{myFunction()}}` when function not constant |
| `FTH-SPARQL-006` | Nested interpolation | `{{outer{{inner}}}}` |

### Runtime Errors

| Error | Cause | Mitigation |
|-------|-------|-----------|
| `InvalidOperationException` | Query execution on disposed store | User error; not compiler concern |
| `SparqlException` | Runtime query failure | User error; not compiler concern |

**Note**: The compiler validates syntax and bindings; runtime execution is outside compiler scope.

## Testing Data Model

### Test Entities

**Valid Samples** (`test/syntax-parser-tests/ValidSparqlLiterals.cs`):
- Empty literal: `?<>`
- Simple SELECT: `?<SELECT * WHERE { ?s ?p ?o }>`
- With variable binding: `age: int = 42; q: Query = ?<SELECT * WHERE { ?s ex:age age }>`
- With interpolation: `prefix: string = "http://ex.org/"; q: Query = ?<SELECT * WHERE { <{{prefix}}res> ?p ?o }>`
- Multi-line literal with whitespace preservation

**Invalid Samples** (`test/syntax-parser-tests/InvalidSparqlLiterals.cs`):
- Malformed SPARQL: `?<SELECT FROM WHERE>`
- Unknown variable: `?<SELECT * WHERE { ?s ex:prop unknownVar }>`
- Type mismatch: `g: Graph = ...; q: Query = ?<SELECT * WHERE { ?s ex:g g }>;` (Graph not bindable)
- Oversized literal: SPARQL text > 1MB
- Nested interpolation: `?<SELECT {{a{{b}}c}}>`

### Integration Test Scenarios

1. **End-to-end compilation**: `.5th` → AST → IL/Roslyn → runtime `Query` object
2. **Parameter binding correctness**: Verify bound values match Fifth variable values
3. **Injection prevention**: Attempt SQL-injection-style attacks in bindings/interpolation; all should fail
4. **Error reporting**: Verify diagnostic locations point to correct line/column in SPARQL literal

## Data Flow Summary

```
┌─────────────────────────────────────────────────────────────────┐
│ Source Code (.5th file)                                         │
│  age: int = 42;                                                 │
│  q: Query = ?<SELECT * WHERE { ?s ex:age age }>;                │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ Lexer/Parser (FifthLexer.g4 + FifthParser.g4)                  │
│  Tokens: SPARQL_START, ID("SELECT"), ..., SPARQL_CLOSE_ANGLE   │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ AST (AstBuilderVisitor → SparqlLiteralExpression)              │
│  SparqlText: "SELECT * WHERE { ?s ex:age age }"                │
│  Bindings: [empty at this stage]                               │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ Variable Binding Pass (SparqlVariableBindingVisitor)           │
│  Bindings: [{ Name: "age", ResolvedExpression: VarRefExp(...) }]│
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ Type Checking (TypeAnnotationVisitor)                          │
│  Expression type: Query                                        │
│  Binding types: [{ Name: "age", Type: int }]                  │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ Lowering (SparqlLoweringRewriter)                              │
│  Emit: SparqlParameterizedString construction                  │
│  Emit: SetLiteral("age", ...) calls                            │
│  Emit: Query(...) constructor                                   │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ IL/Roslyn Backend (LoweredAstToRoslynTranslator)               │
│  Generate C# syntax tree for query construction                │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│ Runtime (.dll assembly)                                         │
│  Query instance with bound parameters                           │
└─────────────────────────────────────────────────────────────────┘
```

## Assumptions & Constraints

### Assumptions
- dotNetRDF 3.x API remains stable (breaking changes require adapter layer)
- SPARQL 1.1 is sufficient (no immediate need for 1.2 features)
- Fifth variables are always in scope when referenced in literal (enforced by compiler)
- Query execution is out of scope (future operators: `query.execute(store)`)

### Constraints
- Literal size: 1MB max (diagnostic suggests external file)
- Performance: <50ms per literal for parsing/validation (typical case <5KB)
- No nested literals: `?< ... ?< ... > ... >` is invalid
- No runtime query modification: `Query` instances are immutable after construction

## Future Extensions

### Query Execution (Future)
```fifth
store: Store = sparql_store(<http://endpoint>);
results: ResultSet = q.execute(store);  // Future operator
```

### Query Composition (Future)
```fifth
q1: Query = ?<SELECT ?s WHERE { ?s rdf:type ex:Person }>;
q2: Query = ?<SELECT ?o WHERE { ?s ex:name ?o }>;
combined: Query = q1.join(q2);  // Future method
```

### IDE Support (Future)
- Syntax highlighting within `?< ... >`
- Autocomplete for SPARQL keywords
- Inline diagnostics (red squiggles for syntax errors)
- Go-to-definition for bound variables

## References

- **AST Design**: Constitution Section VIII (AST Design & Transformation Strategy)
- **Type System**: `src/ast-model/TypeSystem/`
- **TriG Literal Precedent**: `specs/completed-009-trig-literal-expression/data-model.md`
- **dotNetRDF API**: https://github.com/dotnetrdf/dotnetrdf/wiki/UserGuide-Querying-With-SPARQL
