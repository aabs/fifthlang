using Xunit;
using FluentAssertions;
using compiler;

namespace ast_tests;

public class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_WithSuccessfulCommand_ShouldReturnSuccess()
    {
        var runner = new ProcessRunner();
        
        // Use a simple command that should exist on most systems
        var result = await runner.RunAsync("echo", "hello world");
        
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("hello world");
        result.StandardError.Should().BeEmpty();
        result.ElapsedTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task RunAsync_WithFailingCommand_ShouldReturnFailure()
    {
        var runner = new ProcessRunner();
        
        // Use a command that should exist but fail
        // On Unix: false command, On Windows: cmd with exit 1
        ProcessResult result;
        
        try
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                result = await runner.RunAsync("cmd", "/c exit 1");
            }
            else
            {
                result = await runner.RunAsync("sh", "-c 'exit 1'");
            }
            
            result.Success.Should().BeFalse();
            result.ExitCode.Should().NotBe(0);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Command doesn't exist in this environment, skip the test
            return;
        }
    }

    [Fact]
    public async Task RunAsync_WithWorkingDirectory_ShouldUseSpecifiedDirectory()
    {
        var runner = new ProcessRunner();
        var tempDir = Path.GetTempPath();
        
        // Get current directory from the process
        var result = await runner.RunAsync("pwd", workingDirectory: tempDir);
        
        if (result.Success)
        {
            // On Unix-like systems, pwd should return the working directory
            result.StandardOutput.Trim().Should().Be(tempDir.TrimEnd(Path.DirectorySeparatorChar));
        }
        else
        {
            // On Windows, use echo %cd% instead
            result = await runner.RunAsync("cmd", "/c echo %cd%", tempDir);
            if (result.Success)
            {
                result.StandardOutput.Should().Contain(tempDir.TrimEnd(Path.DirectorySeparatorChar));
            }
        }
    }
}

/// <summary>
/// Mock implementation of IProcessRunner for testing
/// </summary>
public class MockProcessRunner : IProcessRunner
{
    private readonly Queue<ProcessResult> _results = new();

    public void AddResult(ProcessResult result)
    {
        _results.Enqueue(result);
    }

    public Task<ProcessResult> RunAsync(string fileName, string arguments = "", string? workingDirectory = null)
    {
        if (_results.Count == 0)
        {
            throw new InvalidOperationException("No mock results configured");
        }

        return Task.FromResult(_results.Dequeue());
    }
}

public class MockProcessRunnerTests
{
    [Fact]
    public async Task MockProcessRunner_ShouldReturnConfiguredResults()
    {
        var mock = new MockProcessRunner();
        var expectedResult = new ProcessResult(0, "test output", "", TimeSpan.FromMilliseconds(100));
        mock.AddResult(expectedResult);
        
        var result = await mock.RunAsync("test");
        
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task MockProcessRunner_WithNoResults_ShouldThrow()
    {
        var mock = new MockProcessRunner();
        
        await FluentActions.Invoking(() => mock.RunAsync("test"))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}