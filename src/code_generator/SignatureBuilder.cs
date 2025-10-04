using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;

namespace code_generator;

/// <summary>
/// Handles creation of method, parameter, and local variable signatures
/// </summary>
internal class SignatureBuilder
{
    private readonly MetadataManager _metadataManager;

    public SignatureBuilder(MetadataManager metadataManager)
    {
        _metadataManager = metadataManager;
    }

    /// <summary>
    /// Create local variable signature from the collected local variables
    /// </summary>
    public EntityHandle CreateLocalVariableSignature(MetadataBuilder metadataBuilder, List<string> localVariables, Dictionary<string, SignatureTypeCode> localTypes)
    {
        var localsSignature = new BlobBuilder();
        localsSignature.WriteByte(0x07); // LOCAL_SIG
        localsSignature.WriteCompressedInteger(localVariables.Count); // Number of locals

        // Use inferred types where available (default Int32)
        foreach (var localVar in localVariables)
        {
            // Prefer explicit class type if known (from newobj)
            if (_metadataManager.LocalVarClassTypeHandles.TryGetValue(localVar, out var typeDefHandle))
            {
                // ELEMENT_TYPE_CLASS (0x12) then TypeDefOrRef coded index for the type
                localsSignature.WriteByte(0x12);
                var rowId = MetadataTokens.GetRowNumber(typeDefHandle);
                var codedIndex = (rowId << 2) | 0; // TypeDefOrRef tag 0 = TypeDef
                localsSignature.WriteCompressedInteger(codedIndex);
            }
            else
            {
                if (!localTypes.TryGetValue(localVar, out var sigType))
                {
                    sigType = SignatureTypeCode.Int32;
                }
                localsSignature.WriteByte((byte)sigType);
            }
        }

        return metadataBuilder.AddStandaloneSignature(metadataBuilder.GetOrAddBlob(localsSignature));
    }

    /// <summary>
    /// Map IL metamodel type to SignatureTypeCode
    /// </summary>
    public SignatureTypeCode GetSignatureTypeCode(il_ast.TypeReference typeRef)
    {
        if (typeRef.Namespace == "System")
        {
            return typeRef.Name switch
            {
                "Void" => SignatureTypeCode.Void,
                "Int32" => SignatureTypeCode.Int32,
                "String" => SignatureTypeCode.String,
                "Single" => SignatureTypeCode.Single,
                "Double" => SignatureTypeCode.Double,
                "Boolean" => SignatureTypeCode.Boolean,
                "SByte" => SignatureTypeCode.SByte,
                "Byte" => SignatureTypeCode.Byte,
                "Int16" => SignatureTypeCode.Int16,
                "UInt16" => SignatureTypeCode.UInt16,
                "Int64" => SignatureTypeCode.Int64,
                "UInt32" => SignatureTypeCode.UInt32,
                "UInt64" => SignatureTypeCode.UInt64,
                "Char" => SignatureTypeCode.Char,
                _ => SignatureTypeCode.Object
            };
        }
        return SignatureTypeCode.Object;
    }

    /// <summary>
    /// Write type information into a signature blob
    /// </summary>
    public void WriteTypeInSignature(BlobBuilder builder, il_ast.TypeReference typeRef)
    {
        if (typeRef.Namespace == "System")
        {
            builder.WriteByte((byte)GetSignatureTypeCode(typeRef));
            return;
        }

        // User-defined types: encode as CLASS with TypeDefOrRef coded index
        if (!string.IsNullOrEmpty(typeRef.Name) && _metadataManager.TryGetType(typeRef.Name, out var typeHandle))
        {
            builder.WriteByte(0x12); // ELEMENT_TYPE_CLASS
            var rowId = MetadataTokens.GetRowNumber(typeHandle);
            var codedIndex = (rowId << 2) | 0; // TypeDefOrRef tag 0 = TypeDef
            builder.WriteCompressedInteger(codedIndex);
            return;
        }

        // Fallback to object if type not found
        builder.WriteByte((byte)SignatureTypeCode.Object);
    }
}
