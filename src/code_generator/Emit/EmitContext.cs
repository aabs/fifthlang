using il_ast;

namespace code_generator.Emit;

/// <summary>
/// Encapsulates per-function emission state for IL code generation.
/// Manages label counters, temporary variable counters, parameter/local types, and current context references.
/// </summary>
public class EmitContext
{
    /// <summary>
    /// Counter for generating unique label names within a function
    /// </summary>
    public int LabelCounter { get; set; }

    /// <summary>
    /// Counter for generating unique temporary local variable names within a function
    /// </summary>
    public int TempCounter { get; set; }

    /// <summary>
    /// Names of parameters in the current function scope
    /// </summary>
    public HashSet<string> ParameterNames { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Types of parameters by name in the current function
    /// </summary>
    public Dictionary<string, Type> ParameterTypes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Types of local variables by name in the current function
    /// </summary>
    public Dictionary<string, Type> LocalVariableTypes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Current assembly being generated
    /// </summary>
    public AssemblyDeclaration? CurrentAssembly { get; set; }

    /// <summary>
    /// Current module being generated
    /// </summary>
    public ModuleDeclaration? CurrentModule { get; set; }

    /// <summary>
    /// Current class being generated
    /// </summary>
    public ClassDefinition? CurrentClass { get; set; }

    /// <summary>
    /// Current method return type for lowering diagnostics and inference
    /// </summary>
    public TypeReference? CurrentReturnType { get; set; }

    /// <summary>
    /// Current method name for diagnostics
    /// </summary>
    public string? CurrentMethodName { get; set; }

    /// <summary>
    /// Creates a new EmitContext with default values
    /// </summary>
    public EmitContext()
    {
        LabelCounter = 0;
        TempCounter = 0;
    }

    /// <summary>
    /// Resets per-method state (counters, parameters, locals) for a new function
    /// </summary>
    public void ResetMethodContext()
    {
        LabelCounter = 0;
        TempCounter = 0;
        ParameterNames.Clear();
        ParameterTypes.Clear();
        LocalVariableTypes.Clear();
        CurrentReturnType = null;
        CurrentMethodName = null;
    }

    /// <summary>
    /// Generates a unique label name using the current counter
    /// </summary>
    public string GenerateLabel(string prefix = "IL")
    {
        return $"{prefix}_{LabelCounter++}";
    }

    /// <summary>
    /// Generates a unique temporary variable name using the current counter
    /// </summary>
    public string GenerateTemp(string prefix = "temp")
    {
        return $"{prefix}_{TempCounter++}";
    }
}
