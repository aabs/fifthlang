using System;
using System.Collections.Generic;
using System.Linq;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowers augmented assignment operators (+= and -=) based on type information to determine
/// whether they represent graph/triple operations or regular arithmetic.
/// 
/// For +=:
/// - If operands have graph/triple types: emit appropriate graph operation
/// - Otherwise: keep as regular assignment
/// 
/// For -=:
/// - If operands have graph/triple types: emit appropriate graph operation  
/// - Otherwise: keep as regular assignment
/// 
/// This rewriter should run AFTER type annotation so that type information is available.
/// </summary>
public class AugmentedAssignmentLoweringRewriter : DefaultRecursiveDescentVisitor
{
    private readonly Stack<HashSet<string>> _functionLocals = new();
    private HashSet<string>? CurrentFunctionLocals => _functionLocals.Count > 0 ? _functionLocals.Peek() : null;

    private bool IsNameInCurrentFunctionScope(string name)
        => !string.IsNullOrEmpty(name) && CurrentFunctionLocals != null && CurrentFunctionLocals.Contains(name);

    private static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        _functionLocals.Push(new HashSet<string>(StringComparer.Ordinal));

        // Record parameters in scope
        if (ctx.Params != null)
        {
            foreach (var param in ctx.Params)
            {
                CurrentFunctionLocals?.Add(param.Name);
            }
        }

