namespace compiler.LanguageTransformations;

public class DestructuringPatternFlattenerVisitor : DefaultRecursiveDescentVisitor
{
    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        var statementGatherer = new PropertyBindingToVariableDeclarationTransformer();
        statementGatherer.Visit(ctx);
        return ctx with
        {
            Body = ctx.Body with
            {
                Statements = [.. statementGatherer.Statements, .. ctx.Body.Statements]
            }
        };
    }
}
