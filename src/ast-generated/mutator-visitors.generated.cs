

namespace ast_generated;

public interface IAstMutatorVisitor<TContext>
{
    UserDefinedType ProcessUserDefinedType(UserDefinedType node, TContext ctx);
    AssemblyDef ProcessAssemblyDef(AssemblyDef node, TContext ctx);
    FunctionDef ProcessFunctionDef(FunctionDef node, TContext ctx);
    FieldDef ProcessFieldDef(FieldDef node, TContext ctx);
    PropertyDef ProcessPropertyDef(PropertyDef node, TContext ctx);
    MethodDef ProcessMethodDef(MethodDef node, TContext ctx);
    InferenceRuleDef ProcessInferenceRuleDef(InferenceRuleDef node, TContext ctx);
    ParamDef ProcessParamDef(ParamDef node, TContext ctx);
    ParamDestructureDef ProcessParamDestructureDef(ParamDestructureDef node, TContext ctx);
    PropertyBindingDef ProcessPropertyBindingDef(PropertyBindingDef node, TContext ctx);
    TypeDef ProcessTypeDef(TypeDef node, TContext ctx);
    ClassDef ProcessClassDef(ClassDef node, TContext ctx);
    VariableDecl ProcessVariableDecl(VariableDecl node, TContext ctx);
    AssemblyRef ProcessAssemblyRef(AssemblyRef node, TContext ctx);
    MemberRef ProcessMemberRef(MemberRef node, TContext ctx);
    TypeRef ProcessTypeRef(TypeRef node, TContext ctx);
    VarRef ProcessVarRef(VarRef node, TContext ctx);
    GraphNamespaceAlias ProcessGraphNamespaceAlias(GraphNamespaceAlias node, TContext ctx);
    AssignmentStatement ProcessAssignmentStatement(AssignmentStatement node, TContext ctx);
    BlockStatement ProcessBlockStatement(BlockStatement node, TContext ctx);
    KnowledgeManagementBlock ProcessKnowledgeManagementBlock(KnowledgeManagementBlock node, TContext ctx);
    ExpStatement ProcessExpStatement(ExpStatement node, TContext ctx);
    ForStatement ProcessForStatement(ForStatement node, TContext ctx);
    ForeachStatement ProcessForeachStatement(ForeachStatement node, TContext ctx);
    GuardStatement ProcessGuardStatement(GuardStatement node, TContext ctx);
    IfElseStatement ProcessIfElseStatement(IfElseStatement node, TContext ctx);
    ReturnStatement ProcessReturnStatement(ReturnStatement node, TContext ctx);
    VarDeclStatement ProcessVarDeclStatement(VarDeclStatement node, TContext ctx);
    WhileStatement ProcessWhileStatement(WhileStatement node, TContext ctx);
    AssertionStatement ProcessAssertionStatement(AssertionStatement node, TContext ctx);
    AssertionObject ProcessAssertionObject(AssertionObject node, TContext ctx);
    AssertionPredicate ProcessAssertionPredicate(AssertionPredicate node, TContext ctx);
    AssertionSubject ProcessAssertionSubject(AssertionSubject node, TContext ctx);
    RetractionStatement ProcessRetractionStatement(RetractionStatement node, TContext ctx);
    WithScopeStatement ProcessWithScopeStatement(WithScopeStatement node, TContext ctx);
    BinaryExp ProcessBinaryExp(BinaryExp node, TContext ctx);
    CastExp ProcessCastExp(CastExp node, TContext ctx);
    LambdaExp ProcessLambdaExp(LambdaExp node, TContext ctx);
    FuncCallExp ProcessFuncCallExp(FuncCallExp node, TContext ctx);
    LiteralExp ProcessLiteralExp(LiteralExp node, TContext ctx);
    MemberAccessExp ProcessMemberAccessExp(MemberAccessExp node, TContext ctx);
    ObjectInstantiationExp ProcessObjectInstantiationExp(ObjectInstantiationExp node, TContext ctx);
    UnaryExp ProcessUnaryExp(UnaryExp node, TContext ctx);
    VarRefExp ProcessVarRefExp(VarRefExp node, TContext ctx);
    List ProcessList(List node, TContext ctx);
    Atom ProcessAtom(Atom node, TContext ctx);
    Triple ProcessTriple(Triple node, TContext ctx);
    Graph ProcessGraph(Graph node, TContext ctx);
}

