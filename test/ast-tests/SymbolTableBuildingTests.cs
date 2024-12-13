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
        Console.WriteLine(string.Join('\n', t.Assembly.GetManifestResourceNames()));
        using (Stream stream = t.Assembly.GetManifestResourceStream(t.Namespace + ".CodeSamples." + resourceName))
        {
            if (stream == null)
            {
                throw new FileNotFoundException("Resource not found", resourceName);
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    #endregion Helpers

    [Fact]
    public void SymbolTableBuilding_SimpleProgram()
    {
        // Arrange
        FifthParser parser = GetParserFor("statement-if.5th");
        // Act
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        var vlv = new TreeLinkageVisitor();
        vlv.Visit((AstThing)ast);
        var visitor = new SymbolTableBuilderVisitor();
        visitor.Visit((AstThing)ast);
        // Assert
        var asm = ast as AssemblyDef;

        asm.Modules[0].Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
        asm.Modules[0].Functions[0].Body.Statements[0].Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
        asm.Modules[0].SymbolTable.Should().NotBeNullOrEmpty();
        asm.Modules[0].SymbolTable.Resolve(new Symbol("main", SymbolKind.FunctionDef)).Should().NotBeNull();
    }
}
