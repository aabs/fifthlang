using FluentAssertions;
using code_generator;

namespace code_generator.Tests;

public class ILCodeGeneratorIntegrationTests
{
    [Test]
    public void ILCodeGeneratorConfiguration_DefaultSettings_ShouldHaveValidDefaults()
    {
        // Arrange & Act
        var config = new ILCodeGeneratorConfiguration();

        // Assert
        config.OutputDirectory.Should().NotBeNullOrEmpty();
        config.OptimizeCode.Should().BeFalse();
        config.GenerateDebugInfo.Should().BeFalse();
        config.ValidateOutput.Should().BeTrue();
    }

    [Test]
    public void ILCodeGeneratorConfiguration_CustomSettings_ShouldRetainValues()
    {
        // Arrange
        var customDir = "/tmp/custom";
        
        // Act
        var config = new ILCodeGeneratorConfiguration
        {
            OutputDirectory = customDir,
            OptimizeCode = true,
            GenerateDebugInfo = true,
            ValidateOutput = false
        };

        // Assert
        config.OutputDirectory.Should().Be(customDir);
        config.OptimizeCode.Should().BeTrue();
        config.GenerateDebugInfo.Should().BeTrue();
        config.ValidateOutput.Should().BeFalse();
    }

    [Test]
    public void AstToIlTransformationVisitor_CanBeInstantiated()
    {
        // Act
        var visitor = new AstToIlTransformationVisitor();

        // Assert
        visitor.Should().NotBeNull();
    }

    [Test]
    public void ILEmissionVisitor_CanBeInstantiated()
    {
        // Act
        var visitor = new ILEmissionVisitor();

        // Assert
        visitor.Should().NotBeNull();
    }

    [Test]
    public void ILCodeGenerator_CanBeInstantiatedWithDefaultConfig()
    {
        // Act
        var generator = new ILCodeGenerator();

        // Assert
        generator.Should().NotBeNull();
    }

    [Test]
    public void ILCodeGenerator_CanBeInstantiatedWithCustomConfig()
    {
        // Arrange
        var config = new ILCodeGeneratorConfiguration
        {
            OutputDirectory = "/tmp/test"
        };

        // Act
        var generator = new ILCodeGenerator(config);

        // Assert
        generator.Should().NotBeNull();
    }
}