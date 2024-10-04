using ast_model;

namespace ast;

public interface IAstThing : IVisitable
{
    IAstThing Parent { get; init; }
}
