using ast;
using System.Reflection;

namespace code_generator.Emit;

/// <summary>
/// Resolves external .NET method calls by selecting the best matching overload based on argument types.
/// </summary>
public class ExternalMethodResolver
{
    /// <summary>
    /// Resolves the best matching .NET method overload for a given call
    /// </summary>
    public MethodInfo? ResolveExternalMethod(
        TypeInference typeInference, 
        Type extType, 
        string methodName, 
        IList<Expression> args, 
        Type? receiverType = null)
    {
        var methods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(mi => string.Equals(mi.Name, methodName, StringComparison.Ordinal))
            .ToList();

        var argCount = args.Count;
        var inferredTypes = new List<Type?>();
        for (int i = 0; i < argCount; i++)
        {
            var arg = args[i];
            inferredTypes.Add(arg != null ? typeInference.InferExpressionType(arg) : null);
        }

        // Filter by arity (exact match, extension method, or optional parameters)
        var candidates = methods
            .Select(mi => new { mi, ps = mi.GetParameters() })
            .Where(x =>
            {
                // Exact match on supplied args
                if (x.ps.Length == argCount) return true;
                // Extension method case: first parameter is receiver, one more param than supplied
                if (x.ps.Length == argCount + 1) return true;
                // Allow methods with optional trailing params
                if (x.ps.Length > argCount && x.ps.Skip(argCount).All(p => p.IsOptional || p.HasDefaultValue)) 
                    return true;
                return false;
            })
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        // Score candidates by type compatibility
        var scored = candidates.Select(x => new
        {
            x.mi,
            x.ps,
            score = ComputeCompatibilityScore(x.ps, inferredTypes, argCount, receiverType),
            isGeneric = x.mi.IsGenericMethodDefinition
        })
        .OrderByDescending(s => s.score)
        .ThenBy(s => s.ps.Length) // prefer fewer total params
        .ThenBy(s => s.isGeneric) // prefer non-generic over generic
        .ToList();

        // Debug output
        LogCandidates(extType, methodName, argCount, receiverType != null, scored);

        // Handle zero-argument calls
        if (argCount == 0 && scored.Count > 0)
        {
            return scored[0].mi;
        }

        var allInferredNull = inferredTypes.All(t => t == null);

        if (scored.Count == 0)
        {
            return null;
        }

        // Reject if best score is too low
        if (scored[0].score <= 0 && !allInferredNull)
        {
            return null;
        }

        // Fallback for all-null inference
        if (allInferredNull && scored.Count > 0)
        {
            return scored[0].mi;
        }

        var best = scored[0];

        // Verify argument compatibility
        var bestOffset = (receiverType != null && best.ps.Length == argCount + 1) ? 1 : 0;
        for (int i = 0; i < argCount; i++)
        {
            var paramIndex = i + bestOffset;
            if (paramIndex < 0 || paramIndex >= best.ps.Length) return null;
            if (CompatibilityScore(inferredTypes[i], best.ps[paramIndex].ParameterType) <= 0)
            {
                return null;
            }
        }

        // Verify receiver compatibility for extension methods
        if (receiverType != null && best.ps.Length == argCount + 1)
        {
            if (CompatibilityScore(receiverType, best.ps[0].ParameterType) <= 0)
            {
                return null;
            }
        }

        return best.mi;
    }

    /// <summary>
    /// Extracts method name from FuncCallExp annotations or FunctionDef
    /// </summary>
    public static string ExtractExternalMethodName(FuncCallExp funcCall)
    {
        if (funcCall.Annotations != null)
        {
            if (funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && 
                extNameObj is string extName && !string.IsNullOrWhiteSpace(extName))
            {
                return extName;
            }
            if (funcCall.Annotations.TryGetValue("FunctionName", out var fnObj) && 
                fnObj is string fn && !string.IsNullOrWhiteSpace(fn))
            {
                return fn;
            }
        }
        var funcDefName = funcCall.FunctionDef?.Name.Value;
        if (!string.IsNullOrWhiteSpace(funcDefName))
        {
            return funcDefName!;
        }
        return "Method";
    }

    private int ComputeCompatibilityScore(
        ParameterInfo[] parameters, 
        List<Type?> inferredTypes, 
        int argCount, 
        Type? receiverType)
    {
        var isExtension = parameters.Length == argCount + 1;
        var paramOffset = isExtension ? 1 : 0;

        var score = Enumerable.Range(0, Math.Min(argCount, parameters.Length - paramOffset))
            .Sum(i =>
            {
                var paramIndex = i + paramOffset;
                if (paramIndex >= 0 && paramIndex < parameters.Length)
                {
                    return CompatibilityScore(inferredTypes[i], parameters[paramIndex].ParameterType);
                }
                return 0;
            });

        // Add receiver compatibility score for extension methods
        if (receiverType != null && isExtension)
        {
            score += CompatibilityScore(receiverType, parameters[0].ParameterType);
        }

        return score;
    }

    private static int CompatibilityScore(Type? argType, Type paramType)
    {
        if (argType == null) return 0;
        if (paramType == argType) return 100;
        if (paramType.IsAssignableFrom(argType)) return 50;
        if (TypeInference.IsImplicitNumericWidening(argType, paramType)) return 10;
        return 0;
    }

    private void LogCandidates<T>(Type extType, string methodName, int argCount, bool receiverPresent, List<T> scored)
    {
        try
        {
            Console.WriteLine($"TRACE: ResolveExternalMethod candidates for {extType.FullName}.{methodName} " +
                            $"(argCount={argCount}, receiverPresent={receiverPresent}):");
            foreach (dynamic sc in scored)
            {
                var pdesc = string.Join(",", 
                    ((ParameterInfo[])sc.ps).Select(p => p.ParameterType.FullName + "(" + p.Name + ")"));
                Console.WriteLine($"  candidate: {sc.mi.DeclaringType?.FullName}.{sc.mi.Name} " +
                                $"params=[{pdesc}] score={sc.score}");
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }
}
