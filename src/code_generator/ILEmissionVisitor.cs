using ast;
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

    private Dictionary<string, string>? _currentLocalsMap = null;
    private Dictionary<string, int>? _currentParamsMap = null;

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
            EmitMethodBody(method.Impl.Body, method);
            DecreaseIndent();
            EmitLine("}");
        }
        else
        {
            EmitLine($".method {visibility}{staticModifier} {returnType} {method.Name}({parameters}) cil managed");
            EmitLine("{");
            IncreaseIndent();
            EmitMethodBody(method.Impl.Body, method);
            DecreaseIndent();
            EmitLine("}");
        }
        EmitLine();
    }

    private void EmitMethodBody(Block body, MethodDefinition method)
    {
        EmitLine(".maxstack 8"); // Default stack size

        // Use AstToIlTransformationVisitor to convert high-level statements to instruction sequences
        var transformer = new AstToIlTransformationVisitor();
        
        // Set the current method parameters so the transformer knows which variables are parameters
        var paramNames = method.Signature.ParameterSignatures.Select(p => p.Name ?? "param").ToList();
        transformer.SetCurrentParameters(paramNames);
        
        // Set parameter types for better type inference
        var paramInfos = method.Signature.ParameterSignatures
            .Select(p => (name: p.Name ?? "param", typeName: p.TypeReference != null ? $"{p.TypeReference.Namespace}.{p.TypeReference.Name}" : null))
            .ToList();
        transformer.SetCurrentParameterTypes(paramInfos);

        var allSequences = new List<InstructionSequence>();
        if (body.Statements.Any())
        {
            foreach (var statement in body.Statements)
            {
                var instructionSequence = transformer.GenerateStatement(statement);
                allSequences.Add(instructionSequence);
            }
        }

        // Collect local variable names used by ldloc/stloc so we can declare them and infer their types
        var localNames = new List<string>();
        var localTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var seq in allSequences)
        {
            for (int i = 0; i < seq.Instructions.Count; i++)
            {
                var ins = seq.Instructions[i];
                if (ins is StoreInstruction st && !string.IsNullOrEmpty(st.Target))
                {
                    if (!localNames.Contains(st.Target)) localNames.Add(st.Target);

                    // Try to infer the local's type from immediately preceding call instruction's return signature
                    if (i > 0 && seq.Instructions[i - 1] is CallInstruction prevCall && !string.IsNullOrEmpty(prevCall.MethodSignature) && prevCall.MethodSignature.StartsWith("extcall:", StringComparison.Ordinal))
                    {
                        try
                        {
                            // ParseExtCallSignature returns full IL call signature; return type is before first space
                            var ilSig = ParseExtCallSignature(prevCall.MethodSignature);
                            var firstSpace = ilSig.IndexOf(' ');
                            var returnIlType = firstSpace > 0 ? ilSig.Substring(0, firstSpace) : "class [System.Runtime]System.Object";
                            localTypes[st.Target] = returnIlType;
                        }
                        catch
                        {
                            // Fallback silently to object
                            localTypes[st.Target] = "class [System.Runtime]System.Object";
                        }
                    }
                }
                else if (ins is LoadInstruction ld && ld.Value is string s && !string.IsNullOrEmpty(s))
                {
                    // ldloc usage may appear as a string value
                    if (ld.Opcode.StartsWith("ldloc", StringComparison.OrdinalIgnoreCase) && !localNames.Contains(s))
                    {
                        localNames.Add(s);
                    }
                }
            }
        }

        // Build map to IL local names V_0, V_1, ... and emit .locals init if any
        _currentLocalsMap = null;
        if (localNames.Count > 0)
        {
            _currentLocalsMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var localsDecls = new List<string>();
            for (int i = 0; i < localNames.Count; i++)
            {
                var original = localNames[i];
                var localId = "V_" + i;
                _currentLocalsMap[original] = localId;
                // Prefer inferred local type when available, otherwise fallback to System.Object
                var ilLocalType = localTypes.TryGetValue(original, out var lt) ? lt : "class [System.Runtime]System.Object";
                localsDecls.Add($"{ilLocalType} {localId}");
            }

            EmitLine($".locals init ({string.Join(", ", localsDecls)})");
        }

        // Build map from parameter names to indices for ldarg instructions
        _currentParamsMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < paramNames.Count; i++)
        {
            _currentParamsMap[paramNames[i]] = i;
        }

        // Emit instructions
        foreach (var seq in allSequences)
        {
            EmitInstructionSequence(seq);
        }

        // Ensure method returns properly: only emit a trailing 'ret' if none of the
        // generated instruction sequences already contain a ReturnInstruction.
        // Emitting an unconditional 'ret' can create duplicate return instructions
        // and produce invalid IL when a ReturnStatement already emitted one.
        var hasExplicitReturn = allSequences
            .SelectMany(s => s.Instructions)
            .Any(i => i is il_ast.ReturnInstruction);
        if (!hasExplicitReturn)
        {
            EmitLine("ret");
        }

        _currentLocalsMap = null;
        _currentParamsMap = null;
    }

    private void EmitInstructionSequence(InstructionSequence sequence)
    {
        foreach (var instruction in sequence.Instructions)
        {
            EmitInstruction(instruction);
        }
    }

    /* TODO: The following methods reference removed il_ast high-level types and need to be refactored
       Commenting out for now to allow build to succeed
    
    /*
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

    private void EmitExpression(ast.Expression expression)
    {
        // TODO: Update to handle ast.Expression types with instruction generation
        throw new NotImplementedException("Expression emission needs to be updated for new instruction-level model");
    }
    */
    /* Removed old expression handling methods - now using instruction sequences only */


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
            // If this is a local load and we have a mapping, rewrite the local name
            if (_currentLocalsMap != null && loadInstr.Opcode.StartsWith("ldloc", StringComparison.OrdinalIgnoreCase) && loadInstr.Value is string name && _currentLocalsMap.TryGetValue(name, out var mapped))
            {
                EmitLine($"{loadInstr.Opcode} {mapped}");
                return;
            }
            
            // If this is an argument load and we have a mapping, rewrite with the parameter index
            if (_currentParamsMap != null && loadInstr.Opcode.StartsWith("ldarg", StringComparison.OrdinalIgnoreCase) && loadInstr.Value is string paramName && _currentParamsMap.TryGetValue(paramName, out var paramIndex))
            {
                EmitLine($"{loadInstr.Opcode}.{paramIndex}");
                return;
            }

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
            if (_currentLocalsMap != null && _currentLocalsMap.TryGetValue(storeInstr.Target, out var mapped))
            {
                EmitLine($"{storeInstr.Opcode} {mapped}");
                return;
            }
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
        if (!string.IsNullOrWhiteSpace(callInstr.MethodSignature) && callInstr.MethodSignature.StartsWith("extcall:", StringComparison.Ordinal))
        {
            var ilSig = ParseExtCallSignature(callInstr.MethodSignature);
            EmitLine($"{callInstr.Opcode} {ilSig}");
            return;
        }

        if (callInstr.MethodSignature != null)
        {
            EmitLine($"{callInstr.Opcode} {callInstr.MethodSignature}");
        }
        else
        {
            EmitLine(callInstr.Opcode);
        }
    }

    // Parse an extcall token generated by AstToIlTransformationVisitor and produce a valid IL call signature
    private string ParseExtCallSignature(string sig)
    {
        // sig format: extcall:Asm=Fifth.System;Ns=Fifth.System;Type=KG;Method=CreateGraph;Params=;Return=VDS.RDF.IGraph@dotNetRDF
        var payload = sig.Substring("extcall:".Length);
        var parts = payload.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = parts.Select(p => p.Split('=', 2)).Where(a => a.Length == 2).ToDictionary(a => a[0], a => a[1]);

        string asm = dict.TryGetValue("Asm", out var a) ? a : string.Empty;
        string ns = dict.TryGetValue("Ns", out var n) ? n : string.Empty;
        string type = dict.TryGetValue("Type", out var t) ? t : string.Empty;
        string method = dict.TryGetValue("Method", out var m) ? m : string.Empty;
        string @params = dict.TryGetValue("Params", out var p) ? p : string.Empty;
        string @return = dict.TryGetValue("Return", out var r) ? r : "System.Void";

        string MapTokenToIlType(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "void";
            // Token can be 'Namespace.Type@Assembly' or 'System.Int32' etc.
            var asmSplit = token.Split('@', 2);
            var typeName = asmSplit[0];
            var asmName = asmSplit.Length > 1 ? asmSplit[1] : string.Empty;

            // Map common system types
            switch (typeName)
            {
                case "System.Int32": return "int32";
                case "System.String": return "string";
                case "System.Single": return "float32";
                case "System.Double": return "float64";
                case "System.Boolean": return "bool";
                case "System.Void": return "void";
                case "System.Object": return "class [System.Runtime]System.Object";
                case "System.Int64": return "int64";
                case "System.Int16": return "int16";
                case "System.SByte": return "int8";
                case "System.Byte": return "uint8";
                case "System.UInt16": return "uint16";
                case "System.UInt32": return "uint32";
                case "System.UInt64": return "uint64";
                case "System.Decimal": return "valuetype [System.Runtime]System.Decimal"; // best-effort
            }

            // For non-system types, emit a class token with assembly first and no extra spaces:
            // 'class [Assembly]Namespace.Type'
            if (!string.IsNullOrEmpty(asmName))
            {
                return $"class [{asmName}]{typeName}";
            }
            return $"class {typeName}";
        }

        // Build IL param list
        var paramList = "";
        if (!string.IsNullOrWhiteSpace(@params))
        {
            var paramTokens = @params.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var mapped = paramTokens.Select(MapTokenToIlType).ToArray();
            paramList = string.Join(", ", mapped);
        }

        var returnIl = MapTokenToIlType(@return);
        // Normalize System.Object to reference the runtime assembly explicitly
        if (string.Equals(returnIl, "class System.Object", StringComparison.OrdinalIgnoreCase) || string.Equals(returnIl, "System.Object", StringComparison.OrdinalIgnoreCase))
        {
            returnIl = "class [System.Runtime]System.Object";
        }

        // Construct owner token: [Asm]Ns.Type (no extra space after the assembly token)
        var owner = string.Empty;
        if (!string.IsNullOrEmpty(asm))
        {
            if (!string.IsNullOrEmpty(ns)) owner = $"[{asm}]{ns}.{type}";
            else owner = $"[{asm}]{type}";
        }
        else
        {
            owner = !string.IsNullOrEmpty(ns) ? $"{ns}.{type}" : type;
        }

        // Compose final call signature
        var paramSegment = string.IsNullOrEmpty(paramList) ? "" : paramList;
        return $"{returnIl} {owner}::{method}({paramSegment})";
    }

    private void EmitStackInstruction(StackInstruction stackInstr)
    {
        EmitLine(stackInstr.Opcode);
    }

    // End of commented section - these methods need refactoring to work with new il_ast model */

    #region Helper Methods

    private void EmitLine(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            _output.AppendLine();
        }
        else
        {
            _output.AppendLine(new string(' ', _indentLevel * IndentString.Length) + line);
        }
    }

    private void IncreaseIndent()
    {
        _indentLevel++;
    }

    private void DecreaseIndent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
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
            MemberAccessability.FamANDAssem => "famandassem",
            MemberAccessability.FamORAssem => "famorassem",
            MemberAccessability.CompilerControlled => "privatescope",
            _ => "private"
        };
    }

    private string GetTypeString(TypeReference typeRef)
    {
        // Map common types to IL types
        return typeRef.Name switch
        {
            "int" or "Int32" => "int32",
            "float" or "Single" => "float32",
            "double" or "Double" => "float64",
            "bool" or "Boolean" => "bool",
            "string" or "String" => "string",
            "void" => "void",
            "sbyte" or "SByte" or "int8" or "Int8" => "int8",
            "byte" or "Byte" => "uint8",
            "uint" or "UInt32" or "uint32" => "uint32",
            "ulong" or "UInt64" or "uint64" => "uint64",
            "ushort" or "UInt16" or "uint16" => "uint16",
            _ => typeRef.Name  // Preserve original casing for custom types
        };
    }

    #endregion
}