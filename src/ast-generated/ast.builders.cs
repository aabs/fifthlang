namespace ast_generated;

using System.Collections.Generic;
#nullable disable

public class AssemblyDefBuilder : IBuilder<ast_model.AssemblyDef>
{

    private System.String _PublicKeyToken;
    private System.String _Version;
    private LinkedList<ast_model.AssemblyRef> _AssemblyRefs;
    private LinkedList<ast_model.ClassDef> _ClassDefs;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.AssemblyDef Build()
    {
        return new ast_model.AssemblyDef(){
             PublicKeyToken = this._PublicKeyToken
           , Version = this._Version
           , AssemblyRefs = this._AssemblyRefs
           , ClassDefs = this._ClassDefs
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public AssemblyDefBuilder WithPublicKeyToken(System.String value){
        _PublicKeyToken = value;
        return this;
    }

    public AssemblyDefBuilder WithVersion(System.String value){
        _Version = value;
        return this;
    }

    public AssemblyDefBuilder WithAssemblyRefs(LinkedList<ast_model.AssemblyRef> value){
        _AssemblyRefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToAssemblyRefs(ast_model.AssemblyRef value){
        _AssemblyRefs  ??= [];
        _AssemblyRefs.AddLast(value);
        return this;
    }
    public AssemblyDefBuilder WithClassDefs(LinkedList<ast_model.ClassDef> value){
        _ClassDefs = value;
        return this;
    }

    public AssemblyDefBuilder AddingItemToClassDefs(ast_model.ClassDef value){
        _ClassDefs  ??= [];
        _ClassDefs.AddLast(value);
        return this;
    }
    public AssemblyDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public AssemblyDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssemblyDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public AssemblyDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class AssemblyRefBuilder : IBuilder<ast_model.AssemblyRef>
{

    private System.String _PublicKeyToken;
    private System.String _Version;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.AssemblyRef Build()
    {
        return new ast_model.AssemblyRef(){
             PublicKeyToken = this._PublicKeyToken
           , Version = this._Version
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
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

    public AssemblyRefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssemblyRefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public AssemblyRefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class AssertionObjectBuilder : IBuilder<ast_model.AssertionObject>
{

    
    public ast_model.AssertionObject Build()
    {
        return new ast_model.AssertionObject(){
        };
    }
}
public class AssertionPredicateBuilder : IBuilder<ast_model.AssertionPredicate>
{

    
    public ast_model.AssertionPredicate Build()
    {
        return new ast_model.AssertionPredicate(){
        };
    }
}
public class AssertionStatementBuilder : IBuilder<ast_model.AssertionStatement>
{

    private ast_model.AssertionSubject _AssertionSubject;
    private ast_model.AssertionPredicate _AssertionPredicate;
    private ast_model.AssertionObject _AssertionObject;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.AssertionStatement Build()
    {
        return new ast_model.AssertionStatement(){
             AssertionSubject = this._AssertionSubject
           , AssertionPredicate = this._AssertionPredicate
           , AssertionObject = this._AssertionObject
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public AssertionStatementBuilder WithAssertionSubject(ast_model.AssertionSubject value){
        _AssertionSubject = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionPredicate(ast_model.AssertionPredicate value){
        _AssertionPredicate = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionObject(ast_model.AssertionObject value){
        _AssertionObject = value;
        return this;
    }

    public AssertionStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssertionStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public AssertionStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class AssertionSubjectBuilder : IBuilder<ast_model.AssertionSubject>
{

    
    public ast_model.AssertionSubject Build()
    {
        return new ast_model.AssertionSubject(){
        };
    }
}
public class AssignmentStatementBuilder : IBuilder<ast_model.AssignmentStatement>
{

    private ast_model.Expression _RHS;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.AssignmentStatement Build()
    {
        return new ast_model.AssignmentStatement(){
             RHS = this._RHS
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public AssignmentStatementBuilder WithRHS(ast_model.Expression value){
        _RHS = value;
        return this;
    }

    public AssignmentStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AssignmentStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public AssignmentStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class AtomBuilder : IBuilder<ast_model.Atom>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.Atom Build()
    {
        return new ast_model.Atom(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public AtomBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public AtomBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public AtomBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class BinaryExpBuilder : IBuilder<ast_model.BinaryExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.BinaryExp Build()
    {
        return new ast_model.BinaryExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public BinaryExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public BinaryExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public BinaryExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class BlockStatementBuilder : IBuilder<ast_model.BlockStatement>
{

    private List<ast_model.Statement> _Statements;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.BlockStatement Build()
    {
        return new ast_model.BlockStatement(){
             Statements = this._Statements
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public BlockStatementBuilder WithStatements(List<ast_model.Statement> value){
        _Statements = value;
        return this;
    }

    public BlockStatementBuilder AddingItemToStatements(ast_model.Statement value){
        _Statements  ??= [];
        _Statements.Add(value);
        return this;
    }
    public BlockStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public BlockStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public BlockStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class CastExpBuilder : IBuilder<ast_model.CastExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.CastExp Build()
    {
        return new ast_model.CastExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public CastExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public CastExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public CastExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ClassDefBuilder : IBuilder<ast_model.ClassDef>
{

    private LinkedList<ast_model.MemberDef> _MemberDefs;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ClassDef Build()
    {
        return new ast_model.ClassDef(){
             MemberDefs = this._MemberDefs
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ClassDefBuilder WithMemberDefs(LinkedList<ast_model.MemberDef> value){
        _MemberDefs = value;
        return this;
    }

    public ClassDefBuilder AddingItemToMemberDefs(ast_model.MemberDef value){
        _MemberDefs  ??= [];
        _MemberDefs.AddLast(value);
        return this;
    }
    public ClassDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public ClassDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ClassDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ClassDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ExpStatementBuilder : IBuilder<ast_model.ExpStatement>
{

    private ast_model.Expression _RHS;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ExpStatement Build()
    {
        return new ast_model.ExpStatement(){
             RHS = this._RHS
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ExpStatementBuilder WithRHS(ast_model.Expression value){
        _RHS = value;
        return this;
    }

    public ExpStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ExpStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ExpStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class FieldDefBuilder : IBuilder<ast_model.FieldDef>
{

    private ast_model.AccessConstraint[] _AccessConstraints;
    private System.Boolean _IsReadOnly;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.FieldDef Build()
    {
        return new ast_model.FieldDef(){
             AccessConstraints = this._AccessConstraints
           , IsReadOnly = this._IsReadOnly
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public FieldDefBuilder WithAccessConstraints(ast_model.AccessConstraint[] value){
        _AccessConstraints = value;
        return this;
    }

    public FieldDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public FieldDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public FieldDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public FieldDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public FieldDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ForeachStatementBuilder : IBuilder<ast_model.ForeachStatement>
{

    private ast_model.Expression _Collection;
    private ast_model.VariableDecl _LoopVariable;
    private ast_model.BlockStatement _Body;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ForeachStatement Build()
    {
        return new ast_model.ForeachStatement(){
             Collection = this._Collection
           , LoopVariable = this._LoopVariable
           , Body = this._Body
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ForeachStatementBuilder WithCollection(ast_model.Expression value){
        _Collection = value;
        return this;
    }

    public ForeachStatementBuilder WithLoopVariable(ast_model.VariableDecl value){
        _LoopVariable = value;
        return this;
    }

    public ForeachStatementBuilder WithBody(ast_model.BlockStatement value){
        _Body = value;
        return this;
    }

    public ForeachStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ForeachStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ForeachStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ForStatementBuilder : IBuilder<ast_model.ForStatement>
{

    private ast_model.Expression _InitialValue;
    private ast_model.Expression _Constraint;
    private ast_model.Expression _IncrementExpression;
    private ast_model.VariableDecl _LoopVariable;
    private ast_model.BlockStatement _Body;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ForStatement Build()
    {
        return new ast_model.ForStatement(){
             InitialValue = this._InitialValue
           , Constraint = this._Constraint
           , IncrementExpression = this._IncrementExpression
           , LoopVariable = this._LoopVariable
           , Body = this._Body
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ForStatementBuilder WithInitialValue(ast_model.Expression value){
        _InitialValue = value;
        return this;
    }

    public ForStatementBuilder WithConstraint(ast_model.Expression value){
        _Constraint = value;
        return this;
    }

    public ForStatementBuilder WithIncrementExpression(ast_model.Expression value){
        _IncrementExpression = value;
        return this;
    }

    public ForStatementBuilder WithLoopVariable(ast_model.VariableDecl value){
        _LoopVariable = value;
        return this;
    }

    public ForStatementBuilder WithBody(ast_model.BlockStatement value){
        _Body = value;
        return this;
    }

    public ForStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ForStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ForStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class FuncCallExpBuilder : IBuilder<ast_model.FuncCallExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.FuncCallExp Build()
    {
        return new ast_model.FuncCallExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public FuncCallExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public FuncCallExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public FuncCallExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class GraphBuilder : IBuilder<ast_model.Graph>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.Graph Build()
    {
        return new ast_model.Graph(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public GraphBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public GraphBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public GraphBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class GraphNamespaceAliasBuilder : IBuilder<ast_model.GraphNamespaceAlias>
{

    private System.String _Name;
    private System.Uri _Uri;
    
    public ast_model.GraphNamespaceAlias Build()
    {
        return new ast_model.GraphNamespaceAlias(){
             Name = this._Name
           , Uri = this._Uri
        };
    }
    public GraphNamespaceAliasBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public GraphNamespaceAliasBuilder WithUri(System.Uri value){
        _Uri = value;
        return this;
    }

}
public class GuardStatementBuilder : IBuilder<ast_model.GuardStatement>
{

    private ast_model.Expression _Condition;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.GuardStatement Build()
    {
        return new ast_model.GuardStatement(){
             Condition = this._Condition
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public GuardStatementBuilder WithCondition(ast_model.Expression value){
        _Condition = value;
        return this;
    }

    public GuardStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public GuardStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public GuardStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class IfElseStatementBuilder : IBuilder<ast_model.IfElseStatement>
{

    private ast_model.Expression _Condition;
    private ast_model.BlockStatement _ThenBlock;
    private ast_model.BlockStatement _ElseBlock;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.IfElseStatement Build()
    {
        return new ast_model.IfElseStatement(){
             Condition = this._Condition
           , ThenBlock = this._ThenBlock
           , ElseBlock = this._ElseBlock
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public IfElseStatementBuilder WithCondition(ast_model.Expression value){
        _Condition = value;
        return this;
    }

    public IfElseStatementBuilder WithThenBlock(ast_model.BlockStatement value){
        _ThenBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithElseBlock(ast_model.BlockStatement value){
        _ElseBlock = value;
        return this;
    }

    public IfElseStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public IfElseStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public IfElseStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class InferenceRuleDefBuilder : IBuilder<ast_model.InferenceRuleDef>
{

    private ast_model.Expression _Antecedent;
    private ast_model.KnowledgeManagementBlock _Consequent;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.InferenceRuleDef Build()
    {
        return new ast_model.InferenceRuleDef(){
             Antecedent = this._Antecedent
           , Consequent = this._Consequent
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public InferenceRuleDefBuilder WithAntecedent(ast_model.Expression value){
        _Antecedent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithConsequent(ast_model.KnowledgeManagementBlock value){
        _Consequent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public InferenceRuleDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public InferenceRuleDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public InferenceRuleDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class KnowledgeManagementBlockBuilder : IBuilder<ast_model.KnowledgeManagementBlock>
{

    private List<ast_model.KnowledgeManagementStatement> _Statements;
    
    public ast_model.KnowledgeManagementBlock Build()
    {
        return new ast_model.KnowledgeManagementBlock(){
             Statements = this._Statements
        };
    }
    public KnowledgeManagementBlockBuilder WithStatements(List<ast_model.KnowledgeManagementStatement> value){
        _Statements = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder AddingItemToStatements(ast_model.KnowledgeManagementStatement value){
        _Statements  ??= [];
        _Statements.Add(value);
        return this;
    }
}
public class LambdaExpBuilder : IBuilder<ast_model.LambdaExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.LambdaExp Build()
    {
        return new ast_model.LambdaExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public LambdaExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public LambdaExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public LambdaExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ListBuilder : IBuilder<ast_model.List>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.List Build()
    {
        return new ast_model.List(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ListBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ListBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ListBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class LiteralExpBuilder : IBuilder<ast_model.LiteralExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.LiteralExp Build()
    {
        return new ast_model.LiteralExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public LiteralExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public LiteralExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public LiteralExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class MemberAccessExpBuilder : IBuilder<ast_model.MemberAccessExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.MemberAccessExp Build()
    {
        return new ast_model.MemberAccessExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public MemberAccessExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MemberAccessExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public MemberAccessExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class MemberDefBuilder : IBuilder<ast_model.MemberDef>
{

    private System.Boolean _IsReadOnly;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.MemberDef Build()
    {
        return new ast_model.MemberDef(){
             IsReadOnly = this._IsReadOnly
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public MemberDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public MemberDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public MemberDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MemberDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public MemberDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class MemberRefBuilder : IBuilder<ast_model.MemberRef>
{

    private ast_model.MemberDef _MemberDef;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.MemberRef Build()
    {
        return new ast_model.MemberRef(){
             MemberDef = this._MemberDef
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public MemberRefBuilder WithMemberDef(ast_model.MemberDef value){
        _MemberDef = value;
        return this;
    }

    public MemberRefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MemberRefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public MemberRefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class MethodDefBuilder : IBuilder<ast_model.MethodDef>
{

    private List<ast_model.ParamDef> _Params;
    private ast_model.BlockStatement _Body;
    private System.Boolean _IsReadOnly;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.MethodDef Build()
    {
        return new ast_model.MethodDef(){
             Params = this._Params
           , Body = this._Body
           , IsReadOnly = this._IsReadOnly
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public MethodDefBuilder WithParams(List<ast_model.ParamDef> value){
        _Params = value;
        return this;
    }

    public MethodDefBuilder AddingItemToParams(ast_model.ParamDef value){
        _Params  ??= [];
        _Params.Add(value);
        return this;
    }
    public MethodDefBuilder WithBody(ast_model.BlockStatement value){
        _Body = value;
        return this;
    }

    public MethodDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public MethodDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public MethodDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public MethodDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public MethodDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ObjectInstantiationExpBuilder : IBuilder<ast_model.ObjectInstantiationExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ObjectInstantiationExp Build()
    {
        return new ast_model.ObjectInstantiationExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ObjectInstantiationExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ObjectInstantiationExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ParamDefBuilder : IBuilder<ast_model.ParamDef>
{

    private ast_model.Expression _ParameterConstraint;
    private ast_model.ParamDestructureDef _DestructureDef;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ParamDef Build()
    {
        return new ast_model.ParamDef(){
             ParameterConstraint = this._ParameterConstraint
           , DestructureDef = this._DestructureDef
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ParamDefBuilder WithParameterConstraint(ast_model.Expression value){
        _ParameterConstraint = value;
        return this;
    }

    public ParamDefBuilder WithDestructureDef(ast_model.ParamDestructureDef value){
        _DestructureDef = value;
        return this;
    }

    public ParamDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public ParamDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ParamDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ParamDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ParamDestructureDefBuilder : IBuilder<ast_model.ParamDestructureDef>
{

    private LinkedList<ast_model.PropertyBindingDef> _Bindings;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ParamDestructureDef Build()
    {
        return new ast_model.ParamDestructureDef(){
             Bindings = this._Bindings
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ParamDestructureDefBuilder WithBindings(LinkedList<ast_model.PropertyBindingDef> value){
        _Bindings = value;
        return this;
    }

    public ParamDestructureDefBuilder AddingItemToBindings(ast_model.PropertyBindingDef value){
        _Bindings  ??= [];
        _Bindings.AddLast(value);
        return this;
    }
    public ParamDestructureDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public ParamDestructureDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ParamDestructureDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ParamDestructureDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class PropertyBindingDefBuilder : IBuilder<ast_model.PropertyBindingDef>
{

    private ast_model.VariableDecl _IntroducedVariable;
    private ast_model.PropertyDef _ReferencedProperty;
    private ast_model.ParamDestructureDef _DestructureDef;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.PropertyBindingDef Build()
    {
        return new ast_model.PropertyBindingDef(){
             IntroducedVariable = this._IntroducedVariable
           , ReferencedProperty = this._ReferencedProperty
           , DestructureDef = this._DestructureDef
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public PropertyBindingDefBuilder WithIntroducedVariable(ast_model.VariableDecl value){
        _IntroducedVariable = value;
        return this;
    }

    public PropertyBindingDefBuilder WithReferencedProperty(ast_model.PropertyDef value){
        _ReferencedProperty = value;
        return this;
    }

    public PropertyBindingDefBuilder WithDestructureDef(ast_model.ParamDestructureDef value){
        _DestructureDef = value;
        return this;
    }

    public PropertyBindingDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public PropertyBindingDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public PropertyBindingDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public PropertyBindingDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class PropertyDefBuilder : IBuilder<ast_model.PropertyDef>
{

    private ast_model.AccessConstraint[] _AccessConstraints;
    private System.Boolean _IsWriteOnly;
    private ast_model.FieldDef _BackingField;
    private ast_model.MethodDef _Getter;
    private ast_model.MethodDef _Setter;
    private System.Boolean _CtorOnlySetter;
    private System.Boolean _IsReadOnly;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.PropertyDef Build()
    {
        return new ast_model.PropertyDef(){
             AccessConstraints = this._AccessConstraints
           , IsWriteOnly = this._IsWriteOnly
           , BackingField = this._BackingField
           , Getter = this._Getter
           , Setter = this._Setter
           , CtorOnlySetter = this._CtorOnlySetter
           , IsReadOnly = this._IsReadOnly
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public PropertyDefBuilder WithAccessConstraints(ast_model.AccessConstraint[] value){
        _AccessConstraints = value;
        return this;
    }

    public PropertyDefBuilder WithIsWriteOnly(System.Boolean value){
        _IsWriteOnly = value;
        return this;
    }

    public PropertyDefBuilder WithBackingField(ast_model.FieldDef value){
        _BackingField = value;
        return this;
    }

    public PropertyDefBuilder WithGetter(ast_model.MethodDef value){
        _Getter = value;
        return this;
    }

    public PropertyDefBuilder WithSetter(ast_model.MethodDef value){
        _Setter = value;
        return this;
    }

    public PropertyDefBuilder WithCtorOnlySetter(System.Boolean value){
        _CtorOnlySetter = value;
        return this;
    }

    public PropertyDefBuilder WithIsReadOnly(System.Boolean value){
        _IsReadOnly = value;
        return this;
    }

    public PropertyDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public PropertyDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public PropertyDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public PropertyDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class RetractionStatementBuilder : IBuilder<ast_model.RetractionStatement>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.RetractionStatement Build()
    {
        return new ast_model.RetractionStatement(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public RetractionStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public RetractionStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public RetractionStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class ReturnStatementBuilder : IBuilder<ast_model.ReturnStatement>
{

    private ast_model.Expression _ReturnValue;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.ReturnStatement Build()
    {
        return new ast_model.ReturnStatement(){
             ReturnValue = this._ReturnValue
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public ReturnStatementBuilder WithReturnValue(ast_model.Expression value){
        _ReturnValue = value;
        return this;
    }

    public ReturnStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public ReturnStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public ReturnStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class TripleBuilder : IBuilder<ast_model.Triple>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.Triple Build()
    {
        return new ast_model.Triple(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public TripleBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TripleBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public TripleBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class TypeDefBuilder : IBuilder<ast_model.TypeDef>
{

    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.TypeDef Build()
    {
        return new ast_model.TypeDef(){
             Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public TypeDefBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public TypeDefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TypeDefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public TypeDefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class TypeRefBuilder : IBuilder<ast_model.TypeRef>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.TypeRef Build()
    {
        return new ast_model.TypeRef(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public TypeRefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public TypeRefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public TypeRefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class UnaryExpBuilder : IBuilder<ast_model.UnaryExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.UnaryExp Build()
    {
        return new ast_model.UnaryExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public UnaryExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public UnaryExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public UnaryExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class VarDeclStatementBuilder : IBuilder<ast_model.VarDeclStatement>
{

    private ast_model.VariableDecl _VariableDecl;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.VarDeclStatement Build()
    {
        return new ast_model.VarDeclStatement(){
             VariableDecl = this._VariableDecl
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public VarDeclStatementBuilder WithVariableDecl(ast_model.VariableDecl value){
        _VariableDecl = value;
        return this;
    }

    public VarDeclStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarDeclStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public VarDeclStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class VariableDeclBuilder : IBuilder<ast_model.VariableDecl>
{

    private ast_model.Expression _InitialValue;
    private ast_model.Visibility _Visibility;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.VariableDecl Build()
    {
        return new ast_model.VariableDecl(){
             InitialValue = this._InitialValue
           , Visibility = this._Visibility
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public VariableDeclBuilder WithInitialValue(ast_model.Expression value){
        _InitialValue = value;
        return this;
    }

    public VariableDeclBuilder WithVisibility(ast_model.Visibility value){
        _Visibility = value;
        return this;
    }

    public VariableDeclBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VariableDeclBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public VariableDeclBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class VarRefBuilder : IBuilder<ast_model.VarRef>
{

    private ast_model.VarDeclStatement _VarDecl;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.VarRef Build()
    {
        return new ast_model.VarRef(){
             VarDecl = this._VarDecl
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public VarRefBuilder WithVarDecl(ast_model.VarDeclStatement value){
        _VarDecl = value;
        return this;
    }

    public VarRefBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarRefBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public VarRefBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class VarRefExpBuilder : IBuilder<ast_model.VarRefExp>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.VarRefExp Build()
    {
        return new ast_model.VarRefExp(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public VarRefExpBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public VarRefExpBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public VarRefExpBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class WhileStatementBuilder : IBuilder<ast_model.WhileStatement>
{

    private ast_model.Expression _Condition;
    private ast_model.BlockStatement _Body;
    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.WhileStatement Build()
    {
        return new ast_model.WhileStatement(){
             Condition = this._Condition
           , Body = this._Body
           , Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public WhileStatementBuilder WithCondition(ast_model.Expression value){
        _Condition = value;
        return this;
    }

    public WhileStatementBuilder WithBody(ast_model.BlockStatement value){
        _Body = value;
        return this;
    }

    public WhileStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public WhileStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public WhileStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}
public class WithScopeStatementBuilder : IBuilder<ast_model.WithScopeStatement>
{

    private ast_model.TypeMetadata _Type;
    private System.String _Name;
    private ast_model.AstThing _Parent;
    
    public ast_model.WithScopeStatement Build()
    {
        return new ast_model.WithScopeStatement(){
             Type = this._Type
           , Name = this._Name
           , Parent = this._Parent
        };
    }
    public WithScopeStatementBuilder WithType(ast_model.TypeMetadata value){
        _Type = value;
        return this;
    }

    public WithScopeStatementBuilder WithName(System.String value){
        _Name = value;
        return this;
    }

    public WithScopeStatementBuilder WithParent(ast_model.AstThing value){
        _Parent = value;
        return this;
    }

}

#nullable restore
