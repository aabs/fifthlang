

namespace ast_generated;

using ast_model;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterUserDefinedType(UserDefinedType ctx);
    public void LeaveUserDefinedType(UserDefinedType ctx);
    public void EnterAssemblyDef(AssemblyDef ctx);
    public void LeaveAssemblyDef(AssemblyDef ctx);
    public void EnterFunctionDef(FunctionDef ctx);
    public void LeaveFunctionDef(FunctionDef ctx);
    public void EnterFieldDef(FieldDef ctx);
    public void LeaveFieldDef(FieldDef ctx);
    public void EnterPropertyDef(PropertyDef ctx);
    public void LeavePropertyDef(PropertyDef ctx);
    public void EnterMethodDef(MethodDef ctx);
    public void LeaveMethodDef(MethodDef ctx);
    public void EnterInferenceRuleDef(InferenceRuleDef ctx);
    public void LeaveInferenceRuleDef(InferenceRuleDef ctx);
    public void EnterParamDef(ParamDef ctx);
    public void LeaveParamDef(ParamDef ctx);
    public void EnterParamDestructureDef(ParamDestructureDef ctx);
    public void LeaveParamDestructureDef(ParamDestructureDef ctx);
    public void EnterPropertyBindingDef(PropertyBindingDef ctx);
    public void LeavePropertyBindingDef(PropertyBindingDef ctx);
    public void EnterTypeDef(TypeDef ctx);
    public void LeaveTypeDef(TypeDef ctx);
    public void EnterClassDef(ClassDef ctx);
    public void LeaveClassDef(ClassDef ctx);
    public void EnterVariableDecl(VariableDecl ctx);
    public void LeaveVariableDecl(VariableDecl ctx);
    public void EnterAssemblyRef(AssemblyRef ctx);
    public void LeaveAssemblyRef(AssemblyRef ctx);
    public void EnterMemberRef(MemberRef ctx);
    public void LeaveMemberRef(MemberRef ctx);
    public void EnterTypeRef(TypeRef ctx);
    public void LeaveTypeRef(TypeRef ctx);
    public void EnterVarRef(VarRef ctx);
    public void LeaveVarRef(VarRef ctx);
    public void EnterGraphNamespaceAlias(GraphNamespaceAlias ctx);
    public void LeaveGraphNamespaceAlias(GraphNamespaceAlias ctx);
    public void EnterAssignmentStatement(AssignmentStatement ctx);
    public void LeaveAssignmentStatement(AssignmentStatement ctx);
    public void EnterBlockStatement(BlockStatement ctx);
    public void LeaveBlockStatement(BlockStatement ctx);
    public void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    public void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    public void EnterExpStatement(ExpStatement ctx);
    public void LeaveExpStatement(ExpStatement ctx);
    public void EnterForStatement(ForStatement ctx);
    public void LeaveForStatement(ForStatement ctx);
    public void EnterForeachStatement(ForeachStatement ctx);
    public void LeaveForeachStatement(ForeachStatement ctx);
    public void EnterGuardStatement(GuardStatement ctx);
    public void LeaveGuardStatement(GuardStatement ctx);
    public void EnterIfElseStatement(IfElseStatement ctx);
    public void LeaveIfElseStatement(IfElseStatement ctx);
    public void EnterReturnStatement(ReturnStatement ctx);
    public void LeaveReturnStatement(ReturnStatement ctx);
    public void EnterVarDeclStatement(VarDeclStatement ctx);
    public void LeaveVarDeclStatement(VarDeclStatement ctx);
    public void EnterWhileStatement(WhileStatement ctx);
    public void LeaveWhileStatement(WhileStatement ctx);
    public void EnterAssertionStatement(AssertionStatement ctx);
    public void LeaveAssertionStatement(AssertionStatement ctx);
    public void EnterAssertionObject(AssertionObject ctx);
    public void LeaveAssertionObject(AssertionObject ctx);
    public void EnterAssertionPredicate(AssertionPredicate ctx);
    public void LeaveAssertionPredicate(AssertionPredicate ctx);
    public void EnterAssertionSubject(AssertionSubject ctx);
    public void LeaveAssertionSubject(AssertionSubject ctx);
    public void EnterRetractionStatement(RetractionStatement ctx);
    public void LeaveRetractionStatement(RetractionStatement ctx);
    public void EnterWithScopeStatement(WithScopeStatement ctx);
    public void LeaveWithScopeStatement(WithScopeStatement ctx);
    public void EnterBinaryExp(BinaryExp ctx);
    public void LeaveBinaryExp(BinaryExp ctx);
    public void EnterCastExp(CastExp ctx);
    public void LeaveCastExp(CastExp ctx);
    public void EnterLambdaExp(LambdaExp ctx);
    public void LeaveLambdaExp(LambdaExp ctx);
    public void EnterFuncCallExp(FuncCallExp ctx);
    public void LeaveFuncCallExp(FuncCallExp ctx);
    public void EnterLiteralExp(LiteralExp ctx);
    public void LeaveLiteralExp(LiteralExp ctx);
    public void EnterMemberAccessExp(MemberAccessExp ctx);
    public void LeaveMemberAccessExp(MemberAccessExp ctx);
    public void EnterObjectInstantiationExp(ObjectInstantiationExp ctx);
    public void LeaveObjectInstantiationExp(ObjectInstantiationExp ctx);
    public void EnterUnaryExp(UnaryExp ctx);
    public void LeaveUnaryExp(UnaryExp ctx);
    public void EnterVarRefExp(VarRefExp ctx);
    public void LeaveVarRefExp(VarRefExp ctx);
    public void EnterList(List ctx);
    public void LeaveList(List ctx);
    public void EnterAtom(Atom ctx);
    public void LeaveAtom(Atom ctx);
    public void EnterTriple(Triple ctx);
    public void LeaveTriple(Triple ctx);
    public void EnterGraph(Graph ctx);
    public void LeaveGraph(Graph ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterUserDefinedType(UserDefinedType ctx){}
    public virtual void LeaveUserDefinedType(UserDefinedType ctx){}
    public virtual void EnterAssemblyDef(AssemblyDef ctx){}
    public virtual void LeaveAssemblyDef(AssemblyDef ctx){}
    public virtual void EnterFunctionDef(FunctionDef ctx){}
    public virtual void LeaveFunctionDef(FunctionDef ctx){}
    public virtual void EnterFieldDef(FieldDef ctx){}
    public virtual void LeaveFieldDef(FieldDef ctx){}
    public virtual void EnterPropertyDef(PropertyDef ctx){}
    public virtual void LeavePropertyDef(PropertyDef ctx){}
    public virtual void EnterMethodDef(MethodDef ctx){}
    public virtual void LeaveMethodDef(MethodDef ctx){}
    public virtual void EnterInferenceRuleDef(InferenceRuleDef ctx){}
    public virtual void LeaveInferenceRuleDef(InferenceRuleDef ctx){}
    public virtual void EnterParamDef(ParamDef ctx){}
    public virtual void LeaveParamDef(ParamDef ctx){}
    public virtual void EnterParamDestructureDef(ParamDestructureDef ctx){}
    public virtual void LeaveParamDestructureDef(ParamDestructureDef ctx){}
    public virtual void EnterPropertyBindingDef(PropertyBindingDef ctx){}
    public virtual void LeavePropertyBindingDef(PropertyBindingDef ctx){}
    public virtual void EnterTypeDef(TypeDef ctx){}
    public virtual void LeaveTypeDef(TypeDef ctx){}
    public virtual void EnterClassDef(ClassDef ctx){}
    public virtual void LeaveClassDef(ClassDef ctx){}
    public virtual void EnterVariableDecl(VariableDecl ctx){}
    public virtual void LeaveVariableDecl(VariableDecl ctx){}
    public virtual void EnterAssemblyRef(AssemblyRef ctx){}
    public virtual void LeaveAssemblyRef(AssemblyRef ctx){}
    public virtual void EnterMemberRef(MemberRef ctx){}
    public virtual void LeaveMemberRef(MemberRef ctx){}
    public virtual void EnterTypeRef(TypeRef ctx){}
    public virtual void LeaveTypeRef(TypeRef ctx){}
    public virtual void EnterVarRef(VarRef ctx){}
    public virtual void LeaveVarRef(VarRef ctx){}
    public virtual void EnterGraphNamespaceAlias(GraphNamespaceAlias ctx){}
    public virtual void LeaveGraphNamespaceAlias(GraphNamespaceAlias ctx){}
    public virtual void EnterAssignmentStatement(AssignmentStatement ctx){}
    public virtual void LeaveAssignmentStatement(AssignmentStatement ctx){}
    public virtual void EnterBlockStatement(BlockStatement ctx){}
    public virtual void LeaveBlockStatement(BlockStatement ctx){}
    public virtual void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx){}
    public virtual void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx){}
    public virtual void EnterExpStatement(ExpStatement ctx){}
    public virtual void LeaveExpStatement(ExpStatement ctx){}
    public virtual void EnterForStatement(ForStatement ctx){}
    public virtual void LeaveForStatement(ForStatement ctx){}
    public virtual void EnterForeachStatement(ForeachStatement ctx){}
    public virtual void LeaveForeachStatement(ForeachStatement ctx){}
    public virtual void EnterGuardStatement(GuardStatement ctx){}
    public virtual void LeaveGuardStatement(GuardStatement ctx){}
    public virtual void EnterIfElseStatement(IfElseStatement ctx){}
    public virtual void LeaveIfElseStatement(IfElseStatement ctx){}
    public virtual void EnterReturnStatement(ReturnStatement ctx){}
    public virtual void LeaveReturnStatement(ReturnStatement ctx){}
    public virtual void EnterVarDeclStatement(VarDeclStatement ctx){}
    public virtual void LeaveVarDeclStatement(VarDeclStatement ctx){}
    public virtual void EnterWhileStatement(WhileStatement ctx){}
    public virtual void LeaveWhileStatement(WhileStatement ctx){}
    public virtual void EnterAssertionStatement(AssertionStatement ctx){}
    public virtual void LeaveAssertionStatement(AssertionStatement ctx){}
    public virtual void EnterAssertionObject(AssertionObject ctx){}
    public virtual void LeaveAssertionObject(AssertionObject ctx){}
    public virtual void EnterAssertionPredicate(AssertionPredicate ctx){}
    public virtual void LeaveAssertionPredicate(AssertionPredicate ctx){}
    public virtual void EnterAssertionSubject(AssertionSubject ctx){}
    public virtual void LeaveAssertionSubject(AssertionSubject ctx){}
    public virtual void EnterRetractionStatement(RetractionStatement ctx){}
    public virtual void LeaveRetractionStatement(RetractionStatement ctx){}
    public virtual void EnterWithScopeStatement(WithScopeStatement ctx){}
    public virtual void LeaveWithScopeStatement(WithScopeStatement ctx){}
    public virtual void EnterBinaryExp(BinaryExp ctx){}
    public virtual void LeaveBinaryExp(BinaryExp ctx){}
    public virtual void EnterCastExp(CastExp ctx){}
    public virtual void LeaveCastExp(CastExp ctx){}
    public virtual void EnterLambdaExp(LambdaExp ctx){}
    public virtual void LeaveLambdaExp(LambdaExp ctx){}
    public virtual void EnterFuncCallExp(FuncCallExp ctx){}
    public virtual void LeaveFuncCallExp(FuncCallExp ctx){}
    public virtual void EnterLiteralExp(LiteralExp ctx){}
    public virtual void LeaveLiteralExp(LiteralExp ctx){}
    public virtual void EnterMemberAccessExp(MemberAccessExp ctx){}
    public virtual void LeaveMemberAccessExp(MemberAccessExp ctx){}
    public virtual void EnterObjectInstantiationExp(ObjectInstantiationExp ctx){}
    public virtual void LeaveObjectInstantiationExp(ObjectInstantiationExp ctx){}
    public virtual void EnterUnaryExp(UnaryExp ctx){}
    public virtual void LeaveUnaryExp(UnaryExp ctx){}
    public virtual void EnterVarRefExp(VarRefExp ctx){}
    public virtual void LeaveVarRefExp(VarRefExp ctx){}
    public virtual void EnterList(List ctx){}
    public virtual void LeaveList(List ctx){}
    public virtual void EnterAtom(Atom ctx){}
    public virtual void LeaveAtom(Atom ctx){}
    public virtual void EnterTriple(Triple ctx){}
    public virtual void LeaveTriple(Triple ctx){}
    public virtual void EnterGraph(Graph ctx){}
    public virtual void LeaveGraph(Graph ctx){}
}


