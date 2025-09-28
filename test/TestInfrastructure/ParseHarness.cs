using System.Diagnostics;
using Antlr4.Runtime;
using compiler;
using compiler.LangProcessingPhases;
using Fifth;

namespace test_infra;

/// <summary>
/// Options to control how far the harness processes the input and what auxiliary data it collects.
/// </summary>
/// <param name="Phase">Highest language analysis phase to execute (None leaves raw AST).</param>
/// <param name="CollectTokens">If true, returns the full token stream for disambiguation assertions.</param>
/// <param name="CollectTimings">If true, captures parse / AST build / phase timings.</param>
public record ParseOptions(
    FifthParserManager.AnalysisPhase Phase = FifthParserManager.AnalysisPhase.All,
    bool CollectTokens = false,
    bool CollectTimings = false
);

public enum DiagnosticSeverity { Info, Warning, Error }

public record TestDiagnostic(string Code, DiagnosticSeverity Severity, string Message, int Line, int Column, string Snippet);

/// <summary>
/// Result of harness parsing; immutable snapshot consumable by tests.
/// </summary>
public sealed class ParseResult
{
    public AssemblyDef? Root { get; init; }
    public IReadOnlyList<TestDiagnostic> Diagnostics { get; init; } = Array.Empty<TestDiagnostic>();
    public IReadOnlyList<IToken>? Tokens { get; init; }
    public TimeSpan? ParseTime { get; init; }
    public TimeSpan? AstBuildTime { get; init; }
    public TimeSpan? PhasesTime { get; init; }
}

