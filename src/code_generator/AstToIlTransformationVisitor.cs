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
    private HashSet<string> _currentParameterNames = new(StringComparer.Ordinal);
    
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
            if (astFunction is FunctionDef functionDef)
            {
                var ilFunction = TransformFunction(functionDef);
                if (ilFunction != null)
                {
                    ilModule.Functions.Add(ilFunction);
                }
            }
            // TODO: Handle OverloadedFunctionDef when overload transforming is implemented
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
            Operator.LogicalAnd => "&&",
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
            case ast.ListLiteral listLit:
                // Placeholder: lists/arrays not yet supported; push 0 to keep stack balanced
                sequence.Add(new LoadInstruction("ldc.i4", 0));
                break;
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
                if (_currentParameterNames.Contains(varRef.VarName))
                {
                    sequence.Add(new LoadInstruction("ldarg", varRef.VarName));
                }
                else
                {
                    sequence.Add(new LoadInstruction("ldloc", varRef.VarName));
                }
                break;
                
            case BinaryExp binaryExp:
                Console.WriteLine($"DEBUG: Processing BinaryExp with LHS: {binaryExp.LHS?.GetType().Name ?? "null"}, RHS: {binaryExp.RHS?.GetType().Name ?? "null"}, Operator: {binaryExp.Operator.ToString()}");
                // Emit operands safely; only emit operation when both sides are present
                var leftPresent = binaryExp.LHS != null;
                var rightPresent = binaryExp.RHS != null;

                if (leftPresent)
                {
                    sequence.AddRange(GenerateExpression(binaryExp.LHS!).Instructions);
                }
                if (rightPresent)
                {
                    sequence.AddRange(GenerateExpression(binaryExp.RHS!).Instructions);
                }

                if (leftPresent && rightPresent)
                {
                    sequence.Add(new ArithmeticInstruction(GetBinaryOpCode(GetOperatorString(binaryExp.Operator))));
                }
                break;
                
            case UnaryExp unaryExp:
                // Emit operand
                sequence.AddRange(GenerateExpression(unaryExp.Operand).Instructions);
                // Emit operation
                sequence.Add(new ArithmeticInstruction(GetUnaryOpCode(GetOperatorString(unaryExp.Operator))));
                break;
                
            case ast.FuncCallExp funcCall:
                Console.WriteLine($"DEBUG: Processing FuncCallExp with function: {funcCall.FunctionDef?.Name.Value ?? "null"}, arguments: {funcCall.InvocationArguments?.Count ?? 0}");
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
                    // Generate proper method signature based on return type
                    var returnType = funcCall.FunctionDef?.ReturnType?.Name.Value ?? "int";
                    var ilReturnType = returnType switch
                    {
                        "int" => "int32",
                        "string" => "string",
                        "float" => "float32",
                        "double" => "float64",
                        "bool" => "bool",
                        "void" => "void",
                        _ => "int32"
                    };
                    sequence.Add(new CallInstruction("call", $"{ilReturnType} {functionName}()"));
                }
                break;
                
            case MemberAccessExp memberAccess:
                Console.WriteLine($"DEBUG: Processing MemberAccessExp with LHS: {memberAccess.LHS?.GetType().Name ?? "null"}, RHS: {memberAccess.RHS?.GetType().Name ?? "null"}");
                
                // Load the object (LHS)
                if (memberAccess.LHS != null)
                {
                    sequence.AddRange(GenerateExpression(memberAccess.LHS).Instructions);
                }
                
                // Load the field value (RHS should be the field/property name)
                if (memberAccess.RHS is VarRefExp memberVarRef)
                {
                    // For now, assume simple field access - this may need enhancement for complex property access
                    sequence.Add(new LoadInstruction("ldfld", memberVarRef.VarName));
                }
                else
                {
                    Console.WriteLine($"DEBUG: Unsupported member access RHS type: {memberAccess.RHS?.GetType().Name ?? "null"}");
                }
                break;
                
            case ObjectInitializerExp objectInit:
                Console.WriteLine($"DEBUG: Processing ObjectInitializerExp for type: {objectInit.TypeToInitialize?.Name.Value ?? "unknown"}");
                
                // For object initialization, we need to:
                // 1. Create new instance (constructor call)
                // 2. Initialize each property
                
                // Create new instance - for now, assume default constructor
                var typeName = objectInit.TypeToInitialize?.Name.Value ?? "object";
                sequence.Add(new CallInstruction("newobj", $"instance void {typeName}::.ctor()"));
                
                // Initialize properties
                if (objectInit.PropertyInitialisers != null)
                {
                    foreach (var propInit in objectInit.PropertyInitialisers)
                    {
                        // Duplicate the object reference for each property assignment
                        sequence.Add(new LoadInstruction("dup", null));
                        
                        // Load the value to assign
                        sequence.AddRange(GenerateExpression(propInit.RHS).Instructions);
                        
                        // Store the field/property
                        var propertyName = propInit.PropertyToInitialize.Property.Name.Value ?? "unknown";
                        sequence.Add(new StoreInstruction("stfld", propertyName));
                    }
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
        var endLabel = $"IL_end_{_labelCounter++}";
        var falseLabel = $"IL_false_{_labelCounter++}";
        
        // Emit condition evaluation
        sequence.AddRange(GenerateExpression(ifStmt.Condition).Instructions);
        
        var thenStmts = ifStmt.ThenBlock?.Statements ?? new List<ast.Statement>();
        var elseStmts = ifStmt.ElseBlock?.Statements ?? new List<ast.Statement>();

        // If there's no else block, branch directly to end when false
        if (elseStmts.Count == 0)
        {
            // Emit a dedicated false label and an end label to match expectations
            sequence.Add(new BranchInstruction("brfalse", falseLabel));
            foreach (var stmt in thenStmts)
            {
                sequence.AddRange(GenerateStatement(stmt).Instructions);
            }
            // Explicitly branch to end to avoid falling into false label position
            sequence.Add(new BranchInstruction("br", endLabel));
            sequence.Add(new LabelInstruction(falseLabel));
            sequence.Add(new LabelInstruction(endLabel));
        }
        else
        {
            // With else: branch to false label if condition is false
            sequence.Add(new BranchInstruction("brfalse", falseLabel));
            foreach (var stmt in thenStmts)
            {
                sequence.AddRange(GenerateStatement(stmt).Instructions);
            }
            sequence.Add(new BranchInstruction("br", endLabel));
            sequence.Add(new LabelInstruction(falseLabel));
            foreach (var stmt in elseStmts)
            {
                sequence.AddRange(GenerateStatement(stmt).Instructions);
            }
            sequence.Add(new LabelInstruction(endLabel));
        }
        
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
        if (whileStmt.Condition != null)
        {
            sequence.AddRange(GenerateExpression(whileStmt.Condition).Instructions);
        }
        
        // Branch to end if condition is false
        sequence.Add(new BranchInstruction("brfalse", endLabel));
        
        // Emit loop body
        var bodyStmts = whileStmt.Body?.Statements ?? new List<ast.Statement>();
        foreach (var stmt in bodyStmts)
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
        
        // Debug: Log statement type and content
        Console.WriteLine($"DEBUG: GenerateStatement called with {statement?.GetType().Name ?? "null"}");
        
        switch (statement)
        {
            case VarDeclStatement varDecl:
                Console.WriteLine($"DEBUG: VarDeclStatement with variable: {varDecl.VariableDecl?.Name ?? "null"}, InitialValue type: {varDecl.InitialValue?.GetType().Name ?? "null"}");
                // IL locals are typically declared in method header, 
                // so we just handle initialization if present
                if (varDecl.InitialValue != null)
                {
                    sequence.AddRange(GenerateExpression(varDecl.InitialValue).Instructions);
                    var localName = varDecl.VariableDecl?.Name ?? "temp";
                    sequence.Add(new StoreInstruction("stloc", localName));
                }
                break;
                
            case AssignmentStatement assignment:
                // Handle assignment differently based on LValue kind
                switch (assignment.LValue)
                {
                    case VarRefExp varRef:
                        // Simple local variable assignment: eval RHS, store to local
                        sequence.AddRange(GenerateExpression(assignment.RValue).Instructions);
                        sequence.Add(new StoreInstruction("stloc", varRef.VarName));
                        break;

                    case MemberAccessExp memberAccess:
                        // Object member assignment: eval object ref, then RHS, then stfld <field>
                        // Load object (LHS)
                        if (memberAccess.LHS != null)
                        {
                            sequence.AddRange(GenerateExpression(memberAccess.LHS).Instructions);
                        }
                        // Load value (RHS)
                        sequence.AddRange(GenerateExpression(assignment.RValue).Instructions);
                        // Store field/property
                        if (memberAccess.RHS is VarRefExp memberVar)
                        {
                            sequence.Add(new StoreInstruction("stfld", memberVar.VarName));
                        }
                        else
                        {
                            // Fallback to unknown field name to avoid nulls; emitter will keep stack balanced
                            sequence.Add(new StoreInstruction("stfld", "unknown"));
                        }
                        break;

                    default:
                        // Fallback: eval RHS and store into a temp to keep stack consistent
                        sequence.AddRange(GenerateExpression(assignment.RValue).Instructions);
                        sequence.Add(new StoreInstruction("stloc", "temp"));
                        break;
                }
                break;
                
            case ast.ReturnStatement returnStmt:
                Console.WriteLine($"DEBUG: Processing ReturnStatement with ReturnValue: {returnStmt.ReturnValue?.GetType().Name ?? "null"}");
                if (returnStmt.ReturnValue != null)
                {
                    sequence.AddRange(GenerateExpression(returnStmt.ReturnValue).Instructions);
                }
                else
                {
                    // To prevent invalid IL for non-void methods, push a default int32 value
                    sequence.Add(new LoadInstruction("ldc.i4", 0));
                }
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

    public void SetCurrentParameters(IEnumerable<string> parameterNames)
    {
        _currentParameterNames = new HashSet<string>(parameterNames ?? Array.Empty<string>(), StringComparer.Ordinal);
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
            "&&" => "and",
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