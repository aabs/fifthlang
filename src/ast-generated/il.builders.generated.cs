namespace il_ast_generated;
using ast_generated;
using il_ast;
using ast_model.TypeSystem;
using System.Collections.Generic;
#nullable disable

public class AssemblyDeclarationBuilder : IBuilder<il_ast.AssemblyDeclaration>
{
    private string _Name;
    private Version _Version;
    private ModuleDeclaration _PrimeModule;
    private List<AssemblyReference> _AssemblyReferences;

    public il_ast.AssemblyDeclaration Build()
    {
        return new il_ast.AssemblyDeclaration()
        {
            Name = this._Name,
            Version = this._Version,
            PrimeModule = this._PrimeModule,
            AssemblyReferences = this._AssemblyReferences
        };
    }
    public AssemblyDeclarationBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public AssemblyDeclarationBuilder WithVersion(Version value)
    {
        _Version = value;
        return this;
    }

    public AssemblyDeclarationBuilder WithPrimeModule(ModuleDeclaration value)
    {
        _PrimeModule = value;
        return this;
    }

    public AssemblyDeclarationBuilder WithAssemblyReferences(List<AssemblyReference> value)
    {
        _AssemblyReferences = value;
        return this;
    }

    public AssemblyDeclarationBuilder AddingItemToAssemblyReferences(AssemblyReference value)
    {
        _AssemblyReferences ??= new List<AssemblyReference>();
        _AssemblyReferences.Add(value);
        return this;
    }

}

public class AssemblyReferenceBuilder : IBuilder<il_ast.AssemblyReference>
{
    private string _Name;
    private string _PublicKeyToken;
    private Version _Version;

    public il_ast.AssemblyReference Build()
    {
        return new il_ast.AssemblyReference()
        {
            Name = this._Name,
            PublicKeyToken = this._PublicKeyToken,
            Version = this._Version
        };
    }
    public AssemblyReferenceBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public AssemblyReferenceBuilder WithPublicKeyToken(string value)
    {
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyReferenceBuilder WithVersion(Version value)
    {
        _Version = value;
        return this;
    }

}

public class ModuleDeclarationBuilder : IBuilder<il_ast.ModuleDeclaration>
{
    private string _FileName;
    private List<ClassDefinition> _Classes;
    private List<MethodDefinition> _Functions;

    public il_ast.ModuleDeclaration Build()
    {
        return new il_ast.ModuleDeclaration()
        {
            FileName = this._FileName,
            Classes = this._Classes,
            Functions = this._Functions
        };
    }
    public ModuleDeclarationBuilder WithFileName(string value)
    {
        _FileName = value;
        return this;
    }

    public ModuleDeclarationBuilder WithClasses(List<ClassDefinition> value)
    {
        _Classes = value;
        return this;
    }

    public ModuleDeclarationBuilder AddingItemToClasses(ClassDefinition value)
    {
        _Classes ??= new List<ClassDefinition>();
        _Classes.Add(value);
        return this;
    }

    public ModuleDeclarationBuilder WithFunctions(List<MethodDefinition> value)
    {
        _Functions = value;
        return this;
    }

    public ModuleDeclarationBuilder AddingItemToFunctions(MethodDefinition value)
    {
        _Functions ??= new List<MethodDefinition>();
        _Functions.Add(value);
        return this;
    }

}

public class VersionBuilder : IBuilder<il_ast.Version>
{
    private int _Major;
    private int? _Minor;
    private int? _Build;
    private int? _Patch;

    public il_ast.Version Build()
    {
        return new il_ast.Version()
        {
            Major = this._Major,
            Minor = this._Minor,
            Build = this._Build,
            Patch = this._Patch
        };
    }
    public VersionBuilder WithMajor(int value)
    {
        _Major = value;
        return this;
    }

    public VersionBuilder WithMinor(int? value)
    {
        _Minor = value;
        return this;
    }

    public VersionBuilder WithBuild(int? value)
    {
        _Build = value;
        return this;
    }

