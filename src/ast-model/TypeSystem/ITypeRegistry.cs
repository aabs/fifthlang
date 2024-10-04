namespace ast_model.TypeSystem
{
    public interface ITypeRegistry
    {
        public bool RegisterType(IType type);

        public bool TryGetType(TypeId typeId, out IType type);

        public bool TrySetType(IType type, out TypeId typeId);
    }
}
