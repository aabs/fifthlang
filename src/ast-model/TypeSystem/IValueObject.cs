namespace ast_model.TypeSystem
{
    public interface IValueObject
    {
        /// <summary>
        /// A list of names by which this variable is known
        /// </summary>
        List<string> NamesInScope { get; set; }

        /// <summary>
        /// Gets the type of the value
        /// </summary>
        /// <value>The type of the value.</value>
        TypeId ValueType { get; }
    }

    /// <summary>
    /// A runtime value that can be stored in an environment and shared between variable bindings
    /// </summary>
    public interface IValueObject<T> : IValueObject
    {
        T Value { get; set; }
    }
}
