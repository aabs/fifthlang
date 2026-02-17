using System;
using System.Linq;
using Fifth.System;
using Xunit;
using FluentAssertions;

namespace ast_tests;

/// <summary>
/// Tests for the Fifth.System wrapper classes: Graph, Triple, and Store.
/// These tests validate the operator semantics per FR-011 from spec.md.
/// </summary>
public class SystemKGTypesTests
{
    private static VDS.RDF.IUriNode CreateUri(string uri) => new VDS.RDF.NodeFactory().CreateUriNode(new Uri(uri));
    private static VDS.RDF.ILiteralNode CreateLiteral(string value) => new VDS.RDF.NodeFactory().CreateLiteralNode(value);
    
    #region Triple Tests
    
    [Fact]
    public void Triple_Create_ReturnsValidTriple()
    {
        var s = CreateUri("http://example.org/s");
        var p = CreateUri("http://example.org/p");
        var o = CreateLiteral("value");
        
        var triple = Triple.Create(s, p, o);
        
        triple.Should().NotBeNull();
        triple.Subject.Should().Be(s);
        triple.Predicate.Should().Be(p);
        triple.Object.Should().Be(o);
    }
    
    [Fact]
    public void Triple_ToVdsTriple_ReturnsValidVdsTriple()
    {
        var s = CreateUri("http://example.org/s");
        var p = CreateUri("http://example.org/p");
        var o = CreateLiteral("value");
        
        var triple = Triple.Create(s, p, o);
        var vdsTriple = triple.ToVdsTriple();
        
        vdsTriple.Should().NotBeNull();
        vdsTriple.Subject.Should().Be(s);
        vdsTriple.Predicate.Should().Be(p);
        vdsTriple.Object.Should().Be(o);
    }
    
    [Fact]
    public void Triple_FromVds_ReturnsValidWrapper()
    {
        var s = CreateUri("http://example.org/s");
        var p = CreateUri("http://example.org/p");
        var o = CreateLiteral("value");
        var vdsTriple = new VDS.RDF.Triple(s, p, o);
        
        var triple = Triple.FromVds(vdsTriple);
        
        triple.Should().NotBeNull();
        triple.Subject.Should().Be(s);
        triple.Predicate.Should().Be(p);
        triple.Object.Should().Be(o);
    }
    
    [Fact]
    public void Triple_PlusTriple_ReturnsGraphWithBothTriples()
    {
        // FR-011: triple + triple -> graph (non-mutating)
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        
        var graph = t1 + t2;
        
        graph.Should().NotBeNull();
        graph.Count.Should().Be(2);
        graph.Triples.Count().Should().Be(2);
    }
    
    #endregion
    
    #region Graph Tests
    
    [Fact]
    public void Graph_Create_ReturnsEmptyGraph()
    {
        var graph = Graph.Create();
        
        graph.Should().NotBeNull();
        graph.Count.Should().Be(0);
    }
    
    [Fact]
    public void Graph_Add_IncreasesCount()
    {
        var graph = Graph.Create();
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        
        graph.Add(triple);
        
        graph.Count.Should().Be(1);
    }
    
    [Fact]
    public void Graph_Add_Idempotent_SetSemantics()
    {
        // Per FR-011: Graphs use set semantics: duplicate triples are suppressed
        var graph = Graph.Create();
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        
        graph.Add(triple);
        graph.Add(triple); // Add same triple again
        
        graph.Count.Should().Be(1); // Should still be 1
    }
    
    [Fact]
    public void Graph_Remove_DecreasesCount()
    {
        var graph = Graph.Create();
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        graph.Add(triple);
        
        graph.Remove(triple);
        
        graph.Count.Should().Be(0);
    }
    
    [Fact]
    public void Graph_PlusTriple_ReturnsNewGraphWithTriple()
    {
        // FR-011: graph + triple -> graph (non-mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        g1.Add(t1);
        
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        var g2 = g1 + t2;
        
        g1.Count.Should().Be(1); // Original unchanged
        g2.Count.Should().Be(2); // Result has both
        object.ReferenceEquals(g1, g2).Should().BeFalse(); // Different instances
    }
    
    [Fact]
    public void Graph_MinusTriple_ReturnsNewGraphWithoutTriple()
    {
        // FR-011: graph - triple -> graph (non-mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        g1.Add(t1);
        g1.Add(t2);
        
        var g2 = g1 - t1;
        
        g1.Count.Should().Be(2); // Original unchanged
        g2.Count.Should().Be(1); // Result has one less
        object.ReferenceEquals(g1, g2).Should().BeFalse(); // Different instances
    }
    
    [Fact]
    public void Graph_PlusGraph_ReturnsNewMergedGraph()
    {
        // FR-011: graph + graph -> graph (non-mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        g1.Add(t1);
        
        var g2 = Graph.Create();
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        g2.Add(t2);
        
        var g3 = g1 + g2;
        
        g1.Count.Should().Be(1); // Originals unchanged
        g2.Count.Should().Be(1);
        g3.Count.Should().Be(2); // Result has both
        object.ReferenceEquals(g1, g3).Should().BeFalse();
        object.ReferenceEquals(g2, g3).Should().BeFalse();
    }
    
