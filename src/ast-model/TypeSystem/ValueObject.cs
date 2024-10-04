namespace ast_model.TypeSystem
{
    /// <summary>
    /// A runtime value that can be stored in an environment and shared between variable bindings
    /// </summary>
    public class ValueObject : IValueObject<object>
    {
        public ValueObject(TypeId fifthType, string name, object value)
        {
            ValueType = fifthType;
            Value = value;
            NamesInScope.Add(name);
        }

        /// <summary>
        /// A list of names by which this variable is known
        /// </summary>
        public List<string> NamesInScope { get; set; } = new List<string>(1);

        public object Value { get; set; }
        public TypeId ValueType { get; }
    }

    /// <summary>
    /// A runtime value that can be stored in an environment and shared between variable bindings
    /// </summary>
    public class ValueObject<T> : IValueObject<T>
    {
        public ValueObject(TypeId fifthType, string name, T value)
        {
            ValueType = fifthType;
            NamesInScope.Add(name);
            Value = value;
        }

        /// <summary>
        /// A list of names by which this variable is known
        /// </summary>
        public List<string> NamesInScope { get; set; } = new List<string>(1);

        /// <summary>
        /// The value in the environment, shared between each of the names defined in scope.
        /// </summary>
        public T Value { get; set; }

        public TypeId ValueType { get; }
    }
}
