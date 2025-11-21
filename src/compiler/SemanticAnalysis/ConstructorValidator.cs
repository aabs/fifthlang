using ast;
using compiler.LanguageTransformations;

namespace compiler.SemanticAnalysis;

/// <summary>
/// Validates constructor declarations and emits diagnostics for:
/// - CTOR009: Value return in constructor
/// - CTOR010: Forbidden modifiers (static, async, abstract, virtual, override, sealed)
/// </summary>
public class ConstructorValidator : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;

    public ConstructorValidator(List<Diagnostic>? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        // Visit the class to find constructors
        var result = base.VisitClassDef(ctx);

        // Check each method to see if it's a constructor
        foreach (var member in result.MemberDefs)
        {
            if (member is MethodDef methodDef && methodDef.FunctionDef?.IsConstructor == true)
            {
                ValidateConstructor(methodDef.FunctionDef, result.Name.Value);
            }
        }

        return result;
    }

    private void ValidateConstructor(FunctionDef constructor, string className)
    {
        // CTOR010: Check for forbidden modifiers
        // Note: IsStatic is already a property on FunctionDef
        // For now, we'll check if constructor is marked as static
        if (constructor.IsStatic)
        {
            _diagnostics?.Add(ConstructorDiagnostics.ForbiddenModifier(
                className,
                "static",
                source: constructor.Location?.OriginalText));
        }

        // TODO: Add checks for async, abstract, virtual, override, sealed when these modifiers are added to the metamodel

        // CTOR009: Check for value return statements
        ValidateNoValueReturn(constructor.Body, className, constructor.Location?.OriginalText);
    }

    private void ValidateNoValueReturn(BlockStatement? block, string className, string? source)
    {
        if (block == null) return;

        foreach (var statement in block.Statements)
        {
            if (statement is ReturnStatement returnStmt)
            {
                // Check if return statement has a value (value return)
                // ReturnStatement always has ReturnValue, but it could be an EmptyExpression or similar
                // For now, we'll emit the diagnostic for any return with a non-null ReturnValue
                // TODO: Check if ReturnValue is actually returning a meaningful value
                if (returnStmt.ReturnValue != null)
                {
                    _diagnostics?.Add(ConstructorDiagnostics.ValueReturnInConstructor(
                        className,
                        source));
                }
            }
            else if (statement is BlockStatement nestedBlock)
            {
                // Recursively check nested blocks (if statements, loops, etc.)
                ValidateNoValueReturn(nestedBlock, className, source);
            }
            else if (statement is IfElseStatement ifStmt)
            {
                ValidateNoValueReturn(ifStmt.ThenBlock, className, source);
                ValidateNoValueReturn(ifStmt.ElseBlock, className, source);
            }
            else if (statement is WhileStatement whileStmt)
            {
                ValidateNoValueReturn(whileStmt.Body, className, source);
            }
            else if (statement is ForStatement forStmt)
            {
                ValidateNoValueReturn(forStmt.Body, className, source);
            }
            else if (statement is ForeachStatement foreachStmt)
            {
                ValidateNoValueReturn(foreachStmt.Body, className, source);
            }
            else if (statement is TryStatement tryStmt)
            {
                ValidateNoValueReturn(tryStmt.TryBlock, className, source);
                foreach (var catchClause in tryStmt.CatchClauses)
                {
                    ValidateNoValueReturn(catchClause.Body, className, source);
                }
                ValidateNoValueReturn(tryStmt.FinallyBlock, className, source);
            }
        }
    }
}
