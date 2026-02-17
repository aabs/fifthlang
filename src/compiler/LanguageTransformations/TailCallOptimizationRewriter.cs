using ast;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Detects and optimizes tail-recursive function calls by rewriting them to loops.
/// This prevents stack overflow for deep recursion patterns.
/// 
/// Transformation pattern:
/// ```
/// func(n: int, acc: int): int {
///     if (n <= 0) { return acc; }
///     else { return func(n - 1, acc + n); }
/// }
/// ```
/// 
/// Becomes:
/// ```
/// func(n: int, acc: int): int {
///     _tco_continue: bool = true;
///     while (_tco_continue) {
///         if (n <= 0) { _tco_continue = false; return acc; }
///         else {
///             // Update params
///             _tco_tmp_n: int = n - 1;
///             _tco_tmp_acc: int = acc + n;
///             n = _tco_tmp_n;
///             acc = _tco_tmp_acc;
///             // Loop continues
///         }
///     }
///     return 0; // Unreachable fallback
/// }
/// ```
/// </summary>
public sealed class TailCallOptimizationRewriter : DefaultRecursiveDescentVisitor
{
    private FunctionDef? currentFunction;
    private int tcoTempCounter;

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        var previous = currentFunction;
        currentFunction = ctx;
        tcoTempCounter = 0;

        // Don't call base visitor - analyze the original function
        // to avoid infinite recursion through nested function definitions
        var result = ctx;

        // Check if function has tail-recursive calls
        if (HasTailRecursiveCalls(result))
        {
            result = TransformToLoop(result);
        }

