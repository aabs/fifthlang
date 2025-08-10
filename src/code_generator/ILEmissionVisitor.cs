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
        EmitLine($".assembly {assembly.Name}");
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

    private void EmitStatement(Statement statement)
    {
        switch (statement)
        {
            case VariableDeclarationStatement varDecl:
                EmitVariableDeclaration(varDecl);
                break;
            case VariableAssignmentStatement assignment:
                EmitVariableAssignment(assignment);
                break;
            case ReturnStatement returnStmt:
                EmitReturn(returnStmt);
                break;
            case IfStatement ifStmt:
                EmitIf(ifStmt);
                break;
            case WhileStatement whileStmt:
                EmitWhile(whileStmt);
                break;
            case ExpressionStatement exprStmt:
                EmitExpression(exprStmt.Expression);
                EmitLine("pop"); // Pop result if not used
                break;
        }
    }

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

    private void EmitExpression(il_ast.Expression expression)
    {
        switch (expression)
        {
            case IntLiteral intLit:
                EmitLine($"ldc.i4 {intLit.Value}");
                break;
            case FloatLiteral floatLit:
                EmitLine($"ldc.r4 {floatLit.Value}");
                break;
            case DoubleLiteral doubleLit:
                EmitLine($"ldc.r8 {doubleLit.Value}");
                break;
            case StringLiteral stringLit:
                EmitLine($"ldstr \"{EscapeString(stringLit.Value)}\"");
                break;
            case BoolLiteral boolLit:
                EmitLine(boolLit.Value ? "ldc.i4.1" : "ldc.i4.0");
                break;
            case VariableReferenceExpression varRef:
                EmitLine($"ldloc {varRef.Name}");
                break;
            case BinaryExpression binaryExp:
                EmitBinaryExpression(binaryExp);
                break;
            case UnaryExpression unaryExp:
                EmitUnaryExpression(unaryExp);
                break;
            case FuncCallExp funcCall:
                EmitFunctionCall(funcCall);
                break;
        }
    }

    private void EmitBinaryExpression(BinaryExpression binaryExp)
    {
        EmitExpression(binaryExp.LHS);
        EmitExpression(binaryExp.RHS);

        switch (binaryExp.Op)
        {
            case "+":
                EmitLine("add");
                break;
            case "-":
                EmitLine("sub");
                break;
            case "*":
                EmitLine("mul");
                break;
            case "/":
                EmitLine("div");
                break;
            case "==":
                EmitLine("ceq");
                break;
            case "!=":
                EmitLine("ceq");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            case "<":
                EmitLine("clt");
                break;
            case ">":
                EmitLine("cgt");
                break;
            case "<=":
                EmitLine("cgt");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            case ">=":
                EmitLine("clt");
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
            default:
                EmitLine("add"); // Default fallback
                break;
        }
    }

    private void EmitUnaryExpression(UnaryExpression unaryExp)
    {
        EmitExpression(unaryExp.Exp);

        switch (unaryExp.Op)
        {
            case "-":
                EmitLine("neg");
                break;
            case "!":
                EmitLine("ldc.i4.0");
                EmitLine("ceq");
                break;
        }
    }

    private void EmitFunctionCall(FuncCallExp funcCall)
    {
        // Emit arguments
        foreach (var arg in funcCall.Args)
        {
            EmitExpression(arg);
        }

        // Emit call - using a simple pattern for system calls
        if (funcCall.Name == "print" || funcCall.Name == "myprint")
        {
            EmitLine("call void [System.Console]System.Console::WriteLine(object)");
        }
        else
        {
            // Generic method call
            EmitLine($"call {funcCall.ReturnType} {funcCall.Name}({string.Join(", ", funcCall.ArgTypes)})");
        }
    }

    private string GetVisibilityString(MemberAccessability visibility)
    {
        return visibility switch
        {
            MemberAccessability.Public => "public",
            MemberAccessability.Private => "private",
            MemberAccessability.Family => "family", 
            MemberAccessability.Assem => "assembly",
            _ => "public"
        };
    }

    private string GetTypeString(TypeReference typeRef)
    {
        if (typeRef.Namespace == "System")
        {
            return typeRef.Name switch
            {
                "Int32" => "int32",
                "String" => "string",
                "Single" => "float32",
                "Double" => "float64",
                "Boolean" => "bool",
                "Void" => "void",
                _ => $"[System.Runtime]{typeRef.Namespace}.{typeRef.Name}"
            };
        }
        
        return $"{typeRef.Namespace}.{typeRef.Name}";
    }

    private string GetTypeNameForLocal(string typeName)
    {
        return typeName switch
        {
            "int" => "int32",
            "string" => "string",
            "float" => "float32",
            "double" => "float64",
            "bool" => "bool",
            _ => typeName
        };
    }

    private string EscapeString(string input)
    {
        return input?.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") ?? "";
    }

    private void EmitLine(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            _output.AppendLine();
        }
        else
        {
            _output.AppendLine(GetIndent() + line);
        }
    }

    private string GetIndent()
    {
        return string.Concat(Enumerable.Repeat(IndentString, _indentLevel));
    }

    private void IncreaseIndent()
    {
        _indentLevel++;
    }

    private void DecreaseIndent()
    {
        _indentLevel = Math.Max(0, _indentLevel - 1);
    }
}