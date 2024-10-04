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

        [Operation(Operator.Add)]
        public static float add_float_float(float left, float right)
        {
            return left + right;
        }

        [Operation(Operator.Divide)]
        public static float divide_float_float(float left, float right)
        {
            return left / right;
        }

        [Operation(Operator.Multiply)]
        public static float multiply_float_float(float left, float right)
        {
            return left * right;
        }

        [Operation(Operator.Subtract)]
        public static float subtract_float_float(float left, float right)
        {
            return left - right;
        }
    }
}
