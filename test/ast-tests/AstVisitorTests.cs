using ast;
using ast_generated;
using ast_model.TypeSystem;
using FluentAssertions;

namespace ast_tests;

public class AstVisitorTests
{
    [Test]
    public void can_visit_and_recreate_an_AST_while_preserving_its_other_data()
    {
        var def = CreateAssemblyDef();
        var expectedAnnotation = Guid.NewGuid().ToString();
        def.Annotations["someKey"] = expectedAnnotation;
        var sut = new DefaultRecursiveDescentVisitor();
        var def2 = (AssemblyDef)sut.Visit(def);
        def2.Should().NotBeNull();
        def2.Should().BeEquivalentTo(def);
        def2.Annotations["someKey"].Should().Be(expectedAnnotation);
    }

    private static readonly FifthType voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };

    private static ClassDef createClassDef(string name, ushort typeId)
    {
        return new ClassDef()
        {
            Annotations = [],
            MemberDefs = [],
            Name = TypeName.From("std.MyClass"),
            Parent = null,
            Type = CreateType(name, typeId, SymbolKind.ClassDef),
            Visibility = Visibility.Public,
            Location = null,
            BaseClasses = [],
            AliasScope = null
        };
    }

    private static FifthType CreateType(string name, ushort typeId, SymbolKind symbolKind = SymbolKind.Assembly)
    {
        return voidType;
    }

    private AssemblyDef CreateAssemblyDef()
    {
        return new AssemblyDef()
        {
            Annotations = [],
            AssemblyRefs = [],
            Modules = [CreateModuleDef()],
            Name = AssemblyName.From("MyAsm"),
            Type = CreateType("myAsm", 0, SymbolKind.Assembly),
            Parent = null,
            PublicKeyToken = "ajhsgjsfdhg",
            TestProperty = "test-value",
            Version = "0.1.1.1",
            Visibility = Visibility.Public
        };
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
            Location = null,
            Parent = null,
            Type = CreateType(name, 0, SymbolKind.MemberDef),
            IsStatic = false,
            IsConstructor = false
        };
    }

    private ModuleDef CreateModuleDef()
    {
        return new ModuleDef()
        {
            Annotations = [],
            Classes = [createClassDef("MyType1", 1), createClassDef("MyType2", 2)],
            OriginalModuleName = "MyModule",
            Type = voidType,
            Parent = null,
            Visibility = Visibility.Public,
            NamespaceDecl = NamespaceName.From("MyNamespace"),
            Imports = [],
            Functions = [createFunctionDef("foo", "int")],
        };
    }
}
