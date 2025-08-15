using ast;
using il_ast;
using il_ast_generated;

namespace code_generator;

/// <summary>
/// Transforms AST nodes from the Fifth language parser into IL metamodel representations.
/// This visitor follows the transformation stage pattern used in FifthParserManager.ApplyLanguageAnalysisPhases.
/// </summary>
public class AstToIlTransformationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly Dictionary<string, TypeReference> _typeMap = new();
    private AssemblyDeclaration? _currentAssembly;
    private ModuleDeclaration? _currentModule;
    private ClassDefinition? _currentClass;
    
    public AstToIlTransformationVisitor()
    {
        InitializeBuiltinTypes();
    }
    
    private void InitializeBuiltinTypes()
    {
        // Map Fifth language type names to System types
        _typeMap["int"] = new TypeReference { Namespace = "System", Name = "Int32" };
        _typeMap["string"] = new TypeReference { Namespace = "System", Name = "String" };
        _typeMap["float"] = new TypeReference { Namespace = "System", Name = "Single" };
        _typeMap["double"] = new TypeReference { Namespace = "System", Name = "Double" };
        _typeMap["bool"] = new TypeReference { Namespace = "System", Name = "Boolean" };
        _typeMap["void"] = new TypeReference { Namespace = "System", Name = "Void" };
        
        // Also map .NET type names to System types (for cases where AST contains .NET type names)
        _typeMap["Int32"] = new TypeReference { Namespace = "System", Name = "Int32" };
        _typeMap["String"] = new TypeReference { Namespace = "System", Name = "String" };
        _typeMap["Single"] = new TypeReference { Namespace = "System", Name = "Single" };
        _typeMap["Double"] = new TypeReference { Namespace = "System", Name = "Double" };
        _typeMap["Boolean"] = new TypeReference { Namespace = "System", Name = "Boolean" };
        _typeMap["Void"] = new TypeReference { Namespace = "System", Name = "Void" };
        
        // Additional .NET primitive types
        _typeMap["byte"] = new TypeReference { Namespace = "System", Name = "Byte" };
        _typeMap["Byte"] = new TypeReference { Namespace = "System", Name = "Byte" };
        _typeMap["char"] = new TypeReference { Namespace = "System", Name = "Char" };
        _typeMap["Char"] = new TypeReference { Namespace = "System", Name = "Char" };
        _typeMap["long"] = new TypeReference { Namespace = "System", Name = "Int64" };
        _typeMap["Int64"] = new TypeReference { Namespace = "System", Name = "Int64" };
        _typeMap["short"] = new TypeReference { Namespace = "System", Name = "Int16" };
        _typeMap["Int16"] = new TypeReference { Namespace = "System", Name = "Int16" };
        _typeMap["decimal"] = new TypeReference { Namespace = "System", Name = "Decimal" };
        _typeMap["Decimal"] = new TypeReference { Namespace = "System", Name = "Decimal" };
    }

    /// <summary>
    /// Transform an AST assembly definition to IL assembly declaration
    /// </summary>
    public AssemblyDeclaration TransformAssembly(AssemblyDef astAssembly)
    {
        var ilAssembly = new AssemblyDeclaration
        {
            Name = astAssembly.Name.Value ?? "DefaultAssembly",
            Version = new il_ast.Version(1, 0, 0, 0),
            AssemblyReferences = CreateStandardAssemblyReferences()
        };

        _currentAssembly = ilAssembly;
        
        // Transform the first module (Fifth programs typically have one module)
        if (astAssembly.Modules.Count > 0)
        {
            ilAssembly.PrimeModule = TransformModule(astAssembly.Modules[0]);
        }
        
        return ilAssembly;
    }

    private List<AssemblyReference> CreateStandardAssemblyReferences()
    {
        return new List<AssemblyReference>
        {
            new AssemblyReference
            {
                Name = "System.Runtime",
                PublicKeyToken = "b03f5f7f11d50a3a",
                Version = new il_ast.Version(8, 0, 0, 0)
            },
            new AssemblyReference
            {
                Name = "System.Console", 
                PublicKeyToken = "b03f5f7f11d50a3a",
                Version = new il_ast.Version(8, 0, 0, 0)
            }
        };
    }

    private ModuleDeclaration TransformModule(ModuleDef astModule)
    {
        var ilModule = new ModuleDeclaration
        {
            FileName = astModule.OriginalModuleName ?? "main.dll"
        };

        _currentModule = ilModule;

        // Transform classes
        foreach (var astClass in astModule.Classes)
        {
            var ilClass = TransformClass(astClass);
            if (ilClass != null)
            {
                ilModule.Classes.Add(ilClass);
            }
        }

        // Transform top-level functions
        foreach (var astFunction in astModule.Functions)
        {
            var ilMethod = TransformFunction(astFunction);
            if (ilMethod != null)
            {
                ilModule.Functions.Add(ilMethod);
            }
        }

        return ilModule;
    }

    private ClassDefinition? TransformClass(ClassDef astClass)
    {
        var ilClass = new ClassDefinition
        {
            Name = astClass.Name.Value ?? "UnnamedClass",
            Namespace = "Fifth.Generated",
            Visibility = MemberAccessability.Public,
            ParentAssembly = _currentAssembly!
        };

        _currentClass = ilClass;

        // Transform members (properties, fields, methods)
        foreach (var astMember in astClass.MemberDefs)
        {
            switch (astMember)
            {
                case PropertyDef propertyDef:
                    var ilField = TransformPropertyToField(propertyDef);
                    if (ilField != null)
                    {
                        ilClass.Fields.Add(ilField);
                    }
                    break;
                case FieldDef fieldDef:
                    var ilFieldDef = TransformField(fieldDef);
                    if (ilFieldDef != null)
                    {
                        ilClass.Fields.Add(ilFieldDef);
                    }
                    break;
            }
        }

        return ilClass;
    }

    private FieldDefinition? TransformPropertyToField(PropertyDef astProperty)
    {
        return new FieldDefinition
        {
            Name = astProperty.Name.Value ?? "UnnamedField",
            TheType = MapType(astProperty.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _currentClass!,
            TypeOfMember = MemberType.Field,
            Visibility = MemberAccessability.Public,
            IsStatic = false
        };
    }

    private FieldDefinition? TransformField(FieldDef astField)
    {
        return new FieldDefinition
        {
            Name = astField.Name.Value ?? "UnnamedField",
            TheType = MapType(astField.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _currentClass!,
            TypeOfMember = MemberType.Field,
            Visibility = MemberAccessability.Public,
            IsStatic = false
        };
    }

    private MethodDefinition? TransformFunction(FunctionDef astFunction)
    {
        var ilMethod = new MethodDefinition
        {
            Name = astFunction.Name.Value ?? "UnnamedMethod",
            ParentClass = _currentClass,
            TypeOfMember = MemberType.Method,
            Visibility = MemberAccessability.Public,
            IsStatic = _currentClass == null || astFunction.IsStatic,
            Header = new MethodHeader
            {
                FunctionKind = astFunction.Name.Value == "main" ? FunctionKind.Normal : FunctionKind.Normal,
                IsEntrypoint = astFunction.Name.Value == "main"
            },
            Signature = CreateMethodSignature(astFunction),
            Impl = new MethodImpl
            {
                IsManaged = true,
                Body = TransformBlock(astFunction.Body)
            },
            CodeTypeFlags = CodeTypeFlag.cil
        };

        return ilMethod;
    }

    private MethodSignature CreateMethodSignature(FunctionDef astFunction)
    {
        var returnTypeName = astFunction.ReturnType?.Name.Value ?? "void";
        
        var signature = new MethodSignature
        {
            CallingConvention = MethodCallingConvention.Default,
            ReturnTypeSignature = MapType(returnTypeName),
            NumberOfParameters = (ushort)(astFunction.Params?.Count ?? 0)
        };

        if (astFunction.Params != null)
        {
            foreach (var param in astFunction.Params)
            {
                signature.ParameterSignatures.Add(new ParameterSignature
                {
                    Name = param.Name ?? "param",
                    TypeReference = MapType(param.TypeName.Value?.ToString() ?? "object"),
                    InOut = InOutFlag.In
                });
            }
        }

        return signature;
    }

    private Block TransformBlock(BlockStatement? astBlock)
    {
        var ilBlock = new Block();
        
        if (astBlock?.Statements != null)
        {
            foreach (var astStatement in astBlock.Statements)
            {
                var ilStatement = TransformStatement(astStatement);
                if (ilStatement != null)
                {
                    ilBlock.Statements.Add(ilStatement);
                }
            }
        }

        return ilBlock;
    }

    private ast.Statement? TransformStatement(ast.Statement astStatement)
    {
        // With the new approach, we keep the ast.Statement types as-is
        // and only convert to instruction sequences when generating IL
        return astStatement;
    }



    /// <summary>
    /// Gets the label counter for generating unique labels
    /// </summary>
    private int _labelCounter = 0;

    private string GetOperatorString(Operator op)
    {
        return op switch
        {
            Operator.ArithmeticAdd => "+",
            Operator.ArithmeticSubtract => "-",
            Operator.ArithmeticMultiply => "*",
            Operator.ArithmeticDivide => "/",
            Operator.Equal => "==",
            Operator.NotEqual => "!=",
            Operator.LessThan => "<",
            Operator.GreaterThan => ">",
            Operator.LessThanOrEqual => "<=",
            Operator.GreaterThanOrEqual => ">=",
            Operator.ArithmeticNegative => "-",
            Operator.LogicalNot => "!",
            _ => "+"
        };
    }

    private TypeReference MapType(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var mappedType))
        {
            return mappedType;
        }

        return new TypeReference
        {
            Namespace = "Fifth.Generated",
            Name = typeName
        };
    }
    
    /// <summary>
    /// Generates instruction sequence for an expression that evaluates to a value on the stack
    /// </summary>
    public InstructionSequence GenerateExpression(ast.Expression expression)
    {
        var sequence = new InstructionSequence();
        
        switch (expression)
        {
            case Int32LiteralExp intLit:
                sequence.Add(new LoadInstruction("ldc.i4", intLit.Value));
                break;
                
            case Float4LiteralExp floatLit:
                sequence.Add(new LoadInstruction("ldc.r4", floatLit.Value));
                break;
                
            case Float8LiteralExp doubleLit:
                sequence.Add(new LoadInstruction("ldc.r8", doubleLit.Value));
                break;
                
            case StringLiteralExp stringLit:
                sequence.Add(new LoadInstruction("ldstr", $"\"{EscapeString(stringLit.Value)}\""));
                break;
                
            case BooleanLiteralExp boolLit:
                sequence.Add(new LoadInstruction("ldc.i4", boolLit.Value ? 1 : 0));
                break;
                
            case VarRefExp varRef:
                sequence.Add(new LoadInstruction("ldloc", varRef.VarName));
                break;
                
            case BinaryExp binaryExp:
                // Emit left operand
                sequence.AddRange(GenerateExpression(binaryExp.LHS).Instructions);
                // Emit right operand
                sequence.AddRange(GenerateExpression(binaryExp.RHS).Instructions);
                // Emit operation
                sequence.Add(new ArithmeticInstruction(GetBinaryOpCode(GetOperatorString(binaryExp.Operator))));
                break;
                
            case UnaryExp unaryExp:
                // Emit operand
                sequence.AddRange(GenerateExpression(unaryExp.Operand).Instructions);
                // Emit operation
                sequence.Add(new ArithmeticInstruction(GetUnaryOpCode(GetOperatorString(unaryExp.Operator))));
                break;
                
            case ast.FuncCallExp funcCall:
                // Emit arguments
                if (funcCall.InvocationArguments != null)
                {
                    foreach (var arg in funcCall.InvocationArguments)
                    {
                        sequence.AddRange(GenerateExpression(arg).Instructions);
                    }
                }
                // Emit call
                var functionName = funcCall.FunctionDef?.Name.Value ?? "unknown";
                if (functionName == "print" || functionName == "myprint")
                {
                    sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)"));
                }
                else
                {
                    sequence.Add(new CallInstruction("call", $"void {functionName}()"));
                }
                break;
        }
        
        return sequence;
    }
    
    /// <summary>
    /// Generates instruction sequence for an if statement using branch instructions
    /// </summary>
    public InstructionSequence GenerateIfStatement(IfElseStatement ifStmt)
    {
        var sequence = new InstructionSequence();
        var falseLabel = $"IL_false_{_labelCounter++}";
        var endLabel = $"IL_end_{_labelCounter++}";
        
        // Emit condition evaluation
        sequence.AddRange(GenerateExpression(ifStmt.Condition).Instructions);
        
        // Branch to false label if condition is false
        sequence.Add(new BranchInstruction("brfalse", falseLabel));
        
        // Emit if block instructions
        foreach (var stmt in ifStmt.ThenBlock.Statements)
        {
            sequence.AddRange(GenerateStatement(stmt).Instructions);
        }
        
        // Branch to end
        sequence.Add(new BranchInstruction("br", endLabel));
        
        // False label
        sequence.Add(new LabelInstruction(falseLabel));
        
        // Emit else block
        foreach (var stmt in ifStmt.ElseBlock.Statements)
        {
            sequence.AddRange(GenerateStatement(stmt).Instructions);
        }
        
        // End label
        sequence.Add(new LabelInstruction(endLabel));
        
        return sequence;
    }
    
    /// <summary>
    /// Generates instruction sequence for a while loop using branch instructions
    /// </summary>
    public InstructionSequence GenerateWhileStatement(ast.WhileStatement whileStmt)
    {
        var sequence = new InstructionSequence();
        var startLabel = $"IL_loop_{_labelCounter++}";
        var endLabel = $"IL_end_{_labelCounter++}";
        
        // Start label
        sequence.Add(new LabelInstruction(startLabel));
        
        // Emit condition evaluation
        sequence.AddRange(GenerateExpression(whileStmt.Condition).Instructions);
        
        // Branch to end if condition is false
        sequence.Add(new BranchInstruction("brfalse", endLabel));
        
        // Emit loop body
        foreach (var stmt in whileStmt.Body.Statements)
        {
            sequence.AddRange(GenerateStatement(stmt).Instructions);
        }
        
        // Branch back to start
        sequence.Add(new BranchInstruction("br", startLabel));
        
        // End label
        sequence.Add(new LabelInstruction(endLabel));
        
        return sequence;
    }
    
    /// <summary>
    /// Generates instruction sequence for any statement
    /// </summary>
    public InstructionSequence GenerateStatement(ast.Statement statement)
    {
        var sequence = new InstructionSequence();
        
        switch (statement)
        {
            case VarDeclStatement varDecl:
                // IL locals are typically declared in method header, 
                // so we just handle initialization if present
                if (varDecl.InitialValue != null)
                {
                    sequence.AddRange(GenerateExpression(varDecl.InitialValue).Instructions);
                    sequence.Add(new StoreInstruction("stloc", varDecl.VariableDecl.Name ?? "temp"));
                }
                break;
                
            case AssignmentStatement assignment:
                sequence.AddRange(GenerateExpression(assignment.RValue).Instructions);
                // Extract variable name from LValue if it's a VarRefExp
                var targetName = assignment.LValue switch
                {
                    VarRefExp varRef => varRef.VarName,
                    _ => "unknown"
                };
                sequence.Add(new StoreInstruction("stloc", targetName));
                break;
                
            case ast.ReturnStatement returnStmt:
                sequence.AddRange(GenerateExpression(returnStmt.ReturnValue).Instructions);
                sequence.Add(new ReturnInstruction());
                break;
                
            case IfElseStatement ifStmt:
                sequence.AddRange(GenerateIfStatement(ifStmt).Instructions);
                break;
                
            case ast.WhileStatement whileStmt:
                sequence.AddRange(GenerateWhileStatement(whileStmt).Instructions);
                break;
                
            case ExpStatement exprStmt:
                sequence.AddRange(GenerateExpression(exprStmt.RHS).Instructions);
                sequence.Add(new StackInstruction("pop")); // Pop unused result
                break;
        }
        
        return sequence;
    }
    
    private string GetBinaryOpCode(string op)
    {
        return op switch
        {
            "+" => "add",
            "-" => "sub", 
            "*" => "mul",
            "/" => "div",
            "==" => "ceq",
            "!=" => "ceq_neg", // Special marker to emit ceq + ldc.i4.0 + ceq
            "<" => "clt",
            ">" => "cgt", 
            "<=" => "cle",
            ">=" => "cge",
            _ => "add"
        };
    }
    
    private string GetUnaryOpCode(string op)
    {
        return op switch
        {
            "-" => "neg",
            "!" => "not", // Special marker for logical not
            _ => "neg"
        };
    }
    
    private string EscapeString(string? input)
    {
        return input?.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") ?? "";
    }
}