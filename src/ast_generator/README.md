# AST Generator - Visitor and Rewriter Patterns Guide

This project generates several AST traversal and manipulation patterns for the Fifth language compiler. Understanding which pattern to use for different transformations is critical for maintaining clean, effective compiler passes.

## Overview

The AST generator produces multiple code generation artifacts based on the AST model definitions in `src/ast-model/AstMetamodel.cs` and `src/ast-model/ILMetamodel.cs`:

### Generated Files

| File | Purpose | Use Case |
|------|---------|----------|
| `builders.generated.cs` | Builder pattern classes | Constructing AST nodes programmatically |
| `visitors.generated.cs` | Visitor pattern classes | Read-only AST traversal and analysis |
| `rewriter.generated.cs` | Rewriter pattern classes | AST transformations with statement hoisting |
| `typeinference.generated.cs` | Type inference support | Type checking and inference passes |
| `il.builders.generated.cs` | IL builder pattern | Constructing IL AST nodes |
| `il.visitors.generated.cs` | IL visitor pattern | IL AST traversal |
| `il.rewriter.generated.cs` | IL rewriter pattern | IL AST transformations |

## Visitor Patterns Explained

### 1. BaseAstVisitor / IAstVisitor

**Purpose**: Read-only traversal for analysis, validation, or diagnostic passes.

**When to use**:
- Gathering information (e.g., symbol table building)
- Validation and error checking
- Metrics and statistics collection
- Any pass that doesn't modify the AST

**Pattern**:
```csharp
public class MyAnalysisVisitor : BaseAstVisitor
{
    public override void EnterBinaryExp(BinaryExp ctx)
    {
        // Called when entering a binary expression node
        // Perform analysis, collect data, emit diagnostics
    }
    
    public override void LeaveBinaryExp(BinaryExp ctx)
    {
        // Called when leaving a binary expression node
        // Finalize analysis for this node
    }
}
```

**Examples in codebase**:
- `SymbolTableBuilderVisitor` - Builds symbol tables
- `TripleDiagnosticsVisitor` - Emits diagnostics for triple patterns
- `ExternalCallValidationVisitor` - Validates external function calls
- `DumpTreeVisitor` - Prints AST structure for debugging

**Characteristics**:
- ✅ Simple enter/leave pattern
- ✅ No return values
- ✅ Side effects only (e.g., collecting data, emitting diagnostics)
- ❌ Cannot modify AST
- ❌ Cannot change node types

---

### 2. DefaultRecursiveDescentVisitor / IAstRecursiveDescentVisitor

**Purpose**: AST transformations that preserve node types (same-type rewrites).

**When to use**:
- Structure-preserving transformations
- Rewrites where each node type remains the same type
- Simple AST modifications without type changes
- Passes that need to modify AST but don't require statement hoisting

**Pattern**:
```csharp
public class MyTransformVisitor : DefaultRecursiveDescentVisitor
{
    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        // Recursively transform children first
        var transformed = base.VisitBinaryExp(ctx);
        
        // Apply transformation (same type returned)
        if (transformed.Operator == Operator.ArithmeticAdd)
        {
            return transformed with { /* modifications */ };
        }
        
        return transformed;
    }
}
```

**Examples in codebase**:
- `OverloadTransformingVisitor` - Transforms overloaded functions
- `DestructuringVisitor` - Expands destructuring patterns
- `TypeAnnotationVisitor` - Adds type annotations
- `VarRefResolverVisitor` - Resolves variable references
- `TreeLinkageVisitor` - Links parent-child relationships

**Characteristics**:
- ✅ Type-safe returns (BinaryExp → BinaryExp)
- ✅ Recursive descent with automatic child traversal
- ✅ Can modify nodes using `with` expressions
- ❌ Cannot change node types (BinaryExp → FuncCallExp not allowed)
- ❌ No statement hoisting support
- ❌ Cannot introduce new statements from expressions

---

### 3. DefaultAstRewriter / IAstRewriter (NEW)

**Purpose**: Advanced AST transformations with cross-type rewrites and statement-level desugaring.

**When to use** (⭐ PREFERRED for new lowering passes):
- **Statement-level lowering**: Need to introduce temporary variables, pre-computation statements
- **Cross-type rewrites**: Transform one expression type into another (e.g., `BinaryExp` → `FuncCallExp`)
- **Desugaring operations**: Break down high-level constructs into simpler primitives
- **Expression hoisting**: Pull sub-expressions into separate statements
- **Any transformation requiring statement insertion**

**Pattern**:
```csharp
public class MyLoweringRewriter : DefaultAstRewriter
{
    private int _tmpCounter = 0;
    
    public override RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        // Recursively rewrite children
        var lhs = Rewrite(ctx.LHS);
        var rhs = Rewrite(ctx.RHS);
        
        // Collect child prologues
        var prologue = new List<Statement>();
        prologue.AddRange(lhs.Prologue);
        prologue.AddRange(rhs.Prologue);
        
        if (ctx.Operator == Operator.ArithmeticAdd)
        {
            // Lower to a function call + temp variable
            var tmpName = $"__tmp{_tmpCounter++}";
            var tmpDecl = new VariableDecl 
            { 
                Name = tmpName,
                TypeName = TypeName.From("int"),
                // ... other properties
            };
            
            // Create function call (cross-type rewrite!)
            var funcCall = new FuncCallExp
            {
                FunctionDef = /* resolve add function */,
                InvocationArguments = [(Expression)lhs.Node, (Expression)rhs.Node]
            };
            
            // Hoist temp declaration to prologue
            var declStmt = new VarDeclStatement 
            { 
                VariableDecl = tmpDecl,
                InitialValue = funcCall
            };
            prologue.Add(declStmt);
            
            // Return reference to temp
            var tmpRef = new VarRefExp 
            { 
                VarName = tmpName,
                VariableDecl = tmpDecl
            };
            return new RewriteResult(tmpRef, prologue);
        }
        
        // Default: preserve structure
        var rebuilt = ctx with
        {
            LHS = (Expression)lhs.Node,
            RHS = (Expression)rhs.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
}
```

