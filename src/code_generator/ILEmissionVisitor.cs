using il_ast;
using il_ast_generated;
using System.Text;

namespace code_generator;

/// <summary>
/// Visitor that emits well-formed IL code from IL metamodel structures.
/// Follows best practices for IL generation and conforms to ECMA-335 standard.
/// </summary>
public class ILEmissionVisitor : DefaultRecursiveDescentVisitor
{
    private readonly StringBuilder _output = new();
    private int _indentLevel = 0;
    private const string IndentString = "    ";

    public string EmitAssembly(AssemblyDeclaration assembly)
    {
        _output.Clear();
        _indentLevel = 0;

        // Emit assembly header
        EmitLine($"// Generated IL for assembly: {assembly.Name}");
        EmitLine($"// Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        EmitLine();

        // Emit assembly references first
        foreach (var reference in assembly.AssemblyReferences)
        {
            EmitAssemblyReference(reference);
        }

        // Emit assembly declaration
        var assemblyName = string.IsNullOrWhiteSpace(assembly.Name) ? "FifthProgram" : assembly.Name;
        EmitLine($".assembly {assemblyName}");
        EmitLine("{");
        IncreaseIndent();
        EmitLine($".ver {assembly.Version.Major}:{assembly.Version.Minor}:{assembly.Version.Build}:{assembly.Version.Patch}");
        DecreaseIndent();
        EmitLine("}");
        EmitLine();

        // Emit module
        if (assembly.PrimeModule != null)
        {
            EmitModule(assembly.PrimeModule);
        }

        return _output.ToString();
    }

    private void EmitAssemblyReference(AssemblyReference reference)
    {
        EmitLine($".assembly extern {reference.Name}");
        EmitLine("{");
        IncreaseIndent();
        EmitLine($".publickeytoken = ({reference.PublicKeyToken})");
        EmitLine($".ver {reference.Version.Major}:{reference.Version.Minor}:{reference.Version.Build}:{reference.Version.Patch}");
        DecreaseIndent();
        EmitLine("}");
        EmitLine();
    }

    private void EmitModule(ModuleDeclaration module)
    {
        EmitLine($".module {module.FileName}");
        EmitLine();

        // Emit classes
        foreach (var classDefinition in module.Classes)
        {
            EmitClass(classDefinition);
        }

        // Emit top-level functions
        foreach (var method in module.Functions)
        {
            EmitMethod(method, isTopLevel: true);
        }
    }

    private void EmitClass(ClassDefinition classDefinition)
    {
        var visibility = GetVisibilityString(classDefinition.Visibility);
        var fullName = string.IsNullOrEmpty(classDefinition.Namespace) 
            ? classDefinition.Name 
            : $"{classDefinition.Namespace}.{classDefinition.Name}";

        EmitLine($".class {visibility} auto ansi beforefieldinit {fullName}");
        EmitLine("{");
        IncreaseIndent();

        // Emit fields
        foreach (var field in classDefinition.Fields)
        {
            EmitField(field);
        }

        // Emit methods
        foreach (var method in classDefinition.Methods)
        {
            EmitMethod(method, isTopLevel: false);
        }

        DecreaseIndent();
        EmitLine("}");
        EmitLine();
    }

    private void EmitField(FieldDefinition field)
    {
        var visibility = GetVisibilityString(field.Visibility);
        var staticModifier = field.IsStatic ? " static" : "";
        var typeName = GetTypeString(field.TheType);

        EmitLine($".field {visibility}{staticModifier} {typeName} {field.Name}");
    }

    private void EmitMethod(MethodDefinition method, bool isTopLevel)
    {
        var visibility = GetVisibilityString(method.Visibility);
        var staticModifier = method.IsStatic ? " static" : "";
        var returnType = GetTypeString(method.Signature.ReturnTypeSignature);
        var parameters = string.Join(", ", method.Signature.ParameterSignatures.Select(p => 
            $"{GetTypeString(p.TypeReference)} {p.Name}"));

        if (method.Header.IsEntrypoint)
        {
            EmitLine($".method {visibility}{staticModifier} {returnType} {method.Name}({parameters}) cil managed");
            EmitLine("{");
            IncreaseIndent();
            EmitLine(".entrypoint");
            EmitMethodBody(method.Impl.Body);
            DecreaseIndent();
            EmitLine("}");
        }
        else
        {
            EmitLine($".method {visibility}{staticModifier} {returnType} {method.Name}({parameters}) cil managed");
            EmitLine("{");
            IncreaseIndent();
            EmitMethodBody(method.Impl.Body);
            DecreaseIndent();
            EmitLine("}");
        }
        EmitLine();
    }

    private void EmitMethodBody(Block body)
    {
        EmitLine(".maxstack 8"); // Default stack size
        
        if (body.Statements.Any())
        {
            foreach (var statement in body.Statements)
            {
                EmitStatement(statement);
            }
        }
        
        // Ensure method returns properly
        EmitLine("ret");
    }

    private void EmitStatement(InstructionStatement statement)
    {
        // With the new approach, we only handle instruction-level constructs
        EmitInstructionSequence(statement.Instructions);
    }

    /* Removed old high-level statement methods - now using instruction sequences only
    private void EmitVariableDeclaration(VariableDeclarationStatement varDecl)
    {
        // IL uses locals for local variables
        EmitLine($".locals init ({GetTypeNameForLocal(varDecl.TypeName)} {varDecl.Name})");
        
        if (varDecl.InitialisationExpression != null)
        {
            EmitExpression(varDecl.InitialisationExpression);
            EmitLine($"stloc {varDecl.Name}");
        }
    }

    private void EmitVariableAssignment(VariableAssignmentStatement assignment)
    {
        EmitExpression(assignment.RHS);
        EmitLine($"stloc {assignment.LHS}");
    }

    private void EmitReturn(ReturnStatement returnStmt)
    {
        if (returnStmt.Exp != null)
        {
            EmitExpression(returnStmt.Exp);
        }
        EmitLine("ret");
    }

    private void EmitIf(IfStatement ifStmt)
    {
        var falseLabel = $"IL_false_{_labelCounter++}";
        var endLabel = $"IL_end_{_labelCounter++}";
        
        EmitExpression(ifStmt.Conditional);
        EmitLine($"brfalse {falseLabel}");
        
        foreach (var stmt in ifStmt.IfBlock.Statements)
        {
            EmitStatement(stmt);
        }
        
        EmitLine($"br {endLabel}");
        EmitLine($"{falseLabel}:");
        
        if (ifStmt.ElseBlock != null)
        {
            foreach (var stmt in ifStmt.ElseBlock.Statements)
            {
                EmitStatement(stmt);
            }
        }
        
        EmitLine($"{endLabel}:");
    }

    private void EmitWhile(WhileStatement whileStmt)
    {
        var startLabel = $"IL_loop_{_labelCounter++}";
        var endLabel = $"IL_end_{_labelCounter++}";
        
        EmitLine($"{startLabel}:");
        EmitExpression(whileStmt.Conditional);
        EmitLine($"brfalse {endLabel}");
        
        foreach (var stmt in whileStmt.LoopBlock.Statements)
        {
            EmitStatement(stmt);
        }
        
        EmitLine($"br {startLabel}");
        EmitLine($"{endLabel}:");
    }
    */
    {
        // IL uses locals for local variables
        EmitLine($".locals init ({GetTypeNameForLocal(varDecl.TypeName)} {varDecl.Name})");
        
        if (varDecl.InitialisationExpression != null)
        {
            EmitExpression(varDecl.InitialisationExpression);
            EmitLine($"stloc {varDecl.Name}");
        }
    }

    private void EmitVariableAssignment(VariableAssignmentStatement assignment)
    {
        EmitExpression(assignment.RHS);
        EmitLine($"stloc {assignment.LHS}");
    }

    private void EmitReturn(ReturnStatement returnStmt)
    {
        if (returnStmt.Exp != null)
        {
            EmitExpression(returnStmt.Exp);
        }
        EmitLine("ret");
    }

    private void EmitIf(IfStatement ifStmt)
    {
        var falseLabel = $"IL_false_{Guid.NewGuid():N}";
        var endLabel = $"IL_end_{Guid.NewGuid():N}";

        // Emit condition
        EmitExpression(ifStmt.Conditional);
        EmitLine($"brfalse {falseLabel}");

        // Emit if block
        foreach (var stmt in ifStmt.IfBlock.Statements)
        {
            EmitStatement(stmt);
        }
        EmitLine($"br {endLabel}");

        // Emit else block
        EmitLine($"{falseLabel}:");
        if (ifStmt.ElseBlock != null)
        {
            foreach (var stmt in ifStmt.ElseBlock.Statements)
            {
                EmitStatement(stmt);
            }
        }

        EmitLine($"{endLabel}:");
    }

    private void EmitWhile(WhileStatement whileStmt)
    {
        var startLabel = $"IL_loop_{Guid.NewGuid():N}";
        var endLabel = $"IL_end_{Guid.NewGuid():N}";

        EmitLine($"{startLabel}:");
        EmitExpression(whileStmt.Conditional);
        EmitLine($"brfalse {endLabel}");

        foreach (var stmt in whileStmt.LoopBlock.Statements)
        {
            EmitStatement(stmt);
        }

        EmitLine($"br {startLabel}");
        EmitLine($"{endLabel}:");
    }

    private void EmitExpression(ast.Expression expression)
    {
        // TODO: Update to handle ast.Expression types with instruction generation
        throw new NotImplementedException("Expression emission needs to be updated for new instruction-level model");
    }

    /* Removed old expression handling methods - now using instruction sequences only */
    private void EmitInstructionSequence(InstructionSequence sequence)
    {
        foreach (var instruction in sequence.Instructions)
        {
            EmitInstruction(instruction);
        }
    }
    
    /// <summary>
    /// Emits a single CIL instruction
    /// </summary>
    private void EmitInstruction(CilInstruction instruction)
    {
        switch (instruction)
        {
            case LoadInstruction loadInstr:
                EmitLoadInstruction(loadInstr);
                break;
                
            case StoreInstruction storeInstr:
                EmitStoreInstruction(storeInstr);
                break;
                
            case ArithmeticInstruction arithInstr:
                EmitArithmeticInstruction(arithInstr);
                break;
                
            case BranchInstruction branchInstr:
                EmitBranchInstruction(branchInstr);
                break;
                
            case CallInstruction callInstr:
                EmitCallInstruction(callInstr);
                break;
                
            case StackInstruction stackInstr:
                EmitStackInstruction(stackInstr);
                break;
                
            case ReturnInstruction:
                EmitLine("ret");
                break;
                
            case LabelInstruction labelInstr:
                EmitLine($"{labelInstr.Label}:");
                break;
        }
    }
    
    private void EmitLoadInstruction(LoadInstruction loadInstr)
    {
        if (loadInstr.Value != null)
        {
            EmitLine($"{loadInstr.Opcode} {loadInstr.Value}");
        }
        else
        {
            EmitLine(loadInstr.Opcode);
        }
    }
    
    private void EmitStoreInstruction(StoreInstruction storeInstr)
    {
        if (storeInstr.Target != null)
        {
            EmitLine($"{storeInstr.Opcode} {storeInstr.Target}");
        }
        else
        {
            EmitLine(storeInstr.Opcode);
        }
    }
    
    private void EmitArithmeticInstruction(ArithmeticInstruction arithInstr)
    {
        switch (arithInstr.Opcode)
        {
            case "ceq_neg": // Special handling for !=
                EmitLine("ceq");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            case "not": // Special handling for logical not
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            case "cle": // Special handling for <=
                EmitLine("cgt");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            case "cge": // Special handling for >=
                EmitLine("clt");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            default:
                EmitLine(arithInstr.Opcode);
                break;
        }
    }
    
    private void EmitBranchInstruction(BranchInstruction branchInstr)
    {
        if (branchInstr.TargetLabel != null)
        {
            EmitLine($"{branchInstr.Opcode} {branchInstr.TargetLabel}");
        }
        else
        {
            EmitLine(branchInstr.Opcode);
        }
    }
    
    private void EmitCallInstruction(CallInstruction callInstr)
    {
        if (callInstr.MethodSignature != null)
        {
            EmitLine($"{callInstr.Opcode} {callInstr.MethodSignature}");
        }
        else
        {
            EmitLine(callInstr.Opcode);
        }
    }
    
    private void EmitStackInstruction(StackInstruction stackInstr)
    {
        EmitLine(stackInstr.Opcode);
    }
}