public partial class NullMutatorVisitor<TContext> : IAstMutatorVisitor<TContext>
{
    public virtual UserDefinedType ProcessUserDefinedType(UserDefinedType node, TContext ctx)=>node;
    public virtual AssemblyDef ProcessAssemblyDef(AssemblyDef node, TContext ctx)=>node;
    public virtual FunctionDef ProcessFunctionDef(FunctionDef node, TContext ctx)=>node;
    public virtual FieldDef ProcessFieldDef(FieldDef node, TContext ctx)=>node;
    public virtual PropertyDef ProcessPropertyDef(PropertyDef node, TContext ctx)=>node;
    public virtual MethodDef ProcessMethodDef(MethodDef node, TContext ctx)=>node;
    public virtual InferenceRuleDef ProcessInferenceRuleDef(InferenceRuleDef node, TContext ctx)=>node;
    public virtual ParamDef ProcessParamDef(ParamDef node, TContext ctx)=>node;
    public virtual ParamDestructureDef ProcessParamDestructureDef(ParamDestructureDef node, TContext ctx)=>node;
    public virtual PropertyBindingDef ProcessPropertyBindingDef(PropertyBindingDef node, TContext ctx)=>node;
    public virtual TypeDef ProcessTypeDef(TypeDef node, TContext ctx)=>node;
    public virtual ClassDef ProcessClassDef(ClassDef node, TContext ctx)=>node;
    public virtual VariableDecl ProcessVariableDecl(VariableDecl node, TContext ctx)=>node;
    public virtual AssemblyRef ProcessAssemblyRef(AssemblyRef node, TContext ctx)=>node;
    public virtual MemberRef ProcessMemberRef(MemberRef node, TContext ctx)=>node;
    public virtual TypeRef ProcessTypeRef(TypeRef node, TContext ctx)=>node;
    public virtual VarRef ProcessVarRef(VarRef node, TContext ctx)=>node;
    public virtual GraphNamespaceAlias ProcessGraphNamespaceAlias(GraphNamespaceAlias node, TContext ctx)=>node;
    public virtual AssignmentStatement ProcessAssignmentStatement(AssignmentStatement node, TContext ctx)=>node;
    public virtual BlockStatement ProcessBlockStatement(BlockStatement node, TContext ctx)=>node;
    public virtual KnowledgeManagementBlock ProcessKnowledgeManagementBlock(KnowledgeManagementBlock node, TContext ctx)=>node;
    public virtual ExpStatement ProcessExpStatement(ExpStatement node, TContext ctx)=>node;
    public virtual ForStatement ProcessForStatement(ForStatement node, TContext ctx)=>node;
    public virtual ForeachStatement ProcessForeachStatement(ForeachStatement node, TContext ctx)=>node;
    public virtual GuardStatement ProcessGuardStatement(GuardStatement node, TContext ctx)=>node;
    public virtual IfElseStatement ProcessIfElseStatement(IfElseStatement node, TContext ctx)=>node;
    public virtual ReturnStatement ProcessReturnStatement(ReturnStatement node, TContext ctx)=>node;
    public virtual VarDeclStatement ProcessVarDeclStatement(VarDeclStatement node, TContext ctx)=>node;
    public virtual WhileStatement ProcessWhileStatement(WhileStatement node, TContext ctx)=>node;
    public virtual AssertionStatement ProcessAssertionStatement(AssertionStatement node, TContext ctx)=>node;
    public virtual AssertionObject ProcessAssertionObject(AssertionObject node, TContext ctx)=>node;
    public virtual AssertionPredicate ProcessAssertionPredicate(AssertionPredicate node, TContext ctx)=>node;
    public virtual AssertionSubject ProcessAssertionSubject(AssertionSubject node, TContext ctx)=>node;
    public virtual RetractionStatement ProcessRetractionStatement(RetractionStatement node, TContext ctx)=>node;
    public virtual WithScopeStatement ProcessWithScopeStatement(WithScopeStatement node, TContext ctx)=>node;
    public virtual BinaryExp ProcessBinaryExp(BinaryExp node, TContext ctx)=>node;
    public virtual CastExp ProcessCastExp(CastExp node, TContext ctx)=>node;
    public virtual LambdaExp ProcessLambdaExp(LambdaExp node, TContext ctx)=>node;
    public virtual FuncCallExp ProcessFuncCallExp(FuncCallExp node, TContext ctx)=>node;
    public virtual LiteralExp ProcessLiteralExp(LiteralExp node, TContext ctx)=>node;
    public virtual MemberAccessExp ProcessMemberAccessExp(MemberAccessExp node, TContext ctx)=>node;
    public virtual ObjectInstantiationExp ProcessObjectInstantiationExp(ObjectInstantiationExp node, TContext ctx)=>node;
    public virtual UnaryExp ProcessUnaryExp(UnaryExp node, TContext ctx)=>node;
    public virtual VarRefExp ProcessVarRefExp(VarRefExp node, TContext ctx)=>node;
    public virtual List ProcessList(List node, TContext ctx)=>node;
    public virtual Atom ProcessAtom(Atom node, TContext ctx)=>node;
    public virtual Triple ProcessTriple(Triple node, TContext ctx)=>node;
    public virtual Graph ProcessGraph(Graph node, TContext ctx)=>node;

}

