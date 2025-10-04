using il_ast;

namespace code_generator.Emit;

/// <summary>
/// Maps Fifth language type names to .NET/IL type references.
/// Centralizes all type name to TypeReference conversions.
/// </summary>
public class TypeMapper
{
    private readonly Dictionary<string, TypeReference> _typeMap = new();

    public TypeMapper()
    {
        InitializeBuiltinTypes();
    }

    /// <summary>
    /// Maps a Fifth type name to an IL TypeReference
    /// </summary>
    public TypeReference MapType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return new TypeReference { Namespace = "System", Name = "Object" };
        }

        if (_typeMap.TryGetValue(typeName, out var typeRef))
        {
            return typeRef;
        }

        // If not in the map, assume it's a custom type in the Fifth.Generated namespace
        return new TypeReference { Namespace = "Fifth.Generated", Name = typeName };
    }

    /// <summary>
    /// Maps a builtin Fifth type name to its corresponding System.Type (for type inference)
    /// </summary>
    public static Type? MapBuiltinFifthTypeNameToSystem(string? name)
    {
        return name switch
        {
            "int" or "System.Int32" or "Int32" => typeof(int),
            "string" or "System.String" or "String" => typeof(string),
            "float" or "System.Single" or "Single" => typeof(float),
            "double" or "System.Double" or "Double" => typeof(double),
            "bool" or "System.Boolean" or "Boolean" => typeof(bool),
            "long" or "System.Int64" or "Int64" => typeof(long),
            "short" or "System.Int16" or "Int16" => typeof(short),
            "byte" or "System.Byte" or "Byte" => typeof(byte),
            "char" or "System.Char" or "Char" => typeof(char),
            "decimal" or "System.Decimal" or "Decimal" => typeof(decimal),
            "sbyte" or "System.SByte" or "SByte" or "int8" or "Int8" => typeof(sbyte),
            "uint" or "System.UInt32" or "UInt32" => typeof(uint),
            "ushort" or "System.UInt16" or "UInt16" => typeof(ushort),
            "ulong" or "System.UInt64" or "UInt64" => typeof(ulong),
            _ => null
        };
    }

    private void InitializeBuiltinTypes()
    {
        // Map Fifth language type names to System types
        _typeMap["int"] = new TypeReference { Namespace = "System", Name = "Int32" };
        _typeMap["string"] = new TypeReference { Namespace = "System", Name = "String" };
        _typeMap["float"] = new TypeReference { Namespace = "System", Name = "Single" };
        _typeMap["double"] = new TypeReference { Namespace = "System", Name = "Double" };
        _typeMap["bool"] = new TypeReference { Namespace = "System", Name = "Boolean" };
        _typeMap["void"] = new TypeReference { Namespace = "System", Name = "Void" };
        _typeMap["graph"] = new TypeReference { Namespace = "VDS.RDF", Name = "IGraph" };

        // Also map .NET type names to System types
        _typeMap["Int32"] = new TypeReference { Namespace = "System", Name = "Int32" };
        _typeMap["String"] = new TypeReference { Namespace = "System", Name = "String" };
        _typeMap["Single"] = new TypeReference { Namespace = "System", Name = "Single" };
        _typeMap["Double"] = new TypeReference { Namespace = "System", Name = "Double" };
        _typeMap["Boolean"] = new TypeReference { Namespace = "System", Name = "Boolean" };
        _typeMap["Void"] = new TypeReference { Namespace = "System", Name = "Void" };

        // Additional .NET primitive types
        _typeMap["byte"] = new TypeReference { Namespace = "System", Name = "Byte" };
        _typeMap["Byte"] = new TypeReference { Namespace = "System", Name = "Byte" };
        _typeMap["char"] = new TypeReference { Namespace = "System", Name = "Char" };
        _typeMap["Char"] = new TypeReference { Namespace = "System", Name = "Char" };
        _typeMap["long"] = new TypeReference { Namespace = "System", Name = "Int64" };
        _typeMap["Int64"] = new TypeReference { Namespace = "System", Name = "Int64" };
        _typeMap["short"] = new TypeReference { Namespace = "System", Name = "Int16" };
        _typeMap["Int16"] = new TypeReference { Namespace = "System", Name = "Int16" };
        _typeMap["decimal"] = new TypeReference { Namespace = "System", Name = "Decimal" };
        _typeMap["Decimal"] = new TypeReference { Namespace = "System", Name = "Decimal" };
        _typeMap["sbyte"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["SByte"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["int8"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["Int8"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["uint"] = new TypeReference { Namespace = "System", Name = "UInt32" };
        _typeMap["UInt32"] = new TypeReference { Namespace = "System", Name = "UInt32" };
        _typeMap["ulong"] = new TypeReference { Namespace = "System", Name = "UInt64" };
        _typeMap["UInt64"] = new TypeReference { Namespace = "System", Name = "UInt64" };
        _typeMap["ushort"] = new TypeReference { Namespace = "System", Name = "UInt16" };
        _typeMap["UInt16"] = new TypeReference { Namespace = "System", Name = "UInt16" };
    }
}
