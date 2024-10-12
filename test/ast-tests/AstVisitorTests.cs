using ast;
using ast_generated;
using ast_model.TypeSystem;
using FluentAssertions;

namespace ast_tests;
public class AstVisitorTests
{
    [Fact]
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

    AssemblyDef CreateAssemblyDef()
    {
        return new AssemblyDef()
        {
            Annotations = [],
            AssemblyRefs = [],
            ClassDefs = [createClassDef("MyType1", 1),createClassDef("MyType2", 2)],
            Name = AssemblyName.From("MyAsm"),
            Type = CreateType("myAsm", 0, SymbolKind.Assembly),
            Parent = null,
            PublicKeyToken = "ajhsgjsfdhg",
            Version = "0.1.1.1",
            Visibility = Visibility.Public
        };
    }

    private static FifthType CreateType(string name, ushort typeId, SymbolKind symbolKind = SymbolKind.Assembly)
    {
        return new()
        {
            TypeId = TypeId.From(typeId),
            Symbol = new Symbol()
            {
                Name = name,
                Kind = symbolKind,
                Location =
                    new()
                    {
                        Column = 1, Filename = "ashksah", Line = 1, OriginalText = "kjahgsfdgasfjd"
                    }
            },
            IsArray = false,
            ParentTypes = [],
            TypeArguments = [],
            Annotations = []
        };
    }

    private static ClassDef createClassDef(string name, ushort typeId)
    {
        return new ClassDef()
        {
            Annotations = [],
            MemberDefs = [],
            Namespace = "std",
            Parent = null,
            Type = CreateType(name, typeId, SymbolKind.ClassDef),
            Visibility = Visibility.Public
        };
    }
}
