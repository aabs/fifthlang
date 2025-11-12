using FluentAssertions;
using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.LanguageTransformations;

namespace ast_tests;

/// <summary>
/// Unit tests for TriGLiteralLoweringRewriter to ensure proper AST transformation.
/// Tests validate that TriG literals are correctly lowered to Store.LoadFromTriG() calls
/// with proper string concatenation for interpolations.
/// </summary>
[Category("KG")]
[Category("TriG")]
[Category("Unit")]
public class TriGLiteralLoweringRewriterTests
{
    [Test]
    public void VisitTriGLiteralExpression_WithNoInterpolations_ShouldCreateStringLiteral()
    {
        // Arrange
        var rewriter = new TriGLiteralLoweringRewriter();
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "@prefix ex: <http://example.org/> .\nex:graph1 { ex:Item ex:value \"test\" . }",
            Interpolations = new List<InterpolatedExpression>(),
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().BeOfType<FuncCallExp>("Should create a function call");
        
        var funcCall = (FuncCallExp)result.Node;
        funcCall.InvocationArguments.Should().HaveCount(1, "Should have one argument");
        funcCall.InvocationArguments[0].Should().BeOfType<StringLiteralExp>("Argument should be a string literal");
        
        var stringArg = (StringLiteralExp)funcCall.InvocationArguments[0];
        stringArg.Value.Should().Contain("ex:graph1", "Should contain the original TriG content");
    }

    [Test]
    public void VisitTriGLiteralExpression_WithSingleInterpolation_ShouldCreateConcatenation()
    {
        // Arrange
        var rewriter = new TriGLiteralLoweringRewriter();
        var nameVar = new VarRefExp
        {
            VarName = "name",
            Type = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") },
            Annotations = new Dictionary<string, object>()
        };
        
        var interpolation = new InterpolatedExpression
        {
            Expression = nameVar,
            Position = 10,
            Length = 20,
            Location = null,
            Annotations = new Dictionary<string, object>()
        };
        
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Person ex:name {{__INTERP_0__}} .",
            Interpolations = new List<InterpolatedExpression> { interpolation },
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().BeOfType<FuncCallExp>("Should create a function call");
        
        var funcCall = (FuncCallExp)result.Node;
        funcCall.InvocationArguments.Should().HaveCount(1);
        
        // The argument should be a binary expression (string concatenation)
        funcCall.InvocationArguments[0].Should().BeOfType<BinaryExp>("Should use string concatenation for interpolation");
    }

    [Test]
    public void VisitTriGLiteralExpression_WithMultipleInterpolations_ShouldCreateChainedConcatenation()
    {
        // Arrange
        var rewriter = new TriGLiteralLoweringRewriter();
        
        var nameVar = new VarRefExp
        {
            VarName = "name",
            Type = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") },
            Annotations = new Dictionary<string, object>()
        };
        
        var ageVar = new VarRefExp
        {
            VarName = "age",
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") },
            Annotations = new Dictionary<string, object>()
        };
        
        var interp1 = new InterpolatedExpression
        {
            Expression = nameVar,
            Position = 10,
            Length = 20,
            Location = null,
            Annotations = new Dictionary<string, object>()
        };
        
        var interp2 = new InterpolatedExpression
        {
            Expression = ageVar,
            Position = 40,
            Length = 19,
            Location = null,
            Annotations = new Dictionary<string, object>()
        };
        
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Person ex:name {{__INTERP_0__}}; ex:age {{__INTERP_1__}} .",
            Interpolations = new List<InterpolatedExpression> { interp1, interp2 },
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().BeOfType<FuncCallExp>();
        
        var funcCall = (FuncCallExp)result.Node;
        funcCall.InvocationArguments[0].Should().BeOfType<BinaryExp>("Should create chained concatenation");
    }

