using ast_model.Symbols;

namespace ast_model.TypeSystem;

public interface FifthTypeInference
{
    public FifthType Infer<T>(IScope scope, T node);

    public FifthType Infer(IAstThing node);
}
