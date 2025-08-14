using Xunit;
using FluentAssertions;
using compiler;

namespace ast_tests;

public class CompilationResultTests
{
    [Fact]
    public void Successful_ShouldCreateSuccessfulResult()
    {
        var result = CompilationResult.Successful("test.exe", "test.il", TimeSpan.FromSeconds(1));
        
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        result.Diagnostics.Should().BeEmpty();
        result.OutputPath.Should().Be("test.exe");
        result.ILPath.Should().Be("test.il");
        result.ElapsedTime.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Failed_WithMessage_ShouldCreateFailedResult()
    {
        var result = CompilationResult.Failed(2, "Parse error", "test.5th");
        
        result.Success.Should().BeFalse();
        result.ExitCode.Should().Be(2);
        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        result.Diagnostics[0].Message.Should().Be("Parse error");
        result.Diagnostics[0].Source.Should().Be("test.5th");
    }

    [Fact]
    public void Failed_WithDiagnostics_ShouldCreateFailedResult()
    {
        var diagnostics = new[]
        {
            new Diagnostic(DiagnosticLevel.Error, "Error 1"),
            new Diagnostic(DiagnosticLevel.Warning, "Warning 1")
        };

        var result = CompilationResult.Failed(3, diagnostics);
        
        result.Success.Should().BeFalse();
        result.ExitCode.Should().Be(3);
        result.Diagnostics.Should().HaveCount(2);
        result.Diagnostics.Should().Equal(diagnostics);
    }
}

public class DiagnosticTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var diagnostic = new Diagnostic(DiagnosticLevel.Warning, "Test message", "test.5th");
        
        diagnostic.Level.Should().Be(DiagnosticLevel.Warning);
        diagnostic.Message.Should().Be("Test message");
        diagnostic.Source.Should().Be("test.5th");
    }

    [Fact]
    public void Constructor_WithoutSource_ShouldAllowNullSource()
    {
        var diagnostic = new Diagnostic(DiagnosticLevel.Info, "Info message");
        
        diagnostic.Level.Should().Be(DiagnosticLevel.Info);
        diagnostic.Message.Should().Be("Info message");
        diagnostic.Source.Should().BeNull();
    }
}