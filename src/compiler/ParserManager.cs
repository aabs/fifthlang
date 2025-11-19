using Antlr4.Runtime;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using compiler.Validation.GuardValidation;
using Fifth;
using Fifth.LangProcessingPhases;
using static Fifth.DebugHelpers;

namespace compiler;

public static class FifthParserManager
{
    // DebugEnabled and DebugLog are provided by shared DebugHelpers (imported statically above)

    public enum AnalysisPhase
    {
        None = 0,
        TreeLink = 1,
        Builtins = 2,
        ClassCtors = 3,
        ConstructorValidation = 4,
        SymbolTableInitial = 5,
        PropertyToField = 6,
        DestructurePatternFlatten = 7,
        OverloadGroup = 8,
        GuardValidation = 9,
        OverloadTransform = 10,
        DestructuringLowering = 11,
        UnaryOperatorLowering = 12,
        SparqlLiteralLowering = 13,
        TriGLiteralLowering = 14,
        AugmentedAssignmentLowering = 15,
        TreeRelink = 16,
        TripleDiagnostics = 17,
        TripleExpansion = 18,
        GraphTripleOperatorLowering = 19,
        SymbolTableFinal = 20,
        VarRefResolver = 21,
        TypeAnnotation = 22,
        QueryApplicationTypeCheck = 23,
        QueryApplicationLowering = 24,
        // All should run through the graph/triple operator lowering so downstream backends never
        // see raw '+'/'-' between graphs/triples.
        // IMPORTANT: Since GraphTripleOperatorLowering runs inside the TypeAnnotation phase block,
        // All must be >= TypeAnnotation to ensure that block executes and the lowering runs.
        All = QueryApplicationLowering  // Update to include query application
    }

    public static AstThing ApplyLanguageAnalysisPhases(AstThing ast, List<compiler.Diagnostic>? diagnostics = null, AnalysisPhase upTo = AnalysisPhase.All)
    {
        // Apply language analysis phases (no debug console output)

        ArgumentNullException.ThrowIfNull(ast);

        try
        {
            if (upTo >= AnalysisPhase.TreeLink)
                ast = new TreeLinkageVisitor().Visit(ast);
        }
        catch (System.Exception ex)
        {
            if (DebugHelpers.DebugEnabled)
            {
                DebugHelpers.DebugLog($"TreeLinkageVisitor failed with: {ex.Message}");
                DebugHelpers.DebugLog($"Stack trace: {ex.StackTrace}");
            }
            throw;
        }

        if (upTo >= AnalysisPhase.Builtins)
            ast = new BuiltinInjectorVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.ClassCtors)
            ast = new ClassCtorInserter(diagnostics).Visit(ast);

        if (upTo >= AnalysisPhase.ConstructorValidation)
            ast = new SemanticAnalysis.ConstructorValidator(diagnostics).Visit(ast);

        if (upTo >= AnalysisPhase.SymbolTableInitial)
            ast = new SymbolTableBuilderVisitor().Visit(ast);

        // Register type parameters in scope after symbol table building (T030)
        if (upTo >= AnalysisPhase.SymbolTableInitial)
            ast = new TypeParameterResolutionVisitor().Visit(ast);

        // Infer generic type arguments after type parameter resolution (T045)
        if (upTo >= AnalysisPhase.SymbolTableInitial)
            ast = new GenericTypeInferenceVisitor().Visit(ast);

        try
        {
            if (upTo >= AnalysisPhase.PropertyToField)
                ast = new PropertyToFieldExpander().Visit(ast);
        }
        catch (System.Exception ex)
        {
            if (DebugHelpers.DebugEnabled)
            {
                DebugHelpers.DebugLog($"PropertyToFieldExpander failed with: {ex.Message}");
                DebugHelpers.DebugLog($"Stack trace: {ex.StackTrace}");
            }
            throw;
        }

        if (upTo >= AnalysisPhase.DestructurePatternFlatten)
        {
            ast = new DestructuringVisitor().Visit(ast);
            ast = new DestructuringConstraintPropagator().Visit(ast);
        }

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
            else if (DebugHelpers.DebugEnabled)
            {
                foreach (var diagnostic in guardValidator.Diagnostics)
                {
                    DebugHelpers.DebugLog($"=== GUARD VALIDATION: {diagnostic.Level}: {diagnostic.Message} ===");
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
            // Return null so caller can observe the diagnostics and fail the build
            return null;
        }

        if (upTo >= AnalysisPhase.OverloadTransform)
            ast = new OverloadTransformingVisitor().Visit(ast);

        // Debug: Check main method after OverloadTransformingVisitor
        if (ast is AssemblyDef asmAfter)
        {
            var mainMethod2 = asmAfter.Modules.SelectMany(m => m.Functions).OfType<FunctionDef>().FirstOrDefault(f => f.Name.Value == "main");
        }

        // Resolve property references in destructuring (still needed for property resolution)
        if (upTo >= AnalysisPhase.DestructuringLowering)
            ast = new DestructuringVisitor().Visit(ast);

        // Now lower destructuring to variable declarations
        if (upTo >= AnalysisPhase.DestructuringLowering)
        {
            var rewriter = new DestructuringLoweringRewriter();
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
        }

        // Lower unary operators (++, --, -, +) to binary expressions
        if (upTo >= AnalysisPhase.UnaryOperatorLowering)
        {
            var rewriter = new UnaryOperatorLoweringRewriter();
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
        }

        // Lower SPARQL literal expressions to Query.Parse() calls
        if (upTo >= AnalysisPhase.SparqlLiteralLowering)
        {
            var rewriter = new SparqlLiteralLoweringRewriter();
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
        }

        // Lower TriG literal expressions to Store.LoadFromTriG() calls
        if (upTo >= AnalysisPhase.TriGLiteralLowering)
        {
            var rewriter = new TriGLiteralLoweringRewriter();
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
        }

        if (upTo >= AnalysisPhase.TreeRelink)
            ast = new TreeLinkageVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.TripleDiagnostics)
        {
            var tripleDiagVisitor = new TripleDiagnosticsVisitor(diagnostics);
            ast = tripleDiagVisitor.Visit(ast);
        }

