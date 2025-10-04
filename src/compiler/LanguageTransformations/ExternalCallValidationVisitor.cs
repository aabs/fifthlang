using System.Reflection;
using ast;
using ast_model.TypeSystem;
using static Fifth.DebugHelpers;
using compiler;
using VDS.RDF;

namespace compiler.LanguageTransformations;

public class ExternalCallValidationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;

    public ExternalCallValidationVisitor(List<Diagnostic>? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    private static int CompatibilityScore(Type? argType, Type paramType)
    {
        if (argType == null) return 0;
        if (paramType == argType) return 100;
        if (paramType.IsAssignableFrom(argType)) return 50;
        // Numeric widening
        if (argType == typeof(int) && (paramType == typeof(long) || paramType == typeof(float) || paramType == typeof(double) || paramType == typeof(decimal))) return 10;
        if (argType == typeof(float) && paramType == typeof(double)) return 10;
        if (argType == typeof(long) && (paramType == typeof(float) || paramType == typeof(double) || paramType == typeof(decimal))) return 10;
        return 0;
    }

    // Map Fifth type annotation to CLR System.Type when available
    private static Type? MapFifthTypeToClr(FifthType? fifthType)
    {
        if (fifthType == null) return null;

        // If this is a dotnet-backed FifthType (TDotnetType), try to extract its TheType property
        try
        {
            if (fifthType is FifthType.TDotnetType dt)
            {
                return dt.TheType;
            }
        }
        catch { }

        // Otherwise, inspect the Name token if available
        try
        {
            string name = string.Empty;
            try
            {
                if (fifthType.Name != null) name = fifthType.Name.Value ?? fifthType.Name.ToString() ?? string.Empty;
            }
            catch { }
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (string.Equals(name, "graph", StringComparison.OrdinalIgnoreCase)) return typeof(IGraph);
                if (string.Equals(name, "string", StringComparison.OrdinalIgnoreCase)) return typeof(string);
                if (string.Equals(name, "int", StringComparison.OrdinalIgnoreCase)) return typeof(int);
                if (string.Equals(name, "float", StringComparison.OrdinalIgnoreCase)) return typeof(float);
                if (string.Equals(name, "double", StringComparison.OrdinalIgnoreCase)) return typeof(double);
                if (string.Equals(name, "bool", StringComparison.OrdinalIgnoreCase)) return typeof(bool);
            }
        }
        catch { }

        return null;
    }

    public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        var result = base.VisitFuncCallExp(ctx);
        if (result == null) return ctx;

        // If this is an external qualified call (TreeLinkageVisitor placed ExternalType annotation), validate it
        if (result.Annotations != null && result.Annotations.TryGetValue("ExternalType", out var extObj) && extObj is Type extType)
        {
            var methodName = string.Empty;
            if (result.Annotations.TryGetValue("ExternalMethodName", out var mn) && mn is string mns) methodName = mns;
            else if (result.Annotations.TryGetValue("FunctionName", out var fn) && fn is string fns) methodName = fns;
            if (string.IsNullOrWhiteSpace(methodName)) return result;

            var invocationArgs = result.InvocationArguments ?? new List<ast.Expression>();
            var argCount = invocationArgs.Count;
            // Infer CLR argument types from AST type annotations
            var inferred = new List<Type?>();
            for (int i = 0; i < argCount; i++)
            {
                var a = invocationArgs[i];
                Type? mapped = null;
                try
                {
                    var ft = a.Type; // FifthType assigned by TypeAnnotationVisitor
                    mapped = MapFifthTypeToClr(ft);
                }
                catch { mapped = null; }
                inferred.Add(mapped);
            }

            // Find candidate methods on the external type
            var methods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(mi => string.Equals(mi.Name, methodName, StringComparison.Ordinal))
                .ToList();

            var candidates = methods
                .Select(mi => new { mi, ps = mi.GetParameters() })
                .Where(x =>
                {
                    if (x.ps.Length == argCount) return true;
                    if (x.ps.Length == argCount + 1) return true; // extension receiver case
                    if (x.ps.Length > argCount && x.ps.Skip(argCount).All(p => p.IsOptional || p.HasDefaultValue)) return true;
                    return false;
                })
                .ToList();

            // If no arity-compatible candidates found, report diagnostic
            if (candidates.Count == 0)
            {
                _diagnostics?.Add(new Diagnostic(DiagnosticLevel.Error, $"Unresolved external call: {extType.FullName}.{methodName} with {argCount} argument(s)"));
                return result;
            }

            var scored = candidates.Select(x => new
            {
                x.mi,
                x.ps,
                score = Enumerable.Range(0, Math.Min(argCount, x.ps.Length - (x.ps.Length == argCount + 1 ? 1 : 0)))
                    .Sum(i =>
                    {
                        var offset = x.ps.Length == argCount + 1 ? 1 : 0;
                        var paramIndex = i + offset;
                        if (paramIndex >= 0 && paramIndex < x.ps.Length)
                        {
                            return CompatibilityScore(inferred[i], x.ps[paramIndex].ParameterType);
                        }
                        return 0;
                    })
                + ((x.ps.Length == argCount + 1 && inferred.Count > 0) ? CompatibilityScore(inferred[0], x.ps[0].ParameterType) : 0),
                isGeneric = x.mi.IsGenericMethodDefinition
            })
            .OrderByDescending(s => s.score)
            .ThenBy(s => s.ps.Length)
            .ThenBy(s => s.isGeneric)
            .ToList();

            var allInferredNull = inferred.All(t => t == null);
            if (scored.Count == 0)
            {
                _diagnostics?.Add(new Diagnostic(DiagnosticLevel.Error, $"Unresolved external call: {extType.FullName}.{methodName} (no compatible overload found)"));
                return result;
            }

            if (scored[0].score <= 0 && !allInferredNull)
            {
                // Arguments are present and incompatible with best candidate
                _diagnostics?.Add(new Diagnostic(DiagnosticLevel.Error, $"Argument types do not match any overload for {extType.FullName}.{methodName}. Supplied argument types: {string.Join(',', inferred.Select(t => t?.Name ?? "unknown"))}"));
                return result;
            }

            // No errors; accept best candidate silently
        }

        return result;
    }
}