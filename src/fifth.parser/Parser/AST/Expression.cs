using Fifth.VirtualMachine;

namespace Fifth.AST
{
    public abstract class Expression : AstNode
    {
        public IFifthType FifthType { get; set; }
    }
}
