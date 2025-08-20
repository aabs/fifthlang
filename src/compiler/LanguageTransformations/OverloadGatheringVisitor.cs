using System.Linq;

namespace compiler.LanguageTransformations;

public class OverloadGatheringVisitor : DefaultRecursiveDescentVisitor
{
    public void Gather(ClassDef ctx)
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

    public Dictionary<FunctionSignature, List<MethodDef>> GatherOverloads(ClassDef ctx)
    {
        // Dictionary to hold groups of methods by their FunctionSignature
        var methodGroups = new Dictionary<FunctionSignature, List<MethodDef>>();

        // Iterate through the MemberDefs to find MethodDefs
        foreach (var member in ctx.MemberDefs)
        {
            if (member is MethodDef methodDef)
            {
                // Get the FunctionSignature for the MethodDef using the extension method
                var functionSignature = methodDef.ToFunctionSignature();

                // Add the MethodDef to the appropriate group in the dictionary
                if (!methodGroups.ContainsKey(functionSignature))
                {
                    methodGroups[functionSignature] = new List<MethodDef>();
                }
                methodGroups[functionSignature].Add(methodDef);
            }
        }
        return methodGroups;
    }

    public void SubstituteFunctionDefinitions(ClassDef classDefinition, List<MethodDef> fdg, OverloadedFunctionDefinition combinedFunction)
    {
        foreach (var fd in fdg)
        {
            classDefinition.MemberDefs.Remove(fd);
        }
        classDefinition.MemberDefs.Add(combinedFunction);
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        Gather(ctx);
        return base.VisitClassDef(ctx);
    }

    /// <summary>
    /// Combine all the different clauses for a set of overloaded functions into a
    /// OverloadedFunctionDefinition (which can be further transformed presently)
    /// </summary>
    /// <param name="functionSignature">the common signature on which this overload group is based</param>
    /// <param name="mds">the original functions</param>
    /// <returns></returns>
    private OverloadedFunctionDefinition TransformOverloadGroup(FunctionSignature functionSignature, List<MethodDef> mds)
    {
        var orderedFuns = mds.OrderBy(md => md.FunctionDef.Location.Value.Line).ToList();
        var firstClause = orderedFuns.First();
        var result = new OverloadedFunctionDefinitionBuilder()
            .WithName(firstClause.Name)
            .WithSignature(functionSignature)
            .WithVisibility(firstClause.Visibility)
            .WithVisibility(firstClause.Visibility)
            .WithIsReadOnly(firstClause.IsReadOnly)
            .WithOverloadClauses(orderedFuns.Cast<IOverloadableFunction>().ToList())
            .Build();
        result.Parent = firstClause.Parent;
        var ctx = mds.Last();
        return result;
    }
}
