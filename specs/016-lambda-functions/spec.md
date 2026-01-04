# Feature Specification: Lambda Functions & Higher-Order Functions

**Feature Branch**: `016-lambda-functions`
**Created**: 2026-01-04
**Status**: Draft
**Input**: User description: "implement functional programming support"

## Summary

Implement support for lambda functions as first-class citizens: anonymous function expressions (LF), assignment to variables and members, passing as parameters, returning as results, generic LFs, closure capture, defunctionalisation of HOFs, worker/wrapper generation, and TCO support for self-recursion.

**Elevator pitch**: Implementation of lambda functions opens a world of functional programming capabilities, obviating the need for other control constructs.

## Terminology

- **Bridge thunk**: The function created to retain the type signature of a HOF that delegates all functionality through to the **defunctionalised function** by instantiating whatever closure instances are required.
- **Call site**: The point of declaration and instantiation of a lambda function.
- **Closure**: A class created as a non-functional substitute for a lambda function capturing the functionality of the lambda function as well as the value of the free variables it referenced at the point of creation in the call site.
- **Defunctionalised Function**: A higher-order function that has been transformed into a regular function accepting closures rather than lambda functions.
- **Free Variable**: A variable used within a function's code block that is not one of its explicitly declared parameters or locally defined variables.
- **Function Signature**: A representation of the logical behaviour of a function in terms of the parameter types it accepts and the result type it returns.
- **Lambda function (LF)**: A function definition not appearing in a symbol table (anonymous), but in the form of an expression that can be passed around (first-class) that acts as a closure over the free variables used within its function body.

## Motivation

- **Problem statement**: Higher-order functions are a missing piece of the language and currently force many one-off control constructs and library functions.
- **Value proposition**: Provide a simple and consistent set of functional programming idioms to ease data manipulation and expressive power.
- **Success metrics**: full support for strongly-typed LFs (0+ args, generics), assignment to variables/members, passing/returning LFs, and implementing standard functional idioms (map, fold, zip, etc.).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define and Use Lambda Functions (Priority: P1)

As a functional programmer, I want to be able to define Lambda Functions (LFs) and assign them to variables so that I can use them in multiple places within my code.

**Why this priority**: This is the foundational capability. Without LFs, HOFs cannot be effectively used.

**Independent Test**: Can be tested by defining an LF, assigning it to a variable, and invoking it.

**Acceptance Scenarios**:

1. **Given** a variable `f` of function type `[int] -> int`, **When** I use function application syntax `f(2)`, **Then** the compiler emits a call to the `Apply` function on the Closure instance in `f`.
2. **Given** a declaration statement `x: [] -> int`, **When** `x` is compiled, **Then** `x` will be transformed to be of type `IClosure<int>`.
3. **Given** a declaration statement `x: [int] -> long`, **When** `x` is compiled, **Then** `x` will be transformed to be of type `IClosure<int, long>`.
4. **Given** a variable `x` of function type `[] -> int`, **When** any Closure implementing `IClosure<int>` is assigned to `x`, **Then** assignment will be successful.

---

### User Story 2 - Higher-Order Functions (Priority: P1)

As a functional programmer, I want to be able to define Higher-Order Functions (HOFs) that accept LFs as parameters and return them as results.

**Why this priority**: Enables the core value proposition of functional programming (map, filter, reduce).

**Independent Test**: Can be tested by defining a function that takes a function argument and calling it with a lambda.

**Acceptance Scenarios**:

1. **Given** a HOF `map` taking a list and a function, **When** I call `map(xs, fun(x){...})`, **Then** the lambda is executed for elements in the list.
2. **Given** a HOF that returns a function, **When** I call it, **Then** I receive a callable LF.

---

### User Story 3 - Closures and Variable Capture (Priority: P2)

As a functional programmer, I want to be able to capture variables referenced in a LF in a Closure so that my LF is able to reference the variables in places where they would not be visible.

**Why this priority**: Essential for practical use of LFs where context is needed.

**Independent Test**: Define an LF that uses a local variable from the enclosing scope, pass the LF out of scope, and verify it still accesses the captured value.

**Acceptance Scenarios**:

1. **Given** a local variable `factor` and an LF capturing it, **When** the LF is invoked, **Then** it uses the value of `factor` captured at the point of creation.
2. **Given** a mutable variable captured by an LF, **When** the original variable changes after capture, **Then** the LF uses the *copied* value (capture-by-value semantics).

