using ast_model.Symbols;

namespace ast_model.TypeSystem;

public interface ITypeChecker
{
    public void Check<T>(IScope scope, T node, IType type) { }
}
