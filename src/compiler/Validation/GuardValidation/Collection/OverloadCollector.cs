using ast;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation.Collection;

/// <summary>
/// Handles collection of overloaded functions from AST nodes into function groups.
/// Implements the gathering logic that was previously embedded in the visitor methods.
/// </summary>
internal class OverloadCollector
{
    private readonly Dictionary<string, FunctionGroup> _functionGroups = new();

    public IReadOnlyDictionary<string, FunctionGroup> FunctionGroups => _functionGroups;

    /// <summary>
    /// Clears all collected function groups for a new analysis session.
    /// </summary>
    public void Reset()
    {
        _functionGroups.Clear();
    }

    /// <summary>
    /// Collects overloaded functions from a module definition.
    /// </summary>
    public void CollectFromModule(ModuleDef module)
    {
        foreach (var function in module.Functions)
        {
            if (function is OverloadedFunctionDef overloadedFunc)
            {
                CollectOverloadedFunction(overloadedFunc.Name.Value, overloadedFunc.OverloadClauses);
            }
        }
    }

    /// <summary>
    /// Collects overloaded functions from a class definition.
    /// </summary>
    public void CollectFromClass(ClassDef classDef)
    {
        foreach (var member in classDef.MemberDefs)
        {
            if (member is OverloadedFunctionDefinition overloadedFunc)
            {
                CollectOverloadedFunction(overloadedFunc.Name.Value, overloadedFunc.OverloadClauses);
            }
        }
    }

    private void CollectOverloadedFunction(string name, IEnumerable<IOverloadableFunction> clauses)
    {
        var groupKey = CreateGroupKey(name, clauses);
        if (!_functionGroups.TryGetValue(groupKey, out var group))
        {
            group = new FunctionGroup(name, GetArity(clauses.FirstOrDefault()));
            _functionGroups[groupKey] = group;
        }

        foreach (var clause in clauses)
        {
            group.AddOverload(clause);
        }
    }

    private string CreateGroupKey(string name, IEnumerable<IOverloadableFunction> clauses)
    {
        // Group by name + parameter types (FR-033)
        var firstClause = clauses.FirstOrDefault();
        if (firstClause == null)
            return name;

        var paramTypeNames = firstClause.Params
            .Select(p => p.TypeName.Value ?? "unknown")
            .ToList();

        return $"{name}({string.Join(",", paramTypeNames)})";
    }

    private int GetArity(IOverloadableFunction? clause)
    {
        return clause?.Params?.Count ?? 0;
    }
}