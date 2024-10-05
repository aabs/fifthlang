using ast_model;

namespace ast_generator;


public partial class AstBuilderGenerator
{
    public AstBuilderGenerator(ITypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public ITypeProvider TypeProvider { get; set; }
}

public partial class AstVisitors
{
    public AstVisitors(ITypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public ITypeProvider TypeProvider { get; set; }
}
public partial class AstTypeCheckerGenerator
{
    public AstTypeCheckerGenerator(ITypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public ITypeProvider TypeProvider { get; set; }
}
