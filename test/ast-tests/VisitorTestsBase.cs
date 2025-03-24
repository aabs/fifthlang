using Antlr4.Runtime;
using ast;
using ast_generated;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using compiler.LanguageTransformations;
using Fifth;

namespace ast_tests;

public class VisitorTestsBase
{
    protected static readonly BooleanLiteralExp False = new BooleanLiteralExp() { Value = false };
    protected static readonly BooleanLiteralExp True = new BooleanLiteralExp() { Value = true };
    protected static readonly FifthType voidType = new FifthType.TVoidType() { Name = TypeName.From("void") };

    protected static FifthType CreateType(string name, ushort typeId, SymbolKind symbolKind = SymbolKind.Assembly)
    {
        return voidType;
    }

    protected ClassDef CreateClassDef(string name) => new ClassDefBuilder().WithName(TypeName.From(name)).WithMemberDefs([]).Build();

    protected FunctionDef createFunctionDef(string name, string returnType)
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

    protected FunctionDef CreateFunctionDef(string name, string returnType)
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

    protected SourceLocationMetadata createLocation()
    {
        return new SourceLocationMetadata
        {
            Filename = "testFile.cs",
            OriginalText = "hello world",
            Line = 1,
            Column = 1
        };
    }

    protected SourceLocationMetadata CreateLocation()
    {
        return new SourceLocationMetadata
        {
            Filename = "testFile.cs",
            OriginalText = "hello world",
            Line = 1,
            Column = 1
        };
    }

    protected MethodDef CreateMethodDef(string name, string returnType)
    {
        var result = new MethodDefBuilder()
            .WithName(MemberName.From(name))
            .WithVisibility(Visibility.Public)
            .Build();
        result.FunctionDef = CreateFunctionDef(name, returnType);
        result.Type = result.FunctionDef.Type;
        return result;
    }

    protected ParamDef CreateParamDef(string name, string typeName, Expression constraint)
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

    #region Helpers

    public AssemblyDef ParseProgram(string programFileName)
    {
        var parser = GetParserFor(programFileName);
        // Act
        var tree = parser.fifth();
        var v = new AstBuilderVisitor();
        var ast = v.Visit(tree);
        var vlv = new TreeLinkageVisitor();
        vlv.Visit((AstThing)ast);
        var visitor = new SymbolTableBuilderVisitor();
        visitor.Visit((AstThing)ast);
        // Assert
        return ast as AssemblyDef;
    }

    private static FifthParser GetParserFor(string sourceFile)
    {
        string content = ReadEmbeddedResource(sourceFile);
        var s = CharStreams.fromString(content);
        return GetParserFor(s);
    }

    private static FifthParser GetParserFor(ICharStream source)
    {
        var lexer = new FifthLexer(source);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ThrowingErrorListener<int>());

        var parser = new FifthParser(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ThrowingErrorListener<IToken>());
        return parser;
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        Type t = typeof(AstBuilderVisitorTests);
        Console.WriteLine(string.Join('\n', t.Assembly.GetManifestResourceNames()));
        using (Stream stream = t.Assembly.GetManifestResourceStream(t.Namespace + ".CodeSamples." + resourceName))
        {
            if (stream == null)
            {
                throw new FileNotFoundException("Resource not found", resourceName);
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    #endregion Helpers
}
