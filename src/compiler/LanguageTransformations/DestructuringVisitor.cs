using ast;
using ast_generated;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

public class DestructuringVisitor : DefaultRecursiveDescentVisitor
{
    public Stack<(string, ISymbolTableEntry)> ResolutionScope { get; } = new();
    public List<Expression> CollectedConstraints { get; } = new();

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
                ResolutionScope.Pop();
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
            if (db.NearestScope().TryResolveByName(propdecl.Value, out var paramType))
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
            ResolutionScope.Pop();
        }
        return result;
    }

    public override PropertyBindingDef VisitPropertyBindingDef(PropertyBindingDef ctx)
    {
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

        // Collect constraint from this property binding
        if (ctx.Constraint != null)
        {
            CollectedConstraints.Add(ctx.Constraint);
        }

        return ctx;
    }
}
