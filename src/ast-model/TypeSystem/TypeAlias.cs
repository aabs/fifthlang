namespace ast_model.TypeSystem
{
    public record TypeAlias : ITypeAlias
    {
        public TypeId AliasedTypeId { get; init; }
        public TypeName Name { get; init; }
        public NamespaceName Namespace { get; init; }
        public TypeId TypeId { get; init; }
    }
}
