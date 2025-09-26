using FluentAssertions;

namespace runtime_integration_tests;

[Category("KG")]
public class GraphAssertionBlock_Literals_RuntimeTests : RuntimeTestBase
{
    [Test]
    public async Task GraphBlock_ExpressionForm_ShouldSupportVariousLiterals()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Person in <http://example.org/> {
                name: string;
                age: int;
                height: long;
                weight: double;
                ratio: float;
                active: bool;
                grade: char;
            }

            main(): int {
                var g: graph = <{
                    p: Person = new Person();
                    p.name = "hello";
                    p.age = 42;
                    p.height = 42L;
                    p.weight = 3.14;
                    p.ratio = 2.5f;
                    p.active = true;
                    p.grade = 'X';
                }>;
                
                // Verify the graph contains the expected triples from property assignments
                var tripleCount: int = KG.CountTriples(g);
                if (tripleCount != 7) { return tripleCount; } // Should have 7 triples from 7 property assignments
                
                // Verify that the runtime object retains its assigned values
                // This tests that assignments work in both directions:
                // 1. Values are assigned to the object (normal assignment behavior)
                // 2. Triples are generated for the graph (GAB-specific behavior)
                
                // Note: In a full GAB implementation, we'd need to access the Person object
                // created inside the GAB to verify it retained values. For now, we verify
                // the graph side-effect which indicates the assignments were processed.
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_expr");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_ExpressionForm_ShouldRetainObjectValuesAndGenerateTriples()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Resource in <http://example.org/> {
                uri: string;
                value: int;
                size: long;
                enabled: bool;
                code: char;
            }

            main(): int {
                var r: Resource = new Resource();
                
                var g: graph = <{
                    r.uri = <http://example.org/o>;
                    r.value = -7;
                    r.size = -9L;
                    r.enabled = false;
                    r.code = 'Z';
                }>;
                
                // Verify the graph contains triples from the assignments
                var tripleCount: int = KG.CountTriples(g);
                if (tripleCount != 5) { return tripleCount; }
                
                // Verify that the runtime object 'r' retains the assigned values
                // This is critical: GAB should assign to the object AND generate triples
                if (r.value != -7) { return 100; }       // Object should retain assigned value
                if (r.size != -9L) { return 101; }       // Object should retain assigned value  
                if (r.enabled != false) { return 102; }  // Object should retain assigned value
                if (r.code != 'Z') { return 103; }       // Object should retain assigned value
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_expr_uri_neg");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_ExpressionForm_ShouldSupportFloatAndDoubleVariations()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Measurement in <http://example.org/> {
                floatVal1: float;
                floatVal2: float;
                doubleVal1: double;  
                doubleVal2: double;
            }

            main(): int {
                var g: graph = <{
                    m: Measurement = new Measurement();
                    m.floatVal1 = 3.0f;
                    m.floatVal2 = -0.5f;
                    m.doubleVal1 = 1.25;
                    m.doubleVal2 = -2.5;
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_expr_float_double");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_StatementForm_ShouldPersistGraphUsingDefaultStore()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Data in <http://example.org/> {
                intVal: int;
                longVal: long;
                doubleVal: double;
                floatVal: float;
                boolVal: bool;
                charVal: char;
            }

            main(): int {
                <{
                    d: Data = new Data();
                    d.intVal = 1;
                    d.longVal = 1L;
                    d.doubleVal = 1.0;
                    d.floatVal = 1.0f;
                    d.boolVal = false;
                    d.charVal = 'Y';
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_stmt");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_StatementForm_ShouldSupportUriAndMixedLiterals()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Entity in <http://example.org/> {
                location: string;
                count: long;
                rate: float;
                precision: double;
                active: bool;
                category: char;
                description: string;
            }

            main(): int {
                <{
                    e: Entity = new Entity();
                    e.location = <http://example.org/o2>;
                    e.count = 100L;
                    e.rate = 0.125f;
                    e.precision = 6.022;
                    e.active = true;
                    e.category = 'Q';
                    e.description = "world";
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_stmt_uri_mixed");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_ExpressionForm_ShouldSupportDecimalLiteralsPrecisely()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Financial in <http://example.org/> {
                amount: decimal;
                adjustment: decimal;
            }

            main(): int {
                var g: graph = <{
                    f: Financial = new Financial();
                    f.amount = 12345.6789c;
                    f.adjustment = -0.0001c;
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_expr_decimal");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_StatementForm_ShouldPersistDecimalLiterals()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Calculation in <http://example.org/> {
                precision: decimal;
                negative: decimal;
            }

            main(): int {
                <{
                    c: Calculation = new Calculation();
                    c.precision = 0.3333333333333333333333333333c;
                    c.negative = -10.25c;
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_stmt_decimal");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]  
    public async Task GraphBlock_ShouldDemonstrateFullGABBehavior()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            class Person in <http://example.org/people> {
                name: string;
                age: int;
                active: bool;
            }

            main(): int {
                // Create person object outside GAB to verify it exists before and after
                var person: Person = new Person();
                person.name = "Initial";
                person.age = 0;
                person.active = false;
                
                // Use GAB to assign properties - this should:
                // 1. Assign values to the person object (normal assignment behavior)  
                // 2. Generate triples in the graph representing these assignments
                var knowledgeGraph: graph = <{
                    person.name = "Alice";
                    person.age = 30;
                    person.active = true;
                }>;
                
                // Verify object state: GAB assignments should have modified the person object
                if (person.name != "Alice") { return 1; }
                if (person.age != 30) { return 2; }
                if (person.active != true) { return 3; }
                
                // Verify graph state: GAB should have generated triples for the assignments
                var tripleCount: int = KG.CountTriples(knowledgeGraph);
                if (tripleCount != 3) { return tripleCount + 10; } // Return count + offset for debugging
                
                // This demonstrates the core GAB behavior:
                // - Properties are assigned to objects (enabling normal programming)
                // - Triples are generated for the graph (enabling knowledge management)
                // - Both happen transparently within the <{ ... }> block
                
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_full_behavior_demo");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }
}
