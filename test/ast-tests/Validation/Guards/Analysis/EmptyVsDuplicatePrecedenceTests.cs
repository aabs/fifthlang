using TUnit.Core;
using FluentAssertions;
using ast;
using compiler.Validation.GuardValidation;
using ast_model.TypeSystem;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Analysis;

public class EmptyVsDuplicatePrecedenceTests
{
    [Test]
    public void EmptyInterval_ShouldTakePrecedenceOverDuplicate()
    {
        // First overload has an empty interval: x > 5 && x <= 5
        var empty = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.GreaterThan,
                    RHS = new Int32LiteralExp { Value = 5 }
                },
                Operator = Operator.LogicalAnd,
                RHS = new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.LessThanOrEqual,
                    RHS = new Int32LiteralExp { Value = 5 }
                }
            });

        // Second overload duplicates the same empty condition syntactically
        var dupeEmpty = new MockOverloadableFunction(hasConstraints: true).WithConstraint(
            new BinaryExp
            {
                LHS = new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.GreaterThan,
                    RHS = new Int32LiteralExp { Value = 5 }
                },
                Operator = Operator.LogicalAnd,
                RHS = new BinaryExp
                {
                    LHS = new VarRefExp { VarName = "param0" },
                    Operator = Operator.LessThanOrEqual,
                    RHS = new Int32LiteralExp { Value = 5 }
                }
            });

        var ofd = new OverloadedFunctionDef
        {
            Name = MemberName.From("f"),
            OverloadClauses = new List<IOverloadableFunction> { empty, dupeEmpty },
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

        // Expect exactly one GUARD_UNREACHABLE emitted, not two, due to empty-interval precedence
        var unreachableCount = validator.Diagnostics.Count(d => d.Message.Contains("GUARD_UNREACHABLE (W1002)"));
        unreachableCount.Should().Be(1);
    }
}
