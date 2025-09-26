using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Analysis;

public class BooleanExhaustiveTests
{
    [Test]
    public void BooleanPair_ShouldBeConsideredCompleteWithoutBase()
    {
        var group = new FunctionGroup("testFunc", 1);
        // param0 == true
        var o1 = new MockOverloadableFunction(hasConstraints: true)
            .WithConstraint(new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new BooleanLiteralExp { Value = true }
            });
        // param0 == false
        var o2 = new MockOverloadableFunction(hasConstraints: true)
            .WithConstraint(new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new BooleanLiteralExp { Value = false }
            });

        group.AddOverload(o1);
        group.AddOverload(o2);

        var asm = new AssemblyDef
        {
            Name = AssemblyName.From("test"),
            PublicKeyToken = "",
            Version = "0.0.0",
            AssemblyRefs = [],
            Modules = [],
            TestProperty = "",
            Visibility = Visibility.Public
        };

        var validator = new GuardCompletenessValidator();
        // Simulate collection by directly validating the group through private flow: easiest is invoke visit then manually call internal path
        // Here we'll rely on GuardCompletenessValidator's public behavior by injecting group via OverloadCollector is internal; instead validate by reflection isn't ideal.
        // Simpler: emulate end-to-end by placing the group into a module-like structure is non-trivial.
        // So directly exercise CoverageEvaluator path by ensuring no E1001 is emitted when running ValidateFunctionGroup via visitation.

        // Build a minimal AST with an OverloadedFunctionDef so the validator collects this group
        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("testFunc"),
            OverloadClauses = [o1, o2],
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

        // Recreate assembly with module list including our function group
        asm = new AssemblyDef
        {
            Name = AssemblyName.From("test"),
            PublicKeyToken = "",
            Version = "0.0.0",
            AssemblyRefs = [],
            Modules = [module],
            TestProperty = "",
            Visibility = Visibility.Public
        };

        validator.VisitAssemblyDef(asm);

        validator.Diagnostics.Should().NotContain(d => d.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }
}
