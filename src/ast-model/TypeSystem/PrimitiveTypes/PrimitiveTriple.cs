namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveTriple : PrimitiveAny
    {
        private PrimitiveTriple()
        {
            Name = TypeName.From("triple");
        }

        public static PrimitiveTriple Default { get; } = new();
    }
}