/// <summary>
/// Central test harness for lex/parse/AST build/pipeline execution.
/// Keeps tests decoupled from internal parser and transformation wiring.
/// </summary>
/// <summary>
/// Test-only harness that performs lexing, parsing, AST construction and (optionally) staged analysis.
/// It deliberately avoids throwing on syntax errors so tests can assert partial results & diagnostics.
/// </summary>
public static class ParseHarness
{
    private sealed class CollectingErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        private readonly List<TestDiagnostic> _diags;
        public CollectingErrorListener(List<TestDiagnostic> diags) => _diags = diags;
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            => _diags.Add(new TestDiagnostic("SYNTAX", DiagnosticSeverity.Error, msg, line, charPositionInLine, string.Empty));
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            => _diags.Add(new TestDiagnostic("SYNTAX", DiagnosticSeverity.Error, msg, line, charPositionInLine, offendingSymbol?.Text ?? string.Empty));
    }

    /// <summary>
    /// Parse a Fifth source string into an AST plus diagnostics.
    /// </summary>
    /// <remarks>
    /// Phases executed are controlled by <see cref="ParseOptions.Phase"/>.
    /// Syntax diagnostics are always collected; later semantic diagnostics will be added once wired.
    /// </remarks>
    public static ParseResult ParseString(string source, ParseOptions? options = null)
    {
        options ??= new ParseOptions();
        var diagnostics = new List<TestDiagnostic>();

        // Configure lexer & parser manually to capture syntax diagnostics without throwing.
        var input = new AntlrInputStream(source);
        var lexer = new FifthLexer(input);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new CollectingErrorListener(diagnostics));
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new FifthParser(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new CollectingErrorListener(diagnostics));

        var swParse = Stopwatch.StartNew();
        var tree = parser.fifth();
        swParse.Stop();

        // If syntax errors, we still attempt AST build so tests can inspect partial structures if desired.
        var swAst = Stopwatch.StartNew();
        var visitor = new AstBuilderVisitor();
        var ast = visitor.Visit(tree) as AssemblyDef;
        swAst.Stop();

        AssemblyDef? processed = ast;
        TimeSpan? phasesTime = null;
        if (ast != null && options.Phase != FifthParserManager.AnalysisPhase.None)
        {
            var phaseDiagnostics = new List<compiler.Diagnostic>();
            var swPhases = Stopwatch.StartNew();
            processed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, diagnostics: phaseDiagnostics, upTo: options.Phase) as AssemblyDef;
            swPhases.Stop();
            phasesTime = swPhases.Elapsed;
            // Merge semantic/phase diagnostics, mapping to TestDiagnostic (preserve already collected syntax diags first)
            foreach (var d in phaseDiagnostics)
            {
                var severity = d.Level switch
                {
                    compiler.DiagnosticLevel.Info => DiagnosticSeverity.Info,
                    compiler.DiagnosticLevel.Warning => DiagnosticSeverity.Warning,
                    compiler.DiagnosticLevel.Error => DiagnosticSeverity.Error,
                    _ => DiagnosticSeverity.Info
                };
                var code = d.Code;
                if (string.IsNullOrEmpty(code))
                {
                    // Fallback: extract leading CODE: pattern
                    var msg = d.Message;
                    if (!string.IsNullOrEmpty(msg))
                    {
                        // Pattern: UPPER+DIGITS (colon optional) e.g. TRPL001:
                        var idx = msg.IndexOf(':');
                        if (idx > 0 && idx <= 12)
                        {
                            var candidate = msg.Substring(0, idx).Trim();
                            if (candidate.Length >= 5 && candidate.All(c => char.IsUpper(c) || char.IsDigit(c)))
                                code = candidate;
                        }
                    }
                }
                diagnostics.Add(new TestDiagnostic(code ?? string.Empty, severity, d.Message, 0, 0, string.Empty));
            }
        }

        return new ParseResult
        {
            Root = processed,
            Diagnostics = diagnostics,
            Tokens = options.CollectTokens ? tokenStream.GetTokens() : null,
            ParseTime = options.CollectTimings ? swParse.Elapsed : null,
            AstBuildTime = options.CollectTimings ? swAst.Elapsed : null,
            PhasesTime = options.CollectTimings ? phasesTime : null
        };
    }

    // --- Triple traversal helper utilities (merged from ast-tests harness) ---
    public static IEnumerable<TripleLiteralExp> FindTriples(AssemblyDef? root)
    {
        if (root == null) yield break;
        foreach (var module in root.Modules)
        {
            foreach (var f in module.Functions.OfType<FunctionDef>())
            {
                foreach (var t in FindTriplesInBlock(f.Body)) yield return t;
            }
        }
    }

    private static IEnumerable<TripleLiteralExp> FindTriplesInBlock(BlockStatement block)
    {
        foreach (var stmt in block.Statements)
        {
            foreach (var t in FindTriplesInStatement(stmt)) yield return t;
        }
    }

    private static IEnumerable<TripleLiteralExp> FindTriplesInStatement(Statement stmt)
    {
        switch (stmt)
        {
            case VarDeclStatement vds when vds.InitialValue != null:
                foreach (var t in FindTriplesInExpression(vds.InitialValue)) yield return t; break;
            case ExpStatement es:
                foreach (var t in FindTriplesInExpression(es.RHS)) yield return t; break;
            case BlockStatement inner:
                foreach (var t in FindTriplesInBlock(inner)) yield return t; break;
        }
    }

    private static IEnumerable<TripleLiteralExp> FindTriplesInExpression(Expression expr)
    {
        if (expr is TripleLiteralExp tr) { yield return tr; yield break; }
        switch (expr)
        {
            case BinaryExp be:
                foreach (var t in FindTriplesInExpression(be.LHS)) yield return t;
                foreach (var t in FindTriplesInExpression(be.RHS)) yield return t;
                break;
            case MemberAccessExp ma:
                if (ma.LHS is Expression lhs)
                    foreach (var t in FindTriplesInExpression(lhs)) yield return t;
                if (ma.RHS is Expression rhs)
                    foreach (var t in FindTriplesInExpression(rhs)) yield return t;
                break;
            case ListLiteral ll:
                foreach (var e in ll.ElementExpressions)
                    foreach (var t in FindTriplesInExpression(e)) yield return t;
                break;
            case FuncCallExp fc:
                foreach (var arg in fc.InvocationArguments)
                    foreach (var t in FindTriplesInExpression(arg)) yield return t;
                break;
        }
    }
}
