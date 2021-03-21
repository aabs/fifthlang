namespace Fifth.AST
{
    using Parser.LangProcessingPhases;

    public class TypeCreateInstExpression : Expression
    {
        public TypeCreateInstExpression(AstNode parentNode, IFifthType fifthType)
            : base(parentNode, fifthType)
        {
        }

        public override void Accept(IAstVisitor visitor)
        {
            visitor.EnterTypeCreateInstExpression(this);
            visitor.LeaveTypeCreateInstExpression(this);
        }
    }
}