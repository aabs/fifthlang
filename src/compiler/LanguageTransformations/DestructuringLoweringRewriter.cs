using System;
using System.Collections.Generic;
using System.Linq;
using ast;
using ast_generated;
using ast_model.Symbols;

namespace compiler.LanguageTransformations;

/// <summary>
/// Rewriter-based lowering pass for destructuring parameters.
/// Replaces the old visitor-based approach with statement-hoisting via DefaultAstRewriter.
/// 
/// Lowering rules:
/// 1) For each ParamDef with DestructureDef, hoist a temp variable for the parameter
/// 2) For each PropertyBindingDef in the destructure:
///    - Create a local variable that reads the property from the temp
///    - If Constraint exists, emit guard statement
///    - If nested DestructureDef exists, recursively process
/// 3) Insert all hoisted statements at the start of the function body
/// </summary>
public class DestructuringLoweringRewriter : DefaultAstRewriter
{
    private int _tmpCounter = 0;

    /// <summary>
    /// Generate a fresh temporary variable name
    /// </summary>
    private string FreshTempName(string prefix = "tmp") => $"__{prefix}{_tmpCounter++}";

    public override RewriteResult VisitFunctionDef(FunctionDef ctx)
    {
        // Check if this function has any destructured parameters
        bool hasDestructuring = ctx.Params != null && ctx.Params.Any(p => p.DestructureDef != null);
        
        if (!hasDestructuring)
        {
            // No destructuring - use default behavior
            return base.VisitFunctionDef(ctx);
        }

        // First, collect constraints from destructured parameters and update them
        // This replicates the constraint collection logic from DestructuringPatternFlattenerVisitor
        var updatedParams = ctx.Params.Select(p => 
        {
            if (p.DestructureDef == null)
                return p;
            
            var constraints = CollectConstraints(p, p.DestructureDef);
            if (constraints.Count == 0)
                return p;
            
            // Combine constraints with existing parameter constraint
            Expression combinedConstraint = constraints[0];
            for (int i = 1; i < constraints.Count; i++)
            {
                combinedConstraint = new BinaryExp
                {
                    Operator = ast.Operator.LogicalAnd,
                    LHS = combinedConstraint,
                    RHS = constraints[i],
                    Location = p.Location ?? new SourceLocationMetadata(),
                    Annotations = new Dictionary<string, object>()
                };
            }
            
            if (p.ParameterConstraint != null)
            {
                combinedConstraint = new BinaryExp
                {
                    Operator = ast.Operator.LogicalAnd,
                    LHS = p.ParameterConstraint,
                    RHS = combinedConstraint,
                    Location = p.Location ?? new SourceLocationMetadata(),
                    Annotations = new Dictionary<string, object>()
                };
            }
            
            return p with { ParameterConstraint = combinedConstraint };
        }).ToList();

        // Process the function body first to get any nested prologues
        var bodyResult = Rewrite(ctx.Body);
        var bodyStatements = new List<Statement>();
        
        // Now process each parameter and emit destructuring statements
        var destructuringStatements = new List<Statement>();
        
        foreach (var param in updatedParams)
        {
            if (param.DestructureDef != null)
            {
                // Lower this destructured parameter
                var stmts = LowerParameterDestructuring(param);
                destructuringStatements.AddRange(stmts);
            }
        }

        // Combine: destructuring statements first, then original body statements
        bodyStatements.AddRange(destructuringStatements);
        bodyStatements.AddRange(bodyResult.Prologue);
        bodyStatements.AddRange(((BlockStatement)bodyResult.Node).Statements);

        var newBody = new BlockStatement
        {
            Statements = bodyStatements,
            Location = ctx.Body.Location,
            Annotations = ctx.Body.Annotations
        };

        var rebuilt = ctx with { Params = updatedParams, Body = newBody };
        return new RewriteResult(rebuilt, []);
    }

    /// <summary>
    /// Collect constraints from property bindings and rewrite them to reference parameter.property
    /// This mirrors the logic from DestructuringPatternFlattenerVisitor
    /// </summary>
    private List<Expression> CollectConstraints(ParamDef param, ParamDestructureDef destructureDef)
    {
        var constraints = new List<Expression>();
        
        foreach (var binding in destructureDef.Bindings)
        {
            if (binding.Constraint != null)
            {
                // Rewrite constraint to reference param.property instead of the introduced variable
                var propName = binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName;
                var rewritten = RewriteConstraint(binding.Constraint, binding.IntroducedVariable.Value, param.Name, propName.Value);
                constraints.Add(rewritten);
            }
            
            // Recursively collect from nested destructuring
            if (binding.DestructureDef != null)
            {
                // For nested constraints, we need to chain the property access
                var nestedConstraints = CollectNestedConstraints(binding, param.Name);
                constraints.AddRange(nestedConstraints);
            }
        }
        
        return constraints;
    }

    /// <summary>
    /// Collect constraints from nested destructuring, chaining property access
    /// </summary>
    private List<Expression> CollectNestedConstraints(PropertyBindingDef binding, string rootParamName)
    {
        var constraints = new List<Expression>();
        
        if (binding.DestructureDef == null)
            return constraints;
        
        foreach (var nestedBinding in binding.DestructureDef.Bindings)
        {
            if (nestedBinding.Constraint != null)
            {
                // Build the chained property access: rootParam.binding.property -> nestedBinding.property
                var intermediateAccess = new MemberAccessExp
                {
                    LHS = new VarRefExp { VarName = rootParamName },
                    RHS = new VarRefExp { VarName = (binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName).Value }
                };
                
                var propName = nestedBinding.ReferencedProperty?.Name ?? nestedBinding.ReferencedPropertyName;
                var finalAccess = new MemberAccessExp
                {
                    LHS = intermediateAccess,
                    RHS = new VarRefExp { VarName = propName.Value }
                };
                
                // Rewrite the constraint to use the chained access
                var rewritten = RewriteConstraintExpression(nestedBinding.Constraint, nestedBinding.IntroducedVariable.Value, finalAccess);
                constraints.Add(rewritten);
            }
            
            // Continue recursively if there's more nesting
            if (nestedBinding.DestructureDef != null)
            {
                // This would require more complex chaining - left as TODO for now
                // Most destructuring is 1-2 levels deep in practice
            }
        }
        
        return constraints;
    }

    /// <summary>
    /// Rewrite constraint expression to replace variable references with parameter.property access
    /// </summary>
    private Expression RewriteConstraint(Expression constraint, string introducedVar, string paramName, string propertyName)
    {
        var memberAccess = new MemberAccessExp
        {
            LHS = new VarRefExp { VarName = paramName },
            RHS = new VarRefExp { VarName = propertyName }
        };
        
        return RewriteConstraintExpression(constraint, introducedVar, memberAccess);
    }

    /// <summary>
    /// Rewrite constraint expression to replace references to introducedVar with targetExpr
    /// </summary>
    private Expression RewriteConstraintExpression(Expression constraint, string introducedVar, Expression targetExpr)
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

    /// <summary>
    /// Lower a destructured parameter into variable declarations
    /// </summary>
    private List<Statement> LowerParameterDestructuring(ParamDef param)
    {
        var statements = new List<Statement>();
        
        if (param.DestructureDef == null)
            return statements;

        // The parameter itself is accessible by param.Name
        var paramSourceExpr = new VarRefExp 
        { 
            VarName = param.Name, 
            Location = param.Location ?? new SourceLocationMetadata(),
            Annotations = new Dictionary<string, object>()
        };

        // Process each property binding
        foreach (var binding in param.DestructureDef.Bindings)
        {
            var bindingStmts = LowerPropertyBinding(binding, paramSourceExpr, param);
            statements.AddRange(bindingStmts);
        }

        return statements;
    }

    /// <summary>
    /// Lower a property binding into variable declarations
    /// </summary>
    private List<Statement> LowerPropertyBinding(PropertyBindingDef binding, Expression sourceExpr, ParamDef param)
    {
        var statements = new List<Statement>();
        var loc = binding.Location ?? new SourceLocationMetadata();

        // Create member access: sourceExpr.PropertyName
        var propertyName = binding.ReferencedProperty?.Name ?? binding.ReferencedPropertyName;
        var propertyAccess = new MemberAccessExp
        {
            LHS = sourceExpr,
            RHS = new VarRefExp 
            { 
                VarName = propertyName.Value, 
                Location = loc,
                Annotations = new Dictionary<string, object>()
            },
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        // Determine the type for the variable
        var typeName = binding.ReferencedProperty?.TypeName ?? TypeName.From("object");

        // Create variable declaration: var introducedVar = sourceExpr.PropertyName
        var varDecl = new VariableDecl
        {
            Name = binding.IntroducedVariable.Value,
            TypeName = typeName,
            CollectionType = CollectionType.SingleInstance,
            Visibility = Visibility.Private,
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        var varDeclStmt = new VarDeclStatement
        {
            VariableDecl = varDecl,
            InitialValue = propertyAccess,
            Location = loc,
            Annotations = new Dictionary<string, object>()
        };

        statements.Add(varDeclStmt);

        // Handle constraint if present
        if (binding.Constraint != null)
        {
            // Create guard statement for the constraint
            // The constraint should reference the introduced variable
            var guardStmt = new GuardStatement
            {
                Condition = binding.Constraint,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };
            statements.Add(guardStmt);
        }

        // Handle nested destructuring if present
        if (binding.DestructureDef != null)
        {
            // The introduced variable becomes the source for nested destructuring
            var nestedSourceExpr = new VarRefExp
            {
                VarName = binding.IntroducedVariable.Value,
                Location = loc,
                Annotations = new Dictionary<string, object>()
            };

            foreach (var nestedBinding in binding.DestructureDef.Bindings)
            {
                var nestedStmts = LowerPropertyBinding(nestedBinding, nestedSourceExpr, param);
                statements.AddRange(nestedStmts);
            }
        }

        return statements;
    }
}
