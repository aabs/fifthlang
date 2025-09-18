using ast;
using compiler;
using System.Text;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// Validates completeness of guarded function overload sets according to specification
/// 002-guard-clause-overload-completeness.
/// 
/// Ensures that:
/// - Functions with guards have a base case or exhaustive coverage
/// - Detects unreachable guards (subsumption)
/// - Validates overload ordering (base case must be last)
/// - Emits appropriate diagnostics (E1001-E1005, W1101-W1102)
/// </summary>
public class GuardCompletenessValidator : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly Dictionary<string, FunctionGroup> _functionGroups = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        // Clear state for new assembly
        _diagnostics.Clear();
        _functionGroups.Clear();

        // Process the assembly
        var result = base.VisitAssemblyDef(ctx);

        // Validate all collected function groups
        ValidateAllFunctionGroups();

        return result;
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        // Collect overloaded functions from this module
        foreach (var function in ctx.Functions)
        {
            if (function is OverloadedFunctionDef overloadedFunc)
            {
                var groupKey = CreateGroupKey(overloadedFunc.Name.Value, overloadedFunc.OverloadClauses);
                if (!_functionGroups.TryGetValue(groupKey, out var group))
                {
                    group = new FunctionGroup(overloadedFunc.Name.Value, GetArity(overloadedFunc.OverloadClauses.FirstOrDefault()));
                    _functionGroups[groupKey] = group;
                }

                foreach (var clause in overloadedFunc.OverloadClauses)
                {
                    group.AddOverload(clause);
                }
            }
        }

        return base.VisitModuleDef(ctx);
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        // Collect overloaded functions from this class
        foreach (var member in ctx.MemberDefs)
        {
            if (member is OverloadedFunctionDefinition overloadedFunc)
            {
                var groupKey = CreateGroupKey(overloadedFunc.Name.Value, overloadedFunc.OverloadClauses);
                if (!_functionGroups.TryGetValue(groupKey, out var group))
                {
                    group = new FunctionGroup(overloadedFunc.Name.Value, GetArity(overloadedFunc.OverloadClauses.FirstOrDefault()));
                    _functionGroups[groupKey] = group;
                }

                foreach (var clause in overloadedFunc.OverloadClauses)
                {
                    group.AddOverload(clause);
                }
            }
        }

        return base.VisitClassDef(ctx);
    }

    private void ValidateAllFunctionGroups()
    {
        foreach (var group in _functionGroups.Values)
        {
            ValidateFunctionGroup(group);
        }
    }

    private void ValidateFunctionGroup(FunctionGroup group)
    {
        // Skip validation for groups with no guards
        if (!group.HasAnyGuards())
        {
            return;
        }

        // FR-050/061: Emit overload count warning for groups >= 33 overloads
        if (group.Overloads.Count >= 33)
        {
            EmitOverloadCountWarning(group);
        }

        // Check for multiple base overloads (FR-034/066)
        var baseOverloads = group.GetBaseOverloads();
        if (baseOverloads.Count > 1)
        {
            EmitMultipleBaseError(group, baseOverloads);
            return; // Priority over other diagnostics per FR-053
        }

        // Check base overload ordering (FR-035)
        var firstBase = baseOverloads.FirstOrDefault();
        if (firstBase != null)
        {
            ValidateBaseOrdering(group, firstBase);
        }

        // Analyze predicate coverage and reachability
        AnalyzePredicateCoverage(group);
    }

    private void AnalyzePredicateCoverage(FunctionGroup group)
    {
        var analyzedOverloads = new List<AnalyzedOverload>();
        var unknownCount = 0;

        // Step 1: Analyze each overload's predicate
        foreach (var overload in group.Overloads)
        {
            var analyzed = AnalyzeOverload(overload);
            analyzedOverloads.Add(analyzed);
            
            if (analyzed.PredicateType == PredicateType.Unknown)
            {
                unknownCount++;
            }
        }

        // Step 2: Check for unreachable overloads (FR-026)
        CheckForUnreachableOverloads(group, analyzedOverloads);

        // Step 3: Check completeness (only if no base case)
        var hasBase = group.GetBaseOverloads().Any();
        if (!hasBase)
        {
            CheckCompleteness(group, analyzedOverloads);
        }

        // Step 4: Check for UNKNOWN explosion (FR-051/062/064)
        if (!hasBase && group.Overloads.Count >= 8)
        {
            var unknownPercent = (unknownCount * 100) / group.Overloads.Count;
            if (unknownPercent > 50)
            {
                EmitUnknownExplosionWarning(group, unknownPercent);
            }
        }
    }

    private AnalyzedOverload AnalyzeOverload(IOverloadableFunction overload)
    {
        var predicateType = ClassifyPredicate(overload);
        var predicateDescriptor = CreatePredicateDescriptor(overload, predicateType);
        
        return new AnalyzedOverload(overload, predicateType, predicateDescriptor);
    }

    private PredicateType ClassifyPredicate(IOverloadableFunction overload)
    {
        // Check if it's a base case (no constraints)
        if (!HasAnyConstraints(overload))
        {
            return PredicateType.Base;
        }

        // Try to normalize constraints
        var constraints = GetAllConstraints(overload);
        if (constraints.Count == 0)
        {
            return PredicateType.Base;
        }

        // Check for tautology (FR-052/055)
        if (constraints.Count == 1 && IsTautology(constraints[0]))
        {
            return PredicateType.Base;
        }

        // Attempt normalization per FR-038
        if (CanNormalizeToConjunction(constraints))
        {
            return PredicateType.Analyzable;
        }

        return PredicateType.Unknown;
    }

    private bool HasAnyConstraints(IOverloadableFunction overload)
    {
        return overload.Params.Any(p => p.ParameterConstraint != null);
    }

    private List<Expression> GetAllConstraints(IOverloadableFunction overload)
    {
        var constraints = new List<Expression>();
        foreach (var param in overload.Params)
        {
            if (param.ParameterConstraint != null)
            {
                constraints.Add(param.ParameterConstraint);
            }
        }
        return constraints;
    }

    private bool IsTautology(Expression expr)
    {
        // FR-055: Limited to literal true, parenthesized true, or compile-time boolean constant
        return expr switch
        {
            BooleanLiteralExp { Value: true } => true,
            // TODO: Handle parenthesized true and compile-time constants
            _ => false
        };
    }

    private bool CanNormalizeToConjunction(List<Expression> constraints)
    {
        // FR-038: Check if all constraints can be normalized to analyzable form
        foreach (var constraint in constraints)
        {
            if (!IsAnalyzableConstraint(constraint))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsAnalyzableConstraint(Expression expr)
    {
        // FR-038: Analyzable patterns
        return expr switch
        {
            BinaryExp { Operator: Operator.Equal } => true, // equality
            BinaryExp { Operator: Operator.LessThan or Operator.LessThanOrEqual or 
                                    Operator.GreaterThan or Operator.GreaterThanOrEqual } => true, // comparisons
            BinaryExp { Operator: Operator.LogicalAnd } be => 
                IsAnalyzableConstraint(be.LHS) && IsAnalyzableConstraint(be.RHS), // conjunction
            // TODO: Add more patterns like destructuring field bindings
            _ => false
        };
    }

    private PredicateDescriptor CreatePredicateDescriptor(IOverloadableFunction overload, PredicateType type)
    {
        return type switch
        {
            PredicateType.Base => PredicateDescriptor.Always,
            PredicateType.Unknown => PredicateDescriptor.Unknown,
            PredicateType.Analyzable => CreateAnalyzableDescriptor(overload),
            _ => PredicateDescriptor.Unknown
        };
    }

    private PredicateDescriptor CreateAnalyzableDescriptor(IOverloadableFunction overload)
    {
        // TODO: Implement detailed predicate analysis
        // For now, return a simple conjunction placeholder
        var constraints = GetAllConstraints(overload);
        return new PredicateDescriptor(PredicateType.Analyzable, constraints);
    }

    private void CheckForUnreachableOverloads(FunctionGroup group, List<AnalyzedOverload> analyzedOverloads)
    {
        for (int i = 0; i < analyzedOverloads.Count; i++)
        {
            var current = analyzedOverloads[i];
            
            // Check if this overload is subsumed by any earlier overload
            for (int j = 0; j < i; j++)
            {
                var earlier = analyzedOverloads[j];
                
                if (IsSubsumed(current, earlier))
                {
                    EmitUnreachableWarning(group, current.Overload, earlier.Overload, i + 1, j + 1);
                    break; // Only report first subsumption
                }
            }
        }
    }

    private bool IsSubsumed(AnalyzedOverload current, AnalyzedOverload earlier)
    {
        // Simplified subsumption check
        if (earlier.PredicateType == PredicateType.Base)
        {
            return true; // Base case subsumes everything after it
        }

        // TODO: Implement detailed subsumption analysis for intervals, etc.
        return false;
    }

    private void CheckCompleteness(FunctionGroup group, List<AnalyzedOverload> analyzedOverloads)
    {
        // Simple heuristic: if we have any unknown predicates and no base, it's incomplete
        var hasAnalyzable = analyzedOverloads.Any(a => a.PredicateType == PredicateType.Analyzable);
        var hasUnknown = analyzedOverloads.Any(a => a.PredicateType == PredicateType.Unknown);
        
        if (hasUnknown || !hasAnalyzable)
        {
            EmitIncompleteError(group);
        }
    }

    #region Diagnostic Emission

    private void EmitIncompleteError(FunctionGroup group)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Error,
            $"GUARD_INCOMPLETE (E1001): Function '{group.Name}/{group.Arity}' has guarded overloads but no base case and guards are not exhaustive."
        );
        _diagnostics.Add(diagnostic);
    }

    private void EmitUnreachableWarning(FunctionGroup group, IOverloadableFunction unreachable, IOverloadableFunction covering, int unreachableIndex, int coveringIndex)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_UNREACHABLE (W1002): Overload #{unreachableIndex} for function '{group.Name}/{group.Arity}' is unreachable (covered by previous guards)."
        );
        _diagnostics.Add(diagnostic);

        // Secondary note per FR-036
        var note = new Diagnostic(
            DiagnosticLevel.Info,
            $"note: overload #{unreachableIndex} unreachable due to earlier coverage by overload #{coveringIndex}"
        );
        _diagnostics.Add(note);
    }

    private void EmitMultipleBaseError(FunctionGroup group, List<IOverloadableFunction> baseOverloads)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Error,
            $"GUARD_MULTIPLE_BASE (E1005): Multiple unguarded base overloads detected for function '{group.Name}/{group.Arity}'. Only one final base overload is permitted."
        );
        _diagnostics.Add(diagnostic);

        // Secondary notes for additional bases per FR-036
        for (int i = 1; i < baseOverloads.Count; i++)
        {
            var note = new Diagnostic(
                DiagnosticLevel.Info,
                $"note: extra base overload ignored; base already declared at earlier position"
            );
            _diagnostics.Add(note);
        }
    }

    private void ValidateBaseOrdering(FunctionGroup group, IOverloadableFunction baseOverload)
    {
        // FR-035: Base must be last
        var baseIndex = group.Overloads.IndexOf(baseOverload);
        if (baseIndex >= 0 && baseIndex < group.Overloads.Count - 1)
        {
            var diagnostic = new Diagnostic(
                DiagnosticLevel.Error,
                $"GUARD_BASE_NOT_LAST (E1004): Base (unguarded) overload for function '{group.Name}/{group.Arity}' must be the final overload; subsequent overload at #{baseIndex + 2} is invalid."
            );
            _diagnostics.Add(diagnostic);

            // Secondary note for invalid subsequent overload
            var note = new Diagnostic(
                DiagnosticLevel.Info,
                $"note: overload #{baseIndex + 2} invalid because base overload terminates overloading at #{baseIndex + 1}"
            );
            _diagnostics.Add(note);
        }
    }

    private void EmitOverloadCountWarning(FunctionGroup group)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_OVERLOAD_COUNT (W1101): Overload group for '{group.Name}/{group.Arity}' exceeds recommended maximum of 32 (found {group.Overloads.Count}). Analysis precision may degrade."
        );
        _diagnostics.Add(diagnostic);
    }

    private void EmitUnknownExplosionWarning(FunctionGroup group, int unknownPercent)
    {
        var diagnostic = new Diagnostic(
            DiagnosticLevel.Warning,
            $"GUARD_UNKNOWN_EXPLOSION (W1102): Overload group for '{group.Name}/{group.Arity}' has excessive UNKNOWN guards ({unknownPercent}%). Refactor or add base case for clarity."
        );
        _diagnostics.Add(diagnostic);
    }

    #endregion

    #region Utility Methods

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

    #endregion
}

#region Supporting Types

public class FunctionGroup
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

public record AnalyzedOverload(
    IOverloadableFunction Overload,
    PredicateType PredicateType,
    PredicateDescriptor PredicateDescriptor
);

public enum PredicateType
{
    Base,       // No constraints or tautology
    Analyzable, // Can be normalized to conjunction of atomic predicates
    Unknown     // Cannot be analyzed
}

public class PredicateDescriptor
{
    public static readonly PredicateDescriptor Always = new(PredicateType.Base, new List<Expression>());
    public static readonly PredicateDescriptor Unknown = new(PredicateType.Unknown, new List<Expression>());

    public PredicateType Type { get; }
    public List<Expression> Constraints { get; }

    public PredicateDescriptor(PredicateType type, List<Expression> constraints)
    {
        Type = type;
        Constraints = constraints;
    }
}

#endregion