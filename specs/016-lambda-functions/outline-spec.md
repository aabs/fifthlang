## Summary

Implement support for lambda functions as first class citizens.  Specifically definition of anonymous functions as syntactic expressions, assigning them to variables, passing them as parameters and returning them as results.

**Elevator pitch**: Implementation of lambda functions opens a world of functional programming capabilities, obviating the need for other control constructs.

## Motivation

- **Problem statement**: Higher-order functions are a missing piece of the language, conferring another layer of expressive power that would otherwise have to be filled with numerous control constructs and one-off library functions.
- **Value proposition**: A set of simple and consistent functional programming idioms will make learning how to work with data much easier.
- **Success metrics**: The language should fully support the following:
    - definition of strongly typed lambda functions (LF)
        - definition of LFs with zero or more arguments 
        - definition of generic LFs accepting one or more type arguments
    - assignment of LFs to variables and members
    - returning LFs as results from functions
    - accepting LFs as parameters
    - Ability to define the full complement of functional programming language staples (map, fold, zip etc)

### Motivating Example Code

Throughout this specification, the following code sample will be used to ground the discussion:
```fifth
// 1. The HOF 'map' is a generic function taking a list and an LF ( called 'f')
map<T1,T2>(ts: [T1], f: [T1] -> T2): [T2]
{
    if(len(ts) == 0)
        return [];
    // 2. a direct recursive tail-call consumes the list elements without need for iteration syntax 
    return [f(head(ts))] + map(tail(ts), f); 
}

// 3. the main entry point that uses the 'map' HOF
main(): int
{
  // 4. define the input list
  xs: [int] = [1,2,3,4,5,6];
  
  // 5. define a variable in the outer scope called 'factor'
  factor: int = 2;
  
  // 6. create an LF variable capturing factor when it is 2
  doubler: [int] -> int = fun(x: int): int{return x * factor;};
  
  // 7. update the factor variable (prior to the use of LF 'doubler')
  factor = 3;

  // 8. now use the map HOF using the doubler LF
  x2s: [int] = map(xs, fun(x: int): int{return x * factor;});
  
  // 9. use the HOF map using another LF that captures the factor when it is 3
  x3s: [int] = map(xs, fun(x: int): int{return x * factor;});
  
  // 10. return the length of the result list
  return len(x2s);
}
```

## Terminology
Bridge thunk
: The function created to retain the type signature of a HOF that delegates all functionality through to the **defunctionalised function** by instantiating whatever closure instances are required. The primary purpose is to provide an exportable function signature that can be used by third parties that don't wish to recast their calls to HOFs into defunctionalised form. It allows defunctionalisation of HOFs while retaining the public higher-order type signature for users. Bridge functions mean the call-site need not be transformed, since the transition from functional to defunctionalised form happens in one place — the bridge.
Call site
: The point of declaration and instantiation of a lambda function. 
Closure
: A class created as a non-functional substitute for a lambda function capturing the functionality of the lambda function as well as the value of the free variables it referenced at the point of creation in the call site.
Declaration
: The assignment of some runtime element to a type.
Definition
: The specification of a function or composite type, describing its parts
Defunctionalised Function
: A higher-order function that has been transformed into a regular function accepting closures rather than lambda functions.
Function (AKA 'Regular Function')
: A regular function definition is bound to a name (in the symbol table), not treated as an expression, and subject to normal lexical scoping rules without closures.
Free Variable
: A variable used within a function's code block that is not one of its explicitly declared parameters or locally defined variables.
Function Signature
: A representation of the logical behaviour of a function in terms of the parameter types it accepts and the result type it returns. 
Generic Function
: A function accepting type parameters for use in its signature definition.
Generic Type Constraint
: A constraint on a generic function placing restrictions on the types it may accept as parameters.  
Higher-order function (HOF)
: Any kind of function that accepts or returns lambda functions
Lambda function (LF, also 'lambda expression')
: A function definition not appearing in a symbol table (anonymous), but in the form of an expression that can be passed around (first-class) that acts as a closure over the free variables used within its function body.
Type Constraint
: A constraint on a regular function using interface parameter types to restrict the function signatures of the actual parameters' types.

## Conceptual Model
`[HOF] -> ([Bridge Thunk], [Defunctionalised Function])`
`[LF] -> ([Closure Class], [Call-site Expression])`

LFs require lowering transformations to convert them into simpler Fifth AST nodes. The lowering involves the creation of a Closure class with 1) a field for each of the captured variables referenced in the body of the LF, 2) a ctor taking a value for each captured variable to assign to the fields and 3) An `Apply` function based on the body of the LF but with the closure's fields substituted for the free-variable references.  

**Note**: This implementation reuses existing AST nodes for object instantiation (`ObjectInitializerExp` or equivalent) and method invocation (`MemberAccessExp` / `FuncCallExp`). No new AST node types like `NewClosureInstanceExp` or `ClosureApplyExp` are required; the feature is fully realized by lowering to standard language constructs.

Each HOF is transformed into a simpler (non-higher-order) form accepting and returning instances of the Closure class instead.  At the call-site expression, a new instance of the closure class is created (with copies of the current values of each of the captured variables), and passed as an argument to the altered HOF.
### Constraints and invariants at a conceptual level.

As is common for closure/runtime interfaces, `IClosure` will be defined for various different arities. The language will only support LFs up to the limit of the interface definitions. For example, if the implementation defines `IClosure` interfaces up to arity 8, then declaring an LF with more than 8 parameters will produce a compile-time diagnostic (error). State the configured arity limit (default: 8) and provide error code/message.

## Requirements
### Goals & Non‑Goals

#### Goals

- G1: Add full support for functional programming idioms to the language
- G2: new syntax for LF definition
- G3: support for type arguments to generic LFs
- G4: new expression type for LF
- G5: support for Higher-Order Functions (HOFs) accepting and/or returning functions.
- G6: Support for tail call optimisation (TCO) through the use of trampolines.  Applicable to all functions, not just HOFs.
#### Non‑Goals

