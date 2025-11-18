using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Visitor that resolves generic type parameters and validates type arguments.
/// Implements User Story 1 (T027-T028): Type parameter resolution and validation.
/// </summary>
public class TypeParameterResolutionVisitor : DefaultRecursiveDescentVisitor
{
    /// <summary>
    /// Visit ClassDef to register type parameters in the scope.
    /// This enables type parameter references within the class to be resolved.
    /// </summary>
    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        // Register each type parameter in the class's scope (T023)
        foreach (var typeParam in ctx.TypeParameters)
        {
            // Type parameters are visible within the class scope
            ctx.Declare(
                new Symbol(typeParam.Name.Value, SymbolKind.TypeParameter),
                typeParam,
                new Dictionary<string, object>
                {
                    ["IsTypeParameter"] = true,
                    ["ScopeKind"] = "Class"
                }
            );
        }

        return base.VisitClassDef(ctx);
    }

    /// <summary>
    /// Visit FunctionDef to register function-level type parameters.
    /// Function type parameters shadow class type parameters with the same name.
    /// </summary>
    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        // Register each type parameter in the function's scope (T023, T039)
        foreach (var typeParam in ctx.TypeParameters)
        {
            // Function type parameters are visible within the function scope
            ctx.Declare(
                new Symbol(typeParam.Name.Value, SymbolKind.TypeParameter),
                typeParam,
                new Dictionary<string, object>
                {
                    ["IsTypeParameter"] = true,
                    ["ScopeKind"] = "Function"
                }
            );
        }

        return base.VisitFunctionDef(ctx);
    }

    /// <summary>
    /// Validate generic type instantiation (T028).
    /// Checks that the number of type arguments matches the number of type parameters.
    /// Reports GEN001 error if there's a mismatch.
    /// </summary>
    private bool ValidateTypeArgumentCount(TypeName genericTypeName, int expectedCount, int actualCount, SourceLocationMetadata? location)
    {
        if (expectedCount != actualCount)
        {
            // TODO: Report GEN001 diagnostic
            // For now, we'll just return false to indicate validation failure
            // Proper diagnostic reporting will be added in a future phase
            Console.Error.WriteLine(
                $"GEN001: Type '{genericTypeName.Value}' expects {expectedCount} type argument(s) but got {actualCount} " +
                $"at {location?.Filename ?? "unknown"}:{location?.Line ?? 0}:{location?.Column ?? 0}"
            );
            return false;
        }
        return true;
    }
}
