# Refactor Transformation Pipeline to Composable Architecture

**Labels:** `arch-review`, `maintainability`, `performance`, `high`  
**Priority:** P1  
**Severity:** HIGH  
**Epic:** Architectural Improvements Q2 2026

## Problem Summary

The compiler's transformation pipeline consists of 18 sequential phases hardcoded in `ParserManager.ApplyLanguageAnalysisPhases()`. This monolithic design makes the compiler rigid, hard to test, difficult to debug, and prevents optimization opportunities.

## Current Issues

### Monolithic Pipeline
- 18 transformation phases in fixed order (ParserManager.cs:39-170)
- 5,236 lines of transformation code across 19 visitor files
- No ability to skip phases or reorder transformations
- No phase-level caching or optimization
- Complex dependencies between phases not explicit

**Code Evidence:**
```csharp
// src/compiler/ParserManager.cs:39
public static AstThing ApplyLanguageAnalysisPhases(...)
{
    if (upTo >= AnalysisPhase.TreeLink)
        ast = new TreeLinkageVisitor().Visit(ast);
    if (upTo >= AnalysisPhase.Builtins)
        ast = new BuiltinInjectorVisitor().Visit(ast);
    if (upTo >= AnalysisPhase.ClassCtors)
        ast = new ClassCtorInserter().Visit(ast);
    // ... 15 more phases in rigid sequence
}
```

### Problems

**Maintainability:**
- Adding new phase requires modifying central orchestration
- Phase dependencies are implicit (order-based)
- Cannot easily disable experimental phases
- Hard to understand phase interactions

**Testing:**
- Cannot test phases in isolation
- Must run earlier phases to test later ones
- No ability to inject test data between phases
- Integration tests expensive (full pipeline)

**Performance:**
- Cannot parallelize independent phases
- Must run all phases even when some are no-ops
- Cannot cache intermediate results per phase
- No way to skip phases for unchanged code

**Debugging:**
- Cannot step through single phase
- Hard to bisect which phase caused error
- No phase-level instrumentation
- Cannot dump AST between specific phases

## Requirements

### Phase Interface
```csharp
public interface ICompilerPhase
{
    string Name { get; }
    IReadOnlyList<string> DependsOn { get; }
    IReadOnlyList<string> ProvidedCapabilities { get; }
    
    PhaseResult Transform(AstThing ast, PhaseContext context);
}

public record PhaseResult(
    AstThing TransformedAst,
    IReadOnlyList<Diagnostic> Diagnostics,
    bool Success
);

public class PhaseContext
{
    public ISymbolTable SymbolTable { get; set; }
    public ITypeRegistry TypeRegistry { get; set; }
    public Dictionary<string, object> SharedData { get; }
    public bool EnableCaching { get; set; }
}
```

### Pipeline Orchestrator
```csharp
public class TransformationPipeline
{
    private readonly List<ICompilerPhase> _phases = new();
    private readonly Dictionary<string, AstThing> _cache = new();
    
    public void RegisterPhase(ICompilerPhase phase)
    {
        // Validate dependencies exist
        foreach (var dep in phase.DependsOn)
        {
            if (!_phases.Any(p => p.ProvidedCapabilities.Contains(dep)))
                throw new InvalidOperationException(
                    $"Dependency '{dep}' not satisfied");
        }
        _phases.Add(phase);
    }
    
    public PipelineResult Execute(AstThing ast, PipelineOptions options)
    {
        var context = new PhaseContext();
        var allDiagnostics = new List<Diagnostic>();
        var currentAst = ast;
        
        // Topologically sort phases by dependencies
        var sortedPhases = TopologicalSort(_phases);
        
        foreach (var phase in sortedPhases)
        {
            if (options.SkipPhases.Contains(phase.Name))
                continue;
                
            // Check cache if enabled
            if (options.EnableCaching && TryGetCached(phase, currentAst, out var cached))
            {
                currentAst = cached;
                continue;
            }
            
            var result = phase.Transform(currentAst, context);
            allDiagnostics.AddRange(result.Diagnostics);
            
            if (!result.Success && options.StopOnError)
                return new PipelineResult(currentAst, allDiagnostics, false);
            
            currentAst = result.TransformedAst;
            
            if (options.EnableCaching)
                Cache(phase, ast, currentAst);
        }
        
        return new PipelineResult(currentAst, allDiagnostics, true);
    }
}
```

### Phase Registration
```csharp
public class TreeLinkagePhase : ICompilerPhase
{
    public string Name => "TreeLinkage";
    public IReadOnlyList<string> DependsOn => Array.Empty<string>();
    public IReadOnlyList<string> ProvidedCapabilities => new[] { "TreeStructure" };
    
    public PhaseResult Transform(AstThing ast, PhaseContext context)
    {
        var visitor = new TreeLinkageVisitor();
        var result = visitor.Visit(ast);
        return new PhaseResult(result, visitor.Diagnostics, true);
    }
}

public class SymbolTablePhase : ICompilerPhase
{
    public string Name => "SymbolTable";
    public IReadOnlyList<string> DependsOn => new[] { "TreeStructure", "Builtins" };
    public IReadOnlyList<string> ProvidedCapabilities => new[] { "Symbols" };
    
    public PhaseResult Transform(AstThing ast, PhaseContext context)
    {
        var visitor = new SymbolTableBuilderVisitor();
        var result = visitor.Visit(ast);
        context.SymbolTable = result.SymbolTable;
        return new PhaseResult(result.Ast, visitor.Diagnostics, true);
    }
}
```

## Benefits

### Testing
```csharp
[Test]
public void TestTypeAnnotationPhase()
{
    var pipeline = new TransformationPipeline();
    pipeline.RegisterPhase(new TreeLinkagePhase());
    pipeline.RegisterPhase(new SymbolTablePhase());
    pipeline.RegisterPhase(new TypeAnnotationPhase());
    
    // Test only specific phase with dependencies
    var result = pipeline.Execute(testAst, new PipelineOptions 
    { 
        StopAfter = "TypeAnnotation" 
    });
    
    Assert.Empty(result.Diagnostics);
}
```

### Debugging
```csharp
// Dump AST after specific phases
var result = pipeline.Execute(ast, new PipelineOptions 
{ 
    DumpAfter = new[] { "SymbolTable", "TypeAnnotation" },
    DumpToFile = true
});

// Step through single phase
var phase = new SymbolTablePhase();
var phaseResult = phase.Transform(ast, context);
// Inspect result
```

### Performance
```csharp
// Parallel execution of independent phases
var parallelPipeline = new ParallelTransformationPipeline();
parallelPipeline.RegisterPhase(new Phase1());
parallelPipeline.RegisterPhase(new Phase2()); // Independent of Phase1
parallelPipeline.Execute(ast); // Runs Phase1 and Phase2 in parallel

// Phase-level caching
var cachedPipeline = new TransformationPipeline();
cachedPipeline.Execute(ast, new PipelineOptions { EnableCaching = true });
// Second run uses cached results for unchanged phases
```

### Extensibility
```csharp
// Third-party can add custom phases
public class CustomAnalysisPhase : ICompilerPhase
{
    public string Name => "CustomAnalysis";
    public IReadOnlyList<string> DependsOn => new[] { "Symbols", "Types" };
    public IReadOnlyList<string> ProvidedCapabilities => new[] { "CustomMetadata" };
    
    public PhaseResult Transform(AstThing ast, PhaseContext context)
    {
        // Custom analysis logic
    }
}

var pipeline = new TransformationPipeline();
pipeline.RegisterPhase(new CustomAnalysisPhase());
```

## Implementation Plan

### Phase 1: Design & Interface (Weeks 1-2)
1. Design ICompilerPhase interface
2. Design TransformationPipeline orchestrator
3. Design PhaseContext for shared data
4. Create phase dependency resolution

### Phase 2: Wrap Existing Phases (Weeks 3-6)
1. Create phase wrappers for existing visitors
2. Declare explicit dependencies
3. Migrate from ApplyLanguageAnalysisPhases
4. Keep existing behavior (no changes yet)

### Phase 3: Enable Features (Weeks 7-8)
1. Add phase-level caching
2. Add skip/stop-after options
3. Add AST dumping between phases
4. Add phase timing instrumentation

### Phase 4: Optimization (Weeks 9-10)
1. Identify independent phases
2. Implement parallel execution
3. Optimize phase ordering
4. Performance benchmarking

## Acceptance Criteria

- [ ] All 18 phases wrapped as ICompilerPhase
- [ ] Dependencies explicitly declared
- [ ] Topological sorting works correctly
- [ ] Can skip arbitrary phases
- [ ] Can stop after specific phase
- [ ] Can dump AST between phases
- [ ] Phase-level timing available
- [ ] Tests for phase isolation
- [ ] Documentation for adding new phases

## Architecture Diagram

```
┌─────────────────────────────────────┐
│   TransformationPipeline            │
├─────────────────────────────────────┤
│ • RegisterPhase(ICompilerPhase)     │
│ • Execute(ast, options)             │
│ • TopologicalSort()                 │
│ • Cache management                  │
└─────────────────────────────────────┘
                 │
                 ├─ phase1: TreeLinkage
                 │     └─ depends: []
                 ├─ phase2: Builtins
                 │     └─ depends: [TreeLinkage]
                 ├─ phase3: SymbolTable
                 │     └─ depends: [TreeLinkage, Builtins]
                 ├─ phase4: TypeAnnotation
                 │     └─ depends: [SymbolTable]
                 └─ ... (14 more phases)
```

## Example Usage

```csharp
// Configure pipeline
var pipeline = new TransformationPipeline();
pipeline.RegisterPhase(new TreeLinkagePhase());
pipeline.RegisterPhase(new BuiltinInjectorPhase());
pipeline.RegisterPhase(new SymbolTablePhase());
// ... register all phases

// Execute with options
var result = pipeline.Execute(ast, new PipelineOptions
{
    EnableCaching = true,
    StopOnError = true,
    DumpAfter = new[] { "SymbolTable", "TypeAnnotation" },
    SkipPhases = new[] { "ExperimentalFeature" }
});

// Inspect results
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Diagnostics: {result.Diagnostics.Count}");
Console.WriteLine($"Phase timings: {string.Join(", ", result.PhaseTimings)}");
```

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #5
- LLVM Pass Manager: https://llvm.org/docs/WritingAnLLVMPass.html
- GHC Pipeline: https://gitlab.haskell.org/ghc/ghc/-/wikis/commentary/compiler/pipeline
- Related Issues: Enables #ISSUE-003 (Incremental Compilation), improves testing (#ISSUE-007)

## Estimated Effort

**10 weeks** (2.5 months)
- Weeks 1-2: Design and interface
- Weeks 3-6: Wrap existing phases
- Weeks 7-8: Enable features
- Weeks 9-10: Optimization

## Dependencies

- None (can be done independently)

## Enables

- Issue #003: Incremental Compilation (phase-level caching)
- Issue #007: Better Testing (phase isolation)
- Future: Parallel compilation
- Future: Plugin architecture

## Success Metrics

- All phases successfully wrapped
- Zero behavior changes from migration
- Phase isolation enables unit testing
- Pipeline configuration is flexible
- Performance neutral or improved
