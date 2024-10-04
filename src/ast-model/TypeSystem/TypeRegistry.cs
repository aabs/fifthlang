using ast_model.TypeSystem.PrimitiveTypes;

namespace ast_model.TypeSystem;

public sealed class TypeRegistry : ITypeRegistry
{
    public static readonly TypeRegistry DefaultRegistry = new();

    public static readonly Dictionary<Type, IType> PrimitiveMappings = new()
    {
        // {typeof(IList), PrimitiveList.Default},
        {typeof(string), PrimitiveString.Default},
        {typeof(short), PrimitiveShort.Default},
        {typeof(int), PrimitiveInteger.Default},
        {typeof(long), PrimitiveLong.Default},
        {typeof(bool), PrimitiveBool.Default},
        {typeof(char), PrimitiveChar.Default},
        {typeof(float), PrimitiveFloat.Default},
        {typeof(double), PrimitiveDouble.Default},
        {typeof(decimal), PrimitiveDecimal.Default},
        {typeof(DateTimeOffset), PrimitiveDate.Default},
        {typeof(DateTime), PrimitiveDate.Default}
    };

    public static readonly IType[] PrimitiveTypes =
    {
        PrimitiveBool.Default, PrimitiveBool.Default, PrimitiveChar.Default, PrimitiveDate.Default,
        PrimitiveDecimal.Default, PrimitiveDouble.Default, PrimitiveFloat.Default, PrimitiveFunction.Default,
        PrimitiveInteger.Default, PrimitiveLong.Default, PrimitiveShort.Default, PrimitiveString.Default,
        PrimitiveTriple.Default
    };

    private static long typeIdDispenser;
    private readonly ConcurrentDictionary<TypeId, IType> typeRegister = new();

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

    public bool RegisterType(IType type) => TrySetType(type, out var id);

    public bool TryGetType(TypeId typeId, out IType type)
    {
        return typeRegister.TryGetValue(typeId, out type);
    }

    public bool TryGetTypeByName(string typeName, out IType type)
    {
        foreach (var fifthType in typeRegister.Values)
        {
            if (fifthType.Name == typeName)
            {
                type = fifthType;
                return true;
            }
        }

        type = null;
        return false;
    }

    public bool TryLookupType(Type t, out IType result)
    {
        return PrimitiveMappings.TryGetValue(t, out result);
    }

    public bool TrySetType(IType type, out TypeId typeId)
    {
        var existingTypeId = type.TypeId;
        if (existingTypeId != null && typeRegister.ContainsKey(existingTypeId))
        {
            typeId = existingTypeId;
            return true;
        }

        var newTypeId = (ushort)Interlocked.Increment(ref typeIdDispenser);
        typeId =  TypeId.From(newTypeId);
        return typeRegister.TryAdd(typeId, type);
    }
}
