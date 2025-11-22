using FluentAssertions;
using Xunit;
using System.Reflection;

namespace runtime_integration_tests.Validation.GuardValidation;

public class PublicSurfaceTests
{
    [Fact]
    public void GuardValidationModule_ShouldOnlyExposePhaseEntryPoint()
    {
        // Arrange
        var assembly = typeof(compiler.Validation.GuardValidation.GuardCompletenessValidator).Assembly;
        var guardValidationNamespace = "compiler.Validation.GuardValidation";

        // Act
        var publicTypes = assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(guardValidationNamespace) == true)
            .Where(t => t.IsPublic)
            .ToList();

        // Assert
        publicTypes.Should().HaveCount(1,
            "only the main validator should be public to maintain encapsulation");

        var publicType = publicTypes.Single();
        publicType.Name.Should().Be("GuardCompletenessValidator",
            "the phase entry point should be the only public type");
        publicType.Namespace.Should().Be(guardValidationNamespace,
            "the public type should be in the root validation namespace");
    }

    [Fact]
    public void GuardValidationSubmodules_ShouldOnlyContainInternalTypes()
    {
        // Arrange
        var assembly = typeof(compiler.Validation.GuardValidation.GuardCompletenessValidator).Assembly;
        var subNamespaces = new[]
        {
            "compiler.Validation.GuardValidation.Infrastructure",
            "compiler.Validation.GuardValidation.Collection",
            "compiler.Validation.GuardValidation.Normalization",
            "compiler.Validation.GuardValidation.Analysis",
            "compiler.Validation.GuardValidation.Diagnostics",
            "compiler.Validation.GuardValidation.Instrumentation"
        };

        foreach (var ns in subNamespaces)
        {
            // Act
            var publicTypesInSubmodule = assembly.GetTypes()
                .Where(t => t.Namespace == ns)
                .Where(t => t.IsPublic)
                .ToList();

            // Assert
            publicTypesInSubmodule.Should().BeEmpty(
                $"submodule {ns} should only contain internal types for proper encapsulation");
        }
    }

    [Fact]
    public void GuardValidationModule_ShouldContainExpectedInternalTypes()
    {
        // Arrange
        var assembly = typeof(compiler.Validation.GuardValidation.GuardCompletenessValidator).Assembly;
        var expectedInternalTypes = new[]
        {
            "compiler.Validation.GuardValidation.Infrastructure.FunctionGroup",
            "compiler.Validation.GuardValidation.Infrastructure.PredicateDescriptor",
            "compiler.Validation.GuardValidation.Infrastructure.AnalyzedOverload",
            "compiler.Validation.GuardValidation.Infrastructure.PredicateType",
            "compiler.Validation.GuardValidation.Collection.OverloadCollector",
            "compiler.Validation.GuardValidation.Normalization.PredicateNormalizer",
            "compiler.Validation.GuardValidation.Analysis.CompletenessAnalyzer",
            "compiler.Validation.GuardValidation.Diagnostics.DiagnosticEmitter",
            "compiler.Validation.GuardValidation.Instrumentation.ValidationInstrumenter"
        };

        foreach (var expectedType in expectedInternalTypes)
        {
            // Act
            var type = assembly.GetType(expectedType);

            // Assert
            type.Should().NotBeNull($"expected internal type {expectedType} should exist");
            type!.IsPublic.Should().BeFalse($"type {expectedType} should be internal");
        }
    }
}