public interface IAstRecursiveDescentVisitor
{
    public AstThing Visit(AstThing ctx);
    public UserDefinedType VisitUserDefinedType(UserDefinedType ctx);
    public AssemblyDef VisitAssemblyDef(AssemblyDef ctx);
    public FunctionDef VisitFunctionDef(FunctionDef ctx);
    public FieldDef VisitFieldDef(FieldDef ctx);
    public PropertyDef VisitPropertyDef(PropertyDef ctx);
    public MethodDef VisitMethodDef(MethodDef ctx);
    public InferenceRuleDef VisitInferenceRuleDef(InferenceRuleDef ctx);
    public ParamDef VisitParamDef(ParamDef ctx);
    public ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx);
    public PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx);
    public TypeDef VisitTypeDef(TypeDef ctx);
    public ClassDef VisitClassDef(ClassDef ctx);
    public VariableDecl VisitVariableDecl(VariableDecl ctx);
    public AssemblyRef VisitAssemblyRef(AssemblyRef ctx);
    public MemberRef VisitMemberRef(MemberRef ctx);
    public TypeRef VisitTypeRef(TypeRef ctx);
    public VarRef VisitVarRef(VarRef ctx);
    public GraphNamespaceAlias VisitGraphNamespaceAlias(GraphNamespaceAlias ctx);
    public AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx);
    public BlockStatement VisitBlockStatement(BlockStatement ctx);
    public KnowledgeManagementBlock VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    public ExpStatement VisitExpStatement(ExpStatement ctx);
    public ForStatement VisitForStatement(ForStatement ctx);
    public ForeachStatement VisitForeachStatement(ForeachStatement ctx);
    public GuardStatement VisitGuardStatement(GuardStatement ctx);
    public IfElseStatement VisitIfElseStatement(IfElseStatement ctx);
    public ReturnStatement VisitReturnStatement(ReturnStatement ctx);
    public VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx);
    public WhileStatement VisitWhileStatement(WhileStatement ctx);
    public AssertionStatement VisitAssertionStatement(AssertionStatement ctx);
    public AssertionObject VisitAssertionObject(AssertionObject ctx);
    public AssertionPredicate VisitAssertionPredicate(AssertionPredicate ctx);
    public AssertionSubject VisitAssertionSubject(AssertionSubject ctx);
    public RetractionStatement VisitRetractionStatement(RetractionStatement ctx);
    public WithScopeStatement VisitWithScopeStatement(WithScopeStatement ctx);
    public BinaryExp VisitBinaryExp(BinaryExp ctx);
    public CastExp VisitCastExp(CastExp ctx);
    public LambdaExp VisitLambdaExp(LambdaExp ctx);
    public FuncCallExp VisitFuncCallExp(FuncCallExp ctx);
    public LiteralExp VisitLiteralExp(LiteralExp ctx);
    public MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx);
    public ObjectInstantiationExp VisitObjectInstantiationExp(ObjectInstantiationExp ctx);
    public UnaryExp VisitUnaryExp(UnaryExp ctx);
    public VarRefExp VisitVarRefExp(VarRefExp ctx);
    public List VisitList(List ctx);
    public Atom VisitAtom(Atom ctx);
    public Triple VisitTriple(Triple ctx);
    public Graph VisitGraph(Graph ctx);
}

