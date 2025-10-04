using ast;
using il_ast;
using il_ast_generated;
using code_generator.Emit;
using static Fifth.DebugHelpers;

namespace code_generator;

/// <summary>
/// Transforms AST nodes from the Fifth language parser into IL metamodel representations.
/// This visitor follows the transformation stage pattern used in FifthParserManager.ApplyLanguageAnalysisPhases.
/// Now refactored to use modular components for type mapping, inference, and code emission.
/// </summary>
public class AstToIlTransformationVisitor : DefaultRecursiveDescentVisitor
{
    // Modular components for code generation
    private readonly EmitContext _context;
    private readonly TypeMapper _typeMapper;
    private readonly TypeInference _typeInference;
    private readonly ExternalMethodResolver _methodResolver;
    private readonly ExpressionEmitter _expressionEmitter;
    private readonly ControlFlowEmitter _controlFlowEmitter;
    private readonly StatementEmitter _statementEmitter;

    // Legacy fields kept for backward compatibility during transition
    private readonly Dictionary<string, TypeReference> _typeMap = new();
    private int _labelCounter = 0;

    // Safe wrapper for zero-lowering recording; if core helper missing, silently ignore.
    private void SafeRecordZeroLowering(string method, string context, string nodeKind)
    {
        // Placeholder – previously would log zero-lowering events. Left intentionally blank.
        if (DebugEnabled)
        {
            DebugLog($"ZERO-LOWERING method={method} ctx={context} node={nodeKind}");
        }
    }

    public AstToIlTransformationVisitor()
    {
        // Initialize modular components
        _context = new EmitContext();
        _typeMapper = new TypeMapper();
        _typeInference = new TypeInference(_context);
        _methodResolver = new ExternalMethodResolver();
        _expressionEmitter = new ExpressionEmitter(_context, _typeInference, _methodResolver, _typeMapper);
        _controlFlowEmitter = new ControlFlowEmitter(_context, _expressionEmitter);
        _statementEmitter = new StatementEmitter(_context, _expressionEmitter, _controlFlowEmitter);

        // Legacy initialization
        InitializeBuiltinTypes();
    }

    public void SetCurrentMethodName(string? methodName)
    {
        _context.CurrentMethodName = methodName;
        ResetMethodContext();
    }

    private void ResetMethodContext()
    {
        _context.ResetMethodContext();
    }

    public void SetCurrentReturnType(il_ast.TypeReference? returnType)
    {
        _context.CurrentReturnType = returnType;
    }

    private static bool IsImplicitNumericWidening(Type src, Type dest)
    {
        return TypeInference.IsImplicitNumericWidening(src, dest);
    }

    private static int CompatibilityScore(Type? argType, Type paramType)
    {
        if (argType == null) return 0;
        if (paramType == argType) return 100;
        if (paramType.IsAssignableFrom(argType)) return 50;
        if (TypeInference.IsImplicitNumericWidening(argType, paramType)) return 10;
        return 0;
    }

    private static bool IsNumeric(Type t)
    {
        return TypeInference.IsNumeric(t);
    }

    private static Type PromoteNumeric(Type a, Type b)
    {
        return TypeInference.PromoteNumeric(a, b);
    }

    private static Type? MapBuiltinFifthTypeNameToSystem(string? name)
    {
        return TypeMapper.MapBuiltinFifthTypeNameToSystem(name);
    }

    private Type? InferExpressionType(ast.Expression expr)
    {
        return _typeInference.InferExpressionType(expr);
    }

    private System.Reflection.MethodInfo? ResolveExternalMethod(Type extType, string methodName, IList<ast.Expression> args, Type? receiverType = null)
    {
        return _methodResolver.ResolveExternalMethod(_typeInference, extType, methodName, args, receiverType);
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

        _context.CurrentAssembly = ilAssembly;

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

        _context.CurrentModule = ilModule;

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
            ParentAssembly = _context.CurrentAssembly!
        };

        _context.CurrentClass = ilClass;

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
            TheType = _typeMapper.MapType(astProperty.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _context.CurrentClass!,
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
            TheType = _typeMapper.MapType(astField.TypeName.Value?.ToString() ?? "object"),
            ParentClass = _context.CurrentClass!,
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
            ParentClass = _context.CurrentClass!,
            TypeOfMember = MemberType.Method,
            Visibility = MemberAccessability.Public,
            IsStatic = _context.CurrentClass == null || astFunction.IsStatic,
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
            ReturnTypeSignature = _typeMapper.MapType(returnTypeName),
            NumberOfParameters = (ushort)(astFunction.Params?.Count ?? 0)
        };

