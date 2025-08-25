using ast;
using ast_generated;
using ast_model.TypeSystem;
using Fifth.LangProcessingPhases;
using FluentAssertions;

namespace ast_tests;

public class TypeAnnotationVisitorTests : VisitorTestsBase
{
    private readonly TypeAnnotationVisitor _visitor = new();

    [Test]
    public void VisitAssemblyDef_ShouldSetVoidType()
    {
        // Arrange
        var assemblyDef = new AssemblyDefBuilder()
            .WithName(AssemblyName.From("TestAssembly"))
            .WithPublicKeyToken("")
            .WithVersion("1.0.0.0")
            .WithAssemblyRefs([])
            .WithModules([])
            .Build();

        // Act
        var result = _visitor.VisitAssemblyDef(assemblyDef);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TVoidType>();
        result.Type.Name.Value.Should().Be("void");
    }

    [Test]
    public void VisitInt32LiteralExp_ShouldSetIntType()
    {
        // Arrange
        var intLiteral = new Int32LiteralExp { Value = 42 };

        // Act
        var result = _visitor.VisitInt32LiteralExp(intLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(int));
        result.Type.Name.Value.Should().Be("int");
    }

    [Test]
    public void VisitInt64LiteralExp_ShouldSetLongType()
    {
        // Arrange
        var longLiteral = new Int64LiteralExp { Value = 42L };

        // Act
        var result = _visitor.VisitInt64LiteralExp(longLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(long));
        result.Type.Name.Value.Should().Be("long");
    }

    [Test]
    public void VisitFloat4LiteralExp_ShouldSetFloatType()
    {
        // Arrange
        var floatLiteral = new Float4LiteralExp { Value = 3.14f };

        // Act
        var result = _visitor.VisitFloat4LiteralExp(floatLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(float));
        result.Type.Name.Value.Should().Be("float");
    }

    [Test]
    public void VisitFloat8LiteralExp_ShouldSetDoubleType()
    {
        // Arrange
        var doubleLiteral = new Float8LiteralExp { Value = 3.14159 };

        // Act
        var result = _visitor.VisitFloat8LiteralExp(doubleLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(double));
        result.Type.Name.Value.Should().Be("double");
    }

    [Test]
    public void VisitBooleanLiteralExp_ShouldSetBoolType()
    {
        // Arrange
        var boolLiteral = new BooleanLiteralExp { Value = true };

        // Act
        var result = _visitor.VisitBooleanLiteralExp(boolLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(bool));
        result.Type.Name.Value.Should().Be("bool");
    }

    [Test]
    public void VisitStringLiteralExp_ShouldSetStringType()
    {
        // Arrange
        var stringLiteral = new StringLiteralExp { Value = "hello" };

        // Act
        var result = _visitor.VisitStringLiteralExp(stringLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(string));
        result.Type.Name.Value.Should().Be("string");
    }

