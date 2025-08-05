using ast;
using compiler.LanguageTransformations;
using Fifth.LangProcessingPhases;

namespace ast_tests;

public class DestructuringPatternFlattenerVisitorTests : VisitorTestsBase
{
    [Fact]
    public void ParsedMethodWithDestructionDecl_WhenVisitedWithDestructuringPatternFlattenerVisitor_ThenReturnsMethodWithFlattenedDestructure()
    {
        // Arrange
        var ast = (AstThing)ParseProgram("recursive-destructuring.5th");
        ast = new TreeLinkageVisitor().Visit(ast);
        ast = new BuiltinInjectorVisitor().Visit(ast);
        ast = new ClassCtorInserter().Visit(ast);
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        ast = new PropertyToFieldExpander().Visit(ast);
        ast = new OverloadGatheringVisitor().Visit(ast);
        ast = new OverloadTransformingVisitor().Visit(ast);
        ast = new DestructuringVisitor().Visit(ast);

        // Act
        ast = new DestructuringPatternFlattenerVisitor().Visit(ast);
        if (ast is AssemblyDef asm)
        {
            var fn_calculate_bmi = asm.Modules[0].Functions[0];
            new DumpTreeVisitor(Console.Out).Visit(fn_calculate_bmi);
        }
        // Assert
    }
}
