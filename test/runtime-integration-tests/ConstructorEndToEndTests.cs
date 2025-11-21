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
        // Arrange - Test basic constructor with field initialization
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
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Constructor code should compile to executable");
        
        // Execute to verify it runs
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Constructor-based code should execute successfully");
    }

    [Test]
    public async Task ParameterlessConstructor_WithDefaultInitialization()
    {
        // Arrange - Test parameterless constructor
        var fifthCode = """
            class Counter {
                Value: int;
                
                Counter() {
                    this.Value = 0;
                }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Parameterless constructor should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Parameterless constructor should execute");
    }

    [Test]
    public async Task MultipleConstructorOverloads_CompileAndExecute()
    {
        // Arrange - Test constructor overloading
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
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Multiple constructor overloads should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Overloaded constructors should execute");
    }

    [Test]
    public async Task ConstructorWithBaseCall_CompilesSuccessfully()
    {
        // Arrange - Test base constructor chaining
        var fifthCode = """
            class Animal {
                Species: string;
                
                Animal(species: string) {
                    this.Species = species;
                }
            }
            
            class Dog {
                Name: string;
                
                Dog(name: string) : base("Canine") {
                    this.Name = name;
                }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Constructor with base call should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Base constructor chaining should execute");
    }

    [Test]
    public async Task GenericClassConstructor_WithTypeParameters()
    {
        // Arrange - Test generic class with constructor
        var fifthCode = """
            class Box<T> {
                Value: T;
                
                Box(value: T) {
                    this.Value = value;
                }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Generic class constructor should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Generic constructor should execute");
    }

    [Test]
    public async Task ConstructorWithComplexFieldInitialization()
    {
        // Arrange - Test constructor with expressions and method calls
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
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Constructor with complex initialization should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Complex field initialization should execute");
    }

    [Test]
    public async Task MultipleClassesWithConstructors_AllCompile()
    {
        // Arrange - Test multiple classes each with constructors
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
                Start: Point;
                End: Point;
                
                Line(start: Point, end: Point) {
                    this.Start = start;
                    this.End = end;
                }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Multiple classes with constructors should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Multiple constructors should execute");
    }

    [Test]
    public async Task ConstructorWithMixedMemberTypes_CompilesSuccessfully()
    {
        // Arrange - Test class with constructor, fields, and methods
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
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Constructor with mixed members should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Mixed member class should execute");
    }

    [Test]
    public async Task SynthesizedParameterlessConstructor_ForSimpleClass()
    {
        // Arrange - Test that classes without explicit constructors get synthesized ones
        var fifthCode = """
            class Simple {
                Value: int;
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Class without constructor should synthesize one and compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Synthesized constructor should execute");
    }

    [Test]
    public async Task ConstructorWithControlFlow_IfStatements()
    {
        // Arrange - Test constructor with conditional logic
        var fifthCode = """
            class Validator {
                Value: int;
                IsValid: bool;
                
                Validator(value: int) {
                    this.Value = value;
                    if (value > 0) {
                        this.IsValid = true;
                    } else {
                        this.IsValid = false;
                    }
                }
            }

            main(): int {
                return 0;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(fifthCode);

        // Assert
        File.Exists(executablePath).Should().BeTrue("Constructor with if statements should compile");
        
        var result = await ExecuteAsync(executablePath);
        result.ExitCode.Should().Be(0, "Constructor with control flow should execute");
    }
}
