using ast;

namespace compiler.Validation.GuardValidation.Infrastructure;

/// <summary>
/// Represents a group of function overloads with the same name and arity.
/// Manages overload collection and provides utility methods for base case detection
/// and guard analysis.
/// </summary>
internal class FunctionGroup
{
    public string Name { get; }
    public int Arity { get; }
    public List<IOverloadableFunction> Overloads { get; } = new();

    public FunctionGroup(string name, int arity)
    {
        Name = name;
        Arity = arity;
    }

    public void AddOverload(IOverloadableFunction overload)
    {
        Overloads.Add(overload);
    }

    public bool HasAnyGuards()
    {
        return Overloads.Any(o => o.Params.Any(p => p.ParameterConstraint != null));
    }

    public List<IOverloadableFunction> GetBaseOverloads()
    {
        return Overloads
            .Where(o => !o.Params.Any(p => p.ParameterConstraint != null))
            .ToList();
    }
}