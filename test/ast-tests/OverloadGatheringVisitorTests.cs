using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.LanguageTransformations;
using FluentAssertions;

namespace ast_tests;

public class OverloadGatheringVisitorTests
{
    [Fact]
    public void Gather_ShouldGroupOverloadedFunctionsAndSubstituteThem()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();

        var methodDef1 = createMethodDef("Test", "int");
        var methodDef2 = createMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;

        // Act
        visitor.Gather(classDef);

        // Assert
        Assert.Single(classDef.MemberDefs);
        Assert.IsType<OverloadedFunctionDefinition>(classDef.MemberDefs.First());
    }

    [Fact]
    public void GatherOverloads_ShouldReturnGroupedMethodsByFunctionSignature()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var methodDef1 = createMethodDef("Test", "int");
        var methodDef2 = createMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;
        // Act
        var result = visitor.GatherOverloads(classDef);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result.First().Value.Count);
    }

    [Fact]
    public void SubstituteFunctionDefinitions_ShouldReplaceOldMethodsWithCombinedFunction()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var methodDef1 = createMethodDef("Test", "int");
        var methodDef2 = createMethodDef("Test", "int");
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs(new List<MemberDef> { methodDef1, methodDef2 }).Build();
        methodDef1.Parent = methodDef1.FunctionDef.Parent = classDef;
        methodDef2.Parent = methodDef2.FunctionDef.Parent = classDef;
        var combinedFunction = new OverloadedFunctionDefinitionBuilder()
        .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 })
        .WithSignature(new FunctionSignature())
        .Build();

        // Act
        visitor.SubstituteFunctionDefinitions(classDef, new List<MethodDef> { methodDef1, methodDef2 }, combinedFunction);

        // Assert
        Assert.Single(classDef.MemberDefs);
        Assert.Equal(combinedFunction, classDef.MemberDefs.First());
    }

    [Fact]
    public void VisitClassDef_ShouldCallGatherAndReturnBaseVisitClassDef()
    {
        // Arrange
        var visitor = new OverloadGatheringVisitor();
        var classDef = new ClassDefBuilder().WithName(TypeName.anonymous).WithMemberDefs([]).Build();

        // Act
        var result = visitor.VisitClassDef(classDef);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(classDef);
    }

    private static readonly FifthType voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };

    private static FifthType CreateType(string name, ushort typeId, SymbolKind symbolKind = SymbolKind.Assembly)
    {
        return voidType;
    }

    private FunctionDef createFunctionDef(string name, string returnType)
    {
        return new FunctionDef()
        {
            Annotations = [],
            Name = MemberName.From(name),
            ReturnType = new FifthType.TType() { Name = TypeName.From(returnType) },
            Visibility = Visibility.Public,
            Params = [],
            Body = new BlockStatement()
            {
                Statements = [],
                Location = null
            },
            Location = createLocation(),
            Parent = null,
            Type = CreateType(name, 0, SymbolKind.MemberDef),
            IsStatic = false,
            IsConstructor = false
        };
    }

    private SourceLocationMetadata createLocation()
    {
        return new SourceLocationMetadata
        {
            Filename = "testFile.cs",
            OriginalText = "hello world",
            Line = 1,
            Column = 1
        };
    }

    private MethodDef createMethodDef(string name, string returnType)
    {
        var result = new MethodDefBuilder().WithName(MemberName.From(name)).WithVisibility(Visibility.Public).Build();
        result.FunctionDef = createFunctionDef(name, returnType);
        return result;
    }
}
