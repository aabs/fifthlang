using System.Reflection;
using ast_model.Attributes;

namespace ast_model;

public static class AstMetamodelProvider
{
    public static IEnumerable<Type> AllTypes => typeof(AstThing).Assembly.ExportedTypes;

    public static IEnumerable<Type> AllAstTypes =>
        from t in AllTypes
        where t.Namespace == "ast_model"
        orderby t.Name
        select t;

    public static IEnumerable<Type> NonIgnoredTypes =>
        from t in AllAstTypes
        where !(t.GetCustomAttributes<IgnoreAttribute>().Any() || t.Name.Contains("IgnoreAttribute"))
        select t;

    public static IEnumerable<Type> ConcreteTypes =>
        from t in NonIgnoredTypes
        where t.IsClass && !t.IsAbstract // && !t.IsGenericType //&& !t.IsGenericTypeParameter
        select t;

    public static IEnumerable<Type> TypeParameters(this Type type)
    {
        return type.GenericTypeArguments;
    }

    public static IEnumerable<PropertyInfo> BuildableProperties(this Type t)
    {
        _ = t ?? throw new ArgumentNullException(nameof(t));
        //return t.GetFields();
        return (from pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                where !(pi?.GetCustomAttributes<IgnoreAttribute>().Any() ?? true)
                select pi).ToList();
    }
}
