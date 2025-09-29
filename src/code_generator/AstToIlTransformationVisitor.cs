using ast;
using il_ast;
using il_ast_generated;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Transforms AST nodes from the Fifth language parser into IL metamodel representations.
/// This visitor follows the transformation stage pattern used in FifthParserManager.ApplyLanguageAnalysisPhases.
/// </summary>
public class AstToIlTransformationVisitor : DefaultRecursiveDescentVisitor
{
    // Debugging provided by shared DebugHelpers.

    private readonly Dictionary<string, TypeReference> _typeMap = new();
    private AssemblyDeclaration? _currentAssembly;
    private ModuleDeclaration? _currentModule;
    private ClassDefinition? _currentClass;
    private HashSet<string> _currentParameterNames = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> _currentParameterTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> _localVariableTypes = new(StringComparer.Ordinal);
    // Counter for generating unique temporary local variable names
    private int _tempCounter = 0;

    public AstToIlTransformationVisitor()
    {
        InitializeBuiltinTypes();
    }

    private static bool IsImplicitNumericWidening(Type src, Type dest)
    {
        // Minimal widening support for common cases used in tests
        if (src == typeof(int))
        {
            return dest == typeof(long) || dest == typeof(float) || dest == typeof(double) || dest == typeof(decimal);
        }
        if (src == typeof(float))
        {
            return dest == typeof(double);
        }
        if (src == typeof(long))
        {
            return dest == typeof(float) || dest == typeof(double) || dest == typeof(decimal);
        }
        return false;
    }

    private static int CompatibilityScore(Type? argType, Type paramType)
    {
        if (argType == null) return 0;
        if (paramType == argType) return 100;
        if (paramType.IsAssignableFrom(argType)) return 50;
        if (IsImplicitNumericWidening(argType, paramType)) return 10;
        return 0;
    }

    private static bool IsNumeric(Type t)
    {
        return t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
    }

    private static Type PromoteNumeric(Type a, Type b)
    {
        if (a == typeof(double) || b == typeof(double)) return typeof(double);
        if (a == typeof(float) || b == typeof(float)) return typeof(float);
        if (a == typeof(long) || b == typeof(long)) return typeof(long);
        if (a == typeof(decimal) || b == typeof(decimal)) return typeof(decimal);
        return typeof(int);
    }

    private static Type? MapBuiltinFifthTypeNameToSystem(string? name)
    {
        return name switch
        {
            "int" or "System.Int32" or "Int32" => typeof(int),
            "string" or "System.String" or "String" => typeof(string),
            "float" or "System.Single" or "Single" => typeof(float),
            "double" or "System.Double" or "Double" => typeof(double),
            "bool" or "System.Boolean" or "Boolean" => typeof(bool),
            "long" or "System.Int64" or "Int64" => typeof(long),
            "short" or "System.Int16" or "Int16" => typeof(short),
            "byte" or "System.Byte" or "Byte" => typeof(byte),
            "char" or "System.Char" or "Char" => typeof(char),
            "decimal" or "System.Decimal" or "Decimal" => typeof(decimal),
            // New/supplementary primitive mappings
            "sbyte" or "System.SByte" or "SByte" or "int8" or "Int8" => typeof(sbyte),
            "uint" or "System.UInt32" or "UInt32" => typeof(uint),
            "ushort" or "System.UInt16" or "UInt16" => typeof(ushort),
            "ulong" or "System.UInt64" or "UInt64" => typeof(ulong),
            _ => null
        };
    }

