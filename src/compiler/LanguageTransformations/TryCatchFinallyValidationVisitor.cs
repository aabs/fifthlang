using ast;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Validates try/catch/finally constructs for semantic correctness:
/// - Catch types must derive from System.Exception
/// - Filter expressions must be boolean-convertible
/// - Unreachable catch clauses are errors
/// - Throw expression operands must be Exception-compatible
/// </summary>
public sealed class TryCatchFinallyValidationVisitor : NullSafeRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;

    public TryCatchFinallyValidationVisitor(List<Diagnostic>? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public override TryStatement VisitTryStatement(TryStatement ctx)
    {
        var result = base.VisitTryStatement(ctx);
        
        // Validate catch clauses
        ValidateCatchClauses(result.CatchClauses);
        
        return result;
    }

    private void ValidateCatchClauses(List<CatchClause> catchClauses)
    {
        if (catchClauses == null || catchClauses.Count == 0)
        {
            return;
        }

        // Track which types we've seen to detect unreachable catches
        var seenTypes = new List<(FifthType? Type, bool IsCatchAll)>();

        for (int i = 0; i < catchClauses.Count; i++)
        {
            var catchClause = catchClauses[i];
            
            // T023: Validate catch type must derive from System.Exception
            if (catchClause.ExceptionType != null)
            {
                if (!IsExceptionType(catchClause.ExceptionType))
                {
                    _diagnostics?.Add(new Diagnostic(
                        DiagnosticLevel.Error,
                        $"Catch type must derive from System.Exception.",
                        null,
                        Code: "TRY001"));
                }
            }

            // T024: Validate filter expression is boolean-convertible
            if (catchClause.Filter != null)
            {
                if (!IsBooleanConvertible(catchClause.Filter))
                {
                    _diagnostics?.Add(new Diagnostic(
                        DiagnosticLevel.Error,
                        $"Catch filter expression must be boolean-convertible.",
                        null,
                        Code: "TRY002"));
                }
            }

            // T025: Detect unreachable catch clauses
            bool isCatchAll = catchClause.ExceptionType == null;
            
            // Check if this catch is unreachable due to earlier catches
            foreach (var (seenType, seenIsCatchAll) in seenTypes)
            {
                if (seenIsCatchAll)
                {
                    // Earlier catch-all makes this one unreachable
                    _diagnostics?.Add(new Diagnostic(
                        DiagnosticLevel.Error,
                        $"Unreachable catch clause detected. Earlier catch-all clause catches all exceptions.",
                        null,
                        Code: "TRY003"));
                    break;
                }
                else if (seenType != null && catchClause.ExceptionType != null)
                {
                    // Check if earlier type is broader or equal
                    if (IsTypeEqualOrBroader(seenType, catchClause.ExceptionType))
                    {
                        _diagnostics?.Add(new Diagnostic(
                            DiagnosticLevel.Error,
                            $"Unreachable catch clause detected. Earlier catch clause for {GetTypeName(seenType)} catches this exception type.",
                            null,
                            Code: "TRY003"));
                        break;
                    }
                }
            }

            seenTypes.Add((catchClause.ExceptionType, isCatchAll));
        }
    }

    public override ThrowStatement VisitThrowStatement(ThrowStatement ctx)
    {
        var result = base.VisitThrowStatement(ctx);
        
        // T026: Validate throw statement operand type
        if (result.Exception != null && result.Exception.Type != null)
        {
            if (!IsExceptionType(result.Exception.Type))
            {
                _diagnostics?.Add(new Diagnostic(
                    DiagnosticLevel.Error,
                    $"Throw statement operand must be an exception type (derive from System.Exception).",
                    null,
                    Code: "TRY004"));
            }
        }
        
        return result;
    }

    public override ThrowExp VisitThrowExp(ThrowExp ctx)
    {
        var result = base.VisitThrowExp(ctx);
        
        // T026: Validate throw expression operand type
        if (result.Exception != null && result.Exception.Type != null)
        {
            if (!IsExceptionType(result.Exception.Type))
            {
                _diagnostics?.Add(new Diagnostic(
                    DiagnosticLevel.Error,
                    $"Throw expression operand must be an exception type (derive from System.Exception).",
                    null,
                    Code: "TRY004"));
            }
        }
        
        return result;
    }

    /// <summary>
    /// Check if a type is or derives from System.Exception
    /// </summary>
    private static bool IsExceptionType(FifthType type)
    {
        // For now, we do a simple name-based check
        // In a full implementation, this would check the type hierarchy
        
        if (type is FifthType.TDotnetType dotnetType)
        {
            return typeof(System.Exception).IsAssignableFrom(dotnetType.TheType);
        }
        
        // Check by name for common exception types
        var typeName = GetTypeName(type);
        if (string.IsNullOrEmpty(typeName))
        {
            return false;
        }

        // Accept System.Exception and types containing "Exception" in the name
        // This is a simplified check - proper implementation would use type inference
        return typeName.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
               typeName.Equals("System.Exception", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if an expression is boolean-convertible
    /// </summary>
    private static bool IsBooleanConvertible(Expression expr)
    {
        if (expr.Type == null)
        {
            // Type not yet inferred - allow for now
            return true;
        }

        if (expr.Type is FifthType.TDotnetType dotnetType)
        {
            return dotnetType.TheType == typeof(bool);
        }

        var typeName = GetTypeName(expr.Type);
        return typeName?.Equals("bool", StringComparison.OrdinalIgnoreCase) == true ||
               typeName?.Equals("boolean", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Check if type1 is equal to or broader than type2
    /// </summary>
    private static bool IsTypeEqualOrBroader(FifthType type1, FifthType type2)
    {
        var name1 = GetTypeName(type1);
        var name2 = GetTypeName(type2);
        
        if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
        {
            return false;
        }

        // If same name, they're equal
        if (name1.Equals(name2, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check if type1 is System.Exception (broadest exception type)
        if (name1.Equals("System.Exception", StringComparison.OrdinalIgnoreCase) ||
            name1.Equals("Exception", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // For .NET types, check actual type hierarchy
        if (type1 is FifthType.TDotnetType dt1 && type2 is FifthType.TDotnetType dt2)
        {
            return dt1.TheType.IsAssignableFrom(dt2.TheType);
        }

        return false;
    }

    private static string? GetTypeName(FifthType type)
    {
        try
        {
            if (type.Name != null)
            {
                return type.Name.Value ?? type.Name.ToString();
            }
        }
        catch { }

        return null;
    }
}
