namespace Fifth.LangProcessingPhases;

/// <summary>
/// Convert a multi-clause function into simpler form.
/// </summary>
public class OverloadTransformingVisitor : DefaultRecursiveDescentVisitor
{
    public void ProcessOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx)
    {
        clauseCounter = 1;
        var clauses = new List<(Expression, MethodDef)>();
        foreach (var clause in ctx.OverloadClauses)
        {
            var precondition = GetPrecondition(clause);
            var subClauseFunction = GetSubclauseFunction(clause);
            clauseCounter++;
            clauses.Add((precondition, subClauseFunction));
        }

        var guardFunction = GenerateGuardFunction(ctx, clauses);
        var newFunctions = clauses.Select(c => c.Item2);
        newFunctions = newFunctions.Prepend(guardFunction);
        SubstituteFunctionDefinitions((ClassDef)ctx.Parent, [ctx], newFunctions);
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        var overloads = ctx.MemberDefs.OfType<OverloadedFunctionDefinition>().ToArray();
        foreach (var overloadedFuncDef in overloads)
        {
            ProcessOverloadedFunctionDefinition(overloadedFuncDef);
        }
        return base.VisitClassDef(ctx);
    }

    internal int clauseCounter;

    internal MethodDef GenerateGuardFunction(OverloadedFunctionDefinition ctx, List<(Expression, MethodDef)> clauses)
    {
        var ifStatements = new List<Statement>();
        FunctionDef fd = (FunctionDef)new FunctionDefBuilder()
               .WithName(ctx.Name)
               .WithReturnType(ctx.Type)
               .WithBody(null)
               .WithParams(ctx.OverloadClauses.First().FunctionDef.Params)
               .Build()
               .WithSameParentAs(ctx);
        foreach (var clause in clauses)
        {
            /*
             var name = context.funcname.GetText();
                var actualParams = (ExpressionList)VisitExplist(context.args);
                return new FuncCallExpression(actualParams, name)
             */
            var args = (from p in clause.Item2.FunctionDef.Params
                        select (Expression)new VarRefExp() { VarName = p.Name, Parent = p as IAstThing }).ToList();

            var funcCallExpression = new FuncCallExp()
            {
                Annotations = [],
                InvocationArguments = args,
                FunctionDef = fd
            };

            if (clause.Item1 != null)
            {
                var ifBlock = new BlockStatementBuilder()
                                          .AddingItemToStatements(new ReturnStatementBuilder().WithReturnValue(funcCallExpression).Build())
                                          .Build();
                var ieb = new IfElseStatementBuilder()
                                       .WithCondition(clause.Item1)
                                       .WithThenBlock(ifBlock)
                                       .Build();
                ifStatements.Add(ieb);
            }
            else
            {
                // if there is no clause condition, it must be the base case (which should be the
                // last clause)
                ifStatements.Add(new ExpStatementBuilder().WithRHS(funcCallExpression).Build());
            }
        }
        fd.Body = new BlockStatement() { Statements = ifStatements };

        // TODO: Use FunctionDefinitionBuilder

        return new MethodDef()
        {
            Annotations = ctx.Annotations,
            IsReadOnly = false,
            Name = ctx.Name,
            FunctionDef = fd,
            Location = ctx.Location,
            Parent = ctx.Parent,
            Type = ctx.Type,
            TypeName = ctx.TypeName,
            Visibility = ctx.Visibility
        };
    }

    /// <summary>
    /// extract the preconditions for entry into a subclause
    /// </summary>
    /// <param name="clause">the original function with preconditions</param>
    /// <returns>an expression combining all preconditions</returns>
    internal Expression GetPrecondition(MethodDef clause)
    {
        var conditions = new Queue<Expression>();
        foreach (var pd in clause.FunctionDef.Params)
        {
            if (pd.ParameterConstraint != null)
            {
                conditions.Enqueue(pd.ParameterConstraint);
            }
        }

        Expression? e = null;
        while (conditions.Count > 0)
        {
            if (e == null)
            {
                e = conditions.Dequeue();
            }
            else
            {
                e = new BinaryExpBuilder()
                    .WithLHS(e)
                    .WithRHS(conditions.Dequeue())
                    .WithOperator(Operator.LogicalAnd)
                                             .Build();
                // (e, Operator.And, conditions.Dequeue());
            }
        }

        return e;
    }

    /// <summary>
    /// transforms the function into a form that can be envoked from a dispatcher guard function
    /// </summary>
    /// <param name="clause">the original function to transform</param>
    /// <returns>a new function</returns>
    internal MethodDef GetSubclauseFunction(MethodDef clause)
    {
        var fd = new FunctionDefBuilder()
            .WithName(MemberName.From($"{clause.Name.Value}_subclause{clauseCounter}"))
                 .WithBody(clause.FunctionDef.Body)
                 .WithParams(clause.FunctionDef.Params)
                 .WithReturnType(clause.Type)
                 .Build();
        fd.Parent = clause.Parent;

        // get all the bindings create a new function with unique name normalise param list and
        // insert into function add binding var decls to front of body insert body of old function
        // into new function
        return new MethodDef()
        {
            FunctionDef = fd,
            Name = fd.Name,
            Type = fd.ReturnType,
            TypeName = fd.ReturnType.Name,
            Visibility = clause.Visibility,
            IsReadOnly = clause.IsReadOnly,
            Parent = clause.Parent
        };
    }

    internal void SubstituteFunctionDefinitions(ClassDef owner, IEnumerable<OverloadedFunctionDefinition> functionsToRemove, IEnumerable<MethodDef> functionsToAdd)
    {
        foreach (var fd in functionsToRemove)
        {
            owner.MemberDefs.Remove(fd);
        }
        foreach (var fd in functionsToAdd)
        {
            owner.MemberDefs.Add(fd);
        }
    }
}
