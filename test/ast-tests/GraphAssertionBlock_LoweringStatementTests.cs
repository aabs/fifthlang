using ast;
using ast_model;
using FluentAssertions;
using compiler.LanguageTransformations;

namespace ast_tests;

public class GraphAssertionBlock_LoweringStatementTests : VisitorTestsBase
{
    private static ModuleDef CreateModule(string name)
    {
        return new ModuleDef
        {
            Annotations = [],
            OriginalModuleName = name,
            NamespaceDecl = NamespaceName.From("test.ns"),
            Imports = [],
            Classes = [],
            Functions = [],
            Location = null,
            Parent = null,
            Type = new ast_model.TypeSystem.FifthType.TType { Name = ast_model.TypeSystem.TypeName.From("module") },
            Visibility = Visibility.Public,
        };
    }

    [Test]
    public void StatementForm_WithoutStores_ShouldThrowCompilationException()
    {
        var module = CreateModule("M");
        var inner = new BlockStatement { Statements = [] };
        var exp = new GraphAssertionBlockExp { Content = inner };
        var stmt = new GraphAssertionBlockStatement { Content = exp };
        // wire parents so visitor can find enclosing module
        exp.Parent = stmt;
        stmt.Parent = module;

        var visitor = new GraphAssertionLoweringVisitor();

        var act = () => visitor.VisitGraphAssertionBlockStatement(stmt);

        act.Should().Throw<CompilationException>()
            .WithMessage("*requires an explicit store declaration*");
    }

    [Test]
    public void StatementForm_WithDefaultStore_ShouldAnnotateResolvedStore()
    {
        var module = CreateModule("M");
        module.Annotations["GraphStores"] = new Dictionary<string, string>
        {
            { "default", "http://example.org/store" }
        };
        module.Annotations["DefaultGraphStore"] = "default";

        var inner = new BlockStatement { Statements = [] };
        var exp = new GraphAssertionBlockExp { Content = inner };
        var stmt = new GraphAssertionBlockStatement { Content = exp };
        exp.Parent = stmt;
        stmt.Parent = module;

        var visitor = new GraphAssertionLoweringVisitor();

        var result = visitor.VisitGraphAssertionBlockStatement(stmt);

        result.Should().NotBeNull();
        result.Annotations.Should().ContainKey("ResolvedStoreName");
        result.Annotations.Should().ContainKey("ResolvedStoreUri");
        result.Annotations["ResolvedStoreName"].Should().Be("default");
        result.Annotations["ResolvedStoreUri"].Should().Be("http://example.org/store");
    }
}
