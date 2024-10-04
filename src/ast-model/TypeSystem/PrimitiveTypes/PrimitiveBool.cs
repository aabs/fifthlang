namespace ast_model.TypeSystem.PrimitiveTypes
{
    public class PrimitiveBool : PrimitiveAny
    {
        public static PrimitiveBool Default { get; } = new();

        [Operation(Operator.And)]
        public static bool logical_and_bool_bool(bool left, bool right)
        {
            return left && right;
        }

        [Operation(Operator.Or)]
        public static bool logical_or_bool_bool(bool left, bool right)
        {
            return left || right;
        }
    }
}
