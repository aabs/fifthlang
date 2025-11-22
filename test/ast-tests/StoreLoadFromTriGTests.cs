using FluentAssertions;
using Fifth.System;
using VDS.RDF;

namespace ast_tests;

/// <summary>
/// Unit tests for Store.LoadFromTriG() method to ensure proper TriG parsing
/// and Store content validation. Tests validate that TriG content is correctly
/// parsed and stored RDF data matches expectations.
/// </summary>
[Trait("Category", "KG")]
[Trait("Category", "TriG")]
[Trait("Category", "Unit")]
public class StoreLoadFromTriGTests
{
    [Fact]
    public void LoadFromTriG_WithBasicTriples_ShouldParseCorrectly()
    {
        // Arrange
        var trigContent = @"
            @prefix ex: <http://example.org/> .
            
            ex:graph1 {
                ex:Andrew ex:name ""Andrew"" ;
                         ex:age 42 .
            }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull("Store should be created");
        
        // Verify the graph exists and contains expected triples
        var graphUri = new Uri("http://example.org/graph1");
        var graph = store.LoadGraph(graphUri);
        
        graph.Should().NotBeNull("Graph should be loaded");
        
        var vdsGraph = graph.ToVds();
        vdsGraph.Triples.Count.Should().Be(2, "Should have 2 triples");
        
        // Check for the name triple
        var nameTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/name")));
        nameTriples.Should().HaveCount(1, "Should have one name triple");
        var nameTriple = nameTriples.First();
        nameTriple.Object.Should().BeOfType<LiteralNode>("Object should be a literal");
        ((LiteralNode)nameTriple.Object).Value.Should().Be("Andrew");
        
        // Check for the age triple
        var ageTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/age")));
        ageTriples.Should().HaveCount(1, "Should have one age triple");
        var ageTriple = ageTriples.First();
        ageTriple.Object.Should().BeOfType<LiteralNode>("Object should be a literal");
        ((LiteralNode)ageTriple.Object).Value.Should().Be("42");
    }

    [Fact]
    public void LoadFromTriG_WithMultipleGraphs_ShouldParseCorrectly()
    {
        // Arrange
        var trigContent = @"
            @prefix ex: <http://example.org/> .
            
            ex:graph1 {
                ex:Person1 ex:name ""Alice"" .
            }
            
            ex:graph2 {
                ex:Person2 ex:name ""Bob"" .
            }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull();
        
        // Verify graph1
        var graph1 = store.LoadGraph(new Uri("http://example.org/graph1"));
        graph1.ToVds().Triples.Count.Should().Be(1, "Graph1 should have 1 triple");
        
        // Verify graph2
        var graph2 = store.LoadGraph(new Uri("http://example.org/graph2"));
        graph2.ToVds().Triples.Count.Should().Be(1, "Graph2 should have 1 triple");
    }

    [Fact]
    public void LoadFromTriG_WithNumericLiterals_ShouldParseCorrectly()
    {
        // Arrange
        var trigContent = @"
            @prefix ex: <http://example.org/> .
            
            ex:graph1 {
                ex:Item ex:count 42 ;
                        ex:price 19.99 .
            }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull();
        
        var graph = store.LoadGraph(new Uri("http://example.org/graph1"));
        var vdsGraph = graph.ToVds();
        
        // Check for numeric values
        var countTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/count")));
        countTriples.Should().HaveCount(1);
        var countTriple = countTriples.First();
        countTriple.Object.Should().BeOfType<LiteralNode>();
        ((LiteralNode)countTriple.Object).Value.Should().Be("42");
        
        var priceTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/price")));
        priceTriples.Should().HaveCount(1);
        var priceTriple = priceTriples.First();
        priceTriple.Object.Should().BeOfType<LiteralNode>();
        ((LiteralNode)priceTriple.Object).Value.Should().Be("19.99");
    }

    [Fact]
    public void LoadFromTriG_WithNestedIRIs_ShouldParseCorrectly()
    {
        // Arrange
        var trigContent = @"
            <http://example.org/graph1> {
                <http://example.org/subject1> <http://example.org/predicate1> <http://example.org/object1> .
                <http://example.org/subject2> <http://example.org/predicate2> ""literal value"" .
            }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull();
        
        var graph = store.LoadGraph(new Uri("http://example.org/graph1"));
        var vdsGraph = graph.ToVds();
        vdsGraph.Triples.Count.Should().Be(2, "Should have 2 triples");
        
        // Verify IRI object
        var iriObjectTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/predicate1")));
        iriObjectTriples.Should().HaveCount(1);
        var iriObjectTriple = iriObjectTriples.First();
        iriObjectTriple.Object.Should().BeOfType<UriNode>("Object should be a URI");
        ((UriNode)iriObjectTriple.Object).Uri.ToString().Should().Be("http://example.org/object1");
    }

    [Fact]
    public void LoadFromTriG_WithEmptyContent_ShouldCreateEmptyStore()
    {
        // Arrange
        var trigContent = "";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull("Store should be created even with empty content");
    }

    [Fact]
    public void LoadFromTriG_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Arrange
        string trigContent = null!;

        // Act
        var act = () => Store.LoadFromTriG(trigContent);

        // Assert
        act.Should().Throw<ArgumentNullException>("Null content should throw");
    }

    [Fact]
    public void LoadFromTriG_WithInvalidTriG_ShouldThrowException()
    {
        // Arrange - Invalid TriG syntax (missing closing brace)
        var trigContent = @"
            @prefix ex: <http://example.org/> .
            
            ex:graph1 {
                ex:Item ex:value ""test"" .
        ";

        // Act
        var act = () => Store.LoadFromTriG(trigContent);

        // Assert
        act.Should().Throw<Exception>("Invalid TriG should throw during parsing");
    }

    [Fact]
    public void LoadFromTriG_WithBooleanLiterals_ShouldParseCorrectly()
    {
        // Arrange
        var trigContent = @"
            @prefix ex: <http://example.org/> .
            
            ex:graph1 {
                ex:Item ex:active true ;
                        ex:enabled false .
            }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull();
        
        var graph = store.LoadGraph(new Uri("http://example.org/graph1"));
        var vdsGraph = graph.ToVds();
        vdsGraph.Triples.Count.Should().Be(2);
        
        // Check boolean values
        var activeTriples = vdsGraph.GetTriplesWithPredicate(vdsGraph.CreateUriNode(new Uri("http://example.org/active")));
        activeTriples.Should().HaveCount(1);
        var activeTriple = activeTriples.First();
        activeTriple.Object.Should().BeOfType<LiteralNode>();
        ((LiteralNode)activeTriple.Object).Value.Should().Be("true");
    }

    [Fact]
    public void LoadFromTriG_WithWhitespaceAndNewlines_ShouldParseCorrectly()
    {
        // Arrange - Content with varying whitespace
        var trigContent = @"
@prefix ex: <http://example.org/> .

        ex:graph1 {
            ex:Item1 ex:value ""one"" .
            
            ex:Item2 ex:value ""two"" .
        }
        ";

        // Act
        var store = Store.LoadFromTriG(trigContent);

        // Assert
        store.Should().NotBeNull();
        
        var graph = store.LoadGraph(new Uri("http://example.org/graph1"));
        var vdsGraph = graph.ToVds();
        vdsGraph.Triples.Count.Should().Be(2, "Whitespace should not affect triple count");
    }
}