public partial class DefaultMutatorVisitor<TContext> : IAstMutatorVisitor<TContext>
{
    public AstThing Process(AstThing x, TContext ctx)
    {
        if (x == null) return x;
        AstThing result = x switch
        {
            UserDefinedType node => ProcessUserDefinedType(node, ctx),
            AssemblyDef node => ProcessAssemblyDef(node, ctx),
            FunctionDef node => ProcessFunctionDef(node, ctx),
            FieldDef node => ProcessFieldDef(node, ctx),
            PropertyDef node => ProcessPropertyDef(node, ctx),
            MethodDef node => ProcessMethodDef(node, ctx),
            InferenceRuleDef node => ProcessInferenceRuleDef(node, ctx),
            ParamDef node => ProcessParamDef(node, ctx),
            ParamDestructureDef node => ProcessParamDestructureDef(node, ctx),
            PropertyBindingDef node => ProcessPropertyBindingDef(node, ctx),
            TypeDef node => ProcessTypeDef(node, ctx),
            ClassDef node => ProcessClassDef(node, ctx),
            VariableDecl node => ProcessVariableDecl(node, ctx),
            AssemblyRef node => ProcessAssemblyRef(node, ctx),
            MemberRef node => ProcessMemberRef(node, ctx),
            TypeRef node => ProcessTypeRef(node, ctx),
            VarRef node => ProcessVarRef(node, ctx),
            GraphNamespaceAlias node => ProcessGraphNamespaceAlias(node, ctx),
            AssignmentStatement node => ProcessAssignmentStatement(node, ctx),
            BlockStatement node => ProcessBlockStatement(node, ctx),
            KnowledgeManagementBlock node => ProcessKnowledgeManagementBlock(node, ctx),
            ExpStatement node => ProcessExpStatement(node, ctx),
            ForStatement node => ProcessForStatement(node, ctx),
            ForeachStatement node => ProcessForeachStatement(node, ctx),
            GuardStatement node => ProcessGuardStatement(node, ctx),
            IfElseStatement node => ProcessIfElseStatement(node, ctx),
            ReturnStatement node => ProcessReturnStatement(node, ctx),
            VarDeclStatement node => ProcessVarDeclStatement(node, ctx),
            WhileStatement node => ProcessWhileStatement(node, ctx),
            AssertionStatement node => ProcessAssertionStatement(node, ctx),
            AssertionObject node => ProcessAssertionObject(node, ctx),
            AssertionPredicate node => ProcessAssertionPredicate(node, ctx),
            AssertionSubject node => ProcessAssertionSubject(node, ctx),
            RetractionStatement node => ProcessRetractionStatement(node, ctx),
            WithScopeStatement node => ProcessWithScopeStatement(node, ctx),
            BinaryExp node => ProcessBinaryExp(node, ctx),
            CastExp node => ProcessCastExp(node, ctx),
            LambdaExp node => ProcessLambdaExp(node, ctx),
            FuncCallExp node => ProcessFuncCallExp(node, ctx),
            LiteralExp node => ProcessLiteralExp(node, ctx),
            MemberAccessExp node => ProcessMemberAccessExp(node, ctx),
            ObjectInstantiationExp node => ProcessObjectInstantiationExp(node, ctx),
            UnaryExp node => ProcessUnaryExp(node, ctx),
            VarRefExp node => ProcessVarRefExp(node, ctx),
            List node => ProcessList(node, ctx),
            Atom node => ProcessAtom(node, ctx),
            Triple node => ProcessTriple(node, ctx),
            Graph node => ProcessGraph(node, ctx),

            { } node => node,
        };

        // in case result is totally new, copy in the metadata, so we don't lose any that's been generated previously
        return result with {SourceContext = x.SourceContext, Type = x.Type};
    }


