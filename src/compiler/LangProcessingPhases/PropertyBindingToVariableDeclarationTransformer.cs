namespace Fifth.Parser.LangProcessingPhases;

using System.Collections.Generic;
using System.Linq;
using AST.Visitors;
using Fifth.AST;
using Fifth.AST.Builders;
using Fifth.Symbols;
using Fifth.TypeSystem;
public class DummyContext
{
}

/// <summary>
/// Converts a property binding in a destructuring declaration, and converts it into a variable declaration within the method body.
/// </summary>
/// <seealso cref="Fifth.AST.Visitors.BaseAstVisitor" />
public class PropertyBindingToVariableDeclarationTransformer : BaseAstVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();
    public StatementListBuilder Statements { get; } = StatementListBuilder.CreateStatementList();

    /// <summary>
    ///   Takes a binding, and attempts to resolve the components of the binding. Adds a variable
    ///   declaration statement to the list to prepend to the body of the enclosing function.
    /// </summary>
    /// <param name="ctx">
    ///   the binding from which the information comes.
    /// </param>
    public override void EnterDestructuringBinding(DestructuringBinding ctx)
    {
        var b = VariableDeclarationStatementBuilder.CreateVariableDeclarationStatement()
            .WithName(ctx.Varname);
        // TODO Use symtab entry Kind to guide processing here.  it could be a class a paramdecl or something else here.

        var (scopeVarName, propertyDefinitionScope) = ResolutionScope.Peek();

        if (propertyDefinitionScope.SymbolKind == SymbolKind.ClassDeclaration && propertyDefinitionScope.Context is ClassDefinition c)
        {
            // if we get here, the paramdecl must be a top level one, or the propdecl clashes with a type name??
            // TODO:  Need to check how this can fail.
            // the propname of the ctx needs to be resolved as a propertydefinition of the class c
            var propdecl = c.Properties.FirstOrDefault(pd => pd.Name == ctx.Propname);
            if (propdecl != null)
            {
                ctx.PropDecl = propdecl;
                // if propdecl is not null, it means we know that the RHS of the assignment is var ref to <scopeVarName>.(propdecl.Name)
                var x = MemberAccessExpressionBuilder.CreateMemberAccessExpression()
                                             .WithLHS(VariableReferenceBuilder.CreateVariableReference()
                                                                              .WithName(scopeVarName).Build())
                                             .WithRHS(new VariableReference(propdecl.Name))
                                             .Build();
                b.WithExpression(x);
                b.WithUnresolvedTypeName(propdecl.TypeName);
            }
        }

        Statements.AddingItemToStatements(b.Build());
    }

    public override void EnterParameterDeclaration(ParameterDeclaration ctx)
    {
        //var paramType = ctx.NearestScope().Resolve(ctx.TypeName);
        //ResolutionScope.Push((ctx.ParameterName.Value, paramType));
    }

    public override void EnterDestructuringDeclaration(DestructuringDeclaration ctx)
    {
        // if we get here, it was because we entered the destrdecl of a paramdecl or a destrbinding
        // if it's a paramdecl, then add it's type directly as the resolution scope
        if (ctx.ParentNode is ParameterDeclaration pd)
        {
            var paramType = ctx.NearestScope().Resolve(pd.TypeName);
            ResolutionScope.Push((pd.ParameterName.Value, paramType));
        }
        else if (ctx.ParentNode is DestructuringBinding db)
        {
            var propdecl = (PropertyDefinition)db.PropDecl;
            var paramType = propdecl.NearestScope().Resolve(propdecl.TypeName);
            ResolutionScope.Push((/*propdecl.Name*/ db.Varname, paramType));

        }
        // if it's a destr binding, then get the prop decl that it relates to from the annotations of the destrbinding.
        base.EnterDestructuringDeclaration(ctx);
    }

    public override void LeaveDestructuringDeclaration(DestructuringDeclaration ctx)
    {
        ResolutionScope.Pop();
    }

    public override void LeaveDestructuringBinding(DestructuringBinding ctx)
    {
        //ResolutionScope.Pop();
    }
}
