using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ast;
using ast_model.TypeSystem;
using compiler.Validation.GuardValidation;

[MemoryDiagnoser]
public class GuardValidationBenchmarks
{
    [Params(10, 100)]
    public int GroupCount { get; set; }

    [Params(4, 16)]
    public int OverloadsPerGroup { get; set; }

    [Params(false, true)]
    public bool UsePooling { get; set; }

    private AssemblyDef _assembly = default!;

    [GlobalSetup]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("FIFTH_GUARD_VALIDATION_POOL", UsePooling ? "1" : "0");
        // Build an assembly with many overloaded function definitions
        var modules = new List<ModuleDef>();
        var module = new ModuleDef
        {
            OriginalModuleName = "bench_module",
            NamespaceDecl = NamespaceName.From("bench"),
            Classes = new List<ClassDef>(),
            Functions = new List<ScopedDefinition>(),
            Visibility = Visibility.Public
        };

        for (int gi = 0; gi < GroupCount; gi++)
        {
            var overloads = new List<IOverloadableFunction>();
            for (int oi = 0; oi < OverloadsPerGroup; oi++)
            {
                var param = new ParamDef
                {
                    Name = "x",
                    TypeName = TypeName.From("int"),
                    CollectionType = default!,
                    ParameterConstraint = (oi % 2 == 0) ? new BinaryExp
                    {
                        Operator = Operator.GreaterThan,
                        LHS = new VarRefExp { VarName = "x" },
                        RHS = new Int32LiteralExp { Value = 0 }
                    } : null,
                    DestructureDef = null,
                    Visibility = Visibility.Public
                };

                var func = new FunctionDef
                {
                    Name = MemberName.From($"f{gi}"),
                    Params = new List<ParamDef> { param },
                    Body = new BlockStatement { Statements = new List<Statement>() },
                    ReturnType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") },
                    IsConstructor = false,
                    IsStatic = false,
                    Visibility = Visibility.Public,
                    TypeParameters = new List<TypeParameterDef>()
                };

                overloads.Add(func);
            }

            var overloaded = new OverloadedFunctionDef
            {
                Name = MemberName.From($"f{gi}"),
                Signature = new FunctionSignature
                {
                    Name = MemberName.From($"f{gi}"),
                    FormalParameterTypes = new[] { new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") } },
                    GenericTypeParameters = Array.Empty<FifthType>(),
                    ReturnType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") },
                    DeclaringType = null
                },
                OverloadClauses = overloads,
                Params = new List<ParamDef>(),
                Body = new BlockStatement { Statements = new List<Statement>() },
                ReturnType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("Int32") },
                IsConstructor = false,
                IsStatic = false,
                Visibility = Visibility.Public
            };

            module.Functions.Add(overloaded);
        }

        modules.Add(module);

        _assembly = new AssemblyDef
        {
            Name = AssemblyName.From("bench"),
            PublicKeyToken = "",
            Version = "0.0.0",
            TestProperty = "",
            AssemblyRefs = new List<AssemblyRef>(),
            Modules = modules,
            Visibility = Visibility.Public
        };
    }

    [Benchmark]
    public void RunGuardCompletenessValidation()
    {
        var validator = new GuardCompletenessValidator();
        // VisitAssemblyDef triggers validation for collected groups
        validator.VisitAssemblyDef(_assembly);
    }
}
