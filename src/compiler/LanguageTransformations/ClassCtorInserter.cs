namespace Fifth.LangProcessingPhases;

using System.Collections.Generic;
using AST;
using AST.Builders;
using AST.Visitors;
using fifth.metamodel.metadata;

public class ClassCtorInserter : DefaultRecursiveDescentVisitor
{

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var result = base.VisitClassDef(ctx);
        var f = new FunctionDefBuilder()
                    .WithName(MemberName.From( "ctor"))
                    .WithIsStatic(false)
                    .WithIsConstructor(true)
                    .WithParams([])
                    .WithBody(new(){Statements = []})
                    .Build();
        f.Parent = ctx;

        return result;
    }
}
