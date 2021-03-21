namespace Fifth.AST
{
    using System;
    using System.Collections.Generic;
    using Fifth.Parser.LangProcessingPhases;

    public class ExpressionList : TypedAstNode
    {
        public List<Expression> Expressions { get; set; }

        public override void Accept(IAstVisitor visitor)
        {
            visitor.EnterExpressionList(this);
            foreach (var e in Expressions)
            {
                e.Accept(visitor);
            }
            visitor.LeaveExpressionList(this);
        }

        public ExpressionList(List<Expression> expressions, IFifthType fifthType) : base(fifthType)
        {
            _ = expressions ?? throw new ArgumentNullException(nameof(expressions));
            Expressions = expressions;
        }
    }
}