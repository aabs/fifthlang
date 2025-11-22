using Xunit;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class BaseNotLastCoverageTests
{
    [Fact]
    public void BaseNotLast_ShouldStillAllowE1001()
    {
        // Create function where guarded overloads come BEFORE base (creating incomplete coverage scenario)
        // followed by invalid overload after base. This tests that E1004 doesn't suppress E1001.

        var guardedOverload = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 42 }
            });
        var baseOverload = new MockOverloadableFunction(hasConstraints: false); // base (no constraint) - not last
        var invalidAfterBase = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 99 }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { guardedOverload, baseOverload, invalidAfterBase },
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

        // Should emit E1004 for base not last (base is at index 2, invalid after base is at index 3) 
        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_BASE_NOT_LAST (E1004)"));

        // Should still emit E1001 for incomplete coverage (guards before base are incomplete: only x==42)
        // Since the base is not properly positioned (not last), the incomplete guarded coverage should still be flagged
        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }
}