    private Type? InferExpressionType(ast.Expression expr)
    {
        switch (expr)
        {
            case Int32LiteralExp:
                return typeof(int);
            case StringLiteralExp:
                return typeof(string);
            case BooleanLiteralExp:
                return typeof(bool);
            case Float4LiteralExp:
                return typeof(float);
            case Float8LiteralExp:
                return typeof(double);
            case Float16LiteralExp:
                return typeof(decimal);
            case VarRefExp v:
                if (_localVariableTypes.TryGetValue(v.VarName, out var lty)) return lty;
                if (_currentParameterTypes.TryGetValue(v.VarName, out var pty)) return pty;
                return null;
            case BinaryExp be:
                {
                    var lt = be.LHS != null ? InferExpressionType(be.LHS) : null;
                    var rt = be.RHS != null ? InferExpressionType(be.RHS) : null;
                    // Comparison/logical -> bool
                    switch (be.Operator)
                    {
                        case Operator.Equal:
                        case Operator.NotEqual:
                        case Operator.LessThan:
                        case Operator.GreaterThan:
                        case Operator.LessThanOrEqual:
                        case Operator.GreaterThanOrEqual:
                        case Operator.LogicalAnd:
                        case Operator.LogicalOr:
                        case Operator.LogicalXor:
                            return typeof(bool);
                    }
                    // Addition: string concatenation if either is string
                    if (be.Operator == Operator.ArithmeticAdd && (lt == typeof(string) || rt == typeof(string)))
                    {
                        return typeof(string);
                    }
                    // Numeric promotion
                    if (lt != null && rt != null && IsNumeric(lt) && IsNumeric(rt))
                    {
                        return PromoteNumeric(lt, rt);
                    }
                    return null;
                }
            case UnaryExp ue:
                {
                    var ot = InferExpressionType(ue.Operand);
                    return ue.Operator switch
                    {
                        Operator.ArithmeticNegative => ot, // preserve numeric type
                        Operator.LogicalNot => typeof(bool),
                        _ => ot
                    };
                }
            case MemberAccessExp ma when ma.RHS is ast.FuncCallExp inner && inner.Annotations != null &&
                                         inner.Annotations.TryGetValue("ExternalType", out var tObj) && tObj is Type innerExtType &&
                                         inner.Annotations.TryGetValue("ExternalMethodName", out var mObj) && mObj is string innerMName:
                {
                    var args = inner.InvocationArguments ?? new List<ast.Expression>();
                    var chosen = ResolveExternalMethod(innerExtType, innerMName, args);
                    return chosen?.ReturnType;
                }
            case ast.FuncCallExp fc when fc.FunctionDef?.ReturnType?.Name.Value is string tn:
                return MapBuiltinFifthTypeNameToSystem(tn);
        }
        return null;
    }

