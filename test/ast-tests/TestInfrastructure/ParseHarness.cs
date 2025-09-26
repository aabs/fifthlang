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

    public static ParseResult ParseString(string source, ParseOptions? options = null)
    {
        options ??= new ParseOptions();
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
            processed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, diagnostics: null, upTo: options.Phase) as AssemblyDef;
            swPhases.Stop();
            phasesTime = swPhases.Elapsed;
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
}
