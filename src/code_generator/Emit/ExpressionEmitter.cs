using ast;
using il_ast;
using static Fifth.DebugHelpers;

namespace code_generator.Emit;

/// <summary>
/// Generates IL instruction sequences for Fifth AST expressions.
/// Handles literals, variables, binary/unary operations, member access, and function calls.
/// </summary>
public class ExpressionEmitter
{
    private readonly EmitContext _context;
    private readonly TypeInference _typeInference;
    private readonly ExternalMethodResolver _methodResolver;
    private readonly TypeMapper _typeMapper;

    public ExpressionEmitter(
        EmitContext context,
        TypeInference typeInference,
        ExternalMethodResolver methodResolver,
        TypeMapper typeMapper)
    {
        _context = context;
        _typeInference = typeInference;
        _methodResolver = methodResolver;
        _typeMapper = typeMapper;
    }

    /// <summary>
    /// Generates instruction sequence for an expression that evaluates to a value on the stack
    /// </summary>
    public InstructionSequence GenerateExpression(Expression? expression)
    {
        var sequence = new InstructionSequence();
        
        if (expression == null)
        {
            if (DebugEnabled)
            {
                DebugLog($"WARNING: Null expression in GenerateExpression");
            }
            return sequence;
        }

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
                GenerateVarRef(sequence, varRef);
                break;

            case BinaryExp binaryExp:
                GenerateBinaryExpression(sequence, binaryExp);
                break;

            case UnaryExp unaryExp:
                GenerateUnaryExpression(sequence, unaryExp);
                break;

            case MemberAccessExp memberAccess:
                GenerateMemberAccess(sequence, memberAccess);
                break;

            case FuncCallExp funcCall:
                GenerateFuncCall(sequence, funcCall, null);
                break;

            case ObjectInitializerExp objectInit:
                GenerateObjectInitializer(sequence, objectInit);
                break;

            case ListLiteral listLiteral:
                GenerateListLiteral(sequence, listLiteral);
                break;

            case ListComprehension listComp:
                GenerateListComprehension(sequence, listComp);
                break;

            case TripleLiteralExp tripleLit:
                GenerateTripleLiteral(sequence, tripleLit);
                break;

            default:
                if (DebugEnabled)
                {
                    DebugLog($"Unknown expression kind '{expression.GetType().Name}'");
                }
                break;
        }

