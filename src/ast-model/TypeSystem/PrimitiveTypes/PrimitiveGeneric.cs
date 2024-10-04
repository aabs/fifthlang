namespace ast_model.TypeSystem.PrimitiveTypes
{
    public abstract class PrimitiveGeneric : PrimitiveAny, IGenericType
    {
        protected PrimitiveGeneric(bool isPrimitive, bool isNumeric, string name, params TypeId[] typeParameters)
        {
            GenericTypeParameters = typeParameters;
        }

        public TypeId[] GenericTypeParameters { get; set; }
    }
}
