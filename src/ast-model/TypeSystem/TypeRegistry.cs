namespace ast_model.TypeSystem;

public class TypeRegistry
{
    public static readonly TypeRegistry DefaultRegistry = new();

    public static Dictionary<Type, TypeCoertionSeniority> NumericPrimitive = new()
    {
        [typeof(byte)] = TypeCoertionSeniority.@byte,
        [typeof(sbyte)] = TypeCoertionSeniority.@byte,
        [typeof(short)] = TypeCoertionSeniority.@short,
        [typeof(ushort)] = TypeCoertionSeniority.@short,
        [typeof(int)] = TypeCoertionSeniority.integer,
        [typeof(uint)] = TypeCoertionSeniority.integer,
        [typeof(long)] = TypeCoertionSeniority.@long,
        [typeof(ulong)] = TypeCoertionSeniority.@long,
        [typeof(float)] = TypeCoertionSeniority.@float,
        [typeof(double)] = TypeCoertionSeniority.@double,
        [typeof(decimal)] = TypeCoertionSeniority.@decimal
    };

    /// <summary>
    /// These are the types that will be treated as primitives (preloaded into the t registry)
    /// </summary>
    public static Type[] Primitives =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(DateTimeOffset),
        typeof(DateTime)
    ];

    public FifthType Register(FifthType type)
    {
        _registeredTypes[type.Name] = type;
        return type;
    }

    public void RegisterPrimitiveTypes()
    {
        foreach (var t in Primitives)
        {
            var ft = Register(new FifthType.TDotnetType(t) { Name = TypeName.From(t.Name) });
            _dotnetTypes[t] = ft;
        }
    }

    public bool TryGetTypeByName(string s, out FifthType? type)
    {
        type = (from kvp in _registeredTypes
                where kvp.Key.Value == s
                select kvp.Value).FirstOrDefault();
        return type != null;
    }

    public bool TryLookupFifthType(Type t, out FifthType result)
    {
        return _dotnetTypes.TryGetValue(t, out result);
    }

    public bool TryLookupType(TypeName tid, out FifthType result)
    {
        return _registeredTypes.TryGetValue(tid, out result);
    }

    public bool TryLookupType(Type t, out FifthType? result)
    {
        result = null;
        if (_dotnetTypes.TryGetValue(t, out result))
        {
            return true;
        }
        else
        {
            result = RegisterDotnetType(t);
        }

        return result is not null;
    }

    private uint _baseIdCounter = 0;
    private ConcurrentDictionary<Type, FifthType> _dotnetTypes = new();
    private ConcurrentDictionary<TypeName, FifthType> _registeredTypes = new();

    private FifthType RegisterDotnetType(Type t) => Register(new FifthType.TDotnetType(t) { Name = TypeName.From(t.Name) });
}
