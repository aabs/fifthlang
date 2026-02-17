# Restructure Testing Architecture for Better Coverage and Maintainability

**Labels:** `arch-review`, `testing`, `quality`, `medium`  
**Priority:** P2  
**Severity:** MEDIUM  
**Epic:** Architectural Improvements Q1-Q2 2026

## Problem Summary

The testing architecture lacks proper separation between unit and integration tests, has no property-based testing for core algorithms, and makes it difficult to test individual compiler phases in isolation. This leads to slow tests, low confidence in changes, and difficulty preventing regressions.

## Current Issues

### Poor Test Organization
```
test/
├── ast-tests/              # Mix of unit and integration
├── runtime-integration-tests/  # All end-to-end
├── syntax-parser-tests/    # Parser tests
├── fifth-runtime-tests/    # Runtime tests
├── perf/                   # Performance benchmarks
└── kg-smoke-tests/         # Knowledge graph tests
```

### Problems
- Most tests are end-to-end (compile + run)
- 161 .5th test files but unclear organization
- No unit tests for individual transformation visitors
- Parser tests mix syntax and semantics
- No property-based tests for critical algorithms
- Test execution relatively slow (compile IL → assembly → run)

### Missing Test Types
1. **Unit Tests:** Individual visitors, transformations, utilities
2. **Property Tests:** Type inference, symbol resolution, AST transformations
3. **Component Tests:** Individual compiler phases
4. **Contract Tests:** Phase interfaces and boundaries

### Impact

**Development Velocity:**
- Slow test feedback (must compile → assemble → run)
- Cannot quickly verify transformation logic
- Hard to test edge cases in isolation

**Confidence:**
- Changes might break distant code
- No property-based invariant checking
- Regressions hard to catch early

**Maintainability:**
- Test setup complex (need full compilation pipeline)
- Hard to isolate failures
- Difficult to add focused tests

## Requirements

### Testing Pyramid
```
test/
├── unit/                      # Fast, focused unit tests
│   ├── Parser/
│   │   ├── LexerTests.cs      # Token generation
│   │   ├── ParserTests.cs     # Grammar rules
│   │   └── AstBuilderTests.cs # Parse tree → AST
│   ├── Transformations/
│   │   ├── TreeLinkageTests.cs
│   │   ├── SymbolTableTests.cs
│   │   ├── TypeAnnotationTests.cs
│   │   └── ... (one per phase)
│   ├── CodeGeneration/
│   │   ├── ILTransformTests.cs
│   │   └── ILEmissionTests.cs
│   └── SymbolTable/
│       ├── SymbolResolutionTests.cs
│       └── ScopeTests.cs
│
├── integration/               # Component integration
│   ├── ParserPipelineTests.cs
│   ├── TransformationPipelineTests.cs
│   └── CodeGenerationPipelineTests.cs
│
├── e2e/                       # End-to-end compilation
│   ├── BasicSyntax/
│   ├── Functions/
│   ├── Classes/
│   └── KnowledgeGraphs/
│
├── property/                  # Property-based tests
│   ├── ParserProperties.cs
│   ├── TypeInferenceProperties.cs
│   └── SymbolTableProperties.cs
│
├── performance/               # Benchmarks
│   └── CompilationBenchmarks.cs
│
└── shared/                    # Test utilities
    ├── AstBuilder.cs          # Fluent AST construction
    ├── TestHarness.cs         # Phase testing
    └── Generators.cs          # Property test generators
```

### Unit Test Infrastructure
```csharp
// Test harness for isolated phase testing
public class PhaseTestHarness
{
    public static (AstThing result, List<Diagnostic> diagnostics) 
        TestPhase<TPhase>(AstThing input, PhaseOptions? options = null)
        where TPhase : ICompilerPhase, new()
    {
        var phase = new TPhase();
        var context = new PhaseContext();
        var result = phase.Transform(input, context);
        return (result.TransformedAst, result.Diagnostics.ToList());
    }
}

// Fluent AST builder for tests
public class AstBuilder
{
    public static FunctionDefBuilder FunctionDef(string name) 
        => new FunctionDefBuilder(name);
    
    public static VarRefExp VarRef(string name) 
        => new VarRefExp { VarName = name };
}

[Test]
public void SymbolTable_ResolvesLocalVariable()
{
    // Arrange: Create minimal AST
    var ast = AstBuilder.FunctionDef("test")
        .WithLocalVar("x", TypeRegistry.Int32)
        .WithBody(AstBuilder.VarRef("x"))
        .Build();
    
    // Act: Run only SymbolTable phase
    var (result, diags) = PhaseTestHarness.TestPhase<SymbolTablePhase>(ast);
    
    // Assert: Verify symbol resolution
    Assert.Empty(diags);
    var varRef = result.FindNode<VarRefExp>(v => v.VarName == "x");
    Assert.NotNull(varRef.ResolvedSymbol);
}
```

### Property-Based Testing
```csharp
// Use FsCheck or CsCheck for property testing
[Property]
public Property Parser_RoundTrip_Preserves_Semantics()
{
    return Prop.ForAll(
        AstGenerators.ValidProgram(),
        program =>
        {
            // Parse → Pretty Print → Parse should be equivalent
            var ast1 = FifthParserManager.Parse(program);
            var printed = PrettyPrinter.Print(ast1);
            var ast2 = FifthParserManager.Parse(printed);
            
            return AstEquals(ast1, ast2);
        });
}

[Property]
public Property TypeInference_Respects_Subtyping()
{
    return Prop.ForAll(
        TypeGenerators.Type(),
        TypeGenerators.Type(),
        (t1, t2) =>
        {
            if (TypeSystem.IsSubtypeOf(t1, t2))
            {
                // If t1 <: t2, then expressions of type t1 
                // should be assignable to t2
                var expr = ExpressionGenerators.OfType(t1);
                var inferredType = TypeInference.Infer(expr);
                return TypeSystem.IsAssignableTo(inferredType, t2);
            }
            return true;
        });
}

[Property]
public Property SymbolTable_ResolveIsIdempotent()
{
    return Prop.ForAll(
        SymbolTableGenerators.ValidSymbolTable(),
        SymbolGenerators.ValidSymbol(),
        (table, symbol) =>
        {
            var result1 = table.Resolve(symbol);
            var result2 = table.Resolve(symbol);
            return Equals(result1, result2);
        });
}
```

### Fast Feedback Loop
```csharp
// Mock heavy dependencies for fast testing
public interface IILAssembler
{
    AssemblyResult Assemble(string ilCode);
}

public class MockILAssembler : IILAssembler
{
    public AssemblyResult Assemble(string ilCode)
    {
        // Validate IL syntax without actually assembling
        return new AssemblyResult { Success = ValidateILSyntax(ilCode) };
    }
}

[Test]
public void CodeGeneration_EmitsValidIL()
{
    var ast = TestAsts.SimpleAddition();
    var generator = new ILCodeGenerator();
    
    var ilCode = generator.GenerateCode(ast);
    
    // Fast validation without ilasm
    var mockAssembler = new MockILAssembler();
    var result = mockAssembler.Assemble(ilCode);
    Assert.True(result.Success);
}
```

## Implementation Plan

### Phase 1: Infrastructure (Weeks 1-2)
1. Create test project structure
2. Add test utilities (AstBuilder, TestHarness)
3. Add property testing library (FsCheck)
4. Set up mock infrastructure

### Phase 2: Unit Tests (Weeks 3-6)
1. Parser unit tests (lexer, parser, AST builder)
2. Transformation unit tests (one per phase)
3. Code generation unit tests
4. Symbol table unit tests

### Phase 3: Property Tests (Weeks 7-8)
1. Parser property tests (round-trip, well-formedness)
2. Type inference property tests (soundness, completeness)
3. Symbol table property tests (idempotence, consistency)
4. Transformation property tests (preservation)

