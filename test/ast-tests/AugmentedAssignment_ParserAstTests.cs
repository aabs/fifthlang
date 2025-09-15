using FluentAssertions;
using Antlr4.Runtime;
using ast;
using Fifth;
using compiler.LangProcessingPhases;

namespace ast_tests;

public class AugmentedAssignment_ParserAstTests
{
    [Test]
    public void PlusAssign_ShouldDesugarTo_KG_SaveGraph_Call()
    {
        var src = "func main():int { home += g; return 0; }";
        var s = CharStreams.fromString(src);
        var parser = new FifthParser(new CommonTokenStream(new FifthLexer(s)));
        var ctx = parser.function_declaration();
        var v = new AstBuilderVisitor();
        var func = v.VisitFunction_declaration(ctx) as FunctionDef;
        func.Should().NotBeNull();

        // Inspect body statements: expect an ExpStatement wrapping a MemberAccessExp KG.SaveGraph(...)
        var stmts = func!.Body!.Statements!;
        stmts.Should().NotBeNull();
        stmts.Should().HaveCountGreaterThan(1);

        var expStmt = stmts[0] as ExpStatement;
        expStmt.Should().NotBeNull();
        var member = expStmt!.RHS as MemberAccessExp;
        member.Should().NotBeNull();
        (member!.LHS as VarRefExp)!.VarName.Should().Be("KG");
        var call = member.RHS as FuncCallExp;
        call.Should().NotBeNull();
        call!.Annotations.Should().ContainKey("FunctionName");
        call!.Annotations!["FunctionName"].Should().Be("SaveGraph");
        call!.InvocationArguments.Should().HaveCount(2);
    }
}
