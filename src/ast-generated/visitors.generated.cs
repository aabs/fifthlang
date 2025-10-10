namespace ast_generated;
using ast;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterAssemblyDef(AssemblyDef ctx);
    public void LeaveAssemblyDef(AssemblyDef ctx);
    public void EnterModuleDef(ModuleDef ctx);
    public void LeaveModuleDef(ModuleDef ctx);
    public void EnterFunctionDef(FunctionDef ctx);
    public void LeaveFunctionDef(FunctionDef ctx);
    public void EnterFunctorDef(FunctorDef ctx);
    public void LeaveFunctorDef(FunctorDef ctx);
    public void EnterFieldDef(FieldDef ctx);
    public void LeaveFieldDef(FieldDef ctx);
    public void EnterPropertyDef(PropertyDef ctx);
    public void LeavePropertyDef(PropertyDef ctx);
    public void EnterMethodDef(MethodDef ctx);
    public void LeaveMethodDef(MethodDef ctx);
    public void EnterOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx);
    public void LeaveOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx);
    public void EnterOverloadedFunctionDef(OverloadedFunctionDef ctx);
    public void LeaveOverloadedFunctionDef(OverloadedFunctionDef ctx);
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
    public void EnterPropertyRef(PropertyRef ctx);
    public void LeavePropertyRef(PropertyRef ctx);
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
    public void EnterGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx);
    public void LeaveGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx);
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
    public void EnterGraphAssertionBlockExp(GraphAssertionBlockExp ctx);
    public void LeaveGraphAssertionBlockExp(GraphAssertionBlockExp ctx);
    public void EnterBinaryExp(BinaryExp ctx);
    public void LeaveBinaryExp(BinaryExp ctx);
    public void EnterCastExp(CastExp ctx);
    public void LeaveCastExp(CastExp ctx);
    public void EnterLambdaExp(LambdaExp ctx);
    public void LeaveLambdaExp(LambdaExp ctx);
    public void EnterFuncCallExp(FuncCallExp ctx);
    public void LeaveFuncCallExp(FuncCallExp ctx);
    public void EnterInt8LiteralExp(Int8LiteralExp ctx);
    public void LeaveInt8LiteralExp(Int8LiteralExp ctx);
    public void EnterInt16LiteralExp(Int16LiteralExp ctx);
    public void LeaveInt16LiteralExp(Int16LiteralExp ctx);
    public void EnterInt32LiteralExp(Int32LiteralExp ctx);
    public void LeaveInt32LiteralExp(Int32LiteralExp ctx);
    public void EnterInt64LiteralExp(Int64LiteralExp ctx);
    public void LeaveInt64LiteralExp(Int64LiteralExp ctx);
    public void EnterUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx);
    public void LeaveUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx);
    public void EnterUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx);
    public void LeaveUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx);
    public void EnterUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx);
    public void LeaveUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx);
    public void EnterUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx);
    public void LeaveUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx);
    public void EnterFloat4LiteralExp(Float4LiteralExp ctx);
    public void LeaveFloat4LiteralExp(Float4LiteralExp ctx);
    public void EnterFloat8LiteralExp(Float8LiteralExp ctx);
    public void LeaveFloat8LiteralExp(Float8LiteralExp ctx);
    public void EnterFloat16LiteralExp(Float16LiteralExp ctx);
    public void LeaveFloat16LiteralExp(Float16LiteralExp ctx);
    public void EnterBooleanLiteralExp(BooleanLiteralExp ctx);
    public void LeaveBooleanLiteralExp(BooleanLiteralExp ctx);
    public void EnterCharLiteralExp(CharLiteralExp ctx);
    public void LeaveCharLiteralExp(CharLiteralExp ctx);
    public void EnterStringLiteralExp(StringLiteralExp ctx);
    public void LeaveStringLiteralExp(StringLiteralExp ctx);
    public void EnterDateLiteralExp(DateLiteralExp ctx);
    public void LeaveDateLiteralExp(DateLiteralExp ctx);
    public void EnterTimeLiteralExp(TimeLiteralExp ctx);
    public void LeaveTimeLiteralExp(TimeLiteralExp ctx);
    public void EnterDateTimeLiteralExp(DateTimeLiteralExp ctx);
    public void LeaveDateTimeLiteralExp(DateTimeLiteralExp ctx);
    public void EnterDurationLiteralExp(DurationLiteralExp ctx);
    public void LeaveDurationLiteralExp(DurationLiteralExp ctx);
    public void EnterUriLiteralExp(UriLiteralExp ctx);
    public void LeaveUriLiteralExp(UriLiteralExp ctx);
    public void EnterAtomLiteralExp(AtomLiteralExp ctx);
    public void LeaveAtomLiteralExp(AtomLiteralExp ctx);
    public void EnterMemberAccessExp(MemberAccessExp ctx);
    public void LeaveMemberAccessExp(MemberAccessExp ctx);
    public void EnterIndexerExpression(IndexerExpression ctx);
    public void LeaveIndexerExpression(IndexerExpression ctx);
    public void EnterObjectInitializerExp(ObjectInitializerExp ctx);
    public void LeaveObjectInitializerExp(ObjectInitializerExp ctx);
    public void EnterPropertyInitializerExp(PropertyInitializerExp ctx);
    public void LeavePropertyInitializerExp(PropertyInitializerExp ctx);
    public void EnterUnaryExp(UnaryExp ctx);
    public void LeaveUnaryExp(UnaryExp ctx);
    public void EnterVarRefExp(VarRefExp ctx);
    public void LeaveVarRefExp(VarRefExp ctx);
    public void EnterListLiteral(ListLiteral ctx);
    public void LeaveListLiteral(ListLiteral ctx);
    public void EnterListComprehension(ListComprehension ctx);
    public void LeaveListComprehension(ListComprehension ctx);
    public void EnterAtom(Atom ctx);
    public void LeaveAtom(Atom ctx);
    public void EnterTripleLiteralExp(TripleLiteralExp ctx);
    public void LeaveTripleLiteralExp(TripleLiteralExp ctx);
    public void EnterMalformedTripleExp(MalformedTripleExp ctx);
    public void LeaveMalformedTripleExp(MalformedTripleExp ctx);
    public void EnterGraph(Graph ctx);
    public void LeaveGraph(Graph ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterAssemblyDef(AssemblyDef ctx){}
    public virtual void LeaveAssemblyDef(AssemblyDef ctx){}
    public virtual void EnterModuleDef(ModuleDef ctx){}
    public virtual void LeaveModuleDef(ModuleDef ctx){}
    public virtual void EnterFunctionDef(FunctionDef ctx){}
    public virtual void LeaveFunctionDef(FunctionDef ctx){}
    public virtual void EnterFunctorDef(FunctorDef ctx){}
    public virtual void LeaveFunctorDef(FunctorDef ctx){}
    public virtual void EnterFieldDef(FieldDef ctx){}
    public virtual void LeaveFieldDef(FieldDef ctx){}
    public virtual void EnterPropertyDef(PropertyDef ctx){}
    public virtual void LeavePropertyDef(PropertyDef ctx){}
    public virtual void EnterMethodDef(MethodDef ctx){}
    public virtual void LeaveMethodDef(MethodDef ctx){}
    public virtual void EnterOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx){}
    public virtual void LeaveOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx){}
    public virtual void EnterOverloadedFunctionDef(OverloadedFunctionDef ctx){}
    public virtual void LeaveOverloadedFunctionDef(OverloadedFunctionDef ctx){}
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
    public virtual void EnterPropertyRef(PropertyRef ctx){}
    public virtual void LeavePropertyRef(PropertyRef ctx){}
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
    public virtual void EnterGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx){}
    public virtual void LeaveGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx){}
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
    public virtual void EnterGraphAssertionBlockExp(GraphAssertionBlockExp ctx){}
    public virtual void LeaveGraphAssertionBlockExp(GraphAssertionBlockExp ctx){}
    public virtual void EnterBinaryExp(BinaryExp ctx){}
    public virtual void LeaveBinaryExp(BinaryExp ctx){}
    public virtual void EnterCastExp(CastExp ctx){}
    public virtual void LeaveCastExp(CastExp ctx){}
    public virtual void EnterLambdaExp(LambdaExp ctx){}
    public virtual void LeaveLambdaExp(LambdaExp ctx){}
    public virtual void EnterFuncCallExp(FuncCallExp ctx){}
    public virtual void LeaveFuncCallExp(FuncCallExp ctx){}
    public virtual void EnterInt8LiteralExp(Int8LiteralExp ctx){}
    public virtual void LeaveInt8LiteralExp(Int8LiteralExp ctx){}
    public virtual void EnterInt16LiteralExp(Int16LiteralExp ctx){}
    public virtual void LeaveInt16LiteralExp(Int16LiteralExp ctx){}
    public virtual void EnterInt32LiteralExp(Int32LiteralExp ctx){}
    public virtual void LeaveInt32LiteralExp(Int32LiteralExp ctx){}
    public virtual void EnterInt64LiteralExp(Int64LiteralExp ctx){}
    public virtual void LeaveInt64LiteralExp(Int64LiteralExp ctx){}
    public virtual void EnterUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx){}
    public virtual void LeaveUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx){}
    public virtual void EnterUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx){}
    public virtual void LeaveUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx){}
    public virtual void EnterUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx){}
    public virtual void LeaveUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx){}
    public virtual void EnterUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx){}
    public virtual void LeaveUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx){}
    public virtual void EnterFloat4LiteralExp(Float4LiteralExp ctx){}
    public virtual void LeaveFloat4LiteralExp(Float4LiteralExp ctx){}
    public virtual void EnterFloat8LiteralExp(Float8LiteralExp ctx){}
    public virtual void LeaveFloat8LiteralExp(Float8LiteralExp ctx){}
    public virtual void EnterFloat16LiteralExp(Float16LiteralExp ctx){}
    public virtual void LeaveFloat16LiteralExp(Float16LiteralExp ctx){}
    public virtual void EnterBooleanLiteralExp(BooleanLiteralExp ctx){}
    public virtual void LeaveBooleanLiteralExp(BooleanLiteralExp ctx){}
    public virtual void EnterCharLiteralExp(CharLiteralExp ctx){}
    public virtual void LeaveCharLiteralExp(CharLiteralExp ctx){}
    public virtual void EnterStringLiteralExp(StringLiteralExp ctx){}
    public virtual void LeaveStringLiteralExp(StringLiteralExp ctx){}
    public virtual void EnterDateLiteralExp(DateLiteralExp ctx){}
    public virtual void LeaveDateLiteralExp(DateLiteralExp ctx){}
    public virtual void EnterTimeLiteralExp(TimeLiteralExp ctx){}
    public virtual void LeaveTimeLiteralExp(TimeLiteralExp ctx){}
    public virtual void EnterDateTimeLiteralExp(DateTimeLiteralExp ctx){}
    public virtual void LeaveDateTimeLiteralExp(DateTimeLiteralExp ctx){}
    public virtual void EnterDurationLiteralExp(DurationLiteralExp ctx){}
    public virtual void LeaveDurationLiteralExp(DurationLiteralExp ctx){}
    public virtual void EnterUriLiteralExp(UriLiteralExp ctx){}
    public virtual void LeaveUriLiteralExp(UriLiteralExp ctx){}
    public virtual void EnterAtomLiteralExp(AtomLiteralExp ctx){}
    public virtual void LeaveAtomLiteralExp(AtomLiteralExp ctx){}
    public virtual void EnterMemberAccessExp(MemberAccessExp ctx){}
    public virtual void LeaveMemberAccessExp(MemberAccessExp ctx){}
    public virtual void EnterIndexerExpression(IndexerExpression ctx){}
    public virtual void LeaveIndexerExpression(IndexerExpression ctx){}
    public virtual void EnterObjectInitializerExp(ObjectInitializerExp ctx){}
    public virtual void LeaveObjectInitializerExp(ObjectInitializerExp ctx){}
    public virtual void EnterPropertyInitializerExp(PropertyInitializerExp ctx){}
    public virtual void LeavePropertyInitializerExp(PropertyInitializerExp ctx){}
    public virtual void EnterUnaryExp(UnaryExp ctx){}
    public virtual void LeaveUnaryExp(UnaryExp ctx){}
    public virtual void EnterVarRefExp(VarRefExp ctx){}
    public virtual void LeaveVarRefExp(VarRefExp ctx){}
    public virtual void EnterListLiteral(ListLiteral ctx){}
    public virtual void LeaveListLiteral(ListLiteral ctx){}
    public virtual void EnterListComprehension(ListComprehension ctx){}
    public virtual void LeaveListComprehension(ListComprehension ctx){}
    public virtual void EnterAtom(Atom ctx){}
    public virtual void LeaveAtom(Atom ctx){}
    public virtual void EnterTripleLiteralExp(TripleLiteralExp ctx){}
    public virtual void LeaveTripleLiteralExp(TripleLiteralExp ctx){}
    public virtual void EnterMalformedTripleExp(MalformedTripleExp ctx){}
    public virtual void LeaveMalformedTripleExp(MalformedTripleExp ctx){}
    public virtual void EnterGraph(Graph ctx){}
    public virtual void LeaveGraph(Graph ctx){}
}