**Examples in codebase**:
- `GraphAssertionLoweringVisitor` - Could be migrated to use rewriter
- `GraphTripleOperatorLoweringVisitor` - Candidate for migration
- `TripleExpansionVisitor` - Candidate for migration
- *(Future passes that need statement hoisting)*

**Characteristics**:
- ✅ Cross-type rewrites (any Expression → any Expression)
- ✅ Statement hoisting via `Prologue`
- ✅ Automatic prologue bubbling to `BlockStatement`
- ✅ Category-level flexibility
- ✅ Perfect for desugaring and lowering
- ✅ Returns `RewriteResult` with node + hoisted statements
- ⚠️ More complex than `DefaultRecursiveDescentVisitor`
- ⚠️ Requires understanding of prologue propagation

**How Prologue Hoisting Works**:

1. Expression rewrites can populate `Prologue` with statements
2. Prologues bubble upward through parent nodes
3. `BlockStatement` consumes prologues and splices them before each statement
4. Result: hoisted statements appear at the correct scope

Example transformation:
```
Input:
  BlockStatement
  └── ExpStatement
      └── BinaryExp(5 + 3)

Output (with hoisting):
  BlockStatement
  ├── VarDeclStatement(__tmp0 = add(5, 3))  ← Hoisted
  └── ExpStatement
      └── VarRefExp(__tmp0)                ← Original replaced
```

---

### 4. NullSafeRecursiveDescentVisitor

**Purpose**: Variation of `DefaultRecursiveDescentVisitor` with null-safety checks.

**When to use**:
- Same as `DefaultRecursiveDescentVisitor`
- When AST nodes might be null (incomplete/partial trees)
- For defensive programming in transformation passes

**Pattern**:
```csharp
public class MySafeTransformVisitor : NullSafeRecursiveDescentVisitor
{
    // Same as DefaultRecursiveDescentVisitor but with null checks
    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        if (ctx == null) return null;
        return base.VisitBinaryExp(ctx);
    }
}
```

**Examples in codebase**:
- `GraphAssertionLoweringVisitor` - Uses null-safe pattern

---

## Decision Guide: Which Pattern to Use?

### Use **BaseAstVisitor** when:
- ✅ You need to analyze the AST (read-only)
- ✅ You're collecting information (symbols, metrics, diagnostics)
- ✅ You don't need to modify the AST

### Use **DefaultRecursiveDescentVisitor** when:
- ✅ You need to modify the AST
- ✅ Each node type stays the same type
- ✅ You don't need statement hoisting
- ✅ Simple transformations (property changes, subtree replacement)

### Use **DefaultAstRewriter** when (⭐ PREFERRED for lowering):
- ✅ You need cross-type rewrites (BinaryExp → FuncCallExp)
- ✅ You need to introduce temporary variables
- ✅ You need statement-level desugaring
- ✅ You need to hoist statements from expressions
- ✅ You're implementing a lowering pass
- ✅ You need category-level flexibility

### Use **NullSafeRecursiveDescentVisitor** when:
- ✅ Same as `DefaultRecursiveDescentVisitor`
- ✅ You need defensive null handling

---

## Migration Path

For **existing lowering passes** that would benefit from statement hoisting:

1. **Identify candidates**: Passes that currently struggle with:
   - Needing temporary variables
   - Complex multi-statement rewrites
   - Cross-type transformations

2. **Migrate incrementally**:
   - Change base class from `DefaultRecursiveDescentVisitor` to `DefaultAstRewriter`
   - Update method signatures to return `RewriteResult`
   - Add prologue accumulation where needed
   - Test thoroughly

3. **Example migration**:
```csharp
// Before:
public class MyPass : DefaultRecursiveDescentVisitor
{
    public override BinaryExp VisitBinaryExp(BinaryExp ctx)
    {
        // Limited to same-type return
        return ctx with { /* changes */ };
    }
}

// After:
public class MyPass : DefaultAstRewriter
{
    public override RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        var prologue = new List<Statement>();
        // Can now return different type + hoist statements
        return new RewriteResult(someOtherExpression, prologue);
    }
}
```

---

## Code Generation

To regenerate all AST infrastructure:

```bash
# From repository root
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

# Or use just
just run-generator
```

The generator reads from:
- `src/ast-model/AstMetamodel.cs` - Core AST definitions
- `src/ast-model/ILMetamodel.cs` - IL AST definitions

And produces all the files listed in the table above.

---

## Design Principles

1. **Separation of Concerns**: Different patterns for different use cases
2. **Type Safety**: `DefaultRecursiveDescentVisitor` ensures type-safe rewrites
3. **Flexibility**: `DefaultAstRewriter` enables radical transformations
4. **Incremental Adoption**: New patterns don't break existing code
5. **Code Generation**: All patterns are generated consistently from the AST model

---

## Future Enhancements

Potential additions to the rewriter pattern:
- Epilogue support (cleanup code after statements)
- Multiple prologue splicing points (not just `BlockStatement`)
- Scope-aware hoisting strategies
- Rewriter composition helpers

---

## See Also

- [AST Rewriter Design Documentation](../../docs/AST_REWRITER_DESIGN.md) - Detailed rewriter design
- [Constitution](../../specs/.specify/memory/constitution.md) - Overall architecture
- [AGENTS.md](../../AGENTS.md) - Agent workflow guidelines