---

### User Story 4 - Generics and TCO (Priority: P3)

As a functional programmer, I want to define Generic LFs and use recursive functions supporting Tail Call Optimisation (TCO) so that I can write efficient, reusable functional code without stack overflows.

**Why this priority**: Critical for performance and robustness of recursive algorithms common in FP.

**Independent Test**: Run a deep recursive function (e.g., 100k iterations) and verify no StackOverflowException.

**Acceptance Scenarios**:

1. **Given** a self-recursive function, **When** back-end code is generated, **Then** the code will be optimized into a loop to prevent stack overflow.
2. **Given** a generic LF definition, **When** used with different types, **Then** it works correctly for each type.

### Edge Cases

- **Too many parameters**: If an LF is defined with more parameters than the configured limit (default 8), a compile-time error `ERR_TOO_MANY_LF_PARAMETERS` should be issued.
- **Unresolved free variable**: If an LF references a variable not in scope, `ERR_UNRESOLVED_FREE_VARIABLE` should be issued.
- **Type mismatch**: Assigning a closure of incompatible type to a function-typed variable should result in `ERR_LF_TYPE_MISMATCH`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST support definition of anonymous lambda functions using the `fun` keyword.
- **FR-002**: The system MUST support first-class function types (e.g., `[int] -> string`) in variable declarations, parameters, and return types.
- **FR-003**: The system MUST support capturing of free variables from the enclosing scope into the lambda closure (capture-by-value).
- **FR-004**: The system MUST support passing lambda functions as arguments to other functions (Higher-Order Functions).
- **FR-005**: The system MUST support returning lambda functions from functions.
- **FR-006**: The system MUST support generic lambda functions and HOFs.
- **FR-007**: The system MUST implement Tail Call Optimisation (TCO) for self-recursive functions (transform to loop).
- **FR-008**: The system MUST enforce a limit on the number of lambda parameters (default 8).

### Non-Goals

- **NG1**: No support for SSA (Static Single Assignment) form.
- **NG2**: No support for referential transparency (pure functions enforcement).
- **NG3**: No intrinsic support for partial application/currying.
- **NG4**: No support for extension methods or method call chaining.
- **NG5**: No support for creating LFs from existing function or method names (Method Groups).

### Key Entities

- **Lambda Function (LF)**: An anonymous function expression.
- **Closure**: A runtime object wrapping the function logic and captured state.
- **Higher-Order Function (HOF)**: A function that operates on other functions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can define and use `map`, `fold`, and `zip` functions using the new syntax.
- **SC-002**: Recursive functions can execute 100,000+ iterations without stack overflow (validating TCO).
- **SC-003**: Compiler successfully transforms all valid LF scenarios (assignment, passing, returning) into valid target code.
- **SC-004**: 100% test coverage for all implemented language features.

---

## Clarifications

### Session 2026-01-04

- Q: Generic Type Constraints on Lambda Functions → A: Yes, add constraint syntax (use the 'constraint_clause' grammar rule for LFs as is done with regular generic functions).
- Q: Closure Implementation Type → A: Use standard `IClosure` interfaces.
- Q: TCO Scope → A: Self-recursion only (optimize to loop), no full trampoline.
- Q: Type Inference for Lambda Parameters → A: No, keep explicit types.
- Q: Function Type Syntax → A: Use square brackets `[T1, T2] -> R`.
- Q: Recursive Lambda Syntax → A: By variable name only (capture variable).
- Q: Closure Equality → A: Reference Equality.
- Q: Void Return Handling → A: Use `void` keyword mapping to `TVoidType`.

## Technical Specification & Implementation Details

### Syntax (ANTLR4)

```antlr
lambda_function: 'fun' type_parameter_list? 
    L_PAREN (plain_paramdecl (COMMA plain_paramdecl)* )? R_PAREN 
    COLON type_name 
    constraint_clause?
    function_body
    ;

function_body: block;
function_name: IDENTIFIER;
type_parameter_list: LESS type_parameter (COMMA type_parameter)* GREATER;
type_parameter: IDENTIFIER;
plain_paramdecl: var_name COLON type_name;

function_type_signature: '[' input_types+=type_name (',' input_types+=type_name)* ']' '->' output_type = type_name;
```

### Conceptual Model

- `[HOF] -> ([Wrapper Function], [Worker Function])`
- `[LF] -> ([Closure Class], [Call-site Expression])`

