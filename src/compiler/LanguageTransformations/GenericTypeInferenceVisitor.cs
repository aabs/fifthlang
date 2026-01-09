using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Visitor that infers generic type arguments from function call context.
/// Implements User Story 2 (T040-T044): Generic type inference.
/// Uses local type inference similar to C# (see research.md).
/// </summary>
public class GenericTypeInferenceVisitor : DefaultRecursiveDescentVisitor
{
    private readonly Dictionary<string, TypeInferenceContext> _inferenceContexts = new();

    /// <summary>
    /// Visit function call to infer type arguments if not explicitly provided (T042).
    /// </summary>
    public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        // If this is a call to a generic function without explicit type arguments,
        // attempt to infer the type arguments from the call site
        if (ctx.FunctionDef != null && ctx.FunctionDef.TypeParameters.Count > 0)
        {
            // Check if type arguments are explicitly provided
            if (ctx.TypeArguments != null && ctx.TypeArguments.Count > 0)
            {
                // Explicit arguments provided - skip inference.
                // Optionally validate count matches definition.
                return base.VisitFuncCallExp(ctx);
            }

            // For now, we create an inference context to track the inference process
            var context = new TypeInferenceContext
            {
                CallSite = ctx,
                InferredTypes = new Dictionary<string, FifthType>(),
                PendingConstraints = new List<TypeConstraint>(),
                Diagnostics = new List<string>()
            };

            // Attempt inference from argument types (T042)
            if (TryInferFromArguments(ctx, context))
            {
                // Inference succeeded - type arguments determined
                // Store for later use
                _inferenceContexts[GetCallSiteKey(ctx)] = context;
            }
            else
            {
                // Inference failed - report diagnostic (T044)
                ReportInferenceFailure(ctx, context);
            }
        }

        return base.VisitFuncCallExp(ctx);
    }

    /// <summary>
    /// Attempts to infer type arguments from function call arguments (T042).
    /// Implements local type inference similar to C#.
    /// </summary>
    private bool TryInferFromArguments(FuncCallExp call, TypeInferenceContext context)
    {
        if (call.FunctionDef == null)
            return false;

        var typeParams = call.FunctionDef.TypeParameters;
        var formalParams = call.FunctionDef.Params;
        var actualArgs = call.InvocationArguments;

        // Match formal parameters with actual arguments
        if (formalParams.Count != actualArgs.Count)
        {
            context.Diagnostics.Add("Argument count mismatch");
            return false;
        }

        // Infer from each argument
        for (int i = 0; i < formalParams.Count; i++)
        {
            var formalType = formalParams[i].TypeName;
            var actualArg = actualArgs[i];

            // If the formal parameter type is a type parameter, infer it
            if (IsTypeParameter(formalType, typeParams))
            {
                // Infer type from actual argument
                var inferredType = GetExpressionType(actualArg);
                if (inferredType != null)
                {
                    context.InferredTypes[formalType.Value] = inferredType;
                }
            }
            // If the formal type contains type parameters (e.g., List<T>), recurse
            else if (ContainsTypeParameters(formalType))
            {
                // TODO: Handle complex generic types
                // This would require structural matching
            }
        }

        // Check if all type parameters were inferred
        foreach (var typeParam in typeParams)
        {
            if (!context.InferredTypes.ContainsKey(typeParam.Name.Value))
            {
                context.Diagnostics.Add($"Could not infer type parameter '{typeParam.Name.Value}'");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts inference from assignment context (T043).
    /// Example: var x: Stack<int> = new Stack();
    /// </summary>
    private bool TryInferFromAssignment(TypeName targetType, TypeInferenceContext context)
    {
        // This would be called when we see an assignment or variable declaration
        // where the target type provides hints about the type arguments

        // TODO: Implement assignment context inference
        return false;
    }

    /// <summary>
    /// Reports inference failure diagnostic (T044).
    /// Reports GEN002 when type arguments cannot be inferred.
    /// </summary>
    private void ReportInferenceFailure(FuncCallExp call, TypeInferenceContext context)
    {
        var functionName = call.FunctionDef?.Name.Value ?? "unknown";
        var diagnostics = string.Join(", ", context.Diagnostics);

        Console.Error.WriteLine(
            $"GEN002: Cannot infer type arguments for '{functionName}'. " +
            $"Provide explicit type arguments. Details: {diagnostics} " +
            $"at {call.Location?.Filename ?? "unknown"}:{call.Location?.Line ?? 0}:{call.Location?.Column ?? 0}"
        );
    }

    /// <summary>
    /// Checks if a type name is a type parameter.
    /// </summary>
    private bool IsTypeParameter(TypeName typeName, List<TypeParameterDef> typeParams)
    {
        return typeParams.Any(tp => tp.Name.Value == typeName.Value);
    }

    /// <summary>
    /// Checks if a type contains type parameters (e.g., List<T>).
    /// </summary>
    private bool ContainsTypeParameters(TypeName typeName)
    {
        // For now, simple check
        // Full implementation would require type resolution
        return false;
    }

    /// <summary>
    /// Gets the type of an expression.
    /// </summary>
    private FifthType? GetExpressionType(Expression expr)
    {
        // Return the Type property if already resolved
        return expr.Type;
    }

    /// <summary>
    /// Creates a unique key for a call site.
    /// </summary>
    private string GetCallSiteKey(FuncCallExp call)
    {
        var location = call.Location;
        return $"{location?.Filename ?? "?"}:{location?.Line ?? 0}:{location?.Column ?? 0}";
    }
}

/// <summary>
/// Context for tracking type inference at a call site (T041).
/// </summary>
public class TypeInferenceContext
{
    /// <summary>
    /// Map from type parameter name to inferred type.
    /// </summary>
    public required Dictionary<string, FifthType> InferredTypes { get; set; }

    /// <summary>
    /// Constraints that must be satisfied by the inferred types.
    /// </summary>
    public required List<TypeConstraint> PendingConstraints { get; set; }

    /// <summary>
    /// Diagnostic messages from inference process.
    /// </summary>
    public required List<string> Diagnostics { get; set; }

    /// <summary>
    /// The call site where inference is happening.
    /// </summary>
    public required FuncCallExp CallSite { get; set; }
}