    private System.Reflection.MethodInfo? ResolveExternalMethod(Type extType, string methodName, IList<ast.Expression> args, Type? receiverType = null)
    {
        var methods = extType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(mi => string.Equals(mi.Name, methodName, StringComparison.Ordinal))
            .ToList();
        var argCount = args.Count;
        var inferred = new List<Type?>();
        for (int i = 0; i < argCount; i++)
        {
            var ai = args[i];
            inferred.Add(ai != null ? InferExpressionType(ai) : null);
        }

        // Filter by arity and optional trailing params
        var candidates = methods
            .Select(mi => new { mi, ps = mi.GetParameters() })
            .Where(x =>
            {
                // Exact match on supplied args
                if (x.ps.Length == argCount) return true;
                // Extension method case: first parameter is the receiver and method has one more parameter than supplied args
                if (x.ps.Length == argCount + 1) return true;
                // Allow methods with optional trailing params
                if (x.ps.Length > argCount && x.ps.Skip(argCount).All(p => p.IsOptional || p.HasDefaultValue)) return true;
                return false;
            })
            .ToList();
        if (candidates.Count == 0)
        {
            // No arity-compatible methods
            return null;
        }

        // Score candidates by type compatibility of provided arguments
        var scored = candidates.Select(x => new
        {
            x.mi,
            x.ps,
            score =
                // Treat candidates that expect a receiver (ps.Length == argCount + 1) specially by shifting
                // the mapping of supplied args to parameters by 1 so args map to ps[1..]. If receiverType is
                // available, include its compatibility score against ps[0].
                Enumerable.Range(0, Math.Min(argCount, x.ps.Length - (x.ps.Length == argCount + 1 ? 1 : 0)))
                    .Sum(i =>
                    {
                        var offset = x.ps.Length == argCount + 1 ? 1 : 0;
                        var paramIndex = i + offset;
                        if (paramIndex >= 0 && paramIndex < x.ps.Length)
                        {
                            return CompatibilityScore(inferred[i], x.ps[paramIndex].ParameterType);
                        }
                        return 0;
                    })
                + (receiverType != null && x.ps.Length == argCount + 1 ? CompatibilityScore(receiverType, x.ps[0].ParameterType) : 0),
            isGeneric = x.mi.IsGenericMethodDefinition
        })
        .OrderByDescending(s => s.score)
        .ThenBy(s => s.ps.Length) // prefer fewer total params (i.e., fewer optionals)
        .ThenBy(s => s.isGeneric) // prefer non-generic over generic
        .ToList();

        // Debug: print scored candidates to help diagnose ambiguous/failed resolutions
        try
        {
            Console.WriteLine($"TRACE: ResolveExternalMethod candidates for {extType.FullName}.{methodName} (argCount={argCount}, receiverPresent={(receiverType != null)}):");
            foreach (var sc in scored)
            {
                var pdesc = string.Join(",", sc.ps.Select(p => p.ParameterType.FullName + "(" + p.Name + ")"));
                Console.WriteLine($"  candidate: {sc.mi.DeclaringType?.FullName}.{sc.mi.Name} params=[{pdesc}] score={sc.score}");
            }
        }
        catch { }

        // If no arguments supplied, any arity-compatible (zero-param) candidate is acceptable
        if (argCount == 0 && scored.Count > 0)
        {
            return scored[0].mi;
        }

        var allInferredNull = inferred.All(t => t == null);

        // Reject if best score indicates incompatible arguments for supplied args
        if (scored.Count == 0)
        {
            return null;
        }
        if (scored[0].score <= 0 && !allInferredNull)
        {
            // Reject when there are inferred types but they don't match candidates
            return null;
        }
        // If all inferred types were null, accept the best arity-compatible candidate (fallback)
        if (allInferredNull && scored.Count > 0)
        {
            return scored[0].mi;
        }

        // Pick best candidate
        var best = scored[0];

        // Ensure each supplied argument is compatible; handle shifted parameters for extension methods
        var bestOffset = (receiverType != null && best.ps.Length == argCount + 1) ? 1 : 0;
        for (int i = 0; i < argCount; i++)
        {
            var paramIndex = i + bestOffset;
            if (paramIndex < 0 || paramIndex >= best.ps.Length) return null;
            if (CompatibilityScore(inferred[i], best.ps[paramIndex].ParameterType) <= 0)
            {
                return null;
            }
        }

        // If this candidate expects a receiver (extension method) ensure receiver compatibility
        if (receiverType != null && best.ps.Length == argCount + 1)
        {
            if (CompatibilityScore(receiverType, best.ps[0].ParameterType) <= 0)
            {
                return null;
            }
        }

        return best.mi;
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
        // New primitive names for signed/unsigned integers
        _typeMap["sbyte"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["SByte"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["int8"] = new TypeReference { Namespace = "System", Name = "SByte" };
        _typeMap["Int8"] = new TypeReference { Namespace = "System", Name = "SByte" };

        _typeMap["uint"] = new TypeReference { Namespace = "System", Name = "UInt32" };
        _typeMap["UInt32"] = new TypeReference { Namespace = "System", Name = "UInt32" };
        _typeMap["ulong"] = new TypeReference { Namespace = "System", Name = "UInt64" };
        _typeMap["UInt64"] = new TypeReference { Namespace = "System", Name = "UInt64" };
        _typeMap["ushort"] = new TypeReference { Namespace = "System", Name = "UInt16" };
        _typeMap["UInt16"] = new TypeReference { Namespace = "System", Name = "UInt16" };
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
            ParentClass = _currentClass!,
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
            Operator.BitwiseOr => "|",
            Operator.LogicalOr => "||",
            Operator.LogicalXor => "xor",
            Operator.ArithmeticNegative => "-",
            Operator.LogicalNot => "!",
            _ => "+"
        };
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
            "!=" => "ceq", // handled by compare+not if needed by emitter
            "<" => "clt",
            ">" => "cgt",
            "<=" => "cgt", // will typically require additional logic
            ">=" => "clt", // will typically require additional logic
            _ => "add",
        };
    }

