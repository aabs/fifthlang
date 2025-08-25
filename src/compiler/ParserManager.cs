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
        
        try 
        {
            ast = new TreeLinkageVisitor().Visit(ast);
        }
        catch (System.Exception ex)
        {
            Console.Error.WriteLine($"=== DEBUG: TreeLinkageVisitor failed with: {ex.Message} ===");
            Console.Error.WriteLine($"=== DEBUG: Stack trace: {ex.StackTrace} ===");
            throw;
        }
        
        ast = new BuiltinInjectorVisitor().Visit(ast);
        
        ast = new ClassCtorInserter().Visit(ast);
        
        ast = new SymbolTableBuilderVisitor().Visit(ast);
        
        try 
        {
            ast = new PropertyToFieldExpander().Visit(ast);
        }
        catch (System.Exception ex)
        {
            Console.Error.WriteLine($"=== DEBUG: PropertyToFieldExpander failed with: {ex.Message} ===");
            Console.Error.WriteLine($"=== DEBUG: Stack trace: {ex.StackTrace} ===");
            throw;
        }
        
    // Collect parameter constraints from destructured bindings BEFORE grouping overloads
    ast = new DestructuringPatternFlattenerVisitor().Visit(ast);

    // Gather overloads into grouped nodes now that constraints are present
    ast = new OverloadGatheringVisitor().Visit(ast);

    // Generate guard and subclause functions using collected constraints on grouped overloads
        // Debug: Check main method before OverloadTransformingVisitor
        if (ast is AssemblyDef asmBefore)
        {
            var mainMethod = asmBefore.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
        }

        ast = new OverloadTransformingVisitor().Visit(ast);

        // Debug: Check main method after OverloadTransformingVisitor
        if (ast is AssemblyDef asmAfter)
        {
            var mainMethod2 = asmAfter.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
        }

        // Now lower destructuring assignments
        ast = new DestructuringVisitor().Visit(ast);  // Handle destructuring transformation
        
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

    // Parse only: lex + parse without building the AST. Used by syntax-only tests.
    public static void ParseFileSyntaxOnly(string sourceFile)
    {
        var parser = GetParserForFile(sourceFile);
        parser.fifth();
        var next = parser.TokenStream.LA(1);
        if (next != Antlr4.Runtime.TokenConstants.EOF)
        {
            throw new System.Exception($"Unexpected trailing tokens after parse. Next token type: {next}");
        }
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
