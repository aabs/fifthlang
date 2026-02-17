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
/// Lowering pass for TriG Literal Expressions.
/// 
/// Transforms TriGLiteralExpression AST nodes into runtime Store initialization code:
/// 
/// Input AST:
///   TriGLiteralExpression { Content = "...", Interpolations = [...] }
/// 
/// Output (lowered) AST:
///   var temp = BuildInterpolatedString(...);
///   FuncCallExp(Fifth.System.Store.LoadFromTriG, args: [temp])
/// 
/// User Story 1: Basic TriG literals without interpolation
/// User Story 2: Expression interpolation with type-aware serialization
/// 
/// The lowering preserves whitespace and newlines as specified in FR-008.
/// Diagnostics reference the original literal span as specified in FR-009.
/// </summary>
public class TriGLiteralLoweringRewriter : DefaultAstRewriter
{
    // Use actual CLR-backed Fifth types so downstream phases (validation, Roslyn translator)
    // can reason about them correctly.
    private static readonly FifthType StoreType = new FifthType.TDotnetType(typeof(Fifth.System.Store)) { Name = TypeName.From("Store") };
    private static readonly FifthType StringType = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };
    private static readonly Regex InterpolationPlaceholder = new Regex(@"\{\{__INTERP_(\d+)__\}\}", RegexOptions.Compiled);
    private int _tempVarCounter = 0;

    /// <summary>
    /// Rewrites TriGLiteralExpression nodes to Store.LoadFromTriG(trigContent) calls.
    /// Handles interpolations with type-aware serialization.
    /// </summary>
    public override RewriteResult VisitTriGLiteralExpression(TriGLiteralExpression ctx)
    {
        var prologue = new List<Statement>();
        Expression trigStringExpression;

        // Check if there are interpolations to process
        if (ctx.Interpolations != null && ctx.Interpolations.Count > 0)
        {
            // Build an interpolated string with serialization
            trigStringExpression = BuildInterpolatedTriGString(ctx.Content ?? string.Empty, ctx.Interpolations, prologue, ctx.Location);
        }
        else
        {
            // No interpolations - simple string literal
            // Mark as TriG content so Roslyn translator uses verbatim strings
            trigStringExpression = new StringLiteralExp
            {
                Value = ctx.Content ?? string.Empty,
                Type = StringType,
                Location = ctx.Location,
                Annotations = new Dictionary<string, object>
                {
                    ["TriGContent"] = true
                }
            };
        }

        // Create a FuncCallExp representing Fifth.System.Store.LoadFromTriG(trigContent)
        var funcCallExp = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { trigStringExpression },
            Type = StoreType,
            Location = ctx.Location,
            Annotations = new Dictionary<string, object>
            {
                // Mark this as an external static method call so translators can emit
                // a qualified invocation and validators can resolve the target method.
                ["ExternalType"] = typeof(Fifth.System.Store),
                ["ExternalMethodName"] = "LoadFromTriG",
                ["TriGLiteralLowering"] = true
            }
        };

        return new RewriteResult(funcCallExp, prologue);
    }

    /// <summary>
    /// Build an interpolated string expression by concatenating string parts with serialized expressions.
    /// </summary>
    private Expression BuildInterpolatedTriGString(string content, List<InterpolatedExpression> interpolations, List<Statement> prologue, SourceLocationMetadata? location)
    {
        // Split the content by placeholders and build a concatenation expression
        var parts = new List<Expression>();
        var currentPos = 0;

        for (int i = 0; i < interpolations.Count; i++)
        {
            var placeholder = $"{{{{__INTERP_{i}__}}}}";
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
                        Annotations = new Dictionary<string, object>
                        {
                            ["TriGContent"] = true
                        }
                    });
                }

                // Add the serialized interpolation
                var interp = interpolations[i];
                var serializedExpr = SerializeExpressionToRDF(interp.Expression, prologue, location);
                parts.Add(serializedExpr);

                currentPos = placeholderIndex + placeholder.Length;
            }
        }

        // Add any remaining text after the last interpolation
        if (currentPos < content.Length)
        {
            var remainingText = content.Substring(currentPos);
            parts.Add(new StringLiteralExp
            {
                Value = remainingText,
                Type = StringType,
                Annotations = new Dictionary<string, object>
                {
                    ["TriGContent"] = true
                }
            });
        }

        // If only one part, return it directly
        if (parts.Count == 1)
        {
            return parts[0];
        }

        // Build a chain of binary + operations for string concatenation
        Expression result = parts[0];
        for (int i = 1; i < parts.Count; i++)
        {
            result = new BinaryExp
            {
                LHS = result,
                RHS = parts[i],
                Operator = Operator.ArithmeticAdd,  // String concatenation uses +
                Type = StringType,
                Annotations = new Dictionary<string, object>
                {
                    ["TriGInterpolation"] = true
                }
            };
        }

        return result;
    }

    /// <summary>
    /// Serialize an expression to RDF format based on its type.
    /// Returns an expression that evaluates to the serialized string.
    /// </summary>
    private Expression SerializeExpressionToRDF(Expression expr, List<Statement> prologue, SourceLocationMetadata? location)
    {
        // Rewrite the expression first to handle any nested transformations
        var rewrittenExpr = (Expression)Rewrite(expr).Node;

        var exprType = rewrittenExpr.Type;

        // Check if this is a string type - either directly as TDotnetType(typeof(string))
        // or compare by name as fallback for type system variations
        bool isStringType = false;
        Type? dotnetType = null;

        if (exprType is FifthType.TDotnetType dt)
        {
            dotnetType = dt.TheType;
            isStringType = dotnetType == typeof(string);
        }
        else if (exprType != null && exprType.Name.Value == "string")
        {
            // Fallback: check by type name
            isStringType = true;
        }

        if (isStringType)
        {
            // String → quoted with proper RDF escaping
            var escapeCall = new FuncCallExp
            {
                InvocationArguments = new List<Expression> { rewrittenExpr },
                Type = StringType,
                Location = location,
                Annotations = new Dictionary<string, object>
                {
                    ["ExternalType"] = typeof(Fifth.System.RdfHelpers),
                    ["ExternalMethodName"] = "EscapeForRdf",
                    ["TriGInterpolation"] = true
                }
            };
            return escapeCall;
        }

        if (exprType is FifthType.TDotnetType dotnetTypeWrapped)
        {
            var type = dotnetTypeWrapped.TheType;

            // Numbers (int, float, etc.) → bare numeric (convert to string)
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
                type == typeof(ushort) || type == typeof(sbyte) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                // Ensure numeric literal participates in string concatenation as a string to avoid
                // type inference promoting the whole expression to a numeric type.
                var toStringCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { rewrittenExpr },
                    Type = StringType,
                    Location = location,
                    Annotations = new Dictionary<string, object>
                    {
                        ["ExternalType"] = typeof(System.Convert),
                        ["ExternalMethodName"] = "ToString",
                        ["TriGInterpolation"] = true
                    }
                };
                return toStringCall;
            }

            // Boolean → "true" or "false" (lowercase for RDF)
            if (type == typeof(bool))
            {
                // Lowercase conversion: Convert.ToString(expr).ToLowerInvariant()
                var toStringCall = new FuncCallExp
                {
                    InvocationArguments = new List<Expression> { rewrittenExpr },
                    Type = StringType,
                    Location = location,
                    Annotations = new Dictionary<string, object>
                    {
                        ["ExternalType"] = typeof(System.Convert),
                        ["ExternalMethodName"] = "ToString",
                        ["TriGInterpolation"] = true,
                        ["TriGBoolLower"] = true
                    }
                };
                return toStringCall;
            }
        }

        // Default: For unknown types or types we can't inspect, use RdfHelpers.EscapeForRdf
        // which will properly quote strings and handle other types appropriately
        var defaultEscape = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { rewrittenExpr },
            Type = StringType,
            Location = location,
            Annotations = new Dictionary<string, object>
            {
                ["ExternalType"] = typeof(Fifth.System.RdfHelpers),
                ["ExternalMethodName"] = "EscapeForRdf",
                ["TriGInterpolation"] = true
            }
        };
        return defaultEscape;
    }
}
