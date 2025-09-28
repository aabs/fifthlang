using Antlr4.Runtime;
using ast;
using ast_model.TypeSystem;
using compiler.LangProcessingPhases;
using Fifth;
using FluentAssertions;

namespace ast_tests;

public class AstBuilderVisitorTests
{
    [Test]
    public void can_build_from_function_def()
    {
        var funcdefsrc = $$"""
                         foo(b: Bar, b2: Baz): Sqz{}
                         """;
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();
        if (x is FifthParser.Function_declarationContext ctx)
        {
            var v = new AstBuilderVisitor();
            var a = v.Visit(ctx) as FunctionDef;
            a.Should().NotBeNull();
        }
    }

    [Test]
    public void can_build_from_function_def_with_if_statement()
    {
        var funcdefsrc = $$"""
                         foo(b: Bar, b2: Baz): Sqz
                         {
                            if(b == 1)
                            {
                                return "hello";
                            }
                            else
                            {
                                return "world";
                            }
                         }
                         """;
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();
        var v = new AstBuilderVisitor();
        var a = v.VisitFunction_declaration(x) as FunctionDef;
        a.Should().NotBeNull();
        var stmt = a.Body.Statements[0];
        stmt.Should().NotBeNull();
        if (stmt is IfElseStatement ies)
        {
            ies.Condition.Should().BeOfType<BinaryExp>();
        }
        else
        {
            throw new InvalidOperationException("Unexpected parse context type");
        }
    }

    [Test]
    [MethodDataSource(nameof(DoubleSamples))]
    public void can_parse_double_literals(double d)
    {
        var nativeRepresentation = $"{d:0.000}d";
        if (nativeRepresentation is "infinityd" or "-infinityd" or "Infinityd" or "-Infinityd" or
            "infinity" or "-infinity" or "Infinity" or "-Infinity" or
            "NaNd" or "NaN" or "\u221ed" or "-\u221ed" or "\u221E" or "-\u221E")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
            a.Should().BeOfType<Float8LiteralExp>();
    }

    [Test]
    public void can_parse_floats_case1()
    {
        var s = CharStreams.fromString("4.940656458e-324");
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Float4LiteralExp>();
    }

    [Test]
    public void can_parse_floats_case2()
    {
        var s = CharStreams.fromString("0.0");
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        a.Should().BeOfType<Float4LiteralExp>();
    }

    [Test]
    [MethodDataSource(nameof(IntSamples))]
    public void can_parse_int_literals(int d)
    {
        var nativeRepresentation = $"{d}";
        if (nativeRepresentation is "infinity" or "-infinity" or "NaN" or "\u221e" or "-\u221e")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
            a.Should().BeOfType<Int32LiteralExp>();
    }

    [Test]
    public void can_parse_int_literals_case1()
    {
        int d = -1;
        var nativeRepresentation = $"{d}";
        if (nativeRepresentation is "infinity" or "-infinity" or "NaN" or "\u221e" or "-\u221e")
        {
            return;
        }

        var s = CharStreams.fromString(nativeRepresentation);
        var p = GetParserFor(s);
        var actual = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(actual);
        a.Should().NotBeNull();
        if (d < 0)
            a.Should().BeOfType<UnaryExp>();
        else
            a.Should().BeOfType<Int32LiteralExp>();
    }

