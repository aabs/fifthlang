using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class MultipleBasePrecedenceTests
{
    [Test]
    public void MultipleBase_ShouldSuppressE1001()
    {
        // Create function with multiple base overloads and some guarded overloads
        // Per spec FR-053: E1005 (multiple base) suppresses E1001 (completeness)
        var baseOverload1 = new MockOverloadableFunction(hasConstraints: false); // no constraint = base
        var baseOverload2 = new MockOverloadableFunction(hasConstraints: false); // second base (E1005)
        var guardedOverload = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new Int32LiteralExp { Value = 42 }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { baseOverload1, baseOverload2, guardedOverload },
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
            Imports = [],
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

        // Should emit E1005 for multiple base
        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_MULTIPLE_BASE (E1005)"));

        // Should NOT emit E1001 (completeness) due to precedence suppression
        validator.Diagnostics.Should().NotContain(d => d.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }
}
