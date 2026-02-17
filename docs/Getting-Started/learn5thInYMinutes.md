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
// IMPORTANT: When using constraints, you must provide a base case
// The base case is an unconstrained version that handles all other inputs
positive(x: int | x > 0): int {
    return x * 2;  // Handles positive numbers
}

positive(x: int): int {
    return 0;  // Base case: handles zero and negative numbers
}

// Multiple constrained overloads with a base case
classify(x: int | x < 0): string {
    return "negative";
}

classify(x: int | x == 0): string {
    return "zero";
}

classify(x: int | x > 0): string {
    return "positive";
}

classify(x: int): string {
    return "unknown";  // Base case (fallback)
}

callClassify(): int {
    return 0;
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

// Try/catch/finally for exception handling
main(): int {
    result: int;
    result = 0;
    
    // Try with finally - finally always executes
    try {
        result = 10;
    } finally {
        std.print("cleanup");
    }
    
    // Try/catch - handles exceptions
    try {
        result = 42;
    } catch {
        result = 1;  // Catch-all handler
    }
    
    // Try/catch/finally combined
    try {
        result = 10;
    } catch {
        result = 1;
    } finally {
        result = result + 5;  // Always executes
    }
    
    return result;
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
// 8. Generic Types
//////////////////////////////////////

// Generic classes allow type parameters for reusable data structures
// Syntax: class Name<TypeParam> { ... }

class Stack<T> {
    items: [T];
    
    push(item: T): int {
        return 0;
    }
    
    pop(): T {
        return items;
    }
}

main(): int {
    intStack: Stack<int>;
    stringStack: Stack<string>;
    
    // Each instantiation creates a distinct type
    intStack = new Stack<int>();
    stringStack = new Stack<string>();
    
    return 0;
}

// Multiple type parameters
class Pair<T1, T2> {
    first: T1;
    second: T2;
}

class Dictionary<TKey, TValue> {
    keys: [TKey];
    values: [TValue];
}

main(): int {
    pair: Pair<int, string>;
    dict: Dictionary<string, int>;
    
    pair = new Pair<int, string>() {
        first = 42,
        second = "answer"
    };
    
    return 0;
}

// Generic functions with type inference
// The compiler can infer type arguments from the call site
identity<T>(x: T): T {
    return x;
}

main(): int {
    result: int;
    text: string;
    
    // Explicit type arguments
    result = identity<int>(42);
    text = identity<string>("hello");
    
    // Type inference (types inferred from arguments)
    result = identity(42);        // Infers T = int
    text = identity("world");     // Infers T = string
    
    return 0;
}

// Functions with multiple type parameters
pair<T1, T2>(a: T1, b: T2): int {
    return 0;
}

triple<T1, T2, T3>(a: T1, b: T2, c: T3): int {
    return 0;
}

main(): int {
    // Type inference works with multiple parameters
    pair(1, "one");          // Infers T1=int, T2=string
    triple(1, 2.0, "three"); // Infers T1=int, T2=float, T3=string
    
    return 0;
}

// Type constraints ensure type parameters meet requirements
// Syntax: where TypeParam: Constraint

// Interface constraint
sort<T>(items: int): int where T: IComparable {
    return 0;
}

// Base class constraint
extend<T>(base: T): int where T: BaseClass {
    return 0;
}

// Multiple constraints
process<T>(item: T): int where T: IComparable, IDisposable {
    return 0;
}

// Constraints on multiple type parameters
class Mapper<TIn, TOut> where TIn: IComparable where TOut: BaseType {
    input: TIn;
    output: TOut;
}

main(): int {
    mapper: Mapper<int, string>;
    
    return 0;
}

// Generic methods in classes
class Container<T> {
    value: T;
    
    // Non-generic method using class type parameter
    getValue(): T {
        return value;
    }
    
    // Methods can have their own type parameters
    // (Note: Parser limitations exist for method-level generics)
}

class Util {
    // Generic methods in non-generic classes
    swap(x: int, y: int): int {
        return 0;
    }
}

main(): int {
    container: Container<int>;
    util: Util;
    
    container = new Container<int>() { value = 42 };
    util = new Util();
    
    return 0;
}

// Nested generic types
class Box<T> {
    items: [T];  // List of T
}

main(): int {
    // Box containing a list of integers
    intBox: Box<int>;
    
    // You can nest generic types
    // (Advanced nested syntax may have parser limitations)
    
    return 0;
}

// Key Points about Generics:
// 1. Full type reification: Stack<int> and Stack<string> are distinct types at runtime
// 2. Type inference works from function call arguments (local inference, C#-style)
// 3. Type parameters can have constraints (interface, base class)
// 4. Multiple type parameters supported: Pair<T1, T2>, Dictionary<TKey, TValue>
// 5. Generic methods inherit functionality from class type parameters
// 6. Backward compatible: all non-generic code works unchanged

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

// Graph declaration
main(): int {
    // Graph variable with colon syntax
    g : graph in <ex:> = KG.CreateGraph();
    // Add triples to the graph
    g += <ex:subject1, ex:predicate1, ex:object1>;
    g += <ex:subject2, ex:predicate2, 42>;
    
    return 0;
}

// Working with graphs
main(): int {
    g : graph = KG.CreateGraph();
    g += <ex:s, ex:p, ex:o>;
    
    return 0;
}

// Working with graphs and objects
class Person {
    Name: string;
    Age: int;
}

main(): int {
    // Create an object
    alice: Person;
    alice = new Person();
    
    // Create graph and add triples
    peopleGraph : graph in <ex:> = KG.CreateGraph();
    // Add explicit triple literals
    peopleGraph += <ex:alice, ex:name, "Alice">;
    peopleGraph += <ex:alice, ex:age, 30>;
    peopleGraph += <ex:alice, ex:knows, ex:bob>;
    
    return 0;
}

// Assigning graphs to stores
alias ex as <http://example.org/>;
myStore : store = sparql_store(<http://localhost:8080/graphdb>);

main(): int {
    myGraph : graph in <ex:> = KG.CreateGraph();
    myGraph += <ex:entity, ex:property, "value">;
    
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
// All constrained overloads require a base case
constrained(
    x: int | x > 0,
    y: int | y < 100
): int {
    return x + y;  // Handles when x>0 AND y<100
}

constrained(x: int, y: int): int {
    return 0;  // Base case: handles all other combinations
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
    
    // Create knowledge graph and add triples
    johnGraph : graph in <ex:> = KG.CreateGraph();
    // Add triples for John's properties
    johnGraph += <ex:john, foaf:firstName, "John">;
    johnGraph += <ex:john, foaf:lastName, "Doe">;
    johnGraph += <ex:john, foaf:age, 30>;
    
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
