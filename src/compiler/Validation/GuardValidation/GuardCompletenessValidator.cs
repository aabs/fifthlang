using ast;
using compiler;
using compiler.Validation.GuardValidation.Collection;
using compiler.Validation.GuardValidation.Normalization;
using compiler.Validation.GuardValidation.Analysis;
using compiler.Validation.GuardValidation.Diagnostics;
using compiler.Validation.GuardValidation.Instrumentation;
using compiler.Validation.GuardValidation.Infrastructure;

namespace compiler.Validation.GuardValidation;

/// <summary>
/// Validates completeness of guarded function overload sets according to specification
/// 002-guard-clause-overload-completeness.
/// 
/// Orchestrates the validation process using modular components:
/// - OverloadCollector: Gathers function groups from AST
/// - PredicateNormalizer: Analyzes and classifies predicates  
/// - CompletenessAnalyzer: Checks coverage and reachability
/// - DiagnosticEmitter: Emits appropriate diagnostics (E1001-E1005, W1101-W1102)
/// - ValidationInstrumenter: Tracks performance metrics
/// </summary>
public class GuardCompletenessValidator : DefaultRecursiveDescentVisitor
{
    private readonly OverloadCollector _collector = new();
    private readonly PredicateNormalizer _normalizer = new();
    private readonly CompletenessAnalyzer _analyzer = new();
    private readonly DiagnosticEmitter _emitter = new();
    private readonly ValidationInstrumenter _instrumenter = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _emitter.Diagnostics;

    public override AssemblyDef VisitAssemblyDef(AssemblyDef ctx)
    {
        // Initialize for new assembly analysis
        _collector.Reset();
        _emitter.Reset();
        _instrumenter.Reset();

        _instrumenter.StartPhase("AssemblyAnalysis");

        // Process the assembly
        var result = base.VisitAssemblyDef(ctx);

        // Validate all collected function groups
        ValidateAllFunctionGroups();

        _instrumenter.EndPhase("AssemblyAnalysis");

        return result;
    }

    public override ModuleDef VisitModuleDef(ModuleDef ctx)
    {
        _instrumenter.StartPhase("ModuleCollection");

        // Collect overloaded functions from this module
        _collector.CollectFromModule(ctx);

        _instrumenter.EndPhase("ModuleCollection");

        return base.VisitModuleDef(ctx);
    }

    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        _instrumenter.StartPhase("ClassCollection");

        // Collect overloaded functions from this class
        _collector.CollectFromClass(ctx);

        _instrumenter.EndPhase("ClassCollection");

        return base.VisitClassDef(ctx);
    }

    private void ValidateAllFunctionGroups()
    {
        _instrumenter.StartPhase("GroupValidation");

        var groups = _collector.FunctionGroups.Values.ToList();
        var totalOverloads = groups.Sum(g => g.Overloads.Count);

        _instrumenter.RecordFunctionGroupMetrics(groups.Count, totalOverloads);

        foreach (var group in groups)
        {
            ValidateFunctionGroup(group);
        }

        _instrumenter.EndPhase("GroupValidation");
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
            _emitter.EmitOverloadCountWarning(group);
        }

        // Check for multiple base overloads (FR-034/066)
        var baseOverloads = group.GetBaseOverloads();
        if (baseOverloads.Count > 1)
        {
            _emitter.EmitMultipleBaseError(group, baseOverloads);
            return; // Priority over other diagnostics per FR-053
        }

        // Check base overload ordering (FR-035)
        var firstBase = baseOverloads.FirstOrDefault();
        if (firstBase != null)
        {
            var invalidIndex = _analyzer.ValidateBaseOrdering(group, firstBase);
            if (invalidIndex.HasValue)
            {
                var baseIndex = group.Overloads.IndexOf(firstBase);
                _emitter.EmitBaseNotLastError(group, baseIndex, invalidIndex.Value);
            }
        }

        // Analyze predicate coverage and reachability
        AnalyzePredicateCoverage(group);
    }

    private void AnalyzePredicateCoverage(FunctionGroup group)
    {
        _instrumenter.StartPhase("PredicateAnalysis");

        var analyzedOverloads = new List<AnalyzedOverload>();

        // Step 1: Analyze each overload's predicate
        foreach (var overload in group.Overloads)
        {
            var analyzed = _normalizer.AnalyzeOverload(overload);
            analyzedOverloads.Add(analyzed);
        }

        // Step 2: Check for unreachable overloads (FR-026)
        var unreachableOverloads = _analyzer.CheckForUnreachableOverloads(analyzedOverloads);
        foreach (var (unreachableIndex, coveringIndex, unreachable, covering) in unreachableOverloads)
        {
            _emitter.EmitUnreachableWarning(group, unreachable.Overload, covering.Overload, unreachableIndex, coveringIndex);
        }

        // Step 3: Check completeness (only if no base case)
        var hasBase = group.GetBaseOverloads().Any();
        if (!hasBase)
        {
            if (!_analyzer.IsComplete(group, analyzedOverloads))
            {
                _emitter.EmitIncompleteError(group);
            }
        }

        // Step 4: Check for UNKNOWN explosion (FR-051/062/064)
        if (!hasBase && group.Overloads.Count >= 8)
        {
            var unknownPercent = _analyzer.CalculateUnknownPercentage(analyzedOverloads);
            if (unknownPercent > 50)
            {
                _emitter.EmitUnknownExplosionWarning(group, unknownPercent);
            }
        }

        _instrumenter.EndPhase("PredicateAnalysis");
    }

}