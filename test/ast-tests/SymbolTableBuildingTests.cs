using Antlr4.Runtime;
using ast;
using ast_model.Symbols;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using Fifth;
using FluentAssertions;

namespace ast_tests;

public class SymbolTableBuildingTests
{
    #region Helpers

    private static AssemblyDef ParseProgram(string programFileName)
    {
        var parser = GetParserFor(programFileName);
        // Act
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        var vlv = new TreeLinkageVisitor();
        vlv.Visit((AstThing)ast);
        var visitor = new SymbolTableBuilderVisitor();
        visitor.Visit((AstThing)ast);
        // Assert
        return (ast as AssemblyDef)!;
    }

    private static FifthParser GetParserFor(string sourceFile)
    {
        string content = ReadEmbeddedResource(sourceFile);
        var s = CharStreams.fromString(content);
        return GetParserFor(s);
    }

    private static FifthParser GetParserFor(ICharStream source)
    {
        var lexer = new FifthLexer(source);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowingErrorListener<int>());

        var parser = new FifthParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThrowingErrorListener<IToken>());
        return parser;
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        Type t = typeof(AstBuilderVisitorTests);
        Stream? streamObj = t.Assembly.GetManifestResourceStream(t.Namespace + ".CodeSamples." + resourceName);
        if (streamObj == null)
        {
            throw new FileNotFoundException("Resource not found", resourceName);
        }
        using Stream stream = streamObj;
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    #endregion Helpers

    [Fact]
    public void SymbolTableBuilding_RecursiveDestructuring1()
    {
        var asm = ParseProgram("recursive-destructuring.5th");

        asm.Modules[0].TryResolveByName("Person", out var steP).Should().BeTrue();
        asm.Modules[0].TryResolveByName("VitalStatistics", out var steVS).Should().BeTrue();
        steP.Symbol.Kind.Should().Be(SymbolKind.ClassDef);
        steVS.Symbol.Kind.Should().Be(SymbolKind.ClassDef);
        steP.OriginatingAstThing.TryResolve(new Symbol("Vitals", SymbolKind.PropertyDef), out var stevprop).Should().BeTrue();
    }

    [Fact]
    public void SymbolTableBuilding_SimpleProgram()
    {
        var asm = ParseProgram("statement-if.5th");

        asm.Modules[0].Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
        ((FunctionDef)asm.Modules[0].Functions[0]).Body.Statements[0].Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
        asm.Modules[0].SymbolTable.Should().NotBeNullOrEmpty();
        asm.Modules[0].SymbolTable.Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
        ((FunctionDef)asm.Modules[0].Functions[0]).TryResolve(new Symbol("y", SymbolKind.VarDeclStatement), out var ste).Should().BeTrue();

        ((FunctionDef)asm.Modules[0].Functions[0]).TryResolveByName("y", out var ste2).Should().BeTrue();
        ste2.Symbol.Kind.Should().Be(SymbolKind.VarDeclStatement);
        asm.Modules[0].TryResolveByName("main", out var ste3).Should().BeTrue();
        ste3.Symbol.Kind.Should().Be(SymbolKind.FunctionDef);
    }
}
