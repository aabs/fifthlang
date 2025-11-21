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
            if (member is MethodDef methodDef && methodDef.FunctionDef.IsConstructor)
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

        // Track which required fields are assigned in this constructor
        var assignedFields = new HashSet<string>();
        
        // Analyze constructor body for field assignments
        if (constructor.Body != null)
        {
            TrackFieldAssignments(constructor.Body, assignedFields);
        }

        // Check if any required fields are unassigned
        var unassignedFields = requiredFields
            .Where(f => !assignedFields.Contains(f.Name.Value))
            .ToList();

        if (unassignedFields.Any())
        {
            var fieldNames = string.Join(", ", unassignedFields.Select(f => f.Name.Value));
            _diagnostics.Add(ConstructorDiagnostics.UnassignedRequiredFields(
                containingClass.Name.Value,
                fieldNames,
                $"{containingClass.Name.Value}::{constructor.Name.Value}"));
        }
    }

    /// <summary>
    /// Tracks field assignments in the constructor body.
    /// Simplified implementation: looks for `this.Field = value` assignments.
    /// Full CFG-based analysis would track all code paths.
    /// </summary>
    private void TrackFieldAssignments(BlockStatement body, HashSet<string> assignedFields)
    {
        if (body.Statements == null)
        {
            return;
        }

        foreach (var statement in body.Statements)
        {
            TrackFieldAssignmentsInStatement(statement, assignedFields);
        }
    }

    private void TrackFieldAssignmentsInStatement(Statement statement, HashSet<string> assignedFields)
    {
        switch (statement)
        {
            case AssignmentStatement assignment:
                // Check if this is an assignment to a field (this.FieldName = value)
                if (assignment.LValue is MemberAccessExp memberAccess && 
                    memberAccess.LHS is VarRefExp varRef &&
                    varRef.VarName == "this" &&
                    memberAccess.RHS is VarRefExp fieldRef)
                {
                    assignedFields.Add(fieldRef.VarName);
                }
                break;

            case IfElseStatement ifStmt:
                // For if statements, a field is only considered assigned if it's assigned in ALL branches
                // Simplified: we track assignments but don't enforce all-paths requirement
                // Full implementation would require CFG analysis
                TrackFieldAssignments(ifStmt.ThenBlock, assignedFields);
                TrackFieldAssignments(ifStmt.ElseBlock, assignedFields);
                break;

            case WhileStatement whileStmt:
                TrackFieldAssignments(whileStmt.Body, assignedFields);
                break;
                
            case BlockStatement blockStmt:
                TrackFieldAssignments(blockStmt, assignedFields);
                break;
        }
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
