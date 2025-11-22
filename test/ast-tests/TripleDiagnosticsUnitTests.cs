using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using ast;
using compiler.LanguageTransformations;

namespace ast_tests;

public class TripleDiagnosticsUnitTests
{
    [Fact]
    public void T001_TripleDiagnostics_EmptyListProduces_TRPL004()
    {
        var triple = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new Uri("http://example.org/p") },
            ObjectExp = new ListLiteral { ElementExpressions = new List<Expression>() }
        };
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(triple);
        diags.Should().Contain(d => d.Code == "TRPL004");
    }

    [Fact]
    public void T002_TripleExpansion_EmptyListProduces_TRPL004()
    {
        var triple = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new Uri("http://example.org/p") },
            ObjectExp = new ListLiteral { ElementExpressions = new List<Expression>() }
        };
        var diags = new List<compiler.Diagnostic>();
        var expander = new TripleExpansionVisitor(diags);

        var block = new BlockStatement { Statements = new List<Statement> { new ExpStatement { RHS = triple } } };
        var visitedBlock = expander.VisitBlockStatement(block);

        // After expansion, the exp statement's RHS should be a ListLiteral with zero elements
        var es = visitedBlock.Statements.OfType<ExpStatement>().First();
        es.RHS.Should().BeOfType<ListLiteral>();
        var list = (ListLiteral)es.RHS;
        list.ElementExpressions.Should().BeEmpty();

        diags.Should().Contain(d => d.Code == "TRPL004");
    }
}
