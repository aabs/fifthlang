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
            if (expStmt.RHS is FuncCallExp funcCall && funcCall.FunctionDef != null)
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

            // Pop expression result to keep stack balanced (but not for void expressions)
            if (!isVoid)
            {
                seq.Add(new StackInstruction("pop"));
            }
        }
    }

    private void GenerateAssignment(InstructionSequence seq, AssignmentStatement assignStmt)
    {
        // Handle member access assignments (e.g., obj.Property = value)
        if (assignStmt.LValue is MemberAccessExp memberAccess)
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
