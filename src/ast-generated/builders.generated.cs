

namespace ast_generated;
using ast_generated;
using ast;
using System.Collections.Generic;
#nullable disable

public class UserDefinedTypeBuilder : IBuilder<ast.UserDefinedType>
{

    private ast.ClassDef _ClassDef;
    private ast.TypeName _Name;
    private ast.NamespaceName _Namespace;
    private ast.TypeId _TypeId;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UserDefinedType Build()
    {
        return new ast.UserDefinedType(){
             ClassDef = this._ClassDef // from UserDefinedType
           , Name = this._Name // from UserDefinedType
           , Namespace = this._Namespace // from UserDefinedType
           , TypeId = this._TypeId // from UserDefinedType
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UserDefinedTypeBuilder WithClassDef(ast.ClassDef value){
        _ClassDef = value;
        return this;
    }

    public UserDefinedTypeBuilder WithName(ast.TypeName value){
        _Name = value;
        return this;
    }

    public UserDefinedTypeBuilder WithNamespace(ast.NamespaceName value){
        _Namespace = value;
        return this;
    }

    public UserDefinedTypeBuilder WithTypeId(ast.TypeId value){
        _TypeId = value;
        return this;
    }

    public UserDefinedTypeBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public UserDefinedTypeBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public UserDefinedTypeBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public UserDefinedTypeBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssemblyDefBuilder : IBuilder<ast.AssemblyDef>
{

    private ast.AssemblyName _Name;
    private System.String _PublicKeyToken;
    private System.String _Version;
    private List<ast.AssemblyRef> _AssemblyRefs;
    private List<ast.ClassDef> _ClassDefs;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssemblyDef Build()
    {
        return new ast.AssemblyDef(){
             Name = this._Name // from AssemblyDef
           , PublicKeyToken = this._PublicKeyToken // from AssemblyDef
           , Version = this._Version // from AssemblyDef
           , AssemblyRefs = this._AssemblyRefs // from AssemblyDef
           , ClassDefs = this._ClassDefs // from AssemblyDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssemblyDefBuilder WithName(ast.AssemblyName value){
        _Name = value;
        return this;
    }

    public AssemblyDefBuilder WithPublicKeyToken(System.String value){
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyDefBuilder WithVersion(System.String value){
        _Version = value;
        return this;
    }

    public AssemblyDefBuilder WithAssemblyRefs(List<ast.AssemblyRef> value){
        _AssemblyRefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToAssemblyRefs(ast.AssemblyRef value){
        _AssemblyRefs  ??= [];
        _AssemblyRefs.Add(value);
        return this;
    }
    public AssemblyDefBuilder WithClassDefs(List<ast.ClassDef> value){
        _ClassDefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToClassDefs(ast.ClassDef value){
        _ClassDefs  ??= [];
        _ClassDefs.Add(value);
        return this;
    }
    public AssemblyDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public AssemblyDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssemblyDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssemblyDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssemblyDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FunctionDefBuilder : IBuilder<ast.FunctionDef>
{

    private List<ast.ParamDef> _Params;
    private ast.BlockStatement _Body;
    private ast.MemberName _Name;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FunctionDef Build()
    {
        return new ast.FunctionDef(){
             Params = this._Params // from FunctionDef
           , Body = this._Body // from FunctionDef
           , Name = this._Name // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public FunctionDefBuilder WithParams(List<ast.ParamDef> value){
        _Params = value;
        return this;
    }

    public FunctionDefBuilder AddingItemToParams(ast.ParamDef value){
        _Params  ??= [];
        _Params.Add(value);
        return this;
    }
    public FunctionDefBuilder WithBody(ast.BlockStatement value){
        _Body = value;
        return this;
    }

    public FunctionDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public FunctionDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public FunctionDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public FunctionDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public FunctionDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public FunctionDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public FunctionDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FieldDefBuilder : IBuilder<ast.FieldDef>
{

    private ast.AccessConstraint[] _AccessConstraints;
    private ast.MemberName _Name;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FieldDef Build()
    {
        return new ast.FieldDef(){
             AccessConstraints = this._AccessConstraints // from FieldDef
           , Name = this._Name // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public FieldDefBuilder WithAccessConstraints(ast.AccessConstraint[] value){
        _AccessConstraints = value;
        return this;
    }

    public FieldDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public FieldDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public FieldDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public FieldDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public FieldDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public FieldDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public FieldDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class PropertyDefBuilder : IBuilder<ast.PropertyDef>
{

    private ast.AccessConstraint[] _AccessConstraints;
    private System.Boolean _IsWriteOnly;
    private ast.FieldDef _BackingField;
    private ast.MethodDef _Getter;
    private ast.MethodDef _Setter;
    private System.Boolean _CtorOnlySetter;
    private ast.MemberName _Name;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.PropertyDef Build()
    {
        return new ast.PropertyDef(){
             AccessConstraints = this._AccessConstraints // from PropertyDef
           , IsWriteOnly = this._IsWriteOnly // from PropertyDef
           , BackingField = this._BackingField // from PropertyDef
           , Getter = this._Getter // from PropertyDef
           , Setter = this._Setter // from PropertyDef
           , CtorOnlySetter = this._CtorOnlySetter // from PropertyDef
           , Name = this._Name // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public PropertyDefBuilder WithAccessConstraints(ast.AccessConstraint[] value){
        _AccessConstraints = value;
        return this;
    }

    public PropertyDefBuilder WithIsWriteOnly(System.Boolean value){
        _IsWriteOnly = value;
        return this;
    }

    public PropertyDefBuilder WithBackingField(ast.FieldDef value){
        _BackingField = value;
        return this;
    }

    public PropertyDefBuilder WithGetter(ast.MethodDef value){
        _Getter = value;
        return this;
    }

    public PropertyDefBuilder WithSetter(ast.MethodDef value){
        _Setter = value;
        return this;
    }

    public PropertyDefBuilder WithCtorOnlySetter(System.Boolean value){
        _CtorOnlySetter = value;
        return this;
    }

    public PropertyDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public PropertyDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public PropertyDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public PropertyDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public PropertyDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public PropertyDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public PropertyDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MethodDefBuilder : IBuilder<ast.MethodDef>
{

    private ast.MemberName _Name;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MethodDef Build()
    {
        return new ast.MethodDef(){
             Name = this._Name // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MethodDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public MethodDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public MethodDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public MethodDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public MethodDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MethodDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public MethodDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class InferenceRuleDefBuilder : IBuilder<ast.InferenceRuleDef>
{

    private ast.Expression _Antecedent;
    private ast.KnowledgeManagementBlock _Consequent;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.InferenceRuleDef Build()
    {
        return new ast.InferenceRuleDef(){
             Antecedent = this._Antecedent // from InferenceRuleDef
           , Consequent = this._Consequent // from InferenceRuleDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public InferenceRuleDefBuilder WithAntecedent(ast.Expression value){
        _Antecedent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithConsequent(ast.KnowledgeManagementBlock value){
        _Consequent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public InferenceRuleDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public InferenceRuleDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public InferenceRuleDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ParamDefBuilder : IBuilder<ast.ParamDef>
{

    private ast.Expression _ParameterConstraint;
    private ast.ParamDestructureDef _DestructureDef;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ParamDef Build()
    {
        return new ast.ParamDef(){
             ParameterConstraint = this._ParameterConstraint // from ParamDef
           , DestructureDef = this._DestructureDef // from ParamDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ParamDefBuilder WithParameterConstraint(ast.Expression value){
        _ParameterConstraint = value;
        return this;
    }

    public ParamDefBuilder WithDestructureDef(ast.ParamDestructureDef value){
        _DestructureDef = value;
        return this;
    }

    public ParamDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public ParamDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ParamDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ParamDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ParamDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ParamDestructureDefBuilder : IBuilder<ast.ParamDestructureDef>
{

    private List<ast.PropertyBindingDef> _Bindings;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ParamDestructureDef Build()
    {
        return new ast.ParamDestructureDef(){
             Bindings = this._Bindings // from ParamDestructureDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ParamDestructureDefBuilder WithBindings(List<ast.PropertyBindingDef> value){
        _Bindings = value;
        return this;
    }

    public ParamDestructureDefBuilder AddingItemToBindings(ast.PropertyBindingDef value){
        _Bindings  ??= [];
        _Bindings.Add(value);
        return this;
    }
    public ParamDestructureDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public ParamDestructureDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ParamDestructureDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ParamDestructureDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ParamDestructureDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class PropertyBindingDefBuilder : IBuilder<ast.PropertyBindingDef>
{

    private ast.VariableDecl _IntroducedVariable;
    private ast.PropertyDef _ReferencedProperty;
    private ast.ParamDestructureDef _DestructureDef;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.PropertyBindingDef Build()
    {
        return new ast.PropertyBindingDef(){
             IntroducedVariable = this._IntroducedVariable // from PropertyBindingDef
           , ReferencedProperty = this._ReferencedProperty // from PropertyBindingDef
           , DestructureDef = this._DestructureDef // from PropertyBindingDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public PropertyBindingDefBuilder WithIntroducedVariable(ast.VariableDecl value){
        _IntroducedVariable = value;
        return this;
    }

    public PropertyBindingDefBuilder WithReferencedProperty(ast.PropertyDef value){
        _ReferencedProperty = value;
        return this;
    }

    public PropertyBindingDefBuilder WithDestructureDef(ast.ParamDestructureDef value){
        _DestructureDef = value;
        return this;
    }

    public PropertyBindingDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public PropertyBindingDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public PropertyBindingDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public PropertyBindingDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public PropertyBindingDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TypeDefBuilder : IBuilder<ast.TypeDef>
{

    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.TypeDef Build()
    {
        return new ast.TypeDef(){
             Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TypeDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public TypeDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public TypeDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TypeDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public TypeDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ClassDefBuilder : IBuilder<ast.ClassDef>
{

    private System.String _Namespace;
    private List<ast.MemberDef> _MemberDefs;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ClassDef Build()
    {
        return new ast.ClassDef(){
             Namespace = this._Namespace // from ClassDef
           , MemberDefs = this._MemberDefs // from ClassDef
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ClassDefBuilder WithNamespace(System.String value){
        _Namespace = value;
        return this;
    }

    public ClassDefBuilder WithMemberDefs(List<ast.MemberDef> value){
        _MemberDefs = value;
        return this;
    }

    public ClassDefBuilder AddingItemToMemberDefs(ast.MemberDef value){
        _MemberDefs  ??= [];
        _MemberDefs.Add(value);
        return this;
    }
    public ClassDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public ClassDefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ClassDefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ClassDefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ClassDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VariableDeclBuilder : IBuilder<ast.VariableDecl>
{

    private ast.Expression _InitialValue;
    private ast.Visibility _Visibility;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VariableDecl Build()
    {
        return new ast.VariableDecl(){
             InitialValue = this._InitialValue // from VariableDecl
           , Visibility = this._Visibility // from Definition
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VariableDeclBuilder WithInitialValue(ast.Expression value){
        _InitialValue = value;
        return this;
    }

    public VariableDeclBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public VariableDeclBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public VariableDeclBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VariableDeclBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public VariableDeclBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssemblyRefBuilder : IBuilder<ast.AssemblyRef>
{

    private System.String _PublicKeyToken;
    private System.String _Version;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssemblyRef Build()
    {
        return new ast.AssemblyRef(){
             PublicKeyToken = this._PublicKeyToken // from AssemblyRef
           , Version = this._Version // from AssemblyRef
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssemblyRefBuilder WithPublicKeyToken(System.String value){
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyRefBuilder WithVersion(System.String value){
        _Version = value;
        return this;
    }

    public AssemblyRefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssemblyRefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssemblyRefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssemblyRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MemberRefBuilder : IBuilder<ast.MemberRef>
{

    private ast.MemberDef _MemberDef;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MemberRef Build()
    {
        return new ast.MemberRef(){
             MemberDef = this._MemberDef // from MemberRef
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MemberRefBuilder WithMemberDef(ast.MemberDef value){
        _MemberDef = value;
        return this;
    }

    public MemberRefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public MemberRefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MemberRefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public MemberRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TypeRefBuilder : IBuilder<ast.TypeRef>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.TypeRef Build()
    {
        return new ast.TypeRef(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TypeRefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public TypeRefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TypeRefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public TypeRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VarRefBuilder : IBuilder<ast.VarRef>
{

    private ast.VarDeclStatement _VarDecl;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarRef Build()
    {
        return new ast.VarRef(){
             VarDecl = this._VarDecl // from VarRef
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarRefBuilder WithVarDecl(ast.VarDeclStatement value){
        _VarDecl = value;
        return this;
    }

    public VarRefBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public VarRefBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarRefBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public VarRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class GraphNamespaceAliasBuilder : IBuilder<ast.GraphNamespaceAlias>
{

    private System.Uri _Uri;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.GraphNamespaceAlias Build()
    {
        return new ast.GraphNamespaceAlias(){
             Uri = this._Uri // from GraphNamespaceAlias
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GraphNamespaceAliasBuilder WithUri(System.Uri value){
        _Uri = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssignmentStatementBuilder : IBuilder<ast.AssignmentStatement>
{

    private ast.Expression _RHS;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssignmentStatement Build()
    {
        return new ast.AssignmentStatement(){
             RHS = this._RHS // from AssignmentStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssignmentStatementBuilder WithRHS(ast.Expression value){
        _RHS = value;
        return this;
    }

    public AssignmentStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssignmentStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssignmentStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssignmentStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class BlockStatementBuilder : IBuilder<ast.BlockStatement>
{

    private List<ast.Statement> _Statements;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.BlockStatement Build()
    {
        return new ast.BlockStatement(){
             Statements = this._Statements // from BlockStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public BlockStatementBuilder WithStatements(List<ast.Statement> value){
        _Statements = value;
        return this;
    }

    public BlockStatementBuilder AddingItemToStatements(ast.Statement value){
        _Statements  ??= [];
        _Statements.Add(value);
        return this;
    }
    public BlockStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public BlockStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public BlockStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public BlockStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class KnowledgeManagementBlockBuilder : IBuilder<ast.KnowledgeManagementBlock>
{

    private List<ast.KnowledgeManagementStatement> _Statements;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.KnowledgeManagementBlock Build()
    {
        return new ast.KnowledgeManagementBlock(){
             Statements = this._Statements // from KnowledgeManagementBlock
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public KnowledgeManagementBlockBuilder WithStatements(List<ast.KnowledgeManagementStatement> value){
        _Statements = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder AddingItemToStatements(ast.KnowledgeManagementStatement value){
        _Statements  ??= [];
        _Statements.Add(value);
        return this;
    }
    public KnowledgeManagementBlockBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ExpStatementBuilder : IBuilder<ast.ExpStatement>
{

    private ast.Expression _RHS;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ExpStatement Build()
    {
        return new ast.ExpStatement(){
             RHS = this._RHS // from ExpStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ExpStatementBuilder WithRHS(ast.Expression value){
        _RHS = value;
        return this;
    }

    public ExpStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ExpStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ExpStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ExpStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ForStatementBuilder : IBuilder<ast.ForStatement>
{

    private ast.Expression _InitialValue;
    private ast.Expression _Constraint;
    private ast.Expression _IncrementExpression;
    private ast.VariableDecl _LoopVariable;
    private ast.BlockStatement _Body;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ForStatement Build()
    {
        return new ast.ForStatement(){
             InitialValue = this._InitialValue // from ForStatement
           , Constraint = this._Constraint // from ForStatement
           , IncrementExpression = this._IncrementExpression // from ForStatement
           , LoopVariable = this._LoopVariable // from ForStatement
           , Body = this._Body // from ForStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ForStatementBuilder WithInitialValue(ast.Expression value){
        _InitialValue = value;
        return this;
    }

    public ForStatementBuilder WithConstraint(ast.Expression value){
        _Constraint = value;
        return this;
    }

    public ForStatementBuilder WithIncrementExpression(ast.Expression value){
        _IncrementExpression = value;
        return this;
    }

    public ForStatementBuilder WithLoopVariable(ast.VariableDecl value){
        _LoopVariable = value;
        return this;
    }

    public ForStatementBuilder WithBody(ast.BlockStatement value){
        _Body = value;
        return this;
    }

    public ForStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ForStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ForStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ForStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ForeachStatementBuilder : IBuilder<ast.ForeachStatement>
{

    private ast.Expression _Collection;
    private ast.VariableDecl _LoopVariable;
    private ast.BlockStatement _Body;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ForeachStatement Build()
    {
        return new ast.ForeachStatement(){
             Collection = this._Collection // from ForeachStatement
           , LoopVariable = this._LoopVariable // from ForeachStatement
           , Body = this._Body // from ForeachStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ForeachStatementBuilder WithCollection(ast.Expression value){
        _Collection = value;
        return this;
    }

    public ForeachStatementBuilder WithLoopVariable(ast.VariableDecl value){
        _LoopVariable = value;
        return this;
    }

    public ForeachStatementBuilder WithBody(ast.BlockStatement value){
        _Body = value;
        return this;
    }

    public ForeachStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ForeachStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ForeachStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ForeachStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class GuardStatementBuilder : IBuilder<ast.GuardStatement>
{

    private ast.Expression _Condition;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.GuardStatement Build()
    {
        return new ast.GuardStatement(){
             Condition = this._Condition // from GuardStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GuardStatementBuilder WithCondition(ast.Expression value){
        _Condition = value;
        return this;
    }

    public GuardStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public GuardStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public GuardStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public GuardStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class IfElseStatementBuilder : IBuilder<ast.IfElseStatement>
{

    private ast.Expression _Condition;
    private ast.BlockStatement _ThenBlock;
    private ast.BlockStatement _ElseBlock;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.IfElseStatement Build()
    {
        return new ast.IfElseStatement(){
             Condition = this._Condition // from IfElseStatement
           , ThenBlock = this._ThenBlock // from IfElseStatement
           , ElseBlock = this._ElseBlock // from IfElseStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public IfElseStatementBuilder WithCondition(ast.Expression value){
        _Condition = value;
        return this;
    }

    public IfElseStatementBuilder WithThenBlock(ast.BlockStatement value){
        _ThenBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithElseBlock(ast.BlockStatement value){
        _ElseBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public IfElseStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public IfElseStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public IfElseStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ReturnStatementBuilder : IBuilder<ast.ReturnStatement>
{

    private ast.Expression _ReturnValue;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ReturnStatement Build()
    {
        return new ast.ReturnStatement(){
             ReturnValue = this._ReturnValue // from ReturnStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ReturnStatementBuilder WithReturnValue(ast.Expression value){
        _ReturnValue = value;
        return this;
    }

    public ReturnStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ReturnStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ReturnStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ReturnStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VarDeclStatementBuilder : IBuilder<ast.VarDeclStatement>
{

    private ast.VariableDecl _VariableDecl;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarDeclStatement Build()
    {
        return new ast.VarDeclStatement(){
             VariableDecl = this._VariableDecl // from VarDeclStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarDeclStatementBuilder WithVariableDecl(ast.VariableDecl value){
        _VariableDecl = value;
        return this;
    }

    public VarDeclStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public VarDeclStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarDeclStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public VarDeclStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class WhileStatementBuilder : IBuilder<ast.WhileStatement>
{

    private ast.Expression _Condition;
    private ast.BlockStatement _Body;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.WhileStatement Build()
    {
        return new ast.WhileStatement(){
             Condition = this._Condition // from WhileStatement
           , Body = this._Body // from WhileStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public WhileStatementBuilder WithCondition(ast.Expression value){
        _Condition = value;
        return this;
    }

    public WhileStatementBuilder WithBody(ast.BlockStatement value){
        _Body = value;
        return this;
    }

    public WhileStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public WhileStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public WhileStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public WhileStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionStatementBuilder : IBuilder<ast.AssertionStatement>
{

    private ast.AssertionSubject _AssertionSubject;
    private ast.AssertionPredicate _AssertionPredicate;
    private ast.AssertionObject _AssertionObject;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionStatement Build()
    {
        return new ast.AssertionStatement(){
             AssertionSubject = this._AssertionSubject // from AssertionStatement
           , AssertionPredicate = this._AssertionPredicate // from AssertionStatement
           , AssertionObject = this._AssertionObject // from AssertionStatement
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionStatementBuilder WithAssertionSubject(ast.AssertionSubject value){
        _AssertionSubject = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionPredicate(ast.AssertionPredicate value){
        _AssertionPredicate = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionObject(ast.AssertionObject value){
        _AssertionObject = value;
        return this;
    }

    public AssertionStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssertionStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssertionStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssertionStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionObjectBuilder : IBuilder<ast.AssertionObject>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionObject Build()
    {
        return new ast.AssertionObject(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionObjectBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssertionObjectBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssertionObjectBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssertionObjectBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionPredicateBuilder : IBuilder<ast.AssertionPredicate>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionPredicate Build()
    {
        return new ast.AssertionPredicate(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionPredicateBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssertionPredicateBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssertionPredicateBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssertionPredicateBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionSubjectBuilder : IBuilder<ast.AssertionSubject>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionSubject Build()
    {
        return new ast.AssertionSubject(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionSubjectBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AssertionSubjectBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssertionSubjectBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AssertionSubjectBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class RetractionStatementBuilder : IBuilder<ast.RetractionStatement>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.RetractionStatement Build()
    {
        return new ast.RetractionStatement(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public RetractionStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public RetractionStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public RetractionStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public RetractionStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class WithScopeStatementBuilder : IBuilder<ast.WithScopeStatement>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.WithScopeStatement Build()
    {
        return new ast.WithScopeStatement(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public WithScopeStatementBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public WithScopeStatementBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public WithScopeStatementBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public WithScopeStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class BinaryExpBuilder : IBuilder<ast.BinaryExp>
{

    private ast.Expression _Left;
    private ast.Operator _Op;
    private ast.Expression _Right;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.BinaryExp Build()
    {
        return new ast.BinaryExp(){
             Left = this._Left // from BinaryExp
           , Op = this._Op // from BinaryExp
           , Right = this._Right // from BinaryExp
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public BinaryExpBuilder WithLeft(ast.Expression value){
        _Left = value;
        return this;
    }

    public BinaryExpBuilder WithOp(ast.Operator value){
        _Op = value;
        return this;
    }

    public BinaryExpBuilder WithRight(ast.Expression value){
        _Right = value;
        return this;
    }

    public BinaryExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public BinaryExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public BinaryExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public BinaryExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class CastExpBuilder : IBuilder<ast.CastExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.CastExp Build()
    {
        return new ast.CastExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public CastExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public CastExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public CastExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public CastExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class LambdaExpBuilder : IBuilder<ast.LambdaExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.LambdaExp Build()
    {
        return new ast.LambdaExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public LambdaExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public LambdaExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public LambdaExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public LambdaExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FuncCallExpBuilder : IBuilder<ast.FuncCallExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FuncCallExp Build()
    {
        return new ast.FuncCallExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public FuncCallExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public FuncCallExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public FuncCallExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public FuncCallExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class LiteralExpBuilder : IBuilder<ast.LiteralExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.LiteralExp Build()
    {
        return new ast.LiteralExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public LiteralExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public LiteralExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public LiteralExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MemberAccessExpBuilder : IBuilder<ast.MemberAccessExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MemberAccessExp Build()
    {
        return new ast.MemberAccessExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MemberAccessExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public MemberAccessExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MemberAccessExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public MemberAccessExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ObjectInstantiationExpBuilder : IBuilder<ast.ObjectInstantiationExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ObjectInstantiationExp Build()
    {
        return new ast.ObjectInstantiationExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ObjectInstantiationExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnaryExpBuilder : IBuilder<ast.UnaryExp>
{

    private ast.Operator _Op;
    private ast.Expression _Operand;
    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnaryExp Build()
    {
        return new ast.UnaryExp(){
             Op = this._Op // from UnaryExp
           , Operand = this._Operand // from UnaryExp
           , SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnaryExpBuilder WithOp(ast.Operator value){
        _Op = value;
        return this;
    }

    public UnaryExpBuilder WithOperand(ast.Expression value){
        _Operand = value;
        return this;
    }

    public UnaryExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public UnaryExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public UnaryExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public UnaryExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VarRefExpBuilder : IBuilder<ast.VarRefExp>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarRefExp Build()
    {
        return new ast.VarRefExp(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarRefExpBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public VarRefExpBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarRefExpBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public VarRefExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ListBuilder : IBuilder<ast.List>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.List Build()
    {
        return new ast.List(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ListBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public ListBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ListBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public ListBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AtomBuilder : IBuilder<ast.Atom>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Atom Build()
    {
        return new ast.Atom(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AtomBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public AtomBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AtomBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public AtomBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TripleBuilder : IBuilder<ast.Triple>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Triple Build()
    {
        return new ast.Triple(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TripleBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public TripleBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TripleBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public TripleBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class GraphBuilder : IBuilder<ast.Graph>
{

    private ast_model.SourceContext _SourceContext;
    private ast.TypeMetadata _Type;
    private ast.IAstThing _Parent;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Graph Build()
    {
        return new ast.Graph(){
             SourceContext = this._SourceContext // from AstThing
           , Type = this._Type // from AstThing
           , Parent = this._Parent // from AstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GraphBuilder WithSourceContext(ast_model.SourceContext value){
        _SourceContext = value;
        return this;
    }

    public GraphBuilder WithType(ast.TypeMetadata value){
        _Type = value;
        return this;
    }

    public GraphBuilder WithParent(ast.IAstThing value){
        _Parent = value;
        return this;
    }

    public GraphBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}

#nullable restore

