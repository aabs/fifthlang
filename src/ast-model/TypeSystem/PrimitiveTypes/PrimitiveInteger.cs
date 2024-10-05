namespace ast_model.TypeSystem.PrimitiveTypes
{
#pragma warning disable IDE1006 // Naming Styles

    public class PrimitiveInteger : PrimitiveNumeric
    {
        private PrimitiveInteger()
        {
            Name = TypeName.From("int");
            Seniority = TypeCoertionSeniority.integer;
        }

        public static PrimitiveInteger Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static int add_int_int(int left, int right)
        {
            return left + right;
        }

        [Operation(Operator.ArithmeticDivide)]
        public static int divide_int_int(int left, int right)
        {
            return left / right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static int multiply_int_int(int left, int right)
        {
            return left * right;
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static int subtract_int_int(int left, int right)
        {
            return left - right;
        }

        [Operation(Operator.ArithmeticGreaterThanOrEqual)]
        public static bool greater_than_or_equal_int_int(int left, int right)
        {
            return left >= right;
        }

        [Operation(Operator.ArithmeticLessThanOrEqual)]
        public static bool less_than_or_equal_int_int(int left, int right)
        {
            return left <= right;
        }

        [Operation(Operator.ArithmeticGreaterThan)]
        public static bool greater_than_int_int(int left, int right)
        {
            return left > right;
        }

        [Operation(Operator.ArithmeticLessThan)]
        public static bool less_than_int_int(int left, int right)
        {
            return left < right;
        }
    }

#pragma warning restore IDE1006 // Naming Styles
}
