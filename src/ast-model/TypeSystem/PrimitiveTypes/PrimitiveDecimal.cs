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

        [Operation(Operator.Add)]
        public static decimal Add(decimal left, decimal right)
        {
            return left + right;
        }

        [Operation(Operator.Divide)]
        public static decimal Divide(decimal left, decimal right)
        {
            return left / right;
        }

        [Operation(Operator.Multiply)]
        public static decimal Multiply(decimal left, decimal right)
        {
            return left * right;
        }

        [Operation(Operator.Subtract)]
        public static decimal Subtract(decimal left, decimal right)
        {
            return left - right;
        }
    }
}
