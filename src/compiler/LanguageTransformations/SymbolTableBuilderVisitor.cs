using ast_model.Symbols;
namespace compiler.LanguageTransformations;

public class SymbolTableBuilderVisitor : DefaultRecursiveDescentVisitor
{
    private string _currentNamespace = string.Empty;

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        var previous = _currentNamespace;
        _currentNamespace = ctx.NamespaceDecl.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_currentNamespace) || _currentNamespace.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
        {
            _currentNamespace = string.Empty;
        }

        var result = base.VisitModuleDef(ctx);
        _currentNamespace = previous;
        return result;
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var enclosingScope = ctx.NearestScopeAbove();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.ClassDef), ctx, BuildAnnotations(ctx.Name.Value));
        return base.VisitClassDef(ctx);
    }

    public override FieldDef VisitFieldDef(FieldDef ctx)
    {
        var enclosingScope = ctx.Parent.NearestScope();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.FieldDef), ctx, BuildAnnotations(ctx.Name.Value));
        return base.VisitFieldDef(ctx);
    }

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        var enclosingScope = ctx.NearestScopeAbove();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.FunctionDef), ctx, BuildAnnotations(ctx.Name.Value));
        return base.VisitFunctionDef(ctx);
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
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.PropertyDef), ctx, BuildAnnotations(ctx.Name.Value));
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
        var annotations = BuildAnnotations(name);
        foreach (var (key, value) in properties)
        {
            annotations[key] = value;
        }
        enclosingScope.Declare(new Symbol(name, kind), ctx, annotations);
    }

    private Dictionary<string, object> BuildAnnotations(string symbolName)
    {
        var qualifiedName = string.IsNullOrWhiteSpace(_currentNamespace)
            ? symbolName
            : $"{_currentNamespace}.{symbolName}";

        return new Dictionary<string, object>
        {
            ["QualifiedName"] = qualifiedName,
            ["IsImported"] = false,
            ["IsLocalShadow"] = false
        };
    }

}
