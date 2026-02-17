# Implement Incremental Compilation for Performance and IDE Support

**Labels:** `arch-review`, `performance`, `ide-support`, `critical`  
**Priority:** P0  
**Severity:** CRITICAL  
**Epic:** Architectural Improvements Q2-Q3 2026

## Problem Summary

The compiler performs full recompilation on every build with no support for incremental compilation. This is incompatible with interactive development, scales poorly, and blocks responsive IDE integration.

## Current Behavior

- Full reparse of all source files on every compilation
- All transformations re-run on entire AST
- No caching of intermediate results
- No dependency tracking
- Build time grows linearly with codebase size
- IDE features too slow for real-time use

**Code Evidence:**
```csharp
// src/compiler/Compiler.cs:233
private (AstThing? ast, int sourceCount) ParsePhase(...)
{
    // Always parses from scratch - no caching
    var ast = FifthParserManager.ParseFile(options.Source);
    return (ast, 1);
}
```

## Impact

### Scalability Problems
- Build times grow linearly with project size
- Cannot efficiently handle projects >100 files
- Full recompilation wastes developer time
- Makes large-scale projects impractical

### Poor Developer Experience
- Slow feedback loop (wait for full rebuild)
- Cannot support "save-and-see" development style
- Makes language feel sluggish vs competitors (Rust, Go, TypeScript)
- Discourages experimentation and iteration

### IDE Integration Blocked
- LSP requires sub-second response times
- Real-time diagnostics need incremental updates
- Cannot provide responsive code completion
- Document synchronization too slow

### Resource Waste
- Re-parses unchanged files
- Re-runs transformations on unaffected code
- Regenerates unchanged IL/assemblies
- Wastes CPU, memory, and battery

## Requirements

### File-Level Tracking
1. Track content hash for each source file
2. Detect which files have changed since last build
3. Only reparse changed files
4. Cache parsed ASTs for unchanged files

### Dependency Tracking
1. Build dependency graph of source files
2. Identify which files depend on changed files
3. Compute transitive closure of affected files
4. Only reprocess affected files

### AST Caching
1. Serialize/deserialize parsed ASTs
2. Store ASTs in efficient format (.ast files)
3. Invalidate cache on source changes
4. Share cache between compiler and LSP

### Transformation Optimization
1. Track which transformations affect which nodes
2. Skip transformations on unchanged subtrees
3. Merge incremental symbol table updates
4. Avoid full AST traversals when possible

### Build Artifact Management
1. Store intermediate representations
2. Track source → artifact mappings
3. Implement proper cache invalidation
4. Support parallel compilation

## Architecture

### Dependency Graph
```csharp
public class DependencyGraph
{
    // File dependencies (imports, references)
    private readonly Dictionary<string, HashSet<string>> _dependencies = new();
    
    // Content hashes for change detection
    private readonly Dictionary<string, string> _contentHashes = new();
    
    // Compute affected files from a change
    public IEnumerable<string> GetAffectedFiles(string changedFile)
    {
        var affected = new HashSet<string> { changedFile };
        var queue = new Queue<string>(new[] { changedFile });
        
        while (queue.Count > 0)
        {
            var file = queue.Dequeue();
            if (_dependencies.TryGetValue(file, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    if (affected.Add(dependent))
                        queue.Enqueue(dependent);
                }
            }
        }
        
        return affected;
    }
    
    public bool HasChanged(string file)
    {
        var currentHash = ComputeHash(File.ReadAllText(file));
        if (!_contentHashes.TryGetValue(file, out var cachedHash))
            return true;
        return currentHash != cachedHash;
    }
    
    public void UpdateHash(string file)
    {
        _contentHashes[file] = ComputeHash(File.ReadAllText(file));
    }
}
```

### Compilation Cache
```csharp
public class CompilationCache
{
    private readonly string _cacheDirectory;
    
    // Cache parsed ASTs per file
    private readonly Dictionary<string, CachedAst> _astCache = new();
    
    // Cache transformed ASTs
    private readonly Dictionary<string, AstThing> _transformedCache = new();
    
    // Cache symbol tables
    private readonly Dictionary<string, ISymbolTable> _symbolCache = new();
    
    public record CachedAst(AstThing Ast, DateTime Timestamp, string ContentHash);
    
    public (AstThing? ast, bool cached) GetOrParse(string file)
    {
        // Try memory cache first
        if (_astCache.TryGetValue(file, out var cached))
        {
            var currentHash = ComputeHash(File.ReadAllText(file));
            if (cached.ContentHash == currentHash)
                return (cached.Ast, true);
        }
        
        // Try disk cache
        var cacheFile = GetCacheFilePath(file);
        if (File.Exists(cacheFile))
        {
            try
            {
                var deserialized = DeserializeAst(cacheFile);
                var currentHash = ComputeHash(File.ReadAllText(file));
                if (deserialized.ContentHash == currentHash)
                {
                    _astCache[file] = deserialized;
                    return (deserialized.Ast, true);
                }
            }
            catch
            {
                // Cache corrupted, will reparse
            }
        }
        
        // Parse from scratch
        var ast = FifthParserManager.ParseFile(file);
        var hash = ComputeHash(File.ReadAllText(file));
        var entry = new CachedAst(ast, DateTime.Now, hash);
        
        _astCache[file] = entry;
        SerializeAst(cacheFile, entry);
        
        return (ast, false);
    }
    
    public void Invalidate(string file)
    {
        _astCache.Remove(file);
        _transformedCache.Remove(file);
        _symbolCache.Remove(file);
        
        var cacheFile = GetCacheFilePath(file);
        if (File.Exists(cacheFile))
            File.Delete(cacheFile);
    }
}
```

