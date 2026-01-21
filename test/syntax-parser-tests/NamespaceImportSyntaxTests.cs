using FluentAssertions;
using System.Linq;

namespace syntax_parser_tests;

public class NamespaceImportSyntaxTests
{
    private static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "fifthlang.sln"))) return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }
        return Directory.GetCurrentDirectory();
    }

    private static string GetSamplePath(params string[] parts)
    {
        var repoRoot = FindRepoRoot();
        return Path.Combine(new[] { repoRoot, "src", "parser", "grammar", "test_samples" }.Concat(parts).ToArray());
    }

    [Fact]
    public void FileScopedNamespaceAndImport_ShouldParse()
    {
        var samples = new[]
        {
            GetSamplePath("namespace_import_basic.5th"),
            GetSamplePath("namespace_import_multiple.5th")
        };

        foreach (var sample in samples)
        {
            var act = () => compiler.FifthParserManager.ParseFileSyntaxOnly(sample);
            act.Should().NotThrow($"Parsing should succeed for {sample}");
        }
    }

    [Fact]
    public void LegacyUseDirective_ShouldFailParsing()
    {
        var sample = GetSamplePath("Invalid", "namespace_import_legacy_use.5th");
        var act = () => compiler.FifthParserManager.ParseFileSyntaxOnly(sample);
        act.Should().Throw<Exception>("Legacy use directives should not parse");
    }

    [Fact]
    public void MultipleFileScopedNamespaces_ShouldFailWithDiagnostic()
    {
        var sample = GetSamplePath("Invalid", "namespace_multiple_declarations.5th");
        var act = () => compiler.FifthParserManager.ParseFileSyntaxOnly(sample);
        act.Should().Throw<Exception>()
            .Where(ex => ex.Message.Contains("namespace", StringComparison.OrdinalIgnoreCase)
                && ex.Message.Contains("at most one", StringComparison.OrdinalIgnoreCase),
                "Modules must declare at most one file-scoped namespace");
    }
}
