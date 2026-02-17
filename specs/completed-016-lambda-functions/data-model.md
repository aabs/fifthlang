# Data Model: Lambda Functions

## AST Metamodel Changes

### New Types (`src/ast-model/TypeSystem/FifthType.cs`)

#### `FunctionType`
Represents the type of a function or lambda.
```csharp
// In FifthType union
partial record TFunc(List<FifthType> InputTypes, FifthType OutputType);
```
*Note: Updated from `TFunc(FifthType InputType, FifthType OutputType)` to support multiple inputs.*

### Existing Expressions (modified) (`src/ast-model/AstMetamodel.cs`)

#### `LambdaExp`
Represents an anonymous function definition.
```csharp
public partial record LambdaExp : Expression
{
    public required List<ParameterDef> Parameters { get; init; }
    public required FifthType ReturnType { get; init; }
    public required Block Body { get; init; }
    
    // Populated during Closure Conversion
    public List<VariableRefExp> CapturedVariables { get; set; } = new();
    public ClassDef? GeneratedClosureClass { get; set; }
    public MethodDef? GeneratedApplyMethod { get; set; }
}
```

#### Closure application (no new AST node)
Closure invocation is represented using the existing call/member-access expression forms by lowering Fifth function application syntax `f(x, y)` into an `.Apply(x, y)` call on the closure instance.

### New Definitions (`src/ast-model/AstMetamodel.cs`)

#### `WorkerFunctionDef`
Represents the internal, defunctionalised version of a function.
```csharp
public partial record WorkerFunctionDef : FunctionDef
{
    public required FunctionDef OriginalFunction { get; init; }
    public override bool IsExported => false;
    public bool IsCompilerGenerated => true;
}
```

#### `WrapperFunctionDef`
Represents the public wrapper for an exported function.
```csharp
public partial record WrapperFunctionDef : FunctionDef
{
    public required WorkerFunctionDef Worker { get; init; }
    public override bool IsExported => true;
    public bool IsCompilerGenerated => true;
}
```

### Modified Definitions

#### `FunctionDef`
```csharp
public partial record FunctionDef
{
    // ... existing fields ...
    
    // New fields for lowering
    public bool HasFunctionTypedParameters { get; set; }
    public WorkerFunctionDef? Worker { get; set; }
    public WrapperFunctionDef? Wrapper { get; set; }
}
```

#### `FunctorDef` (Module/Class)
```csharp
public partial record FunctorDef
{
    // ... existing fields ...
    
    // Container for generated closure classes
    public List<ClassDef> GeneratedClosureClasses { get; set; } = new();
}
```

## Runtime Interfaces (`src/fifthlang.system/`)

### `IClosure`
```csharp
public interface IClosure<out R> { R Apply(); }
public interface IClosure<in T1, out R> { R Apply(T1 t1); }
public interface IClosure<in T1, in T2, out R> { R Apply(T1 t1, T2 t2); }
// ... up to arity 8
```

### `IAction`
```csharp
public interface IAction { void Apply(); }
public interface IAction<in T1> { void Apply(T1 t1); }
public interface IAction<in T1, in T2> { void Apply(T1 t1, T2 t2); }
// ... up to arity 8
```