    private string GetUnaryOpCode(string op)
    {
        return op switch
        {
            "-" => "neg",
            "!" => "ldc.i4.0", // logical not will require compare
            _ => "nop",
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
            case Int32LiteralExp intLit:
                sequence.Add(new LoadInstruction("ldc.i4", intLit.Value));
                break;

            case Float4LiteralExp floatLit:
                sequence.Add(new LoadInstruction("ldc.r4", floatLit.Value));
                break;

            case Float8LiteralExp doubleLit:
                sequence.Add(new LoadInstruction("ldc.r8", doubleLit.Value));
                break;

            case Float16LiteralExp decimalLit:
                sequence.Add(new LoadInstruction("ldstr", decimalLit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal") { ArgCount = 1 });
                break;

            case StringLiteralExp stringLit:
                sequence.Add(new LoadInstruction("ldstr", stringLit.Value ?? string.Empty));
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
                if (binaryExp.LHS != null) sequence.AddRange(GenerateExpression(binaryExp.LHS).Instructions);
                if (binaryExp.RHS != null) sequence.AddRange(GenerateExpression(binaryExp.RHS).Instructions);
                sequence.Add(new ArithmeticInstruction(GetBinaryOpCode(GetOperatorString(binaryExp.Operator))));
                break;

            case UnaryExp unaryExp:
                if (unaryExp.Operand != null) sequence.AddRange(GenerateExpression(unaryExp.Operand).Instructions);
                sequence.Add(new ArithmeticInstruction(GetUnaryOpCode(GetOperatorString(unaryExp.Operator))));
                break;

            case ast.FuncCallExp funcCall:
                if (funcCall.InvocationArguments != null)
                {
                    foreach (var arg in funcCall.InvocationArguments)
                    {
                        sequence.AddRange(GenerateExpression(arg).Instructions);
                    }
                }

                // Default to a simple Console.WriteLine(object) call for top-level function calls
                var argCountForFunc = funcCall.InvocationArguments?.Count ?? 0;
                sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)") { ArgCount = argCountForFunc });
                break;

            default:
                // Unknown expression: leave sequence empty to avoid stack corruption
                break;
        }

        return sequence;
    }

    // Provide current parameter names so expression lowering can emit ldarg for parameters
    public void SetCurrentParameters(List<string> paramNames)
    {
        _currentParameterNames = new HashSet<string>(paramNames ?? new List<string>(), StringComparer.Ordinal);
    }

    // Provide parameter type hints as (name, typeName) tuples where typeName is like 'System.Int32' or 'Fifth.Generated.Foo'
    public void SetCurrentParameterTypes(List<(string name, string? typeName)> paramInfos)
    {
        _currentParameterTypes.Clear();
        if (paramInfos == null) return;
        foreach (var (name, typeName) in paramInfos)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (string.IsNullOrWhiteSpace(typeName)) continue;
            var mapped = MapBuiltinFifthTypeNameToSystem(typeName) ?? (Type.GetType(typeName));
            if (mapped != null)
            {
                _currentParameterTypes[name] = mapped;
            }
        }
    }

    // Generate instruction sequence for a single statement node
    public InstructionSequence GenerateStatement(ast.Statement statement)
    {
        var seq = new InstructionSequence();
        if (statement == null) return seq;

        switch (statement)
        {
            case ast.VarDeclStatement varDeclStmt:
                var varName = varDeclStmt.VariableDecl?.Name ?? "__var";
                if (varDeclStmt.InitialValue != null)
                {
                    seq.AddRange(GenerateExpression(varDeclStmt.InitialValue).Instructions);
                    seq.Add(new StoreInstruction("stloc", varName));
                    // Try to record the declared type for later inference
                    var tn = varDeclStmt.VariableDecl != null ? varDeclStmt.VariableDecl.TypeName.ToString() : null;
                    var mapped = MapBuiltinFifthTypeNameToSystem(tn);
                    if (mapped != null)
                    {
                        _localVariableTypes[varName] = mapped;
                    }
                }
                break;

            case ast.ExpStatement expStmt:
                if (expStmt.RHS != null) seq.AddRange(GenerateExpression(expStmt.RHS).Instructions);
                // Pop expression result to keep stack balanced for statement position
                seq.Add(new StackInstruction("pop"));
                break;

            case ast.AssignmentStatement assignStmt:
                if (assignStmt.RValue != null) seq.AddRange(GenerateExpression(assignStmt.RValue).Instructions);
                if (assignStmt.LValue is VarRefExp lvr)
                {
                    seq.Add(new StoreInstruction("stloc", lvr.VarName));
                }
                break;

            case ast.ReturnStatement retStmt:
                if (retStmt.ReturnValue != null) seq.AddRange(GenerateExpression(retStmt.ReturnValue).Instructions);
                seq.Add(new ReturnInstruction());
                break;

            default:
                // Unhandled statements are no-ops for now
                break;
        }

        return seq;
    }

    public InstructionSequence GenerateIfStatement(ast.IfElseStatement ifStmt)
    {
        var seq = new InstructionSequence();
        if (ifStmt == null) return seq;

        var condition = ifStmt.Condition;
        var falseLabel = $"IL_false_{_labelCounter++}";
        var endLabel = $"IL_end_{_labelCounter++}";

        if (condition != null) seq.AddRange(GenerateExpression(condition).Instructions);
        seq.Add(new BranchInstruction("brfalse", falseLabel));

        // Then block
        if (ifStmt.ThenBlock?.Statements != null)
        {
            foreach (var s in ifStmt.ThenBlock.Statements)
            {
                seq.AddRange(GenerateStatement(s).Instructions);
            }
        }

        seq.Add(new BranchInstruction("br", endLabel));
        seq.Add(new LabelInstruction(falseLabel));

        // Else block
        if (ifStmt.ElseBlock?.Statements != null)
        {
            foreach (var s in ifStmt.ElseBlock.Statements)
            {
                seq.AddRange(GenerateStatement(s).Instructions);
            }
        }

        seq.Add(new LabelInstruction(endLabel));

        return seq;
    }
}