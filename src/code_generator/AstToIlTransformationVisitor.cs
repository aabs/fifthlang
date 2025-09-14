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
    private static bool DebugEnabled =>
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("1", StringComparison.Ordinal) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("true", StringComparison.OrdinalIgnoreCase) ||
        (System.Environment.GetEnvironmentVariable("FIFTH_DEBUG") ?? string.Empty).Equals("on", StringComparison.OrdinalIgnoreCase);

    private static void DebugLog(string message)
    {
        if (DebugEnabled) Console.WriteLine(message);
    }
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
        // Map language 'graph' to dotNetRDF IGraph
        _typeMap["graph"] = new TypeReference { Namespace = "VDS.RDF", Name = "IGraph" };

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
            },
            // Reference Fifth.System where KG helpers live
            new AssemblyReference
            {
                Name = "Fifth.System",
                PublicKeyToken = "",
                Version = new il_ast.Version(1, 0, 0, 0)
            },
            // Reference dotNetRDF for IGraph types
            new AssemblyReference
            {
                Name = "dotNetRDF",
                PublicKeyToken = "",
                Version = new il_ast.Version(3, 4, 0, 0)
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

    private bool IsTypeName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (_typeMap.ContainsKey(name)) return true;
        // Simple heuristic: treat single-word capitalized identifiers as potential types
        return char.IsUpper(name[0]);
    }

    /// <summary>
    /// Generates instruction sequence for an expression that evaluates to a value on the stack
    /// </summary>
    public InstructionSequence GenerateExpression(ast.Expression expression)
    {
        var sequence = new InstructionSequence();

        switch (expression)
        {
            case GraphAssertionBlockExp gab:
                // Placeholder: create a stub value to represent a graph
                // Future: emit construction of an actual graph and assertions
                sequence.Add(new LoadInstruction("ldnull", null));
                break;
            case ast.ListLiteral listLit:
                // Lower list literal to an int32 array for now
                // Pattern: ldc.i4 <len>; newarr int32; dup; init elements with stelem.i4
                var count = listLit.ElementExpressions?.Count ?? 0;
                sequence.Add(new LoadInstruction("ldc.i4", count));
                sequence.Add(new LoadInstruction("newarr", "int32"));
                for (int i = 0; i < count; i++)
                {
                    sequence.Add(new LoadInstruction("dup"));
                    sequence.Add(new LoadInstruction("ldc.i4", i));
                    var elem = listLit.ElementExpressions[i];
                    sequence.AddRange(GenerateExpression(elem).Instructions);
                    sequence.Add(new StoreInstruction("stelem.i4"));
                }
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
                DebugLog($"DEBUG: Processing BinaryExp with LHS: {binaryExp.LHS?.GetType().Name ?? "null"}, RHS: {binaryExp.RHS?.GetType().Name ?? "null"}, Operator: {binaryExp.Operator.ToString()}");
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
                DebugLog($"DEBUG: Processing FuncCallExp with function: {funcCall.FunctionDef?.Name.Value ?? "null"}, arguments: {funcCall.InvocationArguments?.Count ?? 0}");
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
                DebugLog($"DEBUG: Processing MemberAccessExp with LHS: {memberAccess.LHS?.GetType().Name ?? "null"}, RHS: {memberAccess.RHS?.GetType().Name ?? "null"}");

                // Qualified external static call: <Type>.<Method>(args)
                if (memberAccess.RHS is ast.FuncCallExp extCall && extCall.Annotations != null &&
                    extCall.Annotations.TryGetValue("ExternalType", out var tObj) && tObj is Type extType &&
                    extCall.Annotations.TryGetValue("ExternalMethodName", out var mObj) && mObj is string mName)
                {
                    // Emit arguments first
                    foreach (var arg in extCall.InvocationArguments ?? new List<ast.Expression>())
                    {
                        sequence.AddRange(GenerateExpression(arg).Instructions);
                    }
                    // Encode a special extcall signature for PEEmitter to resolve
                    var paramList = string.Join(",", (extCall.InvocationArguments ?? new List<ast.Expression>()).Select(_ => "object"));
                    var sig = $"extcall:Asm=Fifth.System;Ns={extType.Namespace};Type={extType.Name};Method={mName};Params={paramList};Return=object";
                    sequence.Add(new CallInstruction("call", sig));
                    break;
                }

                // Special-case: array creation syntax like int[10]
                if (memberAccess.RHS is VarRefExp rhsIndex1 && string.Equals(rhsIndex1.VarName, "[index]", StringComparison.Ordinal))
                {
                    if (memberAccess.LHS is VarRefExp lhsVar1 && IsTypeName(lhsVar1.VarName))
                    {
                        try
                        {
                            if (rhsIndex1.Annotations != null && rhsIndex1.Annotations.TryGetValue("IndexExpression", out var idxObj) && idxObj is ast.Expression idxExp)
                            {
                                sequence.AddRange(GenerateExpression(idxExp).Instructions);
                            }
                            else
                            {
                                sequence.Add(new LoadInstruction("ldc.i4", 0));
                            }
                            // For now, only support int32 arrays
                            sequence.Add(new LoadInstruction("newarr", "int32"));
                            break;
                        }
                        catch (Exception ex)
                        {
                            DebugLog($"DEBUG: Array creation lowering failed: {ex.Message}");
                            sequence.Add(new LoadInstruction("ldnull", null));
                            break;
                        }
                    }
                }

                // Load the object (LHS) for normal member access or indexing into a variable (skip for extcall)
                if (memberAccess.LHS != null)
                {
                    sequence.AddRange(GenerateExpression(memberAccess.LHS).Instructions);
                }

                // Indexing lowering: RHS as synthetic VarRefExp("[index]") with annotation IndexExpression
                if (memberAccess.RHS is VarRefExp memberVarRefIndex && string.Equals(memberVarRefIndex.VarName, "[index]", StringComparison.Ordinal))
                {
                    try
                    {
                        // Heuristic: Only emit ldelem for obvious array sources (locals or member access off locals)
                        var lhsLooksLikeArray = memberAccess.LHS is VarRefExp
                            || (memberAccess.LHS is MemberAccessExp lhsMa && lhsMa.LHS is VarRefExp);

                        if (lhsLooksLikeArray)
                        {
                            if (memberVarRefIndex.Annotations != null && memberVarRefIndex.Annotations.TryGetValue("IndexExpression", out var idxObj) && idxObj is ast.Expression idxExp)
                            {
                                sequence.AddRange(GenerateExpression(idxExp).Instructions);
                                sequence.Add(new LoadInstruction("ldelem.i4"));
                            }
                            else
                            {
                                // Fallback: missing index expression; push 0 index and ldelem
                                sequence.Add(new LoadInstruction("ldc.i4", 0));
                                sequence.Add(new LoadInstruction("ldelem.i4"));
                            }
                        }
                        else
                        {
                            // Skip indexing when LHS is not a known array source (e.g., function call result)
                            // to avoid invalid IL. The LHS value remains on the stack for outer contexts to consume/pop.
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLog($"DEBUG: Index lowering failed: {ex.Message}");
                    }
                }
                else if (memberAccess.RHS is VarRefExp memberVarRef)
                {
                    // Heuristic: Only emit field load for capitalized member names (likely class fields/properties)
                    // This avoids emitting ldfld against locals/arrays like 'a' in expressions such as a.a[0].a
                    var name = memberVarRef.VarName ?? string.Empty;
                    if (!string.IsNullOrEmpty(name) && char.IsUpper(name[0]))
                    {
                        sequence.Add(new LoadInstruction("ldfld", name));
                    }
                    else
                    {
                        // Skip unknown/lowercase member access to keep stack valid for subsequent operations (e.g., indexing)
                    }
                }
                else
                {
                    // Unsupported member access RHS type; skip emitting and avoid noisy logs
                }
                break;

            case ObjectInitializerExp objectInit:
                DebugLog($"DEBUG: Processing ObjectInitializerExp for type: {objectInit.TypeToInitialize?.Name.Value ?? "unknown"}");

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
        DebugLog($"DEBUG: GenerateStatement called with {statement?.GetType().Name ?? "null"}");

        switch (statement)
        {
            case GraphAssertionBlockStatement gabStmt:
                // Evaluate inner graph block expression and discard result
                if (gabStmt.Content != null)
                {
                    sequence.AddRange(GenerateExpression(gabStmt.Content).Instructions);
                    sequence.Add(new StackInstruction("pop"));
                }
                break;
            case VarDeclStatement varDecl:
                DebugLog($"DEBUG: VarDeclStatement with variable: {varDecl.VariableDecl?.Name ?? "null"}, InitialValue type: {varDecl.InitialValue?.GetType().Name ?? "null"}");
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
                DebugLog($"DEBUG: Processing ReturnStatement with ReturnValue: {returnStmt.ReturnValue?.GetType().Name ?? "null"}");
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