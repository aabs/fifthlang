using ast;
using ast_generated;
using ast_model.TypeSystem;
using Fifth.LangProcessingPhases;
using FluentAssertions;

namespace ast_tests;

public class GraphAssertionBlock_TypeTests : VisitorTestsBase
{
    private readonly TypeAnnotationVisitor _visitor = new();

    [Test]
    public void GraphAssertionBlock_Expression_ShouldHaveGraphType()
    {
        var inner = new BlockStatement { Statements = [] };
        var exp = new GraphAssertionBlockExp { Content = inner };

        var result = _visitor.VisitGraphAssertionBlockExp(exp);

        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TType>();
        result.Type.Name.Value.Should().Be("graph");
    }

    [Test]
    public void GraphAssertionBlock_Statement_ShouldBeVoid()
    {
        var inner = new BlockStatement { Statements = [] };
        var exp = new GraphAssertionBlockExp { Content = inner };
        var stmt = new GraphAssertionBlockStatement { Content = exp };

        var result = _visitor.VisitGraphAssertionBlockStatement(stmt);

        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TVoidType>();
        result.Type.Name.Value.Should().Be("void");
    }
}
