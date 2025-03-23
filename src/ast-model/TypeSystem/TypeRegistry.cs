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

    public void Register(FifthType type)
    {
        _registeredTypes[type.Name] = type;
    }

    public void RegisterPrimitiveTypes()
    {
        foreach (var t in Primitives)
        {
            var ft = new FifthType.TDotnetType((Type)t) { Name = TypeName.From((string)t.Name) };
            Register(ft);
            _dotnetTypes[(Type)t] = ft;
        }
    }

    public bool TryGetTypeByName(string s, out FifthType typeId)
    {
        throw new NotImplementedException();
    }

    public bool TryLookupFifthType(Type t, out FifthType result)
    {
        return _dotnetTypes.TryGetValue(t, out result);
    }

    public bool TryLookupType(TypeName tid, out FifthType result)
    {
        return _registeredTypes.TryGetValue(tid, out result);
    }

    public bool TryLookupType(Type t, out FifthType result)
    {
        FifthType typeId;
        if (_dotnetTypes.TryGetValue(t, out typeId))
        {
            result = _registeredTypes[typeId.Name];
        }
        else
        {
            typeId = RegisterDotnetType(t);
        }

        return _registeredTypes.TryGetValue(typeId.Name, out result);
    }

    private uint _baseIdCounter = 0;
    private ConcurrentDictionary<Type, FifthType> _dotnetTypes = [];
    private ConcurrentDictionary<TypeName, FifthType> _registeredTypes = [];

    private FifthType RegisterDotnetType(Type t)
    {
        var result = new FifthType.TDotnetType(t) { Name = TypeName.From(t.Name) };
        Register(result);
        return result;
    }
}
