using ast;
using compiler.LanguageTransformations;
using FluentAssertions;

namespace ast_tests;

public class GraphAssertionBlock_LoweringExpressionTests : VisitorTestsBase
{
    [Test]
    public void ExpressionForm_ShouldNoOpLowering_AndNotThrow()
    {
        var inner = new BlockStatement { Statements = [] };
        var exp = new GraphAssertionBlockExp { Content = inner };

        var visitor = new GraphAssertionLoweringVisitor();

        var act = () => visitor.VisitGraphAssertionBlockExp(exp);

        act.Should().NotThrow();

        var result = visitor.VisitGraphAssertionBlockExp(exp);
        result.Should().BeSameAs(exp);
        result.Content.Should().BeSameAs(inner);
    }
}
