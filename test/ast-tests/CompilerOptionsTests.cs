using Xunit;
using FluentAssertions;
using compiler;

namespace ast_tests;

public class CompilerOptionsTests
{
    [Fact]
    public void DefaultOptions_ShouldHaveBuildCommand()
    {
        var options = new CompilerOptions();
        
        options.Command.Should().Be(CompilerCommand.Build);
        options.Source.Should().Be("");
        options.Output.Should().Be("");
        options.Args.Should().BeEmpty();
        options.KeepTemp.Should().BeFalse();
        options.Diagnostics.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSourceEmpty_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, "", "test.exe");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Source file or directory must be specified");
    }

    [Fact]
    public void Validate_WhenOutputEmptyForBuild_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, "test.5th", "");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Output path must be specified");
    }

    [Fact]
    public void Validate_WhenOutputEmptyForRun_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Run, "test.5th", "");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Output path must be specified");
    }

    [Fact]
    public void Validate_WhenOutputEmptyForLint_ShouldBeValid()
    {
        // Create a temporary file for testing
        var tempFile = Path.GetTempFileName();
        try
        {
            var options = new CompilerOptions(CompilerCommand.Lint, tempFile, "");
            
            var error = options.Validate();
            
            error.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Validate_WhenHelpCommand_ShouldIgnoreOtherValidation()
    {
        var options = new CompilerOptions(CompilerCommand.Help, "", "");
        
        var error = options.Validate();
        
        error.Should().BeNull();
    }

    [Fact]
    public void Validate_WhenSourceDoesNotExist_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, "nonexistent.5th", "test.exe");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Source path does not exist");
    }

    [Theory]
    [InlineData("arg1", "arg2", "arg3")]
    [InlineData("one two", "three")]
    [InlineData("")]
    public void Args_ShouldPreserveValues(params string[] args)
    {
        var options = new CompilerOptions(Args: args);
        
        options.Args.Should().Equal(args);
    }
}