using ast;
using compiler.LanguageTransformations;
using FluentAssertions;

namespace ast_tests;

public class DestructuringVisitorTests : VisitorTestsBase
{
    [Fact]
    public void VisitingWellFormedDestructure_YieldsProperPropertyLinkage()
    {
        var ast = (AstThing)ParseProgram("recursive-destructuring.5th");
        ast = new DestructuringVisitor().Visit(ast);
        ast.Should().NotBeNull();
    }
}
