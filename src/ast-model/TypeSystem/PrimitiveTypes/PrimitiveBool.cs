namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveBool : PrimitiveAny
    {
        public static PrimitiveBool Default { get; } = new();

        [Operation(Operator.LogicalAnd)]
        public static bool logical_and_bool_bool(bool left, bool right)
        {
            return left && right;
        }

        [Operation(Operator.LogicalOr)]
        public static bool logical_or_bool_bool(bool left, bool right)
        {
            return left || right;
        }
    }
}
