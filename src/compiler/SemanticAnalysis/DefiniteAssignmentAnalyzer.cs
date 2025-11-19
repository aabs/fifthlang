using ast;
using compiler.LanguageTransformations;

namespace compiler.SemanticAnalysis;

/// <summary>
/// Analyzes constructors to ensure all required fields are definitely assigned before construction completes.
/// Emits CTOR003 diagnostic when required fields remain unassigned.
/// 
/// Note: This is a simplified implementation for Phase 4. Full definite assignment analysis
/// requires control flow analysis and tracking assignments through all paths.
/// </summary>
public class DefiniteAssignmentAnalyzer : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public DefiniteAssignmentAnalyzer()
    {
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var result = base.VisitClassDef(ctx);

        // Check each constructor in the class
        foreach (var member in result.MemberDefs)
        {
            if (member is MethodDef methodDef && methodDef.FunctionDef?.IsConstructor == true)
            {
                AnalyzeConstructor(methodDef.FunctionDef, result);
            }
        }

        return result;
    }

    private void AnalyzeConstructor(FunctionDef constructor, ClassDef containingClass)
    {
        // Get required fields for the class
        var requiredFields = GetRequiredFields(containingClass);

        if (!requiredFields.Any())
        {
            // No required fields, nothing to check
            return;
        }

        // TODO: Full implementation would track assignments through control flow
        // For now, this is a placeholder for Phase 4 infrastructure
        // Proper analysis requires:
        // 1. Control flow graph construction
        // 2. Field assignment tracking through all paths
        // 3. Verification that all paths assign all required fields
    }

    /// <summary>
    /// Gets the list of required fields (non-nullable) for a class.
    /// Matches the logic in ClassCtorInserter.
    /// </summary>
    private static List<FieldDef> GetRequiredFields(ClassDef classDef)
    {
        var requiredFields = new List<FieldDef>();

        foreach (var member in classDef.MemberDefs)
        {
            if (member is FieldDef field)
            {
                // A field is required if it's non-nullable
                var isNullable = field.TypeName.Value.EndsWith("?");
                if (!isNullable)
                {
                    requiredFields.Add(field);
                }
            }
        }

        return requiredFields;
    }
}
