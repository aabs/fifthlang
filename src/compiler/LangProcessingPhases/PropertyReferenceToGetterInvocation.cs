namespace Fifth.Parser.LangProcessingPhases;

#region

using AST;
using AST.Builders;
using AST.Visitors;

#endregion

public class PropertyReferenceToGetterInvocation : DefaultMutatorVisitor2<DummyContext>
{
    public override AstNode ProcessVariableReference(VariableReference node, DummyContext ctx)
    {
        // the aim of this is to substitute the original variable reference (a
        // type of expression) with an alternate expression that is a function
        // invocation on the getter method of the property.

        // IF1: Create a FuncCallExpression invoking the Getter/Setter of the property referenced
        // IF1.1: Work out whether varref is on lhs or rhs
        // IF1.2: If lhs, create setter invocation
        // IF1.2.1: this is part of an assignment statement. The parent
        // needs to be transformed. create func call, add in parent class
        // details, create expression to contain the rhs of the assignment
        // that this must be part of
        // IF1.3: If rhs, create getter invocation

        // First let's construct ourselves a functional call.
        // we need the property details, such as what the getter/setter is called
        if (node.SymTabEntry is not null &&
            node.SymTabEntry.Context is PropertyDefinition { GetAccessor: not null } pd)
        {
            var getter = pd.GetAccessor;
            return FuncCallExpressionBuilder.CreateFuncCallExpression()
                                     .WithName(getter.Name)
                                     .Build();
        }

        return base.ProcessVariableReference(node, ctx);
    }

    public override AstNode ProcessAssignmentStmt(AssignmentStmt node, DummyContext ctx)
    {
        // this will transform an assignment statement, where the left side is a
        // property variable reference, into a setter method invocation
        // expression statement. That is the assignment statement becomes an
        // expression statement for a funccall.
        if (node.VariableRef is VariableReference { SymTabEntry: not null } lhs &&
            lhs.SymTabEntry.Context is PropertyDefinition { SetAccessor: not null } pd)
        {
            var setter = pd.SetAccessor;
            var esb = ExpressionStatementBuilder.CreateExpressionStatement();
            var setterInvoke = FuncCallExpressionBuilder.CreateFuncCallExpression()
                                                        .WithName(setter.Name)
                                                        .WithActualParameters(
                                                            (ExpressionList)Process(node.Expression, ctx)) // i.e. 'value'
                                                        .Build();
            setter.CopyMetadataInto(setterInvoke);
            return esb.WithExpression(setterInvoke)
                      .Build();
        }

        return base.ProcessAssignmentStmt(node, ctx);
    }
}