    public VersionBuilder WithPatch(int? value)
    {
        _Patch = value;
        return this;
    }

}

public class ClassDefinitionBuilder : IBuilder<il_ast.ClassDefinition>
{
    private List<FieldDefinition> _Fields;
    private List<PropertyDefinition> _Properties;
    private List<MethodDefinition> _Methods;
    private string _Name;
    private string _Namespace;
    private List<ClassDefinition> _BaseClasses;
    private AssemblyDeclaration _ParentAssembly;
    private MemberAccessability _Visibility;

    public il_ast.ClassDefinition Build()
    {
        return new il_ast.ClassDefinition()
        {
            Fields = this._Fields,
            Properties = this._Properties,
            Methods = this._Methods,
            Name = this._Name,
            Namespace = this._Namespace,
            BaseClasses = this._BaseClasses,
            ParentAssembly = this._ParentAssembly,
            Visibility = this._Visibility
        };
    }
    public ClassDefinitionBuilder WithFields(List<FieldDefinition> value)
    {
        _Fields = value;
        return this;
    }

    public ClassDefinitionBuilder AddingItemToFields(FieldDefinition value)
    {
        _Fields ??= new List<FieldDefinition>();
        _Fields.Add(value);
        return this;
    }

    public ClassDefinitionBuilder WithProperties(List<PropertyDefinition> value)
    {
        _Properties = value;
        return this;
    }

    public ClassDefinitionBuilder AddingItemToProperties(PropertyDefinition value)
    {
        _Properties ??= new List<PropertyDefinition>();
        _Properties.Add(value);
        return this;
    }

    public ClassDefinitionBuilder WithMethods(List<MethodDefinition> value)
    {
        _Methods = value;
        return this;
    }

    public ClassDefinitionBuilder AddingItemToMethods(MethodDefinition value)
    {
        _Methods ??= new List<MethodDefinition>();
        _Methods.Add(value);
        return this;
    }

    public ClassDefinitionBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public ClassDefinitionBuilder WithNamespace(string value)
    {
        _Namespace = value;
        return this;
    }

    public ClassDefinitionBuilder WithBaseClasses(List<ClassDefinition> value)
    {
        _BaseClasses = value;
        return this;
    }

    public ClassDefinitionBuilder AddingItemToBaseClasses(ClassDefinition value)
    {
        _BaseClasses ??= new List<ClassDefinition>();
        _BaseClasses.Add(value);
        return this;
    }

    public ClassDefinitionBuilder WithParentAssembly(AssemblyDeclaration value)
    {
        _ParentAssembly = value;
        return this;
    }

    public ClassDefinitionBuilder WithVisibility(MemberAccessability value)
    {
        _Visibility = value;
        return this;
    }

}

public class MemberAccessExpressionBuilder : IBuilder<il_ast.MemberAccessExpression>
{
    private Expression _Lhs;
    private Expression _Rhs;

    public il_ast.MemberAccessExpression Build()
    {
        return new il_ast.MemberAccessExpression()
        {
            Lhs = this._Lhs,
            Rhs = this._Rhs
        };
    }
    public MemberAccessExpressionBuilder WithLhs(Expression value)
    {
        _Lhs = value;
        return this;
    }

    public MemberAccessExpressionBuilder WithRhs(Expression value)
    {
        _Rhs = value;
        return this;
    }

}

public class ParameterDeclarationBuilder : IBuilder<il_ast.ParameterDeclaration>
{
    private string _Name;
    private string _TypeName;
    private bool _IsUDTType;

    public il_ast.ParameterDeclaration Build()
    {
        return new il_ast.ParameterDeclaration()
        {
            Name = this._Name,
            TypeName = this._TypeName,
            IsUDTType = this._IsUDTType
        };
    }
    public ParameterDeclarationBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public ParameterDeclarationBuilder WithTypeName(string value)
    {
        _TypeName = value;
        return this;
    }

    public ParameterDeclarationBuilder WithIsUDTType(bool value)
    {
        _IsUDTType = value;
        return this;
    }

}

public class ParameterSignatureBuilder : IBuilder<il_ast.ParameterSignature>
{
    private InOutFlag _InOut;
    private string _Name;
    private TypeReference _TypeReference;
    private bool _IsUDTType;

    public il_ast.ParameterSignature Build()
    {
        return new il_ast.ParameterSignature()
        {
            InOut = this._InOut,
            Name = this._Name,
            TypeReference = this._TypeReference,
            IsUDTType = this._IsUDTType
        };
    }
    public ParameterSignatureBuilder WithInOut(InOutFlag value)
    {
        _InOut = value;
        return this;
    }