        if (astFunction.Params != null)
        {
            foreach (var param in astFunction.Params)
            {
                signature.ParameterSignatures.Add(new ParameterSignature
                {
                    Name = param.Name ?? "param",
                    TypeReference = _typeMapper.MapType(param.TypeName.Value?.ToString() ?? "object"),
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
            "!=" => "ceq_neg", // composite handled in GenerateExpression
            "<" => "clt",
            ">" => "cgt",
            "<=" => "cle",   // composite handled in GenerateExpression
            ">=" => "cge",   // composite handled in GenerateExpression
            "&&" => "and",
            "||" => "or",
            "|" => "or",
            "xor" => "xor",
            _ => "add",
        };
    }

    private string GetUnaryOpCode(string op)
    {
        return op switch
        {
            "-" => "neg",
            "!" => "not", // composite handled in GenerateExpression
            _ => "nop",
        };
    }

    private InstructionSequence GenerateFuncCall(ast.FuncCallExp funcCall, ast.Expression? qualifier)
    {
        var sequence = new InstructionSequence();
        if (funcCall == null) return sequence;

        var invocationArgs = funcCall.InvocationArguments ?? new List<ast.Expression>();

        if (funcCall.FunctionDef != null)
        {
            foreach (var arg in invocationArgs)
            {
                sequence.AddRange(GenerateExpression(arg).Instructions);
            }

            var targetName = funcCall.FunctionDef.Name.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(targetName) && funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var fnObj) && fnObj is string resolvedName)
            {
                targetName = resolvedName;
            }
            if (string.IsNullOrWhiteSpace(targetName))
            {
                targetName = "unknown";
            }

            sequence.Add(new CallInstruction("call", targetName) { ArgCount = invocationArgs.Count });
            return sequence;
        }

        if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType)
        {
            var methodName = ExtractExternalMethodName(funcCall);
            var receiverType = qualifier != null ? InferExpressionType(qualifier) : null;
            System.Reflection.MethodInfo? resolvedMethod = null;
            try
            {
                resolvedMethod = ResolveExternalMethod(extType, methodName, invocationArgs, receiverType);
            }
            catch
            {
                // Reflection resolution failure is tolerated; fallback path handles it.
            }

            if (resolvedMethod != null)
            {
                var parameters = resolvedMethod.GetParameters();
                var expectsReceiver = ShouldUseQualifierAsReceiver(resolvedMethod, qualifier, receiverType, parameters, invocationArgs.Count);
                var emittedArgCount = 0;

                if (expectsReceiver && qualifier != null)
                {
                    sequence.AddRange(GenerateExpression(qualifier).Instructions);
                    emittedArgCount++;
                }

                var suppliedIndex = 0;
                var startParam = expectsReceiver ? 1 : 0;
                for (int pi = startParam; pi < parameters.Length; pi++)
                {
                    if (suppliedIndex < invocationArgs.Count)
                    {
                        sequence.AddRange(GenerateExpression(invocationArgs[suppliedIndex]).Instructions);
                        suppliedIndex++;
                        emittedArgCount++;
                    }
                    else if (parameters[pi].IsOptional)
                    {
                        EmitDefaultForParameter(sequence, parameters[pi]);
                        emittedArgCount++;
                    }
                    else
                    {
                        EmitDefaultForType(sequence, parameters[pi].ParameterType);
                        emittedArgCount++;
                    }
                }

                for (int extra = suppliedIndex; extra < invocationArgs.Count; extra++)
                {
                    sequence.AddRange(GenerateExpression(invocationArgs[extra]).Instructions);
                    emittedArgCount++;
                }

                var signature = BuildExternalCallSignature(resolvedMethod, extType);
                sequence.Add(new CallInstruction("call", signature) { ArgCount = emittedArgCount });
                return sequence;
            }
            else
            {
                var emittedArgCount = 0;
                if (qualifier != null && receiverType != null)
                {
                    sequence.AddRange(GenerateExpression(qualifier).Instructions);
                    emittedArgCount++;
                }

                foreach (var arg in invocationArgs)
                {
                    sequence.AddRange(GenerateExpression(arg).Instructions);
                    emittedArgCount++;
                }

                var fallbackSig = BuildFallbackExternalSignature(extType, methodName, emittedArgCount);
                sequence.Add(new CallInstruction("call", fallbackSig) { ArgCount = emittedArgCount });
                return sequence;
            }
        }

        foreach (var arg in invocationArgs)
        {
            sequence.AddRange(GenerateExpression(arg).Instructions);
        }

