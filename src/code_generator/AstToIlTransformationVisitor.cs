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
    private readonly Dictionary<string, Type> _currentParameterTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> _localVariableTypes = new(StringComparer.Ordinal);

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

    private System.Reflection.MethodInfo? ResolveExternalMethod(Type extType, string methodName, IList<ast.Expression> args)
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
            .Where(x => x.ps.Length == argCount || (x.ps.Length > argCount && x.ps.Skip(argCount).All(p => p.IsOptional || p.HasDefaultValue)))
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
            score = Enumerable.Range(0, Math.Min(argCount, x.ps.Length))
                .Sum(i => CompatibilityScore(inferred[i], x.ps[i].ParameterType)),
            isGeneric = x.mi.IsGenericMethodDefinition
        })
        .OrderByDescending(s => s.score)
        .ThenBy(s => s.ps.Length) // prefer fewer total params (i.e., fewer optionals)
        .ThenBy(s => s.isGeneric) // prefer non-generic over generic
        .ToList();

        // If no arguments supplied, any arity-compatible (zero-param) candidate is acceptable
        if (argCount == 0 && scored.Count > 0)
        {
            return scored[0].mi;
        }

        // Reject if best score indicates incompatible arguments for supplied args
        if (scored.Count == 0 || scored[0].score <= 0)
        {
            return null;
        }

        // Additionally ensure each supplied argument is compatible
        var best = scored[0];
        for (int i = 0; i < Math.Min(argCount, best.ps.Length); i++)
        {
            if (CompatibilityScore(inferred[i], best.ps[i].ParameterType) <= 0)
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
            case ast.ListComprehension lc:
                // Minimal lowering: create an empty int32 array for now
                sequence.Add(new LoadInstruction("ldc.i4", 0));
                sequence.Add(new LoadInstruction("newarr", "int32"));
                break;
            case GraphAssertionBlockExp gab:
                // Lower graph block expression to a concrete graph via KG.CreateGraph(),
                // evaluate inner statements (no-ops in IL yet), and leave the graph on stack.
                // Emit extcall to Fifth.System.KG.CreateGraph()
                sequence.Add(new CallInstruction("call", "extcall:Asm=Fifth.System;Ns=Fifth.System;Type=KG;Method=CreateGraph;Params=;Return=VDS.RDF.IGraph@dotNetRDF") { ArgCount = 0 });
                // For now, we do not lower inner assertions; future work can duplicate and assert.
                break;
            case ast.ListLiteral listLit:
                // Lower list literal to an int32 array for now
                // Pattern: ldc.i4 <len>; newarr int32; dup; init elements with stelem.i4
                var elems = listLit.ElementExpressions ?? new List<ast.Expression>();
                var count = elems.Count;
                sequence.Add(new LoadInstruction("ldc.i4", count));
                sequence.Add(new LoadInstruction("newarr", "int32"));
                for (int i = 0; i < count; i++)
                {
                    sequence.Add(new LoadInstruction("dup"));
                    sequence.Add(new LoadInstruction("ldc.i4", i));
                    var elem = elems[i];
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
                // Pass through raw literal; emitter will handle unescaping and trimming quotes
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
                DebugLog($"DEBUG: Processing BinaryExp with LHS: {binaryExp.LHS?.GetType().Name ?? "null"}, RHS: {binaryExp.RHS?.GetType().Name ?? "null"}, Operator: {binaryExp.Operator.ToString()}");
                // Special handling for exponentiation
                if (binaryExp.Operator == Operator.ArithmeticPow)
                {
                    // Constant fold for common int literal cases to keep stack types int32
                    if (binaryExp.LHS is Int32LiteralExp li && binaryExp.RHS is Int32LiteralExp ri)
                    {
                        int baseVal = li.Value;
                        int expVal = ri.Value;
                        int res = 1;
                        if (expVal < 0)
                        {
                            // Negative exponents yield 0 for int folding in our minimal semantics
                            res = 0;
                        }
                        else
                        {
                            for (int i = 0; i < expVal; i++) res *= baseVal;
                        }
                        sequence.Add(new LoadInstruction("ldc.i4", res));
                        break;
                    }

                    // General numeric case: promote both to double and call System.Math.Pow(double,double)
                    var lhsSeq = binaryExp.LHS != null ? GenerateExpression(binaryExp.LHS).Instructions : new List<il_ast.CilInstruction>();
                    var rhsSeq = binaryExp.RHS != null ? GenerateExpression(binaryExp.RHS).Instructions : new List<il_ast.CilInstruction>();
                    var lt = binaryExp.LHS != null ? InferExpressionType(binaryExp.LHS) : null;
                    var rt = binaryExp.RHS != null ? InferExpressionType(binaryExp.RHS) : null;

                    string? TypeToToken(Type? t)
                    {
                        if (t == typeof(int)) return "System.Int32";
                        if (t == typeof(float)) return "System.Single";
                        if (t == typeof(double)) return "System.Double";
                        if (t == typeof(long)) return "System.Int64";
                        return null;
                    }

                    void MaybeConvertTopToDouble(Type? t)
                    {
                        if (t == typeof(double)) return;
                        var tok = TypeToToken(t);
                        if (tok == null) return;
                        sequence.Add(new CallInstruction("call", $"extcall:Asm=System.Runtime;Ns=System;Type=Convert;Method=ToDouble;Params={tok};Return=System.Double") { ArgCount = 1 });
                    }

                    // Emit LHS then convert if needed
                    foreach (var ins in lhsSeq) sequence.Add(ins);
                    MaybeConvertTopToDouble(lt);
                    // Emit RHS then convert if needed
                    foreach (var ins in rhsSeq) sequence.Add(ins);
                    MaybeConvertTopToDouble(rt);
                    // Call Math.Pow(double,double)
                    sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Math;Method=Pow;Params=System.Double,System.Double;Return=System.Double") { ArgCount = 2 });
                    break;
                }

                // Short-circuit logical operations
                if (binaryExp.Operator == Operator.LogicalAnd || binaryExp.Operator == Operator.LogicalOr)
                {
                    var endLabel = $"IL_end_{_labelCounter++}";
                    var skipRhsLabel = $"IL_skip_{_labelCounter++}";
                    // Emit LHS
                    if (binaryExp.LHS != null)
                    {
                        sequence.AddRange(GenerateExpression(binaryExp.LHS).Instructions);
                    }
                    // Duplicate LHS for branching decision
                    sequence.Add(new LoadInstruction("dup"));
                    if (binaryExp.Operator == Operator.LogicalAnd)
                    {
                        // if lhs == 0, skip RHS and leave 0; else pop dup, eval RHS
                        sequence.Add(new BranchInstruction("brfalse", skipRhsLabel));
                        // consume duplicated lhs before evaluating rhs
                        sequence.Add(new StackInstruction("pop"));
                        if (binaryExp.RHS != null)
                        {
                            sequence.AddRange(GenerateExpression(binaryExp.RHS).Instructions);
                        }
                        sequence.Add(new LabelInstruction(skipRhsLabel));
                    }
                    else
                    {
                        // LogicalOr: if lhs != 0, skip RHS and keep 1; else pop dup and eval RHS
                        sequence.Add(new BranchInstruction("brtrue", skipRhsLabel));
                        // consume duplicated lhs before evaluating rhs
                        sequence.Add(new StackInstruction("pop"));
                        if (binaryExp.RHS != null)
                        {
                            sequence.AddRange(GenerateExpression(binaryExp.RHS).Instructions);
                        }
                        sequence.Add(new LabelInstruction(skipRhsLabel));
                    }
                    break;
                }

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
                    // If this is string concatenation, emit a call to System.String.Concat(object, object)
                    var isAdd = binaryExp.Operator == Operator.ArithmeticAdd;
                    var lt = binaryExp.LHS != null ? InferExpressionType(binaryExp.LHS) : null;
                    var rt = binaryExp.RHS != null ? InferExpressionType(binaryExp.RHS) : null;
                    if (isAdd && (lt == typeof(string) || rt == typeof(string)))
                    {
                        // Use the System.String.Concat(string, string) overload to avoid boxing where possible
                        sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=String;Method=Concat;Params=System.String,System.String;Return=System.String") { ArgCount = 2 });
                    }
                    else
                    {
                        sequence.Add(new ArithmeticInstruction(GetBinaryOpCode(GetOperatorString(binaryExp.Operator))));
                    }
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
                var argCountForFunc = funcCall.InvocationArguments?.Count ?? 0;
                if (functionName == "print" || functionName == "myprint")
                {
                    sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)") { ArgCount = argCountForFunc });
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
                    sequence.Add(new CallInstruction("call", $"{ilReturnType} {functionName}()") { ArgCount = argCountForFunc });
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
                    // Build accurate extcall signature using reflection for param/return types
                    string TypeToToken(Type t)
                    {
                        if (t == typeof(void)) return "System.Void";
                        if (t.Namespace != null && t.Namespace.StartsWith("System", StringComparison.Ordinal))
                        {
                            return $"{t.Namespace}.{t.Name}";
                        }
                        var asm = t.Assembly.GetName().Name ?? string.Empty;
                        var full = (t.FullName ?? ($"{t.Namespace}.{t.Name}"))
                            .Replace('+', '.'); // nested types use '+' in FullName
                        return $"{full}@{asm}";
                    }
                    var argCount = (extCall.InvocationArguments?.Count) ?? 0;
                    // Special-case: std.print previously mapped to Console.WriteLine via linkage
                    if (extType == typeof(System.Console) && string.Equals(mName, "WriteLine", StringComparison.Ordinal))
                    {
                        // Prefer WriteLine(string) overload for string arguments; fallback to object otherwise.
                        var useString = false;
                        if ((extCall.InvocationArguments?.Count ?? 0) == 1)
                        {
                            var t0 = InferExpressionType(extCall.InvocationArguments![0]);
                            useString = t0 == typeof(string);
                        }
                        var sigPrint = useString
                            ? "extcall:Asm=System.Console;Ns=System;Type=Console;Method=WriteLine;Params=System.String;Return=System.Void"
                            : "extcall:Asm=System.Console;Ns=System;Type=Console;Method=WriteLine;Params=System.Object;Return=System.Void";
                        sequence.Add(new CallInstruction("call", sigPrint) { ArgCount = argCount });
                        break;
                    }
                    var chosen = ResolveExternalMethod(extType, mName, extCall.InvocationArguments ?? new List<ast.Expression>());
                    if (chosen == null)
                    {
                        // Construct a helpful error message with inferred arg types
                        var inferredTypes = new List<string>();
                        for (int i = 0; i < argCount; i++)
                        {
                            var t = InferExpressionType(extCall.InvocationArguments![i]);
                            inferredTypes.Add(t?.FullName ?? "<unknown>");
                        }
                        var inferredSig = string.Join(", ", inferredTypes);
                        throw new ast_model.CompilationException($"No matching overload found for {extType.FullName}.{mName}({inferredSig})");
                    }
                    var paramTokens = new List<string>();
                    var returnToken = "System.Object";
                    var psChosen = chosen.GetParameters();
                    // Only emit tokens for actually supplied arguments to keep signature consistent
                    for (int i = 0; i < Math.Min(argCount, psChosen.Length); i++)
                    {
                        paramTokens.Add(TypeToToken(psChosen[i].ParameterType));
                    }
                    returnToken = TypeToToken(chosen.ReturnType);

                    var paramList = string.Join(",", paramTokens);
                    // Use declaring assembly name for accurate references
                    var asmName = chosen.DeclaringType?.Assembly?.GetName()?.Name ?? (extType.Assembly?.GetName()?.Name ?? "Fifth.System");
                    var sig = $"extcall:Asm={asmName};Ns={extType.Namespace};Type={extType.Name};Method={mName};Params={paramList};Return={returnToken}";
                    sequence.Add(new CallInstruction("call", sig) { ArgCount = argCount });
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
                            // LHS is not a known array source; consume it and produce a safe default value
                            // to keep expression semantics valid and avoid stack corruption.
                            sequence.Add(new StackInstruction("pop"));
                            sequence.Add(new LoadInstruction("ldc.i4", 0));
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
                sequence.Add(new CallInstruction("newobj", $"instance void {typeName}::.ctor()") { ArgCount = 0 });

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
                    // Track inferred type from initializer; fall back to declared type if available
                    var inferred = InferExpressionType(varDecl.InitialValue);
                    if (inferred != null)
                    {
                        _localVariableTypes[localName] = inferred;
                    }
                    else if (varDecl.VariableDecl != null)
                    {
                        var dtn = varDecl.VariableDecl.TypeName.Value;
                        var mapped = MapBuiltinFifthTypeNameToSystem(dtn) ?? ResolveTypeByFullName(dtn);
                        if (mapped != null) _localVariableTypes[localName] = mapped;
                    }
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
                        // Update local variable type from RHS
                        var rhsType = InferExpressionType(assignment.RValue);
                        if (rhsType != null)
                        {
                            _localVariableTypes[varRef.VarName] = rhsType;
                        }
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
                var exprSeq = GenerateExpression(exprStmt.RHS);
                sequence.AddRange(exprSeq.Instructions);
                var netDelta = ComputeStackDelta(exprSeq);
                if (netDelta > 0)
                {
                    for (int i = 0; i < netDelta; i++)
                    {
                        sequence.Add(new StackInstruction("pop"));
                    }
                }
                break;
        }

        return sequence;
    }

    private int ComputeStackDelta(InstructionSequence seq)
    {
        // Short-circuit logical expressions introduce branches; treat as single-value result
        if (seq.Instructions.Any(i => i is il_ast.BranchInstruction))
        {
            return 1;
        }
        int delta = 0;
        foreach (var ins in seq.Instructions)
        {
            switch (ins)
            {
                case LoadInstruction li:
                    switch ((li.Opcode ?? string.Empty).ToLowerInvariant())
                    {
                        case "ldc.i4":
                        case "ldc.r4":
                        case "ldc.r8":
                        case "ldstr":
                        case "ldnull":
                        case "ldloc":
                        case "ldarg":
                        case "dup":
                            delta += 1; break;
                        case "newarr":
                            // newarr consumes a size and pushes an array ref -> net 0
                            break;
                        case "ldelem.i4":
                            // ldelem consumes array ref and index, pushes element -> net -1
                            delta -= 1; break;
                        case "ldfld":
                            // consumes obj, pushes value => net 0
                            break;
                    }
                    break;
                case StoreInstruction si:
                    switch ((si.Opcode ?? string.Empty).ToLowerInvariant())
                    {
                        case "stloc": delta -= 1; break;
                        case "starg": delta -= 1; break;
                        case "stfld": delta -= 2; break;
                        case "stelem.i4": delta -= 3; break;
                    }
                    break;
                case ArithmeticInstruction ai:
                    switch ((ai.Opcode ?? string.Empty).ToLowerInvariant())
                    {
                        // binary ops: net -1 (two inputs -> one output)
                        case "add":
                        case "sub":
                        case "mul":
                        case "div":
                        case "ceq":
                        case "clt":
                        case "cgt":
                        case "cle":
                        case "cge":
                        case "and":
                        case "or":
                        case "xor":
                        case "ceq_neg":
                            delta -= 1; break;
                        // unary ops: net 0
                        case "not":
                        case "neg":
                            break;
                    }
                    break;
                case CallInstruction ci:
                    {
                        var opc = (ci.Opcode ?? string.Empty).ToLowerInvariant();
                        if (opc == "newobj")
                        {
                            delta += 1; // pushes object ref
                            break;
                        }
                        int retPush = 0;
                        var sig = ci.MethodSignature ?? string.Empty;
                        // Determine return type for common formats
                        string rt;
                        if (sig.StartsWith("extcall:", StringComparison.Ordinal))
                        {
                            // extcall: ...;Return=System.Int32
                            var idx = sig.IndexOf("Return=", StringComparison.Ordinal);
                            if (idx >= 0)
                            {
                                rt = sig.Substring(idx + "Return=".Length).Trim();
                                // strip anything after ';'
                                var sc = rt.IndexOf(';');
                                if (sc >= 0) rt = rt.Substring(0, sc);
                                rt = rt.ToLowerInvariant();
                            }
                            else rt = string.Empty;
                        }
                        else
                        {
                            // Simple parse: leading return token before space
                            rt = sig.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? string.Empty;
                        }
                        if (!string.IsNullOrEmpty(rt) && rt != "void" && rt != "system.void")
                        {
                            retPush = 1;
                        }
                        delta += retPush - Math.Max(0, ci.ArgCount);
                    }
                    break;
                case StackInstruction si2:
                    if (string.Equals(si2.Opcode, "pop", StringComparison.OrdinalIgnoreCase))
                    {
                        delta -= 1;
                    }
                    break;
            }
        }
        return delta;
    }

    public void SetCurrentParameters(IEnumerable<string> parameterNames)
    {
        _currentParameterNames = new HashSet<string>(parameterNames ?? Array.Empty<string>(), StringComparer.Ordinal);
    }

    public void SetCurrentParameterTypes(IEnumerable<(string name, string? typeName)> parameterInfos)
    {
        _currentParameterTypes.Clear();
        if (parameterInfos == null) return;
        foreach (var (name, typeName) in parameterInfos)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            var t = MapBuiltinFifthTypeNameToSystem(typeName) ?? ResolveTypeByFullName(typeName);
            if (t != null)
            {
                _currentParameterTypes[name] = t;
            }
        }
    }

    private static Type? ResolveTypeByFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        var direct = Type.GetType(fullName!, throwOnError: false, ignoreCase: false);
        if (direct != null) return direct;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var tt = asm.GetType(fullName!, throwOnError: false, ignoreCase: false);
                if (tt != null) return tt;
            }
            catch { /* ignore */ }
        }
        // Common external map
        if (string.Equals(fullName, "VDS.RDF.IGraph", StringComparison.Ordinal))
        {
            var alt = Type.GetType("VDS.RDF.IGraph, dotNetRDF", throwOnError: false, ignoreCase: false);
            if (alt != null) return alt;
        }
        return null;
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
            "|" => "or",
            "||" => "or",
            "xor" => "xor",
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