namespace Fifth.LangProcessingPhases;

/// <summary>
/// Convert a multi-clause function into simpler form.
/// </summary>
public class OverloadTransformingVisitor : DefaultRecursiveDescentVisitor
{
    Stack<ModuleDef> moduleStack = [];
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

    public void ProcessOverloadedModuleFunctionDef(OverloadedFunctionDef ctx)
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

        var guardFunction = GenerateModuleGuardFunction(ctx, clauses);
        var subClauseFunctions = clauses.Select(c => c.Item2);
        SubstituteModuleFunctionDefinitions((ModuleDef)ctx.Parent, [ctx], guardFunction, subClauseFunctions);
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

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        try
        {
            moduleStack.Push(ctx);
            var overloads = ctx.Functions.OfType<OverloadedFunctionDef>().ToArray();
            foreach (var overloadedFuncDef in overloads)
            {
                ProcessOverloadedModuleFunctionDef(overloadedFuncDef);
            }
            return base.VisitModuleDef(ctx);
            
        }
        finally
        {
            moduleStack.Pop();
        }
    }

    internal int clauseCounter;

    internal MethodDef GenerateGuardFunction(OverloadedFunctionDefinition ctx, List<(Expression, MethodDef)> clauses)
    {
        var ifStatements = new List<Statement>();
        FunctionDef fd = (FunctionDef)new FunctionDefBuilder()
               .WithName(ctx.Name)
               .WithReturnType(ctx.Type)
               .WithBody(null)
               .WithParams(ctx.OverloadClauses.First().Params)
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
    internal Expression GetPrecondition(IOverloadableFunction clause)
    {
        var conditions = new Queue<Expression>();
        foreach (var pd in clause.Params)
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
    internal MethodDef GetSubclauseFunction(IOverloadableFunction clause)
    {
        var fd = new FunctionDefBuilder()
            .WithName(MemberName.From($"{clause.Name.Value}_subclause{clauseCounter}"))
                 .WithBody(clause.Body)
                 .WithParams(clause.Params)
                 .WithReturnType(clause.ReturnType)
                 .Build();

        // Handle specific properties based on the concrete type
        MethodDef result;
        switch (clause)
        {
            case MethodDef methodDef:
                fd.Parent = methodDef.Parent;
                result = new MethodDef()
                {
                    FunctionDef = fd,
                    Name = fd.Name,
                    Type = fd.ReturnType,
                    TypeName = fd.ReturnType.Name,
                    Visibility = methodDef.Visibility,
                    IsReadOnly = methodDef.IsReadOnly,
                    Parent = methodDef.Parent
                };
                break;
            case FunctionDef functionDef:
                fd.Parent = functionDef.Parent;
                result = new MethodDef()
                {
                    FunctionDef = fd,
                    Name = fd.Name,
                    Type = fd.ReturnType,
                    TypeName = fd.ReturnType.Name,
                    Visibility = functionDef.Visibility,
                    IsReadOnly = false, // FunctionDef doesn't have IsReadOnly
                    Parent = functionDef.Parent
                };
                break;
            default:
                throw new ArgumentException($"Unsupported overloadable function type: {clause.GetType()}");
        }

        return result;
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

    internal FunctionDef GenerateModuleGuardFunction(OverloadedFunctionDef ctx, List<(Expression, MethodDef)> clauses)
    {
        var ifStatements = new List<Statement>();
        FunctionDef fd = (FunctionDef)new FunctionDefBuilder()
               .WithName(ctx.Name)
               .WithReturnType(ctx.ReturnType)
               .WithBody(null)
               .WithParams(ctx.OverloadClauses.First().Params)
               .WithIsStatic(ctx.IsStatic)
               .WithIsConstructor(ctx.IsConstructor)
               .Build()
               .WithSameParentAs(ctx);

        foreach (var clause in clauses)
        {
            var args = (from p in clause.Item2.FunctionDef.Params
                        select (Expression)new VarRefExp() { VarName = p.Name, Parent = p as IAstThing }).ToList();

            var funcCallExpression = new FuncCallExp()
            {
                Annotations = [],
                InvocationArguments = args,
                FunctionDef = clause.Item2.FunctionDef
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

        return fd;
    }

    internal void SubstituteModuleFunctionDefinitions(ModuleDef owner, IEnumerable<OverloadedFunctionDef> functionsToRemove, FunctionDef guardFunction, IEnumerable<MethodDef> subClauseFunctions)
    {
        if (!moduleStack.Any())
        {
            throw new ArgumentException("No module in scope.");
        }
        var module = moduleStack.Peek();
        module.Functions.Remove(functionsToRemove.First());
        
        // Add the guard function
        module.Functions.Add(guardFunction);
        
        // Add sub-clause functions as wrapper MethodDefs converted to FunctionDefs
        foreach (var methodDef in subClauseFunctions)
        {
            module.Functions.Add(methodDef.FunctionDef);
        }
    }
}
