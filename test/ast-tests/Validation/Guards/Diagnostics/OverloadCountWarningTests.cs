using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class OverloadCountWarningTests
{
    [Test]
    public void CountAt32_ShouldNotWarn()
    {
        var overloads = new List<IOverloadableFunction>();
        for (int i = 0; i < 32; i++) overloads.Add(new MockOverloadableFunction());

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = overloads,
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

        validator.Diagnostics.Should().NotContain(d => d.Message.Contains("GUARD_OVERLOAD_COUNT (W1101)"));
    }

    [Test]
    public void CountAt33_ShouldWarn()
    {
        var overloads = new List<IOverloadableFunction>();
        for (int i = 0; i < 33; i++) overloads.Add(new MockOverloadableFunction());

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = overloads,
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

        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_OVERLOAD_COUNT (W1101)"));
    }
}
