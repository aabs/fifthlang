using ast;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Synthesizes constructors for classes that have no explicit constructors.
/// 1. Synthesizes a parameterless constructor IF all fields are optional (nullable or defaulted).
/// 2. Emits CTOR005 if required fields exist and no constructor is provided.
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

        // Check for required fields (fields/properties without default values)
        // In Fifth, all fields are currently required unless nullable (ending in ?)
        // or initialized (which grammar doesn't support yet for fields).
        var requiredFields = new List<string>();
        foreach (var member in result.MemberDefs)
        {
            if (member is FieldDef f)
            {
                if (!IsNullable(f.TypeName)) requiredFields.Add(f.Name.Value);
            }
            else if (member is PropertyDef p)
            {
                if (!IsNullable(p.TypeName)) requiredFields.Add(p.Name.Value);
            }
        }

        if (requiredFields.Any())
        {
            // FR-CTOR-005: If no constructors are declared and any required field lacks initialization,
            // compilation MUST emit a diagnostic requiring an explicit constructor.
            if (_diagnostics != null)
            {
                var fieldList = string.Join(", ", requiredFields);
                _diagnostics.Add(ConstructorDiagnostics.CannotSynthesizeConstructor(
                    result.Name.Value,
                    fieldList,
                    result.Location?.Filename));
            }

            // Do not synthesize any constructors
            return result;
        }

        var updatedMemberDefs = new List<MemberDef>(result.MemberDefs);

        // 1. Synthesize parameterless constructor
        // FR-CTOR-004: If no constructors are declared and all required fields are defaulted or nullable,
        // a parameterless constructor MUST be synthesized.
        var parameterlessCtor = CreateParameterlessConstructor(result);
        updatedMemberDefs.Add(parameterlessCtor);

        result = result with { MemberDefs = updatedMemberDefs };
        return result;
    }

    private bool IsNullable(TypeName typeName)
    {
        return typeName.Value.EndsWith("?");
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
}
