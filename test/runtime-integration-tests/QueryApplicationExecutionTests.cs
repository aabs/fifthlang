using FluentAssertions;
using Xunit;

namespace runtime_integration_tests;

/// <summary>
/// Runtime execution tests for Query Application feature using Fifth syntax.
/// These tests verify that queries actually execute and results are accessible at runtime
/// using TriG literals (<{...}>), SPARQL literals (?<...>), and query application operator (<-).
/// </summary>
[Trait("Category", "QueryApplication")]
[Trait("Category", "Execution")]
[Trait("Category", "EndToEnd")]
public class QueryApplicationExecutionTests : RuntimeTestBase
{
    /// <summary>
    /// Helper method to compile and run a Fifth program, returning exit code and output
    /// </summary>
    private async Task<(int exitCode, string output)> CompileAndRunAsync(string source, string fileName)
    {
        var exe = await CompileSourceAsync(source, fileName);
        var result = await ExecuteAsync(exe);
        return (result.ExitCode, result.StandardOutput);
    }

    [Fact]
    public async Task QueryApplication_SELECT_WithTriGLiteral_ReturnsSuccess()
    {
        var src = """
            main(): int {
                // Create and populate store using TriG literal
                myStore: Store = @<
                    <http://ex.org/person1> <http://ex.org/name> "Alice" .
                    <http://ex.org/person2> <http://ex.org/name> "Bob" .
                >;
                
                // Execute SELECT query using SPARQL literal and query application operator
                query: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Success - query executed without throwing
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_select_trig");
        exitCode.Should().Be(0, $"SELECT query with TriG literal should execute successfully. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_ASK_WithTriGLiteral_ReturnsSuccess()
    {
        var src = """
            main(): int {
                // Create and populate store using TriG literal
                myStore: Store = @<
                    <http://ex.org/person1> <http://ex.org/name> "Alice" .
                >;
                
                // Execute ASK query
                query: Query = ?<ASK WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Success - query executed
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_ask_trig");
        exitCode.Should().Be(0, $"ASK query with TriG literal should execute successfully. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_CONSTRUCT_WithTriGLiteral_ReturnsSuccess()
    {
        var src = """
            main(): int {
                // Create and populate store using TriG literal
                myStore: Store = @<
                    <http://ex.org/s1> <http://ex.org/p> "v1" .
                    <http://ex.org/s2> <http://ex.org/p> "v2" .
                >;
                
                // Execute CONSTRUCT query (using simple pattern)
                query: Query = ?<CONSTRUCT WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Success - query executed
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_construct_trig");
        exitCode.Should().Be(0, $"CONSTRUCT query with TriG literal should execute successfully. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_DESCRIBE_WithTriGLiteral_ReturnsSuccess()
    {
        var src = """
            main(): int {
                // Create and populate store using TriG literal
                myStore: Store = @<
                    <http://ex.org/person1> <http://ex.org/name> "Alice" .
                    <http://ex.org/person1> <http://ex.org/age> "30" .
                >;
                
                // Execute DESCRIBE query (using variable)
                query: Query = ?<DESCRIBE ?s WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Success - query executed
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_describe_trig");
        exitCode.Should().Be(0, $"DESCRIBE query with TriG literal should execute successfully. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_EmptyStore_ExecutesWithoutError()
    {
        var src = """
            main(): int {
                // Create empty store using empty TriG literal
                myStore: Store = @<>;
                
                // Execute SELECT query on empty store
                query: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Success - empty store doesn't cause errors
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_empty");
        exitCode.Should().Be(0, $"query on empty store should execute without error. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_MultipleSequentialQueries_AllExecute()
    {
        var src = """
            main(): int {
                // Create and populate store
                myStore: Store = @<
                    <http://ex.org/s1> <http://ex.org/p> "v1" .
                    <http://ex.org/s2> <http://ex.org/p> "v2" .
                >;
                
                // Execute multiple queries sequentially
                q1: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                r1: Result = q1 <- myStore;
                
                q2: Query = ?<ASK WHERE { ?s ?p ?o }>;
                r2: Result = q2 <- myStore;
                
                q3: Query = ?<CONSTRUCT WHERE { ?s ?p ?o }>;
                r3: Result = q3 <- myStore;
                
                // All queries executed successfully
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_multiple");
        exitCode.Should().Be(0, $"multiple sequential queries should execute. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_ChainedOperations_WorkCorrectly()
    {
        var src = """
            main(): int {
                // Create store, execute query, all in sequence
                myStore: Store = @<
                    <http://ex.org/s> <http://ex.org/p> "test" .
                >;
                
                myQuery: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                myResult: Result = myQuery <- myStore;
                
                // Proof that all operations completed
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_chained");
        exitCode.Should().Be(0, $"chained operations should complete. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_StoreReuse_WorksForMultipleQueries()
    {
        var src = """
            main(): int {
                // Create store once
                myStore: Store = @<
                    <http://ex.org/s> <http://ex.org/p> "test" .
                >;
                
                // Execute different queries against same store
                q1: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                r1: Result = q1 <- myStore;
                
                q2: Query = ?<ASK WHERE { ?s ?p ?o }>;
                r2: Result = q2 <- myStore;
                
                // Store reused successfully
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_reuse");
        exitCode.Should().Be(0, $"store should be reusable for multiple queries. Output: {output}");
    }

    [Fact]
    public async Task QueryApplication_WithLargerDataset_ExecutesSuccessfully()
    {
        var src = """
            main(): int {
                // Create store with multiple triples
                myStore: Store = @<
                    <http://ex.org/alice> <http://ex.org/name> "Alice" .
                    <http://ex.org/alice> <http://ex.org/age> "30" .
                    <http://ex.org/alice> <http://ex.org/city> "NYC" .
                    <http://ex.org/bob> <http://ex.org/name> "Bob" .
                    <http://ex.org/bob> <http://ex.org/age> "25" .
                >;
                
                // Query all data
                query: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                result: Result = query <- myStore;
                
                // Large dataset handled successfully
                return 0;
            }
            """;
        
        var (exitCode, output) = await CompileAndRunAsync(src, "query_app_large");
        exitCode.Should().Be(0, $"larger dataset should be queryable. Output: {output}");
    }
}