    public ParameterSignatureBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public ParameterSignatureBuilder WithTypeReference(TypeReference value)
    {
        _TypeReference = value;
        return this;
    }

    public ParameterSignatureBuilder WithIsUDTType(bool value)
    {
        _IsUDTType = value;
        return this;
    }

}

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

public class MethodHeaderBuilder : IBuilder<il_ast.MethodHeader>
{
    private FunctionKind _FunctionKind;
    private bool _IsEntrypoint;

    public il_ast.MethodHeader Build()
    {
        return new il_ast.MethodHeader()
        {
            FunctionKind = this._FunctionKind,
            IsEntrypoint = this._IsEntrypoint
        };
    }
    public MethodHeaderBuilder WithFunctionKind(FunctionKind value)
    {
        _FunctionKind = value;
        return this;
    }

    public MethodHeaderBuilder WithIsEntrypoint(bool value)
    {
        _IsEntrypoint = value;
        return this;
    }

}

public class MethodRefBuilder : IBuilder<il_ast.MethodRef>
{

    public il_ast.MethodRef Build()
    {
        return new il_ast.MethodRef()
        {
        };
    }
}

public class MethodImplBuilder : IBuilder<il_ast.MethodImpl>
{
    private ImplementationFlag _ImplementationFlags;
    private bool _IsManaged;
    private Block _Body;

    public il_ast.MethodImpl Build()
    {
        return new il_ast.MethodImpl()
        {
            ImplementationFlags = this._ImplementationFlags,
            IsManaged = this._IsManaged,
            Body = this._Body
        };
    }
    public MethodImplBuilder WithImplementationFlags(ImplementationFlag value)
    {
        _ImplementationFlags = value;
        return this;
    }

    public MethodImplBuilder WithIsManaged(bool value)
    {
        _IsManaged = value;
        return this;
    }

    public MethodImplBuilder WithBody(Block value)
    {
        _Body = value;
        return this;
    }

}

public class MethodDefinitionBuilder : IBuilder<il_ast.MethodDefinition>
{
    private MethodHeader _Header;
    private MethodSignature _Signature;
    private MethodImpl _Impl;
    private CodeTypeFlag _CodeTypeFlags;

    public il_ast.MethodDefinition Build()
    {
        return new il_ast.MethodDefinition()
        {
            Header = this._Header,
            Signature = this._Signature,
            Impl = this._Impl,
            CodeTypeFlags = this._CodeTypeFlags
        };
    }
    public MethodDefinitionBuilder WithHeader(MethodHeader value)
    {
        _Header = value;
        return this;
    }

    public MethodDefinitionBuilder WithSignature(MethodSignature value)
    {
        _Signature = value;
        return this;
    }

    public MethodDefinitionBuilder WithImpl(MethodImpl value)
    {
        _Impl = value;
        return this;
    }

    public MethodDefinitionBuilder WithCodeTypeFlags(CodeTypeFlag value)
    {
        _CodeTypeFlags = value;
        return this;
    }

}

public class TypeReferenceBuilder : IBuilder<il_ast.TypeReference>
{
    private string _Namespace;
    private string _Name;
    private string _ModuleName;

    public il_ast.TypeReference Build()
    {
        return new il_ast.TypeReference()
        {
            Namespace = this._Namespace,
            Name = this._Name,
            ModuleName = this._ModuleName
        };
    }
    public TypeReferenceBuilder WithNamespace(string value)
    {
        _Namespace = value;
        return this;
    }

