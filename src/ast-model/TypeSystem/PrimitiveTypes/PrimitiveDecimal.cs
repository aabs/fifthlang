namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveDecimal : PrimitiveNumeric
    {
        private PrimitiveDecimal()
        {
            Name = (TypeName)"decimal";
            Seniority = TypeCoertionSeniority.@decimal;
        }

        public static PrimitiveDecimal Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static decimal Add(decimal left, decimal right)
        {
            return left + right;
        }

        [Operation(Operator.ArithmeticDivide)]
        public static decimal Divide(decimal left, decimal right)
        {
            return left / right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static decimal Multiply(decimal left, decimal right)
        {
            return left * right;
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static decimal Subtract(decimal left, decimal right)
        {
            return left - right;
        }
    }
}