public class DefaultRecursiveDescentVisitor : IAstRecursiveDescentVisitor
{
    public virtual AstThing Visit(AstThing ctx){
        if(ctx == null) return ctx;
        return ctx switch
        {
             UserDefinedType node => VisitUserDefinedType(node),
             AssemblyDef node => VisitAssemblyDef(node),
             FunctionDef node => VisitFunctionDef(node),
             FieldDef node => VisitFieldDef(node),
             PropertyDef node => VisitPropertyDef(node),
             MethodDef node => VisitMethodDef(node),
             InferenceRuleDef node => VisitInferenceRuleDef(node),
             ParamDef node => VisitParamDef(node),
             ParamDestructureDef node => VisitParamDestructureDef(node),
             PropertyBindingDef node => VisitPropertyBindingDef(node),
             TypeDef node => VisitTypeDef(node),
             ClassDef node => VisitClassDef(node),
             VariableDecl node => VisitVariableDecl(node),
             AssemblyRef node => VisitAssemblyRef(node),
             MemberRef node => VisitMemberRef(node),
             TypeRef node => VisitTypeRef(node),
             VarRef node => VisitVarRef(node),
             GraphNamespaceAlias node => VisitGraphNamespaceAlias(node),
             AssignmentStatement node => VisitAssignmentStatement(node),
             BlockStatement node => VisitBlockStatement(node),
             KnowledgeManagementBlock node => VisitKnowledgeManagementBlock(node),
             ExpStatement node => VisitExpStatement(node),
             ForStatement node => VisitForStatement(node),
             ForeachStatement node => VisitForeachStatement(node),
             GuardStatement node => VisitGuardStatement(node),
             IfElseStatement node => VisitIfElseStatement(node),
             ReturnStatement node => VisitReturnStatement(node),
             VarDeclStatement node => VisitVarDeclStatement(node),
             WhileStatement node => VisitWhileStatement(node),
             AssertionStatement node => VisitAssertionStatement(node),
             AssertionObject node => VisitAssertionObject(node),
             AssertionPredicate node => VisitAssertionPredicate(node),
             AssertionSubject node => VisitAssertionSubject(node),
             RetractionStatement node => VisitRetractionStatement(node),
             WithScopeStatement node => VisitWithScopeStatement(node),
             BinaryExp node => VisitBinaryExp(node),
             CastExp node => VisitCastExp(node),
             LambdaExp node => VisitLambdaExp(node),
             FuncCallExp node => VisitFuncCallExp(node),
             LiteralExp node => VisitLiteralExp(node),
             MemberAccessExp node => VisitMemberAccessExp(node),
             ObjectInstantiationExp node => VisitObjectInstantiationExp(node),
             UnaryExp node => VisitUnaryExp(node),
             VarRefExp node => VisitVarRefExp(node),
             List node => VisitList(node),
             Atom node => VisitAtom(node),
             Triple node => VisitTriple(node),
             Graph node => VisitGraph(node),

            { } node => null,
        };
    }

