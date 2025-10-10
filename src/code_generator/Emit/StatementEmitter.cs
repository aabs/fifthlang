using ast;
using ast_model.TypeSystem;
using il_ast;

namespace code_generator.Emit;

/// <summary>
/// Generates IL instruction sequences for Fifth AST statements.
/// Handles variable declarations, assignments, returns, and control flow statements.
/// </summary>
public class StatementEmitter
{
    private readonly EmitContext _context;
    private readonly ExpressionEmitter _expressionEmitter;
    private readonly ControlFlowEmitter _controlFlowEmitter;

    public StatementEmitter(
        EmitContext context,
        ExpressionEmitter expressionEmitter,
        ControlFlowEmitter controlFlowEmitter)
    {
        _context = context;
        _expressionEmitter = expressionEmitter;
        _controlFlowEmitter = controlFlowEmitter;
    }

    /// <summary>
    /// Generates instruction sequence for a single statement node
    /// </summary>
    public InstructionSequence GenerateStatement(Statement? statement)
    {
        var seq = new InstructionSequence();
        
        if (statement == null)
        {
            return seq;
        }

        switch (statement)
        {
            case VarDeclStatement varDeclStmt:
                GenerateVarDecl(seq, varDeclStmt);
                break;

            case ExpStatement expStmt:
                GenerateExpStatement(seq, expStmt);
                break;

            case AssignmentStatement assignStmt:
                GenerateAssignment(seq, assignStmt);
                break;

            case ReturnStatement retStmt:
                GenerateReturn(seq, retStmt);
                break;

            case IfElseStatement ifStmt:
                seq.AddRange(_controlFlowEmitter.GenerateIfStatement(ifStmt).Instructions);
                break;

            case WhileStatement whileStmt:
                seq.AddRange(_controlFlowEmitter.GenerateWhileStatement(whileStmt).Instructions);
                break;

            default:
                // Unhandled statements are no-ops
                break;
        }

        return seq;
    }

    private void GenerateVarDecl(InstructionSequence seq, VarDeclStatement varDeclStmt)
    {
        var varName = varDeclStmt.VariableDecl?.Name ?? "__var";
        
        if (varDeclStmt.InitialValue != null)
        {
            // Generate initialization expression
            var initSeq = _expressionEmitter.GenerateExpression(varDeclStmt.InitialValue);
            seq.AddRange(initSeq.Instructions);
            
            // Store to local variable
            seq.Add(new StoreInstruction("stloc", varName));
            
            // Record type for later inference
            var typeName = varDeclStmt.VariableDecl?.TypeName.ToString();
            var mappedType = TypeMapper.MapBuiltinFifthTypeNameToSystem(typeName);
            if (mappedType != null)
            {
                _context.LocalVariableTypes[varName] = mappedType;
            }
        }
    }