    public virtual UserDefinedType ProcessUserDefinedType(UserDefinedType node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssemblyDef ProcessAssemblyDef(AssemblyDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FunctionDef ProcessFunctionDef(FunctionDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FieldDef ProcessFieldDef(FieldDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual PropertyDef ProcessPropertyDef(PropertyDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MethodDef ProcessMethodDef(MethodDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual InferenceRuleDef ProcessInferenceRuleDef(InferenceRuleDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ParamDef ProcessParamDef(ParamDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ParamDestructureDef ProcessParamDestructureDef(ParamDestructureDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual PropertyBindingDef ProcessPropertyBindingDef(PropertyBindingDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual TypeDef ProcessTypeDef(TypeDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ClassDef ProcessClassDef(ClassDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VariableDecl ProcessVariableDecl(VariableDecl node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssemblyRef ProcessAssemblyRef(AssemblyRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MemberRef ProcessMemberRef(MemberRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual TypeRef ProcessTypeRef(TypeRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarRef ProcessVarRef(VarRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual GraphNamespaceAlias ProcessGraphNamespaceAlias(GraphNamespaceAlias node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssignmentStatement ProcessAssignmentStatement(AssignmentStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual BlockStatement ProcessBlockStatement(BlockStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual KnowledgeManagementBlock ProcessKnowledgeManagementBlock(KnowledgeManagementBlock node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ExpStatement ProcessExpStatement(ExpStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ForStatement ProcessForStatement(ForStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ForeachStatement ProcessForeachStatement(ForeachStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual GuardStatement ProcessGuardStatement(GuardStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual IfElseStatement ProcessIfElseStatement(IfElseStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ReturnStatement ProcessReturnStatement(ReturnStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarDeclStatement ProcessVarDeclStatement(VarDeclStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual WhileStatement ProcessWhileStatement(WhileStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionStatement ProcessAssertionStatement(AssertionStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionObject ProcessAssertionObject(AssertionObject node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionPredicate ProcessAssertionPredicate(AssertionPredicate node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionSubject ProcessAssertionSubject(AssertionSubject node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual RetractionStatement ProcessRetractionStatement(RetractionStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual WithScopeStatement ProcessWithScopeStatement(WithScopeStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual BinaryExp ProcessBinaryExp(BinaryExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual CastExp ProcessCastExp(CastExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual LambdaExp ProcessLambdaExp(LambdaExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FuncCallExp ProcessFuncCallExp(FuncCallExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual LiteralExp ProcessLiteralExp(LiteralExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MemberAccessExp ProcessMemberAccessExp(MemberAccessExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ObjectInstantiationExp ProcessObjectInstantiationExp(ObjectInstantiationExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual UnaryExp ProcessUnaryExp(UnaryExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarRefExp ProcessVarRefExp(VarRefExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual List ProcessList(List node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Atom ProcessAtom(Atom node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Triple ProcessTriple(Triple node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Graph ProcessGraph(Graph node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

} // end class DefaultMutatorVisitor<TContext>


public interface IAstMutatorVisitor2<TContext>
{
    UserDefinedType ProcessUserDefinedType(UserDefinedType node, TContext ctx);
    AssemblyDef ProcessAssemblyDef(AssemblyDef node, TContext ctx);
    FunctionDef ProcessFunctionDef(FunctionDef node, TContext ctx);
    FieldDef ProcessFieldDef(FieldDef node, TContext ctx);
    PropertyDef ProcessPropertyDef(PropertyDef node, TContext ctx);
    MethodDef ProcessMethodDef(MethodDef node, TContext ctx);
    InferenceRuleDef ProcessInferenceRuleDef(InferenceRuleDef node, TContext ctx);
    ParamDef ProcessParamDef(ParamDef node, TContext ctx);
    ParamDestructureDef ProcessParamDestructureDef(ParamDestructureDef node, TContext ctx);
    PropertyBindingDef ProcessPropertyBindingDef(PropertyBindingDef node, TContext ctx);
    TypeDef ProcessTypeDef(TypeDef node, TContext ctx);
    ClassDef ProcessClassDef(ClassDef node, TContext ctx);
    VariableDecl ProcessVariableDecl(VariableDecl node, TContext ctx);
    AssemblyRef ProcessAssemblyRef(AssemblyRef node, TContext ctx);
    MemberRef ProcessMemberRef(MemberRef node, TContext ctx);
    TypeRef ProcessTypeRef(TypeRef node, TContext ctx);
    VarRef ProcessVarRef(VarRef node, TContext ctx);
    GraphNamespaceAlias ProcessGraphNamespaceAlias(GraphNamespaceAlias node, TContext ctx);
    AssignmentStatement ProcessAssignmentStatement(AssignmentStatement node, TContext ctx);
    BlockStatement ProcessBlockStatement(BlockStatement node, TContext ctx);
    KnowledgeManagementBlock ProcessKnowledgeManagementBlock(KnowledgeManagementBlock node, TContext ctx);
    ExpStatement ProcessExpStatement(ExpStatement node, TContext ctx);
    ForStatement ProcessForStatement(ForStatement node, TContext ctx);
    ForeachStatement ProcessForeachStatement(ForeachStatement node, TContext ctx);
    GuardStatement ProcessGuardStatement(GuardStatement node, TContext ctx);
    IfElseStatement ProcessIfElseStatement(IfElseStatement node, TContext ctx);
    ReturnStatement ProcessReturnStatement(ReturnStatement node, TContext ctx);
    VarDeclStatement ProcessVarDeclStatement(VarDeclStatement node, TContext ctx);
    WhileStatement ProcessWhileStatement(WhileStatement node, TContext ctx);
    AssertionStatement ProcessAssertionStatement(AssertionStatement node, TContext ctx);
    AssertionObject ProcessAssertionObject(AssertionObject node, TContext ctx);
    AssertionPredicate ProcessAssertionPredicate(AssertionPredicate node, TContext ctx);
    AssertionSubject ProcessAssertionSubject(AssertionSubject node, TContext ctx);
    RetractionStatement ProcessRetractionStatement(RetractionStatement node, TContext ctx);
    WithScopeStatement ProcessWithScopeStatement(WithScopeStatement node, TContext ctx);
    BinaryExp ProcessBinaryExp(BinaryExp node, TContext ctx);
    CastExp ProcessCastExp(CastExp node, TContext ctx);
    LambdaExp ProcessLambdaExp(LambdaExp node, TContext ctx);
    FuncCallExp ProcessFuncCallExp(FuncCallExp node, TContext ctx);
    LiteralExp ProcessLiteralExp(LiteralExp node, TContext ctx);
    MemberAccessExp ProcessMemberAccessExp(MemberAccessExp node, TContext ctx);
    ObjectInstantiationExp ProcessObjectInstantiationExp(ObjectInstantiationExp node, TContext ctx);
    UnaryExp ProcessUnaryExp(UnaryExp node, TContext ctx);
    VarRefExp ProcessVarRefExp(VarRefExp node, TContext ctx);
    List ProcessList(List node, TContext ctx);
    Atom ProcessAtom(Atom node, TContext ctx);
    Triple ProcessTriple(Triple node, TContext ctx);
    Graph ProcessGraph(Graph node, TContext ctx);
}

public partial class DefaultMutatorVisitor2<TContext> : IAstMutatorVisitor2<TContext>
{
    public AstThing Process(AstThing x, TContext ctx)
    {
        if (x == null) return x;
        var result = x switch
        {
            UserDefinedType node => ProcessUserDefinedType(node, ctx),
            AssemblyDef node => ProcessAssemblyDef(node, ctx),
            FunctionDef node => ProcessFunctionDef(node, ctx),
            FieldDef node => ProcessFieldDef(node, ctx),
            PropertyDef node => ProcessPropertyDef(node, ctx),
            MethodDef node => ProcessMethodDef(node, ctx),
            InferenceRuleDef node => ProcessInferenceRuleDef(node, ctx),
            ParamDef node => ProcessParamDef(node, ctx),
            ParamDestructureDef node => ProcessParamDestructureDef(node, ctx),
            PropertyBindingDef node => ProcessPropertyBindingDef(node, ctx),
            TypeDef node => ProcessTypeDef(node, ctx),
            ClassDef node => ProcessClassDef(node, ctx),
            VariableDecl node => ProcessVariableDecl(node, ctx),
            AssemblyRef node => ProcessAssemblyRef(node, ctx),
            MemberRef node => ProcessMemberRef(node, ctx),
            TypeRef node => ProcessTypeRef(node, ctx),
            VarRef node => ProcessVarRef(node, ctx),
            GraphNamespaceAlias node => ProcessGraphNamespaceAlias(node, ctx),
            AssignmentStatement node => ProcessAssignmentStatement(node, ctx),
            BlockStatement node => ProcessBlockStatement(node, ctx),
            KnowledgeManagementBlock node => ProcessKnowledgeManagementBlock(node, ctx),
            ExpStatement node => ProcessExpStatement(node, ctx),
            ForStatement node => ProcessForStatement(node, ctx),
            ForeachStatement node => ProcessForeachStatement(node, ctx),
            GuardStatement node => ProcessGuardStatement(node, ctx),
            IfElseStatement node => ProcessIfElseStatement(node, ctx),
            ReturnStatement node => ProcessReturnStatement(node, ctx),
            VarDeclStatement node => ProcessVarDeclStatement(node, ctx),
            WhileStatement node => ProcessWhileStatement(node, ctx),
            AssertionStatement node => ProcessAssertionStatement(node, ctx),
            AssertionObject node => ProcessAssertionObject(node, ctx),
            AssertionPredicate node => ProcessAssertionPredicate(node, ctx),
            AssertionSubject node => ProcessAssertionSubject(node, ctx),
            RetractionStatement node => ProcessRetractionStatement(node, ctx),
            WithScopeStatement node => ProcessWithScopeStatement(node, ctx),
            BinaryExp node => ProcessBinaryExp(node, ctx),
            CastExp node => ProcessCastExp(node, ctx),
            LambdaExp node => ProcessLambdaExp(node, ctx),
            FuncCallExp node => ProcessFuncCallExp(node, ctx),
            LiteralExp node => ProcessLiteralExp(node, ctx),
            MemberAccessExp node => ProcessMemberAccessExp(node, ctx),
            ObjectInstantiationExp node => ProcessObjectInstantiationExp(node, ctx),
            UnaryExp node => ProcessUnaryExp(node, ctx),
            VarRefExp node => ProcessVarRefExp(node, ctx),
            List node => ProcessList(node, ctx),
            Atom node => ProcessAtom(node, ctx),
            Triple node => ProcessTriple(node, ctx),
            Graph node => ProcessGraph(node, ctx),

            { } node => node,
        };

        // in case result is totally new, copy in the metadata, so we don't lose any that's been generated previously
        return result with {SourceContext = x.SourceContext, Type = x.Type};
    }


    public virtual UserDefinedType ProcessUserDefinedType(UserDefinedType node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssemblyDef ProcessAssemblyDef(AssemblyDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FunctionDef ProcessFunctionDef(FunctionDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FieldDef ProcessFieldDef(FieldDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual PropertyDef ProcessPropertyDef(PropertyDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MethodDef ProcessMethodDef(MethodDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual InferenceRuleDef ProcessInferenceRuleDef(InferenceRuleDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ParamDef ProcessParamDef(ParamDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ParamDestructureDef ProcessParamDestructureDef(ParamDestructureDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual PropertyBindingDef ProcessPropertyBindingDef(PropertyBindingDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual TypeDef ProcessTypeDef(TypeDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ClassDef ProcessClassDef(ClassDef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VariableDecl ProcessVariableDecl(VariableDecl node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssemblyRef ProcessAssemblyRef(AssemblyRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MemberRef ProcessMemberRef(MemberRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual TypeRef ProcessTypeRef(TypeRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarRef ProcessVarRef(VarRef node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual GraphNamespaceAlias ProcessGraphNamespaceAlias(GraphNamespaceAlias node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssignmentStatement ProcessAssignmentStatement(AssignmentStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual BlockStatement ProcessBlockStatement(BlockStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual KnowledgeManagementBlock ProcessKnowledgeManagementBlock(KnowledgeManagementBlock node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ExpStatement ProcessExpStatement(ExpStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ForStatement ProcessForStatement(ForStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ForeachStatement ProcessForeachStatement(ForeachStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual GuardStatement ProcessGuardStatement(GuardStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual IfElseStatement ProcessIfElseStatement(IfElseStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ReturnStatement ProcessReturnStatement(ReturnStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarDeclStatement ProcessVarDeclStatement(VarDeclStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual WhileStatement ProcessWhileStatement(WhileStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionStatement ProcessAssertionStatement(AssertionStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionObject ProcessAssertionObject(AssertionObject node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionPredicate ProcessAssertionPredicate(AssertionPredicate node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual AssertionSubject ProcessAssertionSubject(AssertionSubject node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual RetractionStatement ProcessRetractionStatement(RetractionStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual WithScopeStatement ProcessWithScopeStatement(WithScopeStatement node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual BinaryExp ProcessBinaryExp(BinaryExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual CastExp ProcessCastExp(CastExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual LambdaExp ProcessLambdaExp(LambdaExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual FuncCallExp ProcessFuncCallExp(FuncCallExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual LiteralExp ProcessLiteralExp(LiteralExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual MemberAccessExp ProcessMemberAccessExp(MemberAccessExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual ObjectInstantiationExp ProcessObjectInstantiationExp(ObjectInstantiationExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual UnaryExp ProcessUnaryExp(UnaryExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual VarRefExp ProcessVarRefExp(VarRefExp node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual List ProcessList(List node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Atom ProcessAtom(Atom node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Triple ProcessTriple(Triple node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }

    public virtual Graph ProcessGraph(Graph node, TContext ctx)
    {
        return node; // now the AST nodes are immutable, a good default is just to return the node.
    }


}

