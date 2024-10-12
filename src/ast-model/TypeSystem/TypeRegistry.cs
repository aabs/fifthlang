using System.Collections.Immutable;
using ast_model.TypeSystem.PrimitiveTypes;

namespace ast_model.TypeSystem;

public sealed class TypeRegistry : FifthTypeRegistry
{
    public static readonly TypeRegistry DefaultRegistry = new();

    public static readonly Dictionary<Type, FifthType> PrimitiveMappings = new()
    {
        // {typeof(IList), PrimitiveList.Default},
        {typeof(string), PrimitiveString.Default.Type},
        {typeof(short), PrimitiveShort.Default.Type},
        {typeof(int), PrimitiveInteger.Default.Type},
        {typeof(long), PrimitiveLong.Default.Type},
        {typeof(bool), PrimitiveBool.Default.Type},
        {typeof(char), PrimitiveChar.Default.Type},
        {typeof(float), PrimitiveFloat.Default.Type},
        {typeof(double), PrimitiveDouble.Default.Type},
        {typeof(decimal), PrimitiveDecimal.Default.Type},
        {typeof(DateTimeOffset), PrimitiveDate.Default.Type},
        {typeof(DateTime), PrimitiveDate.Default.Type}
    };

    public static readonly FifthType[] PrimitiveTypes =
    {
        PrimitiveBool.Default.Type, PrimitiveBool.Default.Type, PrimitiveChar.Default.Type, PrimitiveDate.Default.Type,
        PrimitiveDecimal.Default.Type, PrimitiveDouble.Default.Type, PrimitiveFloat.Default.Type, PrimitiveFunction.Default.Type,
        PrimitiveInteger.Default.Type, PrimitiveLong.Default.Type, PrimitiveShort.Default.Type, PrimitiveString.Default.Type,
        PrimitiveTriple.Default.Type
    };

    private static long typeIdDispenser;
    private readonly ConcurrentDictionary<TypeId, FifthType> typeRegister = new();

    private TypeRegistry()
    {
    }

    public bool LoadPrimitiveTypes()
    {
        var result = true;
        foreach (var t in PrimitiveTypes)
        {
            result &= RegisterType(t);
        }

        return result;
    }

    public bool RegisterType(FifthType type) => TrySetType(type, out var id);

    public bool TryGetType(TypeId typeId, out FifthType type)
    {
        return typeRegister.TryGetValue(typeId, out type);
    }

    public bool TryGetTypeByName(string typeName, out FifthType type)
    {
        foreach (var fifthType in typeRegister.Values)
        {
            if (fifthType.Symbol.Name == typeName)
            {
                type = fifthType;
                return true;
            }
        }

        type = null;
        return false;
    }

    public bool TryLookupType(Type t, out FifthType result)
    {
        return PrimitiveMappings.TryGetValue(t, out result);
    }

    public bool TrySetType(FifthType type, out TypeId typeId)
    {
        var existingTypeId = type.Id;
        if (existingTypeId != null && typeRegister.ContainsKey(existingTypeId))
        {
            typeId = existingTypeId;
            return true;
        }

        var newTypeId = (ushort)Interlocked.Increment(ref typeIdDispenser);
        typeId = TypeId.From(newTypeId);
        return typeRegister.TryAdd(typeId, type);
    }
}

public class NewTypeRegistry
{
    public static readonly NewTypeRegistry DefaultRegistry = new();

    private ConcurrentDictionary<TypeId, FifthType> _registeredTypes = [];
    private ConcurrentDictionary<Type, TypeId> _dotnetTypes = [];

    private uint _baseIdCounter = 0;

    TypeId GetNewId()
    {
        var typeId = TypeId.From(Interlocked.Increment(ref _baseIdCounter));

        return typeId;
    }

    void Register(TypeId id, FifthType type)
    {
        _registeredTypes[id] = type;
    }

    TypeId Register(FifthType type)
    {
        var typeId = GetNewId();
        _registeredTypes[typeId] = type;
        return typeId;
    }
    void RegisterPrimitiveTypes()
    {
        foreach (var t in Primitives)
        {
            var typeId = Register(new FifthType.NetType(t));
            _dotnetTypes[t] = typeId;
        }
    }

    TypeId RegisterDotnetType(Type t)
    {
        return Register(new FifthType.NetType(t));
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
        typeof(DateTime),
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
        [typeof(decimal)] = TypeCoertionSeniority.@decimal,
    };

    public bool TryLookupType(TypeId tid, out FifthType result)
    {
        return _registeredTypes.TryGetValue(tid, out result);
    }
    public bool TryLookupType(Type t, out FifthType result)
    {
        TypeId typeId ;
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

}
