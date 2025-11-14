using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ast;
using ast_generated;
using ast_model.TypeSystem;

namespace compiler.LanguageTransformations;

/// <summary>
/// Lowering pass for SPARQL Literal Expressions.
/// 
/// Transforms SparqlLiteralExpression AST nodes into runtime Query initialization code.
/// 
/// Input AST:
///   SparqlLiteralExpression { SparqlText = "...", Interpolations = [...], Bindings = [...] }
/// 
/// Output (lowered) AST:
///   FuncCallExp(Fifth.System.Query.FromSparql, args: [sparqlText, parameters])
/// 
/// User Story 1: Basic SPARQL literals without interpolation
/// User Story 2: Variable binding via parameters (handled by SparqlVariableBindingVisitor)
/// User Story 3: Expression interpolation with type-aware serialization
/// 
/// The lowering replaces interpolation placeholders with actual values and creates
/// a parameterized query with both interpolated values and bound variables.
/// </summary>
public class SparqlLiteralLoweringRewriter : DefaultAstRewriter
{
    // Use actual CLR-backed Fifth types
    private static readonly FifthType QueryType = new FifthType.TType { Name = TypeName.From("Query") };
    private static readonly FifthType StringType = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };
    private static readonly Regex InterpolationPlaceholder = new Regex(@"\{\{__SPARQL_INTERP_(\d+)__\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Rewrites SparqlLiteralExpression nodes to Query construction calls.
    /// Handles interpolations and variable bindings.
    /// </summary>
    public override RewriteResult VisitSparqlLiteralExpression(SparqlLiteralExpression ctx)
    {
        var prologue = new List<Statement>();
        Expression sparqlStringExpression;

        // Check if there are interpolations to process
        if (ctx.Interpolations != null && ctx.Interpolations.Count > 0)
        {
            // Build an interpolated string with serialization
            sparqlStringExpression = BuildInterpolatedSparqlString(
                ctx.SparqlText ?? string.Empty, 
                ctx.Interpolations, 
                prologue, 
                ctx.Location);
        }
        else
        {
            // No interpolations - simple string literal
            sparqlStringExpression = new StringLiteralExp
            {
                Value = ctx.SparqlText ?? string.Empty,
                Type = StringType,
                Location = ctx.Location,
                Annotations = new Dictionary<string, object>()
            };
        }

        // For now, create a simple FuncCallExp representing the Query construction
        // In a complete implementation, this would also handle variable bindings
        // by creating a parameters dictionary
        var funcCallExp = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { sparqlStringExpression },
            Type = QueryType,
            Location = ctx.Location,
            Annotations = new Dictionary<string, object>
            {
                // Mark this as a Query literal lowering for downstream processing
                ["SparqlLiteralLowering"] = true,
                ["HasInterpolations"] = ctx.Interpolations?.Count > 0,
                ["HasBindings"] = ctx.Bindings?.Count > 0
            }
        };

        return new RewriteResult(funcCallExp, prologue);
    }

    /// <summary>
    /// Build an interpolated string expression by concatenating string parts with serialized expressions.
    /// </summary>
    private Expression BuildInterpolatedSparqlString(
        string content, 
        List<Interpolation> interpolations, 
        List<Statement> prologue, 
        SourceLocationMetadata? location)
    {
        // Split the content by placeholders and build a concatenation expression
        var parts = new List<Expression>();
        var currentPos = 0;
        
        for (int i = 0; i < interpolations.Count; i++)
        {
            var placeholder = $"{{{{__SPARQL_INTERP_{i}__}}}}";
            var placeholderIndex = content.IndexOf(placeholder, currentPos);
            
            if (placeholderIndex >= 0)
            {
                // Add the string part before the placeholder
                if (placeholderIndex > currentPos)
                {
                    var beforeText = content.Substring(currentPos, placeholderIndex - currentPos);
                    parts.Add(new StringLiteralExp
                    {
                        Value = beforeText,
                        Type = StringType,
                        Location = location,
                        Annotations = new Dictionary<string, object>()
                    });
                }
                
                // Add the interpolated expression (serialized to string)
                var interpolation = interpolations[i];
                var serializedExpr = SerializeExpressionToString(interpolation.Expression, location);
                parts.Add(serializedExpr);
                
                currentPos = placeholderIndex + placeholder.Length;
            }
        }
        
        // Add any remaining text after the last placeholder
        if (currentPos < content.Length)
        {
            var remainingText = content.Substring(currentPos);
            parts.Add(new StringLiteralExp
            {
                Value = remainingText,
                Type = StringType,
                Location = location,
                Annotations = new Dictionary<string, object>()
            });
        }
        
        // Build concatenation expression
        if (parts.Count == 0)
        {
            return new StringLiteralExp
            {
                Value = string.Empty,
                Type = StringType,
                Location = location,
                Annotations = new Dictionary<string, object>()
            };
        }
        else if (parts.Count == 1)
        {
            return parts[0];
        }
        else
        {
            // Build a chain of concatenations
            Expression result = parts[0];
            for (int i = 1; i < parts.Count; i++)
            {
                result = new BinaryExp
                {
                    LHS = result,
                    RHS = parts[i],
                    Operator = Operator.ArithmeticAdd,
                    Type = StringType,
                    Location = location,
                    Annotations = new Dictionary<string, object>()
                };
            }
            return result;
        }
    }

    /// <summary>
    /// Serialize an expression to a string representation suitable for SPARQL.
    /// For now, this just converts the expression to a string representation.
    /// A complete implementation would handle type-specific serialization
    /// (e.g., IRIs with angle brackets, literals with appropriate escaping).
    /// </summary>
    private Expression SerializeExpressionToString(Expression expr, SourceLocationMetadata? location)
    {
        // If it's already a string literal or string-typed, return as-is
        if (expr is StringLiteralExp || (expr.Type is FifthType.TDotnetType dotnetType && dotnetType.TheType == typeof(string)))
        {
            return expr;
        }

        // For other types, we would need to call a ToString() or similar serialization method
        // For now, create a function call to convert to string
        var toStringCall = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { expr },
            Type = StringType,
            Location = location,
            Annotations = new Dictionary<string, object>
            {
                ["ImplicitToString"] = true
            }
        };

        return toStringCall;
    }
}
