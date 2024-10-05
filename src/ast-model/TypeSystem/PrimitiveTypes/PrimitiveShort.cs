namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveShort : PrimitiveNumeric
    {
        private PrimitiveShort()
        {
            Name = TypeName.From("short");
            Seniority = TypeCoertionSeniority.@short;
        }

        public static PrimitiveShort Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static short add_short_short(short left, short right)
        {
            return (short)(left + right);
        }

        [Operation(Operator.ArithmeticDivide)]
        public static short divide_short_short(short left, short right)
        {
            return (short)(left / right);
        }

        [Operation(Operator.ArithmeticGreaterThanOrEqual)]
        public static bool greater_than_or_equal_short_short(short left, short right)
        {
            return left >= right;
        }

        [Operation(Operator.ArithmeticGreaterThan)]
        public static bool greater_than_short_short(short left, short right)
        {
            return left > right;
        }

        [Operation(Operator.ArithmeticLessThanOrEqual)]
        public static bool less_than_or_equal_short_short(short left, short right)
        {
            return left <= right;
        }

        [Operation(Operator.ArithmeticLessThan)]
        public static bool less_than_short_short(short left, short right)
        {
            return left < right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static short multiply_short_short(short left, short right)
        {
            return (short)(left * right);
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static short subtract_short_short(short left, short right)
        {
            return (short)(left - right);
        }
    }
}
