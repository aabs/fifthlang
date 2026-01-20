using compiler;
using FluentAssertions;

namespace runtime_integration_tests.Validation;

public class NamespaceImportDiagnosticsTests : RuntimeTestBase
{
    [Fact]
    public async Task UndeclaredNamespaceImport_ShouldEmitWarningWithSchema()
    {
        var moduleDir = Path.Combine(TempDirectory, "undeclared_ns");
        var filePath = await NamespaceImportTestHelpers.WriteSourceAsync(moduleDir, "consumer.5th",
            """
            namespace App.Core;
            import Missing.Namespace;

            main(): int {
                return 0;
            }
            """);

        var outputFile = Path.Combine(TempDirectory, "undeclared_ns.dll");
        GeneratedFiles.Add(outputFile);

        var result = await NamespaceImportTestHelpers.CompileAsync(moduleDir, outputFile, diagnostics: true);

        result.Success.Should().BeTrue("missing namespaces should emit warnings but continue");

        var warning = result.Diagnostics.Single(d => d.Level == DiagnosticLevel.Warning && d.Code == "WNS0001");
        warning.Message.Should().Contain("Missing.Namespace");
        warning.Source.Should().NotBeNull();
        warning.Source!.Should().Contain(filePath);

        var namespaceProperty = warning.GetType().GetProperty("Namespace");
        namespaceProperty.Should().NotBeNull("diagnostic schema must include namespace field");
        namespaceProperty!.GetValue(warning)?.ToString().Should().Be("Missing.Namespace");

        var lineProperty = warning.GetType().GetProperty("Line");
        var columnProperty = warning.GetType().GetProperty("Column");
        lineProperty.Should().NotBeNull("diagnostic schema must include line field");
        columnProperty.Should().NotBeNull("diagnostic schema must include column field");

        var lineValue = Convert.ToInt32(lineProperty!.GetValue(warning));
        var columnValue = Convert.ToInt32(columnProperty!.GetValue(warning));
        lineValue.Should().BeGreaterThan(0);
        columnValue.Should().BeGreaterThan(0);
    }
}
