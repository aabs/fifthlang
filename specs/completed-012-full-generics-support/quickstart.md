# Quickstart: Generic Types in Fifth

**Feature**: `001-full-generics-support` | **Audience**: Fifth language developers | **Date**: 2025-11-18

This guide demonstrates how to use generic types in Fifth with practical examples. After implementing this feature, Fifth will support:

✅ Generic classes with type parameters  
✅ Generic functions with type inference  
✅ Type constraints (interface, base class, constructor)  
✅ Nested generic types  
✅ Multiple type parameters  

---

## Basic Generic Class

Define a simple generic stack data structure:

```fifth
class Stack<T> {
    items: List<T>;
    
    push(item: T) {
        items.add(item);
    }
    
    pop(): T {
        result: T = items[items.length - 1];
        items.removeAt(items.length - 1);
        return result;
    }
    
    isEmpty(): bool {
        return items.length == 0;
    }
}
```

**Usage**:
```fifth
main(): int {
    stack: Stack<int> = Stack<int>();
    stack.push(42);
    stack.push(100);
    
    value: int = stack.pop();
    std.print(value);  // Prints: 100
    
    return 0;
}
```

---

## Generic Function with Type Inference

Write a generic identity function that works with any type:

```fifth
// Explicit type parameter
identity<T>(x: T): T => x;

main(): int {
    // Type inference: T = int
    result: int = identity(42);
    std.print(result);  // Prints: 42
    
    // Type inference: T = string
    text: string = identity("hello");
    std.print(text);  // Prints: hello
    
    return 0;
}
```

**Type inference rules**:
- Type parameter `T` is inferred from argument type `x: T`
- No need to write `identity<int>(42)` — Fifth infers `T = int`
- Explicit type arguments are optional: `identity<string>("hello")` is valid

---

## Generic Function with Multiple Type Parameters

Swap two values of different types:

```fifth
swap<T, U>(a: T, b: U): (U, T) {
    return (b, a);
}

main(): int {
    // Infer T = int, U = string
    (text, num): (string, int) = swap(123, "abc");
    
    std.print(num);   // Prints: 123
    std.print(text);  // Prints: abc
    
    return 0;
}
```

---

## Generic Constraints: Interface Constraint

Constrain a type parameter to implement an interface:

```fifth
// Fifth standard library interface (assumed to exist)
interface IComparable<T> {
    compareTo(other: T): int;
}

// Generic function that requires T to be comparable
max<T>(a: T, b: T): T where T: IComparable<T> {
    if (a.compareTo(b) > 0) {
        return a;
    } else {
        return b;
    }
}

main(): int {
    // int implements IComparable<int>
    larger: int = max(42, 100);
    std.print(larger);  // Prints: 100
    
    // Error: string does not implement IComparable<string> (yet)
    // result: string = max("apple", "banana");  // GEN003 error
    
    return 0;
}
```

**Error message** (if constraint violated):
```
GEN003: Type argument 'string' does not satisfy constraint 'IComparable<string>'
  at line 18: max("apple", "banana")
```

---

## Generic Constraints: Multiple Constraints

Combine multiple constraints:

```fifth
interface IDisposable {
    dispose();
}

interface ISerializable {
    serialize(): string;
}

// Requires T to implement both interfaces AND have a constructor
processResource<T>(resource: T) where T: IDisposable, ISerializable, new() {
    data: string = resource.serialize();
    std.print(data);
    resource.dispose();
}

class FileHandle implements IDisposable, ISerializable {
    path: string;
    
    // Parameterless constructor (satisfies 'new()' constraint)
    FileHandle() {
        path = "default.txt";
    }
    
    dispose() {
        std.print("File closed");
    }
    
    serialize(): string {
        return "FileHandle:" + path;
    }
}

main(): int {
    handle: FileHandle = FileHandle();
    processResource(handle);
    // Prints:
    // FileHandle:default.txt
    // File closed
    
    return 0;
}
```

**Constraint order**:
1. Interface constraints (`IDisposable`, `ISerializable`)
2. Constructor constraint (`new()` must be last)

---

## Generic Constraints: Base Class Constraint

Require a type parameter to derive from a specific base class:

