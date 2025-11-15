# AST Node Contract: SparqlLiteralExpression

This file documents the AST node structure for SPARQL literal expressions.

## Node Definition

```csharp
// Location: src/ast-model/AstMetamodel.cs

/// <summary>
/// Represents a SPARQL query literal expression: ?&lt; ... &gt;
/// Example: q: Query = ?&lt;SELECT * WHERE { ?s ?p ?o }>;
/// </summary>
public partial record SparqlLiteralExpression : Expression
{
    /// <summary>
    /// Raw SPARQL text content (SELECT/CONSTRUCT/ASK/DESCRIBE/UPDATE).
    /// Includes variable placeholders and interpolation markers.
    /// </summary>
    public required string SparqlText { get; init; }
    
    /// <summary>
    /// Variable bindings discovered during resolution.
    /// Populated by SparqlVariableBindingVisitor.
    /// </summary>
    public List<VariableBinding> Bindings { get; init; } = new();
    
    /// <summary>
    /// Interpolation sites ({{expr}}) for computed value injection.
    /// Populated during parsing if interpolation syntax is present.
    /// </summary>
    public List<Interpolation> Interpolations { get; init; } = new();
    
    /// <summary>
    /// Source location for diagnostics.
    /// Covers entire ?&lt; ... &gt; literal including delimiters.
    /// </summary>
    public required TextSpan SourceSpan { get; init; }
}
```

## Supporting Types

### VariableBinding

```csharp
/// <summary>
/// Represents a Fifth variable reference within SPARQL literal.
/// Example: 'age' in ?&lt;SELECT * WHERE { ?s ex:age age }>;
/// </summary>
public partial record VariableBinding
{
    /// <summary>
    /// Variable name as it appears in SPARQL text.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Resolved Fifth variable reference.
    /// Set by SparqlVariableBindingVisitor after symbol table lookup.
    /// Null if resolution failed (diagnostic emitted).
    /// </summary>
    public Expression? ResolvedExpression { get; set; }
    
    /// <summary>
    /// Inferred Fifth type.
    /// Set by TypeAnnotationVisitor.
    /// Used to determine RDF node type during lowering.
    /// </summary>
    public FifthType? Type { get; set; }
    
    /// <summary>
    /// Location within SPARQL text for diagnostics.
    /// Relative to start of SparqlText string.
    /// </summary>
    public required TextSpan SpanInLiteral { get; init; }
}
```

### Interpolation

```csharp
/// <summary>
/// Represents an interpolation placeholder {{expr}} within SPARQL literal.
/// Example: {{prefix}} in ?&lt;SELECT * WHERE { &lt;{{prefix}}resource&gt; ?p ?o }>;
/// </summary>
public partial record Interpolation
{
    /// <summary>
    /// Character position in SparqlText where interpolation starts.
    /// Index of the first '{' in '{{'...'}'.
    /// </summary>
    public required int Position { get; init; }
    
    /// <summary>
    /// Length of interpolation region (including {{ }}).
    /// Used to replace placeholder during lowering.
    /// </summary>
    public required int Length { get; init; }
    
    /// <summary>
    /// Fifth expression to evaluate and inject.
    /// Must be constant or simple variable reference (enforced during type checking).
    /// </summary>
    public required Expression Expression { get; init; }
    
    /// <summary>
    /// Result type after evaluation.
    /// Set by TypeAnnotationVisitor.
    /// Determines serialization strategy (IRI syntax vs literal).
    /// </summary>
    public FifthType? ResultType { get; set; }
}
```

## Usage in Compiler Pipeline

### Phase 1: Parsing (AstBuilderVisitor)
```csharp
public override Expression VisitSparqlLiteral(FifthParser.SparqlLiteralContext ctx)
{
    string sparqlText = ExtractSparqlText(ctx);
    TextSpan span = GetTextSpan(ctx);
    
    // Parse interpolation markers if present
    List<Interpolation> interpolations = ParseInterpolations(sparqlText, ctx);
    
    return new SparqlLiteralExpression
    {
        SparqlText = sparqlText,
        Interpolations = interpolations,
        SourceSpan = span
    };
}
```