public interface IAstRecursiveDescentVisitor
{
    public AstThing Visit(AstThing ctx);
    public AssemblyDef VisitAssemblyDef(AssemblyDef ctx);
    public ModuleDef VisitModuleDef(ModuleDef ctx);
    public FunctionDef VisitFunctionDef(FunctionDef ctx);
    public FunctorDef VisitFunctorDef(FunctorDef ctx);
    public FieldDef VisitFieldDef(FieldDef ctx);
    public PropertyDef VisitPropertyDef(PropertyDef ctx);
    public MethodDef VisitMethodDef(MethodDef ctx);
    public OverloadedFunctionDefinition VisitOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx);
    public OverloadedFunctionDef VisitOverloadedFunctionDef(OverloadedFunctionDef ctx);
    public InferenceRuleDef VisitInferenceRuleDef(InferenceRuleDef ctx);
    public ParamDef VisitParamDef(ParamDef ctx);
    public ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx);
    public PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx);
    public TypeDef VisitTypeDef(TypeDef ctx);
    public ClassDef VisitClassDef(ClassDef ctx);
    public VariableDecl VisitVariableDecl(VariableDecl ctx);
    public AssemblyRef VisitAssemblyRef(AssemblyRef ctx);
    public MemberRef VisitMemberRef(MemberRef ctx);
    public PropertyRef VisitPropertyRef(PropertyRef ctx);
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
    public GraphAssertionBlockStatement VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx);
    public AssertionStatement VisitAssertionStatement(AssertionStatement ctx);
    public AssertionObject VisitAssertionObject(AssertionObject ctx);
    public AssertionPredicate VisitAssertionPredicate(AssertionPredicate ctx);
    public AssertionSubject VisitAssertionSubject(AssertionSubject ctx);
    public RetractionStatement VisitRetractionStatement(RetractionStatement ctx);
    public WithScopeStatement VisitWithScopeStatement(WithScopeStatement ctx);
    public GraphAssertionBlockExp VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx);
    public BinaryExp VisitBinaryExp(BinaryExp ctx);
    public CastExp VisitCastExp(CastExp ctx);
    public LambdaExp VisitLambdaExp(LambdaExp ctx);
    public FuncCallExp VisitFuncCallExp(FuncCallExp ctx);
    public Int8LiteralExp VisitInt8LiteralExp(Int8LiteralExp ctx);
    public Int16LiteralExp VisitInt16LiteralExp(Int16LiteralExp ctx);
    public Int32LiteralExp VisitInt32LiteralExp(Int32LiteralExp ctx);
    public Int64LiteralExp VisitInt64LiteralExp(Int64LiteralExp ctx);
    public UnsignedInt8LiteralExp VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx);
    public UnsignedInt16LiteralExp VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx);
    public UnsignedInt32LiteralExp VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx);
    public UnsignedInt64LiteralExp VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx);
    public Float4LiteralExp VisitFloat4LiteralExp(Float4LiteralExp ctx);
    public Float8LiteralExp VisitFloat8LiteralExp(Float8LiteralExp ctx);
    public Float16LiteralExp VisitFloat16LiteralExp(Float16LiteralExp ctx);
    public BooleanLiteralExp VisitBooleanLiteralExp(BooleanLiteralExp ctx);
    public CharLiteralExp VisitCharLiteralExp(CharLiteralExp ctx);
    public StringLiteralExp VisitStringLiteralExp(StringLiteralExp ctx);
    public DateLiteralExp VisitDateLiteralExp(DateLiteralExp ctx);
    public TimeLiteralExp VisitTimeLiteralExp(TimeLiteralExp ctx);
    public DateTimeLiteralExp VisitDateTimeLiteralExp(DateTimeLiteralExp ctx);
    public DurationLiteralExp VisitDurationLiteralExp(DurationLiteralExp ctx);
    public UriLiteralExp VisitUriLiteralExp(UriLiteralExp ctx);
    public AtomLiteralExp VisitAtomLiteralExp(AtomLiteralExp ctx);
    public MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx);
    public IndexerExpression VisitIndexerExpression(IndexerExpression ctx);
    public ObjectInitializerExp VisitObjectInitializerExp(ObjectInitializerExp ctx);
    public PropertyInitializerExp VisitPropertyInitializerExp(PropertyInitializerExp ctx);
    public UnaryExp VisitUnaryExp(UnaryExp ctx);
    public VarRefExp VisitVarRefExp(VarRefExp ctx);
    public ListLiteral VisitListLiteral(ListLiteral ctx);
    public ListComprehension VisitListComprehension(ListComprehension ctx);
    public Atom VisitAtom(Atom ctx);
    public TripleLiteralExp VisitTripleLiteralExp(TripleLiteralExp ctx);
    public MalformedTripleExp VisitMalformedTripleExp(MalformedTripleExp ctx);
    public Graph VisitGraph(Graph ctx);
}

