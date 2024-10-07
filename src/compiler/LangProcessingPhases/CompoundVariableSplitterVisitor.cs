namespace Fifth.LangProcessingPhases;

using System.Linq;
using AST;
using AST.Visitors;

/*
public class CompoundVariableSplitterVisitor : BaseAstVisitor
{
    public override void EnterVariableReference(VariableReference ctx)
    {
        var separator = '.';
        var names = ctx.Name.Split(separator);
        if (names.Length > 1)
        {
            var vars = names.Select(n => new VariableReference(n)).ToList();
            vars.ForEach(v =>
            {
                v.Filename = ctx.Filename;
                v.Column = ctx.Column;
                v.Line = ctx.Line;
                v.OriginalText = ctx.OriginalText;
                v.ParentNode = v;
            });
            _ = Transplant(ctx, new CompoundVariableReference(vars));
        }
    }

    private bool Transplant(VariableReference vr, CompoundVariableReference cvr)
    {
        var p = vr.ParentNode;
        cvr.ParentNode = p;
        if (p is BinaryExpression be)
        {
            if (vr == be.Left)
            {
                be.Left = cvr;
                return true;
            }

            if (vr == be.Right)
            {
                be.Right = cvr;
                return true;
            }
        }

        if (p is AssignmentStmt ass)
        {
            if (vr == ass.VariableRef)
            {
                ass.VariableRef = cvr;
                return true;
            }

            if (vr == ass.Expression)
            {
                ass.Expression = cvr;
                return true;
            }
        }

        if (p is ReturnStatement rs)
        {
            rs.SubExpression = cvr;
            return true;
        }

        return false;
    }
}
*/
