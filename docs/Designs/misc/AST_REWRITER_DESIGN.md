# AST Rewriter Design

## Overview

The AST Rewriter is a new generated API that enables cross-type, category-safe AST rewrites and statement-level desugaring via a `RewriteResult` that carries both the rewritten node and a list of hoisted statements. This facility exists in parallel to the existing `DefaultRecursiveDescentVisitor` without modifying any current language passes or transformations.

## Generated Files

The rewriter generation produces two files:
- `src/ast-generated/rewriter.generated.cs` - Core AST rewriter (~1000 lines)
- `src/ast-generated/il.rewriter.generated.cs` - IL AST rewriter (~440 lines)

## Key Components

### 1. RewriteResult Record

```csharp
public record RewriteResult(AstThing Node, List<Statement> Prologue)
{
    public static RewriteResult From(AstThing node) => new(node, []);
}
```

The `RewriteResult` carries:
- `Node`: The rewritten AST node (may be any `AstThing`, typically same category as input)
- `Prologue`: List of statements to be emitted before the containing statement
- Factory method `From(node)` creates a result with an empty prologue

### 2. IAstRewriter Interface

```csharp
public interface IAstRewriter
{
    RewriteResult Rewrite(AstThing ctx);
    RewriteResult VisitXxx(Xxx ctx);  // For all concrete AST nodes
}
```

All `VisitXxx` methods return `RewriteResult` instead of the concrete type, enabling:
- Cross-type rewrites (e.g., `BinaryExp` → `FuncCallExp`)
- Category-level flexibility (any `Expression` can become any other `Expression`)
- Statement hoisting via prologue accumulation

### 3. DefaultAstRewriter Class

The `DefaultAstRewriter` provides a structure-preserving default implementation:

```csharp
public class DefaultAstRewriter : IAstRewriter
{
    public virtual RewriteResult Rewrite(AstThing ctx);
    public virtual RewriteResult VisitXxx(Xxx ctx);  // For all concrete nodes
}
```

**Key behaviors:**
- Recursively rewrites all visitable children
- Aggregates child `Prologue` lists during traversal
- Rebuilds nodes using C# record `with` expressions
- Returns `new RewriteResult(rebuiltNode, aggregatedPrologue)`

**Special handling for BlockStatement:**
```csharp
public virtual RewriteResult VisitBlockStatement(BlockStatement ctx)
{
    List<Statement> outStatements = [];
    foreach (var st in ctx.Statements)
    {
        var rr = Rewrite(st);
        outStatements.AddRange(rr.Prologue);  // Splice prologue
        outStatements.Add((Statement)rr.Node);
    }
    return new RewriteResult(ctx with { Statements = outStatements }, []);
}
```

BlockStatement consumes all prologues from child statements and splices them into the statement list. The returned prologue is always empty, preventing prologue leakage beyond blocks.

## Prologue Propagation

### For Non-Collection Properties

```csharp
var rrChild = Rewrite((AstThing)ctx.Child);
prologue.AddRange(rrChild.Prologue);
// ... later in rebuild:
Child = (ChildType)rrChild.Node
```

Each child rewrite result is captured, its prologue is accumulated, and its node is used in the rebuild.

### For Collection Properties

```csharp
List<ChildType> tmpChildren = [];
foreach (var item in ctx.Children)
{
    var rr = Rewrite(item);
    tmpChildren.Add((ChildType)rr.Node);
    prologue.AddRange(rr.Prologue);
}
// ... later in rebuild:
Children = tmpChildren
```

All child prologues are aggregated during collection traversal.

## Usage Example

Here's a rewriter that hoists temporary variables for addition expressions:

```csharp
public class IntroduceTempsRewriter : DefaultAstRewriter
{
    private int _tmpCounter = 0;

    public override RewriteResult VisitBinaryExp(BinaryExp ctx)
    {
        // Rewrite children first
        var lhs = Rewrite(ctx.LHS);
        var rhs = Rewrite(ctx.RHS);
        
        // Collect child prologues
        var prologue = new List<Statement>();
        prologue.AddRange(lhs.Prologue);
        prologue.AddRange(rhs.Prologue);

        if (ctx.Operator == Operator.ArithmeticAdd)
        {
            // Create temporary variable
            var tmpName = $"__tmp{_tmpCounter++}";
            var tmpDecl = new VariableDecl 
            { 
                Name = tmpName,
                TypeName = TypeName.From("int"),
                CollectionType = CollectionType.SingleInstance,
                Visibility = Visibility.Private
            };
            
            // Create rewritten expression
            var rewrittenBinary = ctx with
            {
                LHS = (Expression)lhs.Node,
                RHS = (Expression)rhs.Node
            };
            
            // Hoist: add declaration with initializer to prologue
            var declStmt = new VarDeclStatement 
            { 
                VariableDecl = tmpDecl,
                InitialValue = rewrittenBinary
            };
            prologue.Add(declStmt);
            
            // Return reference to temp instead of binary expression
            var tmpRef = new VarRefExp 
            { 
                VarName = tmpName,
                VariableDecl = tmpDecl
            };
            return new RewriteResult(tmpRef, prologue);
        }

        // Default: rebuild with rewritten children
        var rebuilt = ctx with
        {
            LHS = (Expression)lhs.Node,
            RHS = (Expression)rhs.Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
}
```

### What Happens

Given this input AST:
```
BlockStatement
└── ExpStatement
    └── BinaryExp(+)
        ├── LHS: Int32LiteralExp(5)
        └── RHS: Int32LiteralExp(3)
```

The rewriter produces:
```
BlockStatement
├── VarDeclStatement(__tmp0 = 5 + 3)  // Hoisted
└── ExpStatement
    └── VarRefExp(__tmp0)  // Original expression replaced
```

The prologue bubbles up from `BinaryExp` → `ExpStatement` → `BlockStatement`, where it's consumed and spliced into the statement list.

## Design Rationale

### Why Parallel to DefaultRecursiveDescentVisitor?

The existing `DefaultRecursiveDescentVisitor` has type-safe return values (e.g., `VisitBinaryExp(BinaryExp) : BinaryExp`), which prevents cross-type rewrites. Rather than breaking existing passes, we provide a new API that:
- Enables radical transformations
- Supports statement-level desugaring
- Can be adopted incrementally

### Why RewriteResult?

Returning `RewriteResult` instead of raw nodes enables:
- **Prologue accumulation**: Child statement hoisting bubbles upward
- **Uniform interface**: All visits have the same return type
- **Flexibility**: Callers can inspect both the node and any hoisted statements

### Why Special-Case BlockStatement?

Only `BlockStatement` can introduce new statements into the AST. By having it consume prologues and splice them into its statement list, we ensure:
- Hoisted statements appear in the right scope
- Prologues don't leak beyond blocks
- Clear semantics for where hoisted code appears

## Implementation Details

### Generation Process

1. `RazorLightRewriterGenerator` (similar to visitor/builder generators)
2. `Templates/Rewriter.cshtml` template iterates concrete AST types
3. For each type:
   - Generate `VisitXxx` method
   - Rewrite visitable children (collections and non-collections)
   - Aggregate child prologues
   - Rebuild using `with` expression
   - Return `RewriteResult`
4. Special-case `BlockStatement` to consume prologues

### Testing

Two test files demonstrate functionality:
- `test/ast-tests/AstRewriterTests.cs` - xUnit tests
- `test/ast-tests/AstRewriterManualTest.cs` - Standalone executable test

Tests cover:
1. Structure-preserving default behavior
2. Statement-level hoisting with BlockStatement consumption
3. RewriteResult factory method
4. Cross-type rewrites (BinaryExp → VarRefExp via temp hoisting)

## Future Extensions

Potential enhancements:
- Additional prologue splicing points (e.g., specialized blocks)
- Epilogue support for cleanup code
- Scope-aware hoisting strategies
- Migration of existing passes to use the rewriter

## Non-Goals

- Refactoring existing passes (they continue using DefaultRecursiveDescentVisitor)
- Changing AST shapes or grammar
- Adding null-safety beyond what's needed for rewriting
- Performance optimization (this is a correctness-focused facility)