        string fallbackName = "void [System.Console]System.Console::WriteLine(object)";
        if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("FunctionName", out var genericNameObj) && genericNameObj is string genericName && !string.IsNullOrWhiteSpace(genericName))
        {
            fallbackName = genericName;
        }
        else if (funcCall.Annotations != null && funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extName && !string.IsNullOrWhiteSpace(extName))
        {
            fallbackName = extName;
        }

        sequence.Add(new CallInstruction("call", fallbackName) { ArgCount = invocationArgs.Count });
        return sequence;
    }

    private static string ExtractExternalMethodName(ast.FuncCallExp funcCall)
    {
        if (funcCall.Annotations != null)
        {
            if (funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && extNameObj is string extName && !string.IsNullOrWhiteSpace(extName))
            {
                return extName;
            }
            if (funcCall.Annotations.TryGetValue("FunctionName", out var fnObj) && fnObj is string fn && !string.IsNullOrWhiteSpace(fn))
            {
                return fn;
            }
        }
        var funcDefName = funcCall.FunctionDef?.Name.Value;
        if (!string.IsNullOrWhiteSpace(funcDefName))
        {
            return funcDefName!;
        }
        return "Method";
    }

    private static bool ShouldUseQualifierAsReceiver(System.Reflection.MethodInfo methodInfo, ast.Expression? qualifier, Type? receiverType, System.Reflection.ParameterInfo[] parameters, int suppliedArgCount)
    {
        if (qualifier == null || parameters.Length == 0)
        {
            return false;
        }

        if (receiverType != null && parameters[0].ParameterType.IsAssignableFrom(receiverType))
        {
            return true;
        }

        if (methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), inherit: false))
        {
            return true;
        }

        return parameters.Length > suppliedArgCount;
    }

    private static string BuildExternalCallSignature(System.Reflection.MethodInfo methodInfo, Type fallbackType)
    {
        var declaring = methodInfo.DeclaringType ?? fallbackType;
        var asmName = declaring.Assembly.GetName().Name ?? "System.Runtime";
        var ns = declaring.Namespace ?? string.Empty;
        var typeName = (declaring.Name ?? fallbackType.Name ?? "Type").Replace('+', '.');

        static string FormatToken(Type t, string defaultAssembly)
        {
            if (t.IsByRef) t = t.GetElementType() ?? typeof(object);
            if (t == typeof(void)) return "System.Void";
            if (t == typeof(int)) return "System.Int32";
            if (t == typeof(string)) return "System.String";
            if (t == typeof(float)) return "System.Single";
            if (t == typeof(double)) return "System.Double";
            if (t == typeof(bool)) return "System.Boolean";
            if (t == typeof(long)) return "System.Int64";
            if (t == typeof(short)) return "System.Int16";
            if (t == typeof(byte)) return "System.Byte";
            if (t == typeof(char)) return "System.Char";
            if (t == typeof(object)) return "System.Object";
            if (t == typeof(decimal)) return "System.Decimal";

            var fullName = t.FullName?.Replace('+', '.') ?? t.Name;
            var ownerAsm = t.Assembly.GetName().Name ?? defaultAssembly;
            if ((t.Namespace ?? string.Empty).StartsWith("System", StringComparison.Ordinal))
            {
                return fullName ?? t.Name;
            }
            return $"{fullName}@{ownerAsm}";
        }

        var parameters = methodInfo.GetParameters();
        var paramTokens = parameters.Select(p => FormatToken(p.ParameterType, asmName)).ToList();
        var returnToken = FormatToken(methodInfo.ReturnType, asmName);

        var joinedParams = paramTokens.Count > 0 ? string.Join(',', paramTokens) : string.Empty;
        return $"extcall:Asm={asmName};Ns={ns};Type={typeName};Method={methodInfo.Name};Params={joinedParams};Return={returnToken}";
    }

    private static string BuildFallbackExternalSignature(Type extType, string methodName, int argCount)
    {
        var asmName = extType.Assembly.GetName().Name ?? "System.Runtime";
        var ns = extType.Namespace ?? string.Empty;
        var typeName = (extType.Name ?? "Type").Replace('+', '.');
        var paramList = argCount > 0 ? string.Join(',', Enumerable.Repeat("System.Object@" + asmName, argCount)) : string.Empty;
        return $"extcall:Asm={asmName};Ns={ns};Type={typeName};Method={methodName};Params={paramList};Return=System.Void";
    }

    private void EmitDefaultForParameter(InstructionSequence sequence, System.Reflection.ParameterInfo parameter)
    {
        object? defaultValue = null;
        try
        {
            defaultValue = parameter.DefaultValue;
        }
        catch
        {
            defaultValue = null;
        }

        if (defaultValue == System.DBNull.Value || defaultValue == System.Reflection.Missing.Value)
        {
            defaultValue = null;
        }

        if (defaultValue != null)
        {
            EmitConstant(sequence, parameter.ParameterType, defaultValue);
        }
        else
        {
            EmitDefaultForType(sequence, parameter.ParameterType);
        }
    }

    private void EmitDefaultForType(InstructionSequence sequence, Type parameterType)
    {
        if (parameterType == typeof(void))
        {
            return;
        }

        if (!parameterType.IsValueType)
        {
            sequence.Add(new LoadInstruction("ldnull"));
            return;
        }

        if (parameterType == typeof(bool))
        {
            sequence.Add(new LoadInstruction("ldc.i4", 0));
            return;
        }

        if (parameterType == typeof(float))
        {
            sequence.Add(new LoadInstruction("ldc.r4", 0f));
            return;
        }

        if (parameterType == typeof(double))
        {
            sequence.Add(new LoadInstruction("ldc.r8", 0d));
            return;
        }

        if (parameterType == typeof(decimal))
        {
            sequence.Add(new LoadInstruction("ldstr", 0m.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal") { ArgCount = 1 });
            return;
        }

        sequence.Add(new LoadInstruction("ldc.i4", 0));
    }

    private void EmitConstant(InstructionSequence sequence, Type targetType, object? value)
    {
        if (targetType == typeof(string))
        {
            sequence.Add(new LoadInstruction("ldstr", value?.ToString() ?? string.Empty));
            return;
        }

        if (targetType == typeof(bool))
        {
            var boolVal = value is bool b && b ? 1 : 0;
            sequence.Add(new LoadInstruction("ldc.i4", boolVal));
            return;
        }

        if (targetType == typeof(float) || targetType == typeof(double))
        {
            var dbl = value != null ? Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) : 0d;
            if (targetType == typeof(float))
            {
                sequence.Add(new LoadInstruction("ldc.r4", (float)dbl));
            }
            else
            {
                sequence.Add(new LoadInstruction("ldc.r8", dbl));
            }
            return;
        }

        if (targetType == typeof(decimal))
        {
            var decVal = value is decimal dec ? dec : Convert.ToDecimal(value ?? 0m, System.Globalization.CultureInfo.InvariantCulture);
            sequence.Add(new LoadInstruction("ldstr", decVal.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal") { ArgCount = 1 });
            return;
        }

        if (targetType == typeof(char))
        {
            var charVal = value is char c ? c : Convert.ToChar(value ?? '\0', System.Globalization.CultureInfo.InvariantCulture);
            sequence.Add(new LoadInstruction("ldc.i4", (int)charVal));
            return;
        }

        if (targetType.IsEnum)
        {
            var enumUnderlying = value != null ? Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture) : 0;
            sequence.Add(new LoadInstruction("ldc.i4", enumUnderlying));
            return;
        }

        if (targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(uint) || targetType == typeof(int) || targetType == typeof(short) || targetType == typeof(byte) || targetType == typeof(sbyte) || targetType == typeof(ushort))
        {
            var intValue = value != null ? Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture) : 0L;
            if (intValue < int.MinValue || intValue > int.MaxValue)
            {
                intValue = 0;
            }
            sequence.Add(new LoadInstruction("ldc.i4", (int)intValue));
            return;
        }

        if (!targetType.IsValueType)
        {
            sequence.Add(new LoadInstruction("ldnull"));
            return;
        }

        sequence.Add(new LoadInstruction("ldc.i4", 0));
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
    /// Delegates to ExpressionEmitter for modular code generation
    /// </summary>
    public InstructionSequence GenerateExpression(ast.Expression expression)
    {
        return _expressionEmitter.GenerateExpression(expression);
    }

    // Provide current parameter names so expression lowering can emit ldarg for parameters
    public void SetCurrentParameters(List<string> paramNames)
    {
        _context.ParameterNames = new HashSet<string>(paramNames ?? new List<string>(), StringComparer.Ordinal);
    }

    // Provide parameter type hints as (name, typeName) tuples where typeName is like 'System.Int32' or 'Fifth.Generated.Foo'
    public void SetCurrentParameterTypes(List<(string name, string? typeName)> paramInfos)
    {
        _context.ParameterTypes.Clear();
        if (paramInfos == null) return;
        foreach (var (name, typeName) in paramInfos)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (string.IsNullOrWhiteSpace(typeName)) continue;
            var mapped = MapBuiltinFifthTypeNameToSystem(typeName) ?? (Type.GetType(typeName));
            if (mapped != null)
            {
                _context.ParameterTypes[name] = mapped;
            }
        }
    }

    // Generate instruction sequence for a single statement node
    // Delegates to StatementEmitter for modular code generation
    public InstructionSequence GenerateStatement(ast.Statement statement)
    {
        return _statementEmitter.GenerateStatement(statement);
    }

    // Delegates to ControlFlowEmitter for modular code generation
    public InstructionSequence GenerateIfStatement(ast.IfElseStatement ifStmt)
    {
        return _controlFlowEmitter.GenerateIfStatement(ifStmt);
    }

    // Delegates to ControlFlowEmitter for modular code generation
    public InstructionSequence GenerateWhileStatement(ast.WhileStatement whileStmt)
    {
        return _controlFlowEmitter.GenerateWhileStatement(whileStmt);
    }
}