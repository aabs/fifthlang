using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Handles destructuring parameter processing by resolving type scopes and linking property bindings
/// to their corresponding property definitions. This visitor focuses on establishing the structural
/// relationships between destructured parameters and their underlying types.
/// 
/// Architectural responsibility: Resolves property references in destructuring patterns by linking
/// PropertyBindingDef nodes to their corresponding PropertyDef nodes. This is a prerequisite for
/// the DestructuringLoweringRewriter which performs the actual lowering to variable declarations.
/// </summary>
public class DestructuringVisitor : DefaultRecursiveDescentVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        if (ctx.DestructureDef is null)
        {
            return ctx;
        }
        ParamDef? result = null;
        if (ctx.NearestScope().TryResolveByName(ctx.TypeName.Value, out var paramType))
        {
            ResolutionScope.Push((ctx.Name, paramType));
            try
            {
                result = base.VisitParamDef(ctx);
            }
            finally
            {
                if (ResolutionScope.Count > 0) ResolutionScope.Pop();
            }
        }
        return result ?? ctx;
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        if (ctx.Parent is ParamDef pd)
        {
            if (ctx.NearestScope().TryResolveByName(pd.TypeName.Value, out var paramType))
            {
                ResolutionScope.Push((pd.Name, paramType));
            }
        }
        else if (ctx.Parent is PropertyBindingDef db)
        {
            // For nested destructuring, set scope to the TYPE of the referenced property
            // Prefer the already-resolved property reference; fall back to resolving by name in current class scope
            string? propertyTypeName = db.ReferencedProperty?.TypeName.Value;

            if (propertyTypeName is null && ResolutionScope.Count > 0)
            {
                var (_, currentScope) = ResolutionScope.Peek();
                if (currentScope.Symbol.Kind == SymbolKind.ClassDef && currentScope.OriginatingAstThing is ClassDef c)
                {
                    var prop = c.MemberDefs.OfType<PropertyDef>().FirstOrDefault(p => p.Name == db.ReferencedPropertyName);
                    propertyTypeName = prop?.TypeName.Value;
                }
            }

            if (propertyTypeName != null && ctx.NearestScope().TryResolveByName(propertyTypeName, out var propertyTypeSymbol))
            {
                // Use introduced variable name for clarity; the value isn't used elsewhere currently
                var scopeVar = db.IntroducedVariable.Value;
                ResolutionScope.Push((scopeVar, propertyTypeSymbol));
            }
        }
        ParamDestructureDef result;
        try
        {
            result = base.VisitParamDestructureDef(ctx);
        }
        finally
        {
            if (ResolutionScope.Count > 0) ResolutionScope.Pop();
        }
        return result;
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        if (ResolutionScope.Count == 0)
        {
            return ctx;
        }
        var (scopeVarName, propertyDefinitionScope) = ResolutionScope.Peek();

        if (propertyDefinitionScope.Symbol.Kind == SymbolKind.ClassDef && propertyDefinitionScope.OriginatingAstThing is ClassDef c)
        {
            // the propname of the ctx needs to be resolved as a propertydefinition of the class c
            var propdecl = c.MemberDefs.OfType<PropertyDef>().FirstOrDefault(pd => pd.Name == ctx.ReferencedPropertyName);
            if (propdecl != null)
            {
                ctx.ReferencedProperty = propdecl;
                // if propdecl is not null, it means we know that the RHS of the assignment is var
                // ref to <scopeVarName>.(propdecl.Name)
            }
        }

        // Continue traversal so nested destructuring (ctx.DestructureDef) can be processed
        return base.VisitPropertyBindingDef(ctx);
    }
}
