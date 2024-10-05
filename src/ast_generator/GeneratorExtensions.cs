using ast_model;

namespace ast_generator;


public partial class AstBuilderGenerator
{
    public AstBuilderGenerator(TypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public TypeProvider TypeProvider { get; set; }
}

public partial class AstVisitors
{
    public AstVisitors(TypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public TypeProvider TypeProvider { get; set; }
}
public partial class AstTypeCheckerGenerator
{
    public AstTypeCheckerGenerator(TypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(typeProvider);
        TypeProvider = typeProvider;
    }
    public TypeProvider TypeProvider { get; set; }
}
