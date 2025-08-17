using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Handles destructuring parameter processing by resolving type scopes and linking property bindings
/// to their corresponding property definitions. This visitor focuses on establishing the structural
/// relationships between destructured parameters and their underlying types.
/// 
/// Architectural responsibility: Converting nested destructuring declarations into local variable 
/// declarations within function overloads. Does NOT handle constraint processing - that is handled 
/// by DestructuringPatternFlattenerVisitor.
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
                ResolutionScope.Pop();
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
            // currently the only place that sets this annotation is
            // PropertyBindingToVariableDeclarationTransformer.EnterDestructuringBinding when the
            // propertyDefinitionScope is a classdefinition
            var propdecl = db.ReferencedPropertyName;
            if (db.NearestScope().TryResolveByName(propdecl.Value, out var paramType))
            {
                ResolutionScope.Push((propdecl.Value, paramType));
            }
        }
        ParamDestructureDef result;
        try
        {
            result = base.VisitParamDestructureDef(ctx);
        }
        finally
        {
            ResolutionScope.Pop();
        }
        return result;
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
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

        return ctx;
    }
}
