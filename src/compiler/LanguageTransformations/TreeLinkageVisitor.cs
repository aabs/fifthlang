// ReSharper disable InconsistentNaming

using ast;
using ast_model.Symbols;
using static Fifth.DebugHelpers;

namespace compiler.LanguageTransformations;

public class TreeLinkageVisitor : NullSafeRecursiveDescentVisitor
{
    private readonly Stack<AstThing> parents = new();
    // Debug helpers (DebugEnabled and DebugLog) are provided by the shared DebugHelpers class imported above.

    #region Helpers
    private void EnterNonTerminal(AstThing ctx)
    {
        ctx.Parent = parents.PeekOrDefault();
        parents.Push(ctx);
    }

    private void EnterTerminal(AstThing ctx) => ctx.Parent = parents.PeekOrDefault();

    private void LeaveNonTerminal(AstThing ctx) => parents.Pop();

    private void LeaveTerminal(AstThing ctx)
    {
    }
    #endregion

    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitAssemblyDef(ctx);
        LeaveNonTerminal(ctx);
        return result ?? ctx;
    }

    public override AssemblyRef VisitAssemblyRef(AssemblyRef ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitAssemblyRef(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override AssertionObject VisitAssertionObject(AssertionObject ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitAssertionObject(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssertionPredicate VisitAssertionPredicate(AssertionPredicate ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitAssertionPredicate(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssertionStatement VisitAssertionStatement(AssertionStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitAssertionStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override AssertionSubject VisitAssertionSubject(AssertionSubject ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitAssertionSubject(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override AssignmentStatement VisitAssignmentStatement(AssignmentStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitAssignmentStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override Atom VisitAtom(Atom ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitAtom(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override AtomLiteralExp VisitAtomLiteralExp(AtomLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitAtomLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitBinaryExp(ctx);
        LeaveNonTerminal(ctx);
        return result ?? ctx;
    }

    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitBlockStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override BooleanLiteralExp VisitBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitBooleanLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override CastExp VisitCastExp(CastExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitCastExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override CharLiteralExp VisitCharLiteralExp(CharLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitCharLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitClassDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override DateLiteralExp VisitDateLiteralExp(DateLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitDateLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override DateTimeLiteralExp VisitDateTimeLiteralExp(DateTimeLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitDateTimeLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override DurationLiteralExp VisitDurationLiteralExp(DurationLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitDurationLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override ExpStatement VisitExpStatement(ExpStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitExpStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override FieldDef VisitFieldDef(FieldDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitFieldDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override Float16LiteralExp VisitFloat16LiteralExp(Float16LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitFloat16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Float4LiteralExp VisitFloat4LiteralExp(Float4LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitFloat4LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Float8LiteralExp VisitFloat8LiteralExp(Float8LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitFloat8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override ForeachStatement VisitForeachStatement(ForeachStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitForeachStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ForStatement VisitForStatement(ForStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitForStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitFuncCallExp(ctx);
        if (result == null)
        {
            LeaveNonTerminal(ctx);
            return ctx;
        }

        // Skip if already resolved
        if (result.FunctionDef != null)
        {
            LeaveNonTerminal(ctx);
            return result;
        }

        // Determine function name from annotations (set by parser)
        if (!(result.Annotations?.TryGetValue("FunctionName", out var fnObj) == true && fnObj is string functionName))
        {
            LeaveNonTerminal(ctx);
            return result;
        }

        // External qualified call? (e.g., KG.CreateGraph)
        if (result.Annotations != null && result.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type)
        {
            LeaveNonTerminal(ctx);
            return result;
        }

        var nearestScope = ctx.NearestScope();
        if (nearestScope == null)
        {
            LeaveNonTerminal(ctx);
            return result;
        }

        var functionDef = FindFunctionInScope(functionName, nearestScope);
        if (functionDef != null)
        {
            result = result with { FunctionDef = functionDef };
        }

        LeaveNonTerminal(ctx);
        return result;
    }

    private FunctionDef? FindFunctionInScope(string functionName, ScopeAstThing scope)
    {
        // Walk up scopes looking for matching function
        ScopeAstThing? current = scope;
        while (current != null)
        {
            if (current is ModuleDef module)
            {
                var fd = module.Functions
                    .OfType<FunctionDef>()
                    .FirstOrDefault(f => f.Name.Value == functionName);
                if (fd != null) return fd;
            }
            else if (current is ClassDef cls)
            {
                var md = cls.MemberDefs
                    .OfType<MethodDef>()
                    .FirstOrDefault(m => m.FunctionDef.Name.Value == functionName);
                if (md?.FunctionDef != null) return md.FunctionDef;
            }

            current = current.Parent?.NearestScope();
        }

        // Fallback: search all modules in the assembly
        IAstThing? p = scope;
        AssemblyDef? asm = null;
        while (p != null)
        {
            if (p is AssemblyDef a) { asm = a; break; }
            p = p.Parent;
        }
        if (asm != null)
        {
            foreach (var mod in asm.Modules)
            {
                var fd = mod.Functions
                    .OfType<FunctionDef>()
                    .FirstOrDefault(f => f.Name.Value == functionName);
                if (fd != null) return fd;
            }
        }

        return null;
    }

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        DebugLog($"DEBUG: TreeLinkageVisitor.VisitFunctionDef: {ctx.Name.Value}");
        EnterNonTerminal(ctx);
        var result = base.VisitFunctionDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override FunctorDef VisitFunctorDef(FunctorDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitFunctorDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override Graph VisitGraph(Graph ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitGraph(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override GraphNamespaceAlias VisitGraphNamespaceAlias(GraphNamespaceAlias ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitGraphNamespaceAlias(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override GuardStatement VisitGuardStatement(GuardStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitGuardStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override IfElseStatement VisitIfElseStatement(IfElseStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitIfElseStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override InferenceRuleDef VisitInferenceRuleDef(InferenceRuleDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitInferenceRuleDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override Int16LiteralExp VisitInt16LiteralExp(Int16LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitInt16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int32LiteralExp VisitInt32LiteralExp(Int32LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitInt32LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int64LiteralExp VisitInt64LiteralExp(Int64LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitInt64LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override Int8LiteralExp VisitInt8LiteralExp(Int8LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitInt8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override KnowledgeManagementBlock VisitKnowledgeManagementBlock(KnowledgeManagementBlock ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitKnowledgeManagementBlock(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override LambdaExp VisitLambdaExp(LambdaExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitLambdaExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ListComprehension VisitListComprehension(ListComprehension ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitListComprehension(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ListLiteral VisitListLiteral(ListLiteral ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitListLiteral(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitMemberAccessExp(ctx);

        // Detect pattern: <TypeName>.<FuncCall>(...)
        try
        {
            if (result?.LHS is VarRefExp typeQualifier && result.RHS is FuncCallExp memberCall)
            {
                var qualifierName = typeQualifier.VarName;
                if (!string.IsNullOrWhiteSpace(qualifierName))
                {
                    Type? resolvedType = null;
                    if (ast_model.TypeSystem.TypeRegistry.DefaultRegistry.TryGetTypeByName(qualifierName, out var ft)
                        && ft is ast_model.TypeSystem.FifthType.TDotnetType dotType1)
                    {
                        resolvedType = dotType1.TheType;
                    }
                    else if (string.Equals(qualifierName, "KG", StringComparison.Ordinal))
                    {
                        resolvedType = typeof(Fifth.System.KG);
                    }
                    else if (string.Equals(qualifierName, "std", StringComparison.Ordinal))
                    {
                        // Only map 'std.print' to System.Console.WriteLine; leave other std.* unresolved
                        if (memberCall.Annotations.TryGetValue("FunctionName", out var fnObjStd) && fnObjStd is string fnStd && string.Equals(fnStd, "print", StringComparison.Ordinal))
                        {
                            resolvedType = typeof(System.Console);
                        }
                    }

                    if (resolvedType != null)
                    {
                        memberCall["ExternalType"] = resolvedType;
                        if (memberCall.Annotations.TryGetValue("FunctionName", out var nameObj) && nameObj is string fn)
                        {
                            // For std.print mapped to Console
                            if (resolvedType == typeof(System.Console) && string.Equals(fn, "print", StringComparison.Ordinal))
                            {
                                memberCall["ExternalMethodName"] = "WriteLine";
                            }
                            else
                            {
                                memberCall["ExternalMethodName"] = fn;
                            }
                        }

                        DebugLog($"DEBUG: Qualified external call detected: {resolvedType.FullName}::{(memberCall.Annotations.TryGetValue("FunctionName", out var n) ? n : "?")}");
                    }
                }
            }

            // Additional detection: handle chained calls where qualifier is a MemberAccessExp whose leftmost
            // qualifier is a VarRefExp (e.g., KG.CreateGraph().Assert(...)). Walk left side to find root VarRef.
            if (result?.LHS is MemberAccessExp chainedLeft && result.RHS is FuncCallExp chainedCall)
            {
                // Walk to leftmost qualifier
                ast.VarRefExp? rootVar = null;
                ast.Expression walker = chainedLeft;
                while (walker is MemberAccessExp ma)
                {
                    if (ma.LHS is VarRefExp vr)
                    {
                        rootVar = vr;
                        break;
                    }
                    if (ma.LHS == null) break; // no further left side
                    walker = ma.LHS;
                }

                if (rootVar != null)
                {
                    var qualifierName = rootVar.VarName;
                    if (!string.IsNullOrWhiteSpace(qualifierName))
                    {
                        Type? resolvedType = null;
                        if (ast_model.TypeSystem.TypeRegistry.DefaultRegistry.TryGetTypeByName(qualifierName, out var ft2)
                            && ft2 is ast_model.TypeSystem.FifthType.TDotnetType dotType2)
                        {
                            resolvedType = dotType2.TheType;
                        }
                        else if (string.Equals(qualifierName, "KG", StringComparison.Ordinal))
                        {
                            resolvedType = typeof(Fifth.System.KG);
                        }
                        else if (string.Equals(qualifierName, "std", StringComparison.Ordinal))
                        {
                            if (chainedCall.Annotations.TryGetValue("FunctionName", out var fnObjStd2) && fnObjStd2 is string fnStd2 && string.Equals(fnStd2, "print", StringComparison.Ordinal))
                            {
                                resolvedType = typeof(System.Console);
                            }
                        }

                        if (resolvedType != null)
                        {
                            chainedCall["ExternalType"] = resolvedType;
                            if (chainedCall.Annotations.TryGetValue("FunctionName", out var nameObj2) && nameObj2 is string fn2)
                            {
                                if (resolvedType == typeof(System.Console) && string.Equals(fn2, "print", StringComparison.Ordinal))
                                {
                                    chainedCall["ExternalMethodName"] = "WriteLine";
                                }
                                else
                                {
                                    chainedCall["ExternalMethodName"] = fn2;
                                }
                            }
                            DebugLog($"DEBUG: Chained external call detected: {resolvedType.FullName}::{(chainedCall.Annotations.TryGetValue("FunctionName", out var n2) ? n2 : "?")}");
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            DebugLog($"DEBUG: Exception while annotating qualified call: {ex.Message}");
        }

        LeaveNonTerminal(ctx);
        return result ?? ctx;
    }

    public override MemberRef VisitMemberRef(MemberRef ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitMemberRef(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override MethodDef VisitMethodDef(MethodDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitMethodDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitModuleDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ObjectInitializerExp VisitObjectInitializerExp(ObjectInitializerExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitObjectInitializerExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitParamDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitParamDestructureDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitPropertyBindingDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitPropertyDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override PropertyInitializerExp VisitPropertyInitializerExp(PropertyInitializerExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitPropertyInitializerExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override PropertyRef VisitPropertyRef(PropertyRef ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitPropertyRef(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override RetractionStatement VisitRetractionStatement(RetractionStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitRetractionStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override ReturnStatement VisitReturnStatement(ReturnStatement ctx)
    {
        DebugLog($"DEBUG: TreeLinkageVisitor.VisitReturnStatement: {ctx.ReturnValue?.GetType().Name ?? "null"}");
        EnterNonTerminal(ctx);
        var result = base.VisitReturnStatement(ctx);
        DebugLog($"DEBUG: TreeLinkageVisitor.VisitReturnStatement result: {result?.ReturnValue?.GetType().Name ?? "null"}");
        LeaveNonTerminal(ctx);
        return result ?? ctx;
    }

    public override StringLiteralExp VisitStringLiteralExp(StringLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitStringLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override TimeLiteralExp VisitTimeLiteralExp(TimeLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitTimeLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override TripleLiteralExp VisitTripleLiteralExp(TripleLiteralExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitTripleLiteralExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override TypeDef VisitTypeDef(TypeDef ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitTypeDef(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override TypeRef VisitTypeRef(TypeRef ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitTypeRef(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnaryExp VisitUnaryExp(UnaryExp ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitUnaryExp(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override UnsignedInt16LiteralExp VisitUnsignedInt16LiteralExp(UnsignedInt16LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitUnsignedInt16LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt32LiteralExp VisitUnsignedInt32LiteralExp(UnsignedInt32LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitUnsignedInt32LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt64LiteralExp VisitUnsignedInt64LiteralExp(UnsignedInt64LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitUnsignedInt64LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UnsignedInt8LiteralExp VisitUnsignedInt8LiteralExp(UnsignedInt8LiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitUnsignedInt8LiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override UriLiteralExp VisitUriLiteralExp(UriLiteralExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitUriLiteralExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitVarDeclStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override VariableDecl VisitVariableDecl(VariableDecl ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitVariableDecl(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override VarRef VisitVarRef(VarRef ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitVarRef(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
        EnterTerminal(ctx);
        var result = base.VisitVarRefExp(ctx);
        LeaveTerminal(ctx);
        return result;
    }

    public override WhileStatement VisitWhileStatement(WhileStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitWhileStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }

    public override WithScopeStatement VisitWithScopeStatement(WithScopeStatement ctx)
    {
        EnterNonTerminal(ctx);
        var result = base.VisitWithScopeStatement(ctx);
        LeaveNonTerminal(ctx);
        return result;
    }
}
