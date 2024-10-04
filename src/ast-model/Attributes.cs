namespace ast_model
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public class IgnoreDuringVisitAttribute : Attribute
    {
    }
}
