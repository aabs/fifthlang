namespace ast_model;

public static class AstMetamodelProvider
{
    public static IEnumerable<Type> AllAstTypes =>
        from t in AllTypes
        where t.Namespace == "ast" && !t.Name.EndsWith("SystemTextJsonConverter")
        select t;

    public static IEnumerable<Type> AllTypes => typeof(AstMetamodelProvider).Assembly.ExportedTypes;

    public static IEnumerable<Type> ConcreteTypes =>
      from t in NonIgnoredTypes
      where t.IsClass && !t.IsAbstract // && !t.IsGenericType //&& !t.IsGenericTypeParameter
      select t;

    public static IEnumerable<Type> NonIgnoredTypes =>
      from t in AllAstTypes
      where !t.HavingAttribute<IgnoreAttribute>() || t.Name.Contains("IgnoreAttribute")
      select t;

    public static IEnumerable<PropertyInfo> BuildableProperties(this Type t)
    {
        _ = t ?? throw new ArgumentNullException(nameof(t));
        return (from pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                where !pi.HavingAttribute<IgnoreAttribute>()
                select pi).ToList();
    }

    public static IEnumerable<Type> TypeParameters(this Type type)
    {
        return type.GenericTypeArguments;
    }

    public static bool IgnoreDuringVisit(this MemberInfo memberInfo)
    {
        return !memberInfo.HavingAttribute<IgnoreDuringVisitAttribute>();
    }

    public static bool HasCollectionType(this PropertyInfo mi)
        => mi.PropertyType.IsCollectionType();

    public static bool HavingAttribute<T>(this MemberInfo mi)
        where T : Attribute
        => mi?.GetCustomAttributes<T>().Any() ?? false;

    public static bool HavingAttribute<T>(this Type type)
        where T : Attribute
        => type?.GetCustomAttributes<T>().Any() ?? false;
}
