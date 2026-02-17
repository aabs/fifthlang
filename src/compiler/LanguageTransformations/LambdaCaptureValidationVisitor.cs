using ast;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

public sealed class LambdaCaptureValidationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<compiler.Diagnostic>? diagnostics;

    public LambdaCaptureValidationVisitor(List<compiler.Diagnostic>? diagnostics)
    {
        this.diagnostics = diagnostics;
    }

    public override LambdaExp VisitLambdaExp(LambdaExp ctx)
    {
        var result = base.VisitLambdaExp(ctx);

        var functor = result.FunctorDef;
        var apply = functor?.InvocationFuncDev;
        if (apply == null)
        {
            return result;
        }

        var outerScope = functor.NearestScopeAbove();
        if (outerScope != null)
        {
            var declaredNames = LambdaCaptureUtils.GetDeclaredNames(apply);
            foreach (var name in declaredNames)
            {
                if (outerScope.TryResolveByName(name, out var entry) && entry != null && LambdaCaptureUtils.IsVariableLike(entry.Symbol.Kind))
                {
                    diagnostics?.Add(new compiler.Diagnostic(
                        compiler.DiagnosticLevel.Error,
                        $"Lambda declares '{name}', which shadows an outer variable (shadowing not allowed)",
                        ctx.Location?.Filename ?? "",
                        LambdaClosureDiagnostics.ERR_LF_SHADOWING_NOT_ALLOWED));
                }
            }
        }

        var captureNames = LambdaCaptureUtils.GetCaptureNames(apply);
        if (captureNames.Count > 0)
        {
            foreach (var assigned in LambdaCaptureUtils.FindAssignedVariableNames(apply.Body))
            {
                if (captureNames.Contains(assigned))
                {
                    diagnostics?.Add(new compiler.Diagnostic(
                        compiler.DiagnosticLevel.Error,
                        $"Lambda assigns to captured variable '{assigned}' (captures are read-only)",
                        ctx.Location?.Filename ?? "",
                        LambdaClosureDiagnostics.ERR_LF_CAPTURED_VARIABLE_ASSIGNED));
                }
            }
        }

        return result;
    }
}

internal static class LambdaCaptureUtils
{
    public static bool IsVariableLike(SymbolKind kind) => kind is SymbolKind.VarDeclStatement or SymbolKind.ParamDef or SymbolKind.PropertyBindingDef;

    public static HashSet<string> GetDeclaredNames(FunctionDef apply)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var p in apply.Params)
        {
            names.Add(p.Name);
        }

        foreach (var v in FindVariableDecls(apply.Body))
        {
            names.Add(v.Name);
        }

        foreach (var pb in FindPropertyBindingDefs(apply.Body))
        {
            names.Add(pb.IntroducedVariable.Value);
        }

        // Comprehension introduces a VarName which acts like a local
        foreach (var comp in FindListComprehensions(apply.Body))
        {
            names.Add(comp.VarName);
        }

        return names;
    }

    public static HashSet<string> GetCaptureNames(FunctionDef apply)
    {
        var declared = GetDeclaredNames(apply);
        var captured = new HashSet<string>(StringComparer.Ordinal);

        foreach (var vr in FindVarRefs(apply.Body))
        {
            if (!declared.Contains(vr.VarName))
            {
                captured.Add(vr.VarName);
            }
        }

        return captured;
    }

    public static IEnumerable<string> FindAssignedVariableNames(BlockStatement body)
    {
        foreach (var assign in FindAssignments(body))
        {
            if (assign.LValue is VarRefExp vr)
            {
                yield return vr.VarName;
            }
        }
    }

    private static IEnumerable<AssignmentStatement> FindAssignments(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is AssignmentStatement a)
            {
                yield return a;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerable<VariableDecl> FindVariableDecls(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is VariableDecl v)
            {
                yield return v;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerable<PropertyBindingDef> FindPropertyBindingDefs(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is PropertyBindingDef pb)
            {
                yield return pb;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerable<ListComprehension> FindListComprehensions(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is ListComprehension lc)
            {
                yield return lc;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerable<VarRefExp> FindVarRefs(AstThing node)
    {
        var stack = new Stack<AstThing>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (cur is VarRefExp vr)
            {
                yield return vr;
            }

            foreach (var child in GetChildren(cur))
            {
                stack.Push(child);
            }
        }
    }

    private static IEnumerable<AstThing> GetChildren(AstThing node)
    {
        // Minimal child enumeration for capture validation.
        return node switch
        {
            AssemblyDef a => a.Modules,
            ModuleDef m => m.Classes.Concat<AstThing>(m.Functions),
            ClassDef c => c.MemberDefs,
            MethodDef md => new AstThing[] { md.FunctionDef },
            FunctionDef f => f.Params.Concat<AstThing>(new AstThing[] { f.Body }),
            BlockStatement b => b.Statements,
            VarDeclStatement vds => new AstThing[] { vds.VariableDecl }.Concat(vds.InitialValue != null ? new AstThing[] { vds.InitialValue } : Array.Empty<AstThing>()),
            ExpStatement es => new AstThing[] { es.RHS },
            ReturnStatement rs => new AstThing[] { rs.ReturnValue },
            AssignmentStatement a => new AstThing[] { a.LValue, a.RValue },
            IfElseStatement i => new AstThing[] { i.Condition, i.ThenBlock, i.ElseBlock },
            WhileStatement w => new AstThing[] { w.Condition, w.Body },
            ForeachStatement fe => new AstThing[] { fe.LoopVariable, fe.Collection, fe.Body },
            BinaryExp be => new AstThing[] { be.LHS, be.RHS },
            UnaryExp ue => new AstThing[] { ue.Operand },
            CastExp ce => new AstThing[] { },
            MemberAccessExp ma => new AstThing[] { ma.LHS }.Concat(ma.RHS != null ? new AstThing[] { ma.RHS } : Array.Empty<AstThing>()),
            FuncCallExp fc => fc.InvocationArguments,
            LambdaExp le => new AstThing[] { le.FunctorDef },
            FunctorDef fd => new AstThing[] { fd.InvocationFuncDev },
            ListLiteral ll => ll.ElementExpressions,
            ListComprehension lc => new AstThing[] { lc.Projection, lc.Source }.Concat(lc.Constraints),
            IndexerExpression ie => new AstThing[] { ie.IndexExpression, ie.OffsetExpression },
            ObjectInitializerExp oi => oi.ConstructorArguments.Concat<AstThing>(oi.PropertyInitialisers),
            PropertyInitializerExp pi => new AstThing[] { pi.RHS },
            _ => Array.Empty<AstThing>()
        };
    }
}
