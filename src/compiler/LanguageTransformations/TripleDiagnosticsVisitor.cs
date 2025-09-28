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
}
