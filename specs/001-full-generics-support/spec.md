# Feature Specification: Full Generics Support

**Feature Branch**: `001-full-generics-support`  
**Created**: 2025-11-18  
**Status**: Draft  
**Input**: User description: "Full Generics Support - Implement complete generic type support including generic classes, generic functions, type parameters, type constraints, and type inference for the Fifth programming language"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generic Collection Classes (Priority: P1)

Developers need to define reusable data structures that work with any type, starting with the most common case: collections like Stack, Queue, and List that store elements of a single type.

**Why this priority**: Collections are the foundational use case for generics. Every developer needs type-safe collections, and this provides immediate value by eliminating code duplication and type-casting errors.

**Independent Test**: Can be fully tested by defining a `Stack<T>` class, instantiating it with different types (int, string, custom classes), and verifying push/pop operations maintain type safety without casting. Delivers value by allowing developers to write one Stack implementation instead of Stack_Int, Stack_String, etc.

**Acceptance Scenarios**:

1. **Given** a developer defines `class Stack<T> { items: [T]; push(item: T): void; pop(): T }`, **When** they instantiate `s: Stack<int> = new Stack<int>()` and call `s.push(42)`, **Then** the compiler accepts this without errors and maintains type safety
2. **Given** a `Stack<string>` instance, **When** a developer attempts to push an integer, **Then** the compiler produces a type error
3. **Given** a generic Stack class with type parameter T, **When** a developer calls `pop()` on a `Stack<Person>`, **Then** the return type is correctly inferred as `Person`
4. **Given** multiple instantiations `Stack<int>` and `Stack<string>`, **When** used in the same program, **Then** each maintains independent type safety

---

### User Story 2 - Generic Functions with Type Inference (Priority: P1)

Developers need to write utility functions that work with any type (like identity, swap, map, filter) and have the compiler automatically determine type arguments from usage context to avoid verbose type annotations.

**Why this priority**: Generic functions are equally fundamental to generics as generic classes. Type inference eliminates boilerplate and makes generic code practical for everyday use.

**Independent Test**: Can be fully tested by defining `func identity<T>(x: T): T { return x; }`, calling it with various types without explicit type arguments (`identity(42)`, `identity("hello")`), and verifying the compiler correctly infers types. Delivers value by enabling reusable utility functions without repetitive type annotations.

**Acceptance Scenarios**:

1. **Given** a generic function `func identity<T>(x: T): T`, **When** a developer calls `result: int = identity(42)`, **Then** the compiler infers T=int without requiring explicit type arguments
2. **Given** a generic function `func pair<T1, T2>(a: T1, b: T2): (T1, T2)`, **When** called with `pair(1, "hello")`, **Then** the compiler infers T1=int and T2=string
3. **Given** ambiguous type inference context, **When** a developer writes `identity(null)`, **Then** the compiler reports an error requiring explicit type arguments
4. **Given** explicit type arguments provided `identity<string>("test")`, **When** the argument type matches, **Then** compilation succeeds with explicit type

---

### User Story 3 - Generic Methods in Classes (Priority: P2)

Developers need to add generic methods to both generic and non-generic classes, where method type parameters are independent of class type parameters, enabling flexible APIs like `List<T>.map<U>()`.

**Why this priority**: This enables advanced patterns like transforming collections while maintaining type safety. While less critical than basic generics, it's essential for ergonomic collection APIs.

**Independent Test**: Can be fully tested by defining a non-generic class with a generic method (`class Util { static func swap<T>(x: T, y: T): (T, T) }`), calling it with different types, and verifying correct behavior. Delivers value by enabling flexible utility methods without requiring the entire class to be generic.

**Acceptance Scenarios**:

1. **Given** a non-generic class with generic method `class Util { static func swap<T>(x: T, y: T): (T, T) }`, **When** called with `Util.swap(1, 2)`, **Then** the compiler correctly infers T=int
2. **Given** a generic class `List<T>` with generic method `map<U>(f: (T) -> U): List<U>`, **When** a developer calls `intList.map<string>(toString)`, **Then** U is treated as independent from T
3. **Given** method type parameters shadowing class type parameters, **When** both use the same name `T`, **Then** the method's T takes precedence within method scope

---

### User Story 4 - Type Constraints for Safe Operations (Priority: P2)

Developers need to constrain generic type parameters to ensure they support required operations (like comparison, serialization, or specific methods), enabling algorithms that depend on type capabilities.

**Why this priority**: Constraints make generics practical for real algorithms. Without them, generic code can only perform operations available on all types (essentially nothing useful beyond storage).

**Independent Test**: Can be fully tested by defining `func sort<T>(items: [T]): [T] where T: IComparable`, attempting to call it with types that do/don't implement IComparable, and verifying the compiler enforces the constraint. Delivers value by enabling type-safe generic algorithms like sorting and searching.

**Acceptance Scenarios**:

1. **Given** a generic function `func sort<T>(items: [T]): [T] where T: IComparable`, **When** called with a type implementing IComparable, **Then** compilation succeeds and compareTo() can be used safely
2. **Given** the same constrained function, **When** called with a type not implementing IComparable, **Then** the compiler produces a clear constraint violation error
3. **Given** multiple constraints `func process<T>(item: T): void where T: ISerializable, ICloneable`, **When** a type satisfies all constraints, **Then** all constrained operations are available
4. **Given** constructor constraint `func create<T>(): T where T: new()`, **When** called with a type having a parameterless constructor, **Then** `new T()` compiles successfully

---

### User Story 5 - Multiple Type Parameters (Priority: P3)

Developers need to define types and functions with multiple independent type parameters (like Dictionary<TKey, TValue>) to model complex data structures and relationships.

**Why this priority**: While important for advanced scenarios, single type parameter covers 80% of use cases. This can be added after core generics work.

**Independent Test**: Can be fully tested by defining `class Dictionary<TKey, TValue>` with methods like `add(key: TKey, value: TValue): void` and `get(key: TKey): TValue`, instantiating with different type combinations, and verifying each type parameter is handled independently. Delivers value for complex data structures.

**Acceptance Scenarios**:

1. **Given** a class `Dictionary<TKey, TValue>`, **When** instantiated as `Dictionary<string, Person>`, **Then** both type parameters are correctly tracked independently
2. **Given** a function `func combine<T1, T2, T3>(a: T1, b: T2, c: T3): (T1, T2, T3)`, **When** called with three different types, **Then** all three types are correctly inferred independently
3. **Given** constraints on multiple parameters `class Cache<TKey, TValue> where TKey: IHashable, TValue: ISerializable`, **When** both constraints are satisfied, **Then** operations requiring both constraints work correctly

---

### User Story 6 - Nested Generic Types (Priority: P3)

Developers need to use generic types as type arguments to other generic types (like `List<Dictionary<string, int>>`) to build complex data structures.

**Why this priority**: This is an advanced scenario that enables sophisticated data modeling, but can be deferred until core generics are stable.

**Independent Test**: Can be fully tested by declaring nested generic types like `items: List<Stack<int>>` and performing operations that traverse the nesting levels. Delivers value for modeling hierarchical or nested data.

**Acceptance Scenarios**:

1. **Given** a type declaration `items: List<Stack<int>>`, **When** accessing elements, **Then** the type system correctly tracks `items[0]` as `Stack<int>` and `items[0].pop()` as `int`
2. **Given** nested generics with constraints `List<T> where T: IComparable`, **When** instantiated as `List<Stack<int>>`, **Then** the constraint applies to Stack<int> as a whole

---

### Edge Cases

- What happens when a type parameter is used but never constrained or instantiated? (Should be allowed for abstract definitions)
- How does the system handle recursive generic constraints like `class Node<T> where T: Node<T>`? (Should detect and report cycles)
- What happens when type inference fails due to insufficient information? (Clear error message requiring explicit type arguments)
- How does the system handle generic type instantiation with type arguments that don't satisfy constraints? (Compile-time error with constraint details)
- What happens when a generic function is called with null and inference is ambiguous? (Error requiring explicit type arguments)
- How does the system differentiate between `Stack<int>` and `Stack<long>` at runtime? (Type erasure vs reification decision needed)
- What happens when attempting to instantiate a generic type with void or incomplete types? (Compile-time error)

## Requirements *(mandatory)*

### Functional Requirements

#### Generic Type Definitions

- **FR-001**: System MUST allow class definitions with one or more type parameters using angle bracket syntax `<T>`, `<T1, T2>`, etc.
- **FR-002**: System MUST allow type parameters to be used in property declarations within the generic class
- **FR-003**: System MUST allow type parameters to be used in method parameter types and return types within the generic class
- **FR-004**: System MUST allow type parameters to be used for local variable declarations within generic class methods
- **FR-005**: System MUST support multiple type parameters on a single class definition (e.g., `Dictionary<TKey, TValue>`)
- **FR-006**: System MUST ensure type parameter names are valid identifiers following language naming conventions

#### Generic Function Definitions

- **FR-007**: System MUST allow module-level functions to declare type parameters
- **FR-008**: System MUST allow instance and static methods within classes to declare type parameters independent of class-level type parameters
- **FR-009**: System MUST support multiple type parameters on a single function definition
- **FR-010**: System MUST allow function type parameters to shadow class type parameters when using the same name (function scope takes precedence)

#### Type Constraints

- **FR-011**: System MUST support interface constraints on type parameters using `where T: InterfaceName` syntax
- **FR-012**: System MUST support base class constraints on type parameters using `where T: ClassName` syntax
- **FR-013**: System MUST support constructor constraints on type parameters using `where T: new()` syntax
- **FR-014**: System MUST support multiple constraints on a single type parameter using comma separation
- **FR-015**: System MUST validate at compile time that type arguments satisfy all declared constraints
- **FR-016**: System MUST provide clear error messages when type arguments violate constraints, showing which constraint failed

