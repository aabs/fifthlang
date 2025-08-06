namespace ast_generated;
using ast;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterFunctionDef(FunctionDef ctx);
    public void LeaveFunctionDef(FunctionDef ctx);
    public void EnterFunctorDef(FunctorDef ctx);
    public void LeaveFunctorDef(FunctorDef ctx);
    public void EnterMethodDef(MethodDef ctx);
    public void LeaveMethodDef(MethodDef ctx);
    public void EnterInferenceRuleDef(InferenceRuleDef ctx);
    public void LeaveInferenceRuleDef(InferenceRuleDef ctx);
    public void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    public void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx);
    public void EnterExpStatement(ExpStatement ctx);
    public void LeaveExpStatement(ExpStatement ctx);
    public void EnterGuardStatement(GuardStatement ctx);
    public void LeaveGuardStatement(GuardStatement ctx);
    public void EnterAssertionStatement(AssertionStatement ctx);
    public void LeaveAssertionStatement(AssertionStatement ctx);
    public void EnterAssertionObject(AssertionObject ctx);
    public void LeaveAssertionObject(AssertionObject ctx);
    public void EnterAssertionPredicate(AssertionPredicate ctx);
    public void LeaveAssertionPredicate(AssertionPredicate ctx);
    public void EnterAssertionSubject(AssertionSubject ctx);
    public void LeaveAssertionSubject(AssertionSubject ctx);
    public void EnterLambdaExp(LambdaExp ctx);
    public void LeaveLambdaExp(LambdaExp ctx);
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
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterFunctionDef(FunctionDef ctx) { }
    public virtual void LeaveFunctionDef(FunctionDef ctx) { }
    public virtual void EnterFunctorDef(FunctorDef ctx) { }
    public virtual void LeaveFunctorDef(FunctorDef ctx) { }
    public virtual void EnterMethodDef(MethodDef ctx) { }
    public virtual void LeaveMethodDef(MethodDef ctx) { }
    public virtual void EnterInferenceRuleDef(InferenceRuleDef ctx) { }
    public virtual void LeaveInferenceRuleDef(InferenceRuleDef ctx) { }
    public virtual void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx) { }
    public virtual void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx) { }
    public virtual void EnterExpStatement(ExpStatement ctx) { }
    public virtual void LeaveExpStatement(ExpStatement ctx) { }
    public virtual void EnterGuardStatement(GuardStatement ctx) { }
    public virtual void LeaveGuardStatement(GuardStatement ctx) { }
    public virtual void EnterAssertionStatement(AssertionStatement ctx) { }
    public virtual void LeaveAssertionStatement(AssertionStatement ctx) { }
    public virtual void EnterAssertionObject(AssertionObject ctx) { }
    public virtual void LeaveAssertionObject(AssertionObject ctx) { }
    public virtual void EnterAssertionPredicate(AssertionPredicate ctx) { }
    public virtual void LeaveAssertionPredicate(AssertionPredicate ctx) { }
    public virtual void EnterAssertionSubject(AssertionSubject ctx) { }
    public virtual void LeaveAssertionSubject(AssertionSubject ctx) { }
    public virtual void EnterLambdaExp(LambdaExp ctx) { }
    public virtual void LeaveLambdaExp(LambdaExp ctx) { }
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
}
