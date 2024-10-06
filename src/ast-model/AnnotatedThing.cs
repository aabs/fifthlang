using ast_model;

namespace ast;

public abstract record AnnotatedThing : IAnnotated
{
    [Ignore, IgnoreDuringVisit]
    public object this[string index]
    {
        get
        {
            if (Annotations.TryGetValue(index, out var result))
            {
                return result;
            }

            return default;
        }
        set
        {
            Annotations[index] = value;
        }
    }

    [IgnoreDuringVisit]
    public Dictionary<string, object> Annotations { get; init; }

    public bool HasAnnotation(string key)
    {
        return Annotations.ContainsKey(key);
    }

    public bool TryGetAnnotation<T>(string name, out T result) where T : class
    {
        result = default;
        if (Annotations.TryGetValue(name, out var tmp))
        {
            result = tmp as T;
        }

        return result is not null;
    }
}