### Phase 4: Reorganize Existing Tests (Weeks 9-10)
1. Categorize existing tests (unit/integration/e2e)
2. Move tests to appropriate directories
3. Refactor slow tests to use mocks
4. Add missing coverage

## Acceptance Criteria

- [ ] Unit tests run in <1 second total
- [ ] Integration tests run in <10 seconds total
- [ ] Property tests generate 100s of test cases
- [ ] All compiler phases have unit tests
- [ ] Coverage >80% for core components
- [ ] Tests organized by type (unit/integration/e2e/property)
- [ ] Test infrastructure documented
- [ ] CI runs different test suites appropriately

## Test Organization Guidelines

### Unit Tests
- Test single class/function in isolation
- Use mocks for dependencies
- Fast (<10ms per test)
- Focused assertions
- High coverage (>90%)

### Integration Tests
- Test component interactions
- Minimal mocking
- Moderate speed (<100ms per test)
- Test boundaries between components

### E2E Tests
- Test full compilation pipeline
- No mocking
- Slow but comprehensive
- Test realistic scenarios
- Run on CI for every commit

### Property Tests
- Test invariants and laws
- Generate 100-1000 test cases
- Find edge cases automatically
- Complement unit tests

### Performance Tests
- Benchmark critical paths
- Track performance over time
- Run separately (not in CI)
- Prevent performance regressions

## Example Test Structure

```csharp
// Unit test
[TestClass]
public class SymbolTableTests
{
    [Test]
    public void Add_AddsSymbolToTable()
    {
        var table = new SymbolTable();
        var symbol = new Symbol("test");
        var entry = new VariableEntry(symbol, TypeRegistry.Int32);
        
        table.Add(symbol, entry);
        
        Assert.Equal(entry, table.Resolve(symbol));
    }
    
    [Test]
    public void ResolveByName_FindsSymbol()
    {
        var table = new SymbolTable();
        table.Add(new Symbol("test"), new VariableEntry(...));
        
        var results = table.ResolveByName("test");
        
        Assert.Single(results);
    }
}

// Property test
[TestClass]
public class SymbolTableProperties
{
    [Property]
    public Property Resolve_IsIdempotent()
    {
        return Prop.ForAll(
            SymbolTableGen(),
            SymbolGen(),
            (table, symbol) =>
            {
                var r1 = table.Resolve(symbol);
                var r2 = table.Resolve(symbol);
                return Equals(r1, r2);
            });
    }
}

// Integration test
[TestClass]
public class TransformationPipelineTests
{
    [Test]
    public async Task Pipeline_TransformsSimpleProgram()
    {
        var source = @"
            main(): int {
                let x = 42;
                return x;
            }
        ";
        
        var ast = FifthParserManager.Parse(source);
        var pipeline = CreateDefaultPipeline();
        var result = pipeline.Execute(ast);
        
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
    }
}
```

## Performance Goals

| Test Type | Count | Total Time | Per Test |
|-----------|-------|------------|----------|
| Unit | 500+ | <1s | <2ms |
| Integration | 100+ | <10s | <100ms |
| E2E | 50+ | <60s | <1.2s |
| Property | 20+ | <30s | <1.5s |

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #7
- Property-Based Testing: "PropEr Testing" by Fred Hebert
- Test Pyramid: https://martinfowler.com/articles/practical-test-pyramid.html
- FsCheck: https://fscheck.github.io/FsCheck/
- Related Issues: Improves all development (#ISSUE-001 through #ISSUE-006)

## Estimated Effort

**10 weeks** (2.5 months)
- Weeks 1-2: Infrastructure
- Weeks 3-6: Unit tests
- Weeks 7-8: Property tests
- Weeks 9-10: Reorganize existing tests

## Dependencies

- Issue #005: Composable Pipeline (enables phase isolation)

## Success Metrics

- Unit tests <1s total
- Test coverage >80%
- Property tests find 10+ bugs
- CI feedback <5 minutes
- Developer confidence in changes improved
- Regression rate decreased
