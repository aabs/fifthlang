// T035: Comprehensive tests asserting all triple diagnostics appear with correct codes/messages
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using ast;
using compiler.LanguageTransformations;

namespace ast_tests;

/// <summary>
/// Comprehensive coverage tests for all TRPL diagnostic codes.
/// Tests verify correct diagnostic code and message for each error/warning scenario.
/// </summary>
public class TripleDiagnosticsCoverageTests
{
    [Fact]
    public void TRPL001_MissingObject_EmitsDiagnostic()
    {
        // Malformed triple with missing object component
        var malformed = new MalformedTripleExp
        {
            MalformedKind = "MissingObject",
            Components = new List<Expression>
            {
                new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/p") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(malformed);
        
        diags.Should().Contain(d => d.Code == "TRPL001");
        diags.First(d => d.Code == "TRPL001").Message.Should().Contain("3 components");
    }

    [Fact]
    public void TRPL001_TrailingComma_EmitsDiagnostic()
    {
        // Malformed triple with trailing comma
        var malformed = new MalformedTripleExp
        {
            MalformedKind = "TrailingComma",
            Components = new List<Expression>
            {
                new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/o") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(malformed);
        
        diags.Should().Contain(d => d.Code == "TRPL001");
        diags.First(d => d.Code == "TRPL001").Message.Should().Contain("Trailing comma");
    }

    [Fact]
    public void TRPL001_TooManyComponents_EmitsDiagnostic()
    {
        // Malformed triple with too many components
        var malformed = new MalformedTripleExp
        {
            MalformedKind = "TooManyComponents",
            Components = new List<Expression>
            {
                new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/o") },
                new UriLiteralExp { Value = new System.Uri("http://example.org/x") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(malformed);
        
        diags.Should().Contain(d => d.Code == "TRPL001");
        diags.First(d => d.Code == "TRPL001").Message.Should().Contain("too many");
    }

    [Fact]
    public void TRPL004_EmptyList_EmitsWarning()
    {
        // Triple with empty list in object position
        var triple = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
            ObjectExp = new ListLiteral { ElementExpressions = new List<Expression>() }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(triple);
        
        diags.Should().Contain(d => d.Code == "TRPL004");
        var diagnostic = diags.First(d => d.Code == "TRPL004");
        diagnostic.Level.Should().Be(compiler.DiagnosticLevel.Warning);
        diagnostic.Message.Should().Contain("Empty list");
    }

    [Fact]
    public void TRPL006_NestedList_EmitsError()
    {
        // Triple with nested list in object position
        var triple = new TripleLiteralExp
        {
            SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
            PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
            ObjectExp = new ListLiteral 
            { 
                ElementExpressions = new List<Expression>
                {
                    new ListLiteral { ElementExpressions = new List<Expression>() }
                }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.Visit(triple);
        
        diags.Should().Contain(d => d.Code == "TRPL006");
        var diagnostic = diags.First(d => d.Code == "TRPL006");
        diagnostic.Level.Should().Be(compiler.DiagnosticLevel.Error);
        diagnostic.Message.Should().Contain("Nested lists not allowed");
    }

    [Fact]
    public void TRPL012_TripleMinusOperator_EmitsError()
    {
        // Triple on left of minus operator (not supported)
        var binaryExp = new BinaryExp
        {
            Operator = Operator.ArithmeticSubtract,
            LHS = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o") }
            },
            RHS = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s2") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p2") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o2") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.VisitBinaryExp(binaryExp);
        
        diags.Should().Contain(d => d.Code == "TRPL012");
        diags.First(d => d.Code == "TRPL012").Message.Should().Contain("left of '-'");
    }

    [Fact]
    public void TRPL013_UnsupportedOperator_EmitsError()
    {
        // Triple with unsupported operator (e.g., multiplication)
        var binaryExp = new BinaryExp
        {
            Operator = Operator.ArithmeticMultiply,
            LHS = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o") }
            },
            RHS = new Int32LiteralExp { Value = 2 }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.VisitBinaryExp(binaryExp);
        
        diags.Should().Contain(d => d.Code == "TRPL013");
        diags.First(d => d.Code == "TRPL013").Message.Should().Contain("Unsupported operator");
    }

    [Fact]
    public void TRPL013_LogicalNotOnTriple_EmitsError()
    {
        // Logical NOT on triple (not supported)
        var unaryExp = new UnaryExp
        {
            Operator = Operator.LogicalNot,
            Operand = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.VisitUnaryExp(unaryExp);
        
        diags.Should().Contain(d => d.Code == "TRPL013");
        diags.First(d => d.Code == "TRPL013").Message.Should().Contain("Unsupported operator");
    }

    [Fact]
    public void SupportedOperators_NoErrors()
    {
        // Triple + Triple should not emit any errors (supported operator)
        var binaryExp = new BinaryExp
        {
            Operator = Operator.ArithmeticAdd,
            LHS = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o") }
            },
            RHS = new TripleLiteralExp
            {
                SubjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/s2") },
                PredicateExp = new UriLiteralExp { Value = new System.Uri("http://example.org/p2") },
                ObjectExp = new UriLiteralExp { Value = new System.Uri("http://example.org/o2") }
            }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TripleDiagnosticsVisitor(diags);
        visitor.VisitBinaryExp(binaryExp);
        
        // No errors should be emitted for supported operators
        diags.Should().BeEmpty();
    }
}
