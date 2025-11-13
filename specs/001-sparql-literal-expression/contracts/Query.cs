namespace Fifth.System;

/// <summary>
/// Represents a compiled SPARQL query or update operation.
/// Surface syntax: q: Query = ?&lt;SELECT * WHERE { ?s ?p ?o }>;
/// </summary>
/// <remarks>
/// Query instances are immutable after construction and represent validated SPARQL.
/// The underlying dotNetRDF query object is wrapped to provide Fifth-specific semantics.
/// </remarks>
public sealed class Query
{
    /// <summary>
    /// Underlying dotNetRDF query representation.
    /// Internal to prevent direct manipulation; Fifth code interacts via Query methods.
    /// </summary>
    internal VDS.RDF.Query.SparqlQuery UnderlyingQuery { get; }

    /// <summary>
    /// Query type: SELECT, CONSTRUCT, ASK, DESCRIBE, or UPDATE.
    /// </summary>
    public QueryType Type { get; }

    /// <summary>
    /// Bound parameter names and types.
    /// Read-only view of parameters set during compilation.
    /// </summary>
    public IReadOnlyDictionary<string, ParameterInfo> Parameters { get; }

    /// <summary>
    /// Original SPARQL text (for debugging/logging).
    /// Includes parameterization syntax (@varname).
    /// </summary>
    public string SourceText { get; }

    /// <summary>
    /// Internal constructor (only called by compiler-generated code).
    /// User code cannot construct Query instances directly; must use literals.
    /// </summary>
    /// <param name="underlyingQuery">Validated SPARQL query from dotNetRDF parser</param>
    /// <param name="parameters">Parameter binding metadata</param>
    /// <param name="sourceText">Original SPARQL text with parameters</param>
    /// <exception cref="ArgumentNullException">If underlyingQuery is null</exception>
    internal Query(
        VDS.RDF.Query.SparqlQuery underlyingQuery,
        Dictionary<string, ParameterInfo> parameters,
        string sourceText)
    {
        UnderlyingQuery = underlyingQuery ?? throw new ArgumentNullException(nameof(underlyingQuery));
        Parameters = parameters ?? new Dictionary<string, ParameterInfo>();
        SourceText = sourceText ?? string.Empty;
        Type = MapQueryType(underlyingQuery.QueryType);
    }

    /// <summary>
    /// Returns SPARQL text representation.
    /// </summary>
    public override string ToString() => UnderlyingQuery.ToString();

    /// <summary>
    /// Maps dotNetRDF query type to Fifth QueryType enum.
    /// </summary>
    private static QueryType MapQueryType(VDS.RDF.Query.SparqlQueryType dotNetType)
    {
        return dotNetType switch
        {
            VDS.RDF.Query.SparqlQueryType.Select => QueryType.Select,
            VDS.RDF.Query.SparqlQueryType.Construct => QueryType.Construct,
            VDS.RDF.Query.SparqlQueryType.Ask => QueryType.Ask,
            VDS.RDF.Query.SparqlQueryType.Describe => QueryType.Describe,
            _ => QueryType.Update // Covers Update and unknown types
        };
    }

    /// <summary>
    /// Future: Execute query against a store.
    /// Not implemented in initial release; reserved for query execution operators.
    /// </summary>
    // public ResultSet Execute(Store store) => throw new NotImplementedException();
}

/// <summary>
/// Query type classification.
/// </summary>
public enum QueryType
{
    /// <summary>
    /// SELECT query (returns result set with variable bindings)
    /// </summary>
    Select,

    /// <summary>
    /// CONSTRUCT query (returns RDF graph)
    /// </summary>
    Construct,

    /// <summary>
    /// ASK query (returns boolean)
    /// </summary>
    Ask,

    /// <summary>
    /// DESCRIBE query (returns RDF graph describing resources)
    /// </summary>
    Describe,

    /// <summary>
    /// UPDATE operation (INSERT/DELETE/CLEAR/etc.)
    /// </summary>
    Update
}

/// <summary>
/// Parameter binding metadata.
/// Records how a Fifth variable was bound into the SPARQL query.
/// </summary>
public sealed record ParameterInfo
{
    /// <summary>
    /// Parameter name (matches variable name in Fifth code).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Fifth type of the bound variable (e.g., typeof(int), typeof(string)).
    /// </summary>
    public required Type FifthType { get; init; }

    /// <summary>
    /// RDF node type classification (IRI, Literal, etc.).
    /// Determines how the value was serialized into SPARQL.
    /// </summary>
    public required NodeType RdfNodeType { get; init; }
}

/// <summary>
/// RDF node type classification.
/// Indicates how a Fifth value was represented in the SPARQL query.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// IRI/URI reference (e.g., &lt;http://example.org/resource&gt;)
    /// </summary>
    Iri,

    /// <summary>
    /// Typed or untyped literal (e.g., "text", 42, true)
    /// </summary>
    Literal,

    /// <summary>
    /// Blank node (not typically used for parameter binding)
    /// </summary>
    BlankNode,

    /// <summary>
    /// SPARQL variable (not typically used for parameter binding)
    /// </summary>
    Variable
}
