namespace compiler.LanguageTransformations;

public class ClassCtorInserter : DefaultRecursiveDescentVisitor
{
    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var result = base.VisitClassDef(ctx);
        var f = new FunctionDefBuilder()
                    .WithName(MemberName.From("ctor"))
                    .WithIsStatic(false)
                    .WithIsConstructor(true)
                    .WithParams([])
                    .WithBody(new() { Statements = [] })
                    .Build();
        f.Parent = ctx;

        return result;
    }
}
