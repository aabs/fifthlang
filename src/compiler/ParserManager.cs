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
        
        Console.Error.WriteLine("=== DEBUG: Starting transformation pipeline ===");
        
        ast = new TreeLinkageVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed TreeLinkageVisitor ===");
        
        ast = new BuiltinInjectorVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed BuiltinInjectorVisitor ===");
        
        ast = new ClassCtorInserter().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed ClassCtorInserter ===");
        
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed SymbolTableBuilderVisitor ===");
        
        ast = new PropertyToFieldExpander().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed PropertyToFieldExpander ===");
        
        ast = new OverloadGatheringVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed OverloadGatheringVisitor ===");
        
        ast = new OverloadTransformingVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed OverloadTransformingVisitor ===");
        
        ast = new DestructuringPatternFlattenerVisitor().Visit(ast);  // Handle constraint collection and lowering
        Console.Error.WriteLine("=== DEBUG: Completed DestructuringPatternFlattenerVisitor ===");
        
        ast = new DestructuringVisitor().Visit(ast);  // Handle destructuring transformation
        Console.Error.WriteLine("=== DEBUG: Completed DestructuringVisitor ===");
        
        ast = new TreeLinkageVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed second TreeLinkageVisitor ===");
        
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed second SymbolTableBuilderVisitor ===");
        
        ast = new TypeAnnotationVisitor().Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Completed TypeAnnotationVisitor ===");
        
        //ast = new DumpTreeVisitor(Console.Out).Visit(ast);
        Console.Error.WriteLine("=== DEBUG: Transformation pipeline complete ===");
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
