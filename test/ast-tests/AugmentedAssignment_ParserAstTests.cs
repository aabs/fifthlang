using System.Linq;
using FluentAssertions;
using ast;
using compiler;
using test_infra;

namespace ast_tests;

public class AugmentedAssignment_ParserAstTests
{
    [Fact]
    public void PlusAssign_ShouldLowerInto_MergeCall()
    {
        var src = "main():int { g: graph = KG.CreateGraph(); home: graph = KG.CreateGraph(); home += g; return 0; }";
        var parseResult = ParseHarness.ParseString(src, new ParseOptions(FifthParserManager.AnalysisPhase.TypeAnnotation));
        parseResult.Diagnostics.Should().BeEmpty();

        var module = parseResult.Root!.Modules.Single();
        var func = module.Functions.OfType<FunctionDef>().Single(f => f.Name.Value == "main");

        // Inspect body statements: the augmented assignment should become a hoisted home.Merge(g) call
        // without keeping the original assignment statement.
        var stmts = func.Body!.Statements!;
        stmts.Should().NotBeNull();
        stmts.Should().HaveCountGreaterThan(3);
        stmts.OfType<AssignmentStatement>().Should().BeEmpty();

        var expStmt = stmts.OfType<ExpStatement>().FirstOrDefault();
        expStmt.Should().NotBeNull();

        // The lowering now hoists the merge call directly: home.Merge(g);
        var mergeCall = expStmt!.RHS as MemberAccessExp;
        mergeCall.Should().NotBeNull();

        var targetGraph = mergeCall!.LHS as VarRefExp;
        targetGraph.Should().NotBeNull();
        targetGraph!.VarName.Should().Be("home");

        var call = mergeCall.RHS as FuncCallExp;
        call.Should().NotBeNull();
        call!.Annotations.Should().ContainKey("FunctionName");
        call!.Annotations!["FunctionName"].Should().Be("Merge");
        call!.InvocationArguments.Should().HaveCount(1);

        var argument = call.InvocationArguments![0] as VarRefExp;
        argument.Should().NotBeNull();
        argument!.VarName.Should().Be("g");
    }
}
