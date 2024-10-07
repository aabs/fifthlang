namespace Fifth.LangProcessingPhases
{
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using AST.Builders;
    using AST.Visitors;

    /// <summary>
    /// Convert a multi-clause function into simpler form.
    /// </summary>
    public class OverloadTransformingVisitor : BaseAstVisitor
    {
        private int clauseCounter;

        public override void EnterFifthProgram(FifthProgram ctx)
        {
            var overloads = ctx.Functions.Where(f => f is OverloadedFunctionDefinition).ToArray();
            for (var i = 0; i < overloads.Length; i++)
            {
                ProcessOverloadedFunctionDefinition(overloads[i] as OverloadedFunctionDefinition);
            }
        }

        public void ProcessOverloadedFunctionDefinition(OverloadedFunctionDefinition ctx)
        {
            clauseCounter = 1;
            var clauses = new List<(Expression, IFunctionDefinition)>();
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
            SubstituteFunctionDefinitions((IFunctionCollection)ctx.ParentNode, new[] { ctx }, newFunctions);
        }

        private IFunctionDefinition GenerateGuardFunction(OverloadedFunctionDefinition ctx, List<(Expression, IFunctionDefinition)> clauses)
        {
            var ifStatements = new List<Statement>();
            foreach (var clause in clauses)
            {
                /*
                 var name = context.funcname.GetText();
                    var actualParams = (ExpressionList)VisitExplist(context.args);
                    return new FuncCallExpression(actualParams, name)
                 */
                var args = clause
                           .Item2
                           .ParameterDeclarations
                           .ParameterDeclarations
                           .Select(pd => new VariableReference(pd.ParameterName.Value).HasSameParentAs(pd as IAstNode))
                           .Cast<Expression>()
                           .ToList();
                var funcCallExpression = new FuncCallExpression(new ExpressionList(args), clause.Item2.Name);
                if (clause.Item1 != null)
                {
                    var ifBlock = BlockBuilder.CreateBlock()
                                              .WithStatement(new ReturnStatement(funcCallExpression,
                                                  clause.Item2.ReturnType))
                                              .Build();
                    var ieb = IfElseStatementBuilder.CreateIfElseStatement()
                                           .WithCondition(clause.Item1)
                                           .WithIfBlock(ifBlock)
                                           .Build();
                    ifStatements.Add(ieb);
                }
                else
                {
                    // if there is no clause condition, it must be the base case (which should be the last clause)
                    ifStatements.Add(new ExpressionStatement(funcCallExpression));
                }
            }
            var body = new Block(ifStatements);

            // TODO: Use FunctionDefinitionBuilder
            var fb = FunctionBuilder
                   .NewFunction()
                   .Called(ctx.Name)
                   .WithReturnType(ctx.Typename)
                   .WithBody(body)
                   .WithSameParentAs(ctx);
            if (ctx.ParameterDeclarations?.ParameterDeclarations.Any() ?? false)
            {
                foreach (var pd in ctx.ParameterDeclarations.ParameterDeclarations)
                {
                    fb.WithParam(pd.ParameterName.Value, pd.TypeName);
                }
            }

            return fb.AsAstNode();
        }

        /// <summary>
        /// extract the preconditions for entry into a subclause
        /// </summary>
        /// <param name="clause">the original function with preconditions</param>
        /// <returns>an expression combining all preconditions</returns>
        private Expression GetPrecondition(IFunctionDefinition clause)
        {
            var conditions = new Queue<Expression>();
            foreach (var pd in clause.ParameterDeclarations.ParameterDeclarations)
            {
                if (pd.Constraint != null)
                {
                    conditions.Enqueue(pd.Constraint);
                }
            }

            Expression e = null;
            while (conditions.Count > 0)
            {
                if (e == null)
                {
                    e = conditions.Dequeue();
                }
                else
                {
                    e = new BinaryExpression(e, Operator.And, conditions.Dequeue());
                }
            }

            return e;
        }

        /// <summary>
        /// transforms the function into a form that can be envoked from a dispatcher guard function
        /// </summary>
        /// <param name="clause">the original function to transform</param>
        /// <returns>a new function</returns>
        private IFunctionDefinition GetSubclauseFunction(IFunctionDefinition clause)
        {
            var fb = FunctionBuilder
                     .NewFunction()
                     .Called($"{clause.Name}_subclause{clauseCounter}")
                     .WithBody(clause.Body)
                     .WithReturnType(clause.Typename)
                     .WithSameParentAs((AstNode)clause);
            foreach (var pd in clause.ParameterDeclarations.ParameterDeclarations)
            {
                fb.WithParam(pd.ParameterName.Value, pd.TypeName);
            }
            // get all the bindings
            // create a new function with unique name
            // normalise param list and insert into function
            // add binding var decls to front of body
            // insert body of old function into new function
            return fb.AsAstNode();
        }

        private void SubstituteFunctionDefinitions(IFunctionCollection functionContainer, IEnumerable<IFunctionDefinition> functionsToRemove, IEnumerable<IFunctionDefinition> functionsToAdd)
        {
            foreach (var fd in functionsToRemove)
            {
                functionContainer.RemoveFunction(fd);
            }
            foreach (var fd in functionsToAdd)
            {
                functionContainer.AddFunction(fd);
            }
        }
    }
}
