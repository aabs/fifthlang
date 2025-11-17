# AST Node Contracts — Generic Type Support

**Feature**: `001-full-generics-support` | **Phase**: 1 (Design) | **Date**: 2025-11-18

This document defines the public API contracts for new AST nodes and type system interfaces introduced by the generic type support feature. These contracts are implemented as C# record types in `src/ast-model/AstMetamodel.cs` and are the source of truth for generated builders/visitors.

---

## AST Node Contracts

### TypeParameterDef

**Purpose**: Represents a generic type parameter declaration (e.g., `<T>`, `<T: IComparable>`).

**Public API**:
```csharp
public sealed record TypeParameterDef : AstNode
{
    /// <summary>
    /// Name of the type parameter (e.g., "T", "TKey", "TValue")
    /// INVARIANT: Must be valid identifier, not a Fifth keyword
    /// </summary>
    public required TypeParameterName Name { get; init; }
    
    /// <summary>
    /// Optional constraints on this type parameter
    /// Multiple constraints allowed: [IDisposable, IComparable, new()]
    /// INVARIANT: If present, constructor constraint must be last
    /// </summary>
    public TypeConstraint[]? Constraints { get; init; }
    
    /// <summary>
    /// Source location for diagnostics
    /// </summary>
    public required SourceLocation Location { get; init; }
}
```

**Usage Example**:
```csharp
var typeParam = new TypeParameterDef
{
    Name = TypeParameterName.Create("T").Value,
    Constraints = new TypeConstraint[]
    {
        new InterfaceConstraint { InterfaceType = new TypeName("IComparable"), ... },
        new ConstructorConstraint { ... }
    },
    Location = new SourceLocation(line: 5, column: 10)
};
```

**Validation Rules**:
1. `Name` must not be a Fifth reserved keyword (`int`, `string`, `class`, etc.)
2. If `Constraints` is non-null, array must not be empty
3. Constructor constraint (`ConstructorConstraint`) must appear last if present
4. Within a single type parameter list, all names must be unique

**Generated Code**:
- `TypeParameterDefBuilder` (fluent builder pattern)
- `BaseAstVisitor.VisitTypeParameterDef(TypeParameterDef node)`
- `DefaultRecursiveDescentVisitor.VisitTypeParameterDef(TypeParameterDef node)`

---

### TypeConstraint (Abstract Base)

**Purpose**: Base class for all generic type parameter constraints.

**Public API**:
```csharp
public abstract record TypeConstraint : AstNode
{
    /// <summary>
    /// Source location for diagnostics
    /// </summary>
    public required SourceLocation Location { get; init; }
}
```

**Derived Types**:
- `InterfaceConstraint`
- `BaseClassConstraint`
- `ConstructorConstraint`

**Validation Rules**:
1. Must be one of the three concrete derived types (sealed hierarchy)
2. Cannot instantiate `TypeConstraint` directly (abstract)

---

### InterfaceConstraint

**Purpose**: Requires type argument to implement a specific interface (e.g., `where T: IComparable`).

**Public API**:
```csharp
public sealed record InterfaceConstraint : TypeConstraint
{
    /// <summary>
    /// The interface type that T must implement
    /// INVARIANT: Must reference a valid interface type name
    /// </summary>
    public required TypeName InterfaceType { get; init; }
}
```

**Usage Example**:
```csharp
var constraint = new InterfaceConstraint
{
    InterfaceType = new TypeName("IComparable"),
    Location = new SourceLocation(line: 5, column: 20)
};
```

**Validation Rules**:
1. `InterfaceType` must resolve to an interface (checked during type resolution pass)
2. Type argument must implement this interface (checked during instantiation)

**Validation Performed By**:
- `TypeParameterResolutionVisitor` (semantic validation)

---

### BaseClassConstraint

**Purpose**: Requires type argument to derive from a specific base class (e.g., `where T: Animal`).

**Public API**:
```csharp
public sealed record BaseClassConstraint : TypeConstraint
{
    /// <summary>
    /// The base class type that T must inherit from
    /// INVARIANT: Must reference a valid class type name
    /// </summary>
    public required TypeName BaseClass { get; init; }
}
```

**Usage Example**:
```csharp
var constraint = new BaseClassConstraint
{
    BaseClass = new TypeName("Animal"),
    Location = new SourceLocation(line: 5, column: 20)
};
```

**Validation Rules**:
1. `BaseClass` must resolve to a class (not interface, struct, or primitive)
2. Type argument must derive from this base class (checked during instantiation)
3. At most one `BaseClassConstraint` allowed per type parameter

**Validation Performed By**:
- `TypeParameterResolutionVisitor` (semantic validation)

