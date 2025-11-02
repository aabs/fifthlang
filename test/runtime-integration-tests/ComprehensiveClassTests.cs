using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for class features
/// Each test corresponds to a .5th file in TestPrograms/Classes/
/// </summary>
public class ComprehensiveClassTests : RuntimeTestBase
{
    [Test]
    public async Task simple_class_ShouldReturn30()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Classes", "simple_class.5th");

        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);

        // Execute and validate result
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(30, "main() should return 30 from p.X + p.Y = 10 + 20 = 30");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task class_with_function_ShouldReturn25()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Classes", "class_with_function.5th");

        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);

        // Execute and validate result
        var result = await ExecuteAsync(executablePath);

        // Assert
        result.ExitCode.Should().Be(25, "main() should return 25 from get_age(person) where person.Age = 25");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task destructuring_example_ShouldReturn112()
    {
        // Arrange
        var sourceFile = Path.Combine("TestPrograms", "Classes", "destructuring_example.5th");

        // Act - Compile the test file
        var executablePath = await CompileFileAsync(sourceFile);

        // Execute and validate result
        var result = await ExecuteAsync(executablePath);

        // Assert
        var actual = result.ExitCode;
        if (actual > 256)
        {
            // on windows this can return large exit codes, so normalize
            actual = actual % 256; // Normalize exit code to fit in byte range
        }
        actual.Should().Be(112, "main() should return 112 from calculate_bonus(engineer) = 60000/10 = 6000 for Engineering department");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }
}