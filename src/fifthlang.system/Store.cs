using System;
using System.IO;
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
    private readonly TripleStore? _tripleStore; // For in-memory stores, track the TripleStore for querying

    private Store(IStorageProvider storage, TripleStore? tripleStore = null)
    {
        _inner = storage ?? throw new ArgumentNullException(nameof(storage));
        _tripleStore = tripleStore;
    }

    private Store CloneForImmutableOperations()
    {
        if (_tripleStore == null)
            throw new InvalidOperationException("Immutable store operators are only supported for in-memory stores");

        var cloneTripleStore = new VDS.RDF.TripleStore();

        foreach (var existingGraph in _tripleStore.Graphs)
        {
            var graphCopy = CloneGraph(existingGraph);
            cloneTripleStore.Add(graphCopy, true);
        }

        return new Store(new InMemoryManager(cloneTripleStore), cloneTripleStore);
    }

    private static VDS.RDF.Graph CloneGraph(VDS.RDF.IGraph source)
    {
        var copy = new VDS.RDF.Graph
        {
            BaseUri = source.BaseUri
        };

        copy.NamespaceMap.Import(source.NamespaceMap);
        copy.Assert(source.Triples);
        return copy;
    }

    /// <summary>
    /// Creates a new in-memory store.
    /// </summary>
    public static Store CreateInMemory()
    {
        var tripleStore = new TripleStore();
        return new Store(new InMemoryManager(tripleStore), tripleStore);
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
    /// Gets the underlying TripleStore for in-memory querying.
    /// Returns null for non-in-memory stores (e.g., SPARQL endpoints).
    /// </summary>
    public TripleStore? GetTripleStore() => _tripleStore;

    /// <summary>
    /// Creates a wrapper from a dotNetRDF storage provider for interop.
    /// </summary>
    public static Store FromVds(IStorageProvider storage) => new(storage);

    /// <summary>
    /// Loads a Store from TriG format string.
    /// Creates a new in-memory store and parses the TriG content into it.
    /// </summary>
    /// <param name="trigContent">TriG format RDF dataset string</param>
    /// <returns>A new Store containing the parsed dataset</returns>
    public static Store LoadFromTriG(string trigContent)
    {
        if (trigContent == null)
            throw new ArgumentNullException(nameof(trigContent));

        // Create an in-memory triple store (dataset)
        var tripleStore = new VDS.RDF.TripleStore();

        // Parse the TriG content
        var parser = new VDS.RDF.Parsing.TriGParser();
        using (var reader = new StringReader(trigContent))
        {
            parser.Load(tripleStore, reader);
        }

        // Wrap in an in-memory manager that uses this triple store
        var storage = new InMemoryManager(tripleStore);
        return new Store(storage, tripleStore);
    }

    // ============================================================================
    // Mutating compound assignment operators (modify store, return store)
    // ============================================================================

    /// <summary>
    /// Mutating addition: saves graph to store and returns the store.
    /// Store is modified in place.
    /// </summary>
    /// <remarks>
    /// The C# compiler rewrites <c>store += graph</c> into <c>store = store + graph</c>.
    /// Call this method explicitly whenever you need real mutation semantics.
    /// </remarks>
    public Store AddGraphInPlace(Graph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        SaveGraph(graph);
        return this;
    }

    /// <summary>
    /// Mutating subtraction: removes graph from store and returns the store.
    /// Store is modified in place.
    /// </summary>
    /// <remarks>
    /// See <see cref="AddGraphInPlace"/> for the rationale behind explicit mutation helpers.
    /// </remarks>
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

    public static Store operator +(Store store, Graph graph)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (graph == null) throw new ArgumentNullException(nameof(graph));

        var clone = store.CloneForImmutableOperations();
        clone.AddGraphInPlace(graph);
        return clone;
    }

    public static Store operator -(Store store, Graph graph)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (graph == null) throw new ArgumentNullException(nameof(graph));

        var clone = store.CloneForImmutableOperations();
        clone.RemoveGraphInPlace(graph);
        return clone;
    }

}
