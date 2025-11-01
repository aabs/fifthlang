using FluentAssertions;
using compiler;

namespace ast_tests;

public class CompilerOptionsTests
{
    [Test]
    public void DefaultOptions_ShouldHaveBuildCommand()
    {
        var options = new CompilerOptions();
        
        options.Command.Should().Be(CompilerCommand.Build);
        options.Sources.Should().BeEmpty();
        options.Output.Should().Be("");
        options.Args.Should().BeEmpty();
        options.KeepTemp.Should().BeFalse();
        options.Diagnostics.Should().BeFalse();
    }

    [Test]
    public void Validate_WhenSourcesEmpty_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, Array.Empty<string>(), "test.exe");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("At least one source file must be specified");
    }

    [Test]
    public void Validate_WhenOutputEmptyForBuild_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, new[] { "test.5th" }, "");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Output path must be specified");
    }

    [Test]
    public void Validate_WhenOutputEmptyForRun_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Run, new[] { "test.5th" }, "");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Output path must be specified");
    }

    [Test]
    public void Validate_WhenOutputEmptyForLint_ShouldBeValid()
    {
        // Create a temporary file for testing
        var tempFile = Path.GetTempFileName();
        try
        {
            var options = new CompilerOptions(CompilerCommand.Lint, new[] { tempFile }, "");
            
            var error = options.Validate();
            
            error.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void Validate_WhenHelpCommand_ShouldIgnoreOtherValidation()
    {
        var options = new CompilerOptions(CompilerCommand.Help, Array.Empty<string>(), "");
        
        var error = options.Validate();
        
        error.Should().BeNull();
    }

    [Test]
    public void Validate_WhenSourceDoesNotExist_ShouldReturnError()
    {
        var options = new CompilerOptions(CompilerCommand.Build, new[] { "nonexistent.5th" }, "test.exe");
        
        var error = options.Validate();
        
        error.Should().NotBeNull();
        error.Should().Contain("Source file does not exist");
    }

    [Test]
    [Arguments("arg1", "arg2", "arg3")]
    [Arguments("one two", "three")]
    [Arguments("")]
    public void Args_ShouldPreserveValues(params string[] args)
    {
        var options = new CompilerOptions(Args: args);
        
        options.Args.Should().Equal(args);
    }
}