- NG1: No support for SSA
- NG2: No support for referential transparency
- NG3: No intrinsic support for partial application/currying
- NG4: No support for extension methods or method call chaining
- NG5: No support for creating LFs from existing function or method names (e.g. like Method Groups in C#)
 
### User Stories

- As a functional programmer
  I want to be able to define a Higher-Order Function (HOF)
  So that I can supply LFs as parameters and return them as results
- As a functional programmer, 
  I want to be able to define LFs, 
  So that I can pass them to higher order functions.
- As a functional programmer, 
  I want to be able to define Generic LFs, 
  So that I can create one LF definition and use it for multiple similar types.
- As a Functional Programmer
  I want to be able to assign a LF to a variable
  So that I can use it in multiple places within my code
- As a Functional Programmer
  I want to be able to treat a LF as an Expression Type
  So that I can pass it as a parameter to other functions
- As a Functional Programmer
  I want to be able to treat a LF as an Expression Type
  So that I can return it from functions 
- As a Functional Programmer
  I want to be able to capture variables referenced in a LF in a Closure
  So that my LF is able to reference the variables in places where they would not be visible
- As a functional programmer
  I want to be able to define recursive functions supporting TCO
  So that I avoid stack overflow errors.
### Scenarios
- Given a variable `f` of function type `[int] -> int`
  When I use function application syntax `f(2)`
  Then the compiler emits a call to the `Apply` function on the Closure instance in `f`.
- Given a declaration statement `x: [] -> int`
  When `x` is compiled
  Then `x` will be transformed to be of type `IClosure<int>`
- Given a declaration statement `x: [int] -> long`
  When `x` is compiled
  Then `x` will be transformed to be of type `IClosure<int, long>`
- Given a variable `x` of function type `[] -> int`
  When any Closure implementing `IClosure<int>` is assigned to `x`
  Then assignment will be successful.
- Given a recursive function definition `map<T1, T2>(ts: [T1], f: T1->T2):[T2]{return f(head(ts)) + map(tail(ts), f);}` 
    where the recursive call is the last instruction
  When back-end code is generated for the function
  Then the code generated will incorporate a Trampoline pattern to allow TCO

### Acceptance Criteria

- Feature behaves as specified under all valid scenarios.
- Diagnostics are clear and actionable.
- No regressions in conceptual language model.
- Recursive tail calls will not experience stack overflow for iterations exceeding the size of the permissible stack frames of the CLR. 

## Formal Specification Elements

### Syntax form

The Parser rules (using ANTLR4) are described below.

```antlr
lambda_function: 'fun' type_parameter_list? 
    L_PAREN (plain_paramdecl (COMMA plain_paramdecl)* )? R_PAREN 
    COLON type_name 
    function_body
    ;

function_body: block;
function_name: IDENTIFIER;
type_parameter_list: LESS type_parameter (COMMA type_parameter)* GREATER;
type_parameter: IDENTIFIER;
plain_paramdecl: var_name COLON type_name;

function_type_signature: '[' input_types+=type_name (',' input_types+=type_name)* ']' '->' output_type = type_name;
```

### Sample Code
What follows is a sample of a recursive implementation of a generic version of the standard `map` higher-order function for transforming each element in a list.

```fifth
// recursively transform each element in the list using the mapper function
map<TIn, TOut>(xs: [TIn], mapper: [TIn] -> TOut): [TOut]
{ 
    return mapper(head(xs)) + map(tail(xs), mapper); 
}

// create a list of integers
xs: [int] = [1,2,3,4,5];

// apply the map function to the list using a 'squaring' lambda function
ys: [int] = map(xs, fun(x: int):int{return x * x;});
```

### Semantic meaning

The purpose of the LF syntax is to allow the passing of functions as expressions, allowing a clean coding style and great expressive power, using functional programming principles.

The purpose of the syntactic structure is twofold:

1. Capturing the values of all free variables referenced in the LF at the point of the call-site (at run time).
2. Applying the body of the LF (as implemented in the Closure object) to the combination of the input variables of the LF and the captured values of the free variable references.

The lowering procedure is described above in the conceptual model, showing how the functional representation is converted into a regular function receiving structures by value.
### Constraints
Validity conditions (e.g., type compatibility, scoping rules).

Lambda Functions are a kind of expression.  They can be used wherever an expression is a valid option (e.g. as parameters to a function or the result of function) .  The composition of compound expressions using LFs (such as in forming a BinaryExpression by adding two LFs using the `+` operator) will only be valid if there are existing operators to support it.  Usual rules of type inference will apply, as with all other expression types. 

The scoping of a LF is the same as for any expression:  It is lexically scoped to the statement and block where it is declared.  Passing the expression value out of the enclosing statement or lexical scope depends on assignment to variables, or returning of the expression value as a result.  The values of captured free variables are copied at the point of closure creation (capture-by-value). For mutable captured variables this means subsequent writes to the original variable do not affect the captured copy. If capture-by-reference semantics are desired, they must be expressed explicitly and documented.


### Type System

HOFs require the representation of functions using a new syntax for function types.

The Parser rule is as follows:

```antlr
function_type_signature: '[' input_types+=type_name (',' input_types+=type_name)* ']' '->' output_type = type_name;
```

Here is an example of a function taking an `int` and returning a string:

```
[int] -> string
```

Here is an example of a function taking an `int` and a `datetime` and returning a `datetime`:

```
[int, datetime] -> datetime
```

which might look like this in an implemented fifth lambda function:

```fifth
fun(i: int, dt: datetime): datetime {. . .}
```

**IMPORTANT**: Fifth source code uses language-level function-type syntax (for example `[T1] -> T2`). The Roslyn backend may map exported function types to .NET `Func<>`/`Action<>` as needed when emitting C#.

#### Closure Type Interface

The Closure type interfaces are a set of internal interfaces used to represent the type signature of a LF, and how it might be invoked.

There will be various interfaces based on the arities of the LFs in the system. The arity-0 version of the interface is as follows:

```csharp
public interface IClosure<T>
{
    T Apply();
}
```

No mention of the environment or captured variables is necessary, since the capturing and supply of free vars to the body of the closure object is an internal detail of the closure and will not be visible to the outside world.

The arity-1 interface is largely the same:
```csharp
public interface IClosure<T1, T2>
{
    T2 Apply(T1 t);
}
```

The pattern repeats for each successive arity increment.

### New additions to the AST meta-model
---
Here’s a clean, spec‑ready description of **exactly what AST nodes you need**, **how they behave**, **who is allowed to know about them**, and **what modifications are required to existing AST nodes** to support closure conversion + defunctionalisation + worker/wrapper generation in Fifth.

I’ll keep this tightly aligned with your existing AST metamodel and the Fifth philosophy:  
**all semantic lowering happens in the AST; the Roslyn backend only emits C#.**

#### **AST Nodes Required for Worker and Wrapper Functions**

Fifth currently has only one kind of function definition (`FunctionDef`).  
To support defunctionalisation and exportable higher‑order functions, you need two _new_ AST node types:

- `WorkerFunctionDef`
- `WrapperFunctionDef`

These are _specialisations_ of `FunctionDef`, not replacements.


#### **WorkerFunctionDef**

###### **Purpose**

Represents the _internal_, defunctionalised version of a function that originally accepted function‑typed parameters.

This is the function that:

- takes `IClosure<T1,T2>` instead of `T1 -> T2`
- is called by all internal Fifth code
- is the target of recursion
- is the only version that participates in closure conversion, TCO, and defunctionalisation

###### **Who is allowed to know about it**

- **The AST lowering pipeline** (closure conversion, defunctionalisation, TCO)
- **Other worker functions**
- **Wrapper functions**

###### **Who must NOT know about it**

- User code
- The public API
- The Roslyn backend (except to emit it as an internal C# method)

###### **AST shape**

```csharp
public record WorkerFunctionDef : FunctionDef
{
    // The original user-defined function this worker belongs to
    public required FunctionDef OriginalFunction { get; init; }

    // Indicates this is compiler-generated
    public bool IsCompilerGenerated => true;

    // Indicates this function must not be exported
    public override bool IsExported => false;
}
```

###### **Lowering rules**

- Created during **defunctionalisation**
- Replaces all internal calls to the original function
- All parameters of function type are rewritten to `IClosure<…>`
- Body is closure‑converted and tail‑call analysed

#### **WrapperFunctionDef**

###### **Purpose**

Represents the _public_, user‑visible version of a function that originally accepted function‑typed parameters.

This is the function that:

- keeps the original signature (with `Func<T1,T2>` in the backend)
- constructs closure adapters
- calls the worker function
- is exported in the final assembly

###### **Who is allowed to know about it**

- The Roslyn backend (to emit public C#)
- The module export system

###### **Who must NOT know about it**

- Internal Fifth code
- Closure conversion
- TCO
- Defunctionalisation

###### **AST shape**

```csharp
public record WrapperFunctionDef : FunctionDef
{
    // The worker this wrapper delegates to
    public required WorkerFunctionDef Worker { get; init; }

    // This is exported if the original function was exported
    public override bool IsExported => true;

    // Compiler-generated marker
    public bool IsCompilerGenerated => true;
}
```

###### **Lowering rules**

- Created during **AST → Roslyn backend** phase
- Parameters of function type remain as function types in AST
- Backend maps them to `Func<T1,T2>`
- Body is a simple call to the worker with closure adapters

#### **Modifications Needed to Existing AST Nodes**

To support closure conversion and defunctionalisation, you need small but important additions to:

- `LambdaExp`
- `FunctionDef` (user functions)
- `FunctorDef` (if functors can contain lambdas or functions)

#### **Modifications to `LambdaExp`**

###### **Why**

A lambda must carry enough information for closure conversion to generate:

- a closure class
- captured variables
- an `Apply` method

###### **Additions**

```csharp
public partial record LambdaExp : Expression
{
    // Populated during closure conversion
    public List<VariableRefExp> CapturedVariables { get; set; } = new();

    // The generated closure class (added during closure conversion)
    public ClassDef? GeneratedClosureClass { get; set; }

    // The generated Apply method (added during closure conversion)
    public MethodDef? GeneratedApplyMethod { get; set; }
}
```

###### **Lowering rules**

- Closure conversion computes `CapturedVariables`
- Generates a closure class and stores it in `GeneratedClosureClass`
- Replaces lambda expressions with `NewClosureInstanceExp`

#### **Modifications to `FunctionDef` (user functions)**

###### **Why**

User functions must:

- know whether they require a worker function
- know whether they require a wrapper
- know their function‑typed parameters

###### **Additions**

```csharp
public partial record FunctionDef
{
    // True if any parameter has a function type (TFunc)
    public bool HasFunctionTypedParameters { get; set; }

    // The worker function generated for this function
    public WorkerFunctionDef? Worker { get; set; }

    // The wrapper function generated for this function (if exported)
    public WrapperFunctionDef? Wrapper { get; set; }
}
```

###### **Lowering rules**

- During defunctionalisation:
    - If `HasFunctionTypedParameters == true`, generate a worker
- During backend emission:
    - If function is exported and has function‑typed parameters, generate a wrapper

#### **Modifications to `FunctorDef`**

Only needed if functors can contain:

- lambdas
- functions
- higher‑order functions

###### **Additions**

```csharp
public partial record FunctorDef
{
    // List of closure classes generated inside this functor
    public List<ClassDef> GeneratedClosureClasses { get; set; } = new();
}
```

###### **Lowering rules**

- Closure conversion inserts closure classes into the functor’s scope

#### **New AST Node: ClosureApplyExp**

###### **Why**

The AST currently has no way to represent:

```
closure.Apply(x)
```

###### **Definition**

```csharp
public record ClosureApplyExp : Expression
{
    public required Expression ClosureInstance { get; init; }
    public required List<Expression> Arguments { get; init; }
}
```

###### **Lowering rules**

- Replaces `FuncCallExp` when the callee is a lambda or function value

#### **Putting It All Together**

###### **User writes**

```
map(xs, fun(x) { x * factor })
```

###### **AST after closure conversion**

- `LambdaExp` becomes `New Closure$lambda$1(factor)`
- Calls to `f(x)` become `ClosureApplyExp`

###### **AST after defunctionalisation**

- `map` becomes `map$worker`
- Parameter `f: TFunc` becomes `fClosure: IClosure`

###### **AST before backend**

- WrapperFunctionDef is added for exported functions

###### **Roslyn backend**

- Emits:
    - closure classes
    - worker functions
    - wrapper functions
    - public API using `Func<>`

## Diagnostics & Error Conditions

1. Unresolved free-variable. Severity: Error.
2. Too many parameters in LF. Severity: Error.

## Interactions with Existing Features

How this feature coexists with existing language constructs:
### Type Definitions
classes may define properties of LF type.  They will NOT use the `Func<>` type of .NET to do so.  Instead they will use the new grammar rules defined above in the Type System description.
### Variable Assignments
Variables may be defined of function type `[type, type] -> type` and will be transformed to use `IClosure<type, type, type>` type instead.
### Variable References
variable references can refer to LFs. After transformation they will be of some `IClosure<>` type.
### LF application
application at a call-site will be transformed into the invocation of a closure's `Apply` method; during lowering the callee will be represented by a `ClosureApplyExp` (or an appropriately-typed call AST), replacing a generic `FuncCallExp` when the callee is a function value.
### Generic Functions
LFs can be generic building on the existing generics syntax: `fun<T>(ts: [T]):int{...}`
### NULL value assignment/default value assignment
### Expression Statements (& fn application)
expression statements may contain lambda expressions, though they will have no effect, and could probably be elided or converted to a noop by some optimisation stage.
### Any precedence or associativity rules
The lambda expression AST node is right-associative. Define a numeric precedence table in the grammar; for example (higher number = tighter binding): atoms/literals = 4, application = 3, lambda = 2, binary operators = 1.
### Composition Operator
composition operators for LFs (eg pipes) are out of scope, and will be handled in a future release.
### Compatibility Considerations
none anticipated.

## Backward Compatibility & Migration

- Impact on existing programs - none anticipated.
- Migration strategy - none required.
- Deprecation considerations - none required.

## Performance & Complexity Constraints

- Expected complexity characteristics (e.g., should not introduce exponential parsing)
    - Extra stages will be required for handling lowering of HFs, HOFs and call-sites, but the extra work should be a linear function of the number of each of these things in the source code.
- Resource usage expectations - no extra resources required.

## Internationalization & Accessibility
- Identifiers will use the existing token rules in the FifthLexer.g4 grammar
- Diagnostic messages will follow existing conventions, and be in English only.

## Testability Requirements
- **MANDATORY**: These features should be tested by both compiling and then running test code. It is NOT enough to just test that the code compiles.
- Test cases to explore:
    - declaration of lambda function variables
    - assignment of lambda function to variable
    - passing lambda function as argument to function
    - assigning variable of type lambda function to function invocation
    - returning LF from HOF
    - returning variable of type LF from HOF
    - passing LF to a LF
    - returning LF from a LF
    - capturing free variables in a LF.
        - Verify value copied, and not passed by reference.
- Define what must be verifiable (e.g., semantic correctness, error detection).
- Coverage expectations (positive, negative, edge cases).
    - All usage scenarios (as stated above) must be tested, along with negative tests that use incorrect syntax, unknown free variables, etc.
    - Test coverage of new code must be 100%.

## Prior Art & Alternatives

- [[meta/incoming-material/Survey of Lambda Function Syntaxes|Comparable features in other languages or systems]]
- Some of the syntactic alternatives considered and why they were rejected.
    - C# Syntax `(int a) => a * 2`
    - Haskell Syntax: `\x -> x * 2`
    - Erlang Syntax: `fun(x) -> x * 2 end`
    - Elixir Syntax: `fn x -> x * 2 end`
    - Javascript Syntax: `function(x) { return x * 2; }`
    Rejected because I wanted the LF syntax as close to the syntax of regular functions as possible, and because I want to use the arrows as operators relating to application of queries etc. Forms with `end` require introduction of a new keyword needlessly. The Haskell form is too bare and too far from the rest of the Fifth syntax for a clean look.


## Open Questions

- OQ1: What should be used for type signatures for LFs? 
  Answer: `Func<>` is a backend-only representation. In Fifth source code use the language's function-type syntax (for example, `[T1] -> T2`); the Roslyn backend maps exported public wrappers to `Func<>` where appropriate.
- OQ2: what is exported when a library contains HOFS? The consumable functions that external apps can use will be the defunctionalised forms, but those will not be publicly visible? How can people take references to the HOF form of a function, but get the defunctionalised form at compile time. 
  [Discussion](https://www.reddit.com/r/Compilers/comments/1py01jv/exporting_types_that_are_transformed_during/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button)
  [[me/projects/fifthlang/Language Design/Copilot on Exporting HOFs|Copilot's Answer]]
    - the answer for this is a bridge function transformation that needs to be incorporated into this spec. 

---

## **Fifth Lowering Pipeline (AST‑Centric, Roslyn‑Backend‑Aligned Specification)**

This section defines the complete lowering process for functions, lambdas, closures, and higher‑order functions in Fifth. All transformations occur **within the AST**, and the **Roslyn backend** is responsible only for converting the final lowered AST into a .NET assembly.

The pipeline is:

```
Parse Tree → AST → Closure Conversion → Defunctionalisation → Optional TCO → Roslyn backend
```

Each transformation is described in terms of:

- **Input:** AST construct  
- **Output:** Lowered AST construct  
- **Phase:** Which AST‑level transformation performs the rewrite  
- **Backend:** What the Roslyn backend generates from the lowered AST  
- **Example:** C#‑like code illustrating the lowered form  

### **Function Declarations**

##### Input (AST)
```
fun f(a: A, b: B): C { body }
```

##### Output (AST after closure conversion & defunctionalisation)
Two AST nodes are generated:

- **Worker function** (internal, defunctionalised)  
- **Wrapper function** (only if exported and function has function‑typed parameters)

##### Phase
- Worker generated during **defunctionalisation**  
- Wrapper generated during **AST → Roslyn backend** translation  

##### Backend Output (C#)
```csharp
// Wrapper (public API)
public static C f(A a, B b, Func<X,Y> fn) {
    var closure = new DelegateClosure<X,Y>(fn);
    return f$worker(a, b, closure);
}

// Worker (internal)
internal static C f$worker(A a, B b, IClosure<X,Y> fnClosure) {
    ...
}
```

### **Lambda Expressions**

##### Input (AST)
```
fun(x: T1): T2 { body }
```

##### Output (AST after closure conversion)
A closure class AST node is created:

```
class Closure$lambda$N {
    fields: captured variables
    constructor(captured variables)
    method Apply(x: T1): T2
}
```

##### Phase
Generated during **closure conversion**

##### Backend Output (C#)
```csharp
internal sealed class Closure$lambda$1 : IClosure<int,int> {
    private readonly int factor;
    public Closure$lambda$1(int factor) { this.factor = factor; }
    public int Apply(int x) => x * factor;
}
```

### **Function Types in the AST**

##### Problem
The current grammar uses `IDENTIFIER` in the function-type rule; this must be updated to accept full `type_name` expressions (including generics and compound types) so function types can be used as parameter types.

##### Requirement
The AST must represent function types explicitly so that:

- Higher‑order functions can be type‑checked  
- Closure conversion can replace function types with closure types  
- The Roslyn backend can generate `Func<T1,T2>` for public APIs  

##### Specification: AST Representation of Function Types
Add a new AST type node:

```
FunctionType(
    Parameters: List<Type>,
    ReturnType: Type
)
```

This corresponds to the existing `FunctionSignature` type system object, but is now **first‑class in the AST**.

##### Example AST node
```
Parameter: f: FunctionType([T1], T2)
```

##### Lowered AST (after defunctionalisation)
```
Parameter: fClosure: IClosure<T1,T2>
```

##### Backend Output (C#)
```csharp
public static List<T2> map(List<T1> xs, Func<T1,T2> f)
```

### **Function Application (`f(x)`)**

##### Input (AST)
```
f(x)
```

##### Output (AST after closure conversion)
If `f` is a function value (lambda or captured function):

```
f.Apply(x)
```

If `f` is a named function:

```
call f(...)
```

##### Phase
Rewritten during **closure conversion**

##### Backend Output (C#)
```csharp
// Named function call
var y = map(xs, closure);

// Closure invocation
var y = closure.Apply(x);
```

### **Higher‑Order Function Calls**

##### Input (AST)
```
map(xs, f)
```

##### Output (AST after defunctionalisation)
```
map$worker(xs, fClosure)
```

##### Backend Output (C#)
```csharp
public static List<T2> map(List<T1> xs, Func<T1,T2> f) {
    var closure = new DelegateClosure<T1,T2>(f);
    return map$worker(xs, closure);
}
```

### **Captured Variables**

##### Input (AST)
```
fun(x) { x * factor }
```

##### Output (AST after closure conversion)
```
class Closure$lambda$N {
    field factor
    constructor(factor)
    Apply(x) { x * factor }
}
```

##### Backend Output (C#)
```csharp
new Closure$lambda$1(factor)
```

### **Recursive Calls**

##### Input (AST)
```
map(tail(ts), f)
```

##### Output (AST after defunctionalisation)
```
map$worker(tail(ts), fClosure)
```

##### Backend Output (C#)
```csharp
return map$worker(tail, fClosure);
```

### **Exported Functions**

##### Input (AST)
```
export map
```

##### Output (AST → Roslyn backend)
- Public wrapper with `Func<>`  
- Internal worker with `IClosure<>`

### **Non‑Exported Functions**

##### Input (AST)
```
local helper(...)
```

##### Output
- Only worker is generated  
- No wrapper  

### **Tail‑Call Optimisation (Optional)**

##### Input (AST)
Tail‑recursive call in tail position.

##### Output (AST after TCO pass)
```
return Call(() => f$worker(...))
```

##### Backend Output (C#)
```csharp
return new Call<List<T2>>(() => map$worker(tail, fClosure));
```

### **Invariant Principles (AST vs Backend Responsibilities)**

#### AST Invariants

##### The AST must represent:
- Function declarations  
- Lambda expressions  
- Function types (`FunctionType`)  
- Function application  
- Variable references  
- Captured variables  
- Closure classes (after closure conversion)  
- Worker functions (after defunctionalisation)  
- Tail‑call markers (optional)

##### The AST must not represent:
- `System.Func<>`  
- Method groups  
- .NET metadata  
- Public wrappers  
- Roslyn syntax nodes  
- IL concepts  

#### Roslyn Backend Invariants

##### The Roslyn backend must generate:
- Public wrappers with `Func<>`  
- Internal workers with `IClosure<>`  
- Closure classes  
- Delegate adapters (`DelegateClosure<T1,T2>`)  
- Assembly metadata  

##### The Roslyn backend must not perform:
- Closure conversion  
- Defunctionalisation  
- Tail‑call analysis  
- Lambda lifting  
- Capture analysis  

These belong exclusively to the AST lowering pipeline.

#### Summary

- All semantic transformations occur **in the AST**.  
- The AST must include a **first‑class function type node** (`FunctionType`).  
- Closure conversion introduces closure classes and `Apply` calls.  
- Defunctionalisation replaces function types with `IClosure<>` and generates worker functions.  
- The Roslyn backend generates public wrappers using `Func<>` and emits the final assembly.  
