using System;
using VDS.RDF;

namespace Fifth.System;

/// <summary>
/// Thin wrapper over dotNetRDF Triple providing Fifth language semantics.
/// Represents an RDF triple (subject, predicate, object).
/// </summary>
public sealed class Triple
{
    private readonly INode _subject;
    private readonly INode _predicate;
    private readonly INode _object;

    /// <summary>
    /// Internal constructor for creating a triple from three nodes.
    /// Use Create factory method for public construction.
    /// </summary>
    internal Triple(INode subject, INode predicate, INode obj)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _object = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    /// <summary>
    /// Creates a new triple from three nodes.
    /// </summary>
    public static Triple Create(INode subject, INode predicate, INode obj)
    {
        return new Triple(subject, predicate, obj);
    }

    /// <summary>
    /// Gets the subject node of the triple.
    /// </summary>
    public INode Subject => _subject;

    /// <summary>
    /// Gets the predicate node of the triple.
    /// </summary>
    public INode Predicate => _predicate;

    /// <summary>
    /// Gets the object node of the triple.
    /// </summary>
    public INode Object => _object;

    /// <summary>
    /// Converts this wrapper to a dotNetRDF Triple for interop.
    /// </summary>
    public VDS.RDF.Triple ToVdsTriple()
    {
        return new VDS.RDF.Triple(_subject, _predicate, _object);
    }

    /// <summary>
    /// Creates a wrapper from a dotNetRDF Triple for interop.
    /// </summary>
    public static Triple FromVds(VDS.RDF.Triple triple)
    {
        if (triple == null) throw new ArgumentNullException(nameof(triple));
        return new Triple(triple.Subject, triple.Predicate, triple.Object);
    }

    /// <summary>
    /// Non-mutating addition: creates a new graph containing both triples.
    /// Per FR-011: triple + triple -> graph
    /// </summary>
    public static Graph operator +(Triple a, Triple b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        
        var graph = Graph.Create();
        graph.Add(a);
        graph.Add(b);
        return graph;
    }

    // Note: Triple - Triple is explicitly NOT provided per FR-011

    public override bool Equals(object? obj)
    {
        if (obj is Triple other)
        {
            return _subject.Equals(other._subject) 
                && _predicate.Equals(other._predicate) 
                && _object.Equals(other._object);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_subject, _predicate, _object);
    }

    public override string ToString()
    {
        return $"({_subject}, {_predicate}, {_object})";
    }
}
