namespace ast_model;

public interface IVisitable
{
    void Accept(IVisitor visitor);
}