    [Fact]
    public void Graph_MinusGraph_ReturnsNewDifferenceGraph()
    {
        // FR-011: graph - graph -> graph (non-mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        g1.Add(t1);
        g1.Add(t2);
        
        var g2 = Graph.Create();
        g2.Add(t1); // Only t1
        
        var g3 = g1 - g2;
        
        g1.Count.Should().Be(2); // Originals unchanged
        g2.Count.Should().Be(1);
        g3.Count.Should().Be(1); // Result has only t2
    }
    
    [Fact]
    public void Graph_AddInPlace_ModifiesOriginalGraph()
    {
        // FR-011: graph += triple -> graph (mutating)
        var g = Graph.Create();
        var t = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        
        var result = g.AddInPlace(t);
        
        g.Count.Should().Be(1); // Original modified
        object.ReferenceEquals(g, result).Should().BeTrue(); // Same instance returned
    }
    
    [Fact]
    public void Graph_RemoveInPlace_ModifiesOriginalGraph()
    {
        // FR-011: graph -= triple -> graph (mutating)
        var g = Graph.Create();
        var t = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        g.Add(t);
        
        var result = g.RemoveInPlace(t);
        
        g.Count.Should().Be(0); // Original modified
        object.ReferenceEquals(g, result).Should().BeTrue(); // Same instance returned
    }
    
    [Fact]
    public void Graph_MergeInPlace_ModifiesOriginalGraph()
    {
        // FR-011: graph += graph -> graph (mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        g1.Add(t1);
        
        var g2 = Graph.Create();
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        g2.Add(t2);
        
        var result = g1.MergeInPlace(g2);
        
        g1.Count.Should().Be(2); // Original modified
        g2.Count.Should().Be(1); // g2 unchanged
        object.ReferenceEquals(g1, result).Should().BeTrue(); // Same instance returned
    }
    
    [Fact]
    public void Graph_DifferenceInPlace_ModifiesOriginalGraph()
    {
        // FR-011: graph -= graph -> graph (mutating)
        var g1 = Graph.Create();
        var t1 = Triple.Create(CreateUri("http://ex/s1"), CreateUri("http://ex/p"), CreateLiteral("v1"));
        var t2 = Triple.Create(CreateUri("http://ex/s2"), CreateUri("http://ex/p"), CreateLiteral("v2"));
        g1.Add(t1);
        g1.Add(t2);
        
        var g2 = Graph.Create();
        g2.Add(t1);
        
        var result = g1.DifferenceInPlace(g2);
        
        g1.Count.Should().Be(1); // Original modified
        g2.Count.Should().Be(1); // g2 unchanged
        object.ReferenceEquals(g1, result).Should().BeTrue(); // Same instance returned
    }
    
    [Fact]
    public void Graph_ToVds_ReturnsUnderlyingIGraph()
    {
        var graph = Graph.Create();
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        graph.Add(triple);
        
        var vdsGraph = graph.ToVds();
        
        vdsGraph.Should().NotBeNull();
        vdsGraph.Triples.Count.Should().Be(1);
    }
    
    [Fact]
    public void Graph_FromVds_ReturnsValidWrapper()
    {
        var vdsGraph = new VDS.RDF.Graph();
        var vdsTriple = new VDS.RDF.Triple(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        vdsGraph.Assert(vdsTriple);
        
        var graph = Graph.FromVds(vdsGraph);
        
        graph.Should().NotBeNull();
        graph.Count.Should().Be(1);
    }
    
    #endregion
    
    #region Store Tests
    
    [Fact]
    public void Store_CreateInMemory_ReturnsValidStore()
    {
        var store = Store.CreateInMemory();
        
        store.Should().NotBeNull();
    }
    
    [Fact]
    public void Store_CreateGraph_ReturnsEmptyGraph()
    {
        var store = Store.CreateInMemory();
        
        var graph = store.CreateGraph();
        
        graph.Should().NotBeNull();
        graph.Count.Should().Be(0);
    }
    
    [Fact]
    public void Store_SaveGraph_SavesSuccessfully()
    {
        var store = Store.CreateInMemory();
        var graph = Graph.Create(new Uri("http://example.org/graph"));
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        graph.Add(triple);
        
        // Just verify no exception is thrown
        store.SaveGraph(graph);
        
        graph.Count.Should().Be(1);
    }
    
    [Fact]
    public void Store_LoadGraph_ReturnsGraphWithTriples()
    {
        var store = Store.CreateInMemory();
        var graphUri = new Uri("http://example.org/graph");
        var graph = Graph.Create(graphUri);
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        graph.Add(triple);
        store.SaveGraph(graph);
        
        var loaded = store.LoadGraph(graphUri);
        
        loaded.Should().NotBeNull();
        loaded.Count.Should().Be(1);
    }
    
    [Fact]
    public void Store_AddGraphInPlace_SavesGraph()
    {
        // FR-011: store += graph -> store (mutating)
        var store = Store.CreateInMemory();
        var graph = Graph.Create(new Uri("http://example.org/graph"));
        var triple = Triple.Create(CreateUri("http://ex/s"), CreateUri("http://ex/p"), CreateLiteral("v"));
        graph.Add(triple);
        
        var result = store.AddGraphInPlace(graph);
        
        object.ReferenceEquals(store, result).Should().BeTrue(); // Same instance returned
    }
    
    [Fact]
    public void Store_ToVds_ReturnsUnderlyingStorage()
    {
        var store = Store.CreateInMemory();
        
        var vdsStore = store.ToVds();
        
        vdsStore.Should().NotBeNull();
    }
    
    #endregion
}
