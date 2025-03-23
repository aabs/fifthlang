using ast;
using ast_generated;
using ast_model.TypeSystem;
using Fifth.LangProcessingPhases;

namespace ast_tests;

public class OverloadTransformingVisitorTests
{
    public static BooleanLiteralExp False = new BooleanLiteralExp() { Value = false };
    public static BooleanLiteralExp True = new BooleanLiteralExp() { Value = true };

    [Fact]
    public void GenerateGuardFunction_ShouldCreateGuardFunction()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var cd = CreateClassDef("class1");
        var methodDef1 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var methodDef2 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var clauses = new List<(Expression, MethodDef)>
        {
            (True, methodDef1),
            (False, methodDef2)
        };
        var overloadedFunction = new OverloadedFunctionDefinitionBuilder()
            .WithName(MemberName.From("Test1"))
            .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 })
            .WithSignature(methodDef1.FunctionDef.ToFunctionSignature())
            .Build().WithParent(cd);

        // Act
        var guardFunction = visitor.GenerateGuardFunction((OverloadedFunctionDefinition)overloadedFunction, clauses);

        // Assert
        Assert.NotNull(guardFunction);
        Assert.Equal("Test1", guardFunction.Name.Value);
    }

    [Fact]
    public void GetPrecondition_ShouldReturnCombinedPreconditions()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var methodDef = CreateMethodDef("Test", "int");
        methodDef.FunctionDef.Params = [CreateParamDef("p1", "int", True), CreateParamDef("p2", "int", False)];

        // Act
        var precondition = visitor.GetPrecondition(methodDef);

        // Assert
        Assert.NotNull(precondition);
        Assert.IsType<BinaryExp>(precondition);
    }

    [Fact]
    public void ProcessOverloadedFunctionDefinition_ShouldTransformFunction()
    {
        // Arrange
        var visitor = new OverloadTransformingVisitor();
        var cd = CreateClassDef("class1");
        var methodDef1 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var methodDef2 = (MethodDef)CreateMethodDef("Test1", "int").WithParent(cd);
        var clauses = new List<(Expression, MethodDef)>
        {
            (True, methodDef1),
            (False, methodDef2)
        };
        var overloadedFunction = new OverloadedFunctionDefinitionBuilder()
            .WithName(MemberName.From("Test1"))
            .WithOverloadClauses(new List<MethodDef> { methodDef1, methodDef2 })
            .WithSignature(methodDef1.FunctionDef.ToFunctionSignature())
            .Build().WithParent(cd);

        // Act
        visitor.ProcessOverloadedFunctionDefinition((OverloadedFunctionDefinition)overloadedFunction);

        // Assert
        Assert.Contains(cd.MemberDefs, m => m.Name == "Test1_subclause1");
        Assert.Contains(cd.MemberDefs, m => m.Name == "Test1_subclause2");
    }

    private ClassDef CreateClassDef(string name) => new ClassDefBuilder().WithName(TypeName.From(name)).WithMemberDefs([]).Build();

    private FunctionDef CreateFunctionDef(string name, string returnType)
    {
        return new FunctionDef
        {
            Annotations = [],
            Name = MemberName.From(name),
            ReturnType = new FifthType.TType() { Name = TypeName.From(returnType) },
            Visibility = Visibility.Public,
            Params = [],
            Body = new BlockStatement
            {
                Statements = new List<Statement>(),
                Location = null
            },
            Location = CreateLocation(),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(int)) { Name = TypeName.From("int") },
            IsStatic = false,
            IsConstructor = false
        };
    }

    private SourceLocationMetadata CreateLocation()
    {
        return new SourceLocationMetadata
        {
            Filename = "testFile.cs",
            OriginalText = "hello world",
            Line = 1,
            Column = 1
        };
    }

    private MethodDef CreateMethodDef(string name, string returnType)
    {
        var result = new MethodDefBuilder()
            .WithName(MemberName.From(name))
            .WithVisibility(Visibility.Public)
            .Build();
        result.FunctionDef = CreateFunctionDef(name, returnType);
        result.Type = result.FunctionDef.Type;
        return result;
    }

    private ParamDef CreateParamDef(string name, string typeName, Expression constraint)
    {
        return new ParamDef
        {
            Name = name,
            TypeName = TypeName.From(typeName),
            ParameterConstraint = constraint,
            Visibility = Visibility.Public,
            Annotations = [],
            DestructureDef = null,
            Location = CreateLocation(),
            Parent = null,
        };
    }
}