    private void GenerateExpStatement(InstructionSequence seq, ExpStatement expStmt)
    {
        if (expStmt.RHS != null)
        {
            var exprSeq = _expressionEmitter.GenerateExpression(expStmt.RHS);
            seq.AddRange(exprSeq.Instructions);
            
            // Only pop if the expression produces a value (not void)
            bool isVoid = false;
            
            // Check if it's a function call with void return type
            if (expStmt.RHS is FuncCallExp funcCall)
            {
                // Check Fifth functions
                if (funcCall.FunctionDef != null)
                {
                    var returnType = funcCall.FunctionDef.ReturnType;
                    if (returnType != null)
                    {
                        // Check if it's TVoidType or if the Name is "void"
                        isVoid = returnType is FifthType.TVoidType ||
                                string.Equals(returnType.Name.ToString(), "void", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(returnType.Name.ToString(), "System.Void", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(returnType.Name.ToString(), "Void", StringComparison.OrdinalIgnoreCase);
                    }
                }
                // Check external calls by looking at the generated CallInstruction
                else if (exprSeq.Instructions.Count > 0)
                {
                    var lastInst = exprSeq.Instructions[exprSeq.Instructions.Count - 1];
                    if (lastInst is CallInstruction callInst && callInst.MethodSignature != null)
                    {
                        // Check if signature indicates void return (e.g., "Return=System.Void" in extcall signature)
                        var sig = callInst.MethodSignature;
                        if (sig.Contains("Return=System.Void") || sig.Contains("Return=Void") ||
                            sig.StartsWith("void ", StringComparison.OrdinalIgnoreCase))
                        {
                            isVoid = true;
                        }
                    }
                }
            }
            // Also check by examining the last generated instruction for any expression
            // (e.g., MemberAccessExp that results in external call)
            else if (exprSeq.Instructions.Count > 0)
            {
                var lastInst = exprSeq.Instructions[exprSeq.Instructions.Count - 1];
                if (lastInst is CallInstruction callInst && callInst.MethodSignature != null)
                {
                    var sig = callInst.MethodSignature;
                    if (sig.Contains("Return=System.Void") || sig.Contains("Return=Void") ||
                        sig.StartsWith("void ", StringComparison.OrdinalIgnoreCase))
                    {
                        isVoid = true;
                    }
                }
            }
            
            // Pop expression result to keep stack balanced (but not for void expressions)
            if (!isVoid)
            {
                seq.Add(new StackInstruction("pop"));
            }
        }
    }

    private void GenerateAssignment(InstructionSequence seq, AssignmentStatement assignStmt)
    {
        // Handle indexer assignments (e.g., arr[i] = value)
        if (assignStmt.LValue is IndexerExpression indexer)
        {
            // Load the array/list reference
            var targetSeq = _expressionEmitter.GenerateExpression(indexer.IndexExpression);
            seq.AddRange(targetSeq.Instructions);
            
            // Load the index
            var offsetSeq = _expressionEmitter.GenerateExpression(indexer.OffsetExpression);
            seq.AddRange(offsetSeq.Instructions);
            
            // Generate RValue expression
            if (assignStmt.RValue != null)
            {
                var rvalueSeq = _expressionEmitter.GenerateExpression(assignStmt.RValue);
                seq.AddRange(rvalueSeq.Instructions);
            }
            
            // Infer element type and emit appropriate stelem instruction
            var elementTypeName = InferArrayElementType(indexer.IndexExpression);
            var stelemOpcode = GetStoreElementOpcode(elementTypeName);
            seq.Add(new StoreInstruction(stelemOpcode, null));
        }
        // Handle member access assignments (e.g., obj.Property = value)
        else if (assignStmt.LValue is MemberAccessExp memberAccess)
        {
            // Load the object reference
            if (memberAccess.LHS != null)
            {
                var lhsSeq = _expressionEmitter.GenerateExpression(memberAccess.LHS);
                seq.AddRange(lhsSeq.Instructions);
            }
            
            // Generate RValue expression
            if (assignStmt.RValue != null)
            {
                var rvalueSeq = _expressionEmitter.GenerateExpression(assignStmt.RValue);
                seq.AddRange(rvalueSeq.Instructions);
            }
            
            // Store to the field
            if (memberAccess.RHS is VarRefExp memberRef)
            {
                seq.Add(new StoreInstruction("stfld", memberRef.VarName));
            }
        }
        // Handle simple variable assignments (e.g., x = value)
        else if (assignStmt.LValue is VarRefExp varRef)
        {
            // Generate RValue expression
            if (assignStmt.RValue != null)
            {
                var rvalueSeq = _expressionEmitter.GenerateExpression(assignStmt.RValue);
                seq.AddRange(rvalueSeq.Instructions);
            }
            
            seq.Add(new StoreInstruction("stloc", varRef.VarName));
        }
    }

    /// <summary>
    /// Infers the element type of an array expression.
    /// </summary>
    private string InferArrayElementType(Expression? arrayExpr)
    {
        if (arrayExpr?.Type is ast_model.TypeSystem.FifthType.TArrayOf arrayType)
        {
            return GetTypeNameFromFifthType(arrayType.ElementType);
        }
        if (arrayExpr?.Type is ast_model.TypeSystem.FifthType.TListOf listType)
        {
            return GetTypeNameFromFifthType(listType.ElementType);
        }
        return "System.Int32"; // Default fallback
    }

    /// <summary>
    /// Gets the appropriate .NET type name from a FifthType.
    /// </summary>
    private string GetTypeNameFromFifthType(ast_model.TypeSystem.FifthType fifthType)
    {
        if (fifthType is ast_model.TypeSystem.FifthType.TDotnetType dotnetType)
        {
            return dotnetType.TheType.FullName ?? dotnetType.TheType.Name;
        }
        // Map Fifth type names to .NET types
        return fifthType switch
        {
            ast_model.TypeSystem.FifthType.TType ttype when ttype.Name.Value == "int" => "System.Int32",
            ast_model.TypeSystem.FifthType.TType ttype when ttype.Name.Value == "long" => "System.Int64",
            ast_model.TypeSystem.FifthType.TType ttype when ttype.Name.Value == "float" => "System.Single",
            ast_model.TypeSystem.FifthType.TType ttype when ttype.Name.Value == "double" => "System.Double",
            _ => "System.Object"
        };
    }

    /// <summary>
    /// Gets the appropriate stelem opcode for the given type.
    /// </summary>
    private string GetStoreElementOpcode(string typeName)
    {
        return typeName switch
        {
            "System.Int32" => "stelem.i4",
            "System.Int64" => "stelem.i8",
            "System.Single" => "stelem.r4",
            "System.Double" => "stelem.r8",
            "System.IntPtr" => "stelem.i",
            "System.Byte" => "stelem.i1",
            "System.Int16" => "stelem.i2",
            _ => "stelem.ref" // Reference types and other types
        };
    }

    private void GenerateReturn(InstructionSequence seq, ReturnStatement retStmt)
    {
        // Generate return value expression if present
        if (retStmt.ReturnValue != null)
        {
            var returnSeq = _expressionEmitter.GenerateExpression(retStmt.ReturnValue);
            seq.AddRange(returnSeq.Instructions);
        }
        
        // Emit return instruction
        seq.Add(new ReturnInstruction());
    }
}
