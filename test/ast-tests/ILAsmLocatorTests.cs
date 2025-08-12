using Xunit;
using FluentAssertions;
using compiler;

namespace ast_tests;

public class ILAsmLocatorTests
{
    [Fact]
    public void FindILAsm_ShouldReturnStringOrNull()
    {
        // This test just ensures the method doesn't throw
        var result = ILAsmLocator.FindILAsm();
        
        // Result can be null (if ilasm not found) or a string path
        if (result != null)
        {
            result.Should().BeOfType<string>();
        }
    }

    [Fact]
    public void GetILAsmNotFoundMessage_ShouldReturnHelpfulMessage()
    {
        var message = ILAsmLocator.GetILAsmNotFoundMessage();
        
        message.Should().NotBeNullOrEmpty();
        message.Should().Contain("ILASM_PATH");
        message.Should().Contain("DOTNET_ROOT");
        message.Should().Contain("PATH");
    }

    [Fact]
    public void FindILAsm_WithILASM_PATH_Set_ShouldReturnPath()
    {
        // Create a temporary file to simulate ilasm
        var tempFile = Path.GetTempFileName();
        try
        {
            // Set environment variable
            Environment.SetEnvironmentVariable("ILASM_PATH", tempFile);
            
            var result = ILAsmLocator.FindILAsm();
            
            result.Should().Be(tempFile);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ILASM_PATH", null);
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindILAsm_WithInvalidILASM_PATH_ShouldFallback()
    {
        // Set environment variable to non-existent file
        Environment.SetEnvironmentVariable("ILASM_PATH", "/nonexistent/path/ilasm");
        try
        {
            var result = ILAsmLocator.FindILAsm();
            
            // Should either find ilasm elsewhere or return null, but not use the invalid path
            result.Should().NotBe("/nonexistent/path/ilasm");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ILASM_PATH", null);
        }
    }
}