using TUnit.Core;
using FluentAssertions;
using ast;
using compiler.Validation.GuardValidation;
using ast_model.TypeSystem;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Analysis;

public class DuplicateDetectionTests
{
    [Test]
    public void DuplicateGuards_ShouldBeDetectedAsUnreachable()
    {
        var o1 = new MockOverloadableFunction(hasConstraints: true).WithConstraint(new BinaryExp
        {
            LHS = new VarRefExp { VarName = "param0" },
            Operator = Operator.Equal,
            RHS = new Int32LiteralExp { Value = 5 }
        });
        var o2 = new MockOverloadableFunction(hasConstraints: true).WithConstraint(new BinaryExp
        {
            LHS = new VarRefExp { VarName = "param0" },
            Operator = Operator.Equal,
            RHS = new Int32LiteralExp { Value = 5 }
        });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { o1, o2 },
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

        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
    }
}
