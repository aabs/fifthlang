using ast;
using ast_model.Symbols;
using compiler.LanguageTransformations;

namespace compiler.SemanticAnalysis;

/// <summary>
/// Resolves constructor calls in ObjectInitializerExp nodes by matching arguments to constructor signatures.
/// Implements overload resolution with ranking: exact match > convertible > widening.
/// Emits CTOR001 diagnostic when no matching constructor is found.
/// Emits CTOR002 diagnostic when multiple constructors match ambiguously.
/// Emits CTOR006 diagnostic when duplicate constructor signatures are detected.
/// </summary>
public class ConstructorResolver : DefaultRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic>? _diagnostics;
    private readonly HashSet<string> _seenClasses = new();

    public ConstructorResolver(List<compiler.Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override ObjectInitializerExp VisitObjectInitializerExp(ObjectInitializerExp ctx)
    {
        // Skip array and list types - they don't have user-defined constructors to resolve
        if (ctx.Type is ast_model.TypeSystem.FifthType.TArrayOf || ctx.Type is ast_model.TypeSystem.FifthType.TListOf)
        {
            return base.VisitObjectInitializerExp(ctx);
        }

        // Get the class name from the type being instantiated
        var className = ctx.Type.Name.Value;

        // Handle generic types: extract base name from "Box<int>" -> "Box"
        var lookupName = className;
        if (className.Contains('<'))
        {
            lookupName = className.Substring(0, className.IndexOf('<'));
        }

        // Look up the class definition from symbol table
        var classScope = ctx.NearestScopeAbove();
        var classSymbolEntry = classScope?.ResolveByName(lookupName);

        if (classSymbolEntry == null || classSymbolEntry.OriginatingAstThing is not ClassDef classDef)
        {
            // Class not found - this will be caught by type checking, so we skip it here
            return base.VisitObjectInitializerExp(ctx);
        }

        // Check for duplicate signatures (CTOR006) - only once per class
        if (!_seenClasses.Contains(className))
        {
            _seenClasses.Add(className);
            CheckForDuplicateSignatures(classDef);
        }

        // Find all constructors in the class (constructors are MethodDef members with FunctionDef.IsConstructor=true)
        var constructors = classDef.MemberDefs
            .OfType<MethodDef>()
            .Where(m => m.FunctionDef?.IsConstructor == true)
            .Select(m => m.FunctionDef)
            .ToList();

        if (constructors.Count == 0)
        {
            // No constructors defined - check if parameterless instantiation
            if (ctx.ConstructorArguments.Count == 0)
            {
                // OK - will use synthesized parameterless constructor
                return base.VisitObjectInitializerExp(ctx);
            }
            else
            {
                // Error: trying to call constructor with args when none exist
                EmitCTOR001(ctx, className, ctx.ConstructorArguments.Count);
                return base.VisitObjectInitializerExp(ctx);
            }
        }

        // Arity pre-filtering: find constructors with matching parameter count
        var argCount = ctx.ConstructorArguments.Count;
        var matchingConstructors = constructors
            .Where(c => c.Params.Count == argCount)
            .ToList();

        if (matchingConstructors.Count == 0)
        {
            // No matching constructor by arity
            EmitCTOR001(ctx, className, argCount);
            return base.VisitObjectInitializerExp(ctx);
        }

        if (matchingConstructors.Count == 1)
        {
            // Single match - resolve to this constructor
            var newCtx = ctx with { ResolvedConstructor = matchingConstructors[0] };
            return base.VisitObjectInitializerExp(newCtx);
        }

        // Multiple matches - perform type-based ranking
        var rankedConstructors = RankConstructorsByTypeCompatibility(ctx, matchingConstructors);

        if (rankedConstructors.Count == 0)
        {
            // No constructors passed type checking
            EmitCTOR001(ctx, className, argCount);
            return base.VisitObjectInitializerExp(ctx);
        }

        if (rankedConstructors.Count > 1)
        {
            // Ambiguous - multiple constructors with same rank
            EmitCTOR002(ctx, className, rankedConstructors);
            // Still select the first one for error recovery
        }

        var selectedConstructor = rankedConstructors[0];
        var resolvedCtx = ctx with { ResolvedConstructor = selectedConstructor };
        return base.VisitObjectInitializerExp(resolvedCtx);
    }

    /// <summary>
    /// Check for duplicate constructor signatures in the class definition.
    /// Emits CTOR006 diagnostic for each duplicate pair found.
    /// </summary>
    private void CheckForDuplicateSignatures(ClassDef classDef)
    {
        var constructors = classDef.MemberDefs
            .OfType<MethodDef>()
            .Where(m => m.FunctionDef?.IsConstructor == true)
            .Select(m => m.FunctionDef)
            .ToList();

        var signatureMap = new Dictionary<string, List<FunctionDef>>();

        foreach (var ctor in constructors)
        {
            var signature = GetSignatureKey(ctor);
            if (!signatureMap.ContainsKey(signature))
            {
                signatureMap[signature] = new List<FunctionDef>();
            }
            signatureMap[signature].Add(ctor);
        }

        // Report duplicates
        foreach (var (signature, ctors) in signatureMap)
        {
            if (ctors.Count > 1)
            {
                var className = classDef.Name.Value;
                var signatureStr = $"{className}({signature})";
                _diagnostics?.Add(ConstructorDiagnostics.DuplicateConstructorSignature(
                    className, signatureStr));
            }
        }
    }

    /// <summary>
    /// Ranks constructors by type compatibility with the provided arguments.
    /// Returns constructors with the best rank (lowest number = best).
    /// Ranking: 0 = exact match, 1 = convertible, 2 = widening
    /// NOTE: Currently only implements exact matching. Convertibility and widening
    /// checks are deferred pending full type system integration (TODO: Phase 5 enhancement).
    /// </summary>
    private List<FunctionDef> RankConstructorsByTypeCompatibility(
        ObjectInitializerExp ctx,
        List<FunctionDef> candidates)
    {
        // For now, we implement exact type matching only
        // Full type compatibility checking (convertible/widening) requires
        // type inference to be complete and is deferred as Phase 5 enhancement

        var exactMatches = new List<FunctionDef>();

        foreach (var ctor in candidates)
        {
            bool isExactMatch = true;
            for (int i = 0; i < ctor.Params.Count; i++)
            {
                var paramType = ctor.Params[i].Type.Name.Value;
                var argType = GetArgumentTypeName(ctx.ConstructorArguments[i]);

                if (paramType != argType && argType != "unknown")
                {
                    // Not an exact match
                    isExactMatch = false;
                    break;
                }
            }

            if (isExactMatch)
            {
                exactMatches.Add(ctor);
            }
        }

        // If we have exact matches, return only those
        if (exactMatches.Count > 0)
        {
            return exactMatches;
        }

        // TODO: Implement convertible and widening type checks
        // For now, if no exact matches, return all candidates
        // This allows arity-based resolution to work until type system is enhanced
        return candidates;
    }

    /// <summary>
    /// Gets a string representation of the argument type.
    /// Returns "unknown" if type cannot be determined (type inference incomplete).
    /// </summary>
    private string GetArgumentTypeName(Expression arg)
    {
        // Attempt to infer type from the argument expression
        // This is simplified - full type inference may not be complete at this stage
        return arg switch
        {
            Int32LiteralExp => "int",
            StringLiteralExp => "string",
            BooleanLiteralExp => "bool",
            Float4LiteralExp => "float",
            Float8LiteralExp => "double",
            VarRefExp varRef => varRef.Type?.Name.Value ?? "unknown",
            FuncCallExp funcCall => funcCall.Type?.Name.Value ?? "unknown",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Generates a unique signature key for constructor based on parameter types.
    /// Used for duplicate detection (CTOR006).
    /// </summary>
    private string GetSignatureKey(FunctionDef ctor)
    {
        return string.Join(",", ctor.Params.Select(p => p.Type.Name.Value));
    }

    private bool SignaturesMatch(List<ParamDef> params1, List<ParamDef> params2)
    {
        if (params1.Count != params2.Count) return false;

        for (int i = 0; i < params1.Count; i++)
        {
            if (params1[i].Type.Name.Value != params2[i].Type.Name.Value)
                return false;
        }

        return true;
    }

    private void EmitCTOR001(ObjectInitializerExp ctx, string className, int argCount)
    {
        var argTypes = argCount == 0 ? "" : string.Join(", ", ctx.ConstructorArguments.Select((_, i) => $"arg{i}"));
        _diagnostics?.Add(ConstructorDiagnostics.NoMatchingConstructor(
            className, argTypes));
    }

    private void EmitCTOR002(ObjectInitializerExp ctx, string className, List<FunctionDef> candidates)
    {
        var signatures = string.Join("; ", candidates.Select(c =>
            $"{className}({string.Join(", ", c.Params.Select(p => $"{p.Type.Name.Value}"))})"));
        _diagnostics?.Add(ConstructorDiagnostics.AmbiguousConstructor(
            className, signatures));
    }
}
