namespace ast_model.TypeSystem
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class OpAttribute : Attribute
    {
        public OpAttribute(string commonName, string operatorRepresentation, OperatorPosition position = OperatorPosition.Infix)
        {
            CommonName = commonName;
            OperatorRepresentation = operatorRepresentation;
            Position = position;
        }

        public string CommonName { get; set; }
        public string OperatorRepresentation { get; set; }
        public OperatorPosition Position { get; set; } = OperatorPosition.Infix;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class OperationAttribute : Attribute
    {
        public OperationAttribute(Operator op, OperatorPosition position = OperatorPosition.Infix)
        {
            Op = op;
            Position = position;
        }

        public Operator Op { get; set; }
        public OperatorPosition Position { get; set; } = OperatorPosition.Infix;
    }
}
