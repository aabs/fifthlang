using ast;

namespace compiler.SemanticAnalysis;

/// <summary>
/// Resolves constructor calls in ObjectInitializerExp nodes by matching arguments to constructor signatures.
/// Implements overload resolution with ranking: exact match > convertible > widening.
/// NOTE: This is currently a stub implementation. Full constructor resolution requires symbol table
/// support which comes later in the compilation pipeline. This will be fully implemented in Phase 5.
/// </summary>
public class ConstructorResolver : BaseAstVisitor
{
    private readonly List<compiler.Diagnostic> _diagnostics = new();
    private readonly TypeRegistry _typeRegistry;

    public ConstructorResolver(TypeRegistry typeRegistry)
    {
        _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
    }

    public IReadOnlyList<compiler.Diagnostic> Diagnostics => _diagnostics;

    public override void EnterObjectInitializerExp(ObjectInitializerExp ctx)
    {
        // TODO: Implement proper constructor resolution once symbol tables are available
        // This requires:
        // 1. Class declaration lookup from symbol table  
        // 2. Constructor signature matching with overload resolution
        // 3. Type compatibility checking for arguments
        // 4. Diagnostic emission for CTOR001 (no matching constructor)
        // 5. Diagnostic emission for CTOR002 (ambiguous constructor call)
        // 6. Setting ctx.ResolvedConstructor to the selected constructor
        
        base.EnterObjectInitializerExp(ctx);
    }
}
