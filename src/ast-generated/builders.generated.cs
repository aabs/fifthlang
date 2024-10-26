

namespace ast_generated;
using ast_generated;
using ast;
using System.Collections.Generic;
#nullable disable

public class AssemblyDefBuilder : IBuilder<ast.AssemblyDef>
{

    private ast.AssemblyName _Name;
    private System.String _PublicKeyToken;
    private System.String _Version;
    private List<ast.AssemblyRef> _AssemblyRefs;
    private List<ast.ClassDef> _ClassDefs;
    private ast.Visibility _Visibility;
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

    public AssemblyDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FunctionDefBuilder : IBuilder<ast.FunctionDef>
{

    private List<ast.ParamDef> _Params;
    private ast.BlockStatement _Body;
    private ast_model.TypeSystem.TypeName? _ReturnType;
    private ast.MemberName _Name;
    private ast_model.TypeSystem.TypeName _TypeName;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FunctionDef Build()
    {
        return new ast.FunctionDef(){
             Params = this._Params // from FunctionDef
           , Body = this._Body // from FunctionDef
           , ReturnType = this._ReturnType // from FunctionDef
           , Name = this._Name // from MemberDef
           , TypeName = this._TypeName // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
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

    public FunctionDefBuilder WithReturnType(ast_model.TypeSystem.TypeName? value){
        _ReturnType = value;
        return this;
    }

    public FunctionDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public FunctionDefBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
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

    public FunctionDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FunctorDefBuilder : IBuilder<ast.FunctorDef>
{

    private ast.FunctionDef _InvocationFuncDev;
    private ast_model.Symbols.IScope _EnclosingScope;
    private ast_model.Symbols.ISymbolTable _SymbolTable;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FunctorDef Build()
    {
        return new ast.FunctorDef(){
             InvocationFuncDev = this._InvocationFuncDev // from FunctorDef
           , EnclosingScope = this._EnclosingScope // from ScopeAstThing
           , SymbolTable = this._SymbolTable // from ScopeAstThing
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public FunctorDefBuilder WithInvocationFuncDev(ast.FunctionDef value){
        _InvocationFuncDev = value;
        return this;
    }

    public FunctorDefBuilder WithEnclosingScope(ast_model.Symbols.IScope value){
        _EnclosingScope = value;
        return this;
    }

    public FunctorDefBuilder WithSymbolTable(ast_model.Symbols.ISymbolTable value){
        _SymbolTable = value;
        return this;
    }

    public FunctorDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FieldDefBuilder : IBuilder<ast.FieldDef>
{

    private ast.AccessConstraint[] _AccessConstraints;
    private ast.MemberName _Name;
    private ast_model.TypeSystem.TypeName _TypeName;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FieldDef Build()
    {
        return new ast.FieldDef(){
             AccessConstraints = this._AccessConstraints // from FieldDef
           , Name = this._Name // from MemberDef
           , TypeName = this._TypeName // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
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

    public FieldDefBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
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
    private ast_model.TypeSystem.TypeName _TypeName;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
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
           , TypeName = this._TypeName // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
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

    public PropertyDefBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
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

    public PropertyDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MethodDefBuilder : IBuilder<ast.MethodDef>
{

    private ast.MemberName _Name;
    private ast_model.TypeSystem.TypeName _TypeName;
    private System.Boolean _IsReadOnly;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MethodDef Build()
    {
        return new ast.MethodDef(){
             Name = this._Name // from MemberDef
           , TypeName = this._TypeName // from MemberDef
           , IsReadOnly = this._IsReadOnly // from MemberDef
           , Visibility = this._Visibility // from Definition
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MethodDefBuilder WithName(ast.MemberName value){
        _Name = value;
        return this;
    }

    public MethodDefBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.InferenceRuleDef Build()
    {
        return new ast.InferenceRuleDef(){
             Antecedent = this._Antecedent // from InferenceRuleDef
           , Consequent = this._Consequent // from InferenceRuleDef
           , Visibility = this._Visibility // from Definition
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

    public InferenceRuleDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ParamDefBuilder : IBuilder<ast.ParamDef>
{

    private ast_model.TypeSystem.TypeName _TypeName;
    private System.String _Name;
    private ast.Expression _ParameterConstraint;
    private ast.ParamDestructureDef _DestructureDef;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ParamDef Build()
    {
        return new ast.ParamDef(){
             TypeName = this._TypeName // from ParamDef
           , Name = this._Name // from ParamDef
           , ParameterConstraint = this._ParameterConstraint // from ParamDef
           , DestructureDef = this._DestructureDef // from ParamDef
           , Visibility = this._Visibility // from Definition
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ParamDefBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
        return this;
    }

    public ParamDefBuilder WithName(System.String value){
        _Name = value;
        return this;
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

    public ParamDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ParamDestructureDefBuilder : IBuilder<ast.ParamDestructureDef>
{

    private List<ast.PropertyBindingDef> _Bindings;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ParamDestructureDef Build()
    {
        return new ast.ParamDestructureDef(){
             Bindings = this._Bindings // from ParamDestructureDef
           , Visibility = this._Visibility // from Definition
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.PropertyBindingDef Build()
    {
        return new ast.PropertyBindingDef(){
             IntroducedVariable = this._IntroducedVariable // from PropertyBindingDef
           , ReferencedProperty = this._ReferencedProperty // from PropertyBindingDef
           , DestructureDef = this._DestructureDef // from PropertyBindingDef
           , Visibility = this._Visibility // from Definition
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

    public PropertyBindingDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TypeDefBuilder : IBuilder<ast.TypeDef>
{

    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.TypeDef Build()
    {
        return new ast.TypeDef(){
             Visibility = this._Visibility // from Definition
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TypeDefBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
        return this;
    }

    public TypeDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ClassDefBuilder : IBuilder<ast.ClassDef>
{

    private ast_model.TypeSystem.TypeName _Name;
    private List<ast.MemberDef> _MemberDefs;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ClassDef Build()
    {
        return new ast.ClassDef(){
             Name = this._Name // from ClassDef
           , MemberDefs = this._MemberDefs // from ClassDef
           , Visibility = this._Visibility // from Definition
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ClassDefBuilder WithName(ast_model.TypeSystem.TypeName value){
        _Name = value;
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

    public ClassDefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VariableDeclBuilder : IBuilder<ast.VariableDecl>
{

    private ast_model.TypeSystem.TypeName _TypeName;
    private System.String _Name;
    private ast.Visibility _Visibility;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VariableDecl Build()
    {
        return new ast.VariableDecl(){
             TypeName = this._TypeName // from VariableDecl
           , Name = this._Name // from VariableDecl
           , Visibility = this._Visibility // from Definition
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VariableDeclBuilder WithTypeName(ast_model.TypeSystem.TypeName value){
        _TypeName = value;
        return this;
    }

    public VariableDeclBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public VariableDeclBuilder WithVisibility(ast.Visibility value){
        _Visibility = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssemblyRef Build()
    {
        return new ast.AssemblyRef(){
             PublicKeyToken = this._PublicKeyToken // from AssemblyRef
           , Version = this._Version // from AssemblyRef
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

    public AssemblyRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MemberRefBuilder : IBuilder<ast.MemberRef>
{

    private ast.MemberDef _Member;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MemberRef Build()
    {
        return new ast.MemberRef(){
             Member = this._Member // from MemberRef
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MemberRefBuilder WithMember(ast.MemberDef value){
        _Member = value;
        return this;
    }

    public MemberRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class PropertyRefBuilder : IBuilder<ast.PropertyRef>
{

    private ast.PropertyDef _Property;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.PropertyRef Build()
    {
        return new ast.PropertyRef(){
             Property = this._Property // from PropertyRef
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public PropertyRefBuilder WithProperty(ast.PropertyDef value){
        _Property = value;
        return this;
    }

    public PropertyRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TypeRefBuilder : IBuilder<ast.TypeRef>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.TypeRef Build()
    {
        return new ast.TypeRef(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TypeRefBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VarRefBuilder : IBuilder<ast.VarRef>
{

    private ast.VarDeclStatement _VarDecl;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarRef Build()
    {
        return new ast.VarRef(){
             VarDecl = this._VarDecl // from VarRef
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarRefBuilder WithVarDecl(ast.VarDeclStatement value){
        _VarDecl = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.GraphNamespaceAlias Build()
    {
        return new ast.GraphNamespaceAlias(){
             Uri = this._Uri // from GraphNamespaceAlias
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GraphNamespaceAliasBuilder WithUri(System.Uri value){
        _Uri = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssignmentStatementBuilder : IBuilder<ast.AssignmentStatement>
{

    private System.String _VariableName;
    private ast.Expression _RHS;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssignmentStatement Build()
    {
        return new ast.AssignmentStatement(){
             VariableName = this._VariableName // from AssignmentStatement
           , RHS = this._RHS // from AssignmentStatement
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssignmentStatementBuilder WithVariableName(System.String value){
        _VariableName = value;
        return this;
    }

    public AssignmentStatementBuilder WithRHS(ast.Expression value){
        _RHS = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.BlockStatement Build()
    {
        return new ast.BlockStatement(){
             Statements = this._Statements // from BlockStatement
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
    public BlockStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class KnowledgeManagementBlockBuilder : IBuilder<ast.KnowledgeManagementBlock>
{

    private List<ast.KnowledgeManagementStatement> _Statements;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.KnowledgeManagementBlock Build()
    {
        return new ast.KnowledgeManagementBlock(){
             Statements = this._Statements // from KnowledgeManagementBlock
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
    public KnowledgeManagementBlockBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ExpStatementBuilder : IBuilder<ast.ExpStatement>
{

    private ast.Expression _RHS;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ExpStatement Build()
    {
        return new ast.ExpStatement(){
             RHS = this._RHS // from ExpStatement
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ExpStatementBuilder WithRHS(ast.Expression value){
        _RHS = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ForStatement Build()
    {
        return new ast.ForStatement(){
             InitialValue = this._InitialValue // from ForStatement
           , Constraint = this._Constraint // from ForStatement
           , IncrementExpression = this._IncrementExpression // from ForStatement
           , LoopVariable = this._LoopVariable // from ForStatement
           , Body = this._Body // from ForStatement
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ForeachStatement Build()
    {
        return new ast.ForeachStatement(){
             Collection = this._Collection // from ForeachStatement
           , LoopVariable = this._LoopVariable // from ForeachStatement
           , Body = this._Body // from ForeachStatement
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

    public ForeachStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class GuardStatementBuilder : IBuilder<ast.GuardStatement>
{

    private ast.Expression _Condition;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.GuardStatement Build()
    {
        return new ast.GuardStatement(){
             Condition = this._Condition // from GuardStatement
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GuardStatementBuilder WithCondition(ast.Expression value){
        _Condition = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.IfElseStatement Build()
    {
        return new ast.IfElseStatement(){
             Condition = this._Condition // from IfElseStatement
           , ThenBlock = this._ThenBlock // from IfElseStatement
           , ElseBlock = this._ElseBlock // from IfElseStatement
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

    public IfElseStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ReturnStatementBuilder : IBuilder<ast.ReturnStatement>
{

    private ast.Expression _ReturnValue;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ReturnStatement Build()
    {
        return new ast.ReturnStatement(){
             ReturnValue = this._ReturnValue // from ReturnStatement
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ReturnStatementBuilder WithReturnValue(ast.Expression value){
        _ReturnValue = value;
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
    private ast.Expression _InitialValue;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarDeclStatement Build()
    {
        return new ast.VarDeclStatement(){
             VariableDecl = this._VariableDecl // from VarDeclStatement
           , InitialValue = this._InitialValue // from VarDeclStatement
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarDeclStatementBuilder WithVariableDecl(ast.VariableDecl value){
        _VariableDecl = value;
        return this;
    }

    public VarDeclStatementBuilder WithInitialValue(ast.Expression value){
        _InitialValue = value;
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.WhileStatement Build()
    {
        return new ast.WhileStatement(){
             Condition = this._Condition // from WhileStatement
           , Body = this._Body // from WhileStatement
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
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionStatement Build()
    {
        return new ast.AssertionStatement(){
             AssertionSubject = this._AssertionSubject // from AssertionStatement
           , AssertionPredicate = this._AssertionPredicate // from AssertionStatement
           , AssertionObject = this._AssertionObject // from AssertionStatement
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

    public AssertionStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionObjectBuilder : IBuilder<ast.AssertionObject>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionObject Build()
    {
        return new ast.AssertionObject(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionObjectBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionPredicateBuilder : IBuilder<ast.AssertionPredicate>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionPredicate Build()
    {
        return new ast.AssertionPredicate(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionPredicateBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AssertionSubjectBuilder : IBuilder<ast.AssertionSubject>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AssertionSubject Build()
    {
        return new ast.AssertionSubject(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AssertionSubjectBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class RetractionStatementBuilder : IBuilder<ast.RetractionStatement>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.RetractionStatement Build()
    {
        return new ast.RetractionStatement(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public RetractionStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class WithScopeStatementBuilder : IBuilder<ast.WithScopeStatement>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.WithScopeStatement Build()
    {
        return new ast.WithScopeStatement(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public WithScopeStatementBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class BinaryExpBuilder : IBuilder<ast.BinaryExp>
{

    private ast.Expression _LHS;
    private ast.Operator _Operator;
    private ast.Expression _RHS;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.BinaryExp Build()
    {
        return new ast.BinaryExp(){
             LHS = this._LHS // from BinaryExp
           , Operator = this._Operator // from BinaryExp
           , RHS = this._RHS // from BinaryExp
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public BinaryExpBuilder WithLHS(ast.Expression value){
        _LHS = value;
        return this;
    }

    public BinaryExpBuilder WithOperator(ast.Operator value){
        _Operator = value;
        return this;
    }

    public BinaryExpBuilder WithRHS(ast.Expression value){
        _RHS = value;
        return this;
    }

    public BinaryExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class CastExpBuilder : IBuilder<ast.CastExp>
{

    private ast_model.TypeSystem.FifthType _TargetType;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.CastExp Build()
    {
        return new ast.CastExp(){
             TargetType = this._TargetType // from CastExp
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public CastExpBuilder WithTargetType(ast_model.TypeSystem.FifthType value){
        _TargetType = value;
        return this;
    }

    public CastExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class LambdaExpBuilder : IBuilder<ast.LambdaExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.LambdaExp Build()
    {
        return new ast.LambdaExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public LambdaExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class FuncCallExpBuilder : IBuilder<ast.FuncCallExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.FuncCallExp Build()
    {
        return new ast.FuncCallExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public FuncCallExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Int8LiteralExpBuilder : IBuilder<ast.Int8LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Int8LiteralExp Build()
    {
        return new ast.Int8LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Int8LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Int16LiteralExpBuilder : IBuilder<ast.Int16LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Int16LiteralExp Build()
    {
        return new ast.Int16LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Int16LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Int32LiteralExpBuilder : IBuilder<ast.Int32LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Int32LiteralExp Build()
    {
        return new ast.Int32LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Int32LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Int64LiteralExpBuilder : IBuilder<ast.Int64LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Int64LiteralExp Build()
    {
        return new ast.Int64LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Int64LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnsignedInt8LiteralExpBuilder : IBuilder<ast.UnsignedInt8LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnsignedInt8LiteralExp Build()
    {
        return new ast.UnsignedInt8LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnsignedInt8LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnsignedInt16LiteralExpBuilder : IBuilder<ast.UnsignedInt16LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnsignedInt16LiteralExp Build()
    {
        return new ast.UnsignedInt16LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnsignedInt16LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnsignedInt32LiteralExpBuilder : IBuilder<ast.UnsignedInt32LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnsignedInt32LiteralExp Build()
    {
        return new ast.UnsignedInt32LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnsignedInt32LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnsignedInt64LiteralExpBuilder : IBuilder<ast.UnsignedInt64LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnsignedInt64LiteralExp Build()
    {
        return new ast.UnsignedInt64LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnsignedInt64LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Float4LiteralExpBuilder : IBuilder<ast.Float4LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Float4LiteralExp Build()
    {
        return new ast.Float4LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Float4LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Float8LiteralExpBuilder : IBuilder<ast.Float8LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Float8LiteralExp Build()
    {
        return new ast.Float8LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Float8LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class Float16LiteralExpBuilder : IBuilder<ast.Float16LiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Float16LiteralExp Build()
    {
        return new ast.Float16LiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public Float16LiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class BooleanLiteralExpBuilder : IBuilder<ast.BooleanLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.BooleanLiteralExp Build()
    {
        return new ast.BooleanLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public BooleanLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class CharLiteralExpBuilder : IBuilder<ast.CharLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.CharLiteralExp Build()
    {
        return new ast.CharLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public CharLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class StringLiteralExpBuilder : IBuilder<ast.StringLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.StringLiteralExp Build()
    {
        return new ast.StringLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public StringLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class DateLiteralExpBuilder : IBuilder<ast.DateLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.DateLiteralExp Build()
    {
        return new ast.DateLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public DateLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TimeLiteralExpBuilder : IBuilder<ast.TimeLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.TimeLiteralExp Build()
    {
        return new ast.TimeLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TimeLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class DateTimeLiteralExpBuilder : IBuilder<ast.DateTimeLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.DateTimeLiteralExp Build()
    {
        return new ast.DateTimeLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public DateTimeLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class DurationLiteralExpBuilder : IBuilder<ast.DurationLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.DurationLiteralExp Build()
    {
        return new ast.DurationLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public DurationLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UriLiteralExpBuilder : IBuilder<ast.UriLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UriLiteralExp Build()
    {
        return new ast.UriLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UriLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AtomLiteralExpBuilder : IBuilder<ast.AtomLiteralExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.AtomLiteralExp Build()
    {
        return new ast.AtomLiteralExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AtomLiteralExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class MemberAccessExpBuilder : IBuilder<ast.MemberAccessExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.MemberAccessExp Build()
    {
        return new ast.MemberAccessExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public MemberAccessExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ObjectInitializerExpBuilder : IBuilder<ast.ObjectInitializerExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.ObjectInitializerExp Build()
    {
        return new ast.ObjectInitializerExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ObjectInitializerExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class PropertyInitializerExpBuilder : IBuilder<ast.PropertyInitializerExp>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.PropertyInitializerExp Build()
    {
        return new ast.PropertyInitializerExp(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public PropertyInitializerExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class UnaryExpBuilder : IBuilder<ast.UnaryExp>
{

    private ast.Operator _Operator;
    private ast.Expression _Operand;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.UnaryExp Build()
    {
        return new ast.UnaryExp(){
             Operator = this._Operator // from UnaryExp
           , Operand = this._Operand // from UnaryExp
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public UnaryExpBuilder WithOperator(ast.Operator value){
        _Operator = value;
        return this;
    }

    public UnaryExpBuilder WithOperand(ast.Expression value){
        _Operand = value;
        return this;
    }

    public UnaryExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class VarRefExpBuilder : IBuilder<ast.VarRefExp>
{

    private System.String _VarName;
    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.VarRefExp Build()
    {
        return new ast.VarRefExp(){
             VarName = this._VarName // from VarRefExp
           , Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public VarRefExpBuilder WithVarName(System.String value){
        _VarName = value;
        return this;
    }

    public VarRefExpBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class ListBuilder : IBuilder<ast.List>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.List Build()
    {
        return new ast.List(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public ListBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class AtomBuilder : IBuilder<ast.Atom>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Atom Build()
    {
        return new ast.Atom(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public AtomBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class TripleBuilder : IBuilder<ast.Triple>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Triple Build()
    {
        return new ast.Triple(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public TripleBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}
public class GraphBuilder : IBuilder<ast.Graph>
{

    private Dictionary<System.String, System.Object> _Annotations;
    
    public ast.Graph Build()
    {
        return new ast.Graph(){
             Annotations = this._Annotations // from AnnotatedThing
        };
    }
    public GraphBuilder WithAnnotations(Dictionary<System.String, System.Object> value){
        _Annotations = value;
        return this;
    }

}

#nullable restore

