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

    public override Triple VisitTriple(Triple t)
    {
        // TRPL004: empty list object
        if (t.ObjectExp is ListLiteral ll && ll.ElementExpressions.Count == 0)
        {
            _diagnostics?.Add(new compiler.Diagnostic(compiler.DiagnosticLevel.Warning, "TRPL004: Empty list in triple object produces no triples."));
        }
        // TRPL006: nested list
        if (t.ObjectExp is ListLiteral outer && outer.ElementExpressions.Any(e => e is ListLiteral))
        {
            _diagnostics?.Add(new compiler.Diagnostic(compiler.DiagnosticLevel.Error, "TRPL006: Nested lists not allowed in triple object."));
        }
        return t;
    }

    public override AstThing Visit(AstThing node)
    {
        if (node is MalformedTripleExp malformed)
        {
            string message = malformed.MalformedKind switch
            {
                "MissingObject" => "TRPL001: Triple literal must have exactly 3 components; object component missing.",
                "TrailingComma" => "TRPL001: Trailing comma not allowed in triple literal.",
                "TooManyComponents" => "TRPL001: Triple literal must have exactly 3 components; too many provided.",
                _ => "TRPL001: Malformed triple literal."
            };
            _diagnostics?.Add(new compiler.Diagnostic(compiler.DiagnosticLevel.Error, message));
        }
        return base.Visit(node);
    }
}
