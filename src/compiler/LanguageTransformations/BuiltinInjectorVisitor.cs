using static ast_model.TypeSystem.FifthType;

namespace compiler.LanguageTransformations;

/// <summary>
/// A visitor that injects builtin things into the AST.
/// </summary>
public class BuiltinInjectorVisitor : DefaultRecursiveDescentVisitor
{
    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        WrapType(typeof(Fifth.System.IO));
        WrapType(typeof(Fifth.System.Math));
        // Register knowledge graph helpers so lowering/codegen can reference them later
        WrapType(typeof(Fifth.System.KG));

        // Register KG wrapper types as globally available predeclared types
        // Per FR-002: graph, triple, and store are globally available (no imports required)
        RegisterKGTypes();

        // ctx.Functions.Add(new BuiltinFunctionDefinition("print", "void", ("format", "string"),
        // ("value", "string"))); ctx.Functions.Add(new BuiltinFunctionDefinition("print", "void",
        // ("value", "string"))); ctx.Functions.Add(new BuiltinFunctionDefinition("write", "void",
        // ("value", "string")));
        return base.VisitAssemblyDef(ctx);
    }

    /// <summary>
    /// Registers the Fifth.System KG wrapper types as globally available predeclared types.
    /// These types (graph, triple, store) can be used without importing Fifth.System.
    /// </summary>
    private void RegisterKGTypes()
    {
        // Register Graph type - both "Graph" (C# name) and "graph" (Fifth name)
        var graphType = typeof(Fifth.System.Graph);
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(graphType)
        { Name = TypeName.From(graphType.FullName) });
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(graphType)
        { Name = TypeName.From(graphType.Name) });
        // Lowercase binding for Fifth source code
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(graphType)
        { Name = TypeName.From("graph") });

        // Register Triple type - both "Triple" (C# name) and "triple" (Fifth name)
        var tripleType = typeof(Fifth.System.Triple);
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(tripleType)
        { Name = TypeName.From(tripleType.FullName) });
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(tripleType)
        { Name = TypeName.From(tripleType.Name) });
        // Lowercase binding for Fifth source code
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(tripleType)
        { Name = TypeName.From("triple") });

        // Register Store type - both "Store" (C# name) and "store" (Fifth name)
        var storeType = typeof(Fifth.System.Store);
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(storeType)
        { Name = TypeName.From(storeType.FullName) });
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(storeType)
        { Name = TypeName.From(storeType.Name) });
        // Lowercase binding for Fifth source code
        TypeRegistry.DefaultRegistry.Register(new FifthType.TDotnetType(storeType)
        { Name = TypeName.From("store") });
    }

    public FieldDef WrapField(FieldReflector fi)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(fi.FieldType, out var ft))
        {
            return new FieldDefBuilder()
                .WithName(MemberName.From(fi.Name))
                .WithTypeName(TypeName.From(fi.FieldType.Name))
                .Build();
        }

        return default;
    }

    public PropertyDef WrapProperty(PropertyReflector pi)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(pi.PropertyType, out var ft))
        {
            return new PropertyDefBuilder()
                .WithName(MemberName.From(pi.Name))
                .WithTypeName(TypeName.From(pi.PropertyType.Name))
                .Build();
        }

        return default;
    }

    private FunctionDef WrapMethod(MethodReflector mi)
    {
        if (!TypeRegistry.DefaultRegistry.TryLookupType(mi.ReturnType, out var returnType))
        {
            return default;
        }
        ;
        var builder = new FunctionDefBuilder()
            .WithName(MemberName.From(mi.Name))
            .WithReturnType(new TDotnetType(mi.ReturnType) { Name = TypeName.From(mi.ReturnType.Name) });

        foreach (var pi in mi.Parameters)
        {
            if (TypeRegistry.DefaultRegistry.TryLookupType(pi.ParamType, out var paramtype) && paramtype is FifthType.TDotnetType nt)
            {
                builder.AddingItemToParams(new ParamDefBuilder()
                                               .WithName(pi.Name)
                                               .WithTypeName(TypeName.From(nt.TheType.Name))
                                               .WithCollectionType(CollectionType.SingleInstance) // Default to single instance for now
                                               .Build()
                );
            }
        }

        var argsAsExpressions = mi.Parameters.Select(pi => new VarRefExpBuilder()
            .WithVarName(pi.Name)
            .Build());

        builder.WithBody(new BlockStatementBuilder()
                                     .AddingItemToStatements(
                                         new ExpStatementBuilder()
                                                                   .WithRHS(
                                                                       new FuncCallExpBuilder()
                                                                           .WithAnnotations(new Dictionary<string, object>())
                                                                           .Build() // func call exp
                                                                   ).Build() // expression statement
                                     )
                                     .Build() // block
        );
        return builder.Build();
    }

    private void WrapType(Type t)
    {
        if (TypeRegistry.DefaultRegistry.TryLookupType(t, out var itype))
        {
            return;
        }

        //var type = new TypeReflector(t);
        //var fields = from f in type.Fields select WrapField(f);
        //var properties = from p in type.Properties select WrapProperty(p);
        //var methods = from m in type.Methods where m.IsPublic && m.IsStatic select WrapMethod(m);

        //var builder = new ClassDefBuilder()
        //                    .WithName(TypeName.From(t.Name));
        //foreach (var fd in fields.Cast<MemberDef>())
        //{
        //    builder.AddingItemToMemberDefs(fd);
        //}
        //foreach (var fd in properties.Cast<MemberDef>())
        //{
        //    builder.AddingItemToMemberDefs(fd);
        //}
        //foreach (var fd in methods.Cast<MemberDef>())
        //{
        //    builder.AddingItemToMemberDefs(fd);
        //}
        // Register both full name and short name to allow simple qualifiers like 'KG'
        var full = new FifthType.TDotnetType(t) { Name = TypeName.From(t.FullName) };
        TypeRegistry.DefaultRegistry.Register(full);
        var shortName = new FifthType.TDotnetType(t) { Name = TypeName.From(t.Name) };
        TypeRegistry.DefaultRegistry.Register(shortName);
    }
}
