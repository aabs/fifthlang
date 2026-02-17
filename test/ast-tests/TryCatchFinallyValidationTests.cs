using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using ast;
using ast_model.TypeSystem;
using compiler.LanguageTransformations;

namespace ast_tests;

/// <summary>
/// Tests for try/catch/finally semantic validation (Phase 4)
/// </summary>
public class TryCatchFinallyValidationTests
{
    [Fact]
    public void CatchNonExceptionType_Error()
    {
        // T023: Catch type must derive from System.Exception
        var catchClause = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("int") }, // Invalid: int is not an exception
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { catchClause }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().Contain(d => d.Code == "TRY001", "catch type must be an exception type");
    }
    
    [Fact]
    public void CatchValidExceptionType_NoError()
    {
        // T023: System.Exception is valid
        var catchClause = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.Exception") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { catchClause }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().NotContain(d => d.Code == "TRY001", "System.Exception is a valid exception type");
    }
    
    [Fact]
    public void FilterMustBeBoolean_Error()
    {
        // T024: Filter expression must be boolean-convertible
        var filterExpr = new Int32LiteralExp { Value = 123 };
        filterExpr = filterExpr with { Type = new FifthType.TType { Name = TypeName.From("int") } };
        
        var catchClause = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.Exception") },
            Filter = filterExpr,
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { catchClause }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().Contain(d => d.Code == "TRY002", "filter expression must be boolean-convertible");
    }
    
    [Fact]
    public void FilterBooleanType_NoError()
    {
        // T024: Boolean filter is valid
        var filterExpr = new BooleanLiteralExp { Value = true };
        filterExpr = filterExpr with { Type = new FifthType.TType { Name = TypeName.From("bool") } };
        
        var catchClause = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.Exception") },
            Filter = filterExpr,
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { catchClause }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().NotContain(d => d.Code == "TRY002", "boolean filter is valid");
    }
    
    [Fact]
    public void UnreachableCatch_CatchAll_Error()
    {
        // T025: Catch-all makes later catches unreachable
        var catchAll = new CatchClause
        {
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var specificCatch = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.IOException") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { catchAll, specificCatch }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().Contain(d => d.Code == "TRY003", "catch-all makes later catches unreachable");
    }
    
    [Fact]
    public void UnreachableCatch_BroaderType_Error()
    {
        // T025: Earlier broader type makes later catch unreachable
        var broadCatch = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.Exception") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var specificCatch = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.IOException") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { broadCatch, specificCatch }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().Contain(d => d.Code == "TRY003", "System.Exception is broader than System.IOException");
    }
    
    [Fact]
    public void ReachableCatch_SpecificToGeneral_NoError()
    {
        // T025: Specific before general is allowed
        var specificCatch = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.IOException") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var broadCatch = new CatchClause
        {
            ExceptionType = new FifthType.TType { Name = TypeName.From("System.Exception") },
            Body = new BlockStatement { Statements = new List<Statement>() }
        };
        
        var tryStmt = new TryStatement
        {
            TryBlock = new BlockStatement { Statements = new List<Statement>() },
            CatchClauses = new List<CatchClause> { specificCatch, broadCatch }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitTryStatement(tryStmt);
        
        diags.Should().NotContain(d => d.Code == "TRY003", "specific to general ordering is valid");
    }
    
    [Fact]
    public void ThrowExpression_OperandType_Error()
    {
        // T026: Throw expression operand must be exception type
        var throwExpr = new ThrowExp
        {
            Exception = new Int32LiteralExp { Value = 42 }
        };
        throwExpr = throwExpr with { 
            Exception = throwExpr.Exception with { Type = new FifthType.TType { Name = TypeName.From("int") } }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitThrowExp(throwExpr);
        
        diags.Should().Contain(d => d.Code == "TRY004", "throw expression operand must be exception type");
    }
    
    [Fact]
    public void ThrowStatement_OperandType_Error()
    {
        // T026: Throw statement operand must be exception type
        var throwStmt = new ThrowStatement
        {
            Exception = new Int32LiteralExp { Value = 42 }
        };
        throwStmt = throwStmt with {
            Exception = throwStmt.Exception with { Type = new FifthType.TType { Name = TypeName.From("int") } }
        };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitThrowStatement(throwStmt);
        
        diags.Should().Contain(d => d.Code == "TRY004", "throw statement operand must be exception type");
    }
    
    [Fact]
    public void ThrowStatement_ValidException_NoError()
    {
        // T026: Valid exception type should not produce error
        var newExpr = new ObjectInitializerExp
        {
            TypeToInitialize = new FifthType.TType { Name = TypeName.From("System.Exception") },
            PropertyInitialisers = new List<PropertyInitializerExp>()
        };
        newExpr = newExpr with { Type = new FifthType.TType { Name = TypeName.From("System.Exception") } };
        
        var throwStmt = new ThrowStatement { Exception = newExpr };
        
        var diags = new List<compiler.Diagnostic>();
        var visitor = new TryCatchFinallyValidationVisitor(diags);
        visitor.VisitThrowStatement(throwStmt);
        
        diags.Should().NotContain(d => d.Code == "TRY004", "System.Exception is a valid exception type");
    }
}
