using System;

namespace Fifth.System;

public static class KG
{
    /// <summary>
    /// Creates and returns a new instance of <see cref="IUpdateableStorage"/> using an in-memory storage manager.
    /// </summary>
    /// <returns>
    /// An <see cref="IUpdateableStorage"/> implementation backed by an in-memory manager.
    /// </returns>
    public static IUpdateableStorage CreateStore()
    {
        InMemoryManager mem = new InMemoryManager();
        return mem;
    }

    public static SparqlHttpProtocolConnector ConnectToRemoteStore(string endpointUri)
    {
        SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(endpointUri));
        return sparql;
    }

    /// <summary>
    /// Creates and returns a new, empty RDF graph.
    /// </summary>
    /// <returns>a new instance of <see cref="IGraph"/>.</returns>
    public static IGraph CreateGraph()
    {
        return new Graph();
    }

    /// <summary>
    /// Creates a URI node for the type of the given assertable instance.
    /// </summary>
    /// <param name="g">the graph to which the URI node belongs.</param>
    /// <param name="assertable">an assertable instance.</param>
    /// <returns>a URI node for the type of the given assertable instance.</returns>
    public static IUriNode CreateUriForType(this IGraph g, IAssertable assertable)
    {
        return g.CreateUriNode(UriFactory.Create(assertable.GetTypeUri().AbsoluteUri));
    }

    /// <summary>
    /// Creates a URI node for the instance of the given assertable instance.
    /// </summary>
    /// <param name="g">the graph to which the URI node belongs.</param>
    /// <param name="assertable">an assertable instance.</param>
    /// <returns>a URI node for the instance of the given assertable instance.</returns>
    public static IUriNode CreateUriForInstance(this IGraph g, IAssertable assertable)
    {
        return g.CreateUriNode(UriFactory.Create(assertable.GetInstanceUri().AbsoluteUri));
    }

    /// <summary>
    /// Creates a literal node with the given string value and optional language tag (defaulting to "en").
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the string value of the literal node.</param>
    /// <param name="language">the language tag for the literal node (default is "en").</param>
    /// <returns>a literal node with the specified value and language tag.</returns>
    public static ILiteralNode CreateLiteral(this IGraph g, string value, string language = "en")
    {
        return g.CreateLiteralNode(value?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Creates a typed literal node for the given value and type URI.
    /// </summary>
    /// <typeparam name="T">the type of the value.</typeparam>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the value of the literal node.</param>
    /// <param name="typeUri">the type URI for the literal node.</param>
    /// <returns>a typed literal node with the specified value and type URI.</returns>
    public static ILiteralNode CreateLiteral<T>(this IGraph g, T value, Uri typeUri)
    {
        return g.CreateLiteralNode(value.ToSafeString(), typeUri);
    }

    /// <summary>
    /// Creates a triple with the given subject, predicate, and object nodes.
    /// </summary>
    /// <param name="subj">The subject node of the triple.</param>
    /// <param name="pred">The predicate node of the triple.</param>
    /// <param name="obj">The object node of the triple.</param>
    /// <returns>A triple representing the relationship between the subject, predicate, and object nodes.</returns>
    public static Triple CreateTriple(INode subj, INode pred, INode obj)
    {
        return new Triple(subj, pred, obj);
    }

    /// <summary>
    /// Asserts the given triple into the graph and returns the graph for chaining.
    /// </summary>
    /// <param name="g">The graph to which the triple will be asserted.</param>
    /// <param name="t">The triple to assert.</param>
    /// <returns>The graph after the triple has been asserted.</returns>
    public static IGraph Assert(this IGraph g, Triple t)
    {
        g.Assert(t);
        return g;
    }
    /// <summary>
    /// Retracts the given triple from the graph and returns the graph for chaining.
    /// </summary>
    /// <param name="g">The graph from which the triple will be retracted.</param>
    /// <param name="t">The triple to retract.</param>
    /// <returns>The graph after the triple has been retracted.</returns>
    public static IGraph Retract(this IGraph g, Triple t)
    {
        g.Retract(t);
        return g;
    }
    /// <summary>
    /// Merges the source graph into the target graph and returns the target graph for chaining.
    /// </summary>
    /// <param name="target">The target graph to which the source graph will be merged.</param>
    /// <param name="source">The source graph to merge into the target graph.</param>
    /// <returns>The target graph after the merge.</returns>
    public static IGraph Merge(this IGraph target, IGraph source)
    {
        target.Merge(source);
        return target;
    }

    /// <summary>
    /// Saves the given graph to the specified store, optionally under a specific graph URI, and returns the store for chaining.
    /// </summary>
    /// <param name="store">the store to which the graph will be saved.</param>
    /// <param name="g">the graph to save.</param>
    /// <param name="graphUri">the URI of the graph (optional).</param>
    /// <returns>the updated store.</returns>
    private static IUpdateableStorage Save(this IUpdateableStorage store, IGraph g, Uri? graphUri = null)
    {
        if (graphUri == null)
        {
            store.SaveGraph(g);
        }
        else
        {
            var x = new Graph(graphUri, g.Triples);
            store.SaveGraph(x);
        }
        return store;
    }
}