### Incremental Compiler
```csharp
public class IncrementalCompiler
{
    private readonly DependencyGraph _dependencyGraph;
    private readonly CompilationCache _cache;
    
    public CompilationResult Compile(CompilerOptions options)
    {
        var changedFiles = FindChangedFiles(options.Source);
        var affectedFiles = _dependencyGraph.GetAffectedFiles(changedFiles);
        
        // Parse only affected files
        var asts = new Dictionary<string, AstThing>();
        foreach (var file in affectedFiles)
        {
            var (ast, cached) = _cache.GetOrParse(file);
            asts[file] = ast;
            
            if (!cached)
                _dependencyGraph.UpdateHash(file);
        }
        
        // Merge with cached ASTs for unaffected files
        var completeAst = MergeAsts(asts, GetUnaffectedFiles());
        
        // Run transformations (optimized for incremental)
        var transformed = TransformIncremental(completeAst, affectedFiles);
        
        // Generate code only for affected modules
        return GenerateCodeIncremental(transformed, affectedFiles);
    }
}
```

## Implementation Plan

### Phase 1: Infrastructure (Weeks 1-4)
1. Design cache format and serialization
2. Implement content hashing
3. Create cache management infrastructure
4. Add cache directory configuration

### Phase 2: File-Level Caching (Weeks 5-8)
1. Implement AST serialization/deserialization
2. Add file-level cache get/set operations
3. Integrate with ParsePhase
4. Test cache correctness and performance

### Phase 3: Dependency Tracking (Weeks 9-12)
1. Build dependency graph from imports
2. Implement change detection
3. Compute affected file sets
4. Test dependency resolution

### Phase 4: Incremental Transformations (Weeks 13-16)
1. Track transformation dependencies
2. Skip unchanged subtrees
3. Implement incremental symbol table updates
4. Optimize visitor traversals

### Phase 5: Integration & Optimization (Weeks 17-20)
1. Integrate with LSP server
2. Add parallel compilation support
3. Performance benchmarking
4. Cache tuning and optimization

## Acceptance Criteria

- [ ] Incremental builds significantly faster than full builds (>5x speedup)
- [ ] Cache correctly invalidated on source changes
- [ ] Cache correctly handles file dependencies
- [ ] Incremental builds produce identical output to full builds
- [ ] Cache survives IDE restarts
- [ ] LSP uses incremental compilation for real-time features
- [ ] Performance tests verify speedup
- [ ] Documentation for cache management

## Performance Goals

| Scenario | Current | Target | Improvement |
|----------|---------|--------|-------------|
| Full build (100 files) | 30s | 30s | 1x (baseline) |
| Rebuild (0 changes) | 30s | 1s | 30x |
| Rebuild (1 file change) | 30s | 3s | 10x |
| Rebuild (10 file changes) | 30s | 8s | 3.75x |
| LSP diagnostics | N/A | <500ms | Real-time |

## Technical Considerations

### AST Serialization
- Use MessagePack or Protocol Buffers for efficiency
- Preserve source locations
- Handle large ASTs (>1MB)
- Version stamping for format changes

### Cache Invalidation
- Detect source file changes (content hash)
- Detect dependency changes (transitive)
- Detect compiler version changes
- Handle cache corruption gracefully

### Concurrency
- Thread-safe cache access
- Parallel parsing of independent files
- Lock-free read path
- Proper synchronization on writes

### Disk Space Management
- Implement cache size limits
- LRU eviction for old entries
- Option to clear cache
- Cache location configuration

## Example Usage

```csharp
var compiler = new IncrementalCompiler(
    cacheDirectory: ".fifth-cache",
    maxCacheSize: 1024 * 1024 * 100 // 100 MB
);

// First build: parses everything
var result1 = compiler.Compile(options);
// Time: 30s

// Rebuild with no changes: uses cache
var result2 = compiler.Compile(options);
// Time: 1s (30x faster!)

// Rebuild with 1 file changed: incremental
var result3 = compiler.Compile(options);
// Time: 3s (10x faster!)
```

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #3
- Rust incremental compilation: https://blog.rust-lang.org/2016/09/08/incremental.html
- Roslyn incremental compilation design
- Salsa framework: https://github.com/salsa-rs/salsa
- Related Issues: Enables #ISSUE-002 (LSP), improves #ISSUE-005 (Pipeline)

## Estimated Effort

**20 weeks** (5 months)
- Weeks 1-4: Infrastructure
- Weeks 5-8: File-level caching
- Weeks 9-12: Dependency tracking
- Weeks 13-16: Incremental transformations
- Weeks 17-20: Integration and optimization

## Dependencies

- Issue #005: Composable Pipeline (helps with incremental transforms)
- Nice to have: #ISSUE-006 Enhanced Symbol Table (for better caching)

## Success Metrics

- 10x speedup for single-file changes
- 30x speedup for zero-change rebuilds
- LSP diagnostics <500ms response time
- Cache hit rate >90% for typical workflows
- Zero correctness issues (incremental == full build)
