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
}
