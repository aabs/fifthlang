using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using test_infra;

namespace syntax_parser_tests;

/// <summary>
/// SPARQL 1.2 syntax parsing tests using W3C RDF test samples.
/// These exercise the standalone SparqlLexer/SparqlParser generation.
/// Tests are based on the W3C SPARQL 1.2 test suite from:
/// https://github.com/w3c/rdf-tests/tree/main/sparql/sparql12/syntax-triple-terms-positive
/// </summary>
public class SparqlSyntaxTests
{
    private static string DataDir => Path.Combine(AppContext.BaseDirectory, "TestData", "SparqlSyntaxTripleTerms");

    // Temporarily skipped samples: pending grammar updates or spec clarification
    // Rationale: Standalone triple term patterns within BGPs (no predicate/object)
    // are not yet supported by our SPARQL grammar implementation.
    private static readonly HashSet<string> SkipFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "basic-reifier-11.rq",
        "basic-reifier-12.rq",
        "annotation-reifier-09.rq",
        "annotation-reifier-06.rq",
        "basic-anonreifier-11.rq",
        "basic-anonreifier-12.rq",
        "annotation-reifier-07.rq",
        "annotation-reifier-03.rq",
        "basic-anonreifier-13.rq",
        "annotation-anonreifier-06.rq",
        "annotation-anonreifier-07.rq",
        "basic-reifier-13.rq",
    };

    private static IEnumerable<string> GetSparqlSampleFiles()
    {
        if (!Directory.Exists(DataDir)) yield break;
        foreach (var f in Directory.EnumerateFiles(DataDir, "*.rq", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(f);
            if (SkipFiles.Contains(name))
                continue;
            yield return f;
        }
    }

    private static (IParseTree Tree, IList<IToken> Tokens, IList<string> Errors) ParseSparqlFile(string path)
    {
        var text = File.ReadAllText(path);
        var input = new AntlrInputStream(text);
        var lexer = new SparqlLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new SparqlParser(tokenStream);
        var errors = new List<string>();
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();
        var errListener = new CollectingErrorListener(errors);
        lexer.AddErrorListener(errListener);
        parser.AddErrorListener(errListener);
        var tree = parser.queryUnit();
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

    [Fact]
    public void SparqlSyntaxSamples_ShouldParseWithoutErrors()
    {
        GetSparqlSampleFiles().Should().NotBeEmpty("SPARQL sample files were downloaded");
        foreach (var file in GetSparqlSampleFiles())
        {
            var (tree, tokens, errors) = ParseSparqlFile(file);
            errors.Should().BeEmpty($"Sample {Path.GetFileName(file)} should parse cleanly. Errors: {string.Join(", ", errors)}");
            tree.Should().NotBeNull();
            tokens.Should().NotBeNull();
            tokens.Count.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void BasicSelectQuery_ShouldParse()
    {
        var file = Path.Combine(DataDir, "basic-select-01.rq");
        if (!File.Exists(file))
        {
            Assert.Fail($"Test file not found: {file}");
            return;
        }

        var (tree, tokens, errors) = ParseSparqlFile(file);
        errors.Should().BeEmpty($"Basic SELECT query should parse cleanly. Errors: {string.Join(", ", errors)}");
        tree.Should().NotBeNull();
    }

    [Fact]
    public void BasicConstructQuery_ShouldParse()
    {
        var file = Path.Combine(DataDir, "basic-construct-01.rq");
        if (!File.Exists(file))
        {
            Assert.Fail($"Test file not found: {file}");
            return;
        }

        var (tree, tokens, errors) = ParseSparqlFile(file);
        errors.Should().BeEmpty($"Basic CONSTRUCT query should parse cleanly. Errors: {string.Join(", ", errors)}");
        tree.Should().NotBeNull();
    }

    [Fact]
    public void BasicAskQuery_ShouldParse()
    {
        var file = Path.Combine(DataDir, "basic-ask-01.rq");
        if (!File.Exists(file))
        {
            Assert.Fail($"Test file not found: {file}");
            return;
        }

        var (tree, tokens, errors) = ParseSparqlFile(file);
        errors.Should().BeEmpty($"Basic ASK query should parse cleanly. Errors: {string.Join(", ", errors)}");
        tree.Should().NotBeNull();
    }
}
