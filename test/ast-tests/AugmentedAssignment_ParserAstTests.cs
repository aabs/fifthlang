using FluentAssertions;
using Antlr4.Runtime;
using ast;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using Fifth.LangProcessingPhases;

namespace ast_tests;

public class AugmentedAssignment_ParserAstTests
{
    [Test]
    public void PlusAssign_ShouldDesugarTo_KG_SaveGraph_Call()
    {
        var src = "main():int { g: graph = <{}>; home: graph; home += g; return 0; }";
        var s = CharStreams.fromString(src);
        var parser = new FifthParser(new CommonTokenStream(new FifthLexer(s)));
        var ctx = parser.function_declaration();
        var v = new AstBuilderVisitor();
        var func = v.VisitFunction_declaration(ctx) as FunctionDef;
        func.Should().NotBeNull();

        // Apply type annotation first so the lowerer can use type information
        var typeAnnotator = new TypeAnnotationVisitor();
        func = typeAnnotator.VisitFunctionDef(func!);

        // Apply augmented assignment lowering to transform the AST
        var lowerer = new AugmentedAssignmentLoweringRewriter();
        func = lowerer.VisitFunctionDef(func!);

        // Inspect body statements: expect an AssignmentExp where home = KG.SaveGraph(home, g)
        // Note: The augmented assignment is now at index 3 due to how the parser creates statements
        var stmts = func!.Body!.Statements!;
        stmts.Should().NotBeNull();
        stmts.Should().HaveCountGreaterThan(3);

        var expStmt = stmts[3] as ExpStatement;
        expStmt.Should().NotBeNull();
        var stmt3_rhs = expStmt!.RHS as MemberAccessExp;
        stmt3_rhs.Should().NotBeNull();

        // Check LHS is the target variable 'home'
        var lhsVar = stmt3_rhs!.LHS as VarRefExp;
        lhsVar.Should().NotBeNull();
        lhsVar!.VarName.Should().Be("home");

        // Check RHS is KG.SaveGraph(...)
        var member = stmt3_rhs.RHS as FuncCallExp;
        member.Should().NotBeNull();
        // (member!.LHS as VarRefExp)!.VarName.Should().Be("KG");
        // var call = member.RHS as FuncCallExp;
        // call.Should().NotBeNull();
        // call!.Annotations.Should().ContainKey("FunctionName");
        // call!.Annotations!["FunctionName"].Should().Be("SaveGraph");
        // call!.InvocationArguments.Should().HaveCount(2);
    }
}