---

### ConstructorConstraint

**Purpose**: Requires type argument to have a parameterless public constructor (e.g., `where T: new()`).

**Public API**:
```csharp
public sealed record ConstructorConstraint : TypeConstraint
{
    // No additional properties — presence indicates constraint
}
```

**Usage Example**:
```csharp
var constraint = new ConstructorConstraint
{
    Location = new SourceLocation(line: 5, column: 30)
};
```

**Validation Rules**:
1. Must appear last in constraint list if present
2. Type argument must have a public parameterless constructor (checked during instantiation)

**Validation Performed By**:
- `TypeParameterResolutionVisitor` (semantic validation)

---

## Modified AST Node Contracts

### ClassDef (Extension)

**Purpose**: Existing class definition node extended with type parameters.

**Modified Public API**:
```csharp
public sealed record ClassDef : Statement
{
    public required ClassName Name { get; init; }
    
    /// <summary>
    /// Generic type parameters for this class (e.g., <T, U>)
    /// Null if non-generic class
    /// NEW: Previously did not exist
    /// </summary>
    public TypeParameterDef[]? TypeParameters { get; init; }  // NEW
    
    // ... existing properties (BaseClasses, Members, etc.)
}
```

**Backward Compatibility**:
- `TypeParameters = null` for non-generic classes (default behavior)
- Existing Fifth code without generics parses identically

**Example Fifth Syntax**:
```fifth
// Non-generic (TypeParameters = null)
class Person { name: string; }

// Generic (TypeParameters = [T])
class Stack<T> { items: List<T>; }
```

---

### FunctionDef (Extension)

**Purpose**: Existing function definition node with populated type parameters.

**Modified Public API**:
```csharp
public sealed record FunctionDef : Statement
{
    public required FunctionName Name { get; init; }
    
    /// <summary>
    /// Generic type parameters for this function (e.g., <T, U>)
    /// Null if non-generic function
    /// PREVIOUSLY: Had TODO comment and was unused
    /// NOW: Populated by AstBuilderVisitor when grammar matches type_parameter_list
    /// </summary>
    public TypeParameterDef[]? TypeParameters { get; init; }  // POPULATE (was unused)
    
    // ... existing properties (Parameters, ReturnType, Body, etc.)
}
```

**Backward Compatibility**:
- `TypeParameters = null` for non-generic functions (default behavior)
- Existing Fifth code without generics parses identically

**Example Fifth Syntax**:
```fifth
// Non-generic (TypeParameters = null)
add(x: int, y: int): int => x + y;

// Generic (TypeParameters = [T])
identity<T>(x: T): T => x;
```

---

## Type System Contracts

### FifthType Extensions

**Purpose**: Discriminated union for type system extended with generic type variants.

**New Variants**:

#### TGenericParameter

**Purpose**: Represents an unresolved/unbound generic type parameter (e.g., `T` in `class Stack<T>`).

**Public API**:
```csharp
public sealed record TGenericParameter : FifthType
{
    /// <summary>
    /// Name of the type parameter
    /// </summary>
    public required TypeParameterName Name { get; init; }
    
    /// <summary>
    /// Constraints on this type parameter (for validation)
    /// </summary>
    public TypeConstraint[]? Constraints { get; init; }
    
    /// <summary>
    /// Scope where this type parameter was declared
    /// Used to resolve name conflicts in nested generics
    /// </summary>
    public required TypeParameterScope Scope { get; init; }
}
```

**Equality Semantics**:
- Two `TGenericParameter` instances are equal if:
  1. `Name` is equal (string comparison)
  2. `Scope` is equal (same declaring entity and scope ID)

**Usage Context**:
- Created during AST construction for type parameter declarations
- Replaced with `TGenericInstance` during type parameter resolution pass
- Should not appear in final IL AST (must be resolved before code generation)

---

#### TGenericInstance

**Purpose**: Represents a resolved/bound generic type instantiation (e.g., `Stack<int>`).

**Public API**:
```csharp
public sealed record TGenericInstance : FifthType
{
    /// <summary>
    /// The generic type definition (e.g., "Stack" in Stack<int>)
    /// References a ClassDef or FunctionDef with TypeParameters
    /// </summary>
    public required TypeName GenericTypeDefinition { get; init; }
    
    /// <summary>
    /// Concrete type arguments (e.g., [int] for Stack<int>)
    /// INVARIANT: Length must match number of type parameters in definition
    /// </summary>
    public required FifthType[] TypeArguments { get; init; }
    
    /// <summary>
    /// Cached hash for type equality checks (structural hashing)
    /// Used by GenericTypeCache for interning
    /// </summary>
    internal int CachedHash { get; init; }
}
```

