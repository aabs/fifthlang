using ast;
using ast_model.TypeSystem;
using ast_generated;
using compiler;
using compiler.LanguageTransformations;
using FluentAssertions;
using Fifth;

namespace ast_tests;

/// <summary>
/// Tests for constructor synthesis behavior in ClassCtorInserter
/// </summary>
public class ConstructorSynthesisTests
{
    [Test]
    public void ClassWithoutConstructorsAndNoRequiredFields_ShouldSynthesizeConstructor()
    {
        // Arrange - Class with no fields (all synthesizable)
        var classDef = new ClassDef
        {
            Name = TypeName.From("Empty"),
            TypeParameters = [],
            MemberDefs = [],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Empty") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var inserter = new ClassCtorInserter(diagnostics);

        // Act
        var result = inserter.VisitClassDef(classDef);

        // Assert - Should synthesize a parameterless constructor
        var constructors = result.MemberDefs
            .OfType<MethodDef>()
            .Where(m => m.FunctionDef?.IsConstructor == true)
            .ToList();

        constructors.Should().HaveCount(1, "A parameterless constructor should be synthesized");
        constructors[0].FunctionDef.Params.Should().BeEmpty("Synthesized constructor should be parameterless");
        diagnostics.Should().BeEmpty("No diagnostics should be emitted for successful synthesis");
    }

    [Test]
    public void ClassWithExplicitConstructor_ShouldNotSynthesize()
    {
        // Arrange - Class with an explicit constructor
        var explicitConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From("Person"))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType { Name = TypeName.From("void") })
            .WithBody(new BlockStatement 
            { 
                Statements = [],
                Annotations = [],
                Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
                Type = new FifthType.TVoidType { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        var methodDef = new MethodDef
        {
            Name = MemberName.From("Person"),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = explicitConstructor,
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TVoidType { Name = TypeName.From("void") }
        };

        var classDef = new ClassDef
        {
            Name = TypeName.From("Person"),
            TypeParameters = [],
            MemberDefs = [methodDef],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Person") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var inserter = new ClassCtorInserter(diagnostics);

        // Act
        var result = inserter.VisitClassDef(classDef);

        // Assert - Should NOT synthesize another constructor
        var constructors = result.MemberDefs
            .OfType<MethodDef>()
            .Where(m => m.FunctionDef?.IsConstructor == true)
            .ToList();

        constructors.Should().HaveCount(1, "Should keep only the explicit constructor");
        diagnostics.Should().BeEmpty("No diagnostics for class with explicit constructor");
    }

    [Test]
    public void ClassWithRequiredFields_ShouldEmitCTOR005Diagnostic()
    {
        // Arrange - Class with required fields (non-nullable)
        var requiredField = new FieldDef
        {
            Name = MemberName.From("Name"),
            TypeName = TypeName.From("string"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            AccessConstraints = [],
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("string") },
            Visibility = Visibility.Public
        };

        var classDef = new ClassDef
        {
            Name = TypeName.From("Person"),
            TypeParameters = [],
            MemberDefs = [requiredField],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Person") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var inserter = new ClassCtorInserter(diagnostics);

        // Act
        var result = inserter.VisitClassDef(classDef);

        // Assert - Should emit CTOR005 diagnostic and NOT synthesize constructor
        var constructors = result.MemberDefs
            .OfType<MethodDef>()
            .Where(m => m.FunctionDef?.IsConstructor == true)
            .ToList();

        constructors.Should().BeEmpty("Should not synthesize constructor when required fields lack defaults");
        
        diagnostics.Should().HaveCount(1, "Should emit CTOR005 diagnostic");
        diagnostics[0].Code.Should().Be("CTOR005");
        diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        diagnostics[0].Message.Should().Contain("Person");
        diagnostics[0].Message.Should().Contain("Name");
    }

    [Test]
    public void StaticConstructor_ShouldEmitCTOR010Diagnostic()
    {
        // Arrange - Constructor marked as static (forbidden)
        var staticConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From("Person"))
            .WithIsStatic(true)  // Static is forbidden for constructors
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType { Name = TypeName.From("void") })
            .WithBody(new BlockStatement 
            { 
                Statements = [],
                Annotations = [],
                Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
                Type = new FifthType.TVoidType { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        var methodDef = new MethodDef
        {
            Name = MemberName.From("Person"),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = staticConstructor,
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TVoidType { Name = TypeName.From("void") }
        };

        var classDef = new ClassDef
        {
            Name = TypeName.From("Person"),
            TypeParameters = [],
            MemberDefs = [methodDef],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Person") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var validator = new compiler.SemanticAnalysis.ConstructorValidator(diagnostics);

        // Act
        var result = validator.VisitClassDef(classDef);

        // Assert - Should emit CTOR010 diagnostic for static modifier
        diagnostics.Should().HaveCount(1, "Should emit CTOR010 diagnostic for static constructor");
        diagnostics[0].Code.Should().Be("CTOR010");
        diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        diagnostics[0].Message.Should().Contain("Person");
        diagnostics[0].Message.Should().Contain("static");
    }

    [Test]
    public void ConstructorWithValueReturn_ShouldEmitCTOR009Diagnostic()
    {
        // Arrange - Constructor with return value (forbidden)
        var returnStmt = new ReturnStatement
        {
            ReturnValue = new Int32LiteralExp { Value = 42 },
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("int") }
        };

        var constructorWithReturn = new FunctionDefBuilder()
            .WithName(MemberName.From("Person"))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType { Name = TypeName.From("void") })
            .WithBody(new BlockStatement 
            { 
                Statements = [returnStmt],
                Annotations = [],
                Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
                Type = new FifthType.TVoidType { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        var methodDef = new MethodDef
        {
            Name = MemberName.From("Person"),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = constructorWithReturn,
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TVoidType { Name = TypeName.From("void") }
        };

        var classDef = new ClassDef
        {
            Name = TypeName.From("Person"),
            TypeParameters = [],
            MemberDefs = [methodDef],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Person") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var validator = new compiler.SemanticAnalysis.ConstructorValidator(diagnostics);

        // Act
        var result = validator.VisitClassDef(classDef);

        // Assert - Should emit CTOR009 diagnostic for value return
        diagnostics.Should().HaveCount(1, "Should emit CTOR009 diagnostic for value return in constructor");
        diagnostics[0].Code.Should().Be("CTOR009");
        diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        diagnostics[0].Message.Should().Contain("Person");
        diagnostics[0].Message.Should().Contain("return");
    }

    [Test]
    public void ConstructorWithUnassignedFields_ShouldEmitCTOR003Diagnostic()
    {
        // Arrange - Constructor that doesn't assign required fields
        var requiredField = new FieldDef
        {
            Name = MemberName.From("Name"),
            TypeName = TypeName.From("string"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            AccessConstraints = [],
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("string") },
            Visibility = Visibility.Public
        };

        var emptyConstructor = new FunctionDefBuilder()
            .WithName(MemberName.From("Person"))
            .WithIsStatic(false)
            .WithIsConstructor(true)
            .WithParams([])
            .WithReturnType(new FifthType.TVoidType { Name = TypeName.From("void") })
            .WithBody(new BlockStatement 
            { 
                Statements = [],  // No field assignments!
                Annotations = [],
                Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
                Type = new FifthType.TVoidType { Name = TypeName.From("void") }
            })
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithTypeParameters([])
            .Build();

        var methodDef = new MethodDef
        {
            Name = MemberName.From("Person"),
            TypeName = TypeName.From("void"),
            CollectionType = CollectionType.SingleInstance,
            IsReadOnly = false,
            Visibility = Visibility.Public,
            Annotations = [],
            FunctionDef = emptyConstructor,
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TVoidType { Name = TypeName.From("void") }
        };

        var classDef = new ClassDef
        {
            Name = TypeName.From("Person"),
            TypeParameters = [],
            MemberDefs = [requiredField, methodDef],
            BaseClasses = [],
            AliasScope = null,
            Annotations = [],
            Location = new SourceLocationMetadata(0, "test.5th", 0, ""),
            Type = new FifthType.TType { Name = TypeName.From("Person") },
            Visibility = Visibility.Public
        };

        var diagnostics = new List<Diagnostic>();
        var analyzer = new compiler.SemanticAnalysis.DefiniteAssignmentAnalyzer();

        // Act
        var result = analyzer.VisitClassDef(classDef);

        // Assert - Should emit CTOR003 diagnostic for unassigned required field
        analyzer.Diagnostics.Should().HaveCount(1, "Should emit CTOR003 diagnostic for unassigned field");
        analyzer.Diagnostics[0].Code.Should().Be("CTOR003");
        analyzer.Diagnostics[0].Level.Should().Be(DiagnosticLevel.Error);
        analyzer.Diagnostics[0].Message.Should().Contain("Person");
        analyzer.Diagnostics[0].Message.Should().Contain("Name");
    }
}
