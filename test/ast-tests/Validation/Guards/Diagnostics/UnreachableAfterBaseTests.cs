using Xunit;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class UnreachableAfterBaseTests
{
    [Fact]
    public void AfterBase_AnalyzableStillWarnsUnreachable()
    {
        // Create function with guarded overload, base overload (last), then another guarded overload
        // The overload after base should warn as unreachable since base catches everything

        var firstGuarded = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 42 }
            });

        var baseOverload = new MockOverloadableFunction(hasConstraints: false); // base (no constraint)

        var unreachableAfterBase = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 99 }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { firstGuarded, baseOverload, unreachableAfterBase },
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

        var validator = new GuardCompletenessValidator();
        validator.VisitAssemblyDef(asm);

        // Should warn that the overload after base is unreachable since base catches all cases
        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }
}
