using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Emits triple literal diagnostics (test-codes TRPL004, TRPL006) during analysis phase.
/// For now, codes are emitted using existing Diagnostic infrastructure (no code field yet).
/// Message format: CODE: message text.
/// </summary>
public sealed class TripleDiagnosticsVisitor : NullSafeRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic>? _diagnostics;

    public TripleDiagnosticsVisitor(List<compiler.Diagnostic>? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    private static bool HasGraphAnnotation(Expression? expr)
    {
        if (expr is null)
        {
            return false;
        }

        if (expr is MemberAccessExp member && member.Annotations != null && member.Annotations.ContainsKey("GraphExpr"))
        {
            return true;
        }

        return expr is BinaryExp binary && binary.Annotations != null && binary.Annotations.ContainsKey("LoweredGraphExpr");
    }

    private static bool IsUnsupportedTripleOperator(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticAdd => false,
            Operator.ArithmeticSubtract => false,
            _ => true
        };
    }

    public override AstThing Visit(AstThing node)
    {
        var result = base.Visit(node) as AstThing;
        if (result is TripleLiteralExp t)
        {
            if (t.ObjectExp is ListLiteral ll && (ll.ElementExpressions == null || ll.ElementExpressions.Count == 0))
            {
                _diagnostics?.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Warning,
                    "Empty list in triple object produces no triples.",
                    null,
                    Code: "TRPL004"));
                // Diagnostic(s) added to the shared diagnostics list (no console logging in tests)
            }
            if (t.ObjectExp is ListLiteral outer && outer.ElementExpressions != null && outer.ElementExpressions.Any(e => e is ListLiteral))
            {
                _diagnostics?.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Error,
                    "Nested lists not allowed in triple object.",
                    null,
                    Code: "TRPL006"));
                // Diagnostic(s) added to the shared diagnostics list (no console logging in tests)
            }
        }
        else if (result is MalformedTripleExp malformed)
        {
            // Special-case: missing object that contains an empty-list text should map to TRPL004 (empty-list warning)
            if (malformed.MalformedKind == "MissingObject" && malformed.Annotations != null && malformed.Annotations.TryGetValue("OriginalText", out var orig) && orig is string s && s.Contains("[]"))
            {
                _diagnostics?.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Warning,
                    "Empty list in triple object produces no triples.",
                    null,
                    Code: "TRPL004"));
                // Diagnostic(s) added to the shared diagnostics list (no console logging in tests)
            }
            else
            {
                var (msg, code) = malformed.MalformedKind switch
                {
                    "MissingObject" => ("Triple literal must have exactly 3 components; object component missing.", "TRPL001"),
                    "TrailingComma" => ("Trailing comma not allowed in triple literal.", "TRPL001"),
                    "TooManyComponents" => ("Triple literal must have exactly 3 components; too many provided.", "TRPL001"),
                    _ => ("Malformed triple literal.", "TRPL001")
                };
                _diagnostics?.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Error,
                    msg,
                    null,
                    Code: code));
                // Diagnostic(s) added to the shared diagnostics list (no console logging in tests)
            }
        }
        return result;
    }

    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        var result = base.VisitBinaryExp(ctx);
        if (_diagnostics == null)
        {
            return result;
        }

        var leftIsTriple = result.LHS is TripleLiteralExp;
        var rightIsTriple = result.RHS is TripleLiteralExp;

        if (result.Operator == Operator.ArithmeticSubtract && leftIsTriple)
        {
            if (rightIsTriple || HasGraphAnnotation(result.RHS))
            {
                _diagnostics.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Error,
                    "Triple operands cannot appear on the left of '-' (only graph - triple is supported).",
                    null,
                    Code: "TRPL012"));
            }
            else if (result.RHS != null)
            {
                _diagnostics.Add(new compiler.Diagnostic(
                    compiler.DiagnosticLevel.Error,
                    "Unsupported '-' combination for triple operand.",
                    null,
                    Code: "TRPL013"));
            }
        }
        else if (IsUnsupportedTripleOperator(result.Operator) && (leftIsTriple || rightIsTriple))
        {
            _diagnostics.Add(new compiler.Diagnostic(
                compiler.DiagnosticLevel.Error,
                "Unsupported operator for triple operand.",
                null,
                Code: "TRPL013"));
        }

        return result;
    }

    public override UnaryExp VisitUnaryExp(UnaryExp ctx)
    {
        var result = base.VisitUnaryExp(ctx);
        if (_diagnostics != null && result.Operator == Operator.LogicalNot && result.Operand is TripleLiteralExp)
        {
            _diagnostics.Add(new compiler.Diagnostic(
                compiler.DiagnosticLevel.Error,
                "Unsupported operator for triple operand.",
                null,
                Code: "TRPL013"));
        }

        return result;
    }
}
