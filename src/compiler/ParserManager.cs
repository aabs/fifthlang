using Antlr4.Runtime;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using Fifth;
using Fifth.LangProcessingPhases;

namespace compiler;

public static class FifthParserManager
{
    public static AstThing ApplyLanguageAnalysisPhases(AstThing ast)
    {
        ArgumentNullException.ThrowIfNull(ast);
        ast = new TreeLinkageVisitor().Visit(ast);
        ast = new BuiltinInjectorVisitor().Visit(ast);
        ast = new ClassCtorInserter().Visit(ast);
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        ast = new PropertyToFieldExpander().Visit(ast);
        ast = new OverloadGatheringVisitor().Visit(ast);
        ast = new OverloadTransformingVisitor().Visit(ast);
        // ast = new DestructuringVisitor().Visit(ast);  // Functionality moved to DestructuringPatternFlattenerVisitor
        ast = new DestructuringPatternFlattenerVisitor().Visit(ast);
        ast = new TreeLinkageVisitor().Visit(ast);
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        ast = new TypeAnnotationVisitor().Visit(ast);
        //ast = new DumpTreeVisitor(Console.Out).Visit(ast);
        return ast;
    }

    private static FifthParser GetParserForStream(ICharStream source)
    {
        var lexer = new FifthLexer(source);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowingErrorListener<int>());

        var parser = new FifthParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThrowingErrorListener<IToken>());
        return parser;
    }

    #region File Handling

    public static AstThing ParseFile(string sourceFile)
    {
        var parser = GetParserForFile(sourceFile);
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        return ast as AssemblyDef;
    }

    private static FifthParser GetParserForFile(string sourceFile)
    {
        var s = CharStreams.fromPath(sourceFile);
        return GetParserForStream(s);
    }

    #endregion File Handling

    #region Embedded Resource Handling

    public static AstThing ParseEmbeddedResource(Stream sourceStream)
    {
        var parser = GetParserForEmbeddedResource(sourceStream);
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        return ast as AssemblyDef;
    }

    private static FifthParser GetParserForEmbeddedResource(Stream sourceStream)
    {
        var s = CharStreams.fromStream(sourceStream);
        return GetParserForStream(s);
    }

    #endregion Embedded Resource Handling

    #region String handling

    public static AstThing ParseString(string source)
    {
        var parser = GetParserForString(source);
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        return ast as AssemblyDef;
    }

    private static FifthParser GetParserForString(string source)
    {
        var s = CharStreams.fromString(source);
        return GetParserForStream(s);
    }

    #endregion String handling
}
