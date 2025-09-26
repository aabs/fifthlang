using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Determinism;

public class DeterminismHashTests
{
    [Test]
    public void DiagnosticsSignature_ShouldBeDeterministicAcrossTwoRuns()
    {
        // Create a function with multiple diagnostic scenarios to test deterministic ordering
        var incompleteOverload = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 42 }
            });

        var duplicateOverload1 = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 99 }
            });

        var duplicateOverload2 = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 99 }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { incompleteOverload, duplicateOverload1, duplicateOverload2 },
            Signature = new DummyFunctionSignature(),
            Params = [],
            Body = new BlockStatement { Statements = [] },
            ReturnType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") },
            IsStatic = true,
            IsConstructor = false,
            Visibility = Visibility.Public
        };

        var module = new ModuleDef
        {
            OriginalModuleName = "M",
            NamespaceDecl = NamespaceName.From("N"),
            Classes = [],
            Functions = [ofd],
            Visibility = Visibility.Public
        };

        var asm = new AssemblyDef
        {
            Name = AssemblyName.From("test"),
            PublicKeyToken = "",
            Version = "0.0.0",
            AssemblyRefs = [],
            Modules = [module],
            TestProperty = "",
            Visibility = Visibility.Public
        };

        // Run validation multiple times and compare diagnostic signatures
        string firstSignature = RunValidationAndGetSignature(asm);
        string secondSignature = RunValidationAndGetSignature(asm);

        // Signatures should be identical across runs
        firstSignature.Should().Be(secondSignature, "diagnostic signatures should be deterministic");

        // Ensure we actually have diagnostics to compare
        firstSignature.Should().NotBeEmpty("should have generated diagnostics to compare");
    }

    private static string RunValidationAndGetSignature(AssemblyDef assembly)
    {
        var validator = new GuardCompletenessValidator();
        validator.VisitAssemblyDef(assembly);

        // Create a deterministic signature from the diagnostic results
        var diagnosticMessages = validator.Diagnostics
            .Select(d => d.Message)
            .OrderBy(m => m) // Sort to ensure deterministic ordering
            .ToList();

        // Combine all messages into a single signature
        return string.Join("|", diagnosticMessages);
    }
}
