using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

public class ObjectInstantiationTests
{
    [Fact]
    public void NewKeyword_WithNoArguments_ShouldParse()
    {
        var input = @"
CreatePerson(): Person {
    return new Person();
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "new keyword with no arguments should parse");
    }

    [Fact]
    public void NewKeyword_WithArguments_ShouldParse()
    {
        var input = @"
CreatePerson(): Person {
    return new Person(""John"", 42);
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "new keyword with arguments should parse");
    }

    [Fact]
    public void NewKeyword_WithPropertyInitializers_ShouldParse()
    {
        var input = @"
CreatePerson(): Person {
    return new Person() {
        Name = ""Jane"",
        Age = 30
    };
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "new keyword with property initializers should parse");
    }

    [Fact]
    public void NewKeyword_WithArgumentsAndProperties_ShouldParse()
    {
        var input = @"
CreatePerson(): Person {
    return new Person(""Bob"") {
        Age = 25
    };
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "new keyword with both arguments and property initializers should parse");
    }

    [Fact]
    public void NewKeyword_InReturnStatement_ShouldParse()
    {
        var input = @"
CreatePerson(): Person {
    return new Person(""Alice"", 28);
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "new keyword in return statement should parse");
    }

    [Fact]
    public void NewKeyword_NestedInstantiation_ShouldParse()
    {
        var input = @"
CreateCompany(): Company {
    return new Company(new Address(""123 Main St""), ""Acme Corp"");
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "nested object instantiation should parse");
    }
}
