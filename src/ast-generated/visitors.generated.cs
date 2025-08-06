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
    public void EnterStructDef(StructDef ctx);
    public void LeaveStructDef(StructDef ctx);
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
    public void EnterTriple(Triple ctx);
    public void LeaveTriple(Triple ctx);
    public void EnterGraph(Graph ctx);
    public void LeaveGraph(Graph ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterAssemblyDef(AssemblyDef ctx) { }
    public virtual void LeaveAssemblyDef(AssemblyDef ctx) { }
    public virtual void EnterModuleDef(ModuleDef ctx) { }
    public virtual void LeaveModuleDef(ModuleDef ctx) { }
    public virtual void EnterFunctionDef(FunctionDef ctx) { }
    public virtual void LeaveFunctionDef(FunctionDef ctx) { }
    public virtual void EnterFunctorDef(FunctorDef ctx) { }
    public virtual void LeaveFunctorDef(FunctorDef ctx) { }
    public virtual void EnterFieldDef(FieldDef ctx) { }
    public virtual void LeaveFieldDef(FieldDef ctx) { }
    public virtual void EnterPropertyDef(PropertyDef ctx) { }
    public virtual void LeavePropertyDef(PropertyDef ctx) { }
    public virtual void EnterMethodDef(MethodDef ctx) { }
    public virtual void LeaveMethodDef(MethodDef ctx) { }
    public virtual void EnterOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx) { }
    public virtual void LeaveOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx) { }
    public virtual void EnterInferenceRuleDef(InferenceRuleDef ctx) { }
    public virtual void LeaveInferenceRuleDef(InferenceRuleDef ctx) { }
    public virtual void EnterParamDef(ParamDef ctx) { }
    public virtual void LeaveParamDef(ParamDef ctx) { }
    public virtual void EnterParamDestructureDef(ParamDestructureDef ctx) { }
    public virtual void LeaveParamDestructureDef(ParamDestructureDef ctx) { }
    public virtual void EnterPropertyBindingDef(PropertyBindingDef ctx) { }
    public virtual void LeavePropertyBindingDef(PropertyBindingDef ctx) { }
    public virtual void EnterTypeDef(TypeDef ctx) { }
    public virtual void LeaveTypeDef(TypeDef ctx) { }
    public virtual void EnterClassDef(ClassDef ctx) { }
    public virtual void LeaveClassDef(ClassDef ctx) { }
    public virtual void EnterStructDef(StructDef ctx) { }
    public virtual void LeaveStructDef(StructDef ctx) { }
    public virtual void EnterVariableDecl(VariableDecl ctx) { }
    public virtual void LeaveVariableDecl(VariableDecl ctx) { }
    public virtual void EnterAssemblyRef(AssemblyRef ctx) { }
    public virtual void LeaveAssemblyRef(AssemblyRef ctx) { }
    public virtual void EnterMemberRef(MemberRef ctx) { }
    public virtual void LeaveMemberRef(MemberRef ctx) { }
    public virtual void EnterPropertyRef(PropertyRef ctx) { }
    public virtual void LeavePropertyRef(PropertyRef ctx) { }
    public virtual void EnterTypeRef(TypeRef ctx) { }
    public virtual void LeaveTypeRef(TypeRef ctx) { }
    public virtual void EnterVarRef(VarRef ctx) { }
    public virtual void LeaveVarRef(VarRef ctx) { }
    public virtual void EnterGraphNamespaceAlias(GraphNamespaceAlias ctx) { }
    public virtual void LeaveGraphNamespaceAlias(GraphNamespaceAlias ctx) { }
    public virtual void EnterAssignmentStatement(AssignmentStatement ctx) { }
    public virtual void LeaveAssignmentStatement(AssignmentStatement ctx) { }
    public virtual void EnterBlockStatement(BlockStatement ctx) { }
    public virtual void LeaveBlockStatement(BlockStatement ctx) { }
    public virtual void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx) { }
    public virtual void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx) { }
    public virtual void EnterExpStatement(ExpStatement ctx) { }
    public virtual void LeaveExpStatement(ExpStatement ctx) { }
    public virtual void EnterForStatement(ForStatement ctx) { }
    public virtual void LeaveForStatement(ForStatement ctx) { }
    public virtual void EnterForeachStatement(ForeachStatement ctx) { }
    public virtual void LeaveForeachStatement(ForeachStatement ctx) { }
    public virtual void EnterGuardStatement(GuardStatement ctx) { }
    public virtual void LeaveGuardStatement(GuardStatement ctx) { }
    public virtual void EnterIfElseStatement(IfElseStatement ctx) { }
    public virtual void LeaveIfElseStatement(IfElseStatement ctx) { }
    public virtual void EnterReturnStatement(ReturnStatement ctx) { }
    public virtual void LeaveReturnStatement(ReturnStatement ctx) { }
    public virtual void EnterVarDeclStatement(VarDeclStatement ctx) { }
    public virtual void LeaveVarDeclStatement(VarDeclStatement ctx) { }
    public virtual void EnterWhileStatement(WhileStatement ctx) { }
    public virtual void LeaveWhileStatement(WhileStatement ctx) { }
    public virtual void EnterAssertionStatement(AssertionStatement ctx) { }
    public virtual void LeaveAssertionStatement(AssertionStatement ctx) { }
    public virtual void EnterAssertionObject(AssertionObject ctx) { }
    public virtual void LeaveAssertionObject(AssertionObject ctx) { }
    public virtual void EnterAssertionPredicate(AssertionPredicate ctx) { }
    public virtual void LeaveAssertionPredicate(AssertionPredicate ctx) { }
    public virtual void EnterAssertionSubject(AssertionSubject ctx) { }
    public virtual void LeaveAssertionSubject(AssertionSubject ctx) { }
    public virtual void EnterRetractionStatement(RetractionStatement ctx) { }
    public virtual void LeaveRetractionStatement(RetractionStatement ctx) { }
    public virtual void EnterWithScopeStatement(WithScopeStatement ctx) { }
    public virtual void LeaveWithScopeStatement(WithScopeStatement ctx) { }
    public virtual void EnterBinaryExp(BinaryExp ctx) { }
    public virtual void LeaveBinaryExp(BinaryExp ctx) { }
    public virtual void EnterCastExp(CastExp ctx) { }
    public virtual void LeaveCastExp(CastExp ctx) { }
    public virtual void EnterLambdaExp(LambdaExp ctx) { }
    public virtual void LeaveLambdaExp(LambdaExp ctx) { }
    public virtual void EnterFuncCallExp(FuncCallExp ctx) { }
    public virtual void LeaveFuncCallExp(FuncCallExp ctx) { }
    public virtual void EnterInt8LiteralExp(Int8LiteralExp ctx) { }
    public virtual void LeaveInt8LiteralExp(Int8LiteralExp ctx) { }
    public virtual void EnterInt16LiteralExp(Int16LiteralExp ctx) { }
    public virtual void LeaveInt16LiteralExp(Int16LiteralExp ctx) { }
    public virtual void EnterInt32LiteralExp(Int32LiteralExp ctx) { }
    public virtual void LeaveInt32LiteralExp(Int32LiteralExp ctx) { }
    public virtual void EnterInt64LiteralExp(Int64LiteralExp ctx) { }
    public virtual void LeaveInt64LiteralExp(Int64LiteralExp ctx) { }
    public virtual void EnterUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx) { }
    public virtual void LeaveUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx) { }
    public virtual void EnterUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx) { }
    public virtual void LeaveUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx) { }
    public virtual void EnterUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx) { }
    public virtual void LeaveUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx) { }
    public virtual void EnterUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx) { }
    public virtual void LeaveUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx) { }
    public virtual void EnterFloat4LiteralExp(Float4LiteralExp ctx) { }
    public virtual void LeaveFloat4LiteralExp(Float4LiteralExp ctx) { }
    public virtual void EnterFloat8LiteralExp(Float8LiteralExp ctx) { }
    public virtual void LeaveFloat8LiteralExp(Float8LiteralExp ctx) { }
    public virtual void EnterFloat16LiteralExp(Float16LiteralExp ctx) { }
    public virtual void LeaveFloat16LiteralExp(Float16LiteralExp ctx) { }
    public virtual void EnterBooleanLiteralExp(BooleanLiteralExp ctx) { }
    public virtual void LeaveBooleanLiteralExp(BooleanLiteralExp ctx) { }
    public virtual void EnterCharLiteralExp(CharLiteralExp ctx) { }
    public virtual void LeaveCharLiteralExp(CharLiteralExp ctx) { }
    public virtual void EnterStringLiteralExp(StringLiteralExp ctx) { }
    public virtual void LeaveStringLiteralExp(StringLiteralExp ctx) { }
    public virtual void EnterDateLiteralExp(DateLiteralExp ctx) { }
    public virtual void LeaveDateLiteralExp(DateLiteralExp ctx) { }
    public virtual void EnterTimeLiteralExp(TimeLiteralExp ctx) { }
    public virtual void LeaveTimeLiteralExp(TimeLiteralExp ctx) { }
    public virtual void EnterDateTimeLiteralExp(DateTimeLiteralExp ctx) { }
    public virtual void LeaveDateTimeLiteralExp(DateTimeLiteralExp ctx) { }
    public virtual void EnterDurationLiteralExp(DurationLiteralExp ctx) { }
    public virtual void LeaveDurationLiteralExp(DurationLiteralExp ctx) { }
    public virtual void EnterUriLiteralExp(UriLiteralExp ctx) { }
    public virtual void LeaveUriLiteralExp(UriLiteralExp ctx) { }
    public virtual void EnterAtomLiteralExp(AtomLiteralExp ctx) { }
    public virtual void LeaveAtomLiteralExp(AtomLiteralExp ctx) { }
    public virtual void EnterMemberAccessExp(MemberAccessExp ctx) { }
    public virtual void LeaveMemberAccessExp(MemberAccessExp ctx) { }
    public virtual void EnterObjectInitializerExp(ObjectInitializerExp ctx) { }
    public virtual void LeaveObjectInitializerExp(ObjectInitializerExp ctx) { }
    public virtual void EnterPropertyInitializerExp(PropertyInitializerExp ctx) { }
    public virtual void LeavePropertyInitializerExp(PropertyInitializerExp ctx) { }
    public virtual void EnterUnaryExp(UnaryExp ctx) { }
    public virtual void LeaveUnaryExp(UnaryExp ctx) { }
    public virtual void EnterVarRefExp(VarRefExp ctx) { }
    public virtual void LeaveVarRefExp(VarRefExp ctx) { }
    public virtual void EnterListLiteral(ListLiteral ctx) { }
    public virtual void LeaveListLiteral(ListLiteral ctx) { }
    public virtual void EnterListComprehension(ListComprehension ctx) { }
    public virtual void LeaveListComprehension(ListComprehension ctx) { }
    public virtual void EnterAtom(Atom ctx) { }
    public virtual void LeaveAtom(Atom ctx) { }
    public virtual void EnterTriple(Triple ctx) { }
    public virtual void LeaveTriple(Triple ctx) { }
    public virtual void EnterGraph(Graph ctx) { }
    public virtual void LeaveGraph(Graph ctx) { }
}
