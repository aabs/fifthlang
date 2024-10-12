using ast_model;
using ast_model.TypeSystem;

namespace ast;

public interface IAstThing : IVisitable
{
    IAstThing? Parent { get; set; }
    FifthType Type { get; set; }
    SourceLocationMetadata? Location { get; set; }
}
