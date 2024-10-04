namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveString : PrimitiveAny
    {
        private PrimitiveString() 
        {
            Name = TypeName.From("string");
        }

        public static PrimitiveString Default { get; } = new();

        [Operation(Operator.Add)]
        public static string add_string_string(string left, string right)
        {
            return left + right;
        }
    }
}
