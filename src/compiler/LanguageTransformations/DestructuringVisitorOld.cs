namespace Fifth.Parser.LangProcessingPhases;

using System.Collections.Generic;
using System.Linq;
using AST.Visitors;
using Fifth.AST;
using Fifth.AST.Builders;
using Fifth.Symbols;
using Fifth.TypeSystem;

/// <summary>
/// A visitor intended specifically to track the recurive definitions of Parameter Destructuring Definitions.
/// </summary>
public class DestructuringVisitorOld : BaseAstVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();

    public override void EnterDestructuringBinding(DestructuringBinding ctx)
    {
        var (scopeVarName, propertyDefinitionScope) = ResolutionScope.Peek();

        if (propertyDefinitionScope.SymbolKind == SymbolKind.ClassDeclaration && propertyDefinitionScope.Context is ClassDefinition c)
        {
            // the propname of the ctx needs to be resolved as a propertydefinition of the class c
            var propdecl = c.Properties.FirstOrDefault(pd => pd.Name == ctx.Propname);
            if (propdecl != null)
            {
                ctx.PropDecl = propdecl;
                // if propdecl is not null, it means we know that the RHS of the assignment is var
                // ref to <scopeVarName>.(propdecl.Name)
            }
        }
    }

    public override void EnterDestructuringDeclaration(DestructuringDeclaration ctx)
    {
        if (ctx.ParentNode is ParameterDeclaration pd)
        {
            var paramType = ctx.NearestScope().Resolve(pd.TypeName);
            ResolutionScope.Push((pd.ParameterName.Value, paramType));
        }
        else if (ctx.ParentNode is DestructuringBinding db)
        {
            // currently the only place that sets this annotation is
            // PropertyBindingToVariableDeclarationTransformer.EnterDestructuringBinding when the
            // propertyDefinitionScope is a classdefinition
            var propdecl = db.PropDecl;
            var paramType = propdecl.NearestScope().Resolve(propdecl.TypeName);
            ResolutionScope.Push((propdecl.Name, paramType));
        }
    }

    public override void EnterParameterDeclaration(ParameterDeclaration ctx)
    {
        if (ctx.DestructuringDecl == null)
        {
            return;
        }
        var paramType = ctx.NearestScope().Resolve(ctx.TypeName);
        ResolutionScope.Push((ctx.ParameterName.Value, paramType));
    }

    public override void LeaveDestructuringBinding(DestructuringBinding ctx)
    {
        base.LeaveDestructuringBinding(ctx);
    }

    public override void LeaveDestructuringDeclaration(DestructuringDeclaration ctx)
    {
        ResolutionScope.Pop();
    }

    public override void LeaveParameterDeclaration(ParameterDeclaration ctx)
    {
        if (ctx.DestructuringDecl == null)
        {
            return;
        }
        ResolutionScope.Pop();
    }
}