        if (upTo >= AnalysisPhase.TripleExpansion)
        {
            ast = new TripleExpansionVisitor(diagnostics).Visit(ast);
        }

        // NOTE: GraphTripleOperatorLowering moved to after TypeAnnotation so that
        // VarRefExp nodes have proper types (e.g., 'graph') and lowering can make
        // reliable decisions (graph+graph, graph+triple, etc.).

        if (upTo >= AnalysisPhase.SymbolTableFinal)
            ast = new SymbolTableBuilderVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.VarRefResolver)
            ast = new VarRefResolverVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.TypeAnnotation)
        {
            var typeAnnotationVisitor = new TypeAnnotationVisitor();
            ast = typeAnnotationVisitor.Visit(ast);

            // Rebuild symbol table after type annotation to ensure all references point to
            // the updated AST nodes with proper types and CollectionType information.
            // This fixes the stale reference issue where the symbol table contains old nodes
            // from before type annotation transformed the immutable AST.
            ast = new SymbolTableBuilderVisitor().Visit(ast);

            // Lower augmented assignments (+= and -=) AFTER type annotation so we can use type information
            if (upTo >= AnalysisPhase.AugmentedAssignmentLowering)
                ast = new AugmentedAssignmentLoweringRewriter().Visit(ast);

            // Collect type checking errors (only Error severity, not Info)
            if (diagnostics != null)
            {
                foreach (var error in typeAnnotationVisitor.Errors.Where(e => e.Severity == TypeCheckingSeverity.Error))
                {
                    var diagnostic = new compiler.Diagnostic(
                        compiler.DiagnosticLevel.Error,
                        $"{error.Message} at {error.Filename}:{error.Line}:{error.Column}",
                        error.Filename,
                        "TYPE_ERROR");
                    diagnostics.Add(diagnostic);
                }

                if (diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
                {
                    return null;
                }
            }

            // Now run GraphTripleOperatorLowering with full type info available.
            if (upTo >= AnalysisPhase.GraphTripleOperatorLowering)
            {
                ast = (AstThing)new TripleGraphAdditionLoweringRewriter().Rewrite(ast).Node;
                // Re-link after rewriting, then rebuild symbol table and re-run var ref resolver + type annotation
                ast = new TreeLinkageVisitor().Visit(ast);
                ast = new SymbolTableBuilderVisitor().Visit(ast);
                ast = new VarRefResolverVisitor().Visit(ast);
                var typeAnnotationVisitor2 = new TypeAnnotationVisitor();
                ast = typeAnnotationVisitor2.Visit(ast);
                ast = new SymbolTableBuilderVisitor().Visit(ast);

                if (diagnostics != null)
                {
                    foreach (var error in typeAnnotationVisitor2.Errors.Where(e => e.Severity == TypeCheckingSeverity.Error))
                    {
                        var diagnostic = new compiler.Diagnostic(
                            compiler.DiagnosticLevel.Error,
                            $"{error.Message} at {error.Filename}:{error.Line}:{error.Column}",
                            error.Filename,
                            "TYPE_ERROR");
                        diagnostics.Add(diagnostic);
                    }

                    if (diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
                    {
                        return null;
                    }
                }
            }
        }

        // Validate external qualified calls now that types have been annotated
        if (diagnostics != null)
        {
            ast = new compiler.LanguageTransformations.ExternalCallValidationVisitor(diagnostics).Visit(ast);
            if (diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
            {
                // Early exit - return null to indicate transform failure
                return null;
            }
            
            // Validate try/catch/finally constructs
            ast = new compiler.LanguageTransformations.TryCatchFinallyValidationVisitor(diagnostics).Visit(ast);
            if (diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
            {
                // Early exit - return null to indicate transform failure
                return null;
            }
        }

        // Type check query application expressions (query <- store) AFTER type annotation
        if (upTo >= AnalysisPhase.QueryApplicationTypeCheck)
        {
            var rewriter = new QueryApplicationTypeCheckRewriter(diagnostics);
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
            
            if (diagnostics != null && diagnostics.Any(d => d.Level == compiler.DiagnosticLevel.Error))
            {
                return null;
            }
        }

        // Lower query application expressions to QueryApplicationExecutor.Execute() calls
        if (upTo >= AnalysisPhase.QueryApplicationLowering)
        {
            var rewriter = new QueryApplicationLoweringRewriter();
            var result = rewriter.Rewrite(ast);
            ast = result.Node;
        }

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
