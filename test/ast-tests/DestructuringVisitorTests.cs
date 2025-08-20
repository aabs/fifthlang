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
        if (ast is AssemblyDef asm)
        {
            var fn_calculate_bmi = (FunctionDef)asm.Modules[0].Functions[0];
            fn_calculate_bmi.Should().NotBeNull();
            fn_calculate_bmi.Params.Should().HaveCount(1);
            fn_calculate_bmi.Params[0].Name.Should().Be("p");
            fn_calculate_bmi.Params[0].DestructureDef.Should().NotBeNull();
            fn_calculate_bmi.Params[0].DestructureDef.Bindings.Should().HaveCount(2);
            fn_calculate_bmi.Params[0].DestructureDef.Bindings[0].ReferencedProperty.Should().NotBeNull();
            fn_calculate_bmi.Params[0].DestructureDef.Bindings[1].ReferencedProperty.Should().NotBeNull();
        }
    }
}
