using System.Collections.Immutable;

namespace ast_model.TypeSystem;

public class TypeRegistry
{
    public static readonly TypeRegistry DefaultRegistry = new();

    private ConcurrentDictionary<TypeId, FifthType> _registeredTypes = [];
    private ConcurrentDictionary<Type, TypeId> _dotnetTypes = [];

    private uint _baseIdCounter = 0;

    private TypeId GetNewId()
    {
        var typeId = TypeId.From(Interlocked.Increment(ref _baseIdCounter));

        return typeId;
    }

    public void Register(TypeId id, FifthType type)
    {
        _registeredTypes[id] = type;
    }

    public TypeId Register(FifthType type)
    {
        var typeId = GetNewId();
        _registeredTypes[typeId] = type;
        return typeId;
    }

    public void RegisterPrimitiveTypes()
    {
        foreach (var t in Primitives)
        {
            var typeId = Register(new FifthType.NetType(t) { Name = TypeName.From(t.Name) });
            _dotnetTypes[t] = typeId;
        }
    }

    private TypeId RegisterDotnetType(Type t)
    {
        return Register(new FifthType.NetType(t) { Name = TypeName.From(t.Name) });
    }

    /// <summary>
    /// These are the types that will be treated as primitives (preloaded into the type registry)
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

    public bool TryLookupType(TypeId tid, out FifthType result)
    {
        return _registeredTypes.TryGetValue(tid, out result);
    }

    public bool TryLookupType(Type t, out FifthType result)
    {
        TypeId typeId;
        if (_dotnetTypes.TryGetValue(t, out typeId))
        {
            result = _registeredTypes[typeId];
        }
        else
        {
            typeId = RegisterDotnetType(t);
        }

        return _registeredTypes.TryGetValue(typeId, out result);
    }

    public bool TryLookupTypeId(Type t, out TypeId result)
    {
        return _dotnetTypes.TryGetValue(t, out result);
    }

    public bool TryGetTypeByName(string s, out TypeId typeId)
    {
        throw new NotImplementedException();
    }
}
