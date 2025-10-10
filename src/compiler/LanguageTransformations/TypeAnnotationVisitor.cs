using ast;
using ast_generated;
using ast_model.TypeSystem;
using ast_model.TypeSystem.Inference;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Type annotation visitor that derives from DefaultRecursiveDescentVisitor.
/// This visitor replaces the original TypeAnnotatorVisitor which used the old BaseAstVisitor pattern.
/// It annotates AST nodes with their inferred types by setting the Type property.
/// </summary>
/// <remarks>
/// This visitor processes the AST and performs type inference on nodes that can be typed,
/// setting their Type property to the appropriate FifthType. It uses a simple approach
/// for type annotation while maintaining compatibility with the new visitor system.
/// </remarks>
public class TypeAnnotationVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<TypeCheckingError> errors = new();
    private readonly TypeSystem typeSystem;
    private readonly Dictionary<Type, FifthType> languageFriendlyTypes = new();

    /// <summary>
    /// Initializes a new instance of the TypeAnnotationVisitor.
    /// </summary>
    public TypeAnnotationVisitor()
    {
        // Initialize the type system 
        typeSystem = new TypeSystem();

        // Create language-friendly type mappings
        InitializeLanguageFriendlyTypes();

        // Add all types to the type system
        foreach (var fifthType in languageFriendlyTypes.Values)
        {
            typeSystem.WithType(fifthType);
        }

        // Add void type
        var voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };
        typeSystem.WithType(voidType);

        // Set up common binary operators using the registered types
        if (languageFriendlyTypes.TryGetValue(typeof(int), out var intType) &&
            languageFriendlyTypes.TryGetValue(typeof(float), out var floatType))
        {
            // Arithmetic operators for integers
            typeSystem.WithFunction([intType, intType], intType, "+")
                     .WithFunction([intType, intType], intType, "-")
                     .WithFunction([intType, intType], intType, "*")
                     .WithFunction([intType, intType], floatType, "/");

            // Arithmetic operators for floats
            typeSystem.WithFunction([floatType, floatType], floatType, "+")
                     .WithFunction([floatType, floatType], floatType, "-")
                     .WithFunction([floatType, floatType], floatType, "*")
                     .WithFunction([floatType, floatType], floatType, "/");
        }
    }

    /// <summary>
    /// Visits a GraphAssertionBlockExp and sets its type. We keep the language-level name 'graph',
    /// but downstream codegen will map it to IGraph. For now, annotate with Name 'graph'.
    /// </summary>
    public override GraphAssertionBlockExp VisitGraphAssertionBlockExp(GraphAssertionBlockExp ctx)
    {
        var result = base.VisitGraphAssertionBlockExp(ctx);

        var graphType = new FifthType.TType() { Name = TypeName.From("graph") };
        OnTypeInferred(result, graphType);
        return result with { Type = graphType };
    }

    /// <summary>
    /// Visits a GraphAssertionBlockStatement and annotates it as 'void'.
    /// </summary>
    public override GraphAssertionBlockStatement VisitGraphAssertionBlockStatement(GraphAssertionBlockStatement ctx)
    {
        var result = base.VisitGraphAssertionBlockStatement(ctx);

        var voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };
        OnTypeInferred(result, voidType);
        return result with { Type = voidType };
    }

    /// <summary>
    /// Initializes the language-friendly type mappings using TypeRegistry.Primitives.
    /// </summary>
    private void InitializeLanguageFriendlyTypes()
    {
        // Create a mapping for language-friendly type names
        var typeNameMapping = new Dictionary<Type, string>
        {
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(char)] = "char",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(string)] = "string",
            [typeof(DateTime)] = "DateTime",
            [typeof(DateTimeOffset)] = "DateTimeOffset"
        };

        // Create types with language-friendly names from TypeRegistry.Primitives
        foreach (var primitiveType in TypeRegistry.Primitives)
        {
            if (typeNameMapping.TryGetValue(primitiveType, out var friendlyName))
            {
                var fifthType = new FifthType.TDotnetType(primitiveType) { Name = TypeName.From(friendlyName) };
                languageFriendlyTypes[primitiveType] = fifthType;
            }
            else
            {
                // For types without mapping, use .NET name
                var fifthType = new FifthType.TDotnetType(primitiveType) { Name = TypeName.From(primitiveType.Name) };
                languageFriendlyTypes[primitiveType] = fifthType;
            }
        }
    }

    /// <summary>
    /// Gets a type with language-friendly naming from the internal mapping.
    /// </summary>
    private FifthType? GetLanguageFriendlyType(Type dotnetType)
    {
        return languageFriendlyTypes.TryGetValue(dotnetType, out var fifthType) ? fifthType : null;
    }

    /// <summary>
    /// Gets the list of type checking errors encountered during annotation.
    /// </summary>
    public IReadOnlyList<TypeCheckingError> Errors => errors.AsReadOnly();

    /// <summary>
    /// Visits an AssemblyDef node and processes its contents for type annotation.
    /// This is the main entry point for the type annotation process.
    /// </summary>
    /// <param name="ctx">The AssemblyDef node to process</param>
    /// <returns>The AssemblyDef with type annotations applied</returns>
    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        // First visit children using base implementation to ensure all descendants are processed
        var result = base.VisitAssemblyDef(ctx);

        // The AssemblyDef itself doesn't typically have a meaningful type
        // but we can set it to void for consistency
        var voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };

        return result with { Type = voidType };
    }

    /// <summary>
    /// Visits an Int32LiteralExp and infers its type as int.
    /// </summary>
    public override Int32LiteralExp VisitInt32LiteralExp(Int32LiteralExp ctx)
    {
        var result = base.VisitInt32LiteralExp(ctx);

        var intType = GetLanguageFriendlyType(typeof(int)) ??
                     new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };

        OnTypeInferred(result, intType);
        return result with { Type = intType };
    }

    /// <summary>
    /// Visits an Int64LiteralExp and infers its type as long.
    /// </summary>
    public override Int64LiteralExp VisitInt64LiteralExp(Int64LiteralExp ctx)
    {
        var result = base.VisitInt64LiteralExp(ctx);

        var longType = GetLanguageFriendlyType(typeof(long)) ??
                      new FifthType.TDotnetType(typeof(long)) { Name = TypeName.From("long") };

        OnTypeInferred(result, longType);
        return result with { Type = longType };
    }

    /// <summary>
    /// Visits a Float8LiteralExp and infers its type as double.
    /// </summary>
    public override Float8LiteralExp VisitFloat8LiteralExp(Float8LiteralExp ctx)
    {
        var result = base.VisitFloat8LiteralExp(ctx);

        var doubleType = GetLanguageFriendlyType(typeof(double)) ??
                        new FifthType.TDotnetType(typeof(double)) { Name = TypeName.From("double") };

        OnTypeInferred(result, doubleType);
        return result with { Type = doubleType };
    }

    /// <summary>
    /// Visits a Float4LiteralExp and infers its type as float.
    /// </summary>
    public override Float4LiteralExp VisitFloat4LiteralExp(Float4LiteralExp ctx)
    {
        var result = base.VisitFloat4LiteralExp(ctx);

        var floatType = GetLanguageFriendlyType(typeof(float)) ??
                       new FifthType.TDotnetType(typeof(float)) { Name = TypeName.From("float") };

        OnTypeInferred(result, floatType);
        return result with { Type = floatType };
    }

    /// <summary>
    /// Visits a BooleanLiteralExp and infers its type as bool.
    /// </summary>
    public override BooleanLiteralExp VisitBooleanLiteralExp(BooleanLiteralExp ctx)
    {
        var result = base.VisitBooleanLiteralExp(ctx);

        var boolType = GetLanguageFriendlyType(typeof(bool)) ??
                      new FifthType.TDotnetType(typeof(bool)) { Name = TypeName.From("bool") };

        OnTypeInferred(result, boolType);
        return result with { Type = boolType };
    }

    /// <summary>
    /// Visits a StringLiteralExp and infers its type as string.
    /// </summary>
    public override StringLiteralExp VisitStringLiteralExp(StringLiteralExp ctx)
    {
        var result = base.VisitStringLiteralExp(ctx);

        var stringType = GetLanguageFriendlyType(typeof(string)) ??
                        new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };

        OnTypeInferred(result, stringType);
        return result with { Type = stringType };
    }

    /// <summary>
    /// Visits a ListLiteral and infers its type based on element expressions or type annotation.
    /// </summary>
    public override ListLiteral VisitListLiteral(ListLiteral ctx)
    {
        var result = base.VisitListLiteral(ctx);

        // Try to infer element type from the list literal itself
        FifthType? elementType = null;

        // If there are elements, infer from the first element
        if (result.ElementExpressions?.Count > 0)
        {
            var firstElement = result.ElementExpressions[0];
            if (firstElement?.Type != null)
            {
                elementType = firstElement.Type;
            }
        }

        // If no elements or couldn't infer, try to get from annotation
        if (elementType == null && result.Type is FifthType.TArrayOf arrayType)
        {
            elementType = arrayType.ElementType;
        }
        else if (elementType == null && result.Type is FifthType.TListOf listType)
        {
            elementType = listType.ElementType;
        }

        // Default to int if we still don't have a type
        if (elementType == null)
        {
            elementType = GetLanguageFriendlyType(typeof(int)) ?? 
                         new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        }

        // Create the array type (lists are always arrays in Fifth currently)
        var arrayTypeResult = new FifthType.TArrayOf(elementType) 
        { 
            Name = TypeName.From($"{GetTypeName(elementType)}[]") 
        };

        OnTypeInferred(result, arrayTypeResult);
        return result with { Type = arrayTypeResult };
    }

    /// <summary>
    /// Visits a BinaryExp and infers its result type based on the operator and operand types.
    /// </summary>
    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        var result = base.VisitBinaryExp(ctx);

        // Get the types of the operands  
        var leftType = result.LHS?.Type;
        var rightType = result.RHS?.Type;

        if (leftType != null && rightType != null)
        {
            // Get the operator string
            var operatorStr = GetOperatorString(result.Operator);

            // Try to use the type system for inference first
            try
            {
                var inferredType = typeSystem.InferResultType([leftType, rightType], operatorStr);
                if (inferredType != null)
                {
                    OnTypeInferred(result, inferredType);
                    return result with { Type = inferredType };
                }
            }
            catch
            {
                // Fall back to simple inference if type system inference fails
            }

            // Fallback to simple operator result type inference
            var resultType = GetSimpleOperatorResultType(leftType, rightType, result.Operator);

            if (resultType != null)
            {
                OnTypeInferred(result, resultType);
                return result with { Type = resultType };
            }
        }

        OnTypeNotFound(result);
        var unknownTypeDefault = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
        return result with { Type = unknownTypeDefault };
    }

    /// <summary>
    /// Visits a FuncCallExp and infers its type from the function's return type.
    /// </summary>
    public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
    {
        var result = base.VisitFuncCallExp(ctx);

        // Use the function's return type
        if (result.FunctionDef != null)
        {
            OnTypeInferred(result, result.FunctionDef.ReturnType);
            return result with { Type = result.FunctionDef.ReturnType };
        }

        // If we can't determine the type, mark as unknown
        OnTypeNotFound(result);
        var unknownType = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
        return result with { Type = unknownType };
    }

    /// <summary>
    /// Visits a MemberAccessExp and validates that member access is valid on the LHS type.
    /// </summary>
    public override MemberAccessExp VisitMemberAccessExp(MemberAccessExp ctx)
    {
        var result = base.VisitMemberAccessExp(ctx);

        // Check if LHS has a type
        if (result.LHS?.Type != null)
        {
            var lhsType = result.LHS.Type;
            
            // Check if this is a primitive type (int, float, bool, string, etc.)
            // Primitive types don't have member access (except for built-in methods which would be handled elsewhere)
            if (IsPrimitiveType(lhsType))
            {
                var error = new TypeCheckingError(
                    $"Cannot access member on primitive type '{GetTypeName(lhsType)}'",
                    result.Location?.Filename ?? "",
                    result.Location?.Line ?? 0,
                    result.Location?.Column ?? 0,
                    new[] { lhsType },
                    TypeCheckingSeverity.Error);
                errors.Add(error);
                
                // Return with unknown type
                var unknownType = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
                return result with { Type = unknownType };
            }
        }

        // If we can't determine the type yet, just return the result
        // Type will be inferred later or will be unknown
        return result;
    }

    /// <summary>
    /// Visits an IndexerExpression and infers the element type from the array/list type.
    /// </summary>
    public override IndexerExpression VisitIndexerExpression(IndexerExpression ctx)
    {
        var result = base.VisitIndexerExpression(ctx);

        // Get the type of the expression being indexed
        if (result.IndexExpression?.Type != null)
        {
            var indexedType = result.IndexExpression.Type;
            
            // Extract element type from array or list type
            FifthType? elementType = indexedType switch
            {
                FifthType.TArrayOf arrayType => arrayType.ElementType,
                FifthType.TListOf listType => listType.ElementType,
                _ => null
            };

            if (elementType != null)
            {
                OnTypeInferred(result, elementType);
                return result with { Type = elementType };
            }
            else
            {
                // Error: trying to index a non-indexable type
                var error = new TypeCheckingError(
                    $"Cannot index type '{GetTypeName(indexedType)}' - only arrays and lists support indexing",
                    result.Location?.Filename ?? "",
                    result.Location?.Line ?? 0,
                    result.Location?.Column ?? 0,
                    new[] { indexedType },
                    TypeCheckingSeverity.Error);
                errors.Add(error);
            }
        }

        // If we can't determine the type, mark as unknown
        OnTypeNotFound(result);
        var unknownType = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
        return result with { Type = unknownType };
    }

    /// <summary>
    /// Checks if a type is a primitive type that doesn't support member access.
    /// Arrays and lists are NOT considered primitive types.
    /// </summary>
    private bool IsPrimitiveType(FifthType type)
    {
        return type switch
        {
            // Arrays and lists are not primitive - they support indexing
            FifthType.TArrayOf => false,
            FifthType.TListOf => false,
            FifthType.TDotnetType dotnetType => 
                dotnetType.TheType == typeof(int) ||
                dotnetType.TheType == typeof(long) ||
                dotnetType.TheType == typeof(float) ||
                dotnetType.TheType == typeof(double) ||
                dotnetType.TheType == typeof(bool) ||
                dotnetType.TheType == typeof(string),
            FifthType.TType ttype =>
                ttype.Name.Value == "int" ||
                ttype.Name.Value == "long" ||
                ttype.Name.Value == "float" ||
                ttype.Name.Value == "double" ||
                ttype.Name.Value == "bool" ||
                ttype.Name.Value == "string",
            _ => false
        };
    }

    /// <summary>
    /// Gets a human-readable name for a type.
    /// </summary>
    private string GetTypeName(FifthType type)
    {
        return type switch
        {
            FifthType.TDotnetType dotnetType => dotnetType.TheType?.Name ?? "unknown",
            FifthType.TType ttype => ttype.Name.Value,
            FifthType.TVoidType => "void",
            _ => type.ToString() ?? "unknown"
        };
    }

    /// <summary>
    /// Visits a FunctionDef and uses its declared return type.
    /// </summary>
    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        var result = base.VisitFunctionDef(ctx);

        // Function definitions already have their return type specified
        // We can set the overall type to match the return type
        OnTypeInferred(result, result.ReturnType);
        return result with { Type = result.ReturnType };
    }

    /// <summary>
    /// Visits a VariableDecl and infers its type from TypeName and CollectionType.
    /// </summary>
    public override VariableDecl VisitVariableDecl(VariableDecl ctx)
    {
        var result = base.VisitVariableDecl(ctx);

        // Create FifthType from TypeName and CollectionType
        FifthType fifthType = CreateFifthType(result.TypeName, result.CollectionType);
        
        OnTypeInferred(result, fifthType);
        return result with { Type = fifthType };
    }

    /// <summary>
    /// Visits a VarRefExp and infers its type from the referenced VariableDecl.
    /// </summary>
    public override VarRefExp VisitVarRefExp(VarRefExp ctx)
    {
        var result = base.VisitVarRefExp(ctx);

        // If the variable declaration is resolved, use its type
        if (result.VariableDecl != null && result.VariableDecl.Type != null)
        {
            OnTypeInferred(result, result.VariableDecl.Type);
            return result with { Type = result.VariableDecl.Type };
        }

        // If not resolved, return with unknown type
        OnTypeNotFound(result);
        var unknownType = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
        return result with { Type = unknownType };
    }

    /// <summary>
    /// Creates a FifthType from a TypeName and CollectionType.
    /// </summary>
    private FifthType CreateFifthType(TypeName typeName, CollectionType collectionType)
    {
        // First create the base type
        FifthType baseType = CreateBaseType(typeName);

        // Wrap in collection type if needed
        return collectionType switch
        {
            CollectionType.Array => new FifthType.TArrayOf(baseType) { Name = TypeName.From($"{typeName.Value}[]") },
            CollectionType.List => new FifthType.TListOf(baseType) { Name = TypeName.From($"List<{typeName.Value}>") },
            _ => baseType
        };
    }

    /// <summary>
    /// Creates a base FifthType from a TypeName (without collection wrapper).
    /// </summary>
    private FifthType CreateBaseType(TypeName typeName)
    {
        // Try to get from language-friendly types first
        var friendlyType = GetLanguageFriendlyTypeByName(typeName.Value);
        if (friendlyType != null)
        {
            return friendlyType;
        }

        // Default to TType for unknown types
        return new FifthType.TType() { Name = typeName };
    }

    /// <summary>
    /// Gets a language-friendly type by its name.
    /// </summary>
    private FifthType? GetLanguageFriendlyTypeByName(string typeName)
    {
        return typeName switch
        {
            "int" => GetLanguageFriendlyType(typeof(int)),
            "long" => GetLanguageFriendlyType(typeof(long)),
            "float" => GetLanguageFriendlyType(typeof(float)),
            "double" => GetLanguageFriendlyType(typeof(double)),
            "bool" => GetLanguageFriendlyType(typeof(bool)),
            "string" => GetLanguageFriendlyType(typeof(string)),
            _ => null
        };
    }

    /// <summary>
    /// Converts an AST operator to its string representation for type inference.
    /// </summary>
    private static string GetOperatorString(Operator op)
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
            Operator.LessThanOrEqual => "<=",
            Operator.GreaterThan => ">",
            Operator.GreaterThanOrEqual => ">=",
            Operator.LogicalAnd => "&&",
            Operator.LogicalOr => "||",
            _ => op.ToString()
        };
    }

    /// <summary>
    /// Performs simple type inference for binary operators.
    /// This is a simplified version that uses TypeRegistry types for fallback when full type system integration fails.
    /// </summary>
    private FifthType? GetSimpleOperatorResultType(FifthType leftType, FifthType rightType, Operator op)
    {
        // Simple type inference rules
        return op switch
        {
            // Arithmetic operators
            Operator.ArithmeticAdd or Operator.ArithmeticSubtract or Operator.ArithmeticMultiply =>
                GetArithmeticResultType(leftType, rightType),

            Operator.ArithmeticDivide =>
                // Division always returns float
                GetLanguageFriendlyType(typeof(float)) ?? new FifthType.TDotnetType(typeof(float)) { Name = TypeName.From("float") },

            // Comparison operators always return bool
            Operator.Equal or Operator.NotEqual or
            Operator.LessThan or Operator.LessThanOrEqual or
            Operator.GreaterThan or Operator.GreaterThanOrEqual =>
                GetLanguageFriendlyType(typeof(bool)) ?? new FifthType.TDotnetType(typeof(bool)) { Name = TypeName.From("bool") },

            // Logical operators return bool
            Operator.LogicalAnd or Operator.LogicalOr =>
                GetLanguageFriendlyType(typeof(bool)) ?? new FifthType.TDotnetType(typeof(bool)) { Name = TypeName.From("bool") },

            _ => null // Unknown operator
        };
    }

    /// <summary>
    /// Gets the result type for arithmetic operations using TypeRegistry types.
    /// </summary>
    private FifthType GetArithmeticResultType(FifthType leftType, FifthType rightType)
    {
        // If either operand is float, result is float
        if (IsFloatType(leftType) || IsFloatType(rightType))
        {
            return GetLanguageFriendlyType(typeof(float)) ?? new FifthType.TDotnetType(typeof(float)) { Name = TypeName.From("float") };
        }

        // If both are integers, result is int
        if (IsIntType(leftType) && IsIntType(rightType))
        {
            return GetLanguageFriendlyType(typeof(int)) ?? new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        }

        // Default to the left type
        return leftType;
    }

    /// <summary>
    /// Checks if a type is a floating-point type.
    /// </summary>
    private static bool IsFloatType(FifthType type)
    {
        return type is FifthType.TDotnetType dotnetType &&
               (dotnetType.TheType == typeof(float) || dotnetType.TheType == typeof(double));
    }

    /// <summary>
    /// Checks if a type is an integer type.
    /// </summary>
    private static bool IsIntType(FifthType type)
    {
        return type is FifthType.TDotnetType dotnetType &&
               (dotnetType.TheType == typeof(int) || dotnetType.TheType == typeof(long) ||
                dotnetType.TheType == typeof(short) || dotnetType.TheType == typeof(byte));
    }

    /// <summary>
    /// Event handler called when a type is successfully inferred for a node.
    /// This replicates the callback mechanism from the original TypeAnnotatorVisitor.
    /// </summary>
    /// <param name="node">The AST node whose type was inferred</param>
    /// <param name="type">The inferred type</param>
    public void OnTypeInferred(AstThing node, FifthType type)
    {
        // This method serves as the event mechanism callback for successful type inference
        // The actual type assignment is done in the Visit methods using the 'with' syntax

        // We could add logging or additional processing here if needed
        // For now, this serves as the event callback interface
    }

    /// <summary>
    /// Event handler called when two types don't match in a context where they should.
    /// </summary>
    /// <param name="node">The AST node where the mismatch occurred</param>
    /// <param name="type1">The first type</param>
    /// <param name="type2">The second type</param>
    public void OnTypeMismatch(AstThing node, FifthType type1, FifthType type2)
    {
        var error = new TypeCheckingError(
            $"Type mismatch between {type1.Name} and {type2.Name}",
            node.Location?.Filename ?? "",
            node.Location?.Line ?? 0,
            node.Location?.Column ?? 0,
            new[] { type1, type2 },
            TypeCheckingSeverity.Error);

        errors.Add(error);
    }

    /// <summary>
    /// Event handler called when a type cannot be inferred for a node.
    /// </summary>
    /// <param name="node">The AST node whose type could not be inferred</param>
    public void OnTypeNotFound(AstThing node)
    {
        var error = new TypeCheckingError(
            "Unable to infer type",
            node.Location?.Filename ?? "",
            node.Location?.Line ?? 0,
            node.Location?.Column ?? 0,
            Array.Empty<FifthType>(),
            TypeCheckingSeverity.Info); // This is informational, not an error

        errors.Add(error);
    }

    /// <summary>
    /// Event handler called when type information is not relevant for a node.
    /// </summary>
    /// <param name="node">The AST node for which type is not relevant</param>
    public void OnTypeNotRelevant(AstThing node)
    {
        // No action needed - this is for nodes where type annotation isn't meaningful
    }
}

/// <summary>
/// Represents an error that occurred during type checking.
/// </summary>
public class TypeCheckingError
{
    public string Message { get; }
    public string Filename { get; }
    public int Line { get; }
    public int Column { get; }
    public IReadOnlyList<FifthType> Types { get; }
    public TypeCheckingSeverity Severity { get; }

    public TypeCheckingError(string message, string filename, int line, int column, IEnumerable<FifthType> types, TypeCheckingSeverity severity = TypeCheckingSeverity.Error)
    {
        Message = message;
        Filename = filename;
        Line = line;
        Column = column;
        Types = types.ToList().AsReadOnly();
        Severity = severity;
    }
}

/// <summary>
/// Severity levels for type checking errors
/// </summary>
public enum TypeCheckingSeverity
{
    /// <summary>
    /// Informational message (e.g., type couldn't be inferred but it's expected)
    /// </summary>
    Info,
    
    /// <summary>
    /// Warning that should be reported but doesn't fail compilation
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error that should fail compilation
    /// </summary>
    Error
}