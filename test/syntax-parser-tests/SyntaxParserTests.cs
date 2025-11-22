using System.Collections.Generic;
using FluentAssertions;

namespace syntax_parser_tests;

/// <summary>
/// Parses all sample Syntax .5th files under TestPrograms/Syntax.
/// Files under Syntax/Invalid are expected to fail parsing.
/// These files are linked from the runtime-integration-tests samples.
/// </summary>
public class SyntaxParserTests
{
    private static IEnumerable<string> GetSyntaxSampleFiles(bool invalid)
    {
        var root = Path.Combine(AppContext.BaseDirectory, "TestPrograms", "Syntax");
        var dir = invalid ? Path.Combine(root, "Invalid") : root;
        if (!Directory.Exists(dir)) yield break;
        foreach (var file in Directory.EnumerateFiles(dir, "*.5th", SearchOption.TopDirectoryOnly))
        {
            yield return file;
        }
    }

    [Fact]
    public void AllValidSyntaxSamples_ShouldParse()
    {
        foreach (var file in GetSyntaxSampleFiles(invalid: false))
        {
            var act = () => compiler.FifthParserManager.ParseFileSyntaxOnly(file);
            act.Should().NotThrow($"Parsing should succeed for {file}");
        }
    }

    [Fact]
    public void AllInvalidSyntaxSamples_ShouldFailParsing()
    {
        foreach (var file in GetSyntaxSampleFiles(invalid: true))
        {
            var act = () => compiler.FifthParserManager.ParseFileSyntaxOnly(file);
            act.Should().Throw<Exception>($"Parsing should fail for invalid sample {file}");
        }
    }
}
