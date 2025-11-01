using TUnit.Core;
using FluentAssertions;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class ExplosionThresholdTests
{
    [Test]
    public void UnknownPercent_JustBelowThreshold_ShouldNotWarn()
    {
        // Build 12 overloads, 6 UNKNOWN (50%), no base => should not warn (strict >50 required)
        var overloads = new List<IOverloadableFunction>();
        for (int i = 0; i < 6; i++)
        {
            // analyzable simple equality
            overloads.Add(new MockOverloadableFunction(hasConstraints: true).WithConstraint(
                new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.Equal,
                    RHS = new Int32LiteralExp { Value = i }
                }));
        }
        for (int i = 0; i < 6; i++)
        {
            // UNKNOWN: OR makes it unknown to our normalizer
            overloads.Add(new MockOverloadableFunction(hasConstraints: true).WithConstraint(
                new BinaryExp
                {
                    LHS = new BooleanLiteralExp { Value = true },
                    Operator = Operator.LogicalOr,
                    RHS = new BooleanLiteralExp { Value = false }
                }));
        }

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
        validator.Diagnostics.Should().NotContain(d => d.Message.Contains("GUARD_UNKNOWN_EXPLOSION (W1102)"));
    }

    [Test]
    public void UnknownPercent_JustAboveThreshold_ShouldWarn()
    {
        // 12 overloads, 7 UNKNOWN (~58%), no base => should warn
        var overloads = new List<IOverloadableFunction>();
        for (int i = 0; i < 5; i++)
        {
            overloads.Add(new MockOverloadableFunction(hasConstraints: true).WithConstraint(
                new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.Equal,
                    RHS = new Int32LiteralExp { Value = i }
                }));
        }
        for (int i = 0; i < 7; i++)
        {
            overloads.Add(new MockOverloadableFunction(hasConstraints: true).WithConstraint(
                new BinaryExp
                {
                    LHS = new BooleanLiteralExp { Value = true },
                    Operator = Operator.LogicalOr,
                    RHS = new BooleanLiteralExp { Value = false }
                }));
        }

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
        validator.Diagnostics.Should().Contain(d => d.Message.Contains("GUARD_UNKNOWN_EXPLOSION (W1102)"));
    }
}
