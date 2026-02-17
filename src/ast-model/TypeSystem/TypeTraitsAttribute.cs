namespace ast_model.TypeSystem
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TypeTraitsIgnoreAttribute : Attribute
    {
        public bool IsGeneric { get; set; }
        public bool IsNumeric { get; set; }
        public bool IsPrimitive { get; set; }
        public string Keyword { get; set; }
    }
}
