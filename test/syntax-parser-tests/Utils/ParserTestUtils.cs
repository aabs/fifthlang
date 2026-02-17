using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using Fifth;

namespace syntax_parser_tests.Utils;

/// <summary>
/// Utility functions for focused parser testing and token inspection.
/// Provides helpers to lex, parse, collect errors, and invoke specific parser rules.
/// </summary>
public static class ParserTestUtils
{
    /// <summary>
    /// Create a lexer for the given input text.
    /// </summary>
    public static FifthLexer CreateLexer(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new FifthLexer(inputStream);
        
        // Remove default error listeners to prevent console spam during testing
        lexer.RemoveErrorListeners();
        
        return lexer;
    }

    /// <summary>
    /// Create a parser for the given input text.
    /// </summary>
    public static (FifthParser parser, List<string> errors) CreateParser(string input)
    {
        var lexer = CreateLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new FifthParser(tokens);
        
        // Capture syntax errors
        var errors = new List<string>();
        var errorListener = new TestErrorListener(errors);
        
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);
        
        return (parser, errors);
    }

    /// <summary>
    /// Get all tokens from the input string.
    /// </summary>
    public static List<IToken> GetTokens(string input)
    {
        var lexer = CreateLexer(input);
        var tokens = new List<IToken>();
        
        IToken token;
        do
        {
            token = lexer.NextToken();
            tokens.Add(token);
        } while (token.Type != Antlr4.Runtime.TokenConstants.EOF);
        
        return tokens;
    }

    /// <summary>
    /// Get token types and text from the input string.
    /// </summary>
    public static List<(int type, string text, string typeName)> GetTokenInfo(string input)
    {
        var lexer = CreateLexer(input);
        var tokens = new List<(int type, string text, string typeName)>();
        
        IToken token;
        do
        {
            token = lexer.NextToken();
            var typeName = lexer.Vocabulary.GetSymbolicName(token.Type) ?? $"UNKNOWN_{token.Type}";
            tokens.Add((token.Type, token.Text, typeName));
        } while (token.Type != Antlr4.Runtime.TokenConstants.EOF);
        
        return tokens;
    }

    /// <summary>
    /// Parse a specific rule and return the parse tree with any errors.
    /// </summary>
    public static T ParseRule<T>(string input, Func<FifthParser, T> ruleInvoker) where T : IParseTree
    {
        var (parser, errors) = CreateParser(input);
        var result = ruleInvoker(parser);
        
        if (errors.Any())
        {
            throw new InvalidOperationException($"Parse errors: {string.Join("; ", errors)}");
        }
        
        return result;
    }

    /// <summary>
    /// Parse a specific rule and return both the parse tree and any errors.
    /// </summary>
    public static (T parseTree, List<string> errors) ParseRuleWithErrors<T>(string input, Func<FifthParser, T> ruleInvoker) where T : IParseTree
    {
        var (parser, errors) = CreateParser(input);
        var result = ruleInvoker(parser);
        
        return (result, errors);
    }

    /// <summary>
    /// Assert that parsing succeeds with no errors.
    /// </summary>
    public static void AssertNoErrors(string input, Func<FifthParser, IParseTree> ruleInvoker, string? because = null)
    {
        var (_, errors) = ParseRuleWithErrors(input, ruleInvoker);
        errors.Should().BeEmpty(because ?? $"Parsing should succeed for input: {input}");
    }

    /// <summary>
    /// Assert that parsing fails with at least one error.
    /// </summary>
    public static void AssertHasErrors(string input, Func<FifthParser, IParseTree> ruleInvoker, string? because = null)
    {
        var (_, errors) = ParseRuleWithErrors(input, ruleInvoker);
        errors.Should().NotBeEmpty(because ?? $"Parsing should fail for input: {input}");
    }

    /// <summary>
    /// Assert that specific token types appear in the token stream.
    /// </summary>
    public static void AssertContainsTokens(string input, params string[] expectedTokenNames)
    {
        var tokenInfo = GetTokenInfo(input);
        var actualTokenNames = tokenInfo.Select(t => t.typeName).ToList();
        
        foreach (var expectedToken in expectedTokenNames)
        {
            actualTokenNames.Should().Contain(expectedToken, 
                $"Token stream should contain {expectedToken}. Actual tokens: [{string.Join(", ", actualTokenNames)}]");
        }
    }

    /// <summary>
    /// Assert that specific token types do NOT appear in the token stream.
    /// </summary>
    public static void AssertDoesNotContainTokens(string input, params string[] forbiddenTokenNames)
    {
        var tokenInfo = GetTokenInfo(input);
        var actualTokenNames = tokenInfo.Select(t => t.typeName).ToList();
        
        foreach (var forbiddenToken in forbiddenTokenNames)
        {
            actualTokenNames.Should().NotContain(forbiddenToken, 
                $"Token stream should not contain {forbiddenToken}. Actual tokens: [{string.Join(", ", actualTokenNames)}]");
        }
    }

    /// <summary>
    /// Pretty-print the token stream for debugging.
    /// </summary>
    public static string PrintTokens(string input)
    {
        var tokenInfo = GetTokenInfo(input);
        var result = new StringWriter();
        
        result.WriteLine($"Tokens for input: \"{input}\"");
        foreach (var (type, text, typeName) in tokenInfo)
        {
            result.WriteLine($"  {typeName} ({type}): \"{text}\"");
        }
        
        return result.ToString();
    }
}

/// <summary>
/// Error listener that collects error messages for testing.
/// </summary>
internal class TestErrorListener : BaseErrorListener
{
    private readonly List<string> _errors;

    public TestErrorListener(List<string> errors)
    {
        _errors = errors;
    }

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, 
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        _errors.Add($"line {line}:{charPositionInLine} {msg}");
    }
}