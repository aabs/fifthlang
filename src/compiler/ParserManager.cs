using Antlr4.Runtime;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using compiler.Validation.GuardValidation;
using Fifth;
using Fifth.LangProcessingPhases;

namespace compiler;

public static class FifthParserManager
{
    private static bool DebugEnabled =>
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("1", StringComparison.Ordinal) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("true", StringComparison.OrdinalIgnoreCase) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("on", StringComparison.OrdinalIgnoreCase);

    public enum AnalysisPhase
    {
        None = 0,
        TreeLink = 1,
        Builtins = 2,
        ClassCtors = 3,
        SymbolTableInitial = 4,
        PropertyToField = 5,
        DestructurePatternFlatten = 6,
        OverloadGroup = 7,
        GuardValidation = 8,
        OverloadTransform = 9,
        DestructuringLowering = 10,
        TreeRelink = 11,
        GraphAssertionLowering = 12,
        SymbolTableFinal = 13,
        TypeAnnotation = 14,
        All = TypeAnnotation
    }

    public static AstThing ApplyLanguageAnalysisPhases(AstThing ast, List<compiler.Diagnostic>? diagnostics = null, AnalysisPhase upTo = AnalysisPhase.All)
    {
        ArgumentNullException.ThrowIfNull(ast);

        try
        {
            if (upTo >= AnalysisPhase.TreeLink)
                ast = new TreeLinkageVisitor().Visit(ast);
        }
        catch (System.Exception ex)
        {
            if (DebugEnabled)
            {
                Console.Error.WriteLine($"=== DEBUG: TreeLinkageVisitor failed with: {ex.Message} ===");
                Console.Error.WriteLine($"=== DEBUG: Stack trace: {ex.StackTrace} ===");
            }
            throw;
        }

        if (upTo >= AnalysisPhase.Builtins)
            ast = new BuiltinInjectorVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.ClassCtors)
            ast = new ClassCtorInserter().Visit(ast);

        if (upTo >= AnalysisPhase.SymbolTableInitial)
            ast = new SymbolTableBuilderVisitor().Visit(ast);

        try
        {
            if (upTo >= AnalysisPhase.PropertyToField)
                ast = new PropertyToFieldExpander().Visit(ast);
        }
        catch (System.Exception ex)
        {
            if (DebugEnabled)
            {
                Console.Error.WriteLine($"=== DEBUG: PropertyToFieldExpander failed with: {ex.Message} ===");
                Console.Error.WriteLine($"=== DEBUG: Stack trace: {ex.StackTrace} ===");
            }
            throw;
        }

        if (upTo >= AnalysisPhase.DestructurePatternFlatten)
            ast = new DestructuringPatternFlattenerVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.OverloadGroup)
            ast = new OverloadGatheringVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.GuardValidation)
        {
            var guardValidator = new GuardCompletenessValidator();
            ast = guardValidator.Visit(ast);
            if (diagnostics != null)
            {
                foreach (var diagnostic in guardValidator.Diagnostics)
                {
                    diagnostics.Add(diagnostic);
                }
            }
            else if (DebugEnabled)
            {
                foreach (var diagnostic in guardValidator.Diagnostics)
                {
                    Console.Error.WriteLine($"=== GUARD VALIDATION: {diagnostic.Level}: {diagnostic.Message} ===");
                }
            }
        }

        // Generate guard and subclause functions using collected constraints on grouped overloads
        // Debug: Check main method before OverloadTransformingVisitor
        if (ast is AssemblyDef asmBefore)
        {
            var mainMethod = asmBefore.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
        }

        // If diagnostics list was provided and contains errors, short-circuit to allow caller to handle failures
        if (diagnostics != null && diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
        {
            // Return the AST anyway; caller will inspect diagnostics for errors and act accordingly
        }

        if (upTo >= AnalysisPhase.OverloadTransform)
            ast = new OverloadTransformingVisitor().Visit(ast);

        // Debug: Check main method after OverloadTransformingVisitor
        if (ast is AssemblyDef asmAfter)
        {
            var mainMethod2 = asmAfter.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
        }

        // Now lower destructuring assignments
        if (upTo >= AnalysisPhase.DestructuringLowering)
            ast = new DestructuringVisitor().Visit(ast);  // Handle destructuring transformation

        if (upTo >= AnalysisPhase.TreeRelink)
            ast = new TreeLinkageVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.GraphAssertionLowering)
            ast = new GraphAssertionLoweringVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.SymbolTableFinal)
            ast = new SymbolTableBuilderVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.TypeAnnotation)
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
        return ast as AssemblyDef ?? throw new System.Exception("ParseFile did not produce an AssemblyDef AST");
    }

    public static (FifthParser parser, FifthParser.FifthContext tree) ParseFileToTree(string sourceFile)
    {
        var parser = GetParserForFile(sourceFile);
        var tree = parser.fifth();
        return (parser, tree);
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
        return ast as AssemblyDef ?? throw new System.Exception("ParseEmbeddedResource did not produce an AssemblyDef AST");
    }

    public static (FifthParser parser, FifthParser.FifthContext tree) ParseEmbeddedResourceToTree(Stream sourceStream)
    {
        var parser = GetParserForEmbeddedResource(sourceStream);
        var tree = parser.fifth();
        return (parser, tree);
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
        return ast as AssemblyDef ?? throw new System.Exception("ParseString did not produce an AssemblyDef AST");
    }

    public static (FifthParser parser, FifthParser.FifthContext tree) ParseStringToTree(string source)
    {
        var parser = GetParserForString(source);
        var tree = parser.fifth();
        return (parser, tree);
    }

    private static FifthParser GetParserForString(string source)
    {
        var s = CharStreams.fromString(source);
        return GetParserForStream(s);
    }

    #endregion String handling
}
