namespace ast_model.TypeSystem;

public interface ITypeIgnore
{
    TypeId Id { get; init; }
    Symbol Symbol { get; init; }
    FifthType[] ParentTypes { get; init; }
    FifthType[] TypeArguments { get; init; }
    bool IsArray { get; init; }
}
