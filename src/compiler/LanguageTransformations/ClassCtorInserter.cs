using ast;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Synthesizes constructors for classes that have no explicit constructors.
/// 1. Always synthesizes a parameterless constructor (initializing fields to defaults).
/// 2. Synthesizes a full constructor taking all fields/properties if any exist.
/// </summary>
public class ClassCtorInserter : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;

    public ClassCtorInserter(List<Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var result = base.VisitClassDef(ctx);

        // Check if the class already has constructors
        var hasExplicitConstructor = result.MemberDefs
            .OfType<MethodDef>()
            .Any(m => m.FunctionDef?.IsConstructor == true);

        if (hasExplicitConstructor)
        {
            // Class has explicit constructors, no synthesis needed
            return result;
        }

        var updatedMemberDefs = new List<MemberDef>(result.MemberDefs);

        // 1. Synthesize parameterless constructor
        // We always generate this to support 'new Class()' even if fields are required.
        // This aligns with C# default constructor behavior and allows object initializers.
        var parameterlessCtor = CreateParameterlessConstructor(result);
        updatedMemberDefs.Add(parameterlessCtor);

        // 2. Synthesize full constructor if there are fields/properties
        var membersToInitialize = GetMembersToInitialize(result);
        if (membersToInitialize.Any())
        {
            var fullCtor = CreateFullConstructor(result, membersToInitialize);
            updatedMemberDefs.Add(fullCtor);
        }

        result = result with { MemberDefs = updatedMemberDefs };
        return result;
    }

    private MethodDef CreateParameterlessConstructor(ClassDef classDef)
    {
        var syntheticConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From(classDef.Name.Value))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType() { Name = TypeName.From("void") })
            .WithBody(new BlockStatement
            {
                Statements = [],
                Annotations = [],
                Location = classDef.Location,
                Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        syntheticConstructor = syntheticConstructor with
        {
            Parent = classDef,
            Location = classDef.Location
        };

        return new MethodDef
        {
            Name = MemberName.From(classDef.Name.Value),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = syntheticConstructor,
            Location = classDef.Location,
            Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
        };
    }

    private List<MemberDef> GetMembersToInitialize(ClassDef classDef)
    {
        var members = new List<MemberDef>();
        foreach (var member in classDef.MemberDefs)
        {
            if (member is FieldDef || member is PropertyDef)
            {
                members.Add(member);
            }
        }
        return members;
    }

    private MethodDef CreateFullConstructor(ClassDef classDef, List<MemberDef> members)
    {
        var paramsList = new List<ParamDef>();
        var statements = new List<Statement>();

        foreach (var member in members)
        {
            var memberName = member.Name.Value;
            var paramName = memberName.ToLowerInvariant(); // Simple convention: lowercase param name
            if (paramName == memberName) paramName = "_" + paramName; // Avoid conflict if case-insensitive match

            TypeName typeName;
            CollectionType collectionType;
            FifthType type;

            if (member is FieldDef f)
            {
                typeName = f.TypeName;
                collectionType = f.CollectionType;
                type = f.Type ?? new FifthType.TType { Name = typeName }; // Fallback if Type is null
            }
            else if (member is PropertyDef p)
            {
                typeName = p.TypeName;
                collectionType = p.CollectionType;
                type = p.Type ?? new FifthType.TType { Name = typeName };
            }
            else continue;

            // Create parameter
            var param = new ParamDefBuilder()
                .WithName(paramName)
                .WithTypeName(typeName)
                .WithCollectionType(collectionType)
                .WithAnnotations([])
                .Build();

            // Fix up param type (Builder might not set it fully)
            param = param with { Type = type };

            paramsList.Add(param);

            // Create assignment: this.Member = param
            var lhs = new MemberAccessExp
            {
                LHS = new VarRefExp { VarName = "this", Location = classDef.Location, Type = new FifthType.TType { Name = TypeName.From(classDef.Name.Value) } },
                RHS = new VarRefExp { VarName = memberName, Location = classDef.Location, Type = type },
                Location = classDef.Location,
                Type = type
            };

            var rhs = new VarRefExp
            {
                VarName = paramName,
                Location = classDef.Location,
                Type = type
            };

            var assignment = new AssignmentStatement
            {
                LValue = lhs,
                RValue = rhs,
                Location = classDef.Location,
                Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
            };

            statements.Add(assignment);
        }

        var syntheticConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From(classDef.Name.Value))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams(paramsList)
            .WithReturnType(new FifthType.TVoidType() { Name = TypeName.From("void") })
            .WithBody(new BlockStatement
            {
                Statements = statements,
                Annotations = [],
                Location = classDef.Location,
                Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        syntheticConstructor = syntheticConstructor with
        {
            Parent = classDef,
            Location = classDef.Location
        };

        return new MethodDef
        {
            Name = MemberName.From(classDef.Name.Value),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = syntheticConstructor,
            Location = classDef.Location,
            Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
        };
    }
}
