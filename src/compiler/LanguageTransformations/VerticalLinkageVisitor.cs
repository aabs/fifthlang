// ReSharper disable InconsistentNaming

namespace compiler.LanguageTransformations;

public class VerticalLinkageVisitor : IAstVisitor
{
    private readonly Stack<AstThing> parents = new();

    public void EnterAssemblyDef(AssemblyDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterAssemblyRef(AssemblyRef ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterAssertionObject(AssertionObject ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterAssertionPredicate(AssertionPredicate ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterAssertionStatement(AssertionStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterAssertionSubject(AssertionSubject ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterAssignmentStatement(AssignmentStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterAtom(Atom ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterAtomLiteralExp(AtomLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterBinaryExp(BinaryExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterBlockStatement(BlockStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterCastExp(CastExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterCharLiteralExp(CharLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterClassDef(ClassDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterDateLiteralExp(DateLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterDurationLiteralExp(DurationLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterExpStatement(ExpStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterFieldDef(FieldDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterFloat16LiteralExp(Float16LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterFloat4LiteralExp(Float4LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterFloat8LiteralExp(Float8LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterForeachStatement(ForeachStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterForStatement(ForStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterFuncCallExp(FuncCallExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterFunctionDef(FunctionDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterFunctorDef(FunctorDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterGraph(Graph ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterGraphNamespaceAlias(GraphNamespaceAlias ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterGuardStatement(GuardStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterIfElseStatement(IfElseStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterInferenceRuleDef(InferenceRuleDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterInt16LiteralExp(Int16LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterInt32LiteralExp(Int32LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterInt64LiteralExp(Int64LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterInt8LiteralExp(Int8LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterLambdaExp(LambdaExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterListComprehension(ListComprehension ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterListLiteral(ListLiteral ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterMemberAccessExp(MemberAccessExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterMemberRef(MemberRef ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterMethodDef(MethodDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterModuleDef(ModuleDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterObjectInitializerExp(ObjectInitializerExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterParamDef(ParamDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterParamDestructureDef(ParamDestructureDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterPropertyBindingDef(PropertyBindingDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterPropertyDef(PropertyDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterPropertyInitializerExp(PropertyInitializerExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterPropertyRef(PropertyRef ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterRetractionStatement(RetractionStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterReturnStatement(ReturnStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterStringLiteralExp(StringLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterTimeLiteralExp(TimeLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterTriple(Triple ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterTypeDef(TypeDef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterTypeRef(TypeRef ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterUnaryExp(UnaryExp ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterUriLiteralExp(UriLiteralExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterVarDeclStatement(VarDeclStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterVariableDecl(VariableDecl ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterVarRef(VarRef ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterVarRefExp(VarRefExp ctx)
    {
        EnterTerminal(ctx);
    }

    public void EnterWhileStatement(WhileStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void EnterWithScopeStatement(WithScopeStatement ctx)
    {
        EnterNonTerminal(ctx);
    }

    public void LeaveAssemblyDef(AssemblyDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveAssemblyRef(AssemblyRef ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveAssertionObject(AssertionObject ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveAssertionPredicate(AssertionPredicate ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveAssertionStatement(AssertionStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveAssertionSubject(AssertionSubject ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveAssignmentStatement(AssignmentStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveAtom(Atom ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveAtomLiteralExp(AtomLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveBinaryExp(BinaryExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveBlockStatement(BlockStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveCastExp(CastExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveCharLiteralExp(CharLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveClassDef(ClassDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveDateLiteralExp(DateLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveDurationLiteralExp(DurationLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveExpStatement(ExpStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveFieldDef(FieldDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveFloat16LiteralExp(Float16LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveFloat4LiteralExp(Float4LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveFloat8LiteralExp(Float8LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveForeachStatement(ForeachStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveForStatement(ForStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveFuncCallExp(FuncCallExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveFunctionDef(FunctionDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveFunctorDef(FunctorDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveGraph(Graph ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveGraphNamespaceAlias(GraphNamespaceAlias ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveGuardStatement(GuardStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveIfElseStatement(IfElseStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveInferenceRuleDef(InferenceRuleDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveInt16LiteralExp(Int16LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveInt32LiteralExp(Int32LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveInt64LiteralExp(Int64LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveInt8LiteralExp(Int8LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveLambdaExp(LambdaExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveListComprehension(ListComprehension ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveListLiteral(ListLiteral ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveMemberAccessExp(MemberAccessExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveMemberRef(MemberRef ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveMethodDef(MethodDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveModuleDef(ModuleDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveObjectInitializerExp(ObjectInitializerExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveParamDef(ParamDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveParamDestructureDef(ParamDestructureDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeavePropertyBindingDef(PropertyBindingDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeavePropertyDef(PropertyDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeavePropertyInitializerExp(PropertyInitializerExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeavePropertyRef(PropertyRef ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveRetractionStatement(RetractionStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveReturnStatement(ReturnStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveStringLiteralExp(StringLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveTimeLiteralExp(TimeLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveTriple(Triple ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveTypeDef(TypeDef ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveTypeRef(TypeRef ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveUnaryExp(UnaryExp ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveUriLiteralExp(UriLiteralExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveVarDeclStatement(VarDeclStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveVariableDecl(VariableDecl ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveVarRef(VarRef ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveVarRefExp(VarRefExp ctx)
    {
        LeaveTerminal(ctx);
    }

    public void LeaveWhileStatement(WhileStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }

    public void LeaveWithScopeStatement(WithScopeStatement ctx)
    {
        LeaveNonTerminal(ctx);
    }


    #region Helpers
    private void EnterNonTerminal(AstThing ctx)
    {
        ctx.Parent = parents.PeekOrDefault();
        parents.Push(ctx);
    }

    private void EnterTerminal(AstThing ctx)
    {
        ctx.Parent = parents.PeekOrDefault();
    }

    private void LeaveNonTerminal(AstThing ctx)
    {
        parents.Pop();
    }

    private void LeaveTerminal(AstThing ctx)
    {
    }

    #endregion
}