```fifth
class Animal {
    name: string;
    
    makeSound(): string {
        return "Generic animal sound";
    }
}

class Dog extends Animal {
    makeSound(): string {
        return "Woof!";
    }
}

class Cat extends Animal {
    makeSound(): string {
        return "Meow!";
    }
}

// Generic function that requires T to be an Animal or subclass
playWithAnimal<T>(animal: T) where T: Animal {
    sound: string = animal.makeSound();
    std.print(sound);
}

main(): int {
    dog: Dog = Dog();
    cat: Cat = Cat();
    
    playWithAnimal(dog);  // Prints: Woof!
    playWithAnimal(cat);  // Prints: Meow!
    
    // Error: int does not derive from Animal
    // playWithAnimal(42);  // GEN003 error
    
    return 0;
}
```

---

## Nested Generic Types

Use generic types as type arguments to other generic types:

```fifth
class Pair<T, U> {
    first: T;
    second: U;
    
    Pair(f: T, s: U) {
        first = f;
        second = s;
    }
}

main(): int {
    // Nested generics: List of Pairs
    pairs: List<Pair<string, int>> = List<Pair<string, int>>();
    
    pair1: Pair<string, int> = Pair("age", 30);
    pair2: Pair<string, int> = Pair("height", 180);
    
    pairs.add(pair1);
    pairs.add(pair2);
    
    // Access nested generic data
    foreach (p in pairs) {
        std.print(p.first);   // Prints: age, height
        std.print(p.second);  // Prints: 30, 180
    }
    
    return 0;
}
```

---

## Generic Methods in Generic Classes

Combine class-level and method-level type parameters:

```fifth
class Container<T> {
    value: T;
    
    Container(v: T) {
        value = v;
    }
    
    // Method with its own type parameter U
    map<U>(mapper: (T) => U): Container<U> {
        newValue: U = mapper(value);
        return Container<U>(newValue);
    }
}

main(): int {
    // Container<int> with value 42
    intContainer: Container<int> = Container(42);
    
    // Map int to string (U = string)
    stringContainer: Container<string> = intContainer.map((x: int) => {
        return "Number: " + x.toString();
    });
    
    std.print(stringContainer.value);  // Prints: Number: 42
    
    return 0;
}
```

**Type parameter scoping**:
- `T` is the class-level type parameter (bound at `Container<int>` instantiation)
- `U` is the method-level type parameter (inferred from `mapper` function return type)
- Inner `U` does not shadow outer `T` — both are in scope within `map` method

---

## Type Inference with Return Type Context

Fifth infers type arguments from expected return type:

```fifth
emptyList<T>(): List<T> {
    return List<T>();
}

main(): int {
    // Infer T = int from expected type annotation
    numbers: List<int> = emptyList();
    
    // Infer T = string from expected type annotation
    words: List<string> = emptyList();
    
    return 0;
}
```

---

## Explicit Type Arguments (When Inference Fails)

Sometimes type inference is ambiguous — provide explicit type arguments:

```fifth
parseValue<T>(text: string): T {
    // Parsing logic (implementation omitted)
    // ...
}

main(): int {
    // Ambiguous: Cannot infer T from string argument
    // value: ??? = parseValue("42");  // ERROR
    
    // Solution: Provide explicit type argument
    value: int = parseValue<int>("42");
    std.print(value);  // Prints: 42
    
    flag: bool = parseValue<bool>("true");
    std.print(flag);  // Prints: true
    
    return 0;
}
```

---

## Interop with .NET Generic Types

Fifth generic types map directly to .NET generics — seamless interop:

```fifth
// Use .NET generic collection (System.Collections.Generic.Dictionary)
main(): int {
    // Fifth syntax for .NET Dictionary<string, int>
    ages: Dictionary<string, int> = Dictionary<string, int>();
    
    ages["Alice"] = 30;
    ages["Bob"] = 25;
    
    aliceAge: int = ages["Alice"];
    std.print(aliceAge);  // Prints: 30
    
    return 0;
}
```

**Note**: Fifth's built-in `List<T>` and `Dictionary<K,V>` are aliases for .NET's generic collections.

---

## Common Errors and Diagnostics

### GEN001: Wrong Number of Type Arguments

