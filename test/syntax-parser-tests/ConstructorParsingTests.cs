using syntax_parser_tests.Utils;

namespace syntax_parser_tests;

public class ConstructorParsingTests
{
    [Test]
    public void BasicConstructor_ShouldParse()
    {
        var input = @"
class Person {
    Name: string;
    Age: int;
    
    Person(name: string, age: int) {
        this.Name = name;
        this.Age = age;
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Basic constructor with parameters should parse");
    }

    [Test]
    public void ParameterlessConstructor_ShouldParse()
    {
        var input = @"
class Person {
    Name: string;
    
    Person() {
        this.Name = ""default"";
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Parameterless constructor should parse");
    }

    [Test]
    public void ConstructorWithBaseCall_ShouldParse()
    {
        var input = @"
class Employee {
    Id: int;
    
    Employee(id: int) : base() {
        this.Id = id;
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Constructor with base call should parse");
    }

    [Test]
    public void ConstructorWithBaseCallAndArgs_ShouldParse()
    {
        var input = @"
class Employee {
    Id: int;
    
    Employee(name: string, id: int) : base(name) {
        this.Id = id;
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Constructor with base call and arguments should parse");
    }

    [Test]
    public void MultipleConstructors_ShouldParse()
    {
        var input = @"
class Person {
    Name: string;
    Age: int;
    
    Person(name: string) {
        this.Name = name;
        this.Age = 0;
    }
    
    Person(name: string, age: int) {
        this.Name = name;
        this.Age = age;
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Multiple constructor overloads should parse");
    }

    [Test]
    public void Constructor_WithMixedMembers_ShouldParse()
    {
        var input = @"
class Person {
    Name: string;
    Age: int;
    
    Person(name: string) {
        this.Name = name;
    }
    
    GetName(): string {
        return this.Name;
    }
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
            "Constructor mixed with regular methods should parse");
    }
}
