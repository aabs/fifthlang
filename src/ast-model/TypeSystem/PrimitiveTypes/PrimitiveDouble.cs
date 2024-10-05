namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveDouble : PrimitiveNumeric
    {
        private PrimitiveDouble()
        {
            Name = (TypeName)"double";
            Seniority = TypeCoertionSeniority.@double;
        } 

        public static PrimitiveDouble Default { get; set; } = new();

        [Operation(Operator.ArithmeticAdd)]
        public static double Add(double left, double right)
        {
            return left + right;
        }

        [Operation(Operator.ArithmeticDivide)]
        public static double Divide(double left, double right)
        {
            return left / right;
        }

        [Operation(Operator.ArithmeticMultiply)]
        public static double Multiply(double left, double right)
        {
            return left * right;
        }

        [Operation(Operator.ArithmeticSubtract)]
        public static double Subtract(double left, double right)
        {
            return left - right;
        }
    }
}
