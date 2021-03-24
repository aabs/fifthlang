namespace Fifth.AST
{
    using TypeSystem;

    public abstract class Statement : Expression
    {
        protected Statement(AstNode parentNode, IFifthType fifthType)
            : base(parentNode, fifthType)
        {
        }

        protected Statement(IFifthType fifthType)
            : base(fifthType)
        {
        }
    }
}
