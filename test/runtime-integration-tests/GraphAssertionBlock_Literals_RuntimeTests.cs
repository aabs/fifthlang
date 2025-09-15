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

            main(): int {
                var g: graph = <{
                    (:s, :p, "hello");
                    (:s, :p2, 42);
                    (:s, :p3, 42L);
                    (:s, :p4, 3.14);
                    (:s, :p5, 2.5f);
                    (:s, :p6, true);
                    (:s, :p7, 'X');
                }>;
                // Expect 7 triples asserted
                var n: int = KG.CountTriples(g);
                if (n != 7) { return n; }
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_expr");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }

    [Test]
    public async Task GraphBlock_ExpressionForm_ShouldSupportUriAndNegativeLiterals()
    {
        var src = """
            store default = sparql_store(<http://example.org/store>);

            main(): int {
                var g: graph = <{
                    (:s, :p, <http://example.org/o>);
                    (:s, :p2, -7);
                    (:s, :p3, -9L);
                    (:s, :p4, false);
                    (:s, :p5, 'Z');
                }>;
                var n: int = KG.CountTriples(g);
                if (n != 5) { return n; }
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

            main(): int {
                var g: graph = <{
                    (:s, :pf1, 3.0f);
                    (:s, :pf2, -0.5f);
                    (:s, :pd1, 1.25);
                    (:s, :pd2, -2.5);
                }>;
                var n: int = KG.CountTriples(g);
                if (n != 4) { return n; }
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

            main(): int {
                <{
                    (:s, :p, 1);
                    (:s, :p2, 1L);
                    (:s, :p3, 1.0);
                    (:s, :p4, 1.0f);
                    (:s, :p5, false);
                    (:s, :p6, 'Y');
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

            main(): int {
                <{
                    (:s, :p, <http://example.org/o2>);
                    (:s, :p2, 100L);
                    (:s, :p3, 0.125f);
                    (:s, :p4, 6.022);
                    (:s, :p5, true);
                    (:s, :p6, 'Q');
                    (:s, :p7, "world");
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

            main(): int {
                var g: graph = <{
                    (:s, :pd, 12345.6789c);
                    (:s, :nd, -0.0001c);
                }>;
                var n: int = KG.CountTriples(g);
                if (n != 2) { return n; }
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

            main(): int {
                <{
                    (:s, :pd, 0.3333333333333333333333333333c);
                    (:s, :nd, -10.25c);
                }>;
                return 0;
            }
            """;

        var exe = await CompileSourceAsync(src, "gab_literals_stmt_decimal");
        File.Exists(exe).Should().BeTrue();
        var result = await ExecuteAsync(exe);
        result.ExitCode.Should().Be(0);
    }
}
