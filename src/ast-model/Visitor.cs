namespace ast_model;

public interface IVisitor
{
    public void OnEnter<T>(T ctx);

    public void OnLeave<T>(T ctx);
}
