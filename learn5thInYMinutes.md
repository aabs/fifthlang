---
language: Fifth
filename: learnfifth.5th
contributors:
    - ["Fifth Language Team", "https://github.com/aabs/fifthlang"]
---

Fifth is a systems programming language with first-class support for knowledge graphs and semantic web technologies. It combines imperative programming with RDF triple management.

```fifth
// This is a single-line comment

/*
   Multi-line comments
   work like this
*/

//////////////////////////////////////
// 1. Basic Syntax and Literals
//////////////////////////////////////

// Every Fifth program needs a main function with an int return type
main(): int {
    return 0;
}

// Integer literals support multiple bases
main(): int {
    x: int;
    x = 42;           // Decimal
    x = 0b101010;     // Binary (prefix 0b)
    x = 0o52;         // Octal (prefix 0o)
    x = 0x2A;         // Hexadecimal (prefix 0x)
    x = 42i;          // Imaginary numbers
    
    return 0;
}

// Floating-point literals
main(): int {
    pi: float;
    pi = 3.14159;
    pi = 3.14e0;      // Scientific notation
    pi = 0x1.921fb54442d18p+1;  // Hex float
    
    return 0;
}

// Boolean literals
main(): int {
    t: bool;
    f: bool;
    t = true;
    f = false;
    
    return 0;
}

// String literals
main(): int {
    plain: string;
    raw: string;
    interpolated: string;
    
    plain = "Hello, World!";          // Interpreted strings
    raw = `Raw\nstring\twith\escapes`; // Raw strings (backticks)
    interpolated = $"Value is {x}";   // Interpolated strings ($ prefix)
    
    return 0;
}

// Rune literals (single characters)
main(): int {
    c: rune;
    c = 'A';
    c = '\n';         // Escape sequences
    c = '\u0041';     // Unicode escape
    
    return 0;
}

// Null literal
main(): int {
    ptr: int;
    ptr = null;
    
    return 0;
}

//////////////////////////////////////
// 2. Variables and Type Declarations
//////////////////////////////////////

// Variable declarations use colon syntax: name : type
main(): int {
    x: int;
    y: float;
    s: string;
    
    // Declaration with initialization
    z: int;
    z = 100;
    
    return 0;
}

// Type specifications for arrays and lists
main(): int {
    arr: int[10];          // Fixed-size array
    dynamicArr: int[];     // Dynamic array
    matrix: int[5][5];     // Multi-dimensional array
    
    // List type with brackets
    numbers: [int];
    
    // Generic types
    optional: Maybe<int>;
    
    return 0;
}

//////////////////////////////////////
// 3. Operators
//////////////////////////////////////

// Arithmetic operators
main(): int {
    x: int;
    x = 10 + 5;       // Addition
    x = 10 - 5;       // Subtraction
    x = 10 * 5;       // Multiplication
    x = 10 / 5;       // Division
    x = 10 % 3;       // Modulo
    x = 2 ** 8;       // Power (exponentiation with **)
    x = 2 ^ 8;        // Bitwise XOR (use ** for power)
    
    return 0;
}

// Comparison operators
main(): int {
    result: bool;
    result = 5 == 5;  // Equality
    result = 5 != 3;  // Inequality
    result = 5 < 10;  // Less than
    result = 5 <= 5;  // Less than or equal
    result = 10 > 5;  // Greater than
    result = 10 >= 5; // Greater than or equal
    
    return 0;
}

// Logical operators
main(): int {
    result: bool;
    result = true && false;   // Logical AND
    result = true || false;   // Logical OR
    result = !true;           // Logical NOT
    result = true !& false;   // Logical NAND
    result = true !| false;   // Logical NOR
    result = true ~ false;    // Logical XOR
    
    return 0;
}

// Bitwise operators
main(): int {
    x: int;
    x = 5 | 3;        // Bitwise OR
    x = 5 & 3;        // Bitwise AND
    x = 5 << 2;       // Left shift
    x = 20 >> 2;      // Right shift
    
    return 0;
}

// Increment and decrement
main(): int {
    x: int;
    x = 5;
    x++;              // Post-increment
    x--;              // Post-decrement
    ++x;              // Pre-increment
    --x;              // Pre-decrement
    
    return 0;
}

// Compound assignment
main(): int {
    x: int;
    x = 10;
    x += 5;           // x = x + 5
    x -= 3;           // x = x - 3
    
    return 0;
}

//////////////////////////////////////
// 4. Functions
//////////////////////////////////////

// Function declaration syntax: name(params): returnType { body }
add(x: int, y: int): int {
    return x + y;
}

// Multiple parameters
greet(firstName: string, lastName: string): string {
    return firstName;
}

// Functions are called before main is defined
main(): int {
    sum: int;
    sum = add(5, 3);
    
    return 0;
}

// Function with parameter constraints
// Use pipe | to specify constraints on parameters
positive(x: int | x > 0): int {
    return x;
}

// Parameter destructuring
class Person {
    FirstName: string;
    LastName: string;
}

greetPerson(p: Person { first: FirstName, last: LastName }): string {
    return first;
}

testGreet(): int {
    return 0;
}

//////////////////////////////////////
// 5. Control Flow
//////////////////////////////////////

// If statements
main(): int {
    x: int;
    x = 10;
    
    if (x > 5) {
        x = 1;
    }
    
    // If-else
    if (x < 5) {
        x = 0;
    } else {
        x = 1;
    }
    
    return 0;
}

// While loops
main(): int {
    i: int;
    i = 0;
    
    while (i < 10) {
        i++;
    }
    
    return 0;
}

// With statement (scoped resource management)
main(): int {
    with resource {
        ;
    }
    
    return 0;
}

//////////////////////////////////////
// 6. Lists and Comprehensions
//////////////////////////////////////

// List literals
main(): int {
    empty: [int];
    numbers: [int];
    
    empty = [];
    numbers = [1, 2, 3, 4, 5];
    
    return 0;
}

// List comprehensions with filtering
main(): int {
    xs: [int];
    ys: [int];
    
    xs = [1, 2, 3, 4, 5];
    
    // List comprehension: [var in source # constraint]
    ys = [x in xs # x > 2];  // [3, 4, 5]
    
    return 0;
}

// Accessing list elements
main(): int {
    numbers: [int];
    first: int;
    
    numbers = [10, 20, 30];
    first = numbers[0];      // Index access
    
    return 0;
}

//////////////////////////////////////
// 7. Classes and Objects
//////////////////////////////////////

// Class definition
class Rectangle {
    Width: float;
    Height: float;
}

// Class with methods
class Calculator {
    Value: int;
    
    Add(x: int): int {
        return Value + x;
    }
    
    Multiply(x: int): int {
        return Value * x;
    }
}

// Class instantiation
main(): int {
    rect: Rectangle;
    calc: Calculator;
    
    // Create new object (can be empty)
    rect = new Rectangle();
    
    // Create with property initialization
    rect = new Rectangle() {
        Width = 10.0,
        Height = 5.0
    };
    
    calc = new Calculator() { Value = 100 };
    
    return 0;
}

// Class inheritance
class Shape {
    Color: string;
}

class Circle extends Shape {
    Radius: float;
}

main(): int {
    c: Circle;
    c = new Circle();
    
    return 0;
}

// Member access
main(): int {
    rect: Rectangle;
    w: float;
    
    rect = new Rectangle() { Width = 10.0, Height = 5.0 };
    w = rect.Width;          // Access member
    rect.Width = 15.0;       // Modify member
    
    return 0;
}

//////////////////////////////////////
// 8. Module System
//////////////////////////////////////

// Import modules
use Math, IO, Net;

// Multiple imports
use System, Collections;

main(): int {
    return 0;
}

//////////////////////////////////////
// 9. Knowledge Graphs & Semantic Web
//////////////////////////////////////

// Alias declarations for IRI namespaces
alias ex as <http://example.org/>;
alias foaf as <http://xmlns.com/foaf/0.1/>;

// Store declaration (SPARQL endpoint)
// Syntax: name : store = sparql_store(<iri>);
myStore : store = sparql_store(<http://localhost:8080/graphdb>);

// Triple literals: <subject, predicate, object>
main(): int {
    // Triples use prefixed IRIs
    <ex:john, foaf:name, "John Doe">;
    <ex:john, foaf:age, 30>;
    <ex:john, ex:knows, ex:jane>;
    
    return 0;
}

// Graph declaration with inline triples
// Use <{ ... }> for graph assertion blocks
main(): int {
    // Graph variable with colon syntax
    g : graph in <ex:> = <{
        <ex:subject1, ex:predicate1, ex:object1>;
        <ex:subject2, ex:predicate2, 42>;
    }>;
    
    return 0;
}

// Graph assertion statement
main(): int {
    // Statement form: saves to default store
    <{
        <ex:s, ex:p, ex:o>;
    }>;
    
    return 0;
}

// Assigning graphs to stores
alias ex as <http://example.org/>;
myStore : store = sparql_store(<http://localhost:8080/graphdb>);

main(): int {
    myGraph : graph in <ex:> = <{
        <ex:entity, ex:property, "value">;
    }>;
    
    // Add graph to store
    myStore += myGraph;
    
    // Remove graph from store
    myStore -= myGraph;
    
    return 0;
}

// Classes in semantic context
class Entity in <http://example.org/ontology#> {
    Name: string;
    Value: int;
}

main(): int {
    e: Entity;
    e = new Entity() { Name = "Test", Value = 42 };
    
    return 0;
}

//////////////////////////////////////
// 10. Advanced Features
//////////////////////////////////////

// Nested destructuring
class Address {
    Street: string;
    City: string;
}

class Employee {
    Name: string;
    HomeAddress: Address;
}

processEmployee(
    e: Employee { 
        name: Name, 
        addr: HomeAddress { city: City } 
    }
): string {
    return City;
}

main(): int {
    return 0;
}

// Function with multiple constraints
constrained(
    x: int | x > 0,
    y: int | y < 100
): int {
    return x + y;
}

main(): int {
    result: int;
    result = constrained(5, 50);
    
    return 0;
}

// Expression statements
main(): int {
    x: int;
    
    // Any expression can be a statement
    5 + 3;
    x = 10;
    x > 5;
    
    return 0;
}

// Block statements
main(): int {
    x: int;
    
    {
        y: int;
        y = 5;
    }  // y goes out of scope
    
    return 0;
}

//////////////////////////////////////
// 11. Full Example: Semantic Application
//////////////////////////////////////

// Define namespace aliases
alias foaf as <http://xmlns.com/foaf/0.1/>;
alias ex as <http://example.org/people/>;

// Connect to a SPARQL store
peopleDB : store = sparql_store(<http://localhost:8080/graphdb>);

// Define a class for people
class Person {
    FirstName: string;
    LastName: string;
    Age: int;
}

// Function to calculate if someone is an adult
isAdult(age: int): bool {
    return age >= 18;
}

// Main program
main(): int {
    // Create a person
    john: Person;
    john = new Person() {
        FirstName = "John",
        LastName = "Doe",
        Age = 30
    };
    
    // Check if adult
    adult: bool;
    adult = isAdult(john.Age);
    
    // Create knowledge graph about this person
    johnGraph : graph in <ex:> = <{
        <ex:john, foaf:firstName, "John">;
        <ex:john, foaf:lastName, "Doe">;
        <ex:john, foaf:age, 30>;
    }>;
    
    // Save to the store
    peopleDB += johnGraph;
    
    return 0;
}

```

## Further Reading

* [Fifth Language Repository](https://github.com/aabs/fifthlang)
* [ANTLR Grammar Files](https://github.com/aabs/fifthlang/tree/master/src/parser/grammar)
* [Example Programs](https://github.com/aabs/fifthlang/tree/master/test/runtime-integration-tests/TestPrograms)
* [RDF Primer](https://www.w3.org/TR/rdf11-primer/)
* [SPARQL Query Language](https://www.w3.org/TR/sparql11-query/)