public class DefaultRecursiveDescentVisitor : IAstRecursiveDescentVisitor
{
    public virtual AstThing Visit(AstThing ctx){
        if(ctx == null) return ctx;
        return ctx switch
        {
             AssemblyDef node => VisitAssemblyDef(node),
             ModuleDef node => VisitModuleDef(node),
             FunctionDef node => VisitFunctionDef(node),
             FunctorDef node => VisitFunctorDef(node),
             FieldDef node => VisitFieldDef(node),
             PropertyDef node => VisitPropertyDef(node),
             MethodDef node => VisitMethodDef(node),
             OverloadedFunctionDefinition node => VisitOverloadedFunctionDefinition(node),
             OverloadedFunctionDef node => VisitOverloadedFunctionDef(node),
             InferenceRuleDef node => VisitInferenceRuleDef(node),
             ParamDef node => VisitParamDef(node),
             ParamDestructureDef node => VisitParamDestructureDef(node),
             PropertyBindingDef node => VisitPropertyBindingDef(node),
             TypeDef node => VisitTypeDef(node),
             ClassDef node => VisitClassDef(node),
             VariableDecl node => VisitVariableDecl(node),
             AssemblyRef node => VisitAssemblyRef(node),
             MemberRef node => VisitMemberRef(node),
             PropertyRef node => VisitPropertyRef(node),
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
             GraphAssertionBlockStatement node => VisitGraphAssertionBlockStatement(node),
             AssertionStatement node => VisitAssertionStatement(node),
             AssertionObject node => VisitAssertionObject(node),
             AssertionPredicate node => VisitAssertionPredicate(node),
             AssertionSubject node => VisitAssertionSubject(node),
             RetractionStatement node => VisitRetractionStatement(node),
             WithScopeStatement node => VisitWithScopeStatement(node),
             GraphAssertionBlockExp node => VisitGraphAssertionBlockExp(node),
             BinaryExp node => VisitBinaryExp(node),
             CastExp node => VisitCastExp(node),
             LambdaExp node => VisitLambdaExp(node),
             FuncCallExp node => VisitFuncCallExp(node),
             Int8LiteralExp node => VisitInt8LiteralExp(node),
             Int16LiteralExp node => VisitInt16LiteralExp(node),
             Int32LiteralExp node => VisitInt32LiteralExp(node),
             Int64LiteralExp node => VisitInt64LiteralExp(node),
             UnsignedInt8LiteralExp node => VisitUnsignedInt8LiteralExp(node),
             UnsignedInt16LiteralExp node => VisitUnsignedInt16LiteralExp(node),
             UnsignedInt32LiteralExp node => VisitUnsignedInt32LiteralExp(node),
             UnsignedInt64LiteralExp node => VisitUnsignedInt64LiteralExp(node),
             Float4LiteralExp node => VisitFloat4LiteralExp(node),
             Float8LiteralExp node => VisitFloat8LiteralExp(node),
             Float16LiteralExp node => VisitFloat16LiteralExp(node),
             BooleanLiteralExp node => VisitBooleanLiteralExp(node),
             CharLiteralExp node => VisitCharLiteralExp(node),
             StringLiteralExp node => VisitStringLiteralExp(node),
             DateLiteralExp node => VisitDateLiteralExp(node),
             TimeLiteralExp node => VisitTimeLiteralExp(node),
             DateTimeLiteralExp node => VisitDateTimeLiteralExp(node),
             DurationLiteralExp node => VisitDurationLiteralExp(node),
             UriLiteralExp node => VisitUriLiteralExp(node),
             AtomLiteralExp node => VisitAtomLiteralExp(node),
             MemberAccessExp node => VisitMemberAccessExp(node),
             IndexerExpression node => VisitIndexerExpression(node),
             ObjectInitializerExp node => VisitObjectInitializerExp(node),
             PropertyInitializerExp node => VisitPropertyInitializerExp(node),
             UnaryExp node => VisitUnaryExp(node),
             VarRefExp node => VisitVarRefExp(node),
             ListLiteral node => VisitListLiteral(node),
             ListComprehension node => VisitListComprehension(node),
             Atom node => VisitAtom(node),
             TripleLiteralExp node => VisitTripleLiteralExp(node),
             MalformedTripleExp node => VisitMalformedTripleExp(node),
             Graph node => VisitGraph(node),

            { } node => null,
        };
    }