```fifth
class Pair<T, U> { ... }

main(): int {
    // Error: Pair requires 2 type arguments, got 1
    p: Pair<int> = Pair<int>(1, 2);  // GEN001 error
    
    return 0;
}
```

**Error message**:
```
GEN001: Generic type 'Pair' expects 2 type arguments, but 1 provided
  at line 4: Pair<int>
```

---

### GEN002: Type Argument Inference Failed

```fifth
identity<T>(x: T): T => x;

main(): int {
    // Error: Cannot infer T from void context
    identity(42);  // GEN002 error (if result not assigned)
    
    return 0;
}
```

**Error message**:
```
GEN002: Cannot infer type argument 'T' from usage
  at line 4: identity(42)
  hint: Provide explicit type argument: identity<int>(42)
```

---

### GEN003: Constraint Violation

```fifth
max<T>(a: T, b: T): T where T: IComparable<T> { ... }

main(): int {
    // Error: Custom class does not implement IComparable
    result: MyClass = max(MyClass(), MyClass());  // GEN003 error
    
    return 0;
}
```

**Error message**:
```
GEN003: Type argument 'MyClass' does not satisfy constraint 'IComparable<MyClass>'
  at line 4: max(MyClass(), MyClass())
```

---

### GEN004: Missing Constructor

```fifth
create<T>(): T where T: new() {
    return T();  // Requires parameterless constructor
}

class NoConstructor {
    NoConstructor(x: int) { ... }  // Only parameterized constructor
}

main(): int {
    // Error: NoConstructor lacks parameterless constructor
    obj: NoConstructor = create<NoConstructor>();  // GEN004 error
    
    return 0;
}
```

**Error message**:
```
GEN004: Type argument 'NoConstructor' does not have a parameterless constructor
  at line 10: create<NoConstructor>()
  required by constraint 'new()' on type parameter 'T'
```

---

## Best Practices

### ✅ DO: Use Type Inference When Possible

```fifth
// Good: Type inference reduces boilerplate
result: int = identity(42);

// Avoid: Unnecessary explicit type argument
result: int = identity<int>(42);
```

### ✅ DO: Constrain Type Parameters When Needed

```fifth
// Good: Constraint ensures type safety
sort<T>(items: List<T>) where T: IComparable<T> { ... }

// Bad: No constraint — will fail at runtime if T not comparable
sort<T>(items: List<T>) { 
    // items[0].compareTo(items[1])  // ERROR if T lacks compareTo
}
```

### ✅ DO: Use Meaningful Type Parameter Names

```fifth
// Good: TKey and TValue clearly indicate purpose
class Dictionary<TKey, TValue> { ... }

// Avoid: Single-letter names are confusing in complex generics
class Dictionary<T, U> { ... }
```

### ❌ DON'T: Over-Constrain Type Parameters

```fifth
// Bad: Too many constraints limit reusability
process<T>(item: T) where T: IDisposable, ISerializable, IComparable, new() { ... }

// Good: Only add constraints you actually need
process<T>(item: T) where T: IDisposable { ... }
```

### ❌ DON'T: Create Deeply Nested Generics Unless Necessary

```fifth
// Bad: Deeply nested generics hurt readability
data: Dictionary<string, List<Tuple<int, List<Pair<string, bool>>>>> = ...;

// Good: Use type aliases for complex nested types (future feature)
type PersonData = Tuple<int, List<Pair<string, bool>>>;
data: Dictionary<string, List<PersonData>> = ...;
```

---

## Summary

Generic types in Fifth provide:

- **Type safety**: Compile-time checking prevents type errors
- **Code reuse**: Write once, work with any type
- **Type inference**: Automatic type argument resolution reduces boilerplate
- **Constraints**: Express requirements on type parameters (interfaces, base classes, constructors)
- **.NET interop**: Seamless integration with .NET generic APIs

**Syntax Summary**:
```fifth
// Generic class
class Name<T, U> { ... }

// Generic function
func<T>(x: T): T { ... }

// Constraints
func<T>(x: T) where T: IComparable, new() { ... }

// Instantiation
obj: Stack<int> = Stack<int>();

// Type inference
result: int = identity(42);  // T = int inferred
```

**Next Steps**: See `data-model.md` for implementation details of AST nodes and type system extensions.
