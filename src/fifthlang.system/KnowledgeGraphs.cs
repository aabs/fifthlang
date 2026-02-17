using System;
using VDS.RDF;
using VDS.RDF.Storage;

namespace Fifth.System;

public static class KG
{
    /// <summary>
    /// Creates and returns a new instance of <see cref="IUpdateableStorage"/> using an in-memory storage manager.
    /// </summary>
    /// <returns>
    /// An <see cref="IUpdateableStorage"/> implementation backed by an in-memory manager.
    /// </returns>
    [BuiltinFunction]
    public static IUpdateableStorage CreateStore()
    {
        InMemoryManager mem = new InMemoryManager();
        return mem;
    }

    [BuiltinFunction]
    public static IStorageProvider ConnectToRemoteStore(string endpointUri)
    {
        SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(endpointUri));
        return sparql;
    }

    /// <summary>
    /// Alias for connecting to a remote SPARQL store; mirrors the Fifth keyword usage.
    /// Returns a Store wrapper around the SPARQL endpoint.
    /// </summary>
    /// <param name="endpointUri">the SPARQL endpoint URI.</param>
    /// <returns>a Store wrapper connected to the given endpoint.</returns>
    [BuiltinFunction]
    public static Store sparql_store(string endpointUri)
    {
        return Store.CreateSparqlStore(endpointUri);
    }

    /// <summary>
    /// Legacy version returning IStorageProvider for backward compatibility.
    /// Prefer using the Store wrapper version above.
    /// </summary>
    /// <param name="endpointUri">the SPARQL endpoint URI.</param>
    /// <returns>an updateable storage provider connected to the given endpoint.</returns>
    [BuiltinFunction]
    [Obsolete("Use Store sparql_store(string) instead")]
    public static IStorageProvider sparql_store_legacy(string endpointUri)
    {
        return ConnectToRemoteStore(endpointUri);
    }

    /// <summary>
    /// Creates and returns a new, empty RDF graph.
    /// </summary>
    /// <returns>a new instance of <see cref="IGraph"/>.</returns>
    [BuiltinFunction]
    public static IGraph CreateGraph()
    {
        return new VDS.RDF.Graph();
    }

    /// <summary>
    /// Creates a URI node for the given absolute URI string within the provided graph.
    /// </summary>
    /// <param name="g">the graph that will own the created node.</param>
    /// <param name="uri">an absolute URI string.</param>
    /// <returns>a URI node.</returns>
    [BuiltinFunction]
    public static IUriNode CreateUri(this IGraph g, string uri)
    {
        return g.CreateUriNode(new Uri(uri));
    }

    /// <summary>
    /// Creates a URI node for the given absolute URI within the provided graph.
    /// </summary>
    /// <param name="g">the graph that will own the created node.</param>
    /// <param name="uri">an absolute URI.</param>
    /// <returns>a URI node.</returns>
    [BuiltinFunction]
    public static IUriNode CreateUri(this IGraph g, Uri uri)
    {
        return g.CreateUriNode(uri);
    }

    /// <summary>
    /// Creates a URI node for the type of the given assertable instance.
    /// </summary>
    /// <param name="g">the graph to which the URI node belongs.</param>
    /// <param name="assertable">an assertable instance.</param>
    /// <returns>a URI node for the type of the given assertable instance.</returns>
    [BuiltinFunction]
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
    [BuiltinFunction]
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
    [BuiltinFunction]
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
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral<T>(this IGraph g, T value, Uri typeUri)
    {
        return g.CreateLiteralNode(value.ToSafeString(), typeUri);
    }

    /// <summary>
    /// Creates a typed literal node for a 32-bit integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the integer value.</param>
    /// <returns>a typed literal node with xsd:int datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, int value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#int"));
    }

    /// <summary>
    /// Creates a typed literal node for a 64-bit integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the long value.</param>
    /// <returns>a typed literal node with xsd:long datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, long value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#long"));
    }

    /// <summary>
    /// Creates a typed literal node for a 64-bit floating point value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the double value.</param>
    /// <returns>a typed literal node with xsd:double datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, double value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#double"));
    }

    /// <summary>
    /// Creates a typed literal node for a 32-bit floating point value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the float value.</param>
    /// <returns>a typed literal node with xsd:float datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, float value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#float"));
    }

    /// <summary>
    /// Creates a typed literal node for a 16-bit integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the short value.</param>
    /// <returns>a typed literal node with xsd:short datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, short value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#short"));
    }

    /// <summary>
    /// Creates a typed literal node for an 8-bit signed integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the sbyte value.</param>
    /// <returns>a typed literal node with xsd:byte datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, sbyte value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#byte"));
    }

    /// <summary>
    /// Creates a typed literal node for an 8-bit unsigned integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the byte value.</param>
    /// <returns>a typed literal node with xsd:unsignedByte datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, byte value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedByte"));
    }

    /// <summary>
    /// Creates a typed literal node for a 16-bit unsigned integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the ushort value.</param>
    /// <returns>a typed literal node with xsd:unsignedShort datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, ushort value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedShort"));
    }

    /// <summary>
    /// Creates a typed literal node for a 32-bit unsigned integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the uint value.</param>
    /// <returns>a typed literal node with xsd:unsignedInt datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, uint value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedInt"));
    }

    /// <summary>
    /// Creates a typed literal node for a 64-bit unsigned integer value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the ulong value.</param>
    /// <returns>a typed literal node with xsd:unsignedLong datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, ulong value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedLong"));
    }

    /// <summary>
    /// Creates a typed literal node for a decimal value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the decimal value.</param>
    /// <returns>a typed literal node with xsd:decimal datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, decimal value)
    {
        return g.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#decimal"));
    }

    /// <summary>
    /// Creates a literal node for a char value (as a string literal).
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the char value.</param>
    /// <returns>a literal node with the specified char value.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, char value)
    {
        return g.CreateLiteralNode(value.ToString());
    }

    /// <summary>
    /// Creates a typed literal node for a boolean value.
    /// </summary>
    /// <param name="g">the graph to which the literal node belongs.</param>
    /// <param name="value">the boolean value.</param>
    /// <returns>a typed literal node with xsd:boolean datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteral(this IGraph g, bool value)
    {
        return g.CreateLiteralNode(value ? "true" : "false", UriFactory.Create("http://www.w3.org/2001/XMLSchema#boolean"));
    }

    // Graph-independent node creation methods for triple literals
    // These methods don't require a graph context and are used for creating standalone triples

    /// <summary>
    /// Creates a URI node without requiring a graph context.
    /// Useful for creating standalone triples.
    /// </summary>
    /// <param name="uri">an absolute URI string.</param>
    /// <returns>a URI node.</returns>
    [BuiltinFunction]
    public static IUriNode CreateUriNode(string uri)
    {
        var factory = new NodeFactory();
        return factory.CreateUriNode(new Uri(uri));
    }

    /// <summary>
    /// Creates a URI node without requiring a graph context.
    /// </summary>
    /// <param name="uri">an absolute URI.</param>
    /// <returns>a URI node.</returns>
    [BuiltinFunction]
    public static IUriNode CreateUriNode(Uri uri)
    {
        var factory = new NodeFactory();
        return factory.CreateUriNode(uri);
    }

    /// <summary>
    /// Creates a literal node for an integer value.
    /// </summary>
    /// <param name="value">the integer value.</param>
    /// <returns>a typed literal node with xsd:int datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(int value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#int"));
    }

    /// <summary>
    /// Creates a literal node for a long value.
    /// </summary>
    /// <param name="value">the long value.</param>
    /// <returns>a typed literal node with xsd:long datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(long value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#long"));
    }

    /// <summary>
    /// Creates a literal node for a string value.
    /// </summary>
    /// <param name="value">the string value.</param>
    /// <returns>a literal node.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(string value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value);
    }

    /// <summary>
    /// Creates a literal node for a double value.
    /// </summary>
    /// <param name="value">the double value.</param>
    /// <returns>a typed literal node with xsd:double datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(double value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#double"));
    }

    /// <summary>
    /// Creates a literal node for a float value.
    /// </summary>
    /// <param name="value">the float value.</param>
    /// <returns>a typed literal node with xsd:float datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(float value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#float"));
    }

    /// <summary>
    /// Creates a literal node for a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>A typed literal node with xsd:boolean datatype.</returns>
    [BuiltinFunction]
    public static ILiteralNode CreateLiteralNode(bool value)
    {
        var factory = new NodeFactory();
        return factory.CreateLiteralNode(value ? "true" : "false", UriFactory.Create("http://www.w3.org/2001/XMLSchema#boolean"));
    }

    /// <summary>
    /// Creates a triple with the given subject, predicate, and object nodes.
    /// </summary>
    /// <param name="subj">The subject node of the triple.</param>
    /// <param name="pred">The predicate node of the triple.</param>
    /// <param name="obj">The object node of the triple.</param>
    /// <returns>A triple representing the relationship between the subject, predicate, and object nodes.</returns>
    [BuiltinFunction]
    public static Triple CreateTriple(INode subj, INode pred, INode obj)
    {
        return Triple.Create(subj, pred, obj);
    }

    /// <summary>
    /// Asserts the given triple into the graph and returns the graph for chaining.
    /// </summary>
    /// <param name="g">The graph to which the triple will be asserted.</param>
    /// <param name="t">The triple to assert.</param>
    /// <returns>The graph after the triple has been asserted.</returns>
    [BuiltinFunction]
    public static IGraph Assert(this IGraph g, Triple t)
    {
        // Convert the Fifth.System.Triple to VDS.RDF.Triple
        var vdsTriple = t.ToVdsTriple();

        // Avoid calling this extension recursively: prefer concrete Graph instance method
        if (g is VDS.RDF.Graph concrete)
        {
            concrete.Assert(vdsTriple);
        }
        else
        {
            // Use IEnumerable/params-based overload on IGraph to avoid extension recursion
            g.Assert(new[] { vdsTriple });
        }
        return g;
    }
    /// <summary>
    /// Retracts the given triple from the graph and returns the graph for chaining.
    /// </summary>
    /// <param name="g">The graph from which the triple will be retracted.</param>
    /// <param name="t">The triple to retract.</param>
    /// <returns>The graph after the triple has been retracted.</returns>
    [BuiltinFunction]
    public static IGraph Retract(this IGraph g, Triple t)
    {
        // Convert the Fifth.System.Triple to VDS.RDF.Triple
        var vdsTriple = t.ToVdsTriple();

        // Avoid calling this extension recursively: prefer concrete Graph instance method
        if (g is VDS.RDF.Graph concrete)
        {
            concrete.Retract(vdsTriple);
        }
        else
        {
            g.Retract(new[] { vdsTriple });
        }
        return g;
    }
    /// <summary>
    /// Merges the source graph into the target graph and returns the target graph for chaining.
    /// </summary>
    /// <param name="target">The target graph to which the source graph will be merged.</param>
    /// <param name="source">The source graph to merge into the target graph.</param>
    /// <returns>The target graph after the merge.</returns>
    [BuiltinFunction]
    public static IGraph Merge(this IGraph target, IGraph source)
    {
        // Avoid calling this extension recursively: explicitly copy triples
        foreach (var t in source.Triples)
        {
            if (target is VDS.RDF.Graph concrete)
            {
                concrete.Assert(t);
            }
            else
            {
                target.Assert(new[] { t });
            }
        }
        return target;
    }

    /// <summary>
    /// Creates a copy of the given graph.
    /// </summary>
    /// <param name="source">The source graph to copy.</param>
    /// <returns>A new graph containing all triples from the source graph.</returns>
    [BuiltinFunction]
    public static IGraph CopyGraph(IGraph source)
    {
        var result = new VDS.RDF.Graph();
        // Copy triples explicitly to avoid relying on extension methods
        foreach (var t in source.Triples)
        {
            result.Assert(t);
        }
        return result;
    }

    /// <summary>
    /// Produce a non-mutating graph representing the difference between a and b (a \ b)
    /// </summary>
    [BuiltinFunction]
    public static IGraph Difference(IGraph a, IGraph b)
    {
        var result = new VDS.RDF.Graph();
        // Start with a copy of 'a'
        foreach (var t in a.Triples)
        {
            result.Assert(t);
        }
        // Remove all triples from 'b'
        foreach (var t in b.Triples)
        {
            result.Retract(t);
        }
        return result;
    }

    /// <summary>
    /// Returns the number of triples in the given graph.
    /// </summary>
    /// <param name="g">the graph to inspect.</param>
    /// <returns>the triple count.</returns>
    [BuiltinFunction]
    public static int CountTriples(this IGraph g)
    {
        return g.Triples.Count;
    }

    /// <summary>
    /// Saves the given graph to the specified store and returns the store for chaining.
    /// </summary>
    /// <param name="store">the store to which the graph will be saved.</param>
    /// <param name="g">the graph to save.</param>
    /// <returns>the updated store.</returns>
    [BuiltinFunction]
    public static IStorageProvider SaveGraph(this IStorageProvider store, IGraph g)
    {
        store.SaveGraph(g);
        return store;
    }

    /// <summary>
    /// Saves the given graph to the specified store, under a specific graph URI, and returns the store for chaining.
    /// </summary>
    /// <param name="store">the store to which the graph will be saved.</param>
    /// <param name="g">the graph to save.</param>
    /// <param name="graphUri">the URI of the graph as a string.</param>
    /// <returns>the updated store.</returns>
    [BuiltinFunction]
    public static IStorageProvider SaveGraph(this IStorageProvider store, IGraph g, string graphUri)
    {
        var uri = new Uri(graphUri);
        var x = new VDS.RDF.Graph(uri, g.Triples);
        store.SaveGraph(x);
        return store;
    }

    // ============================================================================
    // Overloads for Fifth.System.Graph wrapper
    // ============================================================================

    /// <summary>
    /// Merges the source graph into the target graph and returns the target graph for chaining.
    /// </summary>
    [BuiltinFunction]
    public static Graph Merge(this Graph target, Graph source) => target.MergeInPlace(source);

    /// <summary>
    /// Asserts the given triple into the graph and returns the graph for chaining.
    /// </summary>
    [BuiltinFunction]
    public static Graph Assert(this Graph target, Triple t) => target.AddInPlace(t);

    /// <summary>
    /// Retracts the given triple from the graph and returns the graph for chaining.
    /// </summary>
    [BuiltinFunction]
    public static Graph Retract(this Graph target, Triple t) => target.RemoveInPlace(t);

    /// <summary>
    /// Returns the number of triples in the given graph.
    /// </summary>
    [BuiltinFunction]
    public static int CountTriples(this Graph g) => g.Count;

}
