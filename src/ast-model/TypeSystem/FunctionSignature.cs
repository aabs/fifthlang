using System.Linq;

namespace ast_model.TypeSystem;

public record FunctionSignature : IFunctionSignature
{
    public FifthType[] FormalParameterTypes { get; set; }
    public FifthType[] GenericTypeParameters { get; set; }
    public MemberName Name { get; set; }
    public FifthType ReturnType { get; set; }
    public FifthType? DeclaringType { get; set; }

    public virtual bool Equals(FunctionSignature? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        var thisParams = FormalParameterTypes ?? Array.Empty<FifthType>();
        var otherParams = other.FormalParameterTypes ?? Array.Empty<FifthType>();

        return EqualityComparer<MemberName>.Default.Equals(Name, other.Name)
            && EqualityComparer<FifthType>.Default.Equals(ReturnType, other.ReturnType)
            && thisParams.SequenceEqual(otherParams);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ReturnType);
        var ps = FormalParameterTypes ?? Array.Empty<FifthType>();
        foreach (var p in ps)
            hash.Add(p);
        return hash.ToHashCode();
    }
}
