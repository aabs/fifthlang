# Phase 1: Data Model — Full Generics Support

**Feature**: `001-full-generics-support` | **Phase**: 1 (Design) | **Date**: 2025-11-18

## Overview

This document defines the AST node extensions, type system additions, and data structures required to implement full generic type support in the Fifth programming language. All changes follow Fifth's constitution: metamodel-as-source-of-truth, generator-driven code generation, multi-pass compilation architecture.

## AST Metamodel Extensions

### 1. Type Parameter Definition

**Location**: `src/ast-model/AstMetamodel.cs`

```csharp
/// <summary>
/// Represents a generic type parameter declaration (e.g., <T>, <T: IComparable>)
/// </summary>
public sealed record TypeParameterDef : AstNode
{
    /// <summary>
    /// Name of the type parameter (e.g., "T", "TKey", "TValue")
    /// </summary>
    public required TypeParameterName Name { get; init; }
    
    /// <summary>
    /// Optional constraints on this type parameter
    /// Multiple constraints are allowed (e.g., T: IDisposable, IComparable, new())
    /// </summary>
    public TypeConstraint[]? Constraints { get; init; }
    
    /// <summary>
    /// Source location for diagnostics
    /// </summary>
    public required SourceLocation Location { get; init; }
}
```

**Value Object**:
```csharp
/// <summary>
/// Strong-typed name for a generic type parameter
/// Validates identifier rules (no special chars, not a keyword)
/// </summary>
public record TypeParameterName : IComparable<TypeParameterName>
{
    public required string Name { get; init; }
    
    // Validation: Must be valid identifier, not a Fifth keyword
    public static Result<TypeParameterName> Create(string name) { ... }
}
```

---

### 2. Type Constraint Hierarchy

**Location**: `src/ast-model/AstMetamodel.cs`

```csharp
/// <summary>
/// Base class for all generic type parameter constraints
/// </summary>
public abstract record TypeConstraint : AstNode
{
    public required SourceLocation Location { get; init; }
}

/// <summary>
/// Interface constraint (e.g., where T: IComparable)
/// Requires the type argument to implement the specified interface
/// </summary>
public sealed record InterfaceConstraint : TypeConstraint
{
    public required TypeName InterfaceType { get; init; }
}

/// <summary>
/// Base class constraint (e.g., where T: Animal)
/// Requires the type argument to derive from the specified base class
/// </summary>
public sealed record BaseClassConstraint : TypeConstraint
{
    public required TypeName BaseClass { get; init; }
}

/// <summary>
/// Constructor constraint (e.g., where T: new())
/// Requires the type argument to have a parameterless constructor
/// </summary>
public sealed record ConstructorConstraint : TypeConstraint
{
    // No additional properties - presence indicates constraint
}

/// <summary>
/// Value type constraint (e.g., where T: struct) - FUTURE ITERATION
/// Requires the type argument to be a value type
/// </summary>
// public sealed record StructConstraint : TypeConstraint { }

/// <summary>
/// Reference type constraint (e.g., where T: class) - FUTURE ITERATION
/// Requires the type argument to be a reference type
/// </summary>
// public sealed record ClassConstraint : TypeConstraint { }
```

**Design Notes**:
- Constraint evaluation order: base class → interfaces → constructor (matches C# semantics)
- Multiple interface constraints allowed, only one base class constraint
- Constructor constraint must be last if present
- Validation happens in `TypeParameterResolutionVisitor`

---

### 3. Modifications to Existing AST Nodes

#### ClassDef Extension

```csharp
/// <summary>
/// Existing class definition node - ADD type parameters
/// </summary>
public sealed record ClassDef : Statement
{
    public required ClassName Name { get; init; }
    
    /// <summary>
    /// Generic type parameters for this class (e.g., <T, U>)
    /// Null if non-generic class
    /// </summary>
    public TypeParameterDef[]? TypeParameters { get; init; }  // NEW
    
    // ... existing properties (base classes, members, etc.)
}
```

**Example Fifth Syntax**:
```fifth
class Stack<T> {
    items: List<T>;
    push(item: T) { ... }
    pop(): T { ... }
}
```

#### FunctionDef Extension

```csharp
/// <summary>
/// Existing function definition node - POPULATE type parameters
/// </summary>
public sealed record FunctionDef : Statement
{
    public required FunctionName Name { get; init; }
    
    /// <summary>
    /// Generic type parameters for this function (e.g., <T, U>)
    /// Null if non-generic function
    /// Previously had TODO comment - now populated
    /// </summary>
    public TypeParameterDef[]? TypeParameters { get; init; }  // POPULATE (was unused)
    
    // ... existing properties (parameters, return type, body, etc.)
}
```

**Example Fifth Syntax**:
```fifth
identity<T>(x: T): T => x;

swap<T, U>(a: T, b: U): (U, T) => (b, a);
```

---

## Type System Extensions

### 4. FifthType Discriminated Union Additions

**Location**: `src/ast-model/AstMetamodel.cs` (within existing `FifthType` partial record)

```csharp
public partial record FifthType
{
    // ... existing variants (TType, TDotnetType, TFunc, TArrayOf, TListOf)
    
    /// <summary>
    /// Generic type parameter (unresolved/unbound)
    /// Represents a type parameter before instantiation (e.g., T in class Stack<T>)
    /// </summary>
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
        /// (for resolving name conflicts between nested generics)
        /// </summary>
        public required TypeParameterScope Scope { get; init; }
    }
    
    /// <summary>
    /// Generic type instance (resolved/bound)
    /// Represents a concrete instantiation of a generic type (e.g., Stack<int>)
    /// </summary>
    public sealed record TGenericInstance : FifthType
    {
        /// <summary>
        /// The generic type definition (e.g., Stack)
        /// References a ClassDef or FunctionDef with TypeParameters
        /// </summary>
        public required TypeName GenericTypeDefinition { get; init; }
        
        /// <summary>
        /// Concrete type arguments (e.g., [int] for Stack<int>)
        /// Length must match number of type parameters in definition
        /// </summary>
        public required FifthType[] TypeArguments { get; init; }
        
        /// <summary>
        /// Cached hash for type equality checks (structural hashing)
        /// Used by GenericTypeCache for interning
        /// </summary>
        internal int CachedHash { get; init; }
    }
}
```

**Type Equality Rules**:
- `TGenericParameter` equality: Same name AND same scope
- `TGenericInstance` equality: Same `GenericTypeDefinition` AND structurally equal `TypeArguments[]`
- Cached hash used for fast lookups in `GenericTypeCache`

---

### 5. Type Parameter Scoping

```csharp
/// <summary>
/// Represents the scope where a type parameter is declared
/// Used to resolve name conflicts (e.g., nested generic classes)
/// </summary>
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
    /// Unique scope ID (for nested scopes)
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

**Scoping Rules**:
- Outer type parameters shadow inner with same name: `class Outer<T> { inner<T>(): T { ... } }` — inner `T` is distinct
- Scope resolution happens during `TypeParameterResolutionVisitor` pass
- Scope IDs assigned during AST construction by `AstBuilderVisitor`

---

## Type Inference Data Structures

### 6. Type Inference Context

```csharp
/// <summary>
/// Context for type inference during a single generic function/method call
/// Tracks type argument bindings and constraint violations
/// </summary>
public sealed class TypeInferenceContext
{
    /// <summary>
    /// Map of type parameter names to inferred types
    /// Populated incrementally as arguments are analyzed
    /// </summary>
    public Dictionary<TypeParameterName, FifthType> InferredTypes { get; init; } = new();
    
    /// <summary>
    /// Constraints that must be satisfied by inferred types
    /// Checked after all type arguments are inferred
    /// </summary>
    public List<(TypeParameterName Param, TypeConstraint Constraint)> PendingConstraints { get; init; } = new();
    
    /// <summary>
    /// Diagnostics collected during inference (errors, ambiguities)
    /// </summary>
    public List<Diagnostic> Diagnostics { get; init; } = new();
    
    /// <summary>
    /// Location of the call site (for error reporting)
    /// </summary>
    public required SourceLocation CallSite { get; init; }
    
    /// <summary>
    /// Whether inference succeeded (all type parameters resolved)
    /// </summary>
    public bool IsComplete => InferredTypes.Count == ExpectedParameterCount && !Diagnostics.Any(d => d.Severity == Severity.Error);
    
    public required int ExpectedParameterCount { get; init; }
}
```

**Usage**:
- Created by `GenericTypeInferenceVisitor` for each generic call site
- Accumulates type information from function arguments, return type context
- After inference, validates all constraints in `PendingConstraints`
- Reports diagnostics if inference fails or constraints violated

---

### 7. Generic Type Cache (Interning)

```csharp
/// <summary>
/// Singleton cache for generic type instantiations
/// Ensures structural equality for generic types (e.g., List<int> is same everywhere)
/// </summary>
public sealed class GenericTypeCache
{
    private readonly Dictionary<int, TGenericInstance> _cache = new();
    
    /// <summary>
    /// Get or create a generic type instance
    /// Uses structural hashing for fast lookups
    /// </summary>
    public TGenericInstance GetOrCreate(TypeName genericTypeDef, FifthType[] typeArguments)
    {
        int hash = ComputeHash(genericTypeDef, typeArguments);
        
        if (_cache.TryGetValue(hash, out var existing))
        {
            // Verify structural equality (hash collisions possible)
            if (AreEqual(existing, genericTypeDef, typeArguments))
                return existing;
        }
        
        var instance = new TGenericInstance
        {
            GenericTypeDefinition = genericTypeDef,
            TypeArguments = typeArguments,
            CachedHash = hash
        };
        
        _cache[hash] = instance;
        return instance;
    }
    
    private static int ComputeHash(TypeName def, FifthType[] args)
    {
        // Combine hash of definition name with hashes of type arguments
        unchecked
        {
            int hash = def.Name.GetHashCode();
            foreach (var arg in args)
                hash = hash * 31 + arg.GetHashCode();
            return hash;
        }
    }
    
    private static bool AreEqual(TGenericInstance existing, TypeName def, FifthType[] args)
    {
        return existing.GenericTypeDefinition.Equals(def) &&
               existing.TypeArguments.SequenceEqual(args);
    }
    
    /// <summary>
    /// Clear cache (called between compilation units)
    /// </summary>
    public void Clear() => _cache.Clear();
}
```

**Lifecycle**:
- Singleton instance in compiler type system module
- Populated during `GenericTypeInferenceVisitor` pass
- Cleared after each compilation unit (not persisted across builds)
- Accessed by type checker for equality comparisons

---

## Grammar Rule Data Model

### 8. Parser Grammar Additions

**Location**: `src/parser/grammar/FifthParser.g4`

New grammar rules to parse generic syntax:

```antlr
// Type parameter list: <T>, <T, U>, <T: IComparable>
type_parameter_list
    : LESS type_parameter (COMMA type_parameter)* GREATER
    ;

// Single type parameter: T or T with constraints
type_parameter
    : IDENTIFIER
    ;

// Constraint clause: where T: IComparable, IDisposable
constraint_clause
    : WHERE IDENTIFIER COLON constraint_list
    ;

// List of constraints: IComparable, IDisposable, new()
constraint_list
    : constraint (COMMA constraint)*
    ;

// Single constraint: interface, base class, or constructor
constraint
    : type_spec                     // Interface or base class
    | NEW LPAREN RPAREN             // Constructor constraint
    ;
```

**Lexer Token Addition** (`FifthLexer.g4`):
```antlr
WHERE : 'where';
```

**Modified Rules**:
```antlr
// Class with optional type parameters
class_definition
    : CLASS IDENTIFIER type_parameter_list? (COLON type_list)? class_body
    ;

// Function with optional type parameters and constraints
function_definition
    : IDENTIFIER type_parameter_list? LPAREN parameter_list? RPAREN (COLON type_spec)? constraint_clause* function_body
    ;

// Generic type spec: List<int>, Stack<T>
type_spec
    : IDENTIFIER                                    # simple_type_spec
    | IDENTIFIER LESS type_spec GREATER             # generic_type_spec  // EXISTING, remove hardcoded list/array
    | LBRACKET type_spec RBRACKET                   # array_type_spec
    | type_spec LBRACKET RBRACKET                   # alt_array_type_spec
    ;
```

---

## AST Builder Visitor Modifications

### 9. Visitor Method Additions

**Location**: `src/parser/AstBuilderVisitor.cs`

New visitor methods to construct generic AST nodes:

```csharp
/// <summary>
/// Parse type parameter list: <T, U>
/// </summary>
private TypeParameterDef[] VisitType_parameter_list(Type_parameter_listContext ctx)
{
    return ctx.type_parameter()
        .Select(VisitType_parameter)
        .ToArray();
}

/// <summary>
/// Parse single type parameter: T
/// </summary>
private TypeParameterDef VisitType_parameter(Type_parameterContext ctx)
{
    var name = ctx.IDENTIFIER().GetText();
    return new TypeParameterDef
    {
        Name = TypeParameterName.Create(name).Value,
        Constraints = null,  // Constraints parsed separately from constraint_clause
        Location = GetLocation(ctx)
    };
}

/// <summary>
/// Parse constraint clause: where T: IComparable
/// </summary>
private (TypeParameterName Param, TypeConstraint[] Constraints) VisitConstraint_clause(Constraint_clauseContext ctx)
{
    var paramName = TypeParameterName.Create(ctx.IDENTIFIER().GetText()).Value;
    var constraints = ctx.constraint_list().constraint()
        .Select(VisitConstraint)
        .ToArray();
    
    return (paramName, constraints);
}

/// <summary>
/// Parse single constraint: IComparable or new()
/// </summary>
private TypeConstraint VisitConstraint(ConstraintContext ctx)
{
    if (ctx.type_spec() != null)
    {
        var (typeName, _) = ParseTypeSpec(ctx.type_spec());
        
        // Heuristic: if type name starts with 'I', assume interface
        // TODO: Improve with proper type lookup (requires symbol table)
        if (typeName.Name.StartsWith("I"))
        {
            return new InterfaceConstraint
            {
                InterfaceType = typeName,
                Location = GetLocation(ctx)
            };
        }
        else
        {
            return new BaseClassConstraint
            {
                BaseClass = typeName,
                Location = GetLocation(ctx)
            };
        }
    }
    else if (ctx.NEW() != null)
    {
        return new ConstructorConstraint
        {
            Location = GetLocation(ctx)
        };
    }
    
    throw new InvalidOperationException("Unknown constraint type");
}
```

**Modifications to `VisitGeneric_type_spec`**:

```csharp
// BEFORE (hardcoded list/array):
else if (typeSpec is FifthParser.Generic_type_specContext genericType)
{
    var genericName = genericType.IDENTIFIER().GetText();
    var innerTypeSpec = genericType.type_spec();
    var (innerTypeName, _) = ParseTypeSpec(innerTypeSpec);
    
    if (string.Equals(genericName, "list", StringComparison.OrdinalIgnoreCase))
        return (innerTypeName, CollectionType.List);
    else if (string.Equals(genericName, "array", StringComparison.OrdinalIgnoreCase))
        return (innerTypeName, CollectionType.Array);
    else
        return (innerTypeName, CollectionType.SingleInstance);
}

// AFTER (proper generic handling):
else if (typeSpec is FifthParser.Generic_type_specContext genericType)
{
    var genericName = genericType.IDENTIFIER().GetText();
    var innerTypeSpec = genericType.type_spec();
    var (innerTypeName, _) = ParseTypeSpec(innerTypeSpec);
    
    // Create generic type instance AST node
    // Type checking happens later in TypeParameterResolutionVisitor
    return (new TypeName(genericName), CollectionType.GenericInstance);  // NEW CollectionType value
}
```

---

## Constraint Validation Rules

### 10. Constraint Satisfaction Logic

**Location**: `src/compiler/LanguageTransformations/TypeParameterResolutionVisitor.cs`

Constraint validation algorithm:

```csharp
/// <summary>
/// Validate that a type argument satisfies all constraints
/// </summary>
private bool ValidateConstraints(FifthType typeArg, TypeConstraint[] constraints, SourceLocation location)
{
    foreach (var constraint in constraints)
    {
        switch (constraint)
        {
            case InterfaceConstraint ic:
                if (!typeArg.ImplementsInterface(ic.InterfaceType))
                {
                    ReportDiagnostic(Diagnostic.GEN003_ConstraintViolation(
                        typeArg, ic.InterfaceType, location));
                    return false;
                }
                break;
                
            case BaseClassConstraint bc:
                if (!typeArg.DerivesFrom(bc.BaseClass))
                {
                    ReportDiagnostic(Diagnostic.GEN003_ConstraintViolation(
                        typeArg, bc.BaseClass, location));
                    return false;
                }
                break;
                
            case ConstructorConstraint cc:
                if (!typeArg.HasParameterlessConstructor())
                {
                    ReportDiagnostic(Diagnostic.GEN004_MissingConstructor(
                        typeArg, location));
                    return false;
                }
                break;
        }
    }
    
    return true;
}
```

**Constraint Ordering**:
1. Base class constraint (at most one)
2. Interface constraints (multiple allowed)
3. Constructor constraint (must be last)

**Error Codes**:
- `GEN003`: Type argument does not satisfy constraint
- `GEN004`: Type argument missing parameterless constructor

---

## Entity Relationship Diagram

```mermaid
erDiagram
    ClassDef ||--o{ TypeParameterDef : "has type parameters"
    FunctionDef ||--o{ TypeParameterDef : "has type parameters"
    TypeParameterDef ||--o{ TypeConstraint : "has constraints"
    
    TypeConstraint ||--|| InterfaceConstraint : "is-a"
    TypeConstraint ||--|| BaseClassConstraint : "is-a"
    TypeConstraint ||--|| ConstructorConstraint : "is-a"
    
    FifthType ||--|| TGenericParameter : "variant"
    FifthType ||--|| TGenericInstance : "variant"
    
    TGenericParameter ||--|| TypeParameterName : "has name"
    TGenericParameter ||--|| TypeParameterScope : "has scope"
    
    TGenericInstance ||--|| TypeName : "references generic definition"
    TGenericInstance ||--o{ FifthType : "has type arguments"
    
    GenericTypeCache ||--o{ TGenericInstance : "caches instances"
    TypeInferenceContext ||--o{ TypeParameterName : "maps to inferred types"
```

---

## Data Model Validation Rules

### Invariants

1. **Type Parameter Uniqueness**: Within a single `type_parameter_list`, all type parameter names must be unique
2. **Constraint Ordering**: Constructor constraint must be last if present
3. **Type Argument Count**: `TGenericInstance.TypeArguments.Length` must equal number of type parameters in definition
4. **Constraint Satisfaction**: All constraints must be satisfied by corresponding type arguments (checked at instantiation)
5. **Scope Validity**: `TypeParameterScope.DeclaringEntity` must reference a valid `ClassDef` or `FunctionDef`
6. **Hash Consistency**: `TGenericInstance.CachedHash` must match `ComputeHash(GenericTypeDefinition, TypeArguments)`

### Validation Points

- **Parse time**: Grammar validates syntax (e.g., `<>` not allowed, must have at least one type parameter)
- **AST construction**: `AstBuilderVisitor` validates constraint order, type parameter uniqueness
- **Type resolution**: `TypeParameterResolutionVisitor` validates scope, constraint satisfaction
- **Type inference**: `GenericTypeInferenceVisitor` validates type argument count, constraint compatibility

---

## Summary

This data model extends Fifth's AST and type system with:

- **3 new AST node types**: `TypeParameterDef`, `TypeConstraint` hierarchy (3 variants)
- **2 new FifthType variants**: `TGenericParameter`, `TGenericInstance`
- **2 new data structures**: `TypeInferenceContext`, `GenericTypeCache`
- **5 new grammar rules**: `type_parameter_list`, `type_parameter`, `constraint_clause`, `constraint_list`, `constraint`
- **1 new lexer token**: `WHERE`

All changes maintain backward compatibility (type parameters are optional) and follow Fifth's constitution (metamodel-driven, multi-pass compilation).

**Next**: Create Phase 1 `quickstart.md` with Fifth language examples demonstrating generic syntax.
