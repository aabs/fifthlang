using ast_model.Symbols;

namespace compiler.LanguageTransformations;

public class PropertyBindingToVariableDeclarationTransformer : DefaultRecursiveDescentVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();
    public List<Statement> Statements { get; } = [];

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        return base.VisitParamDef(ctx);
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        // if we get here, it was because we entered the destrdecl of a paramdecl or a destrbinding
        // if it's a paramdecl, then add it's type directly as the resolution scope
        if (ctx.Parent is ParamDef pd)
        {
            var paramType = ctx.NearestScope().ResolveByName(pd.TypeName.Value);
            ResolutionScope.Push((pd.Name, paramType));
        }
        else if (ctx.Parent is PropertyBindingDef db)
        {
            var propdeclname = db.ReferencedPropertyName;
            if (db.ReferencedProperty is null && ResolutionScope.Peek().Item2.OriginatingAstThing is ScopeAstThing scope)
            {
                // we need to resolve the property that the db.ReferencedPropertyName refers to
                var x = scope.ResolveByName(db.ReferencedPropertyName.Value);
                ResolutionScope.Push((db.IntroducedVariable.Value, x));
            }
            else
            {
                var paramType = db.ReferencedProperty.NearestScope().ResolveByName(db.ReferencedProperty.TypeName.Value);
                ResolutionScope.Push((/*propdecl.Name*/ db.IntroducedVariable.Value, paramType));
            }
        }
        try
        {
            return base.VisitParamDestructureDef(ctx);
        }
        finally
        {
            ResolutionScope.Pop();
        }
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        // TODO Use symtab entry Kind to guide processing here. it could be a class a paramdecl or
        // something else here.

        var (scopeVarName, propertyDefinitionScope) = ResolutionScope.Peek();

        if (propertyDefinitionScope.Symbol.Kind == SymbolKind.ClassDef && propertyDefinitionScope.OriginatingAstThing is ClassDef c)
        {
            // if we get here, the paramdecl must be a top level one, or the propdecl clashes with a
            // type name??
            // TODO:  Need to check how this can fail. the propname of the ctx needs to be resolved
            // as a propertydefinition of the class c
            var propdecl = c.MemberDefs.OfType<PropertyDef>().FirstOrDefault(pd => pd.Name == ctx.ReferencedPropertyName);
            if (propdecl != null)
            {
                ctx.ReferencedProperty = propdecl;
                // if propdecl is not null, it means we know that the RHS of the assignment is var
                // ref to <scopeVarName>.(propdecl.Name)
                var vds = new VarDeclStatementBuilder()
                    .WithVariableDecl(
                        new VariableDeclBuilder()
                            .WithName(ctx.IntroducedVariable.Value)
                            .WithTypeName(ctx.ReferencedProperty.TypeName)
                            .WithVisibility(Visibility.Private)
                            .WithCollectionType(CollectionType.SingleInstance)
                            .Build()
                    )
                    .WithInitialValue(
                        new VarRefExpBuilder()
                        .WithVarName(propdecl.Name.Value)
                        .Build()
                    )
                    .Build();
                Statements.Add(vds);
            }
        }

        return base.VisitPropertyBindingDef(ctx);
    }
}
