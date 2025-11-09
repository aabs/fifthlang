using System;
using VDS.RDF;
using VDS.RDF.Storage;

namespace Fifth.System;

/// <summary>
/// Thin wrapper over dotNetRDF storage abstractions providing Fifth language semantics.
/// Supports both in-memory and SPARQL endpoint stores.
/// </summary>
public sealed class Store
{
    private readonly IStorageProvider _inner;

    private Store(IStorageProvider storage)
    {
        _inner = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <summary>
    /// Creates a new in-memory store.
    /// </summary>
    public static Store CreateInMemory()
    {
        return new Store(new InMemoryManager());
    }

    /// <summary>
    /// Creates a store connected to a SPARQL endpoint.
    /// </summary>
    public static Store CreateSparqlStore(string endpointUri)
    {
        if (string.IsNullOrWhiteSpace(endpointUri))
            throw new ArgumentException("Endpoint URI cannot be null or empty", nameof(endpointUri));
        
        return new Store(new SparqlHttpProtocolConnector(new Uri(endpointUri)));
    }

    /// <summary>
    /// Creates a store connected to a SPARQL endpoint (URI version).
    /// </summary>
    public static Store CreateSparqlStore(Uri endpointUri)
    {
        if (endpointUri == null)
            throw new ArgumentNullException(nameof(endpointUri));
        
        return new Store(new SparqlHttpProtocolConnector(endpointUri));
    }

    /// <summary>
    /// Creates a new empty graph in the store.
    /// </summary>
    public Graph CreateGraph()
    {
        var graph = new VDS.RDF.Graph();
        return Graph.FromVds(graph);
    }

    /// <summary>
    /// Creates a new empty graph with a specific URI in the store.
    /// </summary>
    public Graph CreateGraph(Uri graphUri)
    {
        var graph = new VDS.RDF.Graph(graphUri);
        return Graph.FromVds(graph);
    }

    /// <summary>
    /// Saves a graph to the store.
    /// </summary>
    public void SaveGraph(Graph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        _inner.SaveGraph(graph.ToVds());
    }

    /// <summary>
    /// Loads a graph from the store by its URI.
    /// </summary>
    public Graph LoadGraph(Uri graphUri)
    {
        if (graphUri == null) throw new ArgumentNullException(nameof(graphUri));
        
        var graph = new VDS.RDF.Graph();
        _inner.LoadGraph(graph, graphUri);
        return Graph.FromVds(graph);
    }

    /// <summary>
    /// Deletes a graph from the store by its URI.
    /// </summary>
    public void DeleteGraph(Uri graphUri)
    {
        if (graphUri == null) throw new ArgumentNullException(nameof(graphUri));
        _inner.DeleteGraph(graphUri);
    }

    /// <summary>
    /// Executes a SPARQL query against the store.
    /// </summary>
    public object ExecuteQuery(string sparql)
    {
        if (string.IsNullOrWhiteSpace(sparql))
            throw new ArgumentException("SPARQL query cannot be null or empty", nameof(sparql));
        
        // This is a simplified version. A full implementation would need to handle
        // query results properly, but for now we return the raw result.
        // The actual implementation would depend on how the Fifth compiler handles query results.
        throw new NotImplementedException("SPARQL query execution needs integration with Fifth type system");
    }

    /// <summary>
    /// Gets the underlying storage provider for interop.
    /// </summary>
    public IStorageProvider ToVds() => _inner;

    /// <summary>
    /// Creates a wrapper from a dotNetRDF storage provider for interop.
    /// </summary>
    public static Store FromVds(IStorageProvider storage) => new(storage);

    // ============================================================================
    // Mutating compound assignment operators (modify store, return store)
    // ============================================================================

    /// <summary>
    /// Mutating addition: saves graph to store and returns the store.
    /// Store is modified in place.
    /// </summary>
    public Store AddGraphInPlace(Graph graph)
    {
        SaveGraph(graph);
        return this;
    }

    /// <summary>
    /// Mutating subtraction: removes graph from store and returns the store.
    /// Store is modified in place.
    /// </summary>
    public Store RemoveGraphInPlace(Graph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        
        var uri = graph.BaseUri;
        if (uri != null)
        {
            DeleteGraph(uri);
        }
        else
        {
            throw new InvalidOperationException("Cannot remove a graph without a base URI from the store");
        }
        return this;
    }
}
