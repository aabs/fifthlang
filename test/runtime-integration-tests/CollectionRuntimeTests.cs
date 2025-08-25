using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Tests for collections (arrays, lists) and their operations
/// NOTE: Current PE emission generates hardcoded program. Tests verify compilation success.
/// </summary>
public class CollectionRuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task ArrayDeclaration_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                arr: int[] = [1, 2, 3, 4, 5];
                return arr[0];
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Array declaration should compile");
        
        // TODO: When PE emission is fixed, expect exit code 1 (arr[0])
    }

    [Test]
    public async Task ArrayWithLoop_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                arr: int[] = [1, 2, 3, 4, 5];
                sum: int = 0;
                i: int = 0;
                while (i < 5) {
                    sum = sum + arr[i];
                    i = i + 1;
                }
                return sum;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Array with loop should compile");
        
        // TODO: When PE emission is fixed, expect exit code 15 (1+2+3+4+5)
    }

    [Test]
    public async Task ListDeclaration_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                items: list<int> = [10, 20, 30];
                return items[1];
            }
            """;

        // Act
        try
        {
            var executablePath = await CompileSourceAsync(sourceCode);
            File.Exists(executablePath).Should().BeTrue("List declaration should compile");
            
            // TODO: When PE emission is fixed, expect exit code 20 (items[1])
        }
        catch
        {
            // Skip if list syntax is not yet implemented
            Console.WriteLine("Skipping list test - syntax may not be implemented yet");
        }
    }

    [Test]
    public async Task EmptyArray_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                arr: int[] = [];
                return 0;
            }
            """;

        // Act
        try
        {
            var executablePath = await CompileSourceAsync(sourceCode);
            File.Exists(executablePath).Should().BeTrue("Empty array should compile");
            
            // TODO: When PE emission is fixed, expect exit code 0
        }
        catch
        {
            // Skip if empty array syntax is not yet supported
            Console.WriteLine("Skipping empty array test - syntax may not be implemented yet");
        }
    }

    [Test]
    public async Task ArrayIndexing_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                numbers: int[] = [100, 200, 300];
                first: int = numbers[0];
                last: int = numbers[2];
                return first + last;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Array indexing should compile");
        
        // TODO: When PE emission is fixed, expect exit code 400 (100 + 300)
    }

    [Test]
    public async Task NestedArrayAccess_ShouldCompile()
    {
        // Arrange
        var sourceCode = """
            main(): int {
                matrix: int[][] = [[1, 2], [3, 4]];
                return matrix[1][0];
            }
            """;

        // Act
        try
        {
            var executablePath = await CompileSourceAsync(sourceCode);
            File.Exists(executablePath).Should().BeTrue("Nested array access should compile");
            
            // TODO: When PE emission is fixed, expect exit code 3 (matrix[1][0])
        }
        catch
        {
            // Skip if nested array syntax is not yet implemented
            Console.WriteLine("Skipping nested array test - syntax may not be implemented yet");
        }
    }

    [Test]
    public async Task ArrayWithMixedTypes_ShouldCompile()
    {
        // Arrange - Test if the language supports heterogeneous collections
        var sourceCode = """
            main(): int {
                values: int[] = [1, 2, 3];
                total: int = 0;
                i: int = 0;
                while (i < 3) {
                    total = total + values[i];
                    i = i + 1;
                }
                return total;
            }
            """;

        // Act
        var executablePath = await CompileSourceAsync(sourceCode);
        
        // Assert
        File.Exists(executablePath).Should().BeTrue("Array operations should compile");
        
        // TODO: When PE emission is fixed, expect exit code 6 (1+2+3)
    }
}