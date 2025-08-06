namespace ast_generated;
using ast_generated;
using ast;
using ast_model.TypeSystem;
using System.Collections.Generic;
#nullable disable

public class AssemblyDefBuilder : IBuilder<ast.AssemblyDef>
{
    private Visibility _Visibility = Visibility.Public;
    private AssemblyName _Name;
    private string _PublicKeyToken;
    private string _Version;
    private List<AssemblyRef> _AssemblyRefs;
    private List<ModuleDef> _Modules;
    private string _TestProperty;
    private Dictionary<string, object> _Annotations = new();

    public ast.AssemblyDef Build()
    {
        return new ast.AssemblyDef()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            PublicKeyToken = this._PublicKeyToken,
            Version = this._Version,
            AssemblyRefs = this._AssemblyRefs,
            Modules = this._Modules,
            TestProperty = this._TestProperty,
            Annotations = this._Annotations
        };
    }
    public AssemblyDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public AssemblyDefBuilder WithName(AssemblyName value)
    {
        _Name = value;
        return this;
    }

    public AssemblyDefBuilder WithPublicKeyToken(string value)
    {
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyDefBuilder WithVersion(string value)
    {
        _Version = value;
        return this;
    }

    public AssemblyDefBuilder WithAssemblyRefs(List<AssemblyRef> value)
    {
        _AssemblyRefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToAssemblyRefs(AssemblyRef value)
    {
        _AssemblyRefs ??= new List<AssemblyRef>();
        _AssemblyRefs.Add(value);
        return this;
    }

    public AssemblyDefBuilder WithModules(List<ModuleDef> value)
    {
        _Modules = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToModules(ModuleDef value)
    {
        _Modules ??= new List<ModuleDef>();
        _Modules.Add(value);
        return this;
    }

    public AssemblyDefBuilder WithTestProperty(string value)
    {
        _TestProperty = value;
        return this;
    }

    public AssemblyDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ModuleDefBuilder : IBuilder<ast.ModuleDef>
{
    private Visibility _Visibility = Visibility.Public;
    private string _OriginalModuleName;
    private NamespaceName _NamespaceDecl;
    private List<ClassDef> _Classes;
    private List<FunctionDef> _Functions;
    private Dictionary<string, object> _Annotations = new();

    public ast.ModuleDef Build()
    {
        return new ast.ModuleDef()
        {
            Visibility = this._Visibility,
            OriginalModuleName = this._OriginalModuleName,
            NamespaceDecl = this._NamespaceDecl,
            Classes = this._Classes,
            Functions = this._Functions,
            Annotations = this._Annotations
        };
    }
    public ModuleDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public ModuleDefBuilder WithOriginalModuleName(string value)
    {
        _OriginalModuleName = value;
        return this;
    }

    public ModuleDefBuilder WithNamespaceDecl(NamespaceName value)
    {
        _NamespaceDecl = value;
        return this;
    }

    public ModuleDefBuilder WithClasses(List<ClassDef> value)
    {
        _Classes = value;
        return this;
    }

    public ModuleDefBuilder AddingItemToClasses(ClassDef value)
    {
        _Classes ??= new List<ClassDef>();
        _Classes.Add(value);
        return this;
    }

    public ModuleDefBuilder WithFunctions(List<FunctionDef> value)
    {
        _Functions = value;
        return this;
    }

    public ModuleDefBuilder AddingItemToFunctions(FunctionDef value)
    {
        _Functions ??= new List<FunctionDef>();
        _Functions.Add(value);
        return this;
    }

    public ModuleDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class FunctionDefBuilder : IBuilder<ast.FunctionDef>
{
    private Visibility _Visibility = Visibility.Public;
    private List<ParamDef> _Params;
    private BlockStatement _Body;
    private FifthType _ReturnType;
    private MemberName _Name;
    private bool _IsStatic;
    private bool _IsConstructor;
    private Dictionary<string, object> _Annotations = new();

    public ast.FunctionDef Build()
    {
        return new ast.FunctionDef()
        {
            Visibility = this._Visibility,
            Params = this._Params,
            Body = this._Body,
            ReturnType = this._ReturnType,
            Name = this._Name,
            IsStatic = this._IsStatic,
            IsConstructor = this._IsConstructor,
            Annotations = this._Annotations
        };
    }
    public FunctionDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public FunctionDefBuilder WithParams(List<ParamDef> value)
    {
        _Params = value;
        return this;
    }

    public FunctionDefBuilder AddingItemToParams(ParamDef value)
    {
        _Params ??= new List<ParamDef>();
        _Params.Add(value);
        return this;
    }

    public FunctionDefBuilder WithBody(BlockStatement value)
    {
        _Body = value;
        return this;
    }

    public FunctionDefBuilder WithReturnType(FifthType value)
    {
        _ReturnType = value;
        return this;
    }

    public FunctionDefBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public FunctionDefBuilder WithIsStatic(bool value)
    {
        _IsStatic = value;
        return this;
    }

    public FunctionDefBuilder WithIsConstructor(bool value)
    {
        _IsConstructor = value;
        return this;
    }

    public FunctionDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class FunctorDefBuilder : IBuilder<ast.FunctorDef>
{
    private FunctionDef _InvocationFuncDev;
    private Dictionary<string, object> _Annotations = new();

    public ast.FunctorDef Build()
    {
        return new ast.FunctorDef()
        {
            InvocationFuncDev = this._InvocationFuncDev,
            Annotations = this._Annotations
        };
    }
    public FunctorDefBuilder WithInvocationFuncDev(FunctionDef value)
    {
        _InvocationFuncDev = value;
        return this;
    }

    public FunctorDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class FieldDefBuilder : IBuilder<ast.FieldDef>
{
    private Visibility _Visibility = Visibility.Public;
    private MemberName _Name = MemberName.From("DefaultName");
    private TypeName _TypeName = TypeName.From("DefaultType");
    private bool _IsReadOnly = false;
    private AccessConstraint[] _AccessConstraints;
    private Dictionary<string, object> _Annotations = new();

    public ast.FieldDef Build()
    {
        return new ast.FieldDef()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            TypeName = this._TypeName,
            IsReadOnly = this._IsReadOnly,
            AccessConstraints = this._AccessConstraints,
            Annotations = this._Annotations
        };
    }
    public FieldDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public FieldDefBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public FieldDefBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public FieldDefBuilder WithIsReadOnly(bool value)
    {
        _IsReadOnly = value;
        return this;
    }

    public FieldDefBuilder WithAccessConstraints(AccessConstraint[] value)
    {
        _AccessConstraints = value;
        return this;
    }

    public FieldDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class PropertyDefBuilder : IBuilder<ast.PropertyDef>
{
    private Visibility _Visibility = Visibility.Public;
    private MemberName _Name = MemberName.From("DefaultName");
    private TypeName _TypeName = TypeName.From("DefaultType");
    private bool _IsReadOnly = false;
    private AccessConstraint[] _AccessConstraints;
    private bool _IsWriteOnly;
    private FieldDef? _BackingField;
    private MethodDef? _Getter;
    private MethodDef? _Setter;
    private bool _CtorOnlySetter;
    private Dictionary<string, object> _Annotations = new();

    public ast.PropertyDef Build()
    {
        return new ast.PropertyDef()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            TypeName = this._TypeName,
            IsReadOnly = this._IsReadOnly,
            AccessConstraints = this._AccessConstraints,
            IsWriteOnly = this._IsWriteOnly,
            BackingField = this._BackingField,
            Getter = this._Getter,
            Setter = this._Setter,
            CtorOnlySetter = this._CtorOnlySetter,
            Annotations = this._Annotations
        };
    }
    public PropertyDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public PropertyDefBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public PropertyDefBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public PropertyDefBuilder WithIsReadOnly(bool value)
    {
        _IsReadOnly = value;
        return this;
    }

    public PropertyDefBuilder WithAccessConstraints(AccessConstraint[] value)
    {
        _AccessConstraints = value;
        return this;
    }

    public PropertyDefBuilder WithIsWriteOnly(bool value)
    {
        _IsWriteOnly = value;
        return this;
    }

    public PropertyDefBuilder WithBackingField(FieldDef? value)
    {
        _BackingField = value;
        return this;
    }

    public PropertyDefBuilder WithGetter(MethodDef? value)
    {
        _Getter = value;
        return this;
    }

    public PropertyDefBuilder WithSetter(MethodDef? value)
    {
        _Setter = value;
        return this;
    }

    public PropertyDefBuilder WithCtorOnlySetter(bool value)
    {
        _CtorOnlySetter = value;
        return this;
    }

    public PropertyDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class MethodDefBuilder : IBuilder<ast.MethodDef>
{
    private Visibility _Visibility = Visibility.Public;
    private MemberName _Name = MemberName.From("DefaultName");
    private TypeName _TypeName = TypeName.From("DefaultType");
    private bool _IsReadOnly = false;
    private FunctionDef _FunctionDef;
    private Dictionary<string, object> _Annotations = new();

    public ast.MethodDef Build()
    {
        return new ast.MethodDef()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            TypeName = this._TypeName,
            IsReadOnly = this._IsReadOnly,
            FunctionDef = this._FunctionDef,
            Annotations = this._Annotations
        };
    }
    public MethodDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public MethodDefBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public MethodDefBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public MethodDefBuilder WithIsReadOnly(bool value)
    {
        _IsReadOnly = value;
        return this;
    }

    public MethodDefBuilder WithFunctionDef(FunctionDef value)
    {
        _FunctionDef = value;
        return this;
    }

    public MethodDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class OverloadedFunctionDefinitionBuilder : IBuilder<ast.OverloadedFunctionDefinition>
{
    private Visibility _Visibility = Visibility.Public;
    private MemberName _Name = MemberName.From("DefaultName");
    private TypeName _TypeName = TypeName.From("DefaultType");
    private bool _IsReadOnly = false;
    private List<MethodDef> _OverloadClauses;
    private IFunctionSignature _Signature;
    private Dictionary<string, object> _Annotations = new();

    public ast.OverloadedFunctionDefinition Build()
    {
        return new ast.OverloadedFunctionDefinition()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            TypeName = this._TypeName,
            IsReadOnly = this._IsReadOnly,
            OverloadClauses = this._OverloadClauses,
            Signature = this._Signature,
            Annotations = this._Annotations
        };
    }
    public OverloadedFunctionDefinitionBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithIsReadOnly(bool value)
    {
        _IsReadOnly = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithOverloadClauses(List<MethodDef> value)
    {
        _OverloadClauses = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder AddingItemToOverloadClauses(MethodDef value)
    {
        _OverloadClauses ??= new List<MethodDef>();
        _OverloadClauses.Add(value);
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithSignature(IFunctionSignature value)
    {
        _Signature = value;
        return this;
    }

    public OverloadedFunctionDefinitionBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class InferenceRuleDefBuilder : IBuilder<ast.InferenceRuleDef>
{
    private Visibility _Visibility = Visibility.Public;
    private Expression _Antecedent;
    private KnowledgeManagementBlock _Consequent;
    private Dictionary<string, object> _Annotations = new();

    public ast.InferenceRuleDef Build()
    {
        return new ast.InferenceRuleDef()
        {
            Visibility = this._Visibility,
            Antecedent = this._Antecedent,
            Consequent = this._Consequent,
            Annotations = this._Annotations
        };
    }
    public InferenceRuleDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public InferenceRuleDefBuilder WithAntecedent(Expression value)
    {
        _Antecedent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithConsequent(KnowledgeManagementBlock value)
    {
        _Consequent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ParamDefBuilder : IBuilder<ast.ParamDef>
{
    private Visibility _Visibility = Visibility.Public;
    private TypeName _TypeName;
    private string _Name;
    private Expression? _ParameterConstraint;
    private ParamDestructureDef? _DestructureDef;
    private Dictionary<string, object> _Annotations = new();

    public ast.ParamDef Build()
    {
        return new ast.ParamDef()
        {
            Visibility = this._Visibility,
            TypeName = this._TypeName,
            Name = this._Name,
            ParameterConstraint = this._ParameterConstraint,
            DestructureDef = this._DestructureDef,
            Annotations = this._Annotations
        };
    }
    public ParamDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public ParamDefBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public ParamDefBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public ParamDefBuilder WithParameterConstraint(Expression? value)
    {
        _ParameterConstraint = value;
        return this;
    }

    public ParamDefBuilder WithDestructureDef(ParamDestructureDef? value)
    {
        _DestructureDef = value;
        return this;
    }

    public ParamDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ParamDestructureDefBuilder : IBuilder<ast.ParamDestructureDef>
{
    private Visibility _Visibility = Visibility.Public;
    private List<PropertyBindingDef> _Bindings;
    private Dictionary<string, object> _Annotations = new();

    public ast.ParamDestructureDef Build()
    {
        return new ast.ParamDestructureDef()
        {
            Visibility = this._Visibility,
            Bindings = this._Bindings,
            Annotations = this._Annotations
        };
    }
    public ParamDestructureDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public ParamDestructureDefBuilder WithBindings(List<PropertyBindingDef> value)
    {
        _Bindings = value;
        return this;
    }

    public ParamDestructureDefBuilder AddingItemToBindings(PropertyBindingDef value)
    {
        _Bindings ??= new List<PropertyBindingDef>();
        _Bindings.Add(value);
        return this;
    }

    public ParamDestructureDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class PropertyBindingDefBuilder : IBuilder<ast.PropertyBindingDef>
{
    private Visibility _Visibility = Visibility.Public;
    private MemberName _IntroducedVariable;
    private MemberName _ReferencedPropertyName;
    private PropertyDef? _ReferencedProperty;
    private ParamDestructureDef? _DestructureDef;
    private Dictionary<string, object> _Annotations = new();

    public ast.PropertyBindingDef Build()
    {
        return new ast.PropertyBindingDef()
        {
            Visibility = this._Visibility,
            IntroducedVariable = this._IntroducedVariable,
            ReferencedPropertyName = this._ReferencedPropertyName,
            ReferencedProperty = this._ReferencedProperty,
            DestructureDef = this._DestructureDef,
            Annotations = this._Annotations
        };
    }
    public PropertyBindingDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public PropertyBindingDefBuilder WithIntroducedVariable(MemberName value)
    {
        _IntroducedVariable = value;
        return this;
    }

    public PropertyBindingDefBuilder WithReferencedPropertyName(MemberName value)
    {
        _ReferencedPropertyName = value;
        return this;
    }

    public PropertyBindingDefBuilder WithReferencedProperty(PropertyDef? value)
    {
        _ReferencedProperty = value;
        return this;
    }

    public PropertyBindingDefBuilder WithDestructureDef(ParamDestructureDef? value)
    {
        _DestructureDef = value;
        return this;
    }

    public PropertyBindingDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class TypeDefBuilder : IBuilder<ast.TypeDef>
{
    private Visibility _Visibility = Visibility.Public;
    private Dictionary<string, object> _Annotations = new();

    public ast.TypeDef Build()
    {
        return new ast.TypeDef()
        {
            Visibility = this._Visibility,
            Annotations = this._Annotations
        };
    }
    public TypeDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public TypeDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ClassDefBuilder : IBuilder<ast.ClassDef>
{
    private Visibility _Visibility = Visibility.Public;
    private TypeName _Name;
    private List<MemberDef> _MemberDefs;
    private Dictionary<string, object> _Annotations = new();

    public ast.ClassDef Build()
    {
        return new ast.ClassDef()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            MemberDefs = this._MemberDefs,
            Annotations = this._Annotations
        };
    }
    public ClassDefBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public ClassDefBuilder WithName(TypeName value)
    {
        _Name = value;
        return this;
    }

    public ClassDefBuilder WithMemberDefs(List<MemberDef> value)
    {
        _MemberDefs = value;
        return this;
    }

    public ClassDefBuilder AddingItemToMemberDefs(MemberDef value)
    {
        _MemberDefs ??= new List<MemberDef>();
        _MemberDefs.Add(value);
        return this;
    }

    public ClassDefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class VariableDeclBuilder : IBuilder<ast.VariableDecl>
{
    private Visibility _Visibility = Visibility.Public;
    private string _Name;
    private TypeName _TypeName;
    private CollectionType _CollectionType;
    private Dictionary<string, object> _Annotations = new();

    public ast.VariableDecl Build()
    {
        return new ast.VariableDecl()
        {
            Visibility = this._Visibility,
            Name = this._Name,
            TypeName = this._TypeName,
            CollectionType = this._CollectionType,
            Annotations = this._Annotations
        };
    }
    public VariableDeclBuilder WithVisibility(Visibility value)
    {
        _Visibility = value;
        return this;
    }

    public VariableDeclBuilder WithName(string value)
    {
        _Name = value;
        return this;
    }

    public VariableDeclBuilder WithTypeName(TypeName value)
    {
        _TypeName = value;
        return this;
    }

    public VariableDeclBuilder WithCollectionType(CollectionType value)
    {
        _CollectionType = value;
        return this;
    }

    public VariableDeclBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssemblyRefBuilder : IBuilder<ast.AssemblyRef>
{
    private string _PublicKeyToken;
    private string _Version;
    private Dictionary<string, object> _Annotations = new();

    public ast.AssemblyRef Build()
    {
        return new ast.AssemblyRef()
        {
            PublicKeyToken = this._PublicKeyToken,
            Version = this._Version,
            Annotations = this._Annotations
        };
    }
    public AssemblyRefBuilder WithPublicKeyToken(string value)
    {
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyRefBuilder WithVersion(string value)
    {
        _Version = value;
        return this;
    }

    public AssemblyRefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class MemberRefBuilder : IBuilder<ast.MemberRef>
{
    private MemberDef _Member;
    private Dictionary<string, object> _Annotations = new();

    public ast.MemberRef Build()
    {
        return new ast.MemberRef()
        {
            Member = this._Member,
            Annotations = this._Annotations
        };
    }
    public MemberRefBuilder WithMember(MemberDef value)
    {
        _Member = value;
        return this;
    }

    public MemberRefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class PropertyRefBuilder : IBuilder<ast.PropertyRef>
{
    private PropertyDef _Property;
    private Dictionary<string, object> _Annotations = new();

    public ast.PropertyRef Build()
    {
        return new ast.PropertyRef()
        {
            Property = this._Property,
            Annotations = this._Annotations
        };
    }
    public PropertyRefBuilder WithProperty(PropertyDef value)
    {
        _Property = value;
        return this;
    }

    public PropertyRefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class TypeRefBuilder : IBuilder<ast.TypeRef>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.TypeRef Build()
    {
        return new ast.TypeRef()
        {
            Annotations = this._Annotations
        };
    }
    public TypeRefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class VarRefBuilder : IBuilder<ast.VarRef>
{
    private MemberName _ReferencedVariableName;
    private Dictionary<string, object> _Annotations = new();

    public ast.VarRef Build()
    {
        return new ast.VarRef()
        {
            ReferencedVariableName = this._ReferencedVariableName,
            Annotations = this._Annotations
        };
    }
    public VarRefBuilder WithReferencedVariableName(MemberName value)
    {
        _ReferencedVariableName = value;
        return this;
    }

    public VarRefBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class GraphNamespaceAliasBuilder : IBuilder<ast.GraphNamespaceAlias>
{
    private Uri _Uri;
    private Dictionary<string, object> _Annotations = new();

    public ast.GraphNamespaceAlias Build()
    {
        return new ast.GraphNamespaceAlias()
        {
            Uri = this._Uri,
            Annotations = this._Annotations
        };
    }
    public GraphNamespaceAliasBuilder WithUri(Uri value)
    {
        _Uri = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssignmentStatementBuilder : IBuilder<ast.AssignmentStatement>
{
    private Expression _LValue;
    private Expression _RValue;
    private Dictionary<string, object> _Annotations = new();

    public ast.AssignmentStatement Build()
    {
        return new ast.AssignmentStatement()
        {
            LValue = this._LValue,
            RValue = this._RValue,
            Annotations = this._Annotations
        };
    }
    public AssignmentStatementBuilder WithLValue(Expression value)
    {
        _LValue = value;
        return this;
    }

    public AssignmentStatementBuilder WithRValue(Expression value)
    {
        _RValue = value;
        return this;
    }

    public AssignmentStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class BlockStatementBuilder : IBuilder<ast.BlockStatement>
{
    private List<Statement> _Statements;
    private Dictionary<string, object> _Annotations = new();

    public ast.BlockStatement Build()
    {
        return new ast.BlockStatement()
        {
            Statements = this._Statements,
            Annotations = this._Annotations
        };
    }
    public BlockStatementBuilder WithStatements(List<Statement> value)
    {
        _Statements = value;
        return this;
    }

    public BlockStatementBuilder AddingItemToStatements(Statement value)
    {
        _Statements ??= new List<Statement>();
        _Statements.Add(value);
        return this;
    }

    public BlockStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class KnowledgeManagementBlockBuilder : IBuilder<ast.KnowledgeManagementBlock>
{
    private List<KnowledgeManagementStatement> _Statements;
    private Dictionary<string, object> _Annotations = new();

    public ast.KnowledgeManagementBlock Build()
    {
        return new ast.KnowledgeManagementBlock()
        {
            Statements = this._Statements,
            Annotations = this._Annotations
        };
    }
    public KnowledgeManagementBlockBuilder WithStatements(List<KnowledgeManagementStatement> value)
    {
        _Statements = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder AddingItemToStatements(KnowledgeManagementStatement value)
    {
        _Statements ??= new List<KnowledgeManagementStatement>();
        _Statements.Add(value);
        return this;
    }

    public KnowledgeManagementBlockBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ExpStatementBuilder : IBuilder<ast.ExpStatement>
{
    private Expression _RHS;
    private Dictionary<string, object> _Annotations = new();

    public ast.ExpStatement Build()
    {
        return new ast.ExpStatement()
        {
            RHS = this._RHS,
            Annotations = this._Annotations
        };
    }
    public ExpStatementBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

    public ExpStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ForStatementBuilder : IBuilder<ast.ForStatement>
{
    private Expression _InitialValue;
    private Expression _Constraint;
    private Expression _IncrementExpression;
    private VariableDecl _LoopVariable;
    private BlockStatement _Body;
    private Dictionary<string, object> _Annotations = new();

    public ast.ForStatement Build()
    {
        return new ast.ForStatement()
        {
            InitialValue = this._InitialValue,
            Constraint = this._Constraint,
            IncrementExpression = this._IncrementExpression,
            LoopVariable = this._LoopVariable,
            Body = this._Body,
            Annotations = this._Annotations
        };
    }
    public ForStatementBuilder WithInitialValue(Expression value)
    {
        _InitialValue = value;
        return this;
    }

    public ForStatementBuilder WithConstraint(Expression value)
    {
        _Constraint = value;
        return this;
    }

    public ForStatementBuilder WithIncrementExpression(Expression value)
    {
        _IncrementExpression = value;
        return this;
    }

    public ForStatementBuilder WithLoopVariable(VariableDecl value)
    {
        _LoopVariable = value;
        return this;
    }

    public ForStatementBuilder WithBody(BlockStatement value)
    {
        _Body = value;
        return this;
    }

    public ForStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ForeachStatementBuilder : IBuilder<ast.ForeachStatement>
{
    private Expression _Collection;
    private VariableDecl _LoopVariable;
    private BlockStatement _Body;
    private Dictionary<string, object> _Annotations = new();

    public ast.ForeachStatement Build()
    {
        return new ast.ForeachStatement()
        {
            Collection = this._Collection,
            LoopVariable = this._LoopVariable,
            Body = this._Body,
            Annotations = this._Annotations
        };
    }
    public ForeachStatementBuilder WithCollection(Expression value)
    {
        _Collection = value;
        return this;
    }

    public ForeachStatementBuilder WithLoopVariable(VariableDecl value)
    {
        _LoopVariable = value;
        return this;
    }

    public ForeachStatementBuilder WithBody(BlockStatement value)
    {
        _Body = value;
        return this;
    }

    public ForeachStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class GuardStatementBuilder : IBuilder<ast.GuardStatement>
{
    private Expression _Condition;
    private Dictionary<string, object> _Annotations = new();

    public ast.GuardStatement Build()
    {
        return new ast.GuardStatement()
        {
            Condition = this._Condition,
            Annotations = this._Annotations
        };
    }
    public GuardStatementBuilder WithCondition(Expression value)
    {
        _Condition = value;
        return this;
    }

    public GuardStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class IfElseStatementBuilder : IBuilder<ast.IfElseStatement>
{
    private Expression _Condition;
    private BlockStatement _ThenBlock;
    private BlockStatement _ElseBlock;
    private Dictionary<string, object> _Annotations = new();

    public ast.IfElseStatement Build()
    {
        return new ast.IfElseStatement()
        {
            Condition = this._Condition,
            ThenBlock = this._ThenBlock,
            ElseBlock = this._ElseBlock,
            Annotations = this._Annotations
        };
    }
    public IfElseStatementBuilder WithCondition(Expression value)
    {
        _Condition = value;
        return this;
    }

    public IfElseStatementBuilder WithThenBlock(BlockStatement value)
    {
        _ThenBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithElseBlock(BlockStatement value)
    {
        _ElseBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ReturnStatementBuilder : IBuilder<ast.ReturnStatement>
{
    private Expression _ReturnValue;
    private Dictionary<string, object> _Annotations = new();

    public ast.ReturnStatement Build()
    {
        return new ast.ReturnStatement()
        {
            ReturnValue = this._ReturnValue,
            Annotations = this._Annotations
        };
    }
    public ReturnStatementBuilder WithReturnValue(Expression value)
    {
        _ReturnValue = value;
        return this;
    }

    public ReturnStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class VarDeclStatementBuilder : IBuilder<ast.VarDeclStatement>
{
    private VariableDecl _VariableDecl;
    private Expression? _InitialValue;
    private Dictionary<string, object> _Annotations = new();

    public ast.VarDeclStatement Build()
    {
        return new ast.VarDeclStatement()
        {
            VariableDecl = this._VariableDecl,
            InitialValue = this._InitialValue,
            Annotations = this._Annotations
        };
    }
    public VarDeclStatementBuilder WithVariableDecl(VariableDecl value)
    {
        _VariableDecl = value;
        return this;
    }

    public VarDeclStatementBuilder WithInitialValue(Expression? value)
    {
        _InitialValue = value;
        return this;
    }

    public VarDeclStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class WhileStatementBuilder : IBuilder<ast.WhileStatement>
{
    private Expression _Condition;
    private BlockStatement _Body;
    private Dictionary<string, object> _Annotations = new();

    public ast.WhileStatement Build()
    {
        return new ast.WhileStatement()
        {
            Condition = this._Condition,
            Body = this._Body,
            Annotations = this._Annotations
        };
    }
    public WhileStatementBuilder WithCondition(Expression value)
    {
        _Condition = value;
        return this;
    }

    public WhileStatementBuilder WithBody(BlockStatement value)
    {
        _Body = value;
        return this;
    }

    public WhileStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssertionStatementBuilder : IBuilder<ast.AssertionStatement>
{
    private Triple _Assertion;
    private AssertionSubject _AssertionSubject;
    private AssertionPredicate _AssertionPredicate;
    private AssertionObject _AssertionObject;
    private Dictionary<string, object> _Annotations = new();

    public ast.AssertionStatement Build()
    {
        return new ast.AssertionStatement()
        {
            Assertion = this._Assertion,
            AssertionSubject = this._AssertionSubject,
            AssertionPredicate = this._AssertionPredicate,
            AssertionObject = this._AssertionObject,
            Annotations = this._Annotations
        };
    }
    public AssertionStatementBuilder WithAssertion(Triple value)
    {
        _Assertion = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionSubject(AssertionSubject value)
    {
        _AssertionSubject = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionPredicate(AssertionPredicate value)
    {
        _AssertionPredicate = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionObject(AssertionObject value)
    {
        _AssertionObject = value;
        return this;
    }

    public AssertionStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssertionObjectBuilder : IBuilder<ast.AssertionObject>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.AssertionObject Build()
    {
        return new ast.AssertionObject()
        {
            Annotations = this._Annotations
        };
    }
    public AssertionObjectBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssertionPredicateBuilder : IBuilder<ast.AssertionPredicate>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.AssertionPredicate Build()
    {
        return new ast.AssertionPredicate()
        {
            Annotations = this._Annotations
        };
    }
    public AssertionPredicateBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AssertionSubjectBuilder : IBuilder<ast.AssertionSubject>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.AssertionSubject Build()
    {
        return new ast.AssertionSubject()
        {
            Annotations = this._Annotations
        };
    }
    public AssertionSubjectBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class RetractionStatementBuilder : IBuilder<ast.RetractionStatement>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.RetractionStatement Build()
    {
        return new ast.RetractionStatement()
        {
            Annotations = this._Annotations
        };
    }
    public RetractionStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class WithScopeStatementBuilder : IBuilder<ast.WithScopeStatement>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.WithScopeStatement Build()
    {
        return new ast.WithScopeStatement()
        {
            Annotations = this._Annotations
        };
    }
    public WithScopeStatementBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class BinaryExpBuilder : IBuilder<ast.BinaryExp>
{
    private Expression _LHS;
    private Operator _Operator;
    private Expression _RHS;
    private Dictionary<string, object> _Annotations = new();

    public ast.BinaryExp Build()
    {
        return new ast.BinaryExp()
        {
            LHS = this._LHS,
            Operator = this._Operator,
            RHS = this._RHS,
            Annotations = this._Annotations
        };
    }
    public BinaryExpBuilder WithLHS(Expression value)
    {
        _LHS = value;
        return this;
    }

    public BinaryExpBuilder WithOperator(Operator value)
    {
        _Operator = value;
        return this;
    }

    public BinaryExpBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

    public BinaryExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class CastExpBuilder : IBuilder<ast.CastExp>
{
    private FifthType _TargetType;
    private Dictionary<string, object> _Annotations = new();

    public ast.CastExp Build()
    {
        return new ast.CastExp()
        {
            TargetType = this._TargetType,
            Annotations = this._Annotations
        };
    }
    public CastExpBuilder WithTargetType(FifthType value)
    {
        _TargetType = value;
        return this;
    }

    public CastExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class LambdaExpBuilder : IBuilder<ast.LambdaExp>
{
    private FunctorDef _FunctorDef;
    private Dictionary<string, object> _Annotations = new();

    public ast.LambdaExp Build()
    {
        return new ast.LambdaExp()
        {
            FunctorDef = this._FunctorDef,
            Annotations = this._Annotations
        };
    }
    public LambdaExpBuilder WithFunctorDef(FunctorDef value)
    {
        _FunctorDef = value;
        return this;
    }

    public LambdaExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class FuncCallExpBuilder : IBuilder<ast.FuncCallExp>
{
    private FunctionDef _FunctionDef;
    private List<Expression> _InvocationArguments;
    private Dictionary<string, object> _Annotations = new();

    public ast.FuncCallExp Build()
    {
        return new ast.FuncCallExp()
        {
            FunctionDef = this._FunctionDef,
            InvocationArguments = this._InvocationArguments,
            Annotations = this._Annotations
        };
    }
    public FuncCallExpBuilder WithFunctionDef(FunctionDef value)
    {
        _FunctionDef = value;
        return this;
    }

    public FuncCallExpBuilder WithInvocationArguments(List<Expression> value)
    {
        _InvocationArguments = value;
        return this;
    }

    public FuncCallExpBuilder AddingItemToInvocationArguments(Expression value)
    {
        _InvocationArguments ??= new List<Expression>();
        _InvocationArguments.Add(value);
        return this;
    }

    public FuncCallExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Int8LiteralExpBuilder : IBuilder<ast.Int8LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Int8LiteralExp Build()
    {
        return new ast.Int8LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Int8LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Int16LiteralExpBuilder : IBuilder<ast.Int16LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Int16LiteralExp Build()
    {
        return new ast.Int16LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Int16LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Int32LiteralExpBuilder : IBuilder<ast.Int32LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Int32LiteralExp Build()
    {
        return new ast.Int32LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Int32LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Int64LiteralExpBuilder : IBuilder<ast.Int64LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Int64LiteralExp Build()
    {
        return new ast.Int64LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Int64LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UnsignedInt8LiteralExpBuilder : IBuilder<ast.UnsignedInt8LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.UnsignedInt8LiteralExp Build()
    {
        return new ast.UnsignedInt8LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public UnsignedInt8LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UnsignedInt16LiteralExpBuilder : IBuilder<ast.UnsignedInt16LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.UnsignedInt16LiteralExp Build()
    {
        return new ast.UnsignedInt16LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public UnsignedInt16LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UnsignedInt32LiteralExpBuilder : IBuilder<ast.UnsignedInt32LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.UnsignedInt32LiteralExp Build()
    {
        return new ast.UnsignedInt32LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public UnsignedInt32LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UnsignedInt64LiteralExpBuilder : IBuilder<ast.UnsignedInt64LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.UnsignedInt64LiteralExp Build()
    {
        return new ast.UnsignedInt64LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public UnsignedInt64LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Float4LiteralExpBuilder : IBuilder<ast.Float4LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Float4LiteralExp Build()
    {
        return new ast.Float4LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Float4LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Float8LiteralExpBuilder : IBuilder<ast.Float8LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Float8LiteralExp Build()
    {
        return new ast.Float8LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Float8LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class Float16LiteralExpBuilder : IBuilder<ast.Float16LiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.Float16LiteralExp Build()
    {
        return new ast.Float16LiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public Float16LiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class BooleanLiteralExpBuilder : IBuilder<ast.BooleanLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.BooleanLiteralExp Build()
    {
        return new ast.BooleanLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public BooleanLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class CharLiteralExpBuilder : IBuilder<ast.CharLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.CharLiteralExp Build()
    {
        return new ast.CharLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public CharLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class StringLiteralExpBuilder : IBuilder<ast.StringLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.StringLiteralExp Build()
    {
        return new ast.StringLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public StringLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class DateLiteralExpBuilder : IBuilder<ast.DateLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.DateLiteralExp Build()
    {
        return new ast.DateLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public DateLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class TimeLiteralExpBuilder : IBuilder<ast.TimeLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.TimeLiteralExp Build()
    {
        return new ast.TimeLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public TimeLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class DateTimeLiteralExpBuilder : IBuilder<ast.DateTimeLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.DateTimeLiteralExp Build()
    {
        return new ast.DateTimeLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public DateTimeLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class DurationLiteralExpBuilder : IBuilder<ast.DurationLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.DurationLiteralExp Build()
    {
        return new ast.DurationLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public DurationLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UriLiteralExpBuilder : IBuilder<ast.UriLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.UriLiteralExp Build()
    {
        return new ast.UriLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public UriLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AtomLiteralExpBuilder : IBuilder<ast.AtomLiteralExp>
{
    private Dictionary<string, object> _Annotations = new();

    public ast.AtomLiteralExp Build()
    {
        return new ast.AtomLiteralExp()
        {
            Annotations = this._Annotations
        };
    }
    public AtomLiteralExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class MemberAccessExpBuilder : IBuilder<ast.MemberAccessExp>
{
    private Expression _LHS;
    private Expression? _RHS;
    private Dictionary<string, object> _Annotations = new();

    public ast.MemberAccessExp Build()
    {
        return new ast.MemberAccessExp()
        {
            LHS = this._LHS,
            RHS = this._RHS,
            Annotations = this._Annotations
        };
    }
    public MemberAccessExpBuilder WithLHS(Expression value)
    {
        _LHS = value;
        return this;
    }

    public MemberAccessExpBuilder WithRHS(Expression? value)
    {
        _RHS = value;
        return this;
    }

    public MemberAccessExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ObjectInitializerExpBuilder : IBuilder<ast.ObjectInitializerExp>
{
    private FifthType _TypeToInitialize;
    private List<PropertyInitializerExp> _PropertyInitialisers;
    private Dictionary<string, object> _Annotations = new();

    public ast.ObjectInitializerExp Build()
    {
        return new ast.ObjectInitializerExp()
        {
            TypeToInitialize = this._TypeToInitialize,
            PropertyInitialisers = this._PropertyInitialisers,
            Annotations = this._Annotations
        };
    }
    public ObjectInitializerExpBuilder WithTypeToInitialize(FifthType value)
    {
        _TypeToInitialize = value;
        return this;
    }

    public ObjectInitializerExpBuilder WithPropertyInitialisers(List<PropertyInitializerExp> value)
    {
        _PropertyInitialisers = value;
        return this;
    }

    public ObjectInitializerExpBuilder AddingItemToPropertyInitialisers(PropertyInitializerExp value)
    {
        _PropertyInitialisers ??= new List<PropertyInitializerExp>();
        _PropertyInitialisers.Add(value);
        return this;
    }

    public ObjectInitializerExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class PropertyInitializerExpBuilder : IBuilder<ast.PropertyInitializerExp>
{
    private PropertyRef _PropertyToInitialize;
    private Expression _RHS;
    private Dictionary<string, object> _Annotations = new();

    public ast.PropertyInitializerExp Build()
    {
        return new ast.PropertyInitializerExp()
        {
            PropertyToInitialize = this._PropertyToInitialize,
            RHS = this._RHS,
            Annotations = this._Annotations
        };
    }
    public PropertyInitializerExpBuilder WithPropertyToInitialize(PropertyRef value)
    {
        _PropertyToInitialize = value;
        return this;
    }

    public PropertyInitializerExpBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

    public PropertyInitializerExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class UnaryExpBuilder : IBuilder<ast.UnaryExp>
{
    private Operator _Operator;
    private Expression _Operand;
    private Dictionary<string, object> _Annotations = new();

    public ast.UnaryExp Build()
    {
        return new ast.UnaryExp()
        {
            Operator = this._Operator,
            Operand = this._Operand,
            Annotations = this._Annotations
        };
    }
    public UnaryExpBuilder WithOperator(Operator value)
    {
        _Operator = value;
        return this;
    }

    public UnaryExpBuilder WithOperand(Expression value)
    {
        _Operand = value;
        return this;
    }

    public UnaryExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class VarRefExpBuilder : IBuilder<ast.VarRefExp>
{
    private string _VarName;
    private VariableDecl _VariableDecl;
    private Dictionary<string, object> _Annotations = new();

    public ast.VarRefExp Build()
    {
        return new ast.VarRefExp()
        {
            VarName = this._VarName,
            VariableDecl = this._VariableDecl,
            Annotations = this._Annotations
        };
    }
    public VarRefExpBuilder WithVarName(string value)
    {
        _VarName = value;
        return this;
    }

    public VarRefExpBuilder WithVariableDecl(VariableDecl value)
    {
        _VariableDecl = value;
        return this;
    }

    public VarRefExpBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ListLiteralBuilder : IBuilder<ast.ListLiteral>
{
    private List<Expression> _ElementExpressions;
    private Dictionary<string, object> _Annotations = new();

    public ast.ListLiteral Build()
    {
        return new ast.ListLiteral()
        {
            ElementExpressions = this._ElementExpressions,
            Annotations = this._Annotations
        };
    }
    public ListLiteralBuilder WithElementExpressions(List<Expression> value)
    {
        _ElementExpressions = value;
        return this;
    }

    public ListLiteralBuilder AddingItemToElementExpressions(Expression value)
    {
        _ElementExpressions ??= new List<Expression>();
        _ElementExpressions.Add(value);
        return this;
    }

    public ListLiteralBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class ListComprehensionBuilder : IBuilder<ast.ListComprehension>
{
    private string _VarName;
    private string _SourceName;
    private Expression? _MembershipConstraint;
    private Dictionary<string, object> _Annotations = new();

    public ast.ListComprehension Build()
    {
        return new ast.ListComprehension()
        {
            VarName = this._VarName,
            SourceName = this._SourceName,
            MembershipConstraint = this._MembershipConstraint,
            Annotations = this._Annotations
        };
    }
    public ListComprehensionBuilder WithVarName(string value)
    {
        _VarName = value;
        return this;
    }

    public ListComprehensionBuilder WithSourceName(string value)
    {
        _SourceName = value;
        return this;
    }

    public ListComprehensionBuilder WithMembershipConstraint(Expression? value)
    {
        _MembershipConstraint = value;
        return this;
    }

    public ListComprehensionBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class AtomBuilder : IBuilder<ast.Atom>
{
    private AtomLiteralExp _AtomExp;
    private Dictionary<string, object> _Annotations = new();

    public ast.Atom Build()
    {
        return new ast.Atom()
        {
            AtomExp = this._AtomExp,
            Annotations = this._Annotations
        };
    }
    public AtomBuilder WithAtomExp(AtomLiteralExp value)
    {
        _AtomExp = value;
        return this;
    }

    public AtomBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class TripleBuilder : IBuilder<ast.Triple>
{
    private UriLiteralExp _SubjectExp;
    private UriLiteralExp _PredicateExp;
    private Expression _ObjectExp;
    private Dictionary<string, object> _Annotations = new();

    public ast.Triple Build()
    {
        return new ast.Triple()
        {
            SubjectExp = this._SubjectExp,
            PredicateExp = this._PredicateExp,
            ObjectExp = this._ObjectExp,
            Annotations = this._Annotations
        };
    }
    public TripleBuilder WithSubjectExp(UriLiteralExp value)
    {
        _SubjectExp = value;
        return this;
    }

    public TripleBuilder WithPredicateExp(UriLiteralExp value)
    {
        _PredicateExp = value;
        return this;
    }

    public TripleBuilder WithObjectExp(Expression value)
    {
        _ObjectExp = value;
        return this;
    }

    public TripleBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

public class GraphBuilder : IBuilder<ast.Graph>
{
    private UriLiteralExp _GraphUri;
    private List<Triple> _Triples;
    private Dictionary<string, object> _Annotations = new();

    public ast.Graph Build()
    {
        return new ast.Graph()
        {
            GraphUri = this._GraphUri,
            Triples = this._Triples,
            Annotations = this._Annotations
        };
    }
    public GraphBuilder WithGraphUri(UriLiteralExp value)
    {
        _GraphUri = value;
        return this;
    }

    public GraphBuilder WithTriples(List<Triple> value)
    {
        _Triples = value;
        return this;
    }

    public GraphBuilder AddingItemToTriples(Triple value)
    {
        _Triples ??= new List<Triple>();
        _Triples.Add(value);
        return this;
    }

    public GraphBuilder WithAnnotations(Dictionary<string, object> value)
    {
        _Annotations = value;
        return this;
    }

}

#nullable restore