        var result = base.VisitFunctionDef(ctx);
        _functionLocals.Pop();
        return result;
    }

    public override VarDeclStatement VisitVarDeclStatement(VarDeclStatement ctx)
    {
        if (ctx.VariableDecl != null && !string.IsNullOrEmpty(ctx.VariableDecl.Name))
        {
            CurrentFunctionLocals?.Add(ctx.VariableDecl.Name);
        }
        return base.VisitVarDeclStatement(ctx);
    }

    public override BlockStatement VisitBlockStatement(BlockStatement ctx)
    {
        // Visit children first
        var visited = base.VisitBlockStatement(ctx);

        // Transform augmented assignments by structural pattern (no builder-time markers)
        // This lowering ONLY handles graph/triple operations. Regular arithmetic operations
        // are left as-is since they're already in the correct form.
        var newStatements = new List<Statement>();
        foreach (var stmt in visited.Statements)
        {
            if (stmt is AssignmentStatement assign && assign.RValue is BinaryExp bin &&
                IsSameTarget(assign.LValue, bin.LHS))
            {
                var loc = stmt.Location ?? new SourceLocationMetadata(0, string.Empty, 0, string.Empty);
                var lhs = assign.LValue;
                var actualRhs = bin.RHS;

                // Check if this is a graph/triple operation that needs special lowering
                bool isGraphOrTripleOp = false;
                switch (bin.Operator)
                {
                    case Operator.ArithmeticAdd:
                        isGraphOrTripleOp = RhsLooksLikeGraph(actualRhs) || RhsLooksLikeTriple(actualRhs);
                        break;
                    case Operator.ArithmeticSubtract:
                        isGraphOrTripleOp = RhsLooksLikeTriple(actualRhs);
                        break;
                }

                if (isGraphOrTripleOp)
                {
                    // This is a graph/triple operation - apply special lowering
                    Statement lowered = bin.Operator switch
                    {
                        Operator.ArithmeticAdd => HandlePlusAssign(lhs, actualRhs, loc),
                        Operator.ArithmeticSubtract => HandleMinusAssign(lhs, actualRhs, loc),
                        _ => stmt
                    };
                    newStatements.Add(lowered);
                }
                else
                {
                    // Regular arithmetic operation - keep as-is
                    newStatements.Add(stmt);
                }
            }
            else
            {
                newStatements.Add(stmt);
            }
        }

        return visited with { Statements = newStatements };
    }

    private static bool IsSameTarget(Expression a, Expression b)
    {
        if (a is VarRefExp va && b is VarRefExp vb)
        {
            return string.Equals(va.VarName, vb.VarName, StringComparison.Ordinal);
        }

        if (a is MemberAccessExp ma && b is MemberAccessExp mb)
        {
            // Simple structural check: same LHS and RHS is null or both varrefs with same name
            return IsSameTarget(ma.LHS, mb.LHS) &&
                   ((ma.RHS == null && mb.RHS == null) ||
                    (ma.RHS is VarRefExp mvr1 && mb.RHS is VarRefExp mvr2 && string.Equals(mvr1.VarName, mvr2.VarName, StringComparison.Ordinal)));
        }

        // Fallback: no match
        return false;
    }

    private Statement HandlePlusAssign(Expression lhs, Expression rhs, SourceLocationMetadata loc)
    {
        // Determine if this should be lowered as a graph/triple operation
        bool rhsIsGraph = RhsLooksLikeGraph(rhs);
        bool rhsIsTriple = RhsLooksLikeTriple(rhs);

        if (rhsIsTriple || rhsIsGraph)
        {
            if (rhsIsTriple)
            {
                // Triple operation: emit lhs.Assert(rhs) as an ExpStatement
                var assertExpr = CreateAssertCall(lhs, rhs, loc);
                return new ExpStatement
                {
                    Annotations = new Dictionary<string, object>(),
                    RHS = assertExpr,
                    Location = loc,
                    Type = Void
                };
            }
            else
            {
                // Graph operation: emit KG.SaveGraph(store, rhs) as an ExpStatement
                var saveGraphExpr = CreateSaveGraphCall(lhs, rhs, loc);
                return new ExpStatement
                {
                    Annotations = new Dictionary<string, object>(),
                    RHS = saveGraphExpr,
                    Location = loc,
                    Type = Void
                };
            }
        }
        else
        {
            // Regular arithmetic addition - keep as assignment with binary expression
            // Clone lhs to avoid sharing AST nodes between LValue and BinaryExp.LHS
            var lhsClone = new DefaultRecursiveDescentVisitor().Visit(lhs) as Expression ?? lhs;
            
            var addExpr = new BinaryExp
            {
                Annotations = new Dictionary<string, object>(),
                LHS = lhsClone,
                RHS = rhs,
                Operator = Operator.ArithmeticAdd,
                Location = loc,
                Type = null
            };

            return new AssignmentStatement
            {
                Annotations = new Dictionary<string, object>(),
                LValue = lhs,
                RValue = addExpr,
                Location = loc,
                Type = Void
            };
        }
    }

    private Statement HandleMinusAssign(Expression lhs, Expression rhs, SourceLocationMetadata loc)
    {
        bool rhsIsTriple = RhsLooksLikeTriple(rhs);

        if (rhsIsTriple)
        {
            // Graph operation: emit lhs.Retract(rhs) as an ExpStatement
            var retractExpr = CreateRetractCall(lhs, rhs, loc);
            return new ExpStatement
            {
                Annotations = new Dictionary<string, object>(),
                RHS = retractExpr,
                Location = loc,
                Type = Void
            };
        }
        else
        {
            // Regular arithmetic subtraction - keep as assignment with binary expression
            // Clone lhs to avoid sharing AST nodes between LValue and BinaryExp.LHS
            var lhsClone = new DefaultRecursiveDescentVisitor().Visit(lhs) as Expression ?? lhs;
            
            var subtractExpr = new BinaryExp
            {
                Annotations = new Dictionary<string, object>(),
                LHS = lhsClone,
                RHS = rhs,
                Operator = Operator.ArithmeticSubtract,
                Location = loc,
                Type = Void
            };

            return new AssignmentStatement
            {
                Annotations = new Dictionary<string, object>(),
                LValue = lhs,
                RValue = subtractExpr,
                Location = loc,
                Type = Void
            };
        }
    }

    private static bool RhsLooksLikeGraph(Expression e)
    {
        if (e is Graph || e is GraphAssertionBlockExp) return true;
        if (e is MemberAccessExp ma && ma.RHS is FuncCallExp fc &&
            fc.Annotations != null && fc.Annotations.TryGetValue("FunctionName", out var fnObj) && fnObj is string fn &&
            string.Equals(fn, "CreateGraph", StringComparison.Ordinal))
        {
            return true;
        }
        // Check type if available
        if (e.Type is FifthType.TType tt && tt.Name.Value != null &&
            tt.Name.Value.Equals("graph", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private static bool RhsLooksLikeTriple(Expression e)
    {
        if (e is TripleLiteralExp) return true;
        if (e is MemberAccessExp ma && ma.RHS is FuncCallExp fc &&
            fc.Annotations != null && fc.Annotations.TryGetValue("FunctionName", out var fnObj) && fnObj is string fn &&
            string.Equals(fn, "CreateTriple", StringComparison.Ordinal))
        {
            return true;
        }
        // Check type if available  
        if (e.Type is FifthType.TType tt && tt.Name.Value != null &&
            tt.Name.Value.Equals("triple", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        // Only treat VarRefExp as triple if it has an unknown/unresolved type
        // or if its type is explicitly "triple". Don't treat typed primitive variables as triples.
        if (e is VarRefExp vr)
        {
            // If type is known and not triple, don't treat as triple
            if (vr.Type != null)
            {
                if (vr.Type is FifthType.TType vtt && vtt.Name.Value.Equals("triple", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (vr.Type is FifthType.UnknownType)
                {
                    // Unknown type - could be triple, allow lowering
                    return true;
                }
                // Known non-triple type - don't treat as triple
                return false;
            }
            // No type info - conservatively treat as potential triple only if we have no other info
            // This is a fallback for cases where type annotation hasn't run yet
            return false;
        }
        return false;
    }

    private MemberAccessExp CreateAssertCall(Expression lhs, Expression rhs, SourceLocationMetadata loc)
    {
        var assertCall = new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = new List<Expression> { rhs },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "Assert" },
            Location = loc,
            Parent = null,
            Type = null
        };

        return new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = lhs,
            RHS = assertCall,
            Location = loc,
            Type = Void
        };
    }

    private MemberAccessExp CreateRetractCall(Expression lhs, Expression rhs, SourceLocationMetadata loc)
    {
        var retractCall = new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = new List<Expression> { rhs },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "Retract" },
            Location = loc,
            Parent = null,
            Type = null
        };

        return new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = lhs,
            RHS = retractCall,
            Location = loc,
            Type = Void
        };
    }

    private MemberAccessExp CreateSaveGraphCall(Expression lhs, Expression rhs, SourceLocationMetadata loc)
    {
        Expression storeArg;
        if (lhs is VarRefExp v2 && IsNameInCurrentFunctionScope(v2.VarName))
        {
            storeArg = v2;
        }
        else
        {
            var kgVar = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc, Type = Void };
            storeArg = new MemberAccessExp
            {
                Annotations = new Dictionary<string, object>(),
                LHS = kgVar,
                RHS = new FuncCallExp
                {
                    FunctionDef = null,
                    InvocationArguments = new List<Expression>(),
                    Annotations = new Dictionary<string, object> { ["FunctionName"] = "CreateStore" },
                    Location = loc,
                    Parent = null,
                    Type = null
                },
                Location = loc,
                Type = Void
            };
        }

        var kgVar3 = new VarRefExp { VarName = "KG", Annotations = new Dictionary<string, object>(), Location = loc, Type = Void };
        var func2 = new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = new List<Expression> { storeArg, rhs },
            Annotations = new Dictionary<string, object> { ["FunctionName"] = "SaveGraph" },
            Location = loc,
            Parent = null,
            Type = null
        };

        return new MemberAccessExp
        {
            Annotations = new Dictionary<string, object>(),
            LHS = kgVar3,
            RHS = func2,
            Location = loc,
            Type = Void
        };
    }
}
