namespace il_ast_generated;
using ast_generated;
using il_ast;
using ast_model.TypeSystem;
using System.Collections.Generic;
#nullable disable

public class MethodSignatureBuilder : IBuilder<il_ast.MethodSignature>
{
    private MethodCallingConvention _CallingConvention;
    private ushort _NumberOfParameters;
    private List<ParameterSignature> _ParameterSignatures;
    private TypeReference _ReturnTypeSignature;

    public il_ast.MethodSignature Build()
    {
        return new il_ast.MethodSignature()
        {
            CallingConvention = this._CallingConvention,
            NumberOfParameters = this._NumberOfParameters,
            ParameterSignatures = this._ParameterSignatures,
            ReturnTypeSignature = this._ReturnTypeSignature
        };
    }
    public MethodSignatureBuilder WithCallingConvention(MethodCallingConvention value)
    {
        _CallingConvention = value;
        return this;
    }

    public MethodSignatureBuilder WithNumberOfParameters(ushort value)
    {
        _NumberOfParameters = value;
        return this;
    }

    public MethodSignatureBuilder WithParameterSignatures(List<ParameterSignature> value)
    {
        _ParameterSignatures = value;
        return this;
    }

    public MethodSignatureBuilder AddingItemToParameterSignatures(ParameterSignature value)
    {
        _ParameterSignatures ??= new List<ParameterSignature>();
        _ParameterSignatures.Add(value);
        return this;
    }

    public MethodSignatureBuilder WithReturnTypeSignature(TypeReference value)
    {
        _ReturnTypeSignature = value;
        return this;
    }

}

public class MemberRefBuilder : IBuilder<il_ast.MemberRef>
{
    private MemberTarget _Target;
    private ClassDefinition _ClassDefinition;
    private string _Name;
    private MethodSignature _Sig;
    private FieldDefinition _Field;

    public il_ast.MemberRef Build()
    {
        return new il_ast.MemberRef()
        {
            Target = this._Target,
            ClassDefinition = this._ClassDefinition,
            Name = this._Name,
            Sig = this._Sig,
            Field = this._Field
        };
    }
    public MemberRefBuilder WithTarget(MemberTarget value)
    {
        _Target = value;
        return this;
    }

    public MemberRefBuilder WithClassDefinition(ClassDefinition value)
    {
        _ClassDefinition = value;
        return this;
    }

    public MemberRefBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public MemberRefBuilder WithSig(MethodSignature value)
    {
        _Sig = value;
        return this;
    }

    public MemberRefBuilder WithField(FieldDefinition value)
    {
        _Field = value;
        return this;
    }

}

public class SByteLiteralBuilder : IBuilder<il_ast.SByteLiteral>
{

    public il_ast.SByteLiteral Build()
    {
        return new il_ast.SByteLiteral()
        {
        };
    }
}

public class FloatLiteralBuilder : IBuilder<il_ast.FloatLiteral>
{

    public il_ast.FloatLiteral Build()
    {
        return new il_ast.FloatLiteral()
        {
        };
    }
}

#nullable restore
