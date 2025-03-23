using ast_model.Symbols;

namespace compiler.LanguageTransformations;

public class DestructuringVisitor : DefaultRecursiveDescentVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        if (ctx.DestructureDef is null)
        {
            return ctx;
        }
        var paramType = ctx.NearestScope().Resolve(new Symbol(ctx.TypeName.Value, SymbolKind.ParamDef));
        ResolutionScope.Push((ctx.Name, paramType));
        ParamDef result;
        try
        {
            result = base.VisitParamDef(ctx);
        }
        finally
        {
            ResolutionScope.Pop();
        }
        return result;
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        if (ctx.Parent is ParamDef pd)
        {
            var paramType = ctx.NearestScope().Resolve(new Symbol(pd.TypeName.Value, SymbolKind.ParamDef));
            ResolutionScope.Push((pd.Name, paramType));
        }
        else if (ctx.Parent is PropertyBindingDef db)
        {
            // currently the only place that sets this annotation is
            // PropertyBindingToVariableDeclarationTransformer.EnterDestructuringBinding when the
            // propertyDefinitionScope is a classdefinition
            var propdecl = db.ReferencedPropertyName;
            var paramType = db.NearestScope().Resolve(new Symbol(propdecl.Value, SymbolKind.PropertyDef));
            ResolutionScope.Push((propdecl.Value, paramType));
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
