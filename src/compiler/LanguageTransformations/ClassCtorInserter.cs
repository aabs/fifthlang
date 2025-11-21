using ast;

namespace compiler.LanguageTransformations;

/// <summary>
/// Synthesizes parameterless constructors for classes that:
/// 1. Have no explicit constructors
/// 2. Have all required fields defaulted or nullable
/// Emits CTOR005 diagnostic if synthesis is not possible.
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

        // No explicit constructors - check if we can synthesize one
        var requiredFields = GetRequiredFields(result);
        
        if (requiredFields.Any())
        {
            // Cannot synthesize - required fields lack defaults
            var fieldList = string.Join(", ", requiredFields.Select(f => f.Name));
            _diagnostics?.Add(ConstructorDiagnostics.CannotSynthesizeConstructor(
                result.Name.Value,
                fieldList,
                source: null));
            
            // Return without synthesizing
            return result;
        }

        // Can synthesize parameterless constructor
        var syntheticConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From(result.Name.Value))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType() { Name = TypeName.From("void") })
            .WithBody(new BlockStatement 
            { 
                Statements = [],
                Annotations = [],
                Location = result.Location,
                Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        syntheticConstructor = syntheticConstructor with 
        { 
            Parent = result,
            Location = result.Location 
        };

        // Wrap in MethodDef and add to class
        var methodDef = new MethodDef
        {
            Name = MemberName.From(result.Name.Value),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = syntheticConstructor,
            Location = result.Location,
            Type = new FifthType.TVoidType() { Name = TypeName.From("void") }
        };

        // Add the synthetic constructor to the class
        var updatedMemberDefs = new List<MemberDef>(result.MemberDefs) { methodDef };
        result = result with { MemberDefs = updatedMemberDefs };

        return result;
    }

    /// <summary>
    /// Gets the list of required fields (non-nullable, no default) for a class
    /// </summary>
    private static List<FieldDef> GetRequiredFields(ClassDef classDef)
    {
        var requiredFields = new List<FieldDef>();

        foreach (var member in classDef.MemberDefs)
        {
            if (member is FieldDef field)
            {
                // A field is required if it's non-nullable and has no default value
                // For now, we'll consider all fields as required unless they're explicitly nullable
                // TODO: Implement proper nullable detection and default value checking
                // This is a simplified implementation for Phase 2
                var isNullable = field.TypeName.Value.EndsWith("?");
                if (!isNullable)
                {
                    // Field is required
                    requiredFields.Add(field);
                }
            }
            else if (member is PropertyDef property)
            {
                // Properties with backing fields should also be checked
                // For now, we'll treat properties similarly to fields
                var isNullable = property.TypeName.Value.EndsWith("?");
                if (!isNullable)
                {
                    // Property is required (has a backing field that needs initialization)
                    // We'll approximate this by treating non-nullable properties as requiring a backing field
                    // TODO: Refine this logic to check for auto-properties vs. computed properties
                }
            }
        }

        return requiredFields;
    }
}
