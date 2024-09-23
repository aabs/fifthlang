namespace ast_generated;

using System;
using System.Collections.Generic;

public partial class AssemblyDefBuilder : BaseBuilder<AssemblyDefBuilder,ast_model.AssemblyDef>
{
    public AssemblyDefBuilder()
    {
        Model = new();
    }

    public AssemblyDefBuilder WithPublicKeyToken(System.String value){
        Model.PublicKeyToken = value;
        return this;
    }

    public AssemblyDefBuilder WithVersion(System.String value){
        Model.Version = value;
        return this;
    }

    public AssemblyDefBuilder WithAssemblyRefs(LinkedList<ast_model.AssemblyRef> value){
        Model.AssemblyRefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToAssemblyRefs(ast_model.AssemblyRef value){
        if(Model.AssemblyRefs is null)
            Model.AssemblyRefs = [];
        Model.AssemblyRefs.AddLast(value);
        return this;
    }
    public AssemblyDefBuilder WithClassDefs(LinkedList<ast_model.ClassDef> value){
        Model.ClassDefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToClassDefs(ast_model.ClassDef value){
        if(Model.ClassDefs is null)
            Model.ClassDefs = [];
        Model.ClassDefs.AddLast(value);
        return this;
    }
    public AssemblyDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public AssemblyDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public AssemblyDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public AssemblyDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class AssemblyRefBuilder : BaseBuilder<AssemblyRefBuilder,ast_model.AssemblyRef>
{
    public AssemblyRefBuilder()
    {
        Model = new();
    }

    public AssemblyRefBuilder WithPublicKeyToken(System.String value){
        Model.PublicKeyToken = value;
        return this;
    }

    public AssemblyRefBuilder WithVersion(System.String value){
        Model.Version = value;
        return this;
    }

    public AssemblyRefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public AssemblyRefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public AssemblyRefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class AssertionObjectBuilder : BaseBuilder<AssertionObjectBuilder,ast_model.AssertionObject>
{
    public AssertionObjectBuilder()
    {
        Model = new();
    }

}
public partial class AssertionPredicateBuilder : BaseBuilder<AssertionPredicateBuilder,ast_model.AssertionPredicate>
{
    public AssertionPredicateBuilder()
    {
        Model = new();
    }

}
public partial class AssertionStatementBuilder : BaseBuilder<AssertionStatementBuilder,ast_model.AssertionStatement>
{
    public AssertionStatementBuilder()
    {
        Model = new();
    }

    public AssertionStatementBuilder WithAssertionSubject(ast_model.AssertionSubject value){
        Model.AssertionSubject = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionPredicate(ast_model.AssertionPredicate value){
        Model.AssertionPredicate = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionObject(ast_model.AssertionObject value){
        Model.AssertionObject = value;
        return this;
    }

    public AssertionStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public AssertionStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public AssertionStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class AssertionSubjectBuilder : BaseBuilder<AssertionSubjectBuilder,ast_model.AssertionSubject>
{
    public AssertionSubjectBuilder()
    {
        Model = new();
    }

}
public partial class AssignmentStatementBuilder : BaseBuilder<AssignmentStatementBuilder,ast_model.AssignmentStatement>
{
    public AssignmentStatementBuilder()
    {
        Model = new();
    }

    public AssignmentStatementBuilder WithRHS(ast_model.Expression value){
        Model.RHS = value;
        return this;
    }

    public AssignmentStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public AssignmentStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public AssignmentStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class AtomBuilder : BaseBuilder<AtomBuilder,ast_model.Atom>
{
    public AtomBuilder()
    {
        Model = new();
    }

    public AtomBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public AtomBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public AtomBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class BinaryExpBuilder : BaseBuilder<BinaryExpBuilder,ast_model.BinaryExp>
{
    public BinaryExpBuilder()
    {
        Model = new();
    }

    public BinaryExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public BinaryExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public BinaryExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class BlockStatementBuilder : BaseBuilder<BlockStatementBuilder,ast_model.BlockStatement>
{
    public BlockStatementBuilder()
    {
        Model = new();
    }

    public BlockStatementBuilder WithStatements(List<ast_model.Statement> value){
        Model.Statements = value;
        return this;
    }

    public BlockStatementBuilder AddingItemToStatements(ast_model.Statement value){
        if(Model.Statements is null)
            Model.Statements = [];
        Model.Statements.Add(value);
        return this;
    }
    public BlockStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public BlockStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public BlockStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class CastExpBuilder : BaseBuilder<CastExpBuilder,ast_model.CastExp>
{
    public CastExpBuilder()
    {
        Model = new();
    }

    public CastExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public CastExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public CastExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ClassDefBuilder : BaseBuilder<ClassDefBuilder,ast_model.ClassDef>
{
    public ClassDefBuilder()
    {
        Model = new();
    }

    public ClassDefBuilder WithMemberDefs(LinkedList<ast_model.MemberDef> value){
        Model.MemberDefs = value;
        return this;
    }

    public ClassDefBuilder AddingItemToMemberDefs(ast_model.MemberDef value){
        if(Model.MemberDefs is null)
            Model.MemberDefs = [];
        Model.MemberDefs.AddLast(value);
        return this;
    }
    public ClassDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public ClassDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ClassDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ClassDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ExpStatementBuilder : BaseBuilder<ExpStatementBuilder,ast_model.ExpStatement>
{
    public ExpStatementBuilder()
    {
        Model = new();
    }

    public ExpStatementBuilder WithRHS(ast_model.Expression value){
        Model.RHS = value;
        return this;
    }

    public ExpStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ExpStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ExpStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class FieldDefBuilder : BaseBuilder<FieldDefBuilder,ast_model.FieldDef>
{
    public FieldDefBuilder()
    {
        Model = new();
    }

    public FieldDefBuilder WithAccessConstraints(ast_model.AccessConstraint[] value){
        Model.AccessConstraints = value;
        return this;
    }

    public FieldDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public FieldDefBuilder WithIsReadOnly(System.Boolean value){
        Model.IsReadOnly = value;
        return this;
    }

    public FieldDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public FieldDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public FieldDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ForeachStatementBuilder : BaseBuilder<ForeachStatementBuilder,ast_model.ForeachStatement>
{
    public ForeachStatementBuilder()
    {
        Model = new();
    }

    public ForeachStatementBuilder WithCollection(ast_model.Expression value){
        Model.Collection = value;
        return this;
    }

    public ForeachStatementBuilder WithLoopVariable(ast_model.VariableDecl value){
        Model.LoopVariable = value;
        return this;
    }

    public ForeachStatementBuilder WithBody(ast_model.BlockStatement value){
        Model.Body = value;
        return this;
    }

    public ForeachStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ForeachStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ForeachStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ForStatementBuilder : BaseBuilder<ForStatementBuilder,ast_model.ForStatement>
{
    public ForStatementBuilder()
    {
        Model = new();
    }

    public ForStatementBuilder WithInitialValue(ast_model.Expression value){
        Model.InitialValue = value;
        return this;
    }

    public ForStatementBuilder WithConstraint(ast_model.Expression value){
        Model.Constraint = value;
        return this;
    }

    public ForStatementBuilder WithIncrementExpression(ast_model.Expression value){
        Model.IncrementExpression = value;
        return this;
    }

    public ForStatementBuilder WithLoopVariable(ast_model.VariableDecl value){
        Model.LoopVariable = value;
        return this;
    }

    public ForStatementBuilder WithBody(ast_model.BlockStatement value){
        Model.Body = value;
        return this;
    }

    public ForStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ForStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ForStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class FuncCallExpBuilder : BaseBuilder<FuncCallExpBuilder,ast_model.FuncCallExp>
{
    public FuncCallExpBuilder()
    {
        Model = new();
    }

    public FuncCallExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public FuncCallExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public FuncCallExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class GraphBuilder : BaseBuilder<GraphBuilder,ast_model.Graph>
{
    public GraphBuilder()
    {
        Model = new();
    }

    public GraphBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public GraphBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public GraphBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class GraphNamespaceAliasBuilder : BaseBuilder<GraphNamespaceAliasBuilder,ast_model.GraphNamespaceAlias>
{
    public GraphNamespaceAliasBuilder()
    {
        Model = new();
    }

    public GraphNamespaceAliasBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithUri(System.Uri value){
        Model.Uri = value;
        return this;
    }

}
public partial class GuardStatementBuilder : BaseBuilder<GuardStatementBuilder,ast_model.GuardStatement>
{
    public GuardStatementBuilder()
    {
        Model = new();
    }

    public GuardStatementBuilder WithCondition(ast_model.Expression value){
        Model.Condition = value;
        return this;
    }

    public GuardStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public GuardStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public GuardStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class IfElseStatementBuilder : BaseBuilder<IfElseStatementBuilder,ast_model.IfElseStatement>
{
    public IfElseStatementBuilder()
    {
        Model = new();
    }

    public IfElseStatementBuilder WithCondition(ast_model.Expression value){
        Model.Condition = value;
        return this;
    }

    public IfElseStatementBuilder WithThenBlock(ast_model.BlockStatement value){
        Model.ThenBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithElseBlock(ast_model.BlockStatement value){
        Model.ElseBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public IfElseStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public IfElseStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class InferenceRuleDefBuilder : BaseBuilder<InferenceRuleDefBuilder,ast_model.InferenceRuleDef>
{
    public InferenceRuleDefBuilder()
    {
        Model = new();
    }

    public InferenceRuleDefBuilder WithAntecedent(ast_model.Expression value){
        Model.Antecedent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithConsequent(ast_model.KnowledgeManagementBlock value){
        Model.Consequent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public InferenceRuleDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public InferenceRuleDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public InferenceRuleDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class KnowledgeManagementBlockBuilder : BaseBuilder<KnowledgeManagementBlockBuilder,ast_model.KnowledgeManagementBlock>
{
    public KnowledgeManagementBlockBuilder()
    {
        Model = new();
    }

    public KnowledgeManagementBlockBuilder WithStatements(List<ast_model.KnowledgeManagementStatement> value){
        Model.Statements = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder AddingItemToStatements(ast_model.KnowledgeManagementStatement value){
        if(Model.Statements is null)
            Model.Statements = [];
        Model.Statements.Add(value);
        return this;
    }
}
public partial class LambdaExpBuilder : BaseBuilder<LambdaExpBuilder,ast_model.LambdaExp>
{
    public LambdaExpBuilder()
    {
        Model = new();
    }

    public LambdaExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public LambdaExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public LambdaExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ListBuilder : BaseBuilder<ListBuilder,ast_model.List>
{
    public ListBuilder()
    {
        Model = new();
    }

    public ListBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ListBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ListBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class LiteralExpBuilder : BaseBuilder<LiteralExpBuilder,ast_model.LiteralExp>
{
    public LiteralExpBuilder()
    {
        Model = new();
    }

    public LiteralExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public LiteralExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public LiteralExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class MemberAccessExpBuilder : BaseBuilder<MemberAccessExpBuilder,ast_model.MemberAccessExp>
{
    public MemberAccessExpBuilder()
    {
        Model = new();
    }

    public MemberAccessExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public MemberAccessExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public MemberAccessExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class MemberDefBuilder : BaseBuilder<MemberDefBuilder,ast_model.MemberDef>
{
    public MemberDefBuilder()
    {
        Model = new();
    }

    public MemberDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public MemberDefBuilder WithIsReadOnly(System.Boolean value){
        Model.IsReadOnly = value;
        return this;
    }

    public MemberDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public MemberDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public MemberDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class MemberRefBuilder : BaseBuilder<MemberRefBuilder,ast_model.MemberRef>
{
    public MemberRefBuilder()
    {
        Model = new();
    }

    public MemberRefBuilder WithMemberDef(ast_model.MemberDef value){
        Model.MemberDef = value;
        return this;
    }

    public MemberRefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public MemberRefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public MemberRefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class MethodDefBuilder : BaseBuilder<MethodDefBuilder,ast_model.MethodDef>
{
    public MethodDefBuilder()
    {
        Model = new();
    }

    public MethodDefBuilder WithParams(List<ast_model.ParamDef> value){
        Model.Params = value;
        return this;
    }

    public MethodDefBuilder AddingItemToParams(ast_model.ParamDef value){
        if(Model.Params is null)
            Model.Params = [];
        Model.Params.Add(value);
        return this;
    }
    public MethodDefBuilder WithBody(ast_model.BlockStatement value){
        Model.Body = value;
        return this;
    }

    public MethodDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public MethodDefBuilder WithIsReadOnly(System.Boolean value){
        Model.IsReadOnly = value;
        return this;
    }

    public MethodDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public MethodDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public MethodDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ObjectInstantiationExpBuilder : BaseBuilder<ObjectInstantiationExpBuilder,ast_model.ObjectInstantiationExp>
{
    public ObjectInstantiationExpBuilder()
    {
        Model = new();
    }

    public ObjectInstantiationExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ParamDefBuilder : BaseBuilder<ParamDefBuilder,ast_model.ParamDef>
{
    public ParamDefBuilder()
    {
        Model = new();
    }

    public ParamDefBuilder WithParameterConstraint(ast_model.Expression value){
        Model.ParameterConstraint = value;
        return this;
    }

    public ParamDefBuilder WithDestructureDef(ast_model.ParamDestructureDef value){
        Model.DestructureDef = value;
        return this;
    }

    public ParamDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public ParamDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ParamDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ParamDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ParamDestructureDefBuilder : BaseBuilder<ParamDestructureDefBuilder,ast_model.ParamDestructureDef>
{
    public ParamDestructureDefBuilder()
    {
        Model = new();
    }

    public ParamDestructureDefBuilder WithBindings(LinkedList<ast_model.PropertyBindingDef> value){
        Model.Bindings = value;
        return this;
    }

    public ParamDestructureDefBuilder AddingItemToBindings(ast_model.PropertyBindingDef value){
        if(Model.Bindings is null)
            Model.Bindings = [];
        Model.Bindings.AddLast(value);
        return this;
    }
    public ParamDestructureDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public ParamDestructureDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ParamDestructureDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ParamDestructureDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class PropertyBindingDefBuilder : BaseBuilder<PropertyBindingDefBuilder,ast_model.PropertyBindingDef>
{
    public PropertyBindingDefBuilder()
    {
        Model = new();
    }

    public PropertyBindingDefBuilder WithIntroducedVariable(ast_model.VariableDecl value){
        Model.IntroducedVariable = value;
        return this;
    }

    public PropertyBindingDefBuilder WithReferencedProperty(ast_model.PropertyDef value){
        Model.ReferencedProperty = value;
        return this;
    }

    public PropertyBindingDefBuilder WithDestructureDef(ast_model.ParamDestructureDef value){
        Model.DestructureDef = value;
        return this;
    }

    public PropertyBindingDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public PropertyBindingDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public PropertyBindingDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public PropertyBindingDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class PropertyDefBuilder : BaseBuilder<PropertyDefBuilder,ast_model.PropertyDef>
{
    public PropertyDefBuilder()
    {
        Model = new();
    }

    public PropertyDefBuilder WithAccessConstraints(ast_model.AccessConstraint[] value){
        Model.AccessConstraints = value;
        return this;
    }

    public PropertyDefBuilder WithIsWriteOnly(System.Boolean value){
        Model.IsWriteOnly = value;
        return this;
    }

    public PropertyDefBuilder WithBackingField(ast_model.FieldDef value){
        Model.BackingField = value;
        return this;
    }

    public PropertyDefBuilder WithGetter(ast_model.MethodDef value){
        Model.Getter = value;
        return this;
    }

    public PropertyDefBuilder WithSetter(ast_model.MethodDef value){
        Model.Setter = value;
        return this;
    }

    public PropertyDefBuilder WithCtorOnlySetter(System.Boolean value){
        Model.CtorOnlySetter = value;
        return this;
    }

    public PropertyDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public PropertyDefBuilder WithIsReadOnly(System.Boolean value){
        Model.IsReadOnly = value;
        return this;
    }

    public PropertyDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public PropertyDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public PropertyDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class RetractionStatementBuilder : BaseBuilder<RetractionStatementBuilder,ast_model.RetractionStatement>
{
    public RetractionStatementBuilder()
    {
        Model = new();
    }

    public RetractionStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public RetractionStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public RetractionStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class ReturnStatementBuilder : BaseBuilder<ReturnStatementBuilder,ast_model.ReturnStatement>
{
    public ReturnStatementBuilder()
    {
        Model = new();
    }

    public ReturnStatementBuilder WithReturnValue(ast_model.Expression value){
        Model.ReturnValue = value;
        return this;
    }

    public ReturnStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public ReturnStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public ReturnStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class TripleBuilder : BaseBuilder<TripleBuilder,ast_model.Triple>
{
    public TripleBuilder()
    {
        Model = new();
    }

    public TripleBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public TripleBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public TripleBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class TypeDefBuilder : BaseBuilder<TypeDefBuilder,ast_model.TypeDef>
{
    public TypeDefBuilder()
    {
        Model = new();
    }

    public TypeDefBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public TypeDefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public TypeDefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public TypeDefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class TypeRefBuilder : BaseBuilder<TypeRefBuilder,ast_model.TypeRef>
{
    public TypeRefBuilder()
    {
        Model = new();
    }

    public TypeRefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public TypeRefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public TypeRefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class UnaryExpBuilder : BaseBuilder<UnaryExpBuilder,ast_model.UnaryExp>
{
    public UnaryExpBuilder()
    {
        Model = new();
    }

    public UnaryExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public UnaryExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public UnaryExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class VarDeclStatementBuilder : BaseBuilder<VarDeclStatementBuilder,ast_model.VarDeclStatement>
{
    public VarDeclStatementBuilder()
    {
        Model = new();
    }

    public VarDeclStatementBuilder WithVariableDecl(ast_model.VariableDecl value){
        Model.VariableDecl = value;
        return this;
    }

    public VarDeclStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public VarDeclStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public VarDeclStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class VariableDeclBuilder : BaseBuilder<VariableDeclBuilder,ast_model.VariableDecl>
{
    public VariableDeclBuilder()
    {
        Model = new();
    }

    public VariableDeclBuilder WithInitialValue(ast_model.Expression value){
        Model.InitialValue = value;
        return this;
    }

    public VariableDeclBuilder WithVisibility(ast_model.Visibility value){
        Model.Visibility = value;
        return this;
    }

    public VariableDeclBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public VariableDeclBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public VariableDeclBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class VarRefBuilder : BaseBuilder<VarRefBuilder,ast_model.VarRef>
{
    public VarRefBuilder()
    {
        Model = new();
    }

    public VarRefBuilder WithVarDecl(ast_model.VarDeclStatement value){
        Model.VarDecl = value;
        return this;
    }

    public VarRefBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public VarRefBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public VarRefBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class VarRefExpBuilder : BaseBuilder<VarRefExpBuilder,ast_model.VarRefExp>
{
    public VarRefExpBuilder()
    {
        Model = new();
    }

    public VarRefExpBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public VarRefExpBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public VarRefExpBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class WhileStatementBuilder : BaseBuilder<WhileStatementBuilder,ast_model.WhileStatement>
{
    public WhileStatementBuilder()
    {
        Model = new();
    }

    public WhileStatementBuilder WithCondition(ast_model.Expression value){
        Model.Condition = value;
        return this;
    }

    public WhileStatementBuilder WithBody(ast_model.BlockStatement value){
        Model.Body = value;
        return this;
    }

    public WhileStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public WhileStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public WhileStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
public partial class WithScopeStatementBuilder : BaseBuilder<WithScopeStatementBuilder,ast_model.WithScopeStatement>
{
    public WithScopeStatementBuilder()
    {
        Model = new();
    }

    public WithScopeStatementBuilder WithType(ast_model.TypeMetadata value){
        Model.Type = value;
        return this;
    }

    public WithScopeStatementBuilder WithName(System.String value){
        Model.Name = value;
        return this;
    }

    public WithScopeStatementBuilder WithParent(ast_model.AstThing value){
        Model.Parent = value;
        return this;
    }

}