    [Test]
    [Arguments("a = 5;")]
    [Arguments("a = 5 * 6;")]
    public void handles_assignment_statements(string exp)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.statement();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<AssignmentStatement>();
    }

    [Test]
    [Arguments(0, "Name", "string")]
    [Arguments(1, "Height", "float")]
    [Arguments(2, "Age", "float")]
    [Arguments(3, "Weight", "float")]
    public void handles_class_definition(int ord, string name, string typename)
    {
        var p = GetParserFor("class-definition.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<AssemblyDef>();
        var ad = a as AssemblyDef ?? throw new InvalidOperationException("Expected AssemblyDef");
        var cd = ad.Modules[0].Classes[0];
        cd.MemberDefs.Should().NotBeEmpty();
        cd.MemberDefs.All(o => o is not null).Should().BeTrue();
        cd.Type.Should().BeOfType<FifthType.TType>();
        var prop1 = cd.MemberDefs[ord] as PropertyDef;
        prop1.Should().NotBeNull();
        prop1.Type.Should().BeOfType<FifthType.TVoidType>();
        prop1.Name.Value.Should().Be(name);
        prop1.TypeName.Value.Should().Be(typename);
    }

    [Test]
    public void handles_function_overloading()
    {
        var p = GetParserFor("overloading.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.VisitFifth(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        m.Classes.Should().HaveCount(0);
        m.Functions.Should().HaveCount(4);
        m.Functions.All(o => o is not null).Should().BeTrue();

        // Validate the first three functions
        for (int i = 0; i < 3; i++)
        {
            var func = m.Functions[i] as FunctionDef;
            func.Should().NotBeNull();
            func.Name.Value.Should().Be("foo");
            func.ReturnType.Name.Value.ToLowerInvariant().Should().Be("string");
        }

        // Validate the fourth function
        var mainFunc = m.Functions[3] as FunctionDef;
        mainFunc.Should().NotBeNull();
        mainFunc.Name.Value.Should().Be("main");

        // examine the function params
        var f = m.Functions[0] as FunctionDef ?? throw new InvalidOperationException("Expected FunctionDef");
        f.Params.Should().HaveCount(1);
        var pa = f.Params[0];
        pa.Name.Should().Be("i");
        pa.TypeName.Value.Should().Be("int");
        pa.ParameterConstraint.Should().NotBeNull();
        var pc = pa.ParameterConstraint as BinaryExp;
        pc.Should().NotBeNull();
        pc.LHS.Should().NotBeNull();
        pc.LHS.Should().BeOfType<VarRefExp>();
        var lhsv = ((VarRefExp)pc.LHS).VarName;
        lhsv.Should().Be("i");
        pc.Operator.Should().Be(Operator.LessThanOrEqual);
        pc.RHS.Should().BeOfType<Int32LiteralExp>();
        var rhsv = ((Int32LiteralExp)pc.RHS).Value;
        rhsv.Should().Be(15);

        f.Body.Should().NotBeNull();
        f.Body.Statements.Should().HaveCount(1);
        var stmt = f.Body.Statements[0];
        stmt.Should().BeOfType<ReturnStatement>();
        var ret = stmt as ReturnStatement ?? throw new InvalidOperationException("Expected ReturnStatement");
        ret.ReturnValue.Should().BeOfType<StringLiteralExp>();
        var retv = ret.ReturnValue as StringLiteralExp ?? throw new InvalidOperationException("Expected StringLiteralExp");
        retv.Value.Should().Be("\"child\"");
    }

    [Test]
    public void handles_if_statements()
    {
        var p = GetParserFor("statement-if.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s1 = ((FunctionDef)m.Functions[0]).Body.Statements[1];
        s1.Should().NotBeNull().And
            .Subject.Should().BeOfType<IfElseStatement>();
        var ifstmt = s1 as IfElseStatement ?? throw new InvalidOperationException("Expected IfElseStatement");
        ifstmt.Condition.Should().NotBeNull();
        ifstmt.ThenBlock.Should().NotBeNull();
        ifstmt.ElseBlock.Should().BeNull();
    }

    [Test]
    public void handles_ifelse_statements()
    {
        var p = GetParserFor("statement-ifelse.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s1 = ((FunctionDef)m.Functions[0]).Body.Statements[1];
        s1.Should().NotBeNull().And
            .Subject.Should().BeOfType<IfElseStatement>();
        var ifstmt = s1 as IfElseStatement ?? throw new InvalidOperationException("Expected IfElseStatement");
        ifstmt.Condition.Should().NotBeNull();
        ifstmt.ThenBlock.Should().NotBeNull();
        ifstmt.ElseBlock.Should().NotBeNull();
    }

    [Test]
    public void handles_list_comprehensions()
    {
        var p = GetParserFor("statement-list-decl.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s0 = ((FunctionDef)m.Functions[0]).Body.Statements[0];
        s0.Should().NotBeNull().And
            .Subject.Should().BeOfType<VarDeclStatement>();
        var s0vd = s0 as VarDeclStatement ?? throw new InvalidOperationException("Expected VarDeclStatement");
        s0vd.VariableDecl.Should().NotBeNull();
        s0vd.VariableDecl.TypeName.Value.Should().Be("int");
        s0vd.VariableDecl.CollectionType.Should().Be(CollectionType.List);
        s0vd.InitialValue.Should().NotBeNull();
        s0vd.InitialValue.Should().BeOfType<ListComprehension>();
    }

    [Test]
    public void handles_list_literals()
    {
        var p = GetParserFor("statement-list-literal.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s0 = ((FunctionDef)m.Functions[0]).Body.Statements[0];
        s0.Should().NotBeNull().And
            .Subject.Should().BeOfType<VarDeclStatement>();
        var s0vd = s0 as VarDeclStatement ?? throw new InvalidOperationException("Expected VarDeclStatement");
        s0vd.VariableDecl.Should().NotBeNull();
        s0vd.VariableDecl.TypeName.Value.Should().Be("int");
        s0vd.VariableDecl.CollectionType.Should().Be(CollectionType.List);
        s0vd.InitialValue.Should().NotBeNull();
        s0vd.InitialValue.Should().BeOfType<ListLiteral>();
    }

    [Test]
    public void handles_property_access()
    {
        var p = GetParserFor("property-access.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s1 = ((FunctionDef)m.Functions[0]).Body.Statements[1];
        s1.Should().NotBeNull();
        s1.Should().BeOfType<AssignmentStatement>();
        var s1as = s1 as AssignmentStatement ?? throw new InvalidOperationException("Expected AssignmentStatement");
        s1as.LValue.Should().BeOfType<MemberAccessExp>();
        var s1aslv = s1as.LValue as MemberAccessExp ?? throw new InvalidOperationException("Expected MemberAccessExp");
        s1aslv.LHS.Should().NotBeNull();
        s1aslv.LHS.Should().BeOfType<VarRefExp>();
        var s1aslvl = s1aslv.LHS as VarRefExp ?? throw new InvalidOperationException("Expected VarRefExp");
        s1aslvl.VarName.Should().Be("p");
        var s1aslvr = s1aslv.RHS as VarRefExp ?? throw new InvalidOperationException("Expected VarRefExp");
        s1aslvr.VarName.Should().Be("Weight");
    }

    [Test]
    public void handles_recursive_destructure_definitions()
    {
        var p = GetParserFor("recursive-destructuring.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var p0 = ((FunctionDef)m.Functions[0]).Params[0];
        p0.DestructureDef.Should().NotBeNull();
        p0.DestructureDef.Should().BeOfType<ParamDestructureDef>();
        p0.DestructureDef.Bindings.Should().HaveCount(2);
        var p0b1 = p0.DestructureDef.Bindings[1];
        p0b1.Should().NotBeNull();
        p0b1.IntroducedVariable.Value.Should().Be("vitals");
        p0b1.ReferencedPropertyName.Value.Should().Be("Vitals");
        p0b1.DestructureDef.Should().NotBeNull();
        p0b1.DestructureDef.Should().BeOfType<ParamDestructureDef>();
        p0b1.DestructureDef.Bindings.Should().HaveCount(3);
        var p0b1b2 = p0b1.DestructureDef.Bindings[2];
        p0b1b2.IntroducedVariable.Value.Should().Be("weight");
        p0b1b2.ReferencedPropertyName.Value.Should().Be("Weight");
    }

    [Test]
    [Arguments("a: int;", false)]
    [Arguments("a: int = 5;", true)]
    public void handles_vardecl_statements(string exp, bool shouldHaveInitialiserExpression)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.statement();
        var v = new AstBuilderVisitor();
        var a = v.VisitStatement(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<VarDeclStatement>();
        var vds = a as VarDeclStatement ?? throw new InvalidOperationException("Expected VarDeclStatement");
        vds.VariableDecl.Should().NotBeNull();
        vds.VariableDecl.TypeName.Should().NotBeNull();
        vds.VariableDecl.TypeName.Value.Should().Be("int");
        vds.VariableDecl.Name.Should().NotBeNull();
        vds.VariableDecl.Name.Should().Be("a");
        if (shouldHaveInitialiserExpression)
        {
            vds.InitialValue.Should().NotBeNull();
        }
    }

    [Test]
    public void handles_while_statements()
    {
        var p = GetParserFor("statement-while.5th");
        var x = p.fifth();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as AssemblyDef;
        a.Should().NotBeNull();
        a.Modules.Should().HaveCount(1);
        var m = a.Modules[0];
        var s1 = ((FunctionDef)m.Functions[0]).Body.Statements[1];
        s1.Should().NotBeNull().And
            .Subject.Should().BeOfType<WhileStatement>();
    }

    [Test]
    [Arguments("3 + 7", Operator.ArithmeticAdd)]
    [Arguments("3 - 7", Operator.ArithmeticSubtract)]
    [Arguments("3 * 7", Operator.ArithmeticMultiply)]
    [Arguments("3 / 7", Operator.ArithmeticDivide)]
    [Arguments("3 ** 7", Operator.ArithmeticPow)]
    [Arguments("3 % 7", Operator.ArithmeticMod)]
    [Arguments("3 == 7", Operator.Equal)]
    [Arguments("3 != 7", Operator.NotEqual)]
    [Arguments("3 > 7", Operator.GreaterThan)]
    [Arguments("3 < 7", Operator.LessThan)]
    [Arguments("3 <= 7", Operator.LessThanOrEqual)]
    [Arguments("3 >= 7", Operator.GreaterThanOrEqual)]
    [Arguments("3 & 7", Operator.BitwiseAnd)]
    [Arguments("3 | 7", Operator.BitwiseOr)]
    [Arguments("3 << 7", Operator.BitwiseLeftShift)]
    [Arguments("3 >> 7", Operator.BitwiseRightShift)]
    [Arguments("3 && 7", Operator.LogicalAnd)]
    [Arguments("3 || 7", Operator.LogicalOr)]
    [Arguments("3 ^ 7", Operator.ArithmeticPow)]
    [Arguments("3 ~ 7", Operator.LogicalXor)]
    public void should_handle_all_kinds_of_binary_expressions(string exp, Operator op)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<BinaryExp>();
    }

    [Test]
    [Arguments("+ 7", Operator.ArithmeticAdd)]
    [Arguments("+7", Operator.ArithmeticAdd)]
    [Arguments("- 7", Operator.ArithmeticSubtract)]
    [Arguments("-7", Operator.ArithmeticSubtract)]
    [Arguments("! 7", Operator.LogicalNot)]
    [Arguments("!7", Operator.LogicalNot)]
    public void should_handle_all_kinds_of_unary_expressions(string exp, Operator op)
    {
        var s = CharStreams.fromString(exp);
        var p = GetParserFor(s);
        var x = p.expression();
        var v = new AstBuilderVisitor();
        var a = v.Visit(x);
        a.Should().NotBeNull();
        a.Should().BeOfType<UnaryExp>();
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

    [Test]
    public void function_with_int_return_type_should_have_correct_type_annotation()
    {
        // Initialize TypeRegistry with primitive types
        ast_model.TypeSystem.TypeRegistry.DefaultRegistry.RegisterPrimitiveTypes();

        var funcdefsrc = "main():int{return 42;}";
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();

        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as FunctionDef;
        a.Should().NotBeNull();

        // The function should have the correct name
        a.Name.Value.Should().Be("main");

        // The return type should NOT be UnknownType
        a.ReturnType.Should().NotBeOfType<FifthType.UnknownType>();

        // The return type should be a TDotnetType wrapping int
        a.ReturnType.Should().BeOfType<FifthType.TDotnetType>();

        if (a.ReturnType is FifthType.TDotnetType dotnetType)
        {
            dotnetType.TheType.Should().Be(typeof(int));
            dotnetType.Name.Value.Should().Be("Int32");
        }
    }

    [Test]
    [Arguments("int", typeof(int), "Int32")]
    [Arguments("string", typeof(string), "String")]
    [Arguments("bool", typeof(bool), "Boolean")]
    [Arguments("float", typeof(float), "Single")]
    [Arguments("double", typeof(double), "Double")]
    public void function_return_type_mappings_should_work_correctly(string languageTypeName, Type expectedDotnetType, string expectedTypeName)
    {
        // Initialize TypeRegistry with primitive types
        ast_model.TypeSystem.TypeRegistry.DefaultRegistry.RegisterPrimitiveTypes();

        var funcdefsrc = $"test():{languageTypeName}{{return null;}}";
        var s = CharStreams.fromString(funcdefsrc);
        var p = GetParserFor(s);
        var x = p.function_declaration();
        x.Should().NotBeNull();

        var v = new AstBuilderVisitor();
        var a = v.Visit(x) as FunctionDef;
        a.Should().NotBeNull();

        // The return type should NOT be UnknownType
        a.ReturnType.Should().NotBeOfType<FifthType.UnknownType>();

        // The return type should be a TDotnetType wrapping the expected type
        a.ReturnType.Should().BeOfType<FifthType.TDotnetType>();

        if (a.ReturnType is FifthType.TDotnetType dotnetType)
        {
            dotnetType.TheType.Should().Be(expectedDotnetType);
            dotnetType.Name.Value.Should().Be(expectedTypeName);
        }
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        Type t = typeof(AstBuilderVisitorTests);
        using (Stream? stream = t.Assembly.GetManifestResourceStream(t.Namespace + ".CodeSamples." + resourceName))
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
    public static IEnumerable<double> DoubleSamples()
    {
        return new[] { -100.5, -1.0, 0.0, 1.5, 3.14, 42.0, 999.999 };
    }

    public static IEnumerable<int> IntSamples()
    {
        return new[] { -100, -1, 0, 1, 42, 123456 };
    }
}