        return sequence;
    }

    private void GenerateVarRef(InstructionSequence sequence, VarRefExp varRef)
    {
        if (DebugEnabled)
        {
            DebugLog($"VarRef load '{varRef.VarName}' param={_context.ParameterNames.Contains(varRef.VarName)}");
        }
        
        if (_context.ParameterNames.Contains(varRef.VarName))
        {
            sequence.Add(new LoadInstruction("ldarg", varRef.VarName));
        }
        else
        {
            sequence.Add(new LoadInstruction("ldloc", varRef.VarName));
        }
    }

    private void GenerateBinaryExpression(InstructionSequence sequence, BinaryExp binaryExp)
    {
        if (DebugEnabled)
        {
            var opStr = OperatorMapper.GetBinaryOpCode(binaryExp.Operator);
            DebugLog($"Binary start op='{opStr}' lhs={(binaryExp.LHS?.GetType().Name ?? "null")} rhs={(binaryExp.RHS?.GetType().Name ?? "null")}");
        }

        // Check if this is string concatenation (+ operator with string operands)
        bool isStringConcat = false;
        if (binaryExp.Operator == Operator.ArithmeticAdd)
        {
            // Check if either operand is a string
            var lhsIsString = binaryExp.LHS?.Type?.Name.Value == "string" || binaryExp.LHS is StringLiteralExp;
            var rhsIsString = binaryExp.RHS?.Type?.Name.Value == "string" || binaryExp.RHS is StringLiteralExp;
            isStringConcat = lhsIsString || rhsIsString;
        }

        if (isStringConcat)
        {
            // Generate string concatenation using String.Concat
            // Generate LHS
            if (binaryExp.LHS != null)
            {
                var lhsSeq = GenerateExpression(binaryExp.LHS);
                sequence.AddRange(lhsSeq.Instructions);
            }

            // Generate RHS
            if (binaryExp.RHS != null)
            {
                var rhsSeq = GenerateExpression(binaryExp.RHS);
                sequence.AddRange(rhsSeq.Instructions);
            }

            // Call String.Concat(string, string)
            // Note: We use System.Runtime as the assembly ref, which is a facade that forwards to System.Private.CoreLib
            sequence.Add(new CallInstruction("call", "extcall:Asm=System.Private.CoreLib;Ns=System;Type=String;Method=Concat;Params=System.String,System.String;Return=System.String") { ArgCount = 2 });
        }
        else
        {
            // Generate LHS
            if (binaryExp.LHS != null)
            {
                var lhsSeq = GenerateExpression(binaryExp.LHS);
                sequence.AddRange(lhsSeq.Instructions);
                
                // Convert LHS to double if this is a power operation
                if (binaryExp.Operator == Operator.ArithmeticPow)
                {
                    sequence.Add(new ArithmeticInstruction("conv.r8"));
                }
            }

            // Generate RHS
            if (binaryExp.RHS != null)
            {
                var rhsSeq = GenerateExpression(binaryExp.RHS);
                sequence.AddRange(rhsSeq.Instructions);
                
                // Convert RHS to double if this is a power operation
                if (binaryExp.Operator == Operator.ArithmeticPow)
                {
                    sequence.Add(new ArithmeticInstruction("conv.r8"));
                }
            }

            // Generate operator instruction(s)
            GenerateBinaryOperator(sequence, binaryExp.Operator);
        }
    }

    private void GenerateBinaryOperator(InstructionSequence sequence, Operator op)
    {
        // Handle composite operators that require multiple IL instructions
        switch (op)
        {
            case Operator.NotEqual:
                // a != b  ==>  a == b, then negate: ceq, ldc.i4.0, ceq
                sequence.Add(new ArithmeticInstruction("ceq"));
                sequence.Add(new LoadInstruction("ldc.i4", 0));
                sequence.Add(new ArithmeticInstruction("ceq"));
                break;

            case Operator.LessThanOrEqual:
                // a <= b  ==>  !(a > b): cgt, ldc.i4.0, ceq
                sequence.Add(new ArithmeticInstruction("cgt"));
                sequence.Add(new LoadInstruction("ldc.i4", 0));
                sequence.Add(new ArithmeticInstruction("ceq"));
                break;

            case Operator.GreaterThanOrEqual:
                // a >= b  ==>  !(a < b): clt, ldc.i4.0, ceq
                sequence.Add(new ArithmeticInstruction("clt"));
                sequence.Add(new LoadInstruction("ldc.i4", 0));
                sequence.Add(new ArithmeticInstruction("ceq"));
                break;

            case Operator.ArithmeticPow:
                // a ** b  ==>  call Math.Pow(double a, double b)
                // Note: operands are already on stack and converted to double in GenerateBinaryExpression
                // Stack: [double base, double exponent]
                sequence.Add(new CallInstruction("call", "extcall:Asm=System.Runtime;Ns=System;Type=Math;Method=Pow;Params=System.Double,System.Double;Return=System.Double") { ArgCount = 2 });
                // Result is a double, may need to convert back to int for integer contexts
                sequence.Add(new ArithmeticInstruction("conv.i4")); // Convert result back to int
                break;

            default:
                // Simple operators map directly to IL opcodes
                var opCode = OperatorMapper.GetBinaryOpCode(op);
                sequence.Add(new ArithmeticInstruction(opCode));
                break;
        }
    }

    private void GenerateUnaryExpression(InstructionSequence sequence, UnaryExp unaryExp)
    {
        if (DebugEnabled)
        {
            DebugLog($"Unary start op='{unaryExp.Operator}' operand={(unaryExp.Operand?.GetType().Name ?? "null")}");
        }

        // Generate operand
        if (unaryExp.Operand != null)
        {
            var operandSeq = GenerateExpression(unaryExp.Operand);
            sequence.AddRange(operandSeq.Instructions);
        }

        // Generate operator
        if (unaryExp.Operator == Operator.LogicalNot)
        {
            // Logical not: compare with 0
            sequence.Add(new LoadInstruction("ldc.i4", 0));
            sequence.Add(new ArithmeticInstruction("ceq"));
        }
        else if (unaryExp.Operator == Operator.ArithmeticNegative)
        {
            sequence.Add(new ArithmeticInstruction("neg"));
        }
        else
        {
            sequence.Add(new ArithmeticInstruction("nop"));
        }
    }

    private void GenerateMemberAccess(InstructionSequence sequence, MemberAccessExp memberAccess)
    {
        // Handle method-style member access (e.g., KG.CreateGraph())
        if (memberAccess.RHS is FuncCallExp memberCall)
        {
            GenerateFuncCall(sequence, memberCall, memberAccess.LHS);
            return;
        }

        var lhsIsTypeQualifier = memberAccess.LHS is VarRefExp lhsVar && IsTypeName(lhsVar.VarName);

        // If LHS is not a type qualifier, evaluate it (pushes object reference on stack)
        if (!lhsIsTypeQualifier && memberAccess.LHS != null)
        {
            var lhsSeq = GenerateExpression(memberAccess.LHS);
            sequence.AddRange(lhsSeq.Instructions);
        }

        // Handle RHS
        switch (memberAccess.RHS)
        {
            case VarRefExp memberRef:
                if (lhsIsTypeQualifier && memberAccess.LHS is VarRefExp typeQualifier)
                {
                    // Static field access: TypeName::FieldName
                    var staticToken = $"{typeQualifier.VarName}::{memberRef.VarName}";
                    sequence.Add(new LoadInstruction("ldsfld", staticToken));
                }
                else
                {
                    // Instance field access
                    sequence.Add(new LoadInstruction("ldfld", memberRef.VarName));
                }
                break;

            case null:
                if (DebugEnabled)
                {
                    DebugLog($"WARNING: Null RHS in MemberAccessExp");
                }
                break;

            default:
                // Complex chaining: evaluate RHS independently
                var rhsSeq = GenerateExpression(memberAccess.RHS);
                sequence.AddRange(rhsSeq.Instructions);
                break;
        }
    }

    private void GenerateFuncCall(InstructionSequence sequence, FuncCallExp funcCall, Expression? qualifier)
    {
        var invocationArgs = funcCall.InvocationArguments ?? new List<Expression>();

        // Handle resolved Fifth function calls
        if (funcCall.FunctionDef != null)
        {
            // Emit arguments
            foreach (var arg in invocationArgs)
            {
                var argSeq = GenerateExpression(arg);
                sequence.AddRange(argSeq.Instructions);
            }

            // Determine target name
            var targetName = funcCall.FunctionDef.Name.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(targetName) && 
                funcCall.Annotations != null && 
                funcCall.Annotations.TryGetValue("FunctionName", out var fnObj) && 
                fnObj is string resolvedName)
            {
                targetName = resolvedName;
            }
            if (string.IsNullOrWhiteSpace(targetName))
            {
                targetName = "unknown";
            }

            sequence.Add(new CallInstruction("call", targetName) { ArgCount = invocationArgs.Count });
            return;
        }

        // Handle external .NET method calls
        if (funcCall.Annotations != null && 
            funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && 
            extTypeObj is Type extType)
        {
            GenerateExternalCall(sequence, funcCall, qualifier, extType, invocationArgs);
            return;
        }

        // Fallback: unresolved call
        foreach (var arg in invocationArgs)
        {
            var argSeq = GenerateExpression(arg);
            sequence.AddRange(argSeq.Instructions);
        }

        string fallbackName = "void [System.Console]System.Console::WriteLine(object)";
        if (funcCall.Annotations != null && 
            funcCall.Annotations.TryGetValue("FunctionName", out var genericNameObj) && 
            genericNameObj is string genericName && 
            !string.IsNullOrWhiteSpace(genericName))
        {
            fallbackName = genericName;
        }
        else if (funcCall.Annotations != null && 
                 funcCall.Annotations.TryGetValue("ExternalMethodName", out var extNameObj) && 
                 extNameObj is string extName && 
                 !string.IsNullOrWhiteSpace(extName))
        {
            fallbackName = extName;
        }

        sequence.Add(new CallInstruction("call", fallbackName) { ArgCount = invocationArgs.Count });
    }

    private void GenerateExternalCall(
        InstructionSequence sequence, 
        FuncCallExp funcCall, 
        Expression? qualifier, 
        Type extType, 
        IList<Expression> invocationArgs)
    {
        var methodName = ExternalMethodResolver.ExtractExternalMethodName(funcCall);
        var receiverType = qualifier != null ? _typeInference.InferExpressionType(qualifier) : null;
        
        System.Reflection.MethodInfo? resolvedMethod = null;
        try
        {
            resolvedMethod = _methodResolver.ResolveExternalMethod(_typeInference, extType, methodName, invocationArgs, receiverType);
        }
        catch
        {
            // Resolution failure is tolerated; fallback handles it
        }

        if (resolvedMethod != null)
        {
            GenerateResolvedExternalCall(sequence, resolvedMethod, qualifier, receiverType, invocationArgs, extType);
        }
        else
        {
            GenerateFallbackExternalCall(sequence, qualifier, receiverType, invocationArgs, extType, methodName);
        }
    }

    private void GenerateResolvedExternalCall(
        InstructionSequence sequence,
        System.Reflection.MethodInfo resolvedMethod,
        Expression? qualifier,
        Type? receiverType,
        IList<Expression> invocationArgs,
        Type extType)
    {
        var parameters = resolvedMethod.GetParameters();
        var expectsReceiver = SignatureFormatter.ShouldUseQualifierAsReceiver(
            resolvedMethod, qualifier, receiverType, parameters, invocationArgs.Count);
        
        var emittedArgCount = 0;

        // Emit receiver if needed
        if (expectsReceiver && qualifier != null)
        {
            var qualifierSeq = GenerateExpression(qualifier);
            sequence.AddRange(qualifierSeq.Instructions);
            emittedArgCount++;
        }

        // Emit arguments (with optional parameter handling)
        var suppliedIndex = 0;
        var startParam = expectsReceiver ? 1 : 0;
        for (int pi = startParam; pi < parameters.Length; pi++)
        {
            if (suppliedIndex < invocationArgs.Count)
            {
                var argSeq = GenerateExpression(invocationArgs[suppliedIndex]);
                sequence.AddRange(argSeq.Instructions);
                suppliedIndex++;
                emittedArgCount++;
            }
            else if (parameters[pi].IsOptional)
            {
                ConstantEmitter.EmitDefaultForParameter(sequence, parameters[pi]);
                emittedArgCount++;
            }
            else
            {
                ConstantEmitter.EmitDefaultForType(sequence, parameters[pi].ParameterType);
                emittedArgCount++;
            }
        }

        // Emit any extra supplied arguments
        for (int extra = suppliedIndex; extra < invocationArgs.Count; extra++)
        {
            var argSeq = GenerateExpression(invocationArgs[extra]);
            sequence.AddRange(argSeq.Instructions);
            emittedArgCount++;
        }

        var signature = SignatureFormatter.BuildExternalCallSignature(resolvedMethod, extType);
        sequence.Add(new CallInstruction("call", signature) { ArgCount = emittedArgCount });
    }

    private void GenerateFallbackExternalCall(
        InstructionSequence sequence,
        Expression? qualifier,
        Type? receiverType,
        IList<Expression> invocationArgs,
        Type extType,
        string methodName)
    {
        var emittedArgCount = 0;
        
        if (qualifier != null && receiverType != null)
        {
            var qualifierSeq = GenerateExpression(qualifier);
            sequence.AddRange(qualifierSeq.Instructions);
            emittedArgCount++;
        }

        foreach (var arg in invocationArgs)
        {
            var argSeq = GenerateExpression(arg);
            sequence.AddRange(argSeq.Instructions);
            emittedArgCount++;
        }

        var fallbackSig = SignatureFormatter.BuildFallbackExternalSignature(extType, methodName, emittedArgCount);
        sequence.Add(new CallInstruction("call", fallbackSig) { ArgCount = emittedArgCount });
    }

    private bool IsTypeName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        // Simple heuristic: treat capitalized identifiers as potential type names
        return char.IsUpper(name[0]);
    }

    private void GenerateObjectInitializer(InstructionSequence sequence, ObjectInitializerExp objectInit)
    {
        if (DebugEnabled)
        {
            var typeName = objectInit.TypeToInitialize?.Name.Value ?? "unknown";
            DebugLog($"ObjectInitializer for type '{typeName}' with {objectInit.PropertyInitialisers?.Count ?? 0} properties");
        }

        // Get the type name
        var typeNameStr = objectInit.TypeToInitialize?.Name.Value ?? "object";

        // Emit the constructor call (newobj)
        // For now, we assume a parameterless constructor
        var ctorSignature = $"instance void {typeNameStr}::.ctor()";
        sequence.Add(new CallInstruction("newobj", ctorSignature) { ArgCount = 0 });

        // If there are property initializers, we need to set each property
        // The pattern for each property is:
        //   dup           ; duplicate object reference (stfld will consume it)
        //   <load value>  ; push the value to assign
        //   stfld <name>  ; store value to field (consumes object ref + value)
        // After all properties are set, the original object reference remains on stack
        if (objectInit.PropertyInitialisers != null && objectInit.PropertyInitialisers.Count > 0)
        {
            foreach (var propInit in objectInit.PropertyInitialisers)
            {
                // Duplicate object reference on stack (stfld will consume one copy)
                // Use LoadInstruction for dup to match PEEmitter expectations
                sequence.Add(new LoadInstruction("dup", null));
                
                // Generate the value to assign
                var valueSeq = GenerateExpression(propInit.RHS);
                sequence.AddRange(valueSeq.Instructions);

                // Store to the field (consumes object ref and value, leaving original object on stack)
                var fieldName = propInit.PropertyToInitialize?.Property?.Name.Value ?? "unknown";
                sequence.Add(new StoreInstruction("stfld", fieldName));
            }
        }

        // At this point, the object reference is on the stack
    }

    private void GenerateListLiteral(InstructionSequence sequence, ListLiteral listLiteral)
    {
        var elementCount = listLiteral.ElementExpressions?.Count ?? 0;
        
        if (DebugEnabled)
        {
            DebugLog($"ListLiteral with {elementCount} elements");
        }

        // Infer element type from the list's type or first element
        var elementTypeName = InferListElementType(listLiteral);
        
        // Emit the array size
        sequence.Add(new LoadInstruction("ldc.i4", elementCount));
        
        // Create the array with inferred element type
        // Note: newarr instruction expects the element type (not the array type)
        sequence.Add(new LoadInstruction("newarr", elementTypeName));
        
        // Initialize each element
        if (listLiteral.ElementExpressions != null)
        {
            for (int i = 0; i < listLiteral.ElementExpressions.Count; i++)
            {
                // Duplicate array reference (stelem will consume it)
                sequence.Add(new LoadInstruction("dup", null));
                
                // Push index
                sequence.Add(new LoadInstruction("ldc.i4", i));
                
                // Generate element value
                var elemSeq = GenerateExpression(listLiteral.ElementExpressions[i]);
                sequence.AddRange(elemSeq.Instructions);
                
                // Store element (consumes array ref, index, value)
                // For reference types (including arrays), use stelem.ref
                var stelemOpcode = GetStoreElementOpcode(elementTypeName);
                sequence.Add(new StoreInstruction(stelemOpcode, null));
            }
        }
        
        // Array reference remains on stack
    }

    private void GenerateListComprehension(InstructionSequence sequence, ListComprehension listComp)
    {
        if (DebugEnabled)
        {
            DebugLog($"ListComprehension: {listComp.VarName} in {listComp.SourceName}");
        }

        // TODO: List comprehensions should be lowered to loops in an earlier transformation pass
        // For now, create an empty array as a placeholder to avoid stack underflow
        
        // Infer element type from the comprehension's type
        var elementTypeName = InferListElementType(listComp);
        
        // Create empty array
        sequence.Add(new LoadInstruction("ldc.i4", 0));  // Size 0
        sequence.Add(new LoadInstruction("newarr", elementTypeName));
        
        // Array reference remains on stack
    }

    private void GenerateTripleLiteral(InstructionSequence sequence, TripleLiteralExp tripleLit)
    {
        if (DebugEnabled)
        {
            DebugLog($"TripleLiteral: generating triple creation placeholder");
        }

        // TODO: Triple literals should generate proper calls to KG/RDF helper functions
        // For now, just push a null reference as a placeholder to avoid stack underflow
        // The proper implementation would:
        // 1. Generate subject, predicate, object expressions
        // 2. Call constructor: new Triple(subject, predicate, object)
        // 3. Leave triple object on stack
        
        // Placeholder: just push null for now
        sequence.Add(new LoadInstruction("ldnull", null));
        
        // Triple reference remains on stack
    }

    /// <summary>
    /// Infers the element type name for a list or array expression
    /// </summary>
    private string InferListElementType(Expression listExpr)
    {
        try
        {
            // Try to get element type from the expression's FifthType
            if (listExpr?.Type != null)
            {
                var fifthType = listExpr.Type;
                
                // Check if it's a TArrayOf or TListOf type
                if (fifthType is ast_model.TypeSystem.FifthType.TArrayOf arrayType)
                {
                    return GetTypeNameFromFifthType(arrayType.ElementType);
                }
                else if (fifthType is ast_model.TypeSystem.FifthType.TListOf listType)
                {
                    return GetTypeNameFromFifthType(listType.ElementType);
                }
            }
            
            // If we have a ListLiteral, try to infer from first element
            if (listExpr is ListLiteral listLit && 
                listLit.ElementExpressions != null && 
                listLit.ElementExpressions.Count > 0)
            {
                var firstElem = listLit.ElementExpressions[0];
                if (firstElem?.Type != null)
                {
                    return GetTypeNameFromFifthType(firstElem.Type);
                }
            }
        }
        catch (Exception ex)
        {
            if (DebugEnabled)
            {
                DebugLog($"WARNING: Failed to infer list element type: {ex.Message}. Defaulting to System.Int32");
            }
        }
        
        // Default to Int32 if type inference fails
        return "System.Int32";
    }

    /// <summary>
    /// Converts a FifthType to a .NET type name string
    /// </summary>
    private string GetTypeNameFromFifthType(ast_model.TypeSystem.FifthType fifthType)
    {
        try
        {
            if (fifthType is ast_model.TypeSystem.FifthType.TDotnetType dotnetType)
            {
                // Use the .NET type's full name
                return dotnetType.TheType.FullName ?? dotnetType.TheType.Name;
            }
            
            // Handle array/list types recursively
            if (fifthType is ast_model.TypeSystem.FifthType.TArrayOf arrayType)
            {
                var elementTypeName = GetTypeNameFromFifthType(arrayType.ElementType);
                return $"{elementTypeName}[]";
            }
            
            if (fifthType is ast_model.TypeSystem.FifthType.TListOf listType)
            {
                var elementTypeName = GetTypeNameFromFifthType(listType.ElementType);
                return $"{elementTypeName}[]";
            }
            
            // Map common type names using TypeMapper
            var typeName = fifthType.Name.Value;
            var typeRef = _typeMapper.MapType(typeName);
            
            if (!string.IsNullOrEmpty(typeRef.Namespace) && !string.IsNullOrEmpty(typeRef.Name))
            {
                return $"{typeRef.Namespace}.{typeRef.Name}";
            }
            
            return typeRef.Name ?? "System.Object";
        }
        catch (Exception ex)
        {
            if (DebugEnabled)
            {
                DebugLog($"WARNING: Failed to get type name from FifthType: {ex.Message}. Defaulting to System.Object");
            }
            return "System.Object";
        }
    }

    /// <summary>
    /// Gets the appropriate stelem opcode for a given type
    /// </summary>
    private string GetStoreElementOpcode(string typeName)
    {
        return typeName switch
        {
            "System.Int32" => "stelem.i4",
            "System.Int64" => "stelem.i8",
            "System.Single" => "stelem.r4",
            "System.Double" => "stelem.r8",
            "System.IntPtr" => "stelem.i",
            "System.Byte" => "stelem.i1",
            "System.Int16" => "stelem.i2",
            _ => "stelem.ref" // Reference types and other types
        };
    }
}
