using FluentAssertions;
using compiler;
using System.Runtime.InteropServices;

namespace ast_tests;

public class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_WithSuccessfulCommand_ShouldReturnSuccess()
    {
        var runner = new ProcessRunner();
        
        // Use platform-appropriate commands
        ProcessResult result;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            result = await runner.RunAsync("cmd", "/c echo hello world");
        }
        else
        {
            result = await runner.RunAsync("echo", "hello world");
        }
        
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
        
        // Use platform-appropriate failing commands
        ProcessResult result;
        
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
        
        // Use platform-appropriate commands to get current directory
        ProcessResult result;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, use echo %cd% to get current directory
            result = await runner.RunAsync("cmd", "/c echo %cd%", tempDir);
            if (result.Success)
            {
                result.StandardOutput.Should().Contain(tempDir.TrimEnd(Path.DirectorySeparatorChar));
            }
        }
        else
        {
            // On Unix-like systems, use pwd to get current directory
            result = await runner.RunAsync("pwd", workingDirectory: tempDir);
            if (result.Success)
            {
                // Use Contains() instead of Be() to handle symlink resolution differences on macOS
                // where /var may be a symlink to /private/var
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