using ast;
using ast_generated;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Validates interpolation expressions in SPARQL literals.
/// Ensures interpolations follow the rules defined in User Story 3:
/// - No nested interpolations (SPARQL004)
/// - Only constant or simple variable references allowed (SPARQL005)
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

        // Check for nested interpolations
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

        // Check if expression is constant or simple variable reference
        if (!IsValidInterpolationExpression(interpolation.Expression))
        {
            EmitDiagnostic(
                SparqlDiagnostics.NonConstantInterpolation,
                SparqlDiagnostics.FormatNonConstantInterpolation(),
                DiagnosticSeverity.Error,
                context);
        }
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
            _ => false
        };
    }

    /// <summary>
    /// Checks if an expression is valid for interpolation.
    /// Valid expressions are:
    /// - Literals (string, int, float, etc.)
    /// - Simple variable references
    /// - Simple member access (e.g., obj.property)
    /// </summary>
    private bool IsValidInterpolationExpression(Expression expr)
    {
        return expr switch
        {
            // Literals are always valid
            StringLiteralExp => true,
            Int32LiteralExp => true,
            Int64LiteralExp => true,
            Float4LiteralExp => true,
            Float8LiteralExp => true,
            BooleanLiteralExp => true,

            // Simple variable references are valid
            VarRefExp => true,

            // Simple member access is valid (e.g., obj.property)
            MemberAccessExp memberAccess => IsSimpleMemberAccess(memberAccess),

            // Binary expressions with constants/variables are valid for string concatenation
            BinaryExp binary when binary.Operator == Operator.ArithmeticAdd =>
                IsValidInterpolationExpression(binary.LHS) && IsValidInterpolationExpression(binary.RHS),

            // Everything else is not allowed (function calls, complex expressions, etc.)
            _ => false
        };
    }

    /// <summary>
    /// Checks if a member access expression is simple (no method calls, no complex arguments).
    /// </summary>
    private bool IsSimpleMemberAccess(MemberAccessExp memberAccess)
    {
        // MemberAccessExp in Fifth has LHS and RHS
        // RHS can be a function call (with arguments) or a simple property access
        // For simple member access, RHS should not be a function call with arguments
        if (memberAccess.RHS is FuncCallExp)
        {
            return false;
        }
        return true;
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
