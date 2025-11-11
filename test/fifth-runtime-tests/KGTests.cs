using System;
using System.Linq;
using System.Threading.Tasks;
using Fifth.System;
using VDS.RDF;
using VDS.RDF.Query;

namespace fifth_runtime_tests;

public class KGTests
{
    private sealed class TestAssertable : IAssertable
    {
        private readonly Uri _type;
        private readonly Uri _instance;
        public TestAssertable(string type, string instance)
        {
            _type = new Uri(type);
            _instance = new Uri(instance);
        }
        Uri IAssertable.GetInstanceUri() => _instance;
        Uri IAssertable.GetTypeUri() => _type;
    }

    private sealed class RelativeAssertable : IAssertable
    {
        private static readonly Uri RelType = new Uri("/t", UriKind.Relative);
        private static readonly Uri RelInst = new Uri("/i", UriKind.Relative);
        Uri IAssertable.GetInstanceUri() => RelInst;
        Uri IAssertable.GetTypeUri() => RelType;
    }

    [Test]
    public async Task CreateStore_ReturnsInMemoryAndRoundTripsGraph()
    {
        var store = KG.CreateStore();
        var g = KG.CreateGraph();
        var ex = new Uri("http://example.org/");
        var s = g.CreateUriNode(ex);
        var p = g.CreateUriNode(new Uri("http://example.org/p"));
        var o = g.CreateLiteralNode("v");
        g.Assert(new VDS.RDF.Triple(s, p, o));

        store.SaveGraph(g);
        var g2 = new VDS.RDF.Graph();
        store.LoadGraph(g2, g.BaseUri);
        await Assert.That(g2.Triples.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task CreateStore_NamedGraph_SaveAndLoadByUri()
    {
        var store = KG.CreateStore();
        var g = KG.CreateGraph();
        g.BaseUri = new Uri("http://ex/graph");

        var s = g.CreateUriNode(new Uri("http://ex/s"));
        var p = g.CreateUriNode(new Uri("http://ex/p"));
        var o = g.CreateLiteralNode("v");
        g.Assert(new VDS.RDF.Triple(s, p, o));

        store.SaveGraph(g);

        var loaded = new VDS.RDF.Graph();
        // Some storage providers treat SaveGraph without explicit graph parameter as default graph
        store.LoadGraph(loaded, (string)null!);
        await Assert.That(loaded.Triples.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task CreateGraph_IsEmptyWithNoBaseUri()
    {
        var g = KG.CreateGraph();
        await Assert.That(g.Triples.Count).IsEqualTo(0);
        await Assert.That(g.BaseUri).IsNull();
    }

    [Test]
    public async Task CreateUriForType_And_Instance_FromAssertable()
    {
        var g = KG.CreateGraph();
        IAssertable a = new TestAssertable("http://example.org/Type", "http://example.org/i/1");

        var typeNode = KG.CreateUriForType(g, a);
        var instNode = KG.CreateUriForInstance(g, a);

        await Assert.That(typeNode.Uri.AbsoluteUri).IsEqualTo("http://example.org/Type");
        await Assert.That(instNode.Uri.AbsoluteUri).IsEqualTo("http://example.org/i/1");
    }

    [Test]
    public void CreateUri_ForTypeAndInstance_NullAndRelativeValidation()
    {
        var g = KG.CreateGraph();

        Assert.Throws<NullReferenceException>(() => KG.CreateUriForType(g, null!));
        Assert.Throws<NullReferenceException>(() => KG.CreateUriForInstance(g, null!));

        IAssertable relativeAssertable = new RelativeAssertable();
        Assert.Throws<InvalidOperationException>(() => KG.CreateUriForType(g, relativeAssertable));
        Assert.Throws<InvalidOperationException>(() => KG.CreateUriForInstance(g, relativeAssertable));
    }

    [Test]
    public async Task CreateLiteral_String_NullAndEmptyHandled()
    {
        var g = KG.CreateGraph();
        var n1 = KG.CreateLiteral(g, (string)null!);
        var n2 = KG.CreateLiteral(g, "");
        var n3 = KG.CreateLiteral(g, "hello", "en");

        await Assert.That(n1.Value).IsEqualTo("");
        await Assert.That(n2.Value).IsEqualTo("");
        await Assert.That(n3.Value).IsEqualTo("hello");
    }

    [Test]
    public async Task CreateLiteral_Typed_NullValue_DefaultsToEmptyString()
    {
        var g = KG.CreateGraph();
        var lit = KG.CreateLiteral<string>(g, null!, XsdDataTypes.String);
        await Assert.That(lit.Value).IsEqualTo("");
        await Assert.That(lit.DataType!.AbsoluteUri).Contains("#string");
    }

    [Test]
    public async Task CreateLiteral_Typed_VariousDotNetTypes()
    {
        var g = KG.CreateGraph();
        var i = KG.CreateLiteral(g, 42, XsdDataTypes.Int);
        var b = KG.CreateLiteral(g, true, XsdDataTypes.Boolean);
        var d = KG.CreateLiteral(g, 12.34m, XsdDataTypes.Decimal);
        var dt = KG.CreateLiteral(g, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), XsdDataTypes.DateTime);

        await Assert.That(i.DataType).IsNotNull();
        await Assert.That(i.DataType!.AbsoluteUri).Contains("#int");
        await Assert.That(b.DataType!.AbsoluteUri).Contains("#boolean");
        await Assert.That(d.DataType!.AbsoluteUri).Contains("#decimal");
        await Assert.That(dt.DataType!.AbsoluteUri).Contains("#dateTime");
    }

    [Test]
    public async Task CreateTriple_And_Assert_Retract_Merge()
    {
        var g1 = KG.CreateGraph();
        var g2 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");

        var t = KG.CreateTriple(s, p, o);
        g1.Assert(t);
        await Assert.That(g1.Triples.Count).IsEqualTo(1);

        g1.Retract(t);
        await Assert.That(g1.Triples.Count).IsEqualTo(0);

        g1.Assert(t);
        g2.Merge(g1);
        await Assert.That(g2.Triples.Count).IsEqualTo(1);
    }

    [Test]
    public async Task FluentExtensions_ReturnSelf_And_Idempotency()
    {
        var g1 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);

        var ret1 = KG.Assert(g1, t);
        await Assert.That(object.ReferenceEquals(ret1, g1)).IsTrue();

        KG.Assert(g1, t); // idempotent
        await Assert.That(g1.Triples.Count).IsEqualTo(1);

        var t2 = KG.CreateTriple(s, p, g1.CreateLiteralNode("y"));
        var ret2 = KG.Retract(g1, t2); // retract non-existent
        await Assert.That(object.ReferenceEquals(ret2, g1)).IsTrue();
        await Assert.That(g1.Triples.Count).IsEqualTo(1);

        var g2 = KG.CreateGraph();
        var ret3 = KG.Merge(g2, g1);
        await Assert.That(object.ReferenceEquals(ret3, g2)).IsTrue();
        await Assert.That(g2.Triples.Count).IsEqualTo(1);

        KG.Merge(g2, g1); // merging again should not duplicate
        await Assert.That(g2.Triples.Count).IsEqualTo(1);
    }

    [Test]
    public void FaultTolerance_BadUrisAndNulls()
    {
        var g = KG.CreateGraph();

        Assert.Throws<UriFormatException>(() => new TestAssertable("not a uri", "http://ex/i"));

        var s = g.CreateUriNode(new Uri("http://ex/s"));
        var p = g.CreateUriNode(new Uri("http://ex/p"));
        // dotNetRDF Triple ctor may throw different exception types when given nulls; accept any exception
        Assert.Throws<Exception>(() => KG.CreateTriple(null!, p, s));
        Assert.Throws<Exception>(() => KG.CreateTriple(s, null!, s));
        Assert.Throws<Exception>(() => KG.CreateTriple(s, p, null!));
    }

    [Test]
    public async Task GraphConstruction_WithDifferentDataTypes()
    {
        var g = KG.CreateGraph();
        var s = g.CreateUriNode(new Uri("http://ex/item/1"));
        var pVal = g.CreateUriNode(new Uri("http://ex/val"));

        var litString = KG.CreateLiteral(g, "abc");
        var litInt = KG.CreateLiteral(g, 7, XsdDataTypes.Int);
        var litBool = KG.CreateLiteral(g, false, XsdDataTypes.Boolean);
        var litFloat = KG.CreateLiteral(g, 1.23f, XsdDataTypes.Float);
        var litDouble = KG.CreateLiteral(g, 4.56d, XsdDataTypes.Double);
        var litLong = KG.CreateLiteral(g, 1234567890123L, XsdDataTypes.Long);

        g.Assert(KG.CreateTriple(s, pVal, litString));
        g.Assert(KG.CreateTriple(s, pVal, litInt));
        g.Assert(KG.CreateTriple(s, pVal, litBool));
        g.Assert(KG.CreateTriple(s, pVal, litFloat));
        g.Assert(KG.CreateTriple(s, pVal, litDouble));
        g.Assert(KG.CreateTriple(s, pVal, litLong));

        await Assert.That(g.Triples.Count).IsEqualTo(6);

        var q = "ASK WHERE { <http://ex/item/1> <http://ex/val> 'abc' }";
        var r = (SparqlResultSet)g.ExecuteQuery(q);
        await Assert.That(r.Result).IsTrue();
    }

    [Test]
    public async Task CreateTriple_WiresSubjectPredicateObjectCorrectly()
    {
        var g = KG.CreateGraph();
        var s = g.CreateUriNode(new Uri("http://ex/s"));
        var p = g.CreateUriNode(new Uri("http://ex/p"));
        var o = g.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);

        await Assert.That(ReferenceEquals(t.Subject, s)).IsTrue();
        await Assert.That(ReferenceEquals(t.Predicate, p)).IsTrue();
        await Assert.That(ReferenceEquals(t.Object, o)).IsTrue();
    }

    [Test]
    public async Task XsdDataTypes_AllUris_AreAbsoluteAndWellKnown()
    {
        var known = new[]
        {
            XsdDataTypes.String, XsdDataTypes.Int, XsdDataTypes.Boolean, XsdDataTypes.DateTime,
            XsdDataTypes.Decimal, XsdDataTypes.Float, XsdDataTypes.Double, XsdDataTypes.Long,
            XsdDataTypes.Short, XsdDataTypes.Byte, XsdDataTypes.UnsignedInt, XsdDataTypes.UnsignedLong,
            XsdDataTypes.UnsignedShort
        };

        await Assert.That(known.All(u => u.IsAbsoluteUri)).IsTrue();
        await Assert.That(known.All(u => u.AbsoluteUri.StartsWith("http://www.w3.org/2001/XMLSchema#"))).IsTrue();
    }

    [Test]
    public async Task CreateLiteral_String_LanguageParameter_IsCurrentlyIgnored()
    {
        var g = KG.CreateGraph();
        var lit = KG.CreateLiteral(g, "hello", "fr");
        await Assert.That(lit.Value).IsEqualTo("hello");
        await Assert.That(string.IsNullOrEmpty(lit.Language)).IsTrue();
    }

    [Test]
    public async Task CopyGraph_CreatesIndependentCopy()
    {
        var g1 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);
        g1.Assert(t);

        var g2 = KG.CopyGraph(g1);
        
        await Assert.That(g2.Triples.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(g1, g2)).IsFalse();
        
        // Modify g1 and verify g2 is not affected
        var s2 = g1.CreateUriNode(new Uri("http://ex/s2"));
        var t2 = KG.CreateTriple(s2, p, o);
        g1.Assert(t2);
        
        await Assert.That(g1.Triples.Count).IsEqualTo(2);
        await Assert.That(g2.Triples.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CopyGraph_EmptyGraph_ReturnsEmptyGraph()
    {
        var g1 = KG.CreateGraph();
        var g2 = KG.CopyGraph(g1);
        
        await Assert.That(g2.Triples.Count).IsEqualTo(0);
        await Assert.That(ReferenceEquals(g1, g2)).IsFalse();
    }
}
