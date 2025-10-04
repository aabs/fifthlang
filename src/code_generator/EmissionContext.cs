using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using il_ast;

namespace code_generator;

/// <summary>
/// Context object that encapsulates all state needed during PE emission.
/// This allows passing state explicitly rather than sharing mutable state across classes.
/// </summary>
public class EmissionContext
{
    /// <summary>
    /// Metadata builder for the current assembly
    /// </summary>
    public required MetadataBuilder MetadataBuilder { get; init; }
    
    /// <summary>
    /// Manager for metadata lookups and registrations
    /// </summary>
    public required MetadataManager MetadataManager { get; init; }
    
    /// <summary>
    /// Map of method names to their definition handles
    /// </summary>
    public Dictionary<string, MethodDefinitionHandle> MethodMap { get; init; } = new();
    
    /// <summary>
    /// Map of method names to their parameter names
    /// </summary>
    public Dictionary<string, List<string>> MethodParamNames { get; init; } = new();
    
    /// <summary>
    /// Per-method state: ordered list of local variable names
    /// </summary>
    public List<string>? CurrentLocalVarNames { get; set; }
    
    /// <summary>
    /// Per-method state: map of parameter names to their indices
    /// </summary>
    public Dictionary<string, int>? CurrentParamIndexMap { get; set; }
    
    /// <summary>
    /// Per-method state: map of labels to their handles
    /// </summary>
    public Dictionary<string, LabelHandle>? CurrentLabelMap { get; set; }
    
    /// <summary>
    /// Per-method state: map of method names to their member references
    /// </summary>
    public Dictionary<string, EntityHandle>? CurrentMethodMemberRefMap { get; set; }
    
    /// <summary>
    /// Reset per-method state before processing a new method
    /// </summary>
    public void ResetMethodState()
    {
        CurrentLocalVarNames = null;
        CurrentParamIndexMap = null;
        CurrentLabelMap = null;
        CurrentMethodMemberRefMap = null;
        MetadataManager.ResetMethodState();
    }
}