#### Type Inference

- **FR-017**: System MUST infer type arguments from function call arguments when unambiguous
- **FR-018**: System MUST allow explicit type argument specification using angle bracket syntax when inference is not possible
- **FR-019**: System MUST infer type arguments based on assignment context (e.g., `x: Stack<int> = new Stack()` infers int)
- **FR-020**: System MUST report clear errors when type inference fails, indicating what additional information is needed
- **FR-021**: System MUST prioritize explicit type arguments over inferred ones when both are present

#### Generic Type Instantiation

- **FR-022**: System MUST support instantiating generic types with explicit type arguments (e.g., `new Stack<int>()`)
- **FR-023**: System MUST validate type arguments satisfy constraints at instantiation sites
- **FR-024**: System MUST create distinct type instances for each unique combination of type arguments
- **FR-025**: System MUST support using generic types in variable declarations, parameter types, and return types

#### Type System Integration

- **FR-026**: System MUST extend the existing FifthType discriminated union to represent generic type parameters and instantiated generic types
- **FR-027**: System MUST track type parameter bindings throughout the compilation pipeline
- **FR-028**: System MUST perform type substitution when resolving member access on generic type instances
- **FR-029**: System MUST ensure type safety across all generic operations (no type erasure bugs)

#### Scoping and Resolution

- **FR-030**: System MUST resolve type parameter references within the scope where they are declared
- **FR-031**: System MUST prevent type parameter name conflicts within the same scope
- **FR-032**: System MUST allow type parameters to be referenced anywhere within their defining class or function

### Key Entities

- **TypeParameterDef**: Represents a declared type parameter with a name and optional constraints. Part of the AST model, attached to ClassDef or FunctionDef nodes.
- **TypeConstraint**: Abstract representation of constraints on type parameters, with concrete subtypes for interface, base class, and constructor constraints.
- **GenericTypeInstance**: Represents a specific instantiation of a generic type with concrete type arguments (e.g., Stack<int> is an instance of the generic definition Stack<T>).
- **TypeArgumentBinding**: Maps type parameters to concrete type arguments during instantiation and type checking.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can define a generic collection class (Stack, Queue, or List) with full type safety in under 20 lines of code
- **SC-002**: Developers can call generic functions without explicit type arguments in 80% of common cases (successful type inference)
- **SC-003**: Compilation time for programs using generics increases by less than 15% compared to equivalent non-generic code
- **SC-004**: Type error messages for generic code include specific type parameter information and constraint violations in human-readable format
- **SC-005**: Generic code passes all type safety tests (no successful compilation of type-violating code)
- **SC-006**: 100% of existing non-generic code continues to compile without modification
- **SC-007**: Developers can implement a generic sorting algorithm with type constraints in under 10 lines of constraint declaration
- **SC-008**: System correctly handles nested generic types (like List<Stack<int>>) with proper type checking at each nesting level
- **SC-009**: All generic features work correctly with Fifth's existing features (parameter constraints, destructuring, knowledge graphs, etc.) with no regressions

### Quality Metrics

- **SC-010**: Generic type instantiation errors report line numbers, type parameter names, and specific constraint violations
- **SC-011**: Documentation includes at least 5 complete working examples of generic classes and functions
- **SC-012**: Test suite includes tests for single type parameter, multiple type parameters, constraints, type inference, and nested generics
- **SC-013**: All test scenarios from User Stories 1-6 pass with expected behavior

## Assumptions

- The current Fifth grammar supports angle bracket syntax without ambiguity (less-than operator context can be distinguished)
- The .NET backend supports mapping Fifth generic types to .NET generic types
- Type inference algorithm follows standard unification-based inference similar to C#/TypeScript
- Constructor constraint checking can leverage existing type system capabilities for detecting parameterless constructors
- Existing collection syntax `[T]` and `T[]` will be treated as sugar for `List<T>` and `Array<T>` once generics are implemented
- Runtime will use reified generics (preserving type information) rather than type erasure, given .NET backend capabilities

## Dependencies

- Current Fifth type system infrastructure in `src/ast-model/TypeSystem/`
- Existing grammar in `src/parser/grammar/FifthParser.g4` and `FifthLexer.g4`
- AST builder visitor in `src/parser/AstBuilderVisitor.cs`
- Type registry and type resolution in `src/ast-model/TypeSystem/TypeRegistry.cs`
- Code generation pipeline in `src/compiler/LoweredToRoslyn/`
- Test infrastructure in `test/ast-tests/` and `test/runtime-integration-tests/`

## Out of Scope

- Variance annotations (covariance/contravariance with `in`/`out` modifiers) - future enhancement
- Higher-kinded types (generic type parameters like `Functor<F<_>>`) - future enhancement
- Partial generic type application - all type arguments must be provided together
- Default type arguments for optional type parameters - future enhancement
- Generic type aliases - future enhancement
- Dependent types or refinement types - future enhancement
- Generic constraints based on operator availability - future enhancement
