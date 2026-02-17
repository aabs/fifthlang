using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Validation pass for lambda functions (LFs).
/// Currently enforces the maximum arity rule (FR-008).
/// </summary>
public sealed class LambdaValidationVisitor : NullSafeRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic>? _diagnostics;

    // Spec default: 8
    private const int MaxLambdaParameters = 8;

    public LambdaValidationVisitor(List<compiler.Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override LambdaExp VisitLambdaExp(LambdaExp ctx)
    {
        var apply = ctx.FunctorDef?.InvocationFuncDev;
        var parameterCount = apply?.Params?.Count ?? 0;

        if (parameterCount > MaxLambdaParameters)
        {
            _diagnostics?.Add(new compiler.Diagnostic(
                compiler.DiagnosticLevel.Error,
                LambdaDiagnostics.FormatTooManyParameters(MaxLambdaParameters, parameterCount),
                ctx.Location?.Filename,
                LambdaDiagnostics.TooManyParameters));
        }

        return base.VisitLambdaExp(ctx);
    }
}
