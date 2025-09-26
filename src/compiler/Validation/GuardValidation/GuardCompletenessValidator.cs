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
    private readonly UnreachableAnalyzer _unreachable = new();
    private readonly DuplicateDetector _duplicates = new();
    private readonly CoverageEvaluator _coverage = new();
    private readonly ExplosionAnalyzer _explosion = new();
    private readonly DiagnosticEmitter _emitter = new();
    private readonly GuardValidationReporter _reporter;
    private readonly BaseOrderingRules _baseRules = new();
    private readonly CountAnalyzer _count = new();
    private readonly ValidationInstrumenter _instrumenter = new();

    public GuardCompletenessValidator()
    {
        _reporter = new GuardValidationReporter(_emitter);
    }

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
        // FR-050/061: Emit overload count warning for groups >= 33 overloads (applies regardless of guards)
        if (_count.ShouldWarn(group.Overloads.Count))
        {
            _emitter.EmitOverloadCountWarning(group);
        }

        // Skip validation for groups with no guards
        if (!group.HasAnyGuards())
        {
            return;
        }

        // Apply base ordering/multiple base rules
        var baseOverloads = group.GetBaseOverloads();
        // Consider tautology-typed constraints (e.g., 'true') as base-equivalent
        var tautologyBases = group.Overloads
            .Where(o => o.Params.Any(p => p.ParameterConstraint != null) && _normalizer.ClassifyPredicate(o) == PredicateType.Base)
            .Where(o => !baseOverloads.Contains(o))
            .ToList();
        // Merge base-like tautology overloads into baseOverloads for precedence checks
        baseOverloads.AddRange(tautologyBases);
        if (baseOverloads.Count > 1)
        {
            _emitter.EmitMultipleBaseError(group, baseOverloads);
            return; // precedence: multiple base suppresses completeness
        }
        _baseRules.Apply(group, _emitter);

        // Analyze predicate coverage and reachability
        AnalyzePredicateCoverage(group);
    }

    private void AnalyzePredicateCoverage(FunctionGroup group)
    {
        _instrumenter.StartPhase("PredicateAnalysis");

        var analyzedOverloads = new List<AnalyzedOverload>();
        var emptyIntervalUnreachables = new List<(int unreachableIndex, int coveringIndex, AnalyzedOverload unreachable, AnalyzedOverload covering)>();
        var unreachableAlready = new HashSet<int>();

        // Step 1: Analyze each overload's predicate
        for (int idx = 0; idx < group.Overloads.Count; idx++)
        {
            var overload = group.Overloads[idx];
            var analyzed = _normalizer.AnalyzeOverload(overload);
            analyzedOverloads.Add(analyzed);

            // FR-040/060: Detect empty/inverted intervals early and report as unreachable
            if (analyzed.PredicateType == PredicateType.Analyzable)
            {
                if (TryGetInterval(analyzed.PredicateDescriptor, out var interval))
                {
                    var ie = new IntervalEngine();
                    if (ie.IsEmpty(interval))
                    {
                        // Report empty interval as unreachable only for non-first overloads
                        if (idx > 0)
                        {
                            var oneBased = idx + 1;
                            emptyIntervalUnreachables.Add((oneBased, 0, analyzed, analyzed));
                            unreachableAlready.Add(oneBased);
                        }
                    }
                }
            }
        }

        // Step 2a: Detect exact-duplicate analyzable predicates and report as unreachable
        var dupePairs = _duplicates.DetectDuplicates(analyzedOverloads);
        if (dupePairs.Count > 0)
        {
            var dupesAsUnreachable = new List<(int, int, AnalyzedOverload, AnalyzedOverload)>();
            foreach (var (firstIndex, duplicateIndex, first, duplicate) in dupePairs)
            {
                // Later duplicate is unreachable due to earlier identical guard
                if (!unreachableAlready.Contains(duplicateIndex))
                {
                    dupesAsUnreachable.Add((duplicateIndex, firstIndex, duplicate, first));
                    unreachableAlready.Add(duplicateIndex);
                }
            }
            _reporter.ReportUnreachable(group, dupesAsUnreachable);
        }

        // Step 2b: Check for unreachable overloads via subsumption (FR-026)
        var unreachableOverloads = _unreachable.Analyze(analyzedOverloads)
            .Where(t => !unreachableAlready.Contains(t.unreachableIndex))
            .ToList();
        foreach (var t in unreachableOverloads) unreachableAlready.Add(t.unreachableIndex);
        // Report empty-interval precedences first (coveringIndex=0 to suppress note), then subsumption
        if (emptyIntervalUnreachables.Count > 0)
        {
            _reporter.ReportUnreachable(group, emptyIntervalUnreachables);
        }
        _reporter.ReportUnreachable(group, unreachableOverloads);

        // Step 3: Check completeness 
        var baseOverloads = group.GetBaseOverloads();
        var hasValidBase = false;

        // Base is valid for completeness only if it exists and is properly positioned (last)
        if (baseOverloads.Count == 1)
        {
            var baseOverload = baseOverloads.First();
            var invalidIndex = _analyzer.ValidateBaseOrdering(group, baseOverload);
            hasValidBase = !invalidIndex.HasValue; // base is last = no invalid subsequent index
        }

        var booleanComplete = _coverage.IsBooleanComplete(analyzedOverloads);
        if (!hasValidBase && !booleanComplete && !_analyzer.IsComplete(group, analyzedOverloads))
        {
            _emitter.EmitIncompleteError(group);
        }

        // Step 4: Check for UNKNOWN explosion (FR-051/062/064)
        if (!hasValidBase && group.Overloads.Count >= 8)
        {
            var unknownPercent = _explosion.CalculateUnknownPercent(analyzedOverloads);
            if (unknownPercent > 50)
            {
                _emitter.EmitUnknownExplosionWarning(group, unknownPercent);
            }
        }

        _instrumenter.EndPhase("PredicateAnalysis");
    }

    // Local helper mirrors analyzer mapping for quick interval check
    private static bool TryGetInterval(PredicateDescriptor descriptor, out Analysis.Interval interval)
    {
        interval = Analysis.Interval.Unbounded();
        var atoms = new List<ast.BinaryExp>();
        foreach (var expr in descriptor.Constraints)
        {
            if (!CollectAtoms(expr, atoms)) return false;
        }
        if (atoms.Count == 0) return false;
        string? varName = null;
        var ie = new Analysis.IntervalEngine();
        foreach (var be in atoms)
        {
            if (be.LHS is ast.VarRefExp v && be.RHS is ast.Int32LiteralExp lit)
            {
                if (varName == null) varName = v.VarName; else if (varName != v.VarName) return false;
                Analysis.Interval atomInterval = be.Operator switch
                {
                    ast.Operator.GreaterThan => new Analysis.Interval(lit.Value, false, null, false),
                    ast.Operator.GreaterThanOrEqual => new Analysis.Interval(lit.Value, true, null, false),
                    ast.Operator.LessThan => new Analysis.Interval(null, false, lit.Value, false),
                    ast.Operator.LessThanOrEqual => new Analysis.Interval(null, false, lit.Value, true),
                    ast.Operator.Equal => Analysis.Interval.Closed(lit.Value, lit.Value),
                    _ => default
                };
                if (Equals(atomInterval, default(Analysis.Interval))) return false;
                interval = ie.Intersect(interval, atomInterval);
            }
            else return false;
        }
        return true;
    }

    private static bool CollectAtoms(ast.Expression expr, List<ast.BinaryExp> atoms)
    {
        if (expr is ast.BinaryExp be)
        {
            if (be.Operator == ast.Operator.LogicalAnd)
                return CollectAtoms(be.LHS, atoms) && CollectAtoms(be.RHS, atoms);
            if (be.Operator == ast.Operator.GreaterThan || be.Operator == ast.Operator.GreaterThanOrEqual ||
                be.Operator == ast.Operator.LessThan || be.Operator == ast.Operator.LessThanOrEqual ||
                be.Operator == ast.Operator.Equal)
            {
                atoms.Add(be);
                return true;
            }
            return false;
        }
        return false;
    }

}