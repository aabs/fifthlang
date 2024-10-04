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

        [Operation(Operator.Add)]
        public static double Add(double left, double right)
        {
            return left + right;
        }

        [Operation(Operator.Divide)]
        public static double Divide(double left, double right)
        {
            return left / right;
        }

        [Operation(Operator.Multiply)]
        public static double Multiply(double left, double right)
        {
            return left * right;
        }

        [Operation(Operator.Subtract)]
        public static double Subtract(double left, double right)
        {
            return left - right;
        }
    }
}