### Phase 2: Variable Binding (SparqlVariableBindingVisitor)
```csharp
public override void VisitSparqlLiteralExpression(SparqlLiteralExpression node)
{
    // Scan SPARQL text for identifiers
    foreach (var identifier in ExtractIdentifiers(node.SparqlText))
    {
        // Attempt symbol table lookup
        if (_symbolTable.TryResolve(identifier.Name, out var symbol))
        {
            node.Bindings.Add(new VariableBinding
            {
                Name = identifier.Name,
                ResolvedExpression = new VarRefExp { Name = identifier.Name },
                SpanInLiteral = identifier.Span
            });
        }
        else
        {
            // Emit diagnostic: unknown variable
            _diagnostics.Add(new Diagnostic
            {
                Code = "FTH-SPARQL-002",
                Message = $"Unknown variable '{identifier.Name}' in SPARQL literal",
                Location = OffsetSpan(node.SourceSpan, identifier.Span)
            });
        }
    }
    
    base.VisitSparqlLiteralExpression(node);
}
```

### Phase 3: Type Checking (TypeAnnotationVisitor)
```csharp
public override FifthType VisitSparqlLiteralExpression(SparqlLiteralExpression node)
{
    // Infer types for all bindings
    foreach (var binding in node.Bindings)
    {
        if (binding.ResolvedExpression != null)
        {
            binding.Type = Visit(binding.ResolvedExpression);
            
            // Validate type is SPARQL-compatible
            if (!IsSparqlCompatible(binding.Type))
            {
                _diagnostics.Add(new Diagnostic
                {
                    Code = "FTH-SPARQL-003",
                    Message = $"Cannot bind variable '{binding.Name}' of type '{binding.Type}' as SPARQL parameter",
                    Location = binding.SpanInLiteral
                });
            }
        }
    }
    
    // Infer types for interpolations
    foreach (var interpolation in node.Interpolations)
    {
        interpolation.ResultType = Visit(interpolation.Expression);
    }
    
    // SPARQL literals always type to Query
    return FifthType.FromSystemType(typeof(Fifth.System.Query));
}
```

### Phase 4: Lowering (SparqlLoweringRewriter)
```csharp
public override RewriteResult VisitSparqlLiteralExpression(SparqlLiteralExpression node)
{
    var prologue = new List<Statement>();
    
    // Emit query construction code
    // See data-model.md "Lowering Pattern" section for details
    
    // 1. Create SparqlParameterizedString
    // 2. Set CommandText
    // 3. Add SetLiteral calls for each binding
    // 4. Parse via dotNetRDF
    // 5. Construct Query instance
    
    var queryConstructor = BuildQueryConstructorCall(node);
    
    return new RewriteResult(queryConstructor, prologue);
}
```

## Integration Points

### Grammar (FifthParser.g4)
```antlr
literal
    : primitiveLiteral
    | trigLiteral
    | sparqlLiteral  // New alternative
    ;

sparqlLiteral
    : SPARQL_START sparqlLiteralContent* SPARQL_CLOSE_ANGLE
    ;

sparqlLiteralContent
    : ~(SPARQL_CLOSE_ANGLE)  // Any character except '>'
    ;
```

### Grammar (FifthLexer.g4)
```antlr
SPARQL_START: '?<' -> pushMode(SparqlMode);

mode SparqlMode;
SPARQL_CLOSE_ANGLE: '>' -> popMode;
SPARQL_INTERPOLATION_START: '{{';
SPARQL_INTERPOLATION_END: '}}';
SPARQL_CONTENT: ~[>{}]+;  // SPARQL keywords, identifiers, operators
```

## Validation Rules

| Rule | Enforcer | Diagnostic Code |
|------|----------|----------------|
| Valid SPARQL syntax | SparqlLoweringRewriter + dotNetRDF | FTH-SPARQL-001 |
| Known variables | SparqlVariableBindingVisitor | FTH-SPARQL-002 |
| Compatible types | TypeAnnotationVisitor | FTH-SPARQL-003 |
| Size limit (1MB) | AstBuilderVisitor | FTH-SPARQL-004 |
| Valid interpolation | TypeAnnotationVisitor | FTH-SPARQL-005 |
| No nesting | FifthParser | FTH-SPARQL-006 |

## Test Coverage Requirements

- **Parsing**: Valid/invalid syntax samples
- **Variable resolution**: In-scope, out-of-scope, shadowing
- **Type checking**: All supported Fifth types, incompatible types
- **Lowering**: Correct query construction code
- **Integration**: End-to-end compilation to Query instance
- **Security**: Injection attempt scenarios

See `test/syntax-parser-tests/SparqlLiteralTests.cs` and `test/ast-tests/SparqlLiteralAstTests.cs`.
