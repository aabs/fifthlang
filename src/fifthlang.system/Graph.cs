using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Fifth.System;

/// <summary>
/// Thin wrapper over dotNetRDF IGraph providing Fifth language semantics.
/// Supports set semantics (duplicates suppressed) and provides both mutating 
/// and non-mutating operators.
/// </summary>
public sealed class Graph
{
    private readonly IGraph _inner;

    private Graph(IGraph inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    /// <summary>
    /// Creates a new empty graph.
    /// </summary>
    public static Graph Create() => new(new VDS.RDF.Graph());

    /// <summary>
    /// Creates a new empty graph with a base URI.
    /// </summary>
    public static Graph Create(Uri baseUri) => new(new VDS.RDF.Graph(baseUri));

    /// <summary>
    /// Adds a triple to the graph (mutating operation).
    /// Idempotent: adding the same triple multiple times results in single triple.
    /// </summary>
    public void Add(Triple triple)
    {
        if (triple == null) throw new ArgumentNullException(nameof(triple));
        var vdsTriple = triple.ToVdsTriple();
        if (_inner is VDS.RDF.Graph concrete)
        {
            concrete.Assert(vdsTriple);
        }
        else
        {
            _inner.Assert(new[] { vdsTriple });
        }
    }

    /// <summary>
    /// Removes a triple from the graph (mutating operation).
    /// </summary>
    public void Remove(Triple triple)
    {
        if (triple == null) throw new ArgumentNullException(nameof(triple));
        var vdsTriple = triple.ToVdsTriple();
        if (_inner is VDS.RDF.Graph concrete)
        {
            concrete.Retract(vdsTriple);
        }
        else
        {
            _inner.Retract(new[] { vdsTriple });
        }
    }

    /// <summary>
    /// Gets the number of triples in the graph.
    /// </summary>
    public int Count => _inner.Triples.Count;

    /// <summary>
    /// Gets the triples in the graph as an enumerable.
    /// </summary>
    public IEnumerable<Triple> Triples => _inner.Triples.Select(Triple.FromVds);

    /// <summary>
    /// Gets the base URI of the graph.
    /// </summary>
    public Uri? BaseUri => _inner.BaseUri;

    /// <summary>
    /// Converts this wrapper to the underlying dotNetRDF IGraph for interop.
    /// </summary>
    public IGraph ToVds() => _inner;

    /// <summary>
    /// Creates a wrapper from a dotNetRDF IGraph for interop.
    /// </summary>
    public static Graph FromVds(IGraph graph) => new(graph);

    // ============================================================================
    // Non-mutating binary operators (return new Graph, operands unchanged)
    // ============================================================================

    /// <summary>
    /// Non-mutating addition: returns a new graph containing all triples from g plus t.
    /// Original graph g is unchanged.
    /// </summary>
    public static Graph operator +(Graph g, Triple t)
    {
        if (g == null) throw new ArgumentNullException(nameof(g));
        if (t == null) throw new ArgumentNullException(nameof(t));
        
        var result = new VDS.RDF.Graph();
        // Copy all triples from g
        foreach (var triple in g._inner.Triples)
        {
            result.Assert(triple);
        }
        // Add the new triple
        result.Assert(t.ToVdsTriple());
        return new Graph(result);
    }

    /// <summary>
    /// Non-mutating addition: returns a new graph containing triples from t plus all from g.
    /// Commutative with Graph + Triple.
    /// </summary>
    public static Graph operator +(Triple t, Graph g)
    {
        return g + t; // Commutative
    }

    /// <summary>
    /// Non-mutating subtraction: returns a new graph containing all triples from g except t.
    /// Original graph g is unchanged.
    /// </summary>
    public static Graph operator -(Graph g, Triple t)
    {
        if (g == null) throw new ArgumentNullException(nameof(g));
        if (t == null) throw new ArgumentNullException(nameof(t));
        
        var result = new VDS.RDF.Graph();
        var vdsTriple = t.ToVdsTriple();
        // Copy all triples from g except the one to remove
        foreach (var triple in g._inner.Triples)
        {
            if (!triple.Equals(vdsTriple))
            {
                result.Assert(triple);
            }
        }
        return new Graph(result);
    }

    /// <summary>
    /// Non-mutating addition: returns a new graph containing all triples from both graphs.
    /// Original graphs are unchanged. Duplicates are eliminated (set semantics).
    /// </summary>
    public static Graph operator +(Graph g1, Graph g2)
    {
        if (g1 == null) throw new ArgumentNullException(nameof(g1));
        if (g2 == null) throw new ArgumentNullException(nameof(g2));
        
        var result = new VDS.RDF.Graph();
        // Copy all triples from g1
        foreach (var triple in g1._inner.Triples)
        {
            result.Assert(triple);
        }
        // Merge triples from g2 (set semantics: duplicates suppressed)
        foreach (var triple in g2._inner.Triples)
        {
            result.Assert(triple);
        }
        return new Graph(result);
    }

    /// <summary>
    /// Non-mutating subtraction: returns a new graph containing triples in g1 but not in g2.
    /// Original graphs are unchanged.
    /// </summary>
    public static Graph operator -(Graph g1, Graph g2)
    {
        if (g1 == null) throw new ArgumentNullException(nameof(g1));
        if (g2 == null) throw new ArgumentNullException(nameof(g2));
        
        var result = new VDS.RDF.Graph();
        var g2Triples = new HashSet<VDS.RDF.Triple>(g2._inner.Triples);
        // Copy triples from g1 that are not in g2
        foreach (var triple in g1._inner.Triples)
        {
            if (!g2Triples.Contains(triple))
            {
                result.Assert(triple);
            }
        }
        return new Graph(result);
    }

    // ============================================================================
    // Mutating compound assignment operators (modify LHS, return LHS)
    // Note: C# doesn't allow custom compound assignment operators directly.
    // These are provided as regular methods that the compiler can use.
    // ============================================================================

    /// <summary>
    /// Mutating addition: adds triple t to graph g and returns g.
    /// Graph g is modified in place.
    /// </summary>
    public Graph AddInPlace(Triple t)
    {
        Add(t);
        return this;
    }

    /// <summary>
    /// Mutating subtraction: removes triple t from graph g and returns g.
    /// Graph g is modified in place.
    /// </summary>
    public Graph RemoveInPlace(Triple t)
    {
        Remove(t);
        return this;
    }

    /// <summary>
    /// Mutating addition: merges all triples from g2 into g1 and returns g1.
    /// Graph g1 is modified in place.
    /// </summary>
    public Graph MergeInPlace(Graph g2)
    {
        if (g2 == null) throw new ArgumentNullException(nameof(g2));
        foreach (var triple in g2._inner.Triples)
        {
            if (_inner is VDS.RDF.Graph concrete)
            {
                concrete.Assert(triple);
            }
            else
            {
                _inner.Assert(new[] { triple });
            }
        }
        return this;
    }

    /// <summary>
    /// Mutating subtraction: removes all triples in g2 from g1 and returns g1.
    /// Graph g1 is modified in place.
    /// </summary>
    public Graph DifferenceInPlace(Graph g2)
    {
        if (g2 == null) throw new ArgumentNullException(nameof(g2));
        foreach (var triple in g2._inner.Triples)
        {
            if (_inner is VDS.RDF.Graph concrete)
            {
                concrete.Retract(triple);
            }
            else
            {
                _inner.Retract(new[] { triple });
            }
        }
        return this;
    }
}