        currentFunction = previous;
        return result;
    }

    private bool HasTailRecursiveCalls(FunctionDef func)
    {
        if (func.Name == null || string.IsNullOrEmpty(func.Name.Value))
            return false;

        var tailCalls = FindTailRecursiveCalls(func.Body, func.Name.Value);
        return tailCalls.Any();
    }

    private List<FuncCallExp> FindTailRecursiveCalls(BlockStatement body, string functionName)
    {
        var results = new List<FuncCallExp>();
        FindTailCallsInBlock(body, functionName, results);
        return results;
    }

    private void FindTailCallsInBlock(BlockStatement block, string functionName, List<FuncCallExp> results)
    {
        if (block.Statements.Count == 0)
            return;

        var lastStmt = block.Statements[^1];

        switch (lastStmt)
        {
            case ReturnStatement ret:
                if (IsTailRecursiveCall(ret.ReturnValue, functionName))
                {
                    results.Add((FuncCallExp)ret.ReturnValue);
                }
                break;

            case IfElseStatement ifElse:
                FindTailCallsInBlock(ifElse.ThenBlock, functionName, results);
                FindTailCallsInBlock(ifElse.ElseBlock, functionName, results);
                break;
        }
    }

    private bool IsTailRecursiveCall(Expression exp, string functionName)
    {
        if (exp is not FuncCallExp call)
            return false;

        // Check if it's a call to the same function
        if (call.FunctionDef != null && call.FunctionDef.Name.Value == functionName)
            return true;

        // Also check annotation-based name matching
        if (call.Annotations != null &&
            call.Annotations.TryGetValue("FunctionName", out var nameObj) &&
            nameObj is string name &&
            name == functionName)
        {
            return true;
        }

        return false;
    }

    private FunctionDef TransformToLoop(FunctionDef func)
    {
        // Create TCO control variable
        var boolTypeName = TypeName.From("bool");
        var tcoControlVar = new VariableDecl
        {
            Name = "_tco_continue",
            TypeName = boolTypeName,
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance,
            Type = new FifthType.TType { Name = boolTypeName }
        };

        var tcoControlDecl = new VarDeclStatement
        {
            VariableDecl = tcoControlVar,
            InitialValue = new BooleanLiteralExp { Value = true }
        };

        // Transform the function body
        var transformedBody = TransformBodyForTCO(func.Body, func.Name.Value, func.Params);

        // Wrap in while loop
        var whileLoop = new WhileStatement
        {
            Condition = new VarRefExp { VarName = "_tco_continue" },
            Body = transformedBody
        };

        // Create unreachable return fallback
        var fallbackReturn = new ReturnStatement
        {
            ReturnValue = GetDefaultValueForType(func.ReturnType)
        };

        var newBody = new BlockStatement
        {
            Statements = new List<Statement> { tcoControlDecl, whileLoop, fallbackReturn }
        };

        return func with { Body = newBody };
    }

    private BlockStatement TransformBodyForTCO(BlockStatement body, string functionName, List<ParamDef> parameters)
    {
        var newStatements = new List<Statement>();

        foreach (var stmt in body.Statements)
        {
            var transformed = TransformStatementForTCO(stmt, functionName, parameters);
            newStatements.Add(transformed);
        }

        return new BlockStatement { Statements = newStatements };
    }

    private Statement TransformStatementForTCO(Statement stmt, string functionName, List<ParamDef> parameters)
    {
        return stmt switch
        {
            ReturnStatement ret when IsTailRecursiveCall(ret.ReturnValue, functionName) =>
                TransformTailRecursiveReturn(ret, (FuncCallExp)ret.ReturnValue, parameters),

            IfElseStatement ifElse =>
                ifElse with
                {
                    ThenBlock = TransformBodyForTCO(ifElse.ThenBlock, functionName, parameters),
                    ElseBlock = TransformBodyForTCO(ifElse.ElseBlock, functionName, parameters)
                },

            ReturnStatement ret =>
                CreateTCOExit(ret),

            _ => stmt
        };
    }

    private Statement TransformTailRecursiveReturn(ReturnStatement ret, FuncCallExp call, List<ParamDef> parameters)
    {
        var statements = new List<Statement>();

        // Create temporary variables for new parameter values
        var tempVars = new List<(string TempName, Expression Value)>();

        for (int i = 0; i < parameters.Count && i < call.InvocationArguments.Count; i++)
        {
            var param = parameters[i];
            var arg = call.InvocationArguments[i];
            var tempName = $"_tco_tmp_{param.Name}_{tcoTempCounter++}";

            tempVars.Add((tempName, arg));

            var paramType = param.Type ?? new FifthType.TType { Name = TypeName.From("object") };
            // Get TypeName from the FifthType - will be "object" if type is unresolved
            TypeName paramTypeName;
            if (paramType is FifthType.TType ttype)
            {
                paramTypeName = ttype.Name;
            }
            else
            {
                paramTypeName = TypeName.From("object");
            }

            var tempDecl = new VarDeclStatement
            {
                VariableDecl = new VariableDecl
                {
                    Name = tempName,
                    TypeName = paramTypeName,
                    Visibility = Visibility.Private,
                    CollectionType = CollectionType.SingleInstance,
                    Type = paramType
                },
                InitialValue = arg
            };
            statements.Add(tempDecl);
        }

        // Assign temp values back to parameters
        for (int i = 0; i < parameters.Count && i < tempVars.Count; i++)
        {
            var param = parameters[i];
            var (tempName, _) = tempVars[i];

            var assignment = new AssignmentStatement
            {
                LValue = new VarRefExp { VarName = param.Name },
                RValue = new VarRefExp { VarName = tempName }
            };
            statements.Add(assignment);
        }

        // Loop will continue automatically (no need to set _tco_continue = true)

        return new BlockStatement { Statements = statements };
    }

    private Statement CreateTCOExit(ReturnStatement ret)
    {
        // Set control variable to false and return
        var setFalse = new AssignmentStatement
        {
            LValue = new VarRefExp { VarName = "_tco_continue" },
            RValue = new BooleanLiteralExp { Value = false }
        };

        return new BlockStatement
        {
            Statements = new List<Statement> { setFalse, ret }
        };
    }

    private Expression GetDefaultValueForType(FifthType? type)
    {
        if (type == null)
            return new Int32LiteralExp { Value = 0 };

        // Only handle TType - other FifthType variants default to 0
        if (type is not FifthType.TType ttype)
            return new Int32LiteralExp { Value = 0 };

        var typeName = ttype.Name.Value.ToLowerInvariant();
        return typeName switch
        {
            "int" => new Int32LiteralExp { Value = 0 },
            "bool" => new BooleanLiteralExp { Value = false },
            "string" => new StringLiteralExp { Value = "" },
            _ => new Int32LiteralExp { Value = 0 }
        };
    }
}
