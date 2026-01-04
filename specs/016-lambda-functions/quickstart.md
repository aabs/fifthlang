# Quickstart: Lambda Functions

## Defining a Lambda
Use the `fun` keyword to define an anonymous function.

```fifth
// Assign to a variable
square: [int] -> int = fun(x: int): int { 
    return x * x; 
};

// Use it
result: int = square(5); // 25
```

## Higher-Order Functions
Pass lambdas to functions.

```fifth
// Define a HOF
apply<T>(val: T, f: [T] -> T): T {
    return f(val);
}

// Call it
val: int = apply(10, fun(x: int): int { return x + 1; }); // 11
```

## Capturing Variables
Lambdas can capture variables from their enclosing scope (by value).

```fifth
factor: int = 2;
multiplier: [int] -> int = fun(x: int): int {
    return x * factor;
};

res: int = multiplier(10); // 20
```

## Recursion
Lambdas can be recursive by capturing the variable they are assigned to.

```fifth
factorial: [int] -> int = fun(n: int): int {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
};
```

## Generic Lambdas
Lambdas can have their own type parameters.

```fifth
identity: [T] -> T = fun<T>(x: T): T {
    return x;
};
```
