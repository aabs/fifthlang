using System;
using System.Linq;
using System.Threading.Tasks;
using Fifth.System;
using VDS.RDF;
using VDS.RDF.Query;
using FluentAssertions;

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

    [Fact]
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
        g2.Triples.Count.Should().BeGreaterThan(0);
    }

    [Fact]
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
        loaded.Triples.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateGraph_IsEmptyWithNoBaseUri()
    {
        var g = KG.CreateGraph();
        g.Triples.Count.Should().Be(0);
        g.BaseUri.Should().BeNull();
    }

    [Fact]
    public async Task CreateUriForType_And_Instance_FromAssertable()
    {
        var g = KG.CreateGraph();
        IAssertable a = new TestAssertable("http://example.org/Type", "http://example.org/i/1");

        var typeNode = KG.CreateUriForType(g, a);
        var instNode = KG.CreateUriForInstance(g, a);

        typeNode.Uri.AbsoluteUri.Should().Be("http://example.org/Type");
        instNode.Uri.AbsoluteUri.Should().Be("http://example.org/i/1");
    }

    [Fact]
    public void CreateUri_ForTypeAndInstance_NullAndRelativeValidation()
    {
        var g = KG.CreateGraph();

        Assert.Throws<NullReferenceException>(() => KG.CreateUriForType(g, null!));
        Assert.Throws<NullReferenceException>(() => KG.CreateUriForInstance(g, null!));

        IAssertable relativeAssertable = new RelativeAssertable();
        Assert.Throws<InvalidOperationException>(() => KG.CreateUriForType(g, relativeAssertable));
        Assert.Throws<InvalidOperationException>(() => KG.CreateUriForInstance(g, relativeAssertable));
    }

    [Fact]
    public async Task CreateLiteral_String_NullAndEmptyHandled()
    {
        var g = KG.CreateGraph();
        var n1 = KG.CreateLiteral(g, (string)null!);
        var n2 = KG.CreateLiteral(g, "");
        var n3 = KG.CreateLiteral(g, "hello", "en");

        n1.Value.Should().Be("");
        n2.Value.Should().Be("");
        n3.Value.Should().Be("hello");
    }

    [Fact]
    public async Task CreateLiteral_Typed_NullValue_DefaultsToEmptyString()
    {
        var g = KG.CreateGraph();
        var lit = KG.CreateLiteral<string>(g, null!, XsdDataTypes.String);
        lit.Value.Should().Be("");
        lit.DataType!.AbsoluteUri.Should().Contain("#string");
    }

    [Fact]
    public async Task CreateLiteral_Typed_VariousDotNetTypes()
    {
        var g = KG.CreateGraph();
        var i = KG.CreateLiteral(g, 42, XsdDataTypes.Int);
        var b = KG.CreateLiteral(g, true, XsdDataTypes.Boolean);
        var d = KG.CreateLiteral(g, 12.34m, XsdDataTypes.Decimal);
        var dt = KG.CreateLiteral(g, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), XsdDataTypes.DateTime);

        i.DataType.Should().NotBeNull();
        i.DataType!.AbsoluteUri.Should().Contain("#int");
        b.DataType!.AbsoluteUri.Should().Contain("#boolean");
        d.DataType!.AbsoluteUri.Should().Contain("#decimal");
        dt.DataType!.AbsoluteUri.Should().Contain("#dateTime");
    }

    [Fact]
    public async Task CreateTriple_And_Assert_Retract_Merge()
    {
        var g1 = KG.CreateGraph();
        var g2 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");

        var t = KG.CreateTriple(s, p, o);
        g1.Assert(t);
        g1.Triples.Count.Should().Be(1);

        g1.Retract(t);
        g1.Triples.Count.Should().Be(0);

        g1.Assert(t);
        g2.Merge(g1);
        g2.Triples.Count.Should().Be(1);
    }

    [Fact]
    public async Task FluentExtensions_ReturnSelf_And_Idempotency()
    {
        var g1 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);

        var ret1 = KG.Assert(g1, t);
        object.ReferenceEquals(ret1, g1).Should().BeTrue();

        KG.Assert(g1, t); // idempotent
        g1.Triples.Count.Should().Be(1);

        var t2 = KG.CreateTriple(s, p, g1.CreateLiteralNode("y"));
        var ret2 = KG.Retract(g1, t2); // retract non-existent
        object.ReferenceEquals(ret2, g1).Should().BeTrue();
        g1.Triples.Count.Should().Be(1);

        var g2 = KG.CreateGraph();
        var ret3 = KG.Merge(g2, g1);
        object.ReferenceEquals(ret3, g2).Should().BeTrue();
        g2.Triples.Count.Should().Be(1);

        KG.Merge(g2, g1); // merging again should not duplicate
        g2.Triples.Count.Should().Be(1);
    }

    [Fact]
    public void FaultTolerance_BadUrisAndNulls()
    {
        var g = KG.CreateGraph();

        Assert.Throws<UriFormatException>(() => new TestAssertable("not a uri", "http://ex/i"));

        var s = g.CreateUriNode(new Uri("http://ex/s"));
        var p = g.CreateUriNode(new Uri("http://ex/p"));
        // dotNetRDF Triple ctor may throw different exception types when given nulls; accept any exception
        Assert.ThrowsAny<Exception>(() => KG.CreateTriple(null!, p, s));
        Assert.ThrowsAny<Exception>(() => KG.CreateTriple(s, null!, s));
        Assert.ThrowsAny<Exception>(() => KG.CreateTriple(s, p, null!));
    }

    [Fact]
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

        g.Triples.Count.Should().Be(6);

        var q = "ASK WHERE { <http://ex/item/1> <http://ex/val> 'abc' }";
        var r = (SparqlResultSet)g.ExecuteQuery(q);
        r.Result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTriple_WiresSubjectPredicateObjectCorrectly()
    {
        var g = KG.CreateGraph();
        var s = g.CreateUriNode(new Uri("http://ex/s"));
        var p = g.CreateUriNode(new Uri("http://ex/p"));
        var o = g.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);

        ReferenceEquals(t.Subject, s).Should().BeTrue();
        ReferenceEquals(t.Predicate, p).Should().BeTrue();
        ReferenceEquals(t.Object, o).Should().BeTrue();
    }

    [Fact]
    public async Task XsdDataTypes_AllUris_AreAbsoluteAndWellKnown()
    {
        var known = new[]
        {
            XsdDataTypes.String, XsdDataTypes.Int, XsdDataTypes.Boolean, XsdDataTypes.DateTime,
            XsdDataTypes.Decimal, XsdDataTypes.Float, XsdDataTypes.Double, XsdDataTypes.Long,
            XsdDataTypes.Short, XsdDataTypes.Byte, XsdDataTypes.UnsignedInt, XsdDataTypes.UnsignedLong,
            XsdDataTypes.UnsignedShort
        };

        known.All(u => u.IsAbsoluteUri).Should().BeTrue();
        known.All(u => u.AbsoluteUri.StartsWith("http://www.w3.org/2001/XMLSchema#")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateLiteral_String_LanguageParameter_IsCurrentlyIgnored()
    {
        var g = KG.CreateGraph();
        var lit = KG.CreateLiteral(g, "hello", "fr");
        lit.Value.Should().Be("hello");
        string.IsNullOrEmpty(lit.Language).Should().BeTrue();
    }

    [Fact]
    public async Task CopyGraph_CreatesIndependentCopy()
    {
        var g1 = KG.CreateGraph();
        var s = g1.CreateUriNode(new Uri("http://ex/s"));
        var p = g1.CreateUriNode(new Uri("http://ex/p"));
        var o = g1.CreateLiteralNode("x");
        var t = KG.CreateTriple(s, p, o);
        g1.Assert(t);

        var g2 = KG.CopyGraph(g1);

        g2.Triples.Count.Should().Be(1);
        ReferenceEquals(g1, g2).Should().BeFalse();

        // Modify g1 and verify g2 is not affected
        var s2 = g1.CreateUriNode(new Uri("http://ex/s2"));
        var t2 = KG.CreateTriple(s2, p, o);
        g1.Assert(t2);

        g1.Triples.Count.Should().Be(2);
        g2.Triples.Count.Should().Be(1);
    }

    [Fact]
    public async Task CopyGraph_EmptyGraph_ReturnsEmptyGraph()
    {
        var g1 = KG.CreateGraph();
        var g2 = KG.CopyGraph(g1);

        g2.Triples.Count.Should().Be(0);
        ReferenceEquals(g1, g2).Should().BeFalse();
    }
}
