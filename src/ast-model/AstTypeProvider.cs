using System.Runtime.CompilerServices;

namespace ast_model;
public interface ITypeProvider
{
    Type BaseType { get; set; }
    IEnumerable<Type> AllAstTypes { get; }
    IEnumerable<Type> AllTypes { get; }
    IEnumerable<Type> ConcreteTypes { get; }
    string NamespaceScope { get; set; }
    IEnumerable<Type> NonIgnoredTypes { get; }
}
public class TypeProvider<T> : ITypeProvider
{
    public TypeProvider()
    {
        NamespaceScope = typeof(T).Namespace;
        BaseType = typeof(T);
    }

    public Type BaseType { get; set; }

    public IEnumerable<Type> AllAstTypes =>
        from t in AllTypes
        where t.Namespace == NamespaceScope && !t.Name.EndsWith("SystemTextJsonConverter")
        select t;

    public IEnumerable<Type> AllTypes => typeof(AstTypeProvider).Assembly.ExportedTypes;

    public IEnumerable<Type> ConcreteTypes =>
      from t in NonIgnoredTypes
      where t.IsClass && !t.IsAbstract // && !t.IsGenericType //&& !t.IsGenericTypeParameter
      select t;

    public string NamespaceScope { get; set; }

    public IEnumerable<Type> NonIgnoredTypes =>
      from t in AllAstTypes
      where !t.HavingAttribute<IgnoreAttribute>() || t.Name.Contains("IgnoreAttribute")
      select t;
}

public static class AstTypeProvider
{
    public static IEnumerable<PropertyInfo> BuildableProperties(this Type t)
    {
        _ = t ?? throw new ArgumentNullException(nameof(t));
        return (from pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                select pi).ToList();
    }

    public static string BuildInstanceTypeName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (!type.IsCollectionType())
        {
            return BuildTypeName(type);
        }

        if (type.IsGenericType)
        {
            return BuildTypeName(type.GetGenericArguments().First());
        }

        return type.FullName!;
    }

    public static string BuildTypeName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        // Console.WriteLine($"Building type name for {type}");
        var sb = new StringBuilder();
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                sb.Append("Dictionary<");
                var sep = "";
                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    sb.Append(sep);
                    sb.Append(BuildTypeName(typeArgument));
                    sep = ", ";
                }

                sb.Append(">");
            }
            if (type.GetGenericTypeDefinition() == typeof(LinkedList<>))
            {
                sb.Append("LinkedList<");
                var sep = "";
                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    sb.Append(sep);
                    sb.Append(BuildTypeName(typeArgument));
                    sep = ", ";
                }

                sb.Append(">");
            }
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                sb.Append("List<");
                var sep = "";
                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    sb.Append(sep);
                    sb.Append(BuildTypeName(typeArgument));
                    sep = ", ";
                }

                sb.Append(">");
            }

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                sb.Append(BuildTypeName(type.GenericTypeArguments.First()));
                sb.Append("?");
            }

            return sb.ToString();
        }

        return type.FullName!;
    }

    public static bool HasCollectionType(this PropertyInfo mi)
        => mi.PropertyType.IsCollectionType();

    public static bool HavingAttribute<T>(this MemberInfo mi)
        where T : Attribute
        => mi?.GetCustomAttributes<T>().Any() ?? false;

    public static bool HavingAttribute<T>(this Type type)
        where T : Attribute
        => type?.GetCustomAttributes<T>().Any() ?? false;

    public static bool IgnoreDuringVisit(this MemberInfo mi)
        => mi.HavingAttribute<IgnoreDuringVisitAttribute>() && !mi.IncludeInVisit();

    public static bool IncludeInVisit(this MemberInfo memberInfo)
        => memberInfo.HavingAttribute<IncludeInVisitAttribute>();

    public static IEnumerable<PropertyInfo> InitialisedProperties(this Type t)
                                => from p in t.BuildableProperties()
                                   where p.IsInitOnly() && !p.HavingAttribute<IgnoreAttribute>()
                                   select p;

    public static bool IsAnAstThing(this Type t, Type baseType)
            => t.IsAssignableTo(baseType);

    public static bool IsCollectionType(this Type t)
    {
        if (t.IsGenericType)
        {
            return t.IsArray
                || t.GetGenericTypeDefinition() == typeof(List<>)
                //|| t.GetGenericTypeDefinition() == typeof(LinkedList<>)
                //|| t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                //|| t.GetGenericTypeDefinition() == typeof(Stack<>)
                //|| t.GetGenericTypeDefinition() == typeof(Queue<>)
                ;
        }

        return false;
    }

    /// <summary>
    /// Determines if this property is marked as init-only.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>True if the property is init-only, false otherwise.</returns>
    /// <remarks>
    /// <see
    /// cref="https://alistairevans.co.uk/2020/11/01/detecting-init-only-properties-with-reflection-in-c-9/"/>
    /// for more info
    /// </remarks>
    public static bool IsInitOnly(this PropertyInfo property)
    {
        if (!property.CanWrite)
        {
            return false;
        }

        if (property.HavingAttribute<RequiredMemberAttribute>())
        {
            return true;
        }
        var setMethod = property.SetMethod;

        // Get the modifiers applied to the return parameter.
        var setMethodReturnParameterModifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();

        // Init-only properties are marked with the IsExternalInit type.
        return setMethodReturnParameterModifiers.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
    }

    public static bool IsLinkedListCollectionType(this Type t)
    {
        if (t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(LinkedList<>);
        }

        return false;
    }

    public static Type? TypeParameter(this Type type, int ord)
    {
        if (type.IsGenericType && ord < type.GenericTypeArguments.Length)
        {
            return type.GenericTypeArguments[ord];
        }

        return null;
    }

    public static IEnumerable<Type> TypeParameters(this Type type)
                                                => type.GenericTypeArguments;

    public static IEnumerable<PropertyInfo> VisitableProperties(this Type t, Type baseType)
    {
        _ = t ?? throw new ArgumentNullException(nameof(t));
        return (from pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                where !pi.IgnoreDuringVisit() && (pi.PropertyType.IsAnAstThing(baseType) || (pi.PropertyType.IsGenericType && pi.PropertyType.TypeParameter(0).IsAnAstThing(baseType)))
                select pi).ToList();
    }
}
