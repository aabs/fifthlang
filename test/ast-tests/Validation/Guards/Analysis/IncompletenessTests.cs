using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using compiler;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Analysis;

public class IncompletenessTests
{
    [Test]
    public void IncompleteWithoutBase_ShouldEmitE1001()
    {
        // One boolean guard only -> incomplete
        var o1 = new MockOverloadableFunction(hasConstraints: true)
            .WithConstraint(new BinaryExp
            {
                LHS = new VarRefExp { VarName = "param0" },
                Operator = Operator.Equal,
                RHS = new BooleanLiteralExp { Value = true }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("testFunc"),
            OverloadClauses = [o1],
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

        validator.Diagnostics.Should().Contain(d => d.Level == DiagnosticLevel.Error && d.Message.Contains("GUARD_INCOMPLETE (E1001)"));
    }
}
