# Enhance Symbol Table for Performance and IDE Features

**Labels:** `arch-review`, `symbol-table`, `performance`, `medium`  
**Priority:** P2  
**Severity:** MEDIUM  
**Epic:** Architectural Improvements Q2 2026

## Problem Summary

The symbol table implementation is a simple `Dictionary<Symbol, ISymbolTableEntry>` with no support for efficient scope-based queries, hierarchical lookups, or the rich queries needed for IDE features and advanced type checking.

## Current Issues

### Limited Functionality
- Symbol table is basic dictionary (SymbolTable.cs: 32 lines)
- Linear search for name-based lookup (`ResolveByName()`)
- No scope hierarchy traversal support
- No "find all references" capability
- No "find symbols in scope" query
- Symbol table stored per-scope but no global index

**Code Evidence:**
```csharp
// src/ast-model/Symbols/SymbolTable.cs
public class SymbolTable : Dictionary<Symbol, ISymbolTableEntry>, ISymbolTable
{
    public ISymbolTableEntry ResolveByName(string symbolName)
    {
        // O(n) linear search!
        foreach (var k in Keys)
        {
            if (k.Name == symbolName)
                return this[k];
        }
        return null;
    }
}
```

### Performance Problems
- O(n) lookup for symbol resolution
- No indexing for fast queries
- Cannot efficiently answer "what's in scope?" queries
- Scales poorly with large codebases

### IDE Features Blocked
- "Find all references" requires full AST scan
- "Find symbols" completion has no index
- "Rename symbol" cannot find all uses efficiently
- Hover info requires re-resolution
- No support for workspace-wide symbol search

## Requirements

### Enhanced Symbol Table
```csharp
public class SymbolTable : ISymbolTable
{
    // Fast lookups via multiple indexes
    private readonly Dictionary<string, List<ISymbolTableEntry>> _nameIndex = new();
    private readonly Dictionary<Symbol, ISymbolTableEntry> _symbolIndex = new();
    private readonly Dictionary<SymbolKind, List<ISymbolTableEntry>> _kindIndex = new();
    
    // Scope hierarchy
    private readonly SymbolTable? _parent;
    private readonly List<SymbolTable> _children = new();
    private readonly IScope _scope;
    
    // O(1) lookup by name
    public IEnumerable<ISymbolTableEntry> ResolveByName(string name)
    {
        // Check current scope
        if (_nameIndex.TryGetValue(name, out var entries))
            return entries;
        
        // Walk up scope chain
        return _parent?.ResolveByName(name) 
            ?? Enumerable.Empty<ISymbolTableEntry>();
    }
    
    // Get all visible symbols at location
    public IEnumerable<ISymbolTableEntry> GetVisibleSymbols(SourceLocation location)
    {
        // Return all symbols visible at location
        // Includes current scope + parent scopes
        var symbols = _symbolIndex.Values.ToList();
        
        if (_parent != null)
            symbols.AddRange(_parent.GetVisibleSymbols(location));
        
        return symbols;
    }
    
    // O(1) lookup by symbol kind
    public IEnumerable<ISymbolTableEntry> FindByKind(SymbolKind kind)
    {
        return _kindIndex.TryGetValue(kind, out var entries) 
            ? entries 
            : Enumerable.Empty<ISymbolTableEntry>();
    }
    
    // Qualified name resolution
    public ISymbolTableEntry? ResolveQualified(string[] nameParts)
    {
        // Handle paths like "System.Collections.List"
        var current = this;
        ISymbolTableEntry? result = null;
        
        foreach (var part in nameParts)
        {
            result = current.ResolveByName(part).FirstOrDefault();
            if (result == null)
                return null;
            
            // Navigate into nested scope if available
            if (result.NestedScope != null)
                current = result.NestedScope.SymbolTable;
        }
        
        return result;
    }
}
```

### Global Symbol Index
```csharp
public class GlobalSymbolIndex
{
    // Fast global queries for IDE features
    private readonly Dictionary<string, List<SymbolDefinition>> _definitions = new();
    private readonly Dictionary<Symbol, List<SourceLocation>> _references = new();
    private readonly Dictionary<string, List<SymbolDefinition>> _fuzzyIndex = new();
    
    public void IndexAssembly(AssemblyDef assembly)
    {
        // Build indices from AST
        var visitor = new SymbolIndexingVisitor(this);
        visitor.Visit(assembly);
    }
    
    public IEnumerable<SourceLocation> FindReferences(Symbol symbol)
    {
        return _references.TryGetValue(symbol, out var locs) 
            ? locs 
            : Enumerable.Empty<SourceLocation>();
    }
    
    public IEnumerable<SymbolDefinition> FindDefinitions(string name)
    {
        return _definitions.TryGetValue(name, out var defs)
            ? defs
            : Enumerable.Empty<SymbolDefinition>();
    }
    
    public IEnumerable<SymbolDefinition> FuzzySearch(string prefix)
    {
        // For code completion - find symbols starting with prefix
        return _definitions
            .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .SelectMany(kvp => kvp.Value);
    }
}
```

