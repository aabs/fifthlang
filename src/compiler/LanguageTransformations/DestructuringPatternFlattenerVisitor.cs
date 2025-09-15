using ast;
using ast_generated;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Responsible for flattening destructuring patterns and collecting constraints from property bindings.
/// This visitor creates a primary entry function for overloads that applies all constraint filters
/// in the order they were declared, then passes call handling to the first overloaded function that matches.
/// 
/// Architectural responsibility: 
/// 1. Lowering constraints from destructured properties into parameter-level constraints
/// 2. Creating constraint filters for function overload resolution
/// 3. Coordinating the overall destructuring transformation pipeline
/// </summary>
public class DestructuringPatternFlattenerVisitor : DefaultRecursiveDescentVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();
    public List<Expression> CollectedConstraints { get; } = new();

    private Expression RewriteConstraint(Expression constraint, string introducedVar, string paramName, string propertyName)
    {
        Expression Rewrite(Expression e)
        {
            switch (e)
            {
                case VarRefExp v when string.Equals(v.VarName, introducedVar, StringComparison.Ordinal):
                    return new MemberAccessExp
                    {
                        Annotations = v.Annotations,
                        Location = v.Location,
                        Type = v.Type,
                        LHS = new VarRefExp { Annotations = [], Location = v.Location, Type = v.Type, VarName = paramName },
                        RHS = new VarRefExp { Annotations = [], Location = v.Location, Type = v.Type, VarName = propertyName }
                    };
                case BinaryExp b:
                    return new BinaryExpBuilder()
                        .WithOperator(b.Operator)
                        .WithLHS(Rewrite(b.LHS))
                        .WithRHS(Rewrite(b.RHS))
                        .Build();
                case UnaryExp u:
                    return new UnaryExp { Annotations = u.Annotations, Location = u.Location, Type = u.Type, Operator = u.Operator, Operand = Rewrite(u.Operand) };
                case FuncCallExp f:
                    var newArgs = f.InvocationArguments?.Select(Rewrite).ToList() ?? new List<Expression>();
                    return new FuncCallExp { Annotations = f.Annotations, Location = f.Location, Type = f.Type, FunctionDef = f.FunctionDef, InvocationArguments = newArgs };
                default:
                    return e;
            }
        }

        return Rewrite(constraint);
    }

    public override FunctionDef VisitFunctionDef(FunctionDef ctx)
    {
        // Only attempt to flatten destructuring patterns when the function actually
        // declares parameters with destructuring. This avoids injecting synthesized
        // variable declarations into unrelated functions (e.g., main) that do not
        // use destructuring in their parameter list.
        if (ctx.Params != null && ctx.Params.Any(p => p.DestructureDef != null))
        {
            // First, visit each ParamDef to collect and attach constraints
            var updatedParams = new List<ParamDef>(ctx.Params.Count);
            foreach (var p in ctx.Params)
            {
                updatedParams.Add(VisitParamDef(p));
            }

            // Next, synthesize variable declarations from property bindings
            var statementGatherer = new PropertyBindingToVariableDeclarationTransformer();
            statementGatherer.Visit(ctx);

            return ctx with
            {
                Params = updatedParams,
                Body = ctx.Body with
                {
                    Statements = [.. statementGatherer.Statements, .. ctx.Body.Statements]
                }
            };
        }

        return base.VisitFunctionDef(ctx);
    }

    public override ParamDef VisitParamDef(ParamDef ctx)
    {
        if (ctx.DestructureDef is null)
        {
            return ctx;
        }

        // Clear constraints for this parameter
        CollectedConstraints.Clear();

        ParamDef? result = null;
        if (ctx.NearestScope().TryResolveByName(ctx.TypeName.Value, out var paramType))
        {
            ResolutionScope.Push((ctx.Name, paramType));
            try
            {
                result = base.VisitParamDef(ctx);

                // If we collected any constraints from property bindings, combine them with existing parameter constraint
                if (CollectedConstraints.Count > 0)
                {
                    Expression combinedConstraint;

                    if (CollectedConstraints.Count == 1)
                    {
                        combinedConstraint = CollectedConstraints[0];
                    }
                    else
                    {
                        // Combine multiple constraints with AND
                        combinedConstraint = CollectedConstraints[0];
                        for (int i = 1; i < CollectedConstraints.Count; i++)
                        {
                            combinedConstraint = new BinaryExpBuilder()
                                .WithOperator(ast.Operator.LogicalAnd)
                                .WithLHS(combinedConstraint)
                                .WithRHS(CollectedConstraints[i])
                                .Build();
                        }
                    }

                    // Combine with existing parameter constraint if any
                    if (ctx.ParameterConstraint != null)
                    {
                        combinedConstraint = new BinaryExpBuilder()
                            .WithOperator(ast.Operator.LogicalAnd)
                            .WithLHS(ctx.ParameterConstraint)
                            .WithRHS(combinedConstraint)
                            .Build();
                    }

                    // Create new ParamDef with combined constraint
                    result = result with { ParameterConstraint = combinedConstraint };
                }
            }
            finally
            {
                if (ResolutionScope.Count > 0) ResolutionScope.Pop();
            }
        }
        return result ?? ctx;
    }

    public override ParamDestructureDef VisitParamDestructureDef(ParamDestructureDef ctx)
    {
        if (ctx.Parent is ParamDef pd)
        {
            if (ctx.NearestScope().TryResolveByName(pd.TypeName.Value, out var paramType))
            {
                ResolutionScope.Push((pd.Name, paramType));
            }
        }
        else if (ctx.Parent is PropertyBindingDef db)
        {
            // currently the only place that sets this annotation is
            // PropertyBindingToVariableDeclarationTransformer.EnterDestructuringBinding when the
            // propertyDefinitionScope is a classdefinition
            var propdecl = db.ReferencedPropertyName;
            if (ctx.NearestScope().TryResolveByName(propdecl.Value, out var paramType))
            {
                ResolutionScope.Push((propdecl.Value, paramType));
            }
        }
        ParamDestructureDef result;
        try
        {
            result = base.VisitParamDestructureDef(ctx);
        }
        finally
        {
            if (ResolutionScope.Count > 0) ResolutionScope.Pop();
        }
        return result;
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
        if (ResolutionScope.Count == 0)
        {
            return ctx; // nothing to link against
        }
        var (scopeVarName, propertyDefinitionScope) = ResolutionScope.Peek();

        if (propertyDefinitionScope.Symbol.Kind == SymbolKind.ClassDef && propertyDefinitionScope.OriginatingAstThing is ClassDef c)
        {
            // the propname of the ctx needs to be resolved as a propertydefinition of the class c
            var propdecl = c.MemberDefs.OfType<PropertyDef>().FirstOrDefault(pd => pd.Name == ctx.ReferencedPropertyName);
            if (propdecl != null)
            {
                ctx.ReferencedProperty = propdecl;
                // if propdecl is not null, it means we know that the RHS of the assignment is var
                // ref to <scopeVarName>.(propdecl.Name)
            }
        }

        // Collect constraint from this property binding, but rewrite references to the
        // introduced variable (e.g. 'grade') as param.property (e.g. 'student.Grade') so
        // guard functions (which only receive the param) can evaluate conditions directly.
        if (ctx.Constraint != null)
        {
            var propName = ctx.ReferencedProperty?.Name.Value ?? ctx.ReferencedPropertyName.Value;
            var rewritten = RewriteConstraint(ctx.Constraint, ctx.IntroducedVariable.Value, scopeVarName, propName);
            CollectedConstraints.Add(rewritten);
        }

        return ctx;
    }
}