**Equality Semantics**:
- Two `TGenericInstance` instances are equal if:
  1. `GenericTypeDefinition` is equal (name comparison)
  2. `TypeArguments` are structurally equal (element-wise comparison)

**Caching**:
- All instances are interned via `GenericTypeCache`
- Ensures `Stack<int>` is the same type instance everywhere in the program
- Cache uses structural hashing for fast lookups

**Usage Context**:
- Created during type parameter resolution or type inference passes
- Appears in final IL AST and maps to .NET generic types in Roslyn backend

---

### TypeParameterScope

**Purpose**: Represents the scope where a type parameter is declared (for name conflict resolution).

**Public API**:
```csharp
public sealed record TypeParameterScope
{
    /// <summary>
    /// Kind of scope (class, function, method)
    /// </summary>
    public required ScopeKind Kind { get; init; }
    
    /// <summary>
    /// Name of the declaring entity (class name or function name)
    /// </summary>
    public required string DeclaringEntity { get; init; }
    
    /// <summary>
    /// Unique scope ID (for nested scopes with same name)
    /// </summary>
    public required Guid ScopeId { get; init; }
}

public enum ScopeKind
{
    Class,
    Function,
    Method  // For methods inside generic classes
}
```

**Equality Semantics**:
- Two scopes are equal if `ScopeId` matches (GUID comparison)

**Usage Context**:
- Assigned during AST construction by `AstBuilderVisitor`
- Used during type parameter resolution to distinguish nested type parameters:
  ```fifth
  class Outer<T> {
      inner<T>(): T { ... }  // Inner T is distinct from outer T
  }
  ```

---

## Type Inference Contracts

### TypeInferenceContext

**Purpose**: Context for type inference during a single generic function/method call.

**Public API**:
```csharp
public sealed class TypeInferenceContext
{
    /// <summary>
    /// Map of type parameter names to inferred types
    /// Populated incrementally as arguments are analyzed
    /// </summary>
    public Dictionary<TypeParameterName, FifthType> InferredTypes { get; init; }
    
    /// <summary>
    /// Constraints that must be satisfied by inferred types
    /// Checked after all type arguments are inferred
    /// </summary>
    public List<(TypeParameterName Param, TypeConstraint Constraint)> PendingConstraints { get; init; }
    
    /// <summary>
    /// Diagnostics collected during inference (errors, ambiguities)
    /// </summary>
    public List<Diagnostic> Diagnostics { get; init; }
    
    /// <summary>
    /// Location of the call site (for error reporting)
    /// </summary>
    public required SourceLocation CallSite { get; init; }
    
    /// <summary>
    /// Whether inference succeeded (all type parameters resolved)
    /// </summary>
    public bool IsComplete { get; }
    
    public required int ExpectedParameterCount { get; init; }
}
```

**Lifecycle**:
1. Created by `GenericTypeInferenceVisitor` at function/method call site
2. Accumulates type information from arguments, return type context
3. Validates constraints after inference completes
4. Reports diagnostics if inference fails

**Thread Safety**:
- **Not thread-safe** — each call site gets its own context instance
- Compiler is single-threaded during inference pass

---

### GenericTypeCache

**Purpose**: Singleton cache for generic type instantiations (ensures structural equality).

**Public API**:
```csharp
public sealed class GenericTypeCache
{
    /// <summary>
    /// Get or create a generic type instance
    /// Uses structural hashing for fast lookups
    /// </summary>
    public TGenericInstance GetOrCreate(TypeName genericTypeDef, FifthType[] typeArguments);
    
    /// <summary>
    /// Clear cache (called between compilation units)
    /// </summary>
    public void Clear();
}
```

**Usage Example**:
```csharp
var cache = GenericTypeCache.Instance;  // Singleton

var listOfInt = cache.GetOrCreate(
    new TypeName("List"),
    new[] { FifthType.TType(new TypeName("int")) }
);

// Same instance returned for subsequent calls
var listOfInt2 = cache.GetOrCreate(
    new TypeName("List"),
    new[] { FifthType.TType(new TypeName("int")) }
);

Assert.Same(listOfInt, listOfInt2);  // Reference equality
```

**Thread Safety**:
- **Not thread-safe** — compiler is single-threaded
- Cache is cleared between compilation units (not persisted)

---

## Visitor Pattern Extensions

### Generated Visitor Methods

**BaseAstVisitor** (read-only analysis):
```csharp
public abstract partial class BaseAstVisitor
{
    public virtual void VisitTypeParameterDef(TypeParameterDef node);
    public virtual void VisitInterfaceConstraint(InterfaceConstraint node);
    public virtual void VisitBaseClassConstraint(BaseClassConstraint node);
    public virtual void VisitConstructorConstraint(ConstructorConstraint node);
}
```

