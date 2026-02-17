using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Defunctionalises Higher-Order Function types in declarations to runtime closure interface types.
///
/// This is a backend-facing transformation: earlier analysis phases can keep using Fifth-native
/// function type syntax (e.g., "[int] -> int"), while code generation benefits from seeing
/// concrete runtime types (e.g., "Fifth.System.Runtime.IClosure<int, int>").
/// </summary>
public sealed class DefunctionalisationRewriter : DefaultAstRewriter
{
    public override RewriteResult VisitParamDef(ParamDef ctx)
    {
        var rr = base.VisitParamDef(ctx);
        var updated = (ParamDef)rr.Node;

        if (TryDefunctionalizeTypeName(updated.TypeName.Value, out var rewritten))
        {
            updated = updated with { TypeName = TypeName.From(rewritten) };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    public override RewriteResult VisitVariableDecl(VariableDecl ctx)
    {
        var rr = base.VisitVariableDecl(ctx);
        var updated = (VariableDecl)rr.Node;

        if (TryDefunctionalizeTypeName(updated.TypeName.Value, out var rewritten))
        {
            updated = updated with { TypeName = TypeName.From(rewritten) };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    public override RewriteResult VisitFieldDef(FieldDef ctx)
    {
        var rr = base.VisitFieldDef(ctx);
        var updated = (FieldDef)rr.Node;

        if (TryDefunctionalizeTypeName(updated.TypeName.Value, out var rewritten))
        {
            updated = updated with { TypeName = TypeName.From(rewritten) };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    public override RewriteResult VisitPropertyDef(PropertyDef ctx)
    {
        var rr = base.VisitPropertyDef(ctx);
        var updated = (PropertyDef)rr.Node;

        if (TryDefunctionalizeTypeName(updated.TypeName.Value, out var rewritten))
        {
            updated = updated with { TypeName = TypeName.From(rewritten) };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    public override RewriteResult VisitMethodDef(MethodDef ctx)
    {
        var rr = base.VisitMethodDef(ctx);
        var updated = (MethodDef)rr.Node;

        // MethodDef.TypeName reflects the member type surface; the return type itself is on FunctionDef.ReturnType.
        if (TryDefunctionalizeTypeName(updated.TypeName.Value, out var rewritten))
        {
            updated = updated with { TypeName = TypeName.From(rewritten) };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    public override RewriteResult VisitClassDef(ClassDef ctx)
    {
        var rr = base.VisitClassDef(ctx);
        var updated = (ClassDef)rr.Node;

        // BaseClasses are stored as strings; rewrite any function-type-shaped bases.
        if (updated.BaseClasses.Count > 0)
        {
            var rewrittenBases = updated.BaseClasses
                .Select(b => TryDefunctionalizeTypeName(b, out var r) ? r : b)
                .ToList();
            updated = updated with { BaseClasses = rewrittenBases };
        }

        return new RewriteResult(updated, rr.Prologue);
    }

    private static bool TryDefunctionalizeTypeName(string typeName, out string rewritten)
    {
        rewritten = string.Empty;

        if (!TryParseFunctionType(typeName, out var inputTypes, out var outputType))
        {
            return false;
        }

        var mappedInputs = inputTypes.Select(MapFifthTypeNameToCSharp).ToList();
        var mappedOutput = MapFifthTypeNameToCSharp(outputType);

        if (string.Equals(outputType.Trim(), "void", StringComparison.Ordinal))
        {
            if (mappedInputs.Count == 0)
            {
                rewritten = "Fifth.System.Runtime.IActionClosure";
                return true;
            }

            rewritten = $"Fifth.System.Runtime.IActionClosure<{string.Join(", ", mappedInputs)}>";
            return true;
        }

        if (mappedInputs.Count == 0)
        {
            rewritten = $"Fifth.System.Runtime.IClosure<{mappedOutput}>";
            return true;
        }

        rewritten = $"Fifth.System.Runtime.IClosure<{string.Join(", ", mappedInputs)}, {mappedOutput}>";
        return true;
    }

    private static string MapFifthTypeNameToCSharp(string fifthTypeName)
    {
        var s = fifthTypeName.Trim();

        // Nested function types: defunctionalise recursively.
        if (TryDefunctionalizeTypeName(s, out var rewritten))
        {
            return rewritten;
        }

        // Lists: [T] => List<T>
        if (s.StartsWith("[", StringComparison.Ordinal) && s.EndsWith("]", StringComparison.Ordinal))
        {
            var inner = s.Substring(1, s.Length - 2);
            var mappedInner = MapFifthTypeNameToCSharp(inner);
            return $"System.Collections.Generic.List<{mappedInner}>";
        }

        // Common primitives and builtin runtime types
        return s switch
        {
            "int" => "int",
            "Int32" => "int",
            "float" => "float",
            "Float" => "float",
            "double" => "double",
            "Double" => "double",
            "string" => "string",
            "String" => "string",
            "bool" => "bool",
            "Boolean" => "bool",
            "datetime" => "System.DateTimeOffset",
            "graph" => "VDS.RDF.IGraph",
            "triple" => "Fifth.System.Triple",
            "store" => "Fifth.System.Store",
            "Store" => "Fifth.System.Store",
            "void" => "void",
            "var" => "var",
            _ => s
        };
    }

    private static bool TryParseFunctionType(string input, out List<string> inputTypes, out string outputType)
    {
        inputTypes = [];
        outputType = string.Empty;

        var s = input.Trim();
        var arrowIndex = s.IndexOf("->", StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            return false;
        }

        var left = s.Substring(0, arrowIndex).Trim();
        var right = s.Substring(arrowIndex + 2).Trim();

        if (!left.StartsWith("[", StringComparison.Ordinal) || !left.EndsWith("]", StringComparison.Ordinal))
        {
            return false;
        }

        var args = left.Substring(1, left.Length - 2).Trim();
        outputType = right;

        if (string.IsNullOrWhiteSpace(args))
        {
            return true;
        }

        foreach (var part in SplitTopLevelCommaSeparated(args))
        {
            var t = part.Trim();
            if (!string.IsNullOrWhiteSpace(t))
            {
                inputTypes.Add(t);
            }
        }

        return true;
    }

    private static IEnumerable<string> SplitTopLevelCommaSeparated(string s)
    {
        var start = 0;
        var angle = 0;
        var square = 0;

        for (var i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            switch (ch)
            {
                case '<':
                    angle++;
                    break;
                case '>':
                    angle = Math.Max(0, angle - 1);
                    break;
                case '[':
                    square++;
                    break;
                case ']':
                    square = Math.Max(0, square - 1);
                    break;
                case ',':
                    if (angle == 0 && square == 0)
                    {
                        yield return s.Substring(start, i - start);
                        start = i + 1;
                    }
                    break;
            }
        }

        if (start <= s.Length)
        {
            yield return s.Substring(start);
        }
    }
}
