namespace Fifth.LangProcessingPhases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using AST.Visitors;
    using TypeSystem;

    public class OverloadGatheringVisitor : BaseAstVisitor
    {
        public override void EnterFifthProgram(FifthProgram ctx)
        {
            Gather(ctx);
        }

        public override void EnterClassDefinition(ClassDefinition ctx)
        {
            Gather(ctx);
        }

        void Gather(IFunctionCollection ctx)
        {
            var overloads = GatherOverloads(ctx);
            foreach (var fs in overloads)
            {
                if (fs.Value.Count > 1)
                {
                    var overloadedFunction = TransformOverloadGroup(fs.Key, fs.Value);
                    SubstituteFunctionDefinitions(ctx, fs.Value, overloadedFunction);
                }
            }
        }

        private IDictionary<IFunctionSignature, List<IFunctionDefinition>> GatherOverloads(IFunctionCollection classDefinition)
        {
            var result = new Dictionary<IFunctionSignature, List<IFunctionDefinition>>();
            var x = classDefinition.Functions.GroupBy(f => f.GetFunctionSignature(), f => f, new SignaturesAreEqual());
            foreach (var f in classDefinition.Functions)
            {
                var signature = f.GetFunctionSignature();
                
                if (!result.ContainsKey(signature))
                {
                    result[signature] = new List<IFunctionDefinition>();
                }

                result[signature].Add(f);
            }

            return result;
        }

        private void SubstituteFunctionDefinitions(IFunctionCollection classDefinition, List<IFunctionDefinition> fdg, IFunctionDefinition combinedFunction)
        {
            foreach (var fd in fdg)
            {
                classDefinition.RemoveFunction(fd);
            }
            classDefinition.AddFunction(combinedFunction);
        }

        /// <summary>
        /// Combine all the different clauses for a set of overloaded functions into a OverloadedFunctionDefinition (which can be further transformed presently)
        /// </summary>
        /// <param name="functionSignature">the common signature on which this overload group is based</param>
        /// <param name="functionDefinitions">the original functions</param>
        /// <returns></returns>
        private IFunctionDefinition TransformOverloadGroup(IFunctionSignature functionSignature,
            List<IFunctionDefinition> functionDefinitions)
        {
            var orderedFuns = functionDefinitions.OrderBy(fd => fd.Line).ToList();
            var firstClause = orderedFuns.First();
            var result = new OverloadedFunctionDefinition(orderedFuns, functionSignature)
                         .CameFromSameSourceLocation(firstClause)
                         .HasSameParentAs(functionDefinitions.First());
            result.Name = firstClause.Name;
            result.ReturnType = firstClause.ReturnType;
            result.Typename = firstClause.Typename;
            var ctx = functionDefinitions.Last();
            var paramDecls = new List<IParameterListItem>();
            if (ctx.ParameterDeclarations?.ParameterDeclarations.Any() ?? false)
            {
                foreach (var pd in ctx.ParameterDeclarations.ParameterDeclarations)
                {
                    paramDecls.Add(new ParameterDeclaration(new Identifier(pd.ParameterName.Value), pd.TypeName, null, null));
                }
            }

            result.ParameterDeclarations = new ParameterDeclarationList(paramDecls);
            return result;
        }
    }
}
