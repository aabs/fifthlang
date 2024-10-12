namespace ast_model.TypeSystem
{
    public interface FifthTypeRegistry
    {
        public bool RegisterType(FifthType type);

        public bool TryGetType(TypeId typeId, out FifthType type);

        public bool TrySetType(FifthType type, out TypeId typeId);
    }
}
