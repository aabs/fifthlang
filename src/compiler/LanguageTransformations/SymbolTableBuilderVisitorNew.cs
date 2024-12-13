
namespace compiler.LanguageTransformations;

public class SymbolTableBuilderVisitor : DefaultRecursiveDescentVisitor
{

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var enclosingScope = ctx.Parent.NearestScope();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.ClassDef), ctx, []);
        return base.VisitClassDef(ctx);
    }

    public override FieldDef VisitFieldDef(FieldDef ctx)
    {
        var enclosingScope = ctx.Parent.NearestScope();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.FieldDef), ctx, []);
        return base.VisitFieldDef(ctx);
    }

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        Declare(ctx.Name, SymbolKind.ParamDef, ctx);
        return base.VisitParamDef(ctx);
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        return base.VisitParamDestructureDef(ctx);
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        Declare(ctx.IntroducedVariable.Value, SymbolKind.PropertyBindingDef, ctx);
        return base.VisitPropertyBindingDef(ctx);
    }

    public override PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
        var enclosingScope = ctx.Parent.NearestScope();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.PropertyDef), ctx, []);
        return base.VisitPropertyDef(ctx);
    }

    public override TypeDef VisitTypeDef(TypeDef ctx)
    {
        return base.VisitTypeDef(ctx);
    }

    public override VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
    {
        return base.VisitVarDeclStatement(ctx);
    }

    public override VariableDecl VisitVariableDecl(VariableDecl ctx)
    {
        Declare(ctx.Name, SymbolKind.VarDeclStatement, ctx);
        return base.VisitVariableDecl(ctx);
    }

    private void Declare<T>(string name, SymbolKind kind, T ctx, params (string, object)[] properties)
        where T : AstThing
    {
        var enclosingScope = ctx.NearestScope();
        enclosingScope.Declare(new Symbol(name, kind), ctx, properties.ToDictionary(x => x.Item1, x => x.Item2));
    }

}
