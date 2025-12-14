using System.Collections.Generic;
using System.Linq;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowering pass for list comprehensions.
/// 
/// This is a placeholder implementation that demonstrates the lowering strategy.
/// Full implementation would transform:
///   [projection from varname in source where constraint1, constraint2]
/// 
/// Into imperative code:
///   {
///     temp_result = []
///     temp_source = source
///     for each varname in temp_source:
///       if constraint1 && constraint2:
///         temp_result.append(projection)
///     result = temp_result
///   }
/// 
/// Current status: Returns comprehension unchanged (placeholder).
/// Future work: Implement full lowering with proper type propagation,
/// list append operations, and constraint evaluation.
/// 
/// This requires:
/// - Proper handling of Fifth's list type system
/// - Integration with append/list operations in Fifth.System
/// - Type inference for intermediate results
/// - Location tracking for all generated nodes
/// </summary>
public class ListComprehensionLoweringRewriter : DefaultAstRewriter
{
    private static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };
    private int _tempCounter = 0;

    /// <summary>
    /// Generate a fresh temporary variable name
    /// </summary>
    private string FreshTempName(string prefix = "tmp") => $"__{prefix}_comprehension_{_tempCounter++}";

    public override RewriteResult VisitListComprehension(ListComprehension ctx)
    {
        // Full lowering implementation: Transform comprehensions to imperative code
        // 
        // Transform:
        //   [projection from varname in source where constraint1, constraint2]
        // 
        // Into:
        //   {
        //     temp_result = []
        //     temp_source = source
        //     foreach varname in temp_source:
        //       if constraint1 && constraint2:
        //         temp_append_result = projection
        //         // Note: actual list append would require Fifth.System integration
        //     result = temp_result
        //   }
        
        var prologue = new List<Statement>();
        
        // Step 1: Rewrite source expression and collect its prologue
        var sourceResult = Rewrite(ctx.Source);
        prologue.AddRange(sourceResult.Prologue);
        var sourceExpr = (Expression)sourceResult.Node;
        
        // Step 2: Create temporary variable for source (to evaluate once)
        var sourceTempName = FreshTempName("source");
        var sourceTempDecl = new VariableDecl
        {
            Name = sourceTempName,
            TypeName = sourceExpr.Type?.Name ?? TypeName.From("object"),
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance
        };
        var sourceTempDeclStmt = new VarDeclStatement
        {
            VariableDecl = sourceTempDecl,
            InitialValue = sourceExpr
        };
        prologue.Add(sourceTempDeclStmt);
        
        // Step 3: Create temporary variable for result list (initially empty)
        var resultTempName = FreshTempName("result");
        var elementType = ctx.Type ?? new FifthType.TType() { Name = TypeName.From("object") };
        var resultTempDecl = new VariableDecl
        {
            Name = resultTempName,
            TypeName = ctx.Type?.Name ?? TypeName.From("list"),
            Visibility = Visibility.Private,
            CollectionType = CollectionType.List
        };
        var emptyList = new ListLiteral
        {
            ElementExpressions = new List<Expression>(),
            Type = elementType
        };
        var resultTempDeclStmt = new VarDeclStatement
        {
            VariableDecl = resultTempDecl,
            InitialValue = emptyList
        };
        prologue.Add(resultTempDeclStmt);
        
        // Step 4: Rewrite projection expression
        var projectionResult = Rewrite(ctx.Projection);
        var projectionExpr = (Expression)projectionResult.Node;
        
        // Step 5: Build constraint expression (AND all constraints together)
        Expression? combinedConstraint = null;
        if (ctx.Constraints != null && ctx.Constraints.Count > 0)
        {
            foreach (var constraint in ctx.Constraints)
            {
                var constraintResult = Rewrite(constraint);
                var constraintExpr = (Expression)constraintResult.Node;
                
                if (combinedConstraint == null)
                {
                    combinedConstraint = constraintExpr;
                }
                else
                {
                    combinedConstraint = new BinaryExp
                    {
                        Operator = Operator.LogicalAnd,
                        LHS = combinedConstraint,
                        RHS = constraintExpr,
                        Type = new FifthType.TType() { Name = TypeName.From("bool") }
                    };
                }
            }
        }
        
        // Step 6: Create statement to store projection result
        // Note: Actual list append would require Fifth.System list operations
        // For now, we evaluate and store the projection to demonstrate the pattern
        var projTempName = FreshTempName("proj");
        var projTempDecl = new VariableDecl
        {
            Name = projTempName,
            TypeName = projectionExpr.Type?.Name ?? TypeName.From("object"),
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance
        };
        var projTempDeclStmt = new VarDeclStatement
        {
            VariableDecl = projTempDecl,
            InitialValue = projectionExpr
        };
        
        // Step 7: Build loop body (with optional constraint check)
        var loopBodyStatements = new List<Statement>();
        if (projectionResult.Prologue.Count > 0)
        {
            loopBodyStatements.AddRange(projectionResult.Prologue);
        }
        
        if (combinedConstraint != null)
        {
            // Wrap projection evaluation in if statement
            var ifStmt = new IfElseStatement
            {
                Condition = combinedConstraint,
                ThenBlock = new BlockStatement
                {
                    Statements = new List<Statement> { projTempDeclStmt }
                },
                ElseBlock = new BlockStatement
                {
                    Statements = new List<Statement>()
                }
            };
            loopBodyStatements.Add(ifStmt);
        }
        else
        {
            // No constraint, just evaluate projection
            loopBodyStatements.Add(projTempDeclStmt);
        }
        
        // Step 8: Create foreach loop
        var loopVarDecl = new VariableDecl
        {
            Name = ctx.VarName,
            TypeName = TypeName.From("object"), // Type inference would determine this
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance
        };
        var foreachStmt = new ForeachStatement
        {
            Collection = new VarRefExp
            {
                VarName = sourceTempName,
                Type = sourceExpr.Type
            },
            LoopVariable = loopVarDecl,
            Body = new BlockStatement
            {
                Statements = loopBodyStatements
            }
        };
        prologue.Add(foreachStmt);
        
        // Step 9: Return reference to result variable
        // Note: This demonstrates the lowering pattern, but actual list append
        // functionality requires integration with Fifth.System runtime operations
        var resultRef = new VarRefExp
        {
            VarName = resultTempName,
            Type = elementType
        };
        
        return new RewriteResult(resultRef, prologue);
    }
}