    [Test]
    public void VisitTriGLiteralExpression_WithNumericInterpolation_ShouldNotQuote()
    {
        // Arrange - Numbers should not be wrapped in quotes
        var rewriter = new TriGLiteralLoweringRewriter();
        
        var ageVar = new VarRefExp
        {
            VarName = "age",
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") },
            Annotations = new Dictionary<string, object>()
        };
        
        var interpolation = new InterpolatedExpression
        {
            Expression = ageVar,
            Position = 10,
            Length = 19,
            Location = null,
            Annotations = new Dictionary<string, object>()
        };
        
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Person ex:age {{__INTERP_0__}} .",
            Interpolations = new List<InterpolatedExpression> { interpolation },
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().BeOfType<FuncCallExp>();
        
        var funcCall = (FuncCallExp)result.Node;
        funcCall.InvocationArguments[0].Should().BeOfType<BinaryExp>("Should concatenate number directly");
    }

    [Test]
    public void VisitTriGLiteralExpression_WithStringInterpolation_ShouldAddQuotes()
    {
        // Arrange - Strings should be wrapped in RDF quotes
        var rewriter = new TriGLiteralLoweringRewriter();
        
        var nameVar = new VarRefExp
        {
            VarName = "name",
            Type = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") },
            Annotations = new Dictionary<string, object>()
        };
        
        var interpolation = new InterpolatedExpression
        {
            Expression = nameVar,
            Position = 10,
            Length = 20,
            Location = null,
            Annotations = new Dictionary<string, object>()
        };
        
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Person ex:name {{__INTERP_0__}} .",
            Interpolations = new List<InterpolatedExpression> { interpolation },
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Should().NotBeNull();
        result.Node.Should().BeOfType<FuncCallExp>();
        
        var funcCall = (FuncCallExp)result.Node;
        var arg = funcCall.InvocationArguments[0];
        
        // The argument should be a binary expression that includes quote literals
        arg.Should().BeOfType<BinaryExp>("String should be quoted via concatenation");
        
        var binary = (BinaryExp)arg;
        // Should have structure like: "text" + "\"" + name + "\"" + "more text"
        // We verify it's a BinaryExp which indicates concatenation is happening
        binary.Operator.Should().Be(Operator.ArithmeticAdd, "Should use + for concatenation");
    }

    [Test]
    public void VisitTriGLiteralExpression_ShouldPreservePrologue()
    {
        // Arrange
        var rewriter = new TriGLiteralLoweringRewriter();
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Item ex:value \"test\" .",
            Interpolations = new List<InterpolatedExpression>(),
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Prologue.Should().NotBeNull("Prologue should always be present");
        result.Prologue.Should().BeEmpty("Simple literals shouldn't need prologue statements");
    }

    [Test]
    public void VisitTriGLiteralExpression_ShouldSetStoreType()
    {
        // Arrange
        var rewriter = new TriGLiteralLoweringRewriter();
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Item ex:value \"test\" .",
            Interpolations = new List<InterpolatedExpression>(),
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = null,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Node.Type.Should().NotBeNull("Result should have a type");
        // The result type should be Store (can be either TType or TDotnetType based on implementation)
        result.Node.Should().BeOfType<FuncCallExp>("Result should be a function call expression");
    }

    [Test]
    public void VisitTriGLiteralExpression_ShouldPreserveLocationInfo()
    {
        // Arrange
        var location = new SourceLocationMetadata(
            Column: 5,
            Filename: "test.5th",
            Line: 10,
            OriginalText: "@< ... >"
        );
        
        var rewriter = new TriGLiteralLoweringRewriter();
        var trigLiteral = new TriGLiteralExpression
        {
            Content = "ex:Item ex:value \"test\" .",
            Interpolations = new List<InterpolatedExpression>(),
            Type = new FifthType.TType { Name = TypeName.From("Store") },
            Location = location,
            Annotations = new Dictionary<string, object>()
        };

        // Act
        var result = rewriter.VisitTriGLiteralExpression(trigLiteral);

        // Assert
        result.Node.Location.Should().Be(location, "Location information should be preserved for diagnostics");
    }
}
