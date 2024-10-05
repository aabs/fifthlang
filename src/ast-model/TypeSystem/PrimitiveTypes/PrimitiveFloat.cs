namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveFloat : PrimitiveNumeric
    {
        private PrimitiveFloat()
        {
            Name = TypeName.From("float");
            Seniority = TypeCoertionSeniority.@float;
        }

        public static PrimitiveFloat Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static float add_float_float(float left, float right)
        {
            return left + right;
        }

        [Operation(Operator.ArithmeticDivide)]
        public static float divide_float_float(float left, float right)
        {
            return left / right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static float multiply_float_float(float left, float right)
        {
            return left * right;
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static float subtract_float_float(float left, float right)
        {
            return left - right;
        }
    }
}