    public virtual UserDefinedType VisitUserDefinedType(UserDefinedType ctx)
        => ctx;
    public virtual AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
        => ctx;
    public virtual FunctionDef VisitFunctionDef(FunctionDef ctx)
        => ctx;
    public virtual FieldDef VisitFieldDef(FieldDef ctx)
        => ctx;
    public virtual PropertyDef VisitPropertyDef(PropertyDef ctx)
        => ctx;
    public virtual MethodDef VisitMethodDef(MethodDef ctx)
        => ctx;
    public virtual InferenceRuleDef VisitInferenceRuleDef(InferenceRuleDef ctx)
        => ctx;
    public virtual ParamDef VisitParamDef(ParamDef ctx)
        => ctx;
    public virtual ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
        => ctx;
    public virtual PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
        => ctx;
    public virtual TypeDef VisitTypeDef(TypeDef ctx)
        => ctx;
    public virtual ClassDef VisitClassDef(ClassDef ctx)
        => ctx;
    public virtual VariableDecl VisitVariableDecl(VariableDecl ctx)
        => ctx;
    public virtual AssemblyRef VisitAssemblyRef(AssemblyRef ctx)
        => ctx;
    public virtual MemberRef VisitMemberRef(MemberRef ctx)
        => ctx;
    public virtual TypeRef VisitTypeRef(TypeRef ctx)
        => ctx;
    public virtual VarRef VisitVarRef(VarRef ctx)
        => ctx;
    public virtual GraphNamespaceAlias VisitGraphNamespaceAlias(GraphNamespaceAlias ctx)
        => ctx;
    public virtual AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
        => ctx;
    public virtual BlockStatement VisitBlockStatement(BlockStatement ctx)
        => ctx;
    public virtual KnowledgeManagementBlock VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
        => ctx;
    public virtual ExpStatement VisitExpStatement(ExpStatement ctx)
        => ctx;
    public virtual ForStatement VisitForStatement(ForStatement ctx)
        => ctx;
    public virtual ForeachStatement VisitForeachStatement(ForeachStatement ctx)
        => ctx;
    public virtual GuardStatement VisitGuardStatement(GuardStatement ctx)
        => ctx;
    public virtual IfElseStatement VisitIfElseStatement(IfElseStatement ctx)
        => ctx;
    public virtual ReturnStatement VisitReturnStatement(ReturnStatement ctx)
        => ctx;
    public virtual VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
        => ctx;
    public virtual WhileStatement VisitWhileStatement(WhileStatement ctx)
        => ctx;
    public virtual AssertionStatement VisitAssertionStatement(AssertionStatement ctx)
        => ctx;
    public virtual AssertionObject VisitAssertionObject(AssertionObject ctx)
        => ctx;
    public virtual AssertionPredicate VisitAssertionPredicate(AssertionPredicate ctx)
        => ctx;
    public virtual AssertionSubject VisitAssertionSubject(AssertionSubject ctx)
        => ctx;
    public virtual RetractionStatement VisitRetractionStatement(RetractionStatement ctx)
        => ctx;
    public virtual WithScopeStatement VisitWithScopeStatement(WithScopeStatement ctx)
        => ctx;
    public virtual BinaryExp VisitBinaryExp(BinaryExp ctx)
        => ctx;
    public virtual CastExp VisitCastExp(CastExp ctx)
        => ctx;
    public virtual LambdaExp VisitLambdaExp(LambdaExp ctx)
        => ctx;
    public virtual FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
        => ctx;
    public virtual LiteralExp VisitLiteralExp(LiteralExp ctx)
        => ctx;
    public virtual MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
        => ctx;
    public virtual ObjectInstantiationExp VisitObjectInstantiationExp(ObjectInstantiationExp ctx)
        => ctx;
    public virtual UnaryExp VisitUnaryExp(UnaryExp ctx)
        => ctx;
    public virtual VarRefExp VisitVarRefExp(VarRefExp ctx)
        => ctx;
    public virtual List VisitList(List ctx)
        => ctx;
    public virtual Atom VisitAtom(Atom ctx)
        => ctx;
    public virtual Triple VisitTriple(Triple ctx)
        => ctx;
    public virtual Graph VisitGraph(Graph ctx)
        => ctx;

}