    public TypeReferenceBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public TypeReferenceBuilder WithModuleName(string value)
    {
        _ModuleName = value;
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

public class FieldDefinitionBuilder : IBuilder<il_ast.FieldDefinition>
{

    public il_ast.FieldDefinition Build()
    {
        return new il_ast.FieldDefinition()
        {
        };
    }
}

public class PropertyDefinitionBuilder : IBuilder<il_ast.PropertyDefinition>
{
    private string _TypeName;
    private string _Name;
    private FieldDefinition? _FieldDefinition;

    public il_ast.PropertyDefinition Build()
    {
        return new il_ast.PropertyDefinition()
        {
            TypeName = this._TypeName,
            Name = this._Name,
            FieldDefinition = this._FieldDefinition
        };
    }
    public PropertyDefinitionBuilder WithTypeName(string value)
    {
        _TypeName = value;
        return this;
    }

    public PropertyDefinitionBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public PropertyDefinitionBuilder WithFieldDefinition(FieldDefinition? value)
    {
        _FieldDefinition = value;
        return this;
    }

}

public class BlockBuilder : IBuilder<il_ast.Block>
{
    private List<Statement> _Statements;

    public il_ast.Block Build()
    {
        return new il_ast.Block()
        {
            Statements = this._Statements
        };
    }
    public BlockBuilder WithStatements(List<Statement> value)
    {
        _Statements = value;
        return this;
    }

    public BlockBuilder AddingItemToStatements(Statement value)
    {
        _Statements ??= new List<Statement>();
        _Statements.Add(value);
        return this;
    }

}

public class IfStatementBuilder : IBuilder<il_ast.IfStatement>
{
    private Expression _Conditional;
    private Block _IfBlock;
    private Block? _ElseBlock;

    public il_ast.IfStatement Build()
    {
        return new il_ast.IfStatement()
        {
            Conditional = this._Conditional,
            IfBlock = this._IfBlock,
            ElseBlock = this._ElseBlock
        };
    }
    public IfStatementBuilder WithConditional(Expression value)
    {
        _Conditional = value;
        return this;
    }

    public IfStatementBuilder WithIfBlock(Block value)
    {
        _IfBlock = value;
        return this;
    }

    public IfStatementBuilder WithElseBlock(Block? value)
    {
        _ElseBlock = value;
        return this;
    }

}

public class VariableAssignmentStatementBuilder : IBuilder<il_ast.VariableAssignmentStatement>
{
    private int? _Ordinal;
    private string _LHS;
    private Expression _RHS;

    public il_ast.VariableAssignmentStatement Build()
    {
        return new il_ast.VariableAssignmentStatement()
        {
            Ordinal = this._Ordinal,
            LHS = this._LHS,
            RHS = this._RHS
        };
    }
    public VariableAssignmentStatementBuilder WithOrdinal(int? value)
    {
        _Ordinal = value;
        return this;
    }

    public VariableAssignmentStatementBuilder WithLHS(string value)
    {
        _LHS = value;
        return this;
    }

    public VariableAssignmentStatementBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

}

public class VariableDeclarationStatementBuilder : IBuilder<il_ast.VariableDeclarationStatement>
{
    private int? _Ordinal;
    private string _Name;
    private string _TypeName;
    private Expression? _InitialisationExpression;

    public il_ast.VariableDeclarationStatement Build()
    {
        return new il_ast.VariableDeclarationStatement()
        {
            Ordinal = this._Ordinal,
            Name = this._Name,
            TypeName = this._TypeName,
            InitialisationExpression = this._InitialisationExpression
        };
    }
    public VariableDeclarationStatementBuilder WithOrdinal(int? value)
    {
        _Ordinal = value;
        return this;
    }

    public VariableDeclarationStatementBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public VariableDeclarationStatementBuilder WithTypeName(string value)
    {
        _TypeName = value;
        return this;
    }

    public VariableDeclarationStatementBuilder WithInitialisationExpression(Expression? value)
    {
        _InitialisationExpression = value;
        return this;
    }

}

public class ReturnStatementBuilder : IBuilder<il_ast.ReturnStatement>
{
    private Expression _Exp;

    public il_ast.ReturnStatement Build()
    {
        return new il_ast.ReturnStatement()
        {
            Exp = this._Exp
        };
    }
    public ReturnStatementBuilder WithExp(Expression value)
    {
        _Exp = value;
        return this;
    }

}

public class WhileStatementBuilder : IBuilder<il_ast.WhileStatement>
{
    private Expression _Conditional;
    private Block _LoopBlock;

    public il_ast.WhileStatement Build()
    {
        return new il_ast.WhileStatement()
        {
            Conditional = this._Conditional,
            LoopBlock = this._LoopBlock
        };
    }
    public WhileStatementBuilder WithConditional(Expression value)
    {
        _Conditional = value;
        return this;
    }

    public WhileStatementBuilder WithLoopBlock(Block value)
    {
        _LoopBlock = value;
        return this;
    }

}

public class ExpressionStatementBuilder : IBuilder<il_ast.ExpressionStatement>
{
    private Expression _Expression;

    public il_ast.ExpressionStatement Build()
    {
        return new il_ast.ExpressionStatement()
        {
            Expression = this._Expression
        };
    }
    public ExpressionStatementBuilder WithExpression(Expression value)
    {
        _Expression = value;
        return this;
    }

}

public class UnaryExpressionBuilder : IBuilder<il_ast.UnaryExpression>
{
    private string _Op;
    private Expression _Exp;

    public il_ast.UnaryExpression Build()
    {
        return new il_ast.UnaryExpression()
        {
            Op = this._Op,
            Exp = this._Exp
        };
    }
    public UnaryExpressionBuilder WithOp(string value)
    {
        _Op = value;
        return this;
    }

    public UnaryExpressionBuilder WithExp(Expression value)
    {
        _Exp = value;
        return this;
    }

}

public class BinaryExpressionBuilder : IBuilder<il_ast.BinaryExpression>
{
    private string _Op;
    private Expression _LHS;
    private Expression _RHS;

    public il_ast.BinaryExpression Build()
    {
        return new il_ast.BinaryExpression()
        {
            Op = this._Op,
            LHS = this._LHS,
            RHS = this._RHS
        };
    }
    public BinaryExpressionBuilder WithOp(string value)
    {
        _Op = value;
        return this;
    }

    public BinaryExpressionBuilder WithLHS(Expression value)
    {
        _LHS = value;
        return this;
    }

    public BinaryExpressionBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

}

public class VariableReferenceExpressionBuilder : IBuilder<il_ast.VariableReferenceExpression>
{
    private string _Name;
    private object _SymTabEntry;
    private bool _IsParameterReference;
    private int _Ordinal;

    public il_ast.VariableReferenceExpression Build()
    {
        return new il_ast.VariableReferenceExpression()
        {
            Name = this._Name,
            SymTabEntry = this._SymTabEntry,
            IsParameterReference = this._IsParameterReference,
            Ordinal = this._Ordinal
        };
    }
    public VariableReferenceExpressionBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public VariableReferenceExpressionBuilder WithSymTabEntry(object value)
    {
        _SymTabEntry = value;
        return this;
    }

    public VariableReferenceExpressionBuilder WithIsParameterReference(bool value)
    {
        _IsParameterReference = value;
        return this;
    }

    public VariableReferenceExpressionBuilder WithOrdinal(int value)
    {
        _Ordinal = value;
        return this;
    }

}

public class TypeCastExpressionBuilder : IBuilder<il_ast.TypeCastExpression>
{
    private string _TargetTypeName;
    private string _TargetTypeCilName;
    private Expression _Expression;

    public il_ast.TypeCastExpression Build()
    {
        return new il_ast.TypeCastExpression()
        {
            TargetTypeName = this._TargetTypeName,
            TargetTypeCilName = this._TargetTypeCilName,
            Expression = this._Expression
        };
    }
    public TypeCastExpressionBuilder WithTargetTypeName(string value)
    {
        _TargetTypeName = value;
        return this;
    }

    public TypeCastExpressionBuilder WithTargetTypeCilName(string value)
    {
        _TargetTypeCilName = value;
        return this;
    }

    public TypeCastExpressionBuilder WithExpression(Expression value)
    {
        _Expression = value;
        return this;
    }

}

public class FuncCallExpBuilder : IBuilder<il_ast.FuncCallExp>
{
    private string _Name;
    private List<Expression> _Args;
    private string _ReturnType;
    private ClassDefinition _ClassDefinition;
    private List<string> _ArgTypes;

    public il_ast.FuncCallExp Build()
    {
        return new il_ast.FuncCallExp()
        {
            Name = this._Name,
            Args = this._Args,
            ReturnType = this._ReturnType,
            ClassDefinition = this._ClassDefinition,
            ArgTypes = this._ArgTypes
        };
    }
    public FuncCallExpBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public FuncCallExpBuilder WithArgs(List<Expression> value)
    {
        _Args = value;
        return this;
    }

    public FuncCallExpBuilder AddingItemToArgs(Expression value)
    {
        _Args ??= new List<Expression>();
        _Args.Add(value);
        return this;
    }

    public FuncCallExpBuilder WithReturnType(string value)
    {
        _ReturnType = value;
        return this;
    }

    public FuncCallExpBuilder WithClassDefinition(ClassDefinition value)
    {
        _ClassDefinition = value;
        return this;
    }

    public FuncCallExpBuilder WithArgTypes(List<string> value)
    {
        _ArgTypes = value;
        return this;
    }

    public FuncCallExpBuilder AddingItemToArgTypes(string value)
    {
        _ArgTypes ??= new List<string>();
        _ArgTypes.Add(value);
        return this;
    }

}

public class BoolLiteralBuilder : IBuilder<il_ast.BoolLiteral>
{

    public il_ast.BoolLiteral Build()
    {
        return new il_ast.BoolLiteral()
        {
        };
    }
}

public class CharLiteralBuilder : IBuilder<il_ast.CharLiteral>
{

    public il_ast.CharLiteral Build()
    {
        return new il_ast.CharLiteral()
        {
        };
    }
}

public class StringLiteralBuilder : IBuilder<il_ast.StringLiteral>
{

    public il_ast.StringLiteral Build()
    {
        return new il_ast.StringLiteral()
        {
        };
    }
}

public class UriLiteralBuilder : IBuilder<il_ast.UriLiteral>
{

    public il_ast.UriLiteral Build()
    {
        return new il_ast.UriLiteral()
        {
        };
    }
}

public class DateTimeOffsetLiteralBuilder : IBuilder<il_ast.DateTimeOffsetLiteral>
{

    public il_ast.DateTimeOffsetLiteral Build()
    {
        return new il_ast.DateTimeOffsetLiteral()
        {
        };
    }
}

public class DateOnlyLiteralBuilder : IBuilder<il_ast.DateOnlyLiteral>
{

    public il_ast.DateOnlyLiteral Build()
    {
        return new il_ast.DateOnlyLiteral()
        {
        };
    }
}

public class TimeOnlyLiteralBuilder : IBuilder<il_ast.TimeOnlyLiteral>
{

    public il_ast.TimeOnlyLiteral Build()
    {
        return new il_ast.TimeOnlyLiteral()
        {
        };
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

public class ByteLiteralBuilder : IBuilder<il_ast.ByteLiteral>
{

    public il_ast.ByteLiteral Build()
    {
        return new il_ast.ByteLiteral()
        {
        };
    }
}

public class ShortLiteralBuilder : IBuilder<il_ast.ShortLiteral>
{

    public il_ast.ShortLiteral Build()
    {
        return new il_ast.ShortLiteral()
        {
        };
    }
}

public class UShortLiteralBuilder : IBuilder<il_ast.UShortLiteral>
{

    public il_ast.UShortLiteral Build()
    {
        return new il_ast.UShortLiteral()
        {
        };
    }
}

public class IntLiteralBuilder : IBuilder<il_ast.IntLiteral>
{

    public il_ast.IntLiteral Build()
    {
        return new il_ast.IntLiteral()
        {
        };
    }
}

public class UIntLiteralBuilder : IBuilder<il_ast.UIntLiteral>
{

    public il_ast.UIntLiteral Build()
    {
        return new il_ast.UIntLiteral()
        {
        };
    }
}

public class LongLiteralBuilder : IBuilder<il_ast.LongLiteral>
{

    public il_ast.LongLiteral Build()
    {
        return new il_ast.LongLiteral()
        {
        };
    }
}

public class ULongLiteralBuilder : IBuilder<il_ast.ULongLiteral>
{

    public il_ast.ULongLiteral Build()
    {
        return new il_ast.ULongLiteral()
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

public class DoubleLiteralBuilder : IBuilder<il_ast.DoubleLiteral>
{

    public il_ast.DoubleLiteral Build()
    {
        return new il_ast.DoubleLiteral()
        {
        };
    }
}

public class DecimalLiteralBuilder : IBuilder<il_ast.DecimalLiteral>
{

    public il_ast.DecimalLiteral Build()
    {
        return new il_ast.DecimalLiteral()
        {
        };
    }
}

#nullable restore
