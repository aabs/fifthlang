using FluentAssertions;
using TUnit.Core;

namespace runtime_integration_tests;

/// <summary>
/// Integration tests for Query Application feature (spec 011-query-application-result-type).
/// Tests runtime execution of query application operator (<-) with all result types.
/// </summary>
[Category("QueryApplication")]
public class QueryApplicationRuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task QueryApplication_SELECT_BasicQuery_ShouldCompile()
    {
        var src = """
            main(): int {
                // Create an in-memory store
                myStore: Store = Store.CreateInMemory();
                
                // Create a SELECT query
                myQuery: Query = ?<SELECT ?name WHERE { ?s ex:name ?name }>;
                
                // Apply query to store using <- operator
                result: Result = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_select_basic");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_CONSTRUCT_Query_ShouldCompile()
    {
        var src = """
            main(): int {
                // Create an in-memory store
                myStore: Store = Store.CreateInMemory();
                
                // Create a CONSTRUCT query
                myQuery: Query = ?<CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }>;
                
                // Apply query to store using <- operator
                result: Result = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_construct");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_ASK_Query_ShouldCompile()
    {
        var src = """
            main(): int {
                // Create an in-memory store
                myStore: Store = Store.CreateInMemory();
                
                // Create an ASK query
                myQuery: Query = ?<ASK WHERE { ?s ?p ?o }>;
                
                // Apply query to store using <- operator
                result: Result = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_ask");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_DESCRIBE_Query_ShouldCompile()
    {
        var src = """
            main(): int {
                // Create an in-memory store
                myStore: Store = Store.CreateInMemory();
                
                // Create a DESCRIBE query
                myQuery: Query = ?<DESCRIBE <http://example.org/resource>>;
                
                // Apply query to store using <- operator
                result: Result = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_describe");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_OperatorPrecedence_FunctionCallBeforeApplication_ShouldCompile()
    {
        var src = """
            main(): int {
                myStore: Store = Store.CreateInMemory();
                
                // Test that function calls have higher precedence than <-
                // This should parse as: (getQuery()) <- (getStore())
                result: Result = getQuery() <- getStore();
                
                return 0;
            }
            
            getQuery(): Query {
                return ?<SELECT * WHERE { ?s ?p ?o }>;
            }
            
            getStore(): Store {
                return Store.CreateInMemory();
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_precedence");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_WithVariables_ShouldCompile()
    {
        var src = """
            main(): int {
                // Create store
                myStore: Store = Store.CreateInMemory();
                
                // Variables from Fifth code
                personName: string = "Alice";
                
                // Query using SPARQL literal (SPARQL literal feature already implemented)
                myQuery: Query = ?<SELECT ?age WHERE { ?s ex:name "Alice" ; ex:age ?age }>;
                
                // Apply query
                result: Result = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_with_variables");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_ChainedQueries_ShouldCompile()
    {
        var src = """
            main(): int {
                // Initial store
                store1: Store = Store.CreateInMemory();
                
                // Query to construct a new graph
                constructQuery: Query = ?<CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }>;
                result1: Result = constructQuery <- store1;
                
                // Query the initial store again
                selectQuery: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                result2: Result = selectQuery <- store1;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_chained");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_MultipleApplicationsInSequence_ShouldCompile()
    {
        var src = """
            main(): int {
                myStore: Store = Store.CreateInMemory();
                
                // Multiple query applications
                query1: Query = ?<SELECT ?s WHERE { ?s ?p ?o }>;
                result1: Result = query1 <- myStore;
                
                query2: Query = ?<ASK WHERE { ?s ?p ?o }>;
                result2: Result = query2 <- myStore;
                
                query3: Query = ?<CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }>;
                result3: Result = query3 <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_multiple");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_InFunctionCall_ShouldCompile()
    {
        var src = """
            main(): int {
                myStore: Store = Store.CreateInMemory();
                myQuery: Query = ?<SELECT * WHERE { ?s ?p ?o }>;
                
                // Pass result to function
                processResult(myQuery <- myStore);
                
                return 0;
            }
            
            processResult(r: Result): int {
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_in_function");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }

    [Test]
    public async Task QueryApplication_AssignToVariable_ShouldCompile()
    {
        var src = """
            main(): int {
                myStore: Store = Store.CreateInMemory();
                myQuery: Query = ?<SELECT ?name WHERE { ?s ex:name ?name }>;
                
                // Assign result to variable with explicit type
                result1: Result = myQuery <- myStore;
                
                // Assign result to variable with type inference
                result2 = myQuery <- myStore;
                
                return 0;
            }
            """;
        
        var exe = await CompileSourceAsync(src, "query_application_assign");
        File.Exists(exe).Should().BeTrue("source should compile successfully");
    }
}

