using ast;
using ast_generated;

namespace compiler.LanguageTransformations;

/// <summary>
/// Propagates guard constraints from destructured parameters onto the ParamDef.ParameterConstraint
/// so that guard validation and overload dispatch can observe them prior to lowering.
/// </summary>
public class DestructuringConstraintPropagator : DefaultRecursiveDescentVisitor
{
    internal const string ConstraintAnnotationKey = "__destructuring_constraints_applied";

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        if (ctx.DestructureDef == null || ctx.HasAnnotation(ConstraintAnnotationKey))
        {
            return base.VisitParamDef(ctx);
        }

        var constraints = DestructuringConstraintUtilities.CollectConstraints(ctx, ctx.DestructureDef);
        if (constraints.Count == 0)
        {
            return base.VisitParamDef(ctx);
        }

        var combinedConstraint = DestructuringConstraintUtilities.CombineConstraints(ctx, constraints);
        if (combinedConstraint == null)
        {
            return base.VisitParamDef(ctx);
        }

        var updatedParam = ctx with { ParameterConstraint = combinedConstraint };
        updatedParam[ConstraintAnnotationKey] = true;
        return base.VisitParamDef(updatedParam);
    }
}