    [Test]
    [Arguments(Operator.ArithmeticAdd)]
    [Arguments(Operator.ArithmeticSubtract)]
    [Arguments(Operator.ArithmeticMultiply)]
    public void VisitBinaryExp_IntArithmetic_ShouldReturnIntType(Operator op)
    {
        // Arrange
        var left = new Int32LiteralExp { Value = 5, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var right = new Int32LiteralExp { Value = 3, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = op,
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(int));
        result.Type.Name.Value.Should().Be("int");
    }

    [Test]
    public void VisitBinaryExp_IntDivision_ShouldReturnFloatType()
    {
        // Arrange
        var left = new Int32LiteralExp { Value = 6, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var right = new Int32LiteralExp { Value = 2, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = Operator.ArithmeticDivide,
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(float));
        result.Type.Name.Value.Should().Be("float");
    }

    [Test]
    public void VisitBinaryExp_FloatArithmetic_ShouldReturnFloatType()
    {
        // Arrange
        var left = new Float4LiteralExp { Value = 3.5f, Type = new FifthType.TDotnetType(typeof(float)) { Name = TypeName.From("float") } };
        var right = new Int32LiteralExp { Value = 2, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = Operator.ArithmeticAdd,
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(float));
        result.Type.Name.Value.Should().Be("float");
    }

    [Test]
    [Arguments(Operator.Equal)]
    [Arguments(Operator.NotEqual)]
    [Arguments(Operator.LessThan)]
    [Arguments(Operator.LessThanOrEqual)]
    [Arguments(Operator.GreaterThan)]
    [Arguments(Operator.GreaterThanOrEqual)]
    public void VisitBinaryExp_ComparisonOperators_ShouldReturnBoolType(Operator op)
    {
        // Arrange
        var left = new Int32LiteralExp { Value = 5, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var right = new Int32LiteralExp { Value = 3, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = op,
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(bool));
        result.Type.Name.Value.Should().Be("bool");
    }

    [Test]
    [Arguments(Operator.LogicalAnd)]
    [Arguments(Operator.LogicalOr)]
    public void VisitBinaryExp_LogicalOperators_ShouldReturnBoolType(Operator op)
    {
        // Arrange
        var left = new BooleanLiteralExp { Value = true, Type = new FifthType.TDotnetType(typeof(bool)) { Name = TypeName.From("bool") } };
        var right = new BooleanLiteralExp { Value = false, Type = new FifthType.TDotnetType(typeof(bool)) { Name = TypeName.From("bool") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = op,
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(bool));
        result.Type.Name.Value.Should().Be("bool");
    }

    [Test]
    public void VisitBinaryExp_WithUnsupportedOperator_ShouldSetUnknownType()
    {
        // Arrange - using an operator that our simple type inference doesn't handle
        var left = new Int32LiteralExp { Value = 5, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var right = new Int32LiteralExp { Value = 3, Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") } };
        var binaryExp = new BinaryExp
        {
            LHS = left,
            Operator = Operator.BitwiseAnd, // This operator is not handled by our simple type inference
            RHS = right
        };

        // Act
        var result = _visitor.VisitBinaryExp(binaryExp);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.UnknownType>();
        result.Type.Name.Value.Should().Be("unknown");
    }

    [Test]
    public void VisitFunctionDef_ShouldSetReturnType()
    {
        // Arrange
        var returnType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        var functionDef = new FunctionDefBuilder()
            .WithName(MemberName.From("testFunc"))
            .WithReturnType(returnType)
            .WithParams([])
            .WithBody(new BlockStatement { Statements = [] })
            .WithIsStatic(false)
            .WithIsConstructor(false)
            .Build();

        // Act
        var result = _visitor.VisitFunctionDef(functionDef);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().NotBeNull();
        result.Type.Should().Be(returnType);
        result.Type.Name.Value.Should().Be("int");
    }

    [Test]
    public void OnTypeInferred_ShouldNotThrow()
    {
        // Arrange
        var node = new Int32LiteralExp { Value = 42 };
        var type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };

        // Act & Assert
        var act = () => _visitor.OnTypeInferred(node, type);
        act.Should().NotThrow();
    }

    [Test]
    public void OnTypeMismatch_ShouldAddError()
    {
        // Arrange
        var node = new Int32LiteralExp { Value = 42 };
        var type1 = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        var type2 = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };

        // Act
        _visitor.OnTypeMismatch(node, type1, type2);

        // Assert
        _visitor.Errors.Should().HaveCount(1);
        var error = _visitor.Errors.First();
        error.Message.Should().Contain("Type mismatch");
        error.Message.Should().Contain("int");
        error.Message.Should().Contain("string");
        error.Types.Should().HaveCount(2);
        error.Types.Should().Contain(type1);
        error.Types.Should().Contain(type2);
    }

    [Test]
    public void OnTypeNotFound_ShouldAddError()
    {
        // Arrange
        var node = new Int32LiteralExp { Value = 42 };

        // Act
        _visitor.OnTypeNotFound(node);

        // Assert
        _visitor.Errors.Should().HaveCount(1);
        var error = _visitor.Errors.First();
        error.Message.Should().Be("Unable to infer type");
        error.Types.Should().BeEmpty();
    }

    [Test]
    public void OnTypeNotRelevant_ShouldNotThrow()
    {
        // Arrange
        var node = new Int32LiteralExp { Value = 42 };

        // Act & Assert
        var act = () => _visitor.OnTypeNotRelevant(node);
        act.Should().NotThrow();
    }

    [Test]
    public void Errors_InitiallyEmpty()
    {
        // Arrange
        var visitor = new TypeAnnotationVisitor();

        // Act & Assert
        visitor.Errors.Should().BeEmpty();
    }

    [Test]
    public void TypeCheckingError_ShouldInitializeCorrectly()
    {
        // Arrange
        var message = "Test error";
        var filename = "test.cs";
        var line = 10;
        var column = 5;
        var type1 = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        var type2 = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };
        var types = new[] { type1, type2 };

        // Act
        var error = new TypeCheckingError(message, filename, line, column, types);

        // Assert
        error.Message.Should().Be(message);
        error.Filename.Should().Be(filename);
        error.Line.Should().Be(line);
        error.Column.Should().Be(column);
        error.Types.Should().HaveCount(2);
        error.Types.Should().Contain(type1);
        error.Types.Should().Contain(type2);
    }

    [Test]
    public void ComplexExpression_ShouldAnnotateAllNodes()
    {
        // Arrange - Create a complex expression: (5 + 3) * 2
        var innerLeft = new Int32LiteralExp { Value = 5 };
        var innerRight = new Int32LiteralExp { Value = 3 };
        var innerBinary = new BinaryExp
        {
            LHS = innerLeft,
            Operator = Operator.ArithmeticAdd,
            RHS = innerRight
        };

        var outerRight = new Int32LiteralExp { Value = 2 };
        var outerBinary = new BinaryExp
        {
            LHS = innerBinary,
            Operator = Operator.ArithmeticMultiply,
            RHS = outerRight
        };

        // Act
        var result = _visitor.VisitBinaryExp(outerBinary);

        // Assert
        result.Should().NotBeNull();
        
        // Check that all literal nodes got their types
        result.LHS.Type.Should().NotBeNull();
        result.LHS.Type.Should().BeOfType<FifthType.TDotnetType>();
        
        result.RHS.Type.Should().NotBeNull();
        result.RHS.Type.Should().BeOfType<FifthType.TDotnetType>();
        
        // Check that the final result has the correct type
        result.Type.Should().NotBeNull();
        result.Type.Should().BeOfType<FifthType.TDotnetType>();
        var dotnetType = (FifthType.TDotnetType)result.Type;
        dotnetType.TheType.Should().Be(typeof(int));
    }

    [Test]
    public void IntegrationTest_ShouldAnnotateNestedAstStructure()
    {
        // Arrange - Create a more realistic AST structure with a function containing expressions
        var intType = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") };
        
        // Create expression: x + 5
        var varRef = new VarRefExp { VarName = "x" };
        var literal = new Int32LiteralExp { Value = 5 };
        var addExp = new BinaryExp
        {
            LHS = varRef,
            Operator = Operator.ArithmeticAdd,
            RHS = literal
        };
        
        // Create a return statement
        var returnStmt = new ReturnStatement { ReturnValue = addExp };
        var blockStmt = new BlockStatement { Statements = [returnStmt] };
        
        // Create a function
        var functionDef = new FunctionDefBuilder()
            .WithName(MemberName.From("testFunction"))
            .WithReturnType(intType)
            .WithParams([])
            .WithBody(blockStmt)
            .WithIsStatic(false)
            .WithIsConstructor(false)
            .Build();

        // Act
        var result = _visitor.VisitFunctionDef(functionDef);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(intType);
        result.Type.Name.Value.Should().Be("int");
        
        // The literal inside the function should have been annotated
        var processedAddExp = (BinaryExp)result.Body.Statements[0].As<ReturnStatement>().ReturnValue;
        processedAddExp.RHS.Type.Should().NotBeNull();
        processedAddExp.RHS.Type.Should().BeOfType<FifthType.TDotnetType>();
        var literalType = (FifthType.TDotnetType)processedAddExp.RHS.Type;
        literalType.TheType.Should().Be(typeof(int));
    }
}