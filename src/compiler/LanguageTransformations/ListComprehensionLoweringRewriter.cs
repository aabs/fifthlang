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
    private string? _currentLoopVar = null;  // Track the current SPARQL row loop variable
    private bool _isResultIteration = false; // Track if we're in a Result iteration context

    /// <summary>
    /// Generate a fresh temporary variable name
    /// </summary>
    private string FreshTempName(string prefix = "tmp") => $"__{prefix}_comprehension_{_tempCounter++}";

    /// <summary>
    /// Override MemberAccessExp to transform x.property into TabularResultBindings.GetBindingAsString(row, "property")
    /// when x is the SPARQL row loop variable.
    /// </summary>
    public override RewriteResult VisitMemberAccessExp(MemberAccessExp ctx)
    {
        // Check if this is a property access on the SPARQL row loop variable
        if (_isResultIteration &&
            ctx.LHS is VarRefExp varRef &&
            varRef.VarName == _currentLoopVar &&
            ctx.RHS is VarRefExp propertyName)
        {
            // Transform x.property to TabularResultBindings.GetBindingAsString(x, "property")
            var getBindingCall = new FuncCallExp
            {
                FunctionDef = null,
                InvocationArguments = new List<Expression>
                {
                    varRef,  // The row variable
                    new StringLiteralExp
                    {
                        Value = propertyName.VarName,
                        Type = new FifthType.TType() { Name = TypeName.From("string") }
                    }
                },
                Type = new FifthType.TType() { Name = TypeName.From("string") },
                Annotations = new Dictionary<string, object>
                {
                    ["ExternalType"] = typeof(Fifth.System.TabularResultBindings),
                    ["ExternalMethodName"] = "GetBindingAsString"
                }
            };

            return new RewriteResult(getBindingCall, new List<Statement>());
        }

        // Otherwise, use default behavior
        return base.VisitMemberAccessExp(ctx);
    }

    public override RewriteResult VisitVarDeclStatement(VarDeclStatement ctx)
    {
        // If the initial value is a ListComprehension without a proper type, 
        // propagate the variable's type to it before rewriting
        if (ctx.InitialValue is ListComprehension lc &&
            ctx.VariableDecl.Type is FifthType.TListOf listType)
        {
            // Create a new ListComprehension with the correct type
            var typedComprehension = lc with { Type = listType };

            // Rewrite the typed comprehension
            var result = Rewrite(typedComprehension);

            // Create new VarDeclStatement with the rewritten expression
            var newVarDecl = ctx with { InitialValue = (Expression)result.Node };

            // Return the new statement with prologue from comprehension
            return new RewriteResult(newVarDecl, result.Prologue);
        }

        return base.VisitVarDeclStatement(ctx);
    }

    public override RewriteResult VisitListComprehension(ListComprehension ctx)
    {
        // Full lowering implementation: Transform comprehensions to imperative code
        // 
        // For SPARQL Result sources:
        //   [projection from varname in result where constraint1, constraint2]
        // 
        // Into:
        //   {
        //     temp_result_list = []
        //     temp_source = source
        //     temp_rows = Fifth.System.TabularResultBindings.EnumerateRows(temp_source)
        //     foreach row in temp_rows:
        //       // Map x.property to TabularResultBindings.GetBindingAsString(row, "property")
        //       if constraint1 && constraint2:
        //         temp_append_result = projection
        //     result = temp_result_list
        //   }

        var prologue = new List<Statement>();

        // Step 1: Rewrite source expression and collect its prologue
        var sourceResult = Rewrite(ctx.Source);
        prologue.AddRange(sourceResult.Prologue);
        var sourceExpr = (Expression)sourceResult.Node;

        // Check if source is a Result type (from SPARQL query application)
        bool isResultSource = (sourceExpr.Type?.Name.Value == "Result") ||
                              (sourceExpr.Type is FifthType.TDotnetType dt &&
                               dt.TheType == typeof(Fifth.System.Result));

        // Step 2: Create temporary variable for source (to evaluate once)
        var sourceTempName = FreshTempName("source");
        var sourceTempDecl = new VariableDecl
        {
            Name = sourceTempName,
            TypeName = sourceExpr.Type?.Name ?? TypeName.From("object"),
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance,
            Type = sourceExpr.Type
        };
        var sourceTempDeclStmt = new VarDeclStatement
        {
            VariableDecl = sourceTempDecl,
            InitialValue = sourceExpr
        };
        prologue.Add(sourceTempDeclStmt);

        // Step 2.5: If source is Result, extract rows using TabularResultBindings.EnumerateRows
        Expression collectionToIterate;
        if (isResultSource)
        {
            var rowsTempName = FreshTempName("rows");
            var enumerateRowsCall = new FuncCallExp
            {
                FunctionDef = null,
                InvocationArguments = new List<Expression>
                {
                    new VarRefExp
                    {
                        VarName = sourceTempName,
                        Type = sourceExpr.Type
                    }
                },
                Type = new FifthType.TDotnetType(typeof(IEnumerable<VDS.RDF.Query.ISparqlResult>))
                {
                    Name = TypeName.From("IEnumerable<ISparqlResult>")
                },
                Annotations = new Dictionary<string, object>
                {
                    ["ExternalType"] = typeof(Fifth.System.TabularResultBindings),
                    ["ExternalMethodName"] = "EnumerateRows"
                }
            };

            var rowsTempDecl = new VariableDecl
            {
                Name = rowsTempName,
                TypeName = TypeName.From("IEnumerable<ISparqlResult>"),
                Visibility = Visibility.Private,
                CollectionType = CollectionType.SingleInstance,
                Type = enumerateRowsCall.Type
            };
            var rowsTempDeclStmt = new VarDeclStatement
            {
                VariableDecl = rowsTempDecl,
                InitialValue = enumerateRowsCall
            };
            prologue.Add(rowsTempDeclStmt);

            collectionToIterate = new VarRefExp
            {
                VarName = rowsTempName,
                Type = enumerateRowsCall.Type
            };
        }
        else
        {
            collectionToIterate = new VarRefExp
            {
                VarName = sourceTempName,
                Type = sourceExpr.Type
            };
        }

        // Step 3: Create temporary variable for result list (initially empty)
        // Extract element type from list comprehension type
        var resultTempName = FreshTempName("result");
        FifthType elementType;
        FifthType listType;

        if (ctx.Type is FifthType.TListOf listOf)
        {
            elementType = listOf.ElementType;
            listType = listOf;  // Use the original list type
        }
        else if (ctx.Type != null)
        {
            // Fallback: ctx.Type is not a TListOf, use projection type if available
            elementType = ctx.Projection.Type ?? new FifthType.TType() { Name = TypeName.From("object") };
            listType = new FifthType.TListOf(elementType)
            {
                Name = TypeName.From($"[{elementType.Name}]")
            };
        }
        else
        {
            // No type info available - use object as fallback
            elementType = new FifthType.TType() { Name = TypeName.From("object") };
            listType = new FifthType.TListOf(elementType)
            {
                Name = TypeName.From("[object]")
            };
        }

        var resultTempDecl = new VariableDecl
        {
            Name = resultTempName,
            TypeName = elementType.Name,
            Visibility = Visibility.Private,
            CollectionType = CollectionType.List,
            Type = listType  // Set the full list type
        };
        var emptyList = new ListLiteral
        {
            ElementExpressions = new List<Expression>(),
            Type = listType  // Set the list type, not element type
        };
        var resultTempDeclStmt = new VarDeclStatement
        {
            VariableDecl = resultTempDecl,
            InitialValue = emptyList
        };
        prologue.Add(resultTempDeclStmt);

        // Step 4: Set context for SPARQL row iteration and rewrite projection/constraints
        var previousLoopVar = _currentLoopVar;
        var previousIsResult = _isResultIteration;
        _currentLoopVar = ctx.VarName;
        _isResultIteration = isResultSource;

        // Rewrite projection expression with SPARQL row context
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

        // Restore context
        _currentLoopVar = previousLoopVar;
        _isResultIteration = previousIsResult;

        // Step 6: Create call to Add() method to append projection to result list
        // This generates: resultList.Add(projection)
        var addCall = new FuncCallExp
        {
            FunctionDef = null,
            InvocationArguments = new List<Expression> { projectionExpr },
            Type = Void,
            Annotations = new Dictionary<string, object>
            {
                ["ExternalMethodName"] = "Add",
                ["IsInstanceMethod"] = true,
                ["Target"] = new VarRefExp
                {
                    VarName = resultTempName,
                    Type = listType
                }
            }
        };
        var addCallStmt = new ExpStatement
        {
            RHS = addCall
        };

        // Step 7: Build loop body (with optional constraint check)
        var loopBodyStatements = new List<Statement>();
        if (projectionResult.Prologue.Count > 0)
        {
            loopBodyStatements.AddRange(projectionResult.Prologue);
        }

        if (combinedConstraint != null)
        {
            // Wrap Add call in if statement
            var ifStmt = new IfElseStatement
            {
                Condition = combinedConstraint,
                ThenBlock = new BlockStatement
                {
                    Statements = new List<Statement> { addCallStmt }
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
            // No constraint, just Add to list
            loopBodyStatements.Add(addCallStmt);
        }

        // Step 8: Create foreach loop
        // Determine the type of the loop variable based on the collection being iterated
        FifthType loopVarType;
        TypeName loopVarTypeName;

        if (isResultSource)
        {
            loopVarType = new FifthType.TDotnetType(typeof(VDS.RDF.Query.ISparqlResult))
            {
                Name = TypeName.From("ISparqlResult")
            };
            loopVarTypeName = TypeName.From("ISparqlResult");
        }
        else
        {
            // Determine loop variable type from the source collection type
            if (sourceExpr.Type is FifthType.TListOf sourceListType)
            {
                loopVarType = sourceListType.ElementType;
            }
            else if (sourceExpr.Type is FifthType.TArrayOf sourceArrayType)
            {
                loopVarType = sourceArrayType.ElementType;
            }
            else
            {
                // Fallback to unknown/dynamic
                loopVarType = new FifthType.UnknownType() { Name = TypeName.From("unknown") };
            }
            loopVarTypeName = loopVarType.Name;
        }

        var loopVarDecl = new VariableDecl
        {
            Name = ctx.VarName,
            TypeName = loopVarTypeName,
            Visibility = Visibility.Private,
            CollectionType = CollectionType.SingleInstance,
            Type = loopVarType
        };
        var foreachStmt = new ForeachStatement
        {
            Collection = collectionToIterate,
            LoopVariable = loopVarDecl,
            Body = new BlockStatement
            {
                Statements = loopBodyStatements
            }
        };
        prologue.Add(foreachStmt);

        // Step 9: Return reference to result variable wrapped in list type
        var resultRef = new VarRefExp
        {
            VarName = resultTempName,
            Type = new FifthType.TListOf(elementType)
            {
                Name = TypeName.From($"[{elementType.Name}]")
            }
        };

        return new RewriteResult(resultRef, prologue);
    }
}
