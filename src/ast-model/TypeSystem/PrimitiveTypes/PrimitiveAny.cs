namespace ast_model.TypeSystem.PrimitiveTypes;

public abstract class PrimitiveAny : IAstThing
{
    public TypeName Name { get; set; }
    public NamespaceName Namespace { get; set; }
    public TypeId TypeId { get; init; }
    public IAstThing? Parent { get; set; }
    public FifthType Type { get; set; }
    public SourceLocationMetadata? Location { get; set; }

    public void Accept(IVisitor visitor)
    {
        throw new NotImplementedException();
    }
}
