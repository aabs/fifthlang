namespace ast_model;

public static class GenerationHelpers
{
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

    public static bool IsCollectionType(this Type t)
    {
        if (t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(List<>)
                   || t.GetGenericTypeDefinition() == typeof(LinkedList<>);
        }

        return false;
    }

    public static bool IsLinkedListCollectionType(this Type t)
    {
        if (t.IsGenericType)
        {
            return t.GetGenericTypeDefinition() == typeof(LinkedList<>);
        }

        return false;
    }
}
