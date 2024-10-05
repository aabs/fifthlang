namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveChar : PrimitiveAny
    {
        public static PrimitiveChar Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static string Add(char left, char right)
        {
            return $"{left}{right}";
        }
    }
}