LFs require lowering transformations to convert them into simpler Fifth AST nodes. The lowering involves the creation of a Closure class with:
1. A field for each of the captured variables referenced in the body of the LF.
2. A constructor taking a value for each captured variable to assign to the fields.
3. An `Apply` function based on the body of the LF but with the closure's fields substituted for the free-variable references.

Each HOF is transformed into a simpler (non-higher-order) form accepting and returning instances of the Closure class instead.

### Closure Type Interface

The Closure type interfaces are a set of internal interfaces used to represent the type signature of a LF.

```csharp
public interface IClosure<T>
{
    T Apply();
}

public interface IClosure<T1, T2>
{
    T2 Apply(T1 t);
}
// ... and so on for higher arities

// For void-returning functions (mapped from TVoidType)
public interface IActionClosure
{
    void Apply();
}
public interface IActionClosure<T>
{
    void Apply(T t);
}
```

### AST Meta-Model Changes

#### New AST Nodes

- **FunctionType**: Represents `[params] -> return`.
    - *Note*: `FifthType.TFunc` should be updated to support multiple input types or `FunctionType` should map to it appropriately.
- **ClosureApplyExp**: Represents `closure.Apply(args)`.
    ```csharp
    public record ClosureApplyExp : Expression
    {
        public required Expression ClosureInstance { get; init; }
        public required List<Expression> Arguments { get; init; }
    }
    ```
- **WorkerFunctionDef**: Internal defunctionalised function taking `IClosure<>`.
    ```csharp
    public record WorkerFunctionDef : FunctionDef
    {
        public required FunctionDef OriginalFunction { get; init; }
        public bool IsCompilerGenerated => true;
        public override bool IsExported => false;
    }
    ```
- **WrapperFunctionDef**: Public wrapper for exported functions.
    ```csharp
    public record WrapperFunctionDef : FunctionDef
    {
        public required WorkerFunctionDef Worker { get; init; }
        public override bool IsExported => true;
        public bool IsCompilerGenerated => true;
    }
    ```

#### Modifications

- **LambdaExp**:
    ```csharp
    public partial record LambdaExp : Expression
    {
        public List<VariableRefExp> CapturedVariables { get; set; } = new();
        public ClassDef? GeneratedClosureClass { get; set; }
        public MethodDef? GeneratedApplyMethod { get; set; }
    }
    ```
- **FunctionDef**:
    ```csharp
    public partial record FunctionDef
    {
        public bool HasFunctionTypedParameters { get; set; }
        public WorkerFunctionDef? Worker { get; set; }
        public WrapperFunctionDef? Wrapper { get; set; }
    }
    ```
- **FunctorDef**:
    ```csharp
    public partial record FunctorDef
    {
        public List<ClassDef> GeneratedClosureClasses { get; set; } = new();
    }
    ```

### Lowering Pipeline

The pipeline is: `Parse Tree → AST → Closure Conversion → Defunctionalisation → Optional TCO → Roslyn backend`.

#### 1. Closure Conversion
- **Input**: `LambdaExp` (e.g., `fun(x) { x * factor }`)
- **Action**:
    - Compute captured variables.
    - Generate closure class with `Apply` method.
    - Replace `LambdaExp` with `NewClosureInstanceExp`.
    - Replace function calls `f(x)` with `ClosureApplyExp`.
- **Output**: `NewClosureInstanceExp` instantiating the generated closure class.

#### 2. Defunctionalisation
- **Input**: Function definitions with function-typed parameters.
- **Action**:
    - Generate `WorkerFunctionDef` replacing function types with `IClosure<>`.
    - Rewrite internal calls to use the worker.
- **Output**: `WorkerFunctionDef` (internal).

#### 3. AST -> Roslyn Backend
- **Input**: Exported functions with function-typed parameters.
- **Action**:
    - Generate `WrapperFunctionDef` (public API) using `Func<>`.
    - Emit closure classes and workers.
- **Output**: C# code with `Func<>` wrappers calling `IClosure<>` workers.

### Diagnostics

- `ERR_UNRESOLVED_FREE_VARIABLE`: Unresolved free variable.
- `ERR_TOO_MANY_LF_PARAMETERS`: Exceeds arity limit.
- `ERR_LF_TYPE_MISMATCH`: Incompatible closure type.

### Assumptions

- Default closure arity limit is 8.
- Capture-by-value semantics.
- Roslyn backend maps to .NET `Func<>`/`Action<>` only for public wrappers.