**DefaultRecursiveDescentVisitor** (type-preserving rewrites):
```csharp
public abstract partial class DefaultRecursiveDescentVisitor : BaseAstVisitor
{
    public override TypeParameterDef VisitTypeParameterDef(TypeParameterDef node);
    public override InterfaceConstraint VisitInterfaceConstraint(InterfaceConstraint node);
    public override BaseClassConstraint VisitBaseClassConstraint(BaseClassConstraint node);
    public override ConstructorConstraint VisitConstructorConstraint(ConstructorConstraint node);
}
```

**Usage Example** (Custom Visitor):
```csharp
public class GenericTypeCollector : BaseAstVisitor
{
    public List<TypeParameterDef> TypeParameters { get; } = new();
    
    public override void VisitTypeParameterDef(TypeParameterDef node)
    {
        TypeParameters.Add(node);
        base.VisitTypeParameterDef(node);  // Visit constraints
    }
}
```

---

## Contract Invariants

### Global Invariants

1. **Type Parameter Uniqueness**: Within a single `type_parameter_list`, all type parameter names must be unique
2. **Type Argument Count**: `TGenericInstance.TypeArguments.Length` must equal number of type parameters in generic definition
3. **Constraint Ordering**: Constructor constraint must be last if present
4. **Scope Validity**: `TypeParameterScope.DeclaringEntity` must reference a valid `ClassDef` or `FunctionDef`
5. **Hash Consistency**: `TGenericInstance.CachedHash` must match `ComputeHash(GenericTypeDefinition, TypeArguments)`
6. **Cache Interning**: All `TGenericInstance` instances for the same generic type + type arguments must be reference-equal (same instance)

### Validation Points

- **Parse time** (ANTLR grammar):
  - Validates syntax: `<>` not allowed, at least one type parameter required
  - Validates token sequences: `WHERE` keyword precedes constraint clauses
  
- **AST construction** (`AstBuilderVisitor`):
  - Validates type parameter name uniqueness within a list
  - Validates constraint order (constructor constraint last)
  - Assigns scope IDs to type parameters
  
- **Type resolution** (`TypeParameterResolutionVisitor`):
  - Validates type argument count matches type parameter count
  - Validates constraint satisfaction (interface implementation, base class inheritance, constructor existence)
  - Resolves `TGenericParameter` to `TGenericInstance`
  
- **Type inference** (`GenericTypeInferenceVisitor`):
  - Validates all type parameters can be inferred from usage
  - Validates inferred types satisfy constraints
  - Reports `GEN002` diagnostic if inference fails

---

## Diagnostic Codes

New diagnostic codes introduced by generic type support:

| Code | Severity | Description | Example |
|------|----------|-------------|---------|
| `GEN001` | Error | Wrong number of type arguments | `Pair<int>` when `Pair<T, U>` expects 2 |
| `GEN002` | Error | Type argument inference failed | `identity(42)` when result type unknown |
| `GEN003` | Error | Type argument does not satisfy constraint | `max<MyClass>(...)` when `MyClass` doesn't implement `IComparable` |
| `GEN004` | Error | Type argument missing parameterless constructor | `create<MyClass>()` when `MyClass` has no default constructor |
| `GEN005` | Warning | Redundant explicit type argument | `identity<int>(42)` when `T` can be inferred |
| `GEN006` | Info | Type argument inferred | `identity(42)` → `T = int` (verbose mode) |

**Diagnostic Format**:
```
GEN003: Type argument 'MyClass' does not satisfy constraint 'IComparable<MyClass>'
  at line 10, column 5: max(obj1, obj2)
  required by constraint 'IComparable<T>' on type parameter 'T' of function 'max'
```

---

## Summary

This document defines the contracts for:

- **3 new AST nodes**: `TypeParameterDef`, `InterfaceConstraint`, `BaseClassConstraint`, `ConstructorConstraint`
- **2 modified AST nodes**: `ClassDef`, `FunctionDef` (added `TypeParameters` property)
- **2 new FifthType variants**: `TGenericParameter`, `TGenericInstance`
- **2 new type system types**: `TypeParameterScope`, `TypeInferenceContext`
- **1 new cache**: `GenericTypeCache`
- **6 new diagnostic codes**: `GEN001` through `GEN006`

All contracts follow Fifth's constitution: strongly-typed record types, immutability, generator-driven code generation, multi-pass compilation architecture.

**Next**: Update agent context files (AGENTS.md, constitution.md) with generic type support information.
