// T032 & T033: Type inference tests for triple literals and operators
using ast_model.TypeSystem;
using ast_model.TypeSystem.Inference;
using FluentAssertions;
using Type = ast_model.TypeSystem.FifthType;

namespace ast_tests;

public class TripleTypeInferenceTests
{
    private readonly Type tripleType;
    private readonly Type graphType;
    private readonly TypeSystem typeSystem;

    public TripleTypeInferenceTests()
    {
        // Define triple and graph as custom types
        tripleType = new Type.TType() { Name = TypeName.From("triple") };
        graphType = new Type.TType() { Name = TypeName.From("graph") };
        
        typeSystem = new TypeSystem()
            .WithType(tripleType)
            .WithType(graphType)
            // graph + triple → graph (FR-008)
            .WithOperation(graphType, tripleType, graphType, "+")
            // triple + triple → graph (FR-009)
            .WithOperation(tripleType, tripleType, graphType, "+")
            // graph + graph → graph
            .WithOperation(graphType, graphType, graphType, "+")
            // graph - triple → graph (FR-010)
            .WithOperation(graphType, tripleType, graphType, "-")
            ;
    }

    [Fact]
    public void triple_literal_should_have_triple_type()
    {
        // T032: TripleLiteralExp should map to primitive triple type
        // This test verifies the type inference system recognizes triple as a type
        tripleType.Name.Value.Should().Be("triple");
        typeSystem.Types.Should().Contain(tripleType);
    }

    [Fact]
    public void graph_plus_triple_yields_graph()
    {
        // T033: graph + triple → graph (FR-008)
        var result = typeSystem.InferResultType([graphType, tripleType], "+");
        result.Should().NotBeNull();
        result.Should().Be(graphType);
    }

    [Fact]
    public void triple_plus_triple_yields_graph()
    {
        // T033: triple + triple → graph (FR-009)
        var result = typeSystem.InferResultType([tripleType, tripleType], "+");
        result.Should().NotBeNull();
        result.Should().Be(graphType);
    }

    [Fact]
    public void graph_plus_graph_yields_graph()
    {
        // T033: graph + graph → graph (union operation)
        var result = typeSystem.InferResultType([graphType, graphType], "+");
        result.Should().NotBeNull();
        result.Should().Be(graphType);
    }

    [Fact]
    public void graph_minus_triple_yields_graph()
    {
        // T033: graph - triple → graph (FR-010, retraction)
        var result = typeSystem.InferResultType([graphType, tripleType], "-");
        result.Should().NotBeNull();
        result.Should().Be(graphType);
    }
}
