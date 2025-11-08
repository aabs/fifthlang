using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using test_infra;

namespace syntax_parser_tests;

/// <summary>
/// Basic TriG syntax parsing tests using W3C RDF 1.2 TriG samples.
/// These exercise the standalone TrigLexer/TrigParser generation.
/// </summary>
public class TrigBasicSyntaxTests
{
    private static string DataDir => Path.Combine(AppContext.BaseDirectory, "TestData", "TrigBasic");

    private static IEnumerable<string> GetTrigSampleFiles()
    {
        if (!Directory.Exists(DataDir)) yield break;
        foreach (var f in Directory.EnumerateFiles(DataDir, "trig12-syntax-basic-*.trig", SearchOption.TopDirectoryOnly))
            yield return f;
    }

    private static (IParseTree Tree, IList<IToken> Tokens, IList<string> Errors) ParseTrigFile(string path)
    {
        var text = File.ReadAllText(path);
        var input = new AntlrInputStream(text);
        var lexer = new TrigLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new TrigParser(tokenStream);
        var errors = new List<string>();
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();
        var errListener = new CollectingErrorListener(errors);
        lexer.AddErrorListener(errListener);
        parser.AddErrorListener(errListener);
        var tree = parser.trigDoc();
        return (tree, tokenStream.GetTokens(), errors);
    }

    // Local error listener (parser + lexer)
    private sealed class CollectingErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        private readonly IList<string> _errors;
        public CollectingErrorListener(IList<string> errors) => _errors = errors;
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            => _errors.Add($"line {line}:{charPositionInLine} {msg}");
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            => _errors.Add($"line {line}:{charPositionInLine} {msg} token='{offendingSymbol?.Text}'");
    }

    [Test]
    public void TriGBasicSamples_ShouldParseWithoutErrors()
    {
        GetTrigSampleFiles().Should().NotBeEmpty("TriG sample files were downloaded");
        foreach (var file in GetTrigSampleFiles())
        {
            var (tree, tokens, errors) = ParseTrigFile(file);
            errors.Should().BeEmpty($"Sample {Path.GetFileName(file)} should parse cleanly");
            tree.Should().NotBeNull();
            tokens.Should().NotBeNull();
            tokens.Count.Should().BeGreaterThan(0);
        }
    }
}
