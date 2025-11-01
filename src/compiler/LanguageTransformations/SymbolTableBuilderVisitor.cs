using ast_model.Symbols;
namespace compiler.LanguageTransformations;

/// <summary>
/// Builds symbol tables for all scoped entities in the AST.
/// This visitor declares symbols in their appropriate scopes for later resolution.
/// 
/// Namespace Awareness (T020):
/// This visitor currently operates on a single module's symbol table. 
/// For multi-module namespace support, future enhancements would:
/// - Accept NamespaceScopeIndex from namespace resolution phase
/// - Flag duplicate symbols across modules in the same namespace  
/// - Preserve module-local shadow indicators for import precedence
/// - Emit namespace-qualified symbol names for cross-module references
/// 
/// The NamespaceImportResolverVisitor (which runs immediately after this)
/// validates namespace imports and prepares for symbol resolution across namespaces.
/// </summary>
public class SymbolTableBuilderVisitor : DefaultRecursiveDescentVisitor
{

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var enclosingScope = ctx.NearestScopeAbove();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.ClassDef), ctx, []);
        return base.VisitClassDef(ctx);
    }

    public override FieldDef VisitFieldDef(FieldDef ctx)
    {
        var enclosingScope = ctx.Parent.NearestScope();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.FieldDef), ctx, []);
        return base.VisitFieldDef(ctx);
    }

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        var enclosingScope = ctx.NearestScopeAbove();
        enclosingScope.Declare(new Symbol(ctx.Name.Value, SymbolKind.FunctionDef), ctx, []);
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