    public virtual AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        List<ast.AssemblyRef> tmpAssemblyRefs = [];
        tmpAssemblyRefs.AddRange(ctx.AssemblyRefs.Select(x => (ast.AssemblyRef)Visit(x)));
        List<ast.ModuleDef> tmpModules = [];
        tmpModules.AddRange(ctx.Modules.Select(x => (ast.ModuleDef)Visit(x)));
     return ctx with {
         AssemblyRefs = tmpAssemblyRefs
        ,Modules = tmpModules
        };
    }
    public virtual ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        List<ast.ClassDef> tmpClasses = [];
        tmpClasses.AddRange(ctx.Classes.Select(x => (ast.ClassDef)Visit(x)));
        List<ast.ScopedDefinition> tmpFunctions = [];
        tmpFunctions.AddRange(ctx.Functions.Select(x => (ast.ScopedDefinition)Visit(x)));
     return ctx with {
         Classes = tmpClasses
        ,Functions = tmpFunctions
        };
    }
    public virtual FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        List<ast.ParamDef> tmpParams = [];
        tmpParams.AddRange(ctx.Params.Select(x => (ast.ParamDef)Visit(x)));
     return ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)Visit((AstThing)ctx.Body)
        };
    }
    public virtual FunctorDef VisitFunctorDef(FunctorDef ctx)
    {
     return ctx with {
         InvocationFuncDev = (ast.FunctionDef)Visit((AstThing)ctx.InvocationFuncDev)
        };
    }
    public virtual FieldDef VisitFieldDef(FieldDef ctx)
    {
     return ctx with {
        };
    }
    public virtual PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
     return ctx with {
         BackingField = (ast.FieldDef)Visit((AstThing)ctx.BackingField)
        ,Getter = (ast.MethodDef)Visit((AstThing)ctx.Getter)
        ,Setter = (ast.MethodDef)Visit((AstThing)ctx.Setter)
        };
    }
    public virtual MethodDef VisitMethodDef(MethodDef ctx)
    {
     return ctx with {
         FunctionDef = (ast.FunctionDef)Visit((AstThing)ctx.FunctionDef)
        };
    }
    public virtual OverloadedFunctionDefinition VisitOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx)
    {
     return ctx with {
        };
    }
    public virtual OverloadedFunctionDef VisitOverloadedFunctionDef(OverloadedFunctionDef ctx)
    {
        List<ast.ParamDef> tmpParams = [];
        tmpParams.AddRange(ctx.Params.Select(x => (ast.ParamDef)Visit(x)));
     return ctx with {
         Params = tmpParams
        ,Body = (ast.BlockStatement)Visit((AstThing)ctx.Body)
        };
    }
    public virtual InferenceRuleDef VisitInferenceRuleDef(InferenceRuleDef ctx)
    {
     return ctx with {
         Antecedent = (ast.Expression)Visit((AstThing)ctx.Antecedent)
        ,Consequent = (ast.KnowledgeManagementBlock)Visit((AstThing)ctx.Consequent)
        };
    }
    public virtual ParamDef VisitParamDef(ParamDef ctx)
    {
     return ctx with {
         ParameterConstraint = (ast.Expression)Visit((AstThing)ctx.ParameterConstraint)
        ,DestructureDef = (ast.ParamDestructureDef)Visit((AstThing)ctx.DestructureDef)
        };
    }
    public virtual ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        List<ast.PropertyBindingDef> tmpBindings = [];
        tmpBindings.AddRange(ctx.Bindings.Select(x => (ast.PropertyBindingDef)Visit(x)));
     return ctx with {
         Bindings = tmpBindings
        };
    }
    public virtual PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
     return ctx with {
         ReferencedProperty = (ast.PropertyDef)Visit((AstThing)ctx.ReferencedProperty)
        ,DestructureDef = (ast.ParamDestructureDef)Visit((AstThing)ctx.DestructureDef)
        ,Constraint = (ast.Expression)Visit((AstThing)ctx.Constraint)
        };
    }
    public virtual TypeDef VisitTypeDef(TypeDef ctx)
    {
     return ctx with {
        };
    }
    public virtual ClassDef VisitClassDef(ClassDef ctx)
    {
        List<ast.MemberDef> tmpMemberDefs = [];
        tmpMemberDefs.AddRange(ctx.MemberDefs.Select(x => (ast.MemberDef)Visit(x)));
     return ctx with {
         MemberDefs = tmpMemberDefs
        };
    }
    public virtual VariableDecl VisitVariableDecl(VariableDecl ctx)
    {
     return ctx with {
        };
    }
    public virtual AssemblyRef VisitAssemblyRef(AssemblyRef ctx)
    {
     return ctx with {
        };
    }
    public virtual MemberRef VisitMemberRef(MemberRef ctx)
    {
     return ctx with {
         Member = (ast.MemberDef)Visit((AstThing)ctx.Member)
        };
    }
    public virtual PropertyRef VisitPropertyRef(PropertyRef ctx)
    {
     return ctx with {
         Property = (ast.PropertyDef)Visit((AstThing)ctx.Property)
        };
    }
    public virtual TypeRef VisitTypeRef(TypeRef ctx)
    {
     return ctx with {
        };
    }
    public virtual VarRef VisitVarRef(VarRef ctx)
    {
     return ctx with {
        };
    }
    public virtual GraphNamespaceAlias VisitGraphNamespaceAlias(GraphNamespaceAlias ctx)
    {
     return ctx with {
        };
    }
    public virtual AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
    {
     return ctx with {
         LValue = (ast.Expression)Visit((AstThing)ctx.LValue)
        ,RValue = (ast.Expression)Visit((AstThing)ctx.RValue)
        };
    }
    public virtual BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        List<ast.Statement> tmpStatements = [];
        tmpStatements.AddRange(ctx.Statements.Select(x => (ast.Statement)Visit(x)));
     return ctx with {
         Statements = tmpStatements
        };
    }
    public virtual KnowledgeManagementBlock VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
    {
        List<ast.KnowledgeManagementStatement> tmpStatements = [];
        tmpStatements.AddRange(ctx.Statements.Select(x => (ast.KnowledgeManagementStatement)Visit(x)));
     return ctx with {
         Statements = tmpStatements
        };
    }
    public virtual ExpStatement VisitExpStatement(ExpStatement ctx)
    {
     return ctx with {
         RHS = (ast.Expression)Visit((AstThing)ctx.RHS)
        };
    }
    public virtual ForStatement VisitForStatement(ForStatement ctx)
    {
     return ctx with {
         InitialValue = (ast.Expression)Visit((AstThing)ctx.InitialValue)
        ,Constraint = (ast.Expression)Visit((AstThing)ctx.Constraint)
        ,IncrementExpression = (ast.Expression)Visit((AstThing)ctx.IncrementExpression)
        ,LoopVariable = (ast.VariableDecl)Visit((AstThing)ctx.LoopVariable)
        ,Body = (ast.BlockStatement)Visit((AstThing)ctx.Body)
        };
    }
    public virtual ForeachStatement VisitForeachStatement(ForeachStatement ctx)
    {
     return ctx with {
         Collection = (ast.Expression)Visit((AstThing)ctx.Collection)
        ,LoopVariable = (ast.VariableDecl)Visit((AstThing)ctx.LoopVariable)
        ,Body = (ast.BlockStatement)Visit((AstThing)ctx.Body)
        };
    }
    public virtual GuardStatement VisitGuardStatement(GuardStatement ctx)
    {
     return ctx with {
         Condition = (ast.Expression)Visit((AstThing)ctx.Condition)
        };
    }
    public virtual IfElseStatement VisitIfElseStatement(IfElseStatement ctx)
    {
     return ctx with {
         Condition = (ast.Expression)Visit((AstThing)ctx.Condition)
        ,ThenBlock = (ast.BlockStatement)Visit((AstThing)ctx.ThenBlock)
        ,ElseBlock = (ast.BlockStatement)Visit((AstThing)ctx.ElseBlock)
        };
    }
    public virtual ReturnStatement VisitReturnStatement(ReturnStatement ctx)
    {
     return ctx with {
         ReturnValue = (ast.Expression)Visit((AstThing)ctx.ReturnValue)
        };
    }
    public virtual VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
    {
     return ctx with {
         VariableDecl = (ast.VariableDecl)Visit((AstThing)ctx.VariableDecl)
        ,InitialValue = (ast.Expression)Visit((AstThing)ctx.InitialValue)
        };
    }
    public virtual WhileStatement VisitWhileStatement(WhileStatement ctx)
    {
     return ctx with {
         Condition = (ast.Expression)Visit((AstThing)ctx.Condition)
        ,Body = (ast.BlockStatement)Visit((AstThing)ctx.Body)
        };
    }
    public virtual GraphAssertionBlockStatement VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx)
    {
     return ctx with {
         Content = (ast.GraphAssertionBlockExp)Visit((AstThing)ctx.Content)
        };
    }
    public virtual AssertionStatement VisitAssertionStatement(AssertionStatement ctx)
    {
     return ctx with {
         Assertion = (ast.TripleLiteralExp)Visit((AstThing)ctx.Assertion)
        ,AssertionSubject = (ast.AssertionSubject)Visit((AstThing)ctx.AssertionSubject)
        ,AssertionPredicate = (ast.AssertionPredicate)Visit((AstThing)ctx.AssertionPredicate)
        ,AssertionObject = (ast.AssertionObject)Visit((AstThing)ctx.AssertionObject)
        };
    }
    public virtual AssertionObject VisitAssertionObject(AssertionObject ctx)
    {
     return ctx with {
        };
    }
    public virtual AssertionPredicate VisitAssertionPredicate(AssertionPredicate ctx)
    {
     return ctx with {
        };
    }
    public virtual AssertionSubject VisitAssertionSubject(AssertionSubject ctx)
    {
     return ctx with {
        };
    }
    public virtual RetractionStatement VisitRetractionStatement(RetractionStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual WithScopeStatement VisitWithScopeStatement(WithScopeStatement ctx)
    {
     return ctx with {
        };
    }
    public virtual GraphAssertionBlockExp VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx)
    {
     return ctx with {
         Content = (ast.BlockStatement)Visit((AstThing)ctx.Content)
        };
    }
    public virtual BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
     return ctx with {
         LHS = (ast.Expression)Visit((AstThing)ctx.LHS)
        ,RHS = (ast.Expression)Visit((AstThing)ctx.RHS)
        };
    }
    public virtual CastExp VisitCastExp(CastExp ctx)
    {
     return ctx with {
        };
    }
    public virtual LambdaExp VisitLambdaExp(LambdaExp ctx)
    {
     return ctx with {
         FunctorDef = (ast.FunctorDef)Visit((AstThing)ctx.FunctorDef)
        };
    }
    public virtual FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        List<ast.Expression> tmpInvocationArguments = [];
        tmpInvocationArguments.AddRange(ctx.InvocationArguments.Select(x => (ast.Expression)Visit(x)));
     return ctx with {
         FunctionDef = (ast.FunctionDef)Visit((AstThing)ctx.FunctionDef)
        ,InvocationArguments = tmpInvocationArguments
        };
    }
    public virtual Int8LiteralExp VisitInt8LiteralExp(Int8LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Int16LiteralExp VisitInt16LiteralExp(Int16LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Int32LiteralExp VisitInt32LiteralExp(Int32LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Int64LiteralExp VisitInt64LiteralExp(Int64LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual UnsignedInt8LiteralExp VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual UnsignedInt16LiteralExp VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual UnsignedInt32LiteralExp VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual UnsignedInt64LiteralExp VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Float4LiteralExp VisitFloat4LiteralExp(Float4LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Float8LiteralExp VisitFloat8LiteralExp(Float8LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual Float16LiteralExp VisitFloat16LiteralExp(Float16LiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual BooleanLiteralExp VisitBooleanLiteralExp(BooleanLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual CharLiteralExp VisitCharLiteralExp(CharLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual StringLiteralExp VisitStringLiteralExp(StringLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual DateLiteralExp VisitDateLiteralExp(DateLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual TimeLiteralExp VisitTimeLiteralExp(TimeLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual DateTimeLiteralExp VisitDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual DurationLiteralExp VisitDurationLiteralExp(DurationLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual UriLiteralExp VisitUriLiteralExp(UriLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual AtomLiteralExp VisitAtomLiteralExp(AtomLiteralExp ctx)
    {
     return ctx with {
        };
    }
    public virtual MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
    {
     return ctx with {
         LHS = (ast.Expression)Visit((AstThing)ctx.LHS)
        ,RHS = (ast.Expression)Visit((AstThing)ctx.RHS)
        };
    }
    public virtual IndexerExpression VisitIndexerExpression(IndexerExpression ctx)
    {
     return ctx with {
         IndexExpression = (ast.Expression)Visit((AstThing)ctx.IndexExpression)
        ,OffsetExpression = (ast.Expression)Visit((AstThing)ctx.OffsetExpression)
        };
    }
    public virtual ObjectInitializerExp VisitObjectInitializerExp(ObjectInitializerExp ctx)
    {
        List<ast.PropertyInitializerExp> tmpPropertyInitialisers = [];
        tmpPropertyInitialisers.AddRange(ctx.PropertyInitialisers.Select(x => (ast.PropertyInitializerExp)Visit(x)));
     return ctx with {
         PropertyInitialisers = tmpPropertyInitialisers
        };
    }
    public virtual PropertyInitializerExp VisitPropertyInitializerExp(PropertyInitializerExp ctx)
    {
     return ctx with {
         PropertyToInitialize = (ast.PropertyRef)Visit((AstThing)ctx.PropertyToInitialize)
        ,RHS = (ast.Expression)Visit((AstThing)ctx.RHS)
        };
    }
    public virtual UnaryExp VisitUnaryExp(UnaryExp ctx)
    {
     return ctx with {
         Operand = (ast.Expression)Visit((AstThing)ctx.Operand)
        };
    }
    public virtual VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
     return ctx with {
         VariableDecl = (ast.VariableDecl)Visit((AstThing)ctx.VariableDecl)
        };
    }
    public virtual ListLiteral VisitListLiteral(ListLiteral ctx)
    {
        List<ast.Expression> tmpElementExpressions = [];
        tmpElementExpressions.AddRange(ctx.ElementExpressions.Select(x => (ast.Expression)Visit(x)));
     return ctx with {
         ElementExpressions = tmpElementExpressions
        };
    }
    public virtual ListComprehension VisitListComprehension(ListComprehension ctx)
    {
     return ctx with {
         MembershipConstraint = (ast.Expression)Visit((AstThing)ctx.MembershipConstraint)
        };
    }
    public virtual Atom VisitAtom(Atom ctx)
    {
     return ctx with {
         AtomExp = (ast.AtomLiteralExp)Visit((AstThing)ctx.AtomExp)
        };
    }
    public virtual TripleLiteralExp VisitTripleLiteralExp(TripleLiteralExp ctx)
    {
     return ctx with {
         SubjectExp = (ast.UriLiteralExp)Visit((AstThing)ctx.SubjectExp)
        ,PredicateExp = (ast.UriLiteralExp)Visit((AstThing)ctx.PredicateExp)
        ,ObjectExp = (ast.Expression)Visit((AstThing)ctx.ObjectExp)
        };
    }
    public virtual MalformedTripleExp VisitMalformedTripleExp(MalformedTripleExp ctx)
    {
        List<ast.Expression> tmpComponents = [];
        tmpComponents.AddRange(ctx.Components.Select(x => (ast.Expression)Visit(x)));
     return ctx with {
         Components = tmpComponents
        };
    }
    public virtual Graph VisitGraph(Graph ctx)
    {
        List<ast.TripleLiteralExp> tmpTriples = [];
        tmpTriples.AddRange(ctx.Triples.Select(x => (ast.TripleLiteralExp)Visit(x)));
     return ctx with {
         GraphUri = (ast.UriLiteralExp)Visit((AstThing)ctx.GraphUri)
        ,Triples = tmpTriples
        };
    }

}
