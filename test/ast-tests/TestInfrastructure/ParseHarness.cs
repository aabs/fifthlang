using System;
using System.Diagnostics;
using Antlr4.Runtime;
using compiler;
using compiler.LangProcessingPhases;
using Fifth;
using ast; // needed for AssemblyDef

namespace test_infra;

public record ParseOptions(
    FifthParserManager.AnalysisPhase Phase = FifthParserManager.AnalysisPhase.All,
    bool CollectTokens = false,
    bool CollectTimings = false
);

public enum DiagnosticSeverity { Info, Warning, Error }

public record TestDiagnostic(string Code, DiagnosticSeverity Severity, string Message, int Line, int Column, string Snippet);

public sealed class ParseResult
{
    public AssemblyDef? Root { get; init; }
    public IReadOnlyList<TestDiagnostic> Diagnostics { get; init; } = Array.Empty<TestDiagnostic>();
    public IReadOnlyList<IToken>? Tokens { get; init; }
    public TimeSpan? ParseTime { get; init; }
    public TimeSpan? AstBuildTime { get; init; }
    public TimeSpan? PhasesTime { get; init; }
}

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

    public static ParseResult ParseString(string source, ParseOptions? options = null)
    {
        options ??= new ParseOptions();
        if (source.Contains("\\n", StringComparison.Ordinal))
        {
            source = source.Replace("\\r\\n", "\n", StringComparison.Ordinal)
                           .Replace("\\n", "\n", StringComparison.Ordinal);
        }
        var diagnostics = new List<TestDiagnostic>();

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

        var swAst = Stopwatch.StartNew();
        var visitor = new AstBuilderVisitor();
        var ast = visitor.Visit(tree) as AssemblyDef;
        swAst.Stop();

        AssemblyDef? processed = ast;
        TimeSpan? phasesTime = null;
        if (ast != null && options.Phase != FifthParserManager.AnalysisPhase.None)
        {
            var swPhases = Stopwatch.StartNew();
            var compDiags = new List<compiler.Diagnostic>();
            processed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, diagnostics: compDiags, upTo: options.Phase) as AssemblyDef;
            swPhases.Stop();
            phasesTime = swPhases.Elapsed;

            if (processed == null)
            {
                processed = ast;
            }

            // Phase diagnostics are merged into harness result (no console logging)
            foreach (var d in compDiags)
            {
                var codeCandidate = d.Code ?? string.Empty;
                if (string.IsNullOrEmpty(codeCandidate) && d.Message.StartsWith("TRPL"))
                {
                    codeCandidate = d.Message.Split(':')[0];
                }
                if (codeCandidate.StartsWith("TRPL"))
                {
                    diagnostics.Add(new TestDiagnostic(codeCandidate, d.Level switch
                    {
                        compiler.DiagnosticLevel.Error => DiagnosticSeverity.Error,
                        compiler.DiagnosticLevel.Warning => DiagnosticSeverity.Warning,
                        _ => DiagnosticSeverity.Info
                    }, d.Message, 0, 0, string.Empty));
                }
            }
        }


        return new ParseResult
        {
            Root = processed,
            Diagnostics = diagnostics,
            Tokens = options.CollectTokens ? tokenStream.GetTokens().ToList() : null,
            ParseTime = options.CollectTimings ? swParse.Elapsed : null,
            AstBuildTime = options.CollectTimings ? swAst.Elapsed : null,
            PhasesTime = options.CollectTimings ? phasesTime : null
        };
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
