using ast;
using ast_model.TypeSystem;
using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// End-to-end runtime tests for constructor functionality demonstrating actual working features.
/// These tests prove that constructors compile and execute correctly, not just parse.
/// </summary>
public class ConstructorEndToEndTests : RuntimeTestBase
{
    [Test]
    public async Task BasicConstructor_InstantiatesObject_WithFieldsInitialized()
    {
        // Arrange - Test that constructor actually initializes fields and object can be used
        var fifthCode = """
            class Person {
                Name: string;
                Age: int;
                
                Person(name: string, age: int) {
                    this.Name = name;
                    this.Age = age;
                }
            }

            main(): int {
                p: Person = new Person("Alice", 30);
                return p.Age;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Exit code should be 30 (the age we set)
        result.ExitCode.Should().Be(30, "Constructor should initialize Age field to 30");
    }

    [Test]
    public async Task ParameterlessConstructor_WithDefaultInitialization()
    {
        // Arrange - Test that parameterless constructor initializes field and object can be used
        var fifthCode = """
            class Counter {
                Value: int;
                
                Counter() {
                    this.Value = 42;
                }
            }

            main(): int {
                c: Counter = new Counter();
                return c.Value;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Exit code should be 42 (the value set by constructor)
        result.ExitCode.Should().Be(42, "Parameterless constructor should initialize Value field to 42");
    }

    [Test]
    public async Task MultipleConstructorOverloads_SelectCorrectOverload()
    {
        // Arrange - Test that correct overload is called based on argument count
        var fifthCode = """
            class Rectangle {
                Width: int;
                Height: int;
                
                Rectangle(size: int) {
                    this.Width = size;
                    this.Height = size;
                }
                
                Rectangle(width: int, height: int) {
                    this.Width = width;
                    this.Height = height;
                }
            }

            main(): int {
                r1: Rectangle = new Rectangle(5);
                r2: Rectangle = new Rectangle(3, 7);
                return (r1.Width * r1.Height) + (r2.Width * r2.Height);
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - (5*5) + (3*7) = 25 + 21 = 46
        result.ExitCode.Should().Be(46, "Should call correct overload: square(5*5=25) + rect(3*7=21) = 46");
    }

    [Test]
    public async Task ConstructorWithBaseCall_InitializesBaseFields()
    {
        // Arrange - Test that base constructor actually initializes base class fields
        var fifthCode = """
            class Animal {
                Legs: int;
                
                Animal(legs: int) {
                    this.Legs = legs;
                }
            }
            
            class Dog {
                Name: string;
                
                Dog(name: string) : base(4) {
                    this.Name = name;
                }
            }

            main(): int {
                d: Dog = new Dog("Buddy");
                return d.Legs;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Should return 4 (legs set by base constructor)
        result.ExitCode.Should().Be(4, "Base constructor should initialize Legs field to 4");
    }

    [Test]
    public async Task GenericClassConstructor_StoresTypedValue()
    {
        // Arrange - Test that generic constructor works with type parameter
        var fifthCode = """
            class Box<T> {
                Value: T;
                
                Box(value: T) {
                    this.Value = value;
                }
            }

            main(): int {
                b: Box<int> = new Box<int>(99);
                return b.Value;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Should return 99 (the value stored in generic box)
        result.ExitCode.Should().Be(99, "Generic constructor should store and retrieve value 99");
    }

    [Test]
    public async Task ConstructorWithComplexFieldInitialization_ComputesValues()
    {
        // Arrange - Test that constructor can perform computations during initialization
        var fifthCode = """
            class Calculator {
                Initial: int;
                Doubled: int;
                
                Calculator(value: int) {
                    this.Initial = value;
                    this.Doubled = value + value;
                }
            }

            main(): int {
                c: Calculator = new Calculator(7);
                return c.Initial + c.Doubled;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Initial=7, Doubled=14, sum=21
        result.ExitCode.Should().Be(21, "Constructor should compute: initial(7) + doubled(14) = 21");
    }

    [Test]
    public async Task MultipleClassesWithConstructors_WorkTogether()
    {
        // Arrange - Test that objects from multiple classes with constructors interact correctly
        var fifthCode = """
            class Point {
                X: int;
                Y: int;
                
                Point(x: int, y: int) {
                    this.X = x;
                    this.Y = y;
                }
            }
            
            class Line {
                StartX: int;
                EndX: int;
                
                Line(start: Point, end: Point) {
                    this.StartX = start.X;
                    this.EndX = end.X;
                }
            }

            main(): int {
                p1: Point = new Point(3, 5);
                p2: Point = new Point(8, 10);
                line: Line = new Line(p1, p2);
                return line.StartX + line.EndX;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - StartX=3, EndX=8, sum=11
        result.ExitCode.Should().Be(11, "Multiple constructors should work together: 3 + 8 = 11");
    }

    [Test]
    public async Task ConstructorWithMixedMemberTypes_MethodsUseConstructorInitializedFields()
    {
        // Arrange - Test that methods can use fields initialized by constructor
        var fifthCode = """
            class Employee {
                Name: string;
                Salary: int;
                
                Employee(name: string, salary: int) {
                    this.Name = name;
                    this.Salary = salary;
                }
                
                GetBonus(): int {
                    return this.Salary / 10;
                }
            }

            main(): int {
                e: Employee = new Employee("Alice", 50000);
                return e.GetBonus();
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Salary=50000, bonus=5000
        result.ExitCode.Should().Be(5000, "Method should use constructor-initialized field: 50000/10 = 5000");
    }

    [Test]
    public async Task SynthesizedParameterlessConstructor_CreatesUsableObject()
    {
        // Arrange - Test that synthesized constructor actually creates usable objects
        var fifthCode = """
            class Simple {
            }

            main(): int {
                s: Simple = new Simple();
                return 123;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - Should successfully create object with synthesized constructor
        result.ExitCode.Should().Be(123, "Synthesized constructor should create usable object");
    }

    [Test]
    public async Task ConstructorWithControlFlow_ConditionalInitialization()
    {
        // Arrange - Test that constructor control flow actually executes correctly
        var fifthCode = """
            class Validator {
                Value: int;
                Status: int;
                
                Validator(value: int) {
                    this.Value = value;
                    if (value > 10) {
                        this.Status = 1;
                    } else {
                        this.Status = 0;
                    }
                }
            }

            main(): int {
                v1: Validator = new Validator(5);
                v2: Validator = new Validator(15);
                return (v1.Status * 100) + v2.Status;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);
        var result = await ExecuteAsync(executablePath);

        // Assert - v1.Status=0 (5<=10), v2.Status=1 (15>10), result = 0*100 + 1 = 1
        result.ExitCode.Should().Be(1, "Control flow in constructor should execute: (0*100) + 1 = 1");
    }
}
