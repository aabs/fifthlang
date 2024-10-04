using ast_model.Symbols;

namespace ast_model.TypeSystem;

public interface ITypeInference
{
    public IType Infer<T>(IScope scope, T node);

    public IType Infer(IAstThing node);
}
