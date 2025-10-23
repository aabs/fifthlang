using Antlr4.Runtime;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using compiler.Validation.GuardValidation;
using Fifth;
using Fifth.LangProcessingPhases;
using static Fifth.DebugHelpers;

namespace compiler;

// Debug visitor to detect MemberAccessExp and BinaryExp nodes
internal class DebugMemberAccessDetector : DefaultRecursiveDescentVisitor
{
    public override MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
    {
        Console.Error.WriteLine($"DEBUG: Found MemberAccessExp at {ctx.Location?.Line}:{ctx.Location?.Column}");
        Console.Error.WriteLine($"  LHS: {ctx.LHS?.GetType().Name}");
        Console.Error.WriteLine($"  RHS: {ctx.RHS?.GetType().Name}");
        return base.VisitMemberAccessExp(ctx);
    }
    
    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        if (ctx.Location?.Line == 3)  // Focus on line 3
        {
            Console.Error.WriteLine($"DEBUG: Found BinaryExp at {ctx.Location?.Line}:{ctx.Location?.Column}");
            Console.Error.WriteLine($"  LHS: {ctx.LHS?.GetType().Name}");
            Console.Error.WriteLine($"  RHS: {ctx.RHS?.GetType().Name}");
            Console.Error.WriteLine($"  Operator: {ctx.Operator}");
        }
        return base.VisitBinaryExp(ctx);
    }
    
    public override AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
    {
        if (ctx.Location?.Line == 3)
        {
            Console.Error.WriteLine($"DEBUG: Visiting AssignmentStatement at {ctx.Location?.Line}:{ctx.Location?.Column}");
            Console.Error.WriteLine($"  LValue: {ctx.LValue?.GetType().Name} at {ctx.LValue?.Location?.Line}:{ctx.LValue?.Location?.Column}");
            Console.Error.WriteLine($"  RValue: {ctx.RValue?.GetType().Name} at {ctx.RValue?.Location?.Line}:{ctx.RValue?.Location?.Column}");
        }
        var result = base.VisitAssignmentStatement(ctx);
        if (ctx.Location?.Line == 3)
        {
            Console.Error.WriteLine($"DEBUG: After visiting AssignmentStatement - result type: {result.GetType().Name}");
            Console.Error.WriteLine($"  Result LValue: {result.LValue?.GetType().Name} at {result.LValue?.Location?.Line}:{result.LValue?.Location?.Column}");
            Console.Error.WriteLine($"  Result RValue: {result.RValue?.GetType().Name} at {result.RValue?.Location?.Line}:{result.RValue?.Location?.Column}");
        }
        return result;
    }
    
    public override ExpStatement VisitExpStatement(ExpStatement ctx)
    {
        if (ctx.Location?.Line == 3)
        {
            Console.Error.WriteLine($"DEBUG: Found ExpStatement at {ctx.Location?.Line}:{ctx.Location?.Column}");
            Console.Error.WriteLine($"  RHS: {ctx.RHS?.GetType().Name}");
        }
        return base.VisitExpStatement(ctx);
    }
    
    public override VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
        if (ctx.Location?.Line == 3)
        {
            Console.Error.WriteLine($"DEBUG: Visiting VarRefExp '{ctx.VarName}' at {ctx.Location?.Line}:{ctx.Location?.Column}");
        }
        var result = base.VisitVarRefExp(ctx);
        if (ctx.Location?.Line == 3)
        {
            Console.Error.WriteLine($"DEBUG: After visiting VarRefExp - result type: {result.GetType().Name}");
        }
        return result;
    }
}

public static class FifthParserManager
{
    // DebugEnabled and DebugLog are provided by shared DebugHelpers (imported statically above)

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
        AugmentedAssignmentLowering = 11,
        TreeRelink = 12,
        GraphAssertionLowering = 13,
        TripleDiagnostics = 14,
        TripleExpansion = 15,
        GraphTripleOperatorLowering = 16,
        SymbolTableFinal = 17,
        VarRefResolver = 18,
        TypeAnnotation = 19,
        // All should run through the graph/triple operator lowering so downstream backends never
        // see raw '+'/'-' between graphs/triples.
        // IMPORTANT: Since GraphTripleOperatorLowering runs inside the TypeAnnotation phase block,
        // All must be >= TypeAnnotation to ensure that block executes and the lowering runs.
        All = TypeAnnotation
    }

    public static AstThing ApplyLanguageAnalysisPhases(AstThing ast, List<compiler.Diagnostic>? diagnostics = null, AnalysisPhase upTo = AnalysisPhase.All)
    {
        // Apply language analysis phases (no debug console output)

        ArgumentNullException.ThrowIfNull(ast);

        try
        {
            if (upTo >= AnalysisPhase.TreeLink)
            {
                Console.Error.WriteLine("DEBUG: Before TreeLink");
                new DebugMemberAccessDetector().Visit(ast);
                ast = new TreeLinkageVisitor().Visit(ast);
                Console.Error.WriteLine("DEBUG: After TreeLink");
                new DebugMemberAccessDetector().Visit(ast);
            }
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
            if (DebugHelpers.DebugEnabled)
            {
                DebugHelpers.DebugLog($"PropertyToFieldExpander failed with: {ex.Message}");
                DebugHelpers.DebugLog($"Stack trace: {ex.StackTrace}");
            }
            throw;
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

        if (upTo >= AnalysisPhase.TreeRelink)
            ast = new TreeLinkageVisitor().Visit(ast);

        if (upTo >= AnalysisPhase.GraphAssertionLowering)
            ast = new GraphAssertionLoweringVisitor().Visit(ast);

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
