using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;

namespace code_generator;

/// <summary>
/// Manages metadata lookups and registrations for types, fields, methods, and constructors
/// </summary>
internal class MetadataManager
{
    // Maps for types, fields, and constructors defined in this assembly
    private readonly Dictionary<string, TypeDefinitionHandle> _typeHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FieldDefinitionHandle> _fieldHandlesByTypeAndName = new(StringComparer.Ordinal);
    private readonly Dictionary<TypeDefinitionHandle, string> _typeNamesByHandle = new();
    private readonly Dictionary<string, MethodDefinitionHandle> _ctorHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<FieldDefinitionHandle, il_ast.TypeReference> _fieldDeclaredTypes = new();

    // Per-method emission state for simple local type inference
    private string? _lastLoadedLocal;
    private bool _lastWasNewobj;
    private Dictionary<string, SignatureTypeCode> _localVarTypeMap = new(StringComparer.Ordinal);
    private string? _pendingNewobjTypeName;
    private readonly Dictionary<string, TypeDefinitionHandle> _localVarClassTypeHandles = new(StringComparer.Ordinal);
    private TypeDefinitionHandle? _pendingStackTopClassType;
    private readonly Dictionary<string, TypeDefinitionHandle> _paramClassTypeHandles = new(StringComparer.Ordinal);
    private string? _lastLoadedParam;

    // Cached type references for common system types
    private EntityHandle _systemInt32TypeRef;
    private readonly Dictionary<string, AssemblyReferenceHandle> _assemblyRefHandles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TypeReferenceHandle> _typeRefHandlesCache = new(StringComparer.Ordinal);

    public void RegisterType(string name, TypeDefinitionHandle handle)
    {
        _typeHandles[name] = handle;
        _typeNamesByHandle[handle] = name;
    }

    public void RegisterField(string name, FieldDefinitionHandle handle)
    {
        _fieldHandles[name] = handle;
    }

    public void RegisterFieldByTypeAndName(string typeAndName, FieldDefinitionHandle handle)
    {
        _fieldHandlesByTypeAndName[typeAndName] = handle;
    }

    public void RegisterConstructor(string typeName, MethodDefinitionHandle handle)
    {
        _ctorHandles[typeName] = handle;
    }

    public void RegisterFieldType(FieldDefinitionHandle fieldHandle, il_ast.TypeReference typeRef)
    {
        _fieldDeclaredTypes[fieldHandle] = typeRef;
    }

    public void RegisterAssemblyReference(string name, AssemblyReferenceHandle handle)
    {
        _assemblyRefHandles[name] = handle;
    }

    public void RegisterTypeReference(string fullName, TypeReferenceHandle handle)
    {
        _typeRefHandlesCache[fullName] = handle;
    }

    public bool TryGetType(string name, out TypeDefinitionHandle handle) => _typeHandles.TryGetValue(name, out handle);
    
    public bool TryGetField(string name, out FieldDefinitionHandle handle) => _fieldHandles.TryGetValue(name, out handle);
    
    public bool TryGetFieldByTypeAndName(string typeAndName, out FieldDefinitionHandle handle) => 
        _fieldHandlesByTypeAndName.TryGetValue(typeAndName, out handle);
    
    public bool TryGetConstructor(string typeName, out MethodDefinitionHandle handle) => _ctorHandles.TryGetValue(typeName, out handle);
    
    public bool TryGetFieldType(FieldDefinitionHandle fieldHandle, out il_ast.TypeReference typeRef) => 
        _fieldDeclaredTypes.TryGetValue(fieldHandle, out typeRef!);
    
    public bool TryGetTypeName(TypeDefinitionHandle handle, out string name) => _typeNamesByHandle.TryGetValue(handle, out name!);

    public bool TryGetAssemblyReference(string name, out AssemblyReferenceHandle handle) => 
        _assemblyRefHandles.TryGetValue(name, out handle);

    public bool TryGetTypeReference(string fullName, out TypeReferenceHandle handle) => 
        _typeRefHandlesCache.TryGetValue(fullName, out handle);

    // Per-method state management
    public void ResetMethodState()
    {
        _lastLoadedLocal = null;
        _lastWasNewobj = false;
        _localVarTypeMap = new Dictionary<string, SignatureTypeCode>(StringComparer.Ordinal);
        _pendingNewobjTypeName = null;
        _pendingStackTopClassType = null;
        _localVarClassTypeHandles.Clear();
        _paramClassTypeHandles.Clear();
        _lastLoadedParam = null;
    }

    public string? LastLoadedLocal
    {
        get => _lastLoadedLocal;
        set => _lastLoadedLocal = value;
    }

    public bool LastWasNewobj
    {
        get => _lastWasNewobj;
        set => _lastWasNewobj = value;
    }

    public string? PendingNewobjTypeName
    {
        get => _pendingNewobjTypeName;
        set => _pendingNewobjTypeName = value;
    }

    public TypeDefinitionHandle? PendingStackTopClassType
    {
        get => _pendingStackTopClassType;
        set => _pendingStackTopClassType = value;
    }

    public string? LastLoadedParam
    {
        get => _lastLoadedParam;
        set => _lastLoadedParam = value;
    }

    public Dictionary<string, SignatureTypeCode> LocalVarTypeMap => _localVarTypeMap;
    
    public Dictionary<string, TypeDefinitionHandle> LocalVarClassTypeHandles => _localVarClassTypeHandles;
    
    public Dictionary<string, TypeDefinitionHandle> ParamClassTypeHandles => _paramClassTypeHandles;

    public EntityHandle SystemInt32TypeRef
    {
        get => _systemInt32TypeRef;
        set => _systemInt32TypeRef = value;
    }
}
