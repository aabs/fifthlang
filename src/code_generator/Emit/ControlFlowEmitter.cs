using ast;
using ast_model.TypeSystem;
using il_ast;

namespace code_generator.Emit;

/// <summary>
/// Generates IL instruction sequences for control flow statements (if/else, while loops).
/// Manages label generation and branch instructions.
/// </summary>
public class ControlFlowEmitter
{
    private readonly EmitContext _context;
    private readonly ExpressionEmitter _expressionEmitter;

    public ControlFlowEmitter(EmitContext context, ExpressionEmitter expressionEmitter)
    {
        _context = context;
        _expressionEmitter = expressionEmitter;
    }

    /// <summary>
    /// Generates IL instructions for an if/else statement
    /// </summary>
    public InstructionSequence GenerateIfStatement(IfElseStatement? ifStmt)
    {
        var seq = new InstructionSequence();
        
        if (ifStmt == null)
        {
            return seq;
        }

        var falseLabel = _context.GenerateLabel("IL_false");
        var endLabel = _context.GenerateLabel("IL_end");

        // Evaluate condition
        if (ifStmt.Condition != null)
        {
            var conditionSeq = _expressionEmitter.GenerateExpression(ifStmt.Condition);
            seq.AddRange(conditionSeq.Instructions);
        }

        // Branch to false label if condition is false
        seq.Add(new BranchInstruction("brfalse", falseLabel));

        // Then block
        if (ifStmt.ThenBlock?.Statements != null)
        {
            foreach (var statement in ifStmt.ThenBlock.Statements)
            {
                var stmtSeq = GenerateStatementForControlFlow(statement);
                seq.AddRange(stmtSeq.Instructions);
            }
        }

        // Only branch to end if the then block doesn't already terminate (e.g., with return)
        bool thenBlockTerminates = seq.Instructions.Count > 0 && 
            seq.Instructions[seq.Instructions.Count - 1] is ReturnInstruction;
        
        if (!thenBlockTerminates)
        {
            seq.Add(new BranchInstruction("br", endLabel));
        }
        seq.Add(new LabelInstruction(falseLabel));

        // Else block
        if (ifStmt.ElseBlock?.Statements != null)
        {
            foreach (var statement in ifStmt.ElseBlock.Statements)
            {
                var stmtSeq = GenerateStatementForControlFlow(statement);
                seq.AddRange(stmtSeq.Instructions);
            }
        }

        // Only add end label if it's reachable (i.e., the then block didn't terminate)
        // If both blocks terminate, the end label is unreachable and shouldn't be emitted
        bool elseBlockTerminates = false;
        for (int i = seq.Instructions.Count - 1; i >= 0; i--)
        {
            if (seq.Instructions[i] is LabelInstruction label && label.Label == falseLabel)
            {
                // Found the start of else block, check if anything after it is a return
                for (int j = i + 1; j < seq.Instructions.Count; j++)
                {
                    if (seq.Instructions[j] is ReturnInstruction)
                    {
                        elseBlockTerminates = true;
                        break;
                    }
                }
                break;
            }
        }
        
        // Only emit end label if at least one path doesn't terminate
        if (!thenBlockTerminates || !elseBlockTerminates)
        {
            seq.Add(new LabelInstruction(endLabel));
        }

        return seq;
    }

    /// <summary>
    /// Generates IL instructions for a while loop
    /// </summary>
    public InstructionSequence GenerateWhileStatement(WhileStatement? whileStmt)
    {
        var seq = new InstructionSequence();
        
        if (whileStmt == null)
        {
            return seq;
        }

        var startLabel = _context.GenerateLabel("IL_while_start");
        var endLabel = _context.GenerateLabel("IL_while_end");

        // Start of loop label
        seq.Add(new LabelInstruction(startLabel));

        // Evaluate condition
        if (whileStmt.Condition != null)
        {
            var conditionSeq = _expressionEmitter.GenerateExpression(whileStmt.Condition);
            seq.AddRange(conditionSeq.Instructions);
        }

        // Exit loop if condition is false
        seq.Add(new BranchInstruction("brfalse", endLabel));

        // Loop body
        if (whileStmt.Body?.Statements != null)
        {
            foreach (var statement in whileStmt.Body.Statements)
            {
                var stmtSeq = GenerateStatementForControlFlow(statement);
                seq.AddRange(stmtSeq.Instructions);
            }
        }

        // Jump back to start
        seq.Add(new BranchInstruction("br", startLabel));

        // End label
        seq.Add(new LabelInstruction(endLabel));

        return seq;
    }

    /// <summary>
    /// Helper method to generate statements within control flow blocks.
    /// Uses a simple delegation pattern to avoid circular dependencies with StatementEmitter.
    /// The full integration will wire up the StatementEmitter properly.
    /// </summary>
    private InstructionSequence GenerateStatementForControlFlow(Statement statement)
    {
        // This is a simplified version. In the final integration, this will delegate
        // to the StatementEmitter that is passed in during construction or via a property.
        // For now, we handle the basic cases directly to avoid circular dependencies.
        var seq = new InstructionSequence();

        switch (statement)
        {
            case VarDeclStatement varDecl:
                if (varDecl.InitialValue != null)
                {
                    seq.AddRange(_expressionEmitter.GenerateExpression(varDecl.InitialValue).Instructions);
                    seq.Add(new StoreInstruction("stloc", varDecl.VariableDecl?.Name ?? "__var"));
                }
                break;

            case ExpStatement expStmt:
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
                        // Check external calls by examining the generated instructions
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
                    }
                    
                    if (!isVoid)
                    {
                        seq.Add(new StackInstruction("pop"));
                    }
                }
                break;

            case AssignmentStatement assignStmt:
                if (assignStmt.RValue != null)
                {
                    seq.AddRange(_expressionEmitter.GenerateExpression(assignStmt.RValue).Instructions);
                }
                if (assignStmt.LValue is VarRefExp varRef)
                {
                    seq.Add(new StoreInstruction("stloc", varRef.VarName));
                }
                break;

            case ReturnStatement retStmt:
                if (retStmt.ReturnValue != null)
                {
                    seq.AddRange(_expressionEmitter.GenerateExpression(retStmt.ReturnValue).Instructions);
                }
                seq.Add(new ReturnInstruction());
                break;

            case IfElseStatement nestedIf:
                seq.AddRange(GenerateIfStatement(nestedIf).Instructions);
                break;

            case WhileStatement nestedWhile:
                seq.AddRange(GenerateWhileStatement(nestedWhile).Instructions);
                break;
        }

        return seq;
    }
}
