namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveLong : PrimitiveNumeric
    {
        private PrimitiveLong()
        {
            Name = TypeName.From("long");
            Seniority = TypeCoertionSeniority.@long;
        }

        public static PrimitiveLong Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static long Add(long left, long right)
        {
            return left + right;
        }

        [Operation(Operator.ArithmeticDivide)]
        public static long Divide(long left, long right)
        {
            return left / right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static long Multiply(long left, long right)
        {
            return left * right;
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static long Subtract(long left, long right)
        {
            return left - right;
        }
    }
}
