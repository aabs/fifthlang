using ast;
using ast_generated;
using ast_model.Symbols;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Variable reference resolver visitor that converts VarRefExp nodes 
/// from unresolved to resolved by finding their corresponding VariableDecl
/// through symbol table lookup.
/// </summary>
/// <remarks>
/// This visitor replaces the original VariableReferenceResolver which used 
/// the old BaseAstVisitor pattern. It uses the new DefaultRecursiveDescentVisitor
/// approach and works with VarRefExp nodes, resolving their VariableDecl property
/// by performing symbol table lookups.
/// </remarks>
public class VarRefResolverVisitor : DefaultRecursiveDescentVisitor
{
    /// <summary>
    /// Attempts to resolve a variable reference by name within the given scope.
    /// </summary>
    /// <param name="varName">The name of the variable to resolve</param>
    /// <param name="scope">The scope context to search within</param>
    /// <param name="resolvedDecl">The resolved variable declaration if found</param>
    /// <returns>True if the variable was successfully resolved, false otherwise</returns>
    public bool TryResolve(string varName, ScopeAstThing scope, out VariableDecl? resolvedDecl)
    {
        resolvedDecl = null;
        
        if (string.IsNullOrEmpty(varName) || scope == null)
        {
            return false;
        }

        var symbol = new Symbol(varName, SymbolKind.VarDeclStatement);
        
        if (scope.TryResolve(symbol, out var symbolTableEntry))
        {
            if (symbolTableEntry?.OriginatingAstThing is VariableDecl variableDecl)
            {
                resolvedDecl = variableDecl;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Visits a VarRefExp node and attempts to resolve its VariableDecl property
    /// if it's not already resolved.
    /// </summary>
    /// <param name="ctx">The VarRefExp node to process</param>
    /// <returns>A new VarRefExp with the VariableDecl resolved if possible</returns>
    public override VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
        // First visit children using base implementation
        var result = base.VisitVarRefExp(ctx);
        
        // If already resolved, nothing to do
        if (result.VariableDecl != null)
        {
            return result;
        }

        // Find the nearest scope
        var nearestScope = ctx.NearestScope();
        if (nearestScope == null)
        {
            return result;
        }

        // Try to resolve the variable reference
        if (TryResolve(result.VarName, nearestScope, out var resolvedDecl))
        {
            return result with { VariableDecl = resolvedDecl };
        }

        // Return unmodified if resolution failed
        return result;
    }
}