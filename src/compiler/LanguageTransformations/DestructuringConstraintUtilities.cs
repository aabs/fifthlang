using System;
using System.Collections.Generic;
using System.Linq;
using ast;
using ast_model;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

internal static class DestructuringConstraintUtilities
{
    internal static IReadOnlyList<Expression> CollectConstraints(ParamDef param, ParamDestructureDef destructureDef)
    {
        var constraints = new List<Expression>();

        foreach (var binding in destructureDef.Bindings)
        {
            var bindingLocation = binding.Location ?? default;

            if (binding.Constraint != null)
            {
                var propName = binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName;
                var rewritten = RewriteConstraint(binding.Constraint, binding.IntroducedVariable.Value, param.Name, propName.Value, bindingLocation);
                constraints.Add(rewritten);
            }

            if (binding.DestructureDef != null)
            {
                constraints.AddRange(CollectNestedConstraints(binding, param.Name));
            }
        }

        return constraints;
    }

    internal static Expression? CombineConstraints(ParamDef param, IReadOnlyList<Expression> constraints)
    {
        Expression? combinedConstraint = null;

        foreach (var constraint in constraints)
        {
            combinedConstraint = combinedConstraint == null
                ? constraint
                : CreateLogicalAnd(param, combinedConstraint, constraint);
        }

        if (param.ParameterConstraint != null)
        {
            combinedConstraint = combinedConstraint == null
                ? param.ParameterConstraint
                : CreateLogicalAnd(param, param.ParameterConstraint, combinedConstraint);
        }

        return combinedConstraint;
    }

    private static IEnumerable<Expression> CollectNestedConstraints(PropertyBindingDef binding, string rootParamName)
    {
        var constraints = new List<Expression>();

        if (binding.DestructureDef == null)
        {
            return constraints;
        }

        foreach (var nestedBinding in binding.DestructureDef.Bindings)
        {
            if (nestedBinding.Constraint != null)
            {
                var bindingLocation = binding.Location ?? default;
                var nestedLocation = nestedBinding.Location ?? default;

                var intermediateAccess = new MemberAccessExp
                {
                    LHS = new VarRefExp { VarName = rootParamName, Location = bindingLocation, Annotations = new Dictionary<string, object>() },
                    RHS = new VarRefExp { VarName = (binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName).Value, Location = bindingLocation, Annotations = new Dictionary<string, object>() },
                    Location = bindingLocation,
                    Annotations = new Dictionary<string, object>()
                };

                var propName = nestedBinding.ReferencedProperty?.Name ?? nestedBinding.ReferencedPropertyName;
                var finalAccess = new MemberAccessExp
                {
                    LHS = intermediateAccess,
                    RHS = new VarRefExp { VarName = propName.Value, Location = nestedLocation, Annotations = new Dictionary<string, object>() },
                    Location = nestedLocation,
                    Annotations = new Dictionary<string, object>()
                };

                var rewritten = RewriteConstraintExpression(nestedBinding.Constraint, nestedBinding.IntroducedVariable.Value, finalAccess);
                constraints.Add(rewritten);
            }

            if (nestedBinding.DestructureDef != null)
            {
                // TODO: Support deeper nesting levels if needed.
            }
        }

        return constraints;
    }

    private static BinaryExp CreateLogicalAnd(ParamDef param, Expression lhs, Expression rhs)
    {
        return new BinaryExp
        {
            Operator = Operator.LogicalAnd,
            LHS = lhs,
            RHS = rhs,
            Location = param.Location ?? default,
            Annotations = new Dictionary<string, object>()
        };
    }

    private static Expression RewriteConstraint(Expression constraint, string introducedVar, string paramName, string propertyName, SourceLocationMetadata bindingLocation)
    {
        var memberAccess = new MemberAccessExp
        {
            LHS = new VarRefExp { VarName = paramName, Location = bindingLocation, Annotations = new Dictionary<string, object>() },
            RHS = new VarRefExp { VarName = propertyName, Location = bindingLocation, Annotations = new Dictionary<string, object>() },
            Location = bindingLocation,
            Annotations = new Dictionary<string, object>()
        };

        return RewriteConstraintExpression(constraint, introducedVar, memberAccess);
    }

    private static Expression RewriteConstraintExpression(Expression constraint, string introducedVar, Expression targetExpr)
    {
        return constraint switch
        {
            VarRefExp v when string.Equals(v.VarName, introducedVar, StringComparison.Ordinal) => targetExpr,
            BinaryExp b => new BinaryExp
            {
                Operator = b.Operator,
                LHS = RewriteConstraintExpression(b.LHS, introducedVar, targetExpr),
                RHS = RewriteConstraintExpression(b.RHS, introducedVar, targetExpr),
                Location = b.Location,
                Annotations = b.Annotations
            },
            UnaryExp u => new UnaryExp
            {
                Operator = u.Operator,
                Operand = RewriteConstraintExpression(u.Operand, introducedVar, targetExpr),
                Location = u.Location,
                Annotations = u.Annotations
            },
            FuncCallExp f => new FuncCallExp
            {
                FunctionDef = f.FunctionDef,
                InvocationArguments = f.InvocationArguments?.Select(arg => RewriteConstraintExpression(arg, introducedVar, targetExpr)).ToList(),
                Location = f.Location,
                Annotations = f.Annotations
            },
            _ => constraint
        };
    }
}
