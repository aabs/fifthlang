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

        [Operation(Operator.Add)]
        public static short add_short_short(short left, short right)
        {
            return (short)(left + right);
        }

        [Operation(Operator.Divide)]
        public static short divide_short_short(short left, short right)
        {
            return (short)(left / right);
        }

        [Operation(Operator.GreaterThanOrEqual)]
        public static bool greater_than_or_equal_short_short(short left, short right)
        {
            return left >= right;
        }

        [Operation(Operator.GreaterThan)]
        public static bool greater_than_short_short(short left, short right)
        {
            return left > right;
        }

        [Operation(Operator.LessThanOrEqual)]
        public static bool less_than_or_equal_short_short(short left, short right)
        {
            return left <= right;
        }

        [Operation(Operator.LessThan)]
        public static bool less_than_short_short(short left, short right)
        {
            return left < right;
        }

        [Operation(Operator.Multiply)]
        public static short multiply_short_short(short left, short right)
        {
            return (short)(left * right);
        }

        [Operation(Operator.Subtract)]
        public static short subtract_short_short(short left, short right)
        {
            return (short)(left - right);
        }
    }
}
