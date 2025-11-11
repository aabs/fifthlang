using System;
using System.Collections.Generic;
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
///   TriGLiteralExpression { Content = "..." }
/// 
/// Output (lowered) AST:
///   FuncCallExp(
///     function: Fifth.System.Store.LoadFromTriG,
///     args: [StringLiteralExp(trigContent)]
///   )
/// 
/// User Story 1 (MVP): Basic TriG literals without interpolation
/// User Story 2 (Future): Expression interpolation with type-aware serialization
/// 
/// The lowering preserves whitespace and newlines as specified in FR-008.
/// Diagnostics reference the original literal span as specified in FR-009.
/// </summary>
public class TriGLiteralLoweringRewriter : DefaultAstRewriter
{
    private static readonly FifthType StoreType = new FifthType.TType { Name = TypeName.From("Store") };
    private static readonly FifthType StringType = new FifthType.TDotnetType(typeof(string)) { Name = TypeName.From("string") };

    /// <summary>
    /// Rewrites TriGLiteralExpression nodes to Store.LoadFromTriG(trigContent) calls.
    /// </summary>
    public override RewriteResult VisitTriGLiteralExpression(TriGLiteralExpression ctx)
    {
        var prologue = new List<Statement>();

        // For MVP (User Story 1): just use the raw content without interpolation processing
        // The Content field already contains the TriG text captured by the parser
        var trigContent = ctx.Content ?? string.Empty;

        // Create a string literal for the TriG content
        var trigStringLiteral = new StringLiteralExp
        {
            Value = trigContent,
            Type = StringType,
            Location = ctx.Location,
            Annotations = new Dictionary<string, object>()
        };

        // Create a FuncCallExp representing Fifth.System.Store.LoadFromTriG(trigContent)
        // This will be resolved later during type annotation/code generation
        var funcCallExp = new FuncCallExp
        {
            InvocationArguments = new List<Expression> { trigStringLiteral },
            Type = StoreType,
            Location = ctx.Location,
            Annotations = new Dictionary<string, object>
            {
                ["TriGLiteralLowering"] = true,
                ["StaticMethodCall"] = "Fifth.System.Store.LoadFromTriG"
            }
        };

        return new RewriteResult(funcCallExp, prologue);
    }

    // Note: VisitInterpolatedExpression would be implemented in User Story 2
    // to handle interpolation serialization
}
