using ast;
using ast_generated;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Validates interpolation expressions in SPARQL literals.
/// Ensures interpolations follow the rules defined in User Story 3:
/// - No nested interpolations (SPARQL004) - no ?<...> or @<...> inside {{...}}
/// - All other expressions are allowed (function calls, arithmetic, complex expressions, etc.)
/// </summary>
public class SparqlInterpolationValidator : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic> diagnostics = new();
    private bool isInsideInterpolation = false;

    /// <summary>
    /// Gets the list of diagnostics generated during validation.
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => diagnostics.AsReadOnly();

    /// <summary>
    /// Visits a SparqlLiteralExpression and validates its interpolations.
    /// </summary>
    public override SparqlLiteralExpression VisitSparqlLiteralExpression(SparqlLiteralExpression ctx)
    {
        // First visit children using base implementation
        var result = base.VisitSparqlLiteralExpression(ctx);

        // Validate each interpolation
        foreach (var interpolation in result.Interpolations)
        {
            ValidateInterpolation(interpolation, result);
        }

        return result;
    }

    /// <summary>
    /// Validates a single interpolation expression.
    /// </summary>
    private void ValidateInterpolation(Interpolation interpolation, SparqlLiteralExpression context)
    {
        if (interpolation.Expression == null)
        {
            return;
        }

        // Check for nested interpolations (nested SPARQL or TriG literals)
        // This is the only restriction - all other expressions are allowed
        isInsideInterpolation = true;
        var hasNestedInterpolation = ContainsNestedInterpolation(interpolation.Expression);
        isInsideInterpolation = false;

        if (hasNestedInterpolation)
        {
            EmitDiagnostic(
                SparqlDiagnostics.NestedInterpolation,
                SparqlDiagnostics.FormatNestedInterpolation(),
                DiagnosticSeverity.Error,
                context);
        }

        // Note: We no longer restrict to simple expressions. Complex expressions
        // including function calls, arithmetic, etc. are all allowed.
        // The only restriction is no nested SPARQL/TriG literals (checked above).
    }

    /// <summary>
    /// Checks if an expression contains nested interpolations (inside SPARQL or TriG literals).
    /// </summary>
    private bool ContainsNestedInterpolation(Expression expr)
    {
        return expr switch
        {
            SparqlLiteralExpression => true,
            TriGLiteralExpression => true,
            BinaryExp binary => ContainsNestedInterpolation(binary.LHS) || ContainsNestedInterpolation(binary.RHS),
            UnaryExp unary => ContainsNestedInterpolation(unary.Operand),
            FuncCallExp funcCall => funcCall.InvocationArguments.Any(arg => ContainsNestedInterpolation(arg)),
            MemberAccessExp memberAccess => 
                ContainsNestedInterpolation(memberAccess.LHS) || 
                (memberAccess.RHS != null && ContainsNestedInterpolation(memberAccess.RHS)),
            _ => false
        };
    }

    /// <summary>
    /// Emits a diagnostic message.
    /// </summary>
    private void EmitDiagnostic(string code, string message, DiagnosticSeverity severity, SparqlLiteralExpression context)
    {
        var diagnostic = new Diagnostic
        {
            Code = code,
            Message = message,
            Severity = severity,
            Filename = context.Location?.Filename ?? "",
            Line = context.Location?.Line ?? 0,
            Column = context.Location?.Column ?? 0
        };

        diagnostics.Add(diagnostic);
    }
}