### Scope-Aware Resolution
```csharp
public class ScopeResolver
{
    private readonly GlobalSymbolIndex _index;
    
    public ResolvedSymbol? Resolve(string name, IScope scope)
    {
        // Try local scope first
        var local = scope.SymbolTable.ResolveByName(name);
        if (local.Any())
            return new ResolvedSymbol(local.First(), ResolutionKind.Local);
        
        // Try parent scopes
        var parent = scope.EnclosingScope;
        while (parent != null)
        {
            var parentResult = parent.SymbolTable.ResolveByName(name);
            if (parentResult.Any())
                return new ResolvedSymbol(parentResult.First(), ResolutionKind.Outer);
            parent = parent.EnclosingScope;
        }
        
        // Try imported modules
        foreach (var import in scope.Imports)
        {
            var imported = _index.FindDefinitions($"{import}.{name}");
            if (imported.Any())
                return new ResolvedSymbol(imported.First(), ResolutionKind.Imported);
        }
        
        return null;
    }
}
```

## Implementation Plan

### Phase 1: Enhanced Symbol Table (Weeks 1-2)
1. Design indexed symbol table structure
2. Implement multiple index types (name, kind, location)
3. Add scope hierarchy support
4. Test performance improvements

### Phase 2: Global Symbol Index (Weeks 3-4)
1. Design global index structure
2. Implement symbol indexing visitor
3. Build definition and reference indices
4. Add fuzzy search support

### Phase 3: Scope-Aware Resolution (Weeks 5-6)
1. Implement qualified name resolution
2. Add import resolution
3. Create scope resolver with fallback chain
4. Test resolution correctness

### Phase 4: IDE Integration (Weeks 7-8)
1. Add "find references" API
2. Add "find symbols in scope" API
3. Integrate with LSP (if available)
4. Performance benchmarking

## Acceptance Criteria

- [ ] Symbol lookup is O(1) instead of O(n)
- [ ] Can find all references to a symbol
- [ ] Can find all symbols in scope
- [ ] Can search symbols by kind (types, functions, variables)
- [ ] Qualified name resolution works
- [ ] Global index handles cross-file symbols
- [ ] Performance tests show >10x speedup
- [ ] Integration tests verify correctness

## Performance Goals

| Operation | Current | Target | Improvement |
|-----------|---------|--------|-------------|
| Resolve by name (100 symbols) | O(n) ~100μs | O(1) ~1μs | 100x |
| Find all references | Full AST scan | Indexed | >100x |
| Symbols in scope | Walk AST | Indexed | >50x |
| Fuzzy search | N/A | <10ms | N/A |

## Benefits

### Performance
- O(1) symbol lookups (instead of O(n))
- Fast "find all references" (indexed)
- Efficient scope queries
- Scales to large codebases

### IDE Features
- Real-time "find references"
- Fast code completion
- Workspace-wide symbol search
- Efficient "rename symbol"
- Quick "go to definition"

### Type Checking
- Efficient overload resolution
- Fast generic type resolution
- Trait/interface resolution

## Example Usage

```csharp
// Create enhanced symbol table
var symbolTable = new SymbolTable(parentScope);

// Add symbol (automatically indexed)
symbolTable.Add(new Symbol("myVar"), new VariableEntry(...));

// Fast O(1) lookup
var results = symbolTable.ResolveByName("myVar");

// Find all variables in scope
var variables = symbolTable.FindByKind(SymbolKind.Variable);

// Global index for "find references"
var globalIndex = new GlobalSymbolIndex();
globalIndex.IndexAssembly(assembly);

var references = globalIndex.FindReferences(symbol);
Console.WriteLine($"Found {references.Count()} references");

// Fuzzy search for code completion
var completions = globalIndex.FuzzySearch("my");
// Returns: myVar, myFunction, myClass, etc.
```

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #6
- Roslyn symbol tables: https://github.com/dotnet/roslyn
- rust-analyzer symbol indexing
- Related Issues: Enables #ISSUE-002 (LSP navigation), improves #ISSUE-003 (Incremental Compilation)

## Estimated Effort

**8 weeks** (2 months)
- Weeks 1-2: Enhanced symbol table
- Weeks 3-4: Global symbol index
- Weeks 5-6: Scope-aware resolution
- Weeks 7-8: IDE integration

## Dependencies

- Nice to have: #ISSUE-002 LSP (for integration)
- Nice to have: #ISSUE-004 Diagnostics (for source locations)

## Enables

- Issue #002: LSP navigation features
- Issue #003: Incremental compilation (symbol caching)
- Better type checking performance
- Workspace-wide refactoring

## Success Metrics

- 100x faster symbol lookups
- "Find references" <100ms for typical project
- Code completion responds instantly
- Zero performance regressions
- Scales to 10,000+ symbols
