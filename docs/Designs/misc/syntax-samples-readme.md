# Syntax Samples

This folder contains one-file-per-bullet samples generated from the syntax test plan. Valid cases are under `TestPrograms/Syntax/`, invalid under `TestPrograms/Syntax/Invalid/`.

The test `SyntaxParserTests` parses all valid samples and asserts invalid samples fail to parse.

Also see `docs/knowledge-graphs.md` for canonical store syntax and graph assertion block examples.

## Exception Handling

Fifth supports C#-style exception handling with `try`/`catch`/`finally` blocks:

```fifth
// Basic try/finally
main(): int {
    x: int = 10;
    try {
        x = x + 5;
    } finally {
        std.print("cleanup");
    }
    return x;
}

// Try/catch with catch-all
main(): int {
    result: int = 0;
    try {
        result = 42;
    } catch {
        result = 1;
    }
    return result;
}

// Try/catch/finally combined
main(): int {
    result: int = 0;
    try {
        result = 10;
    } catch {
        result = 1;
    } finally {
        result = result + 5;
    }
    return result;
}
```

### Throw Expressions

Throw can be used in expression contexts:

```fifth
// Throw expression in null-coalescing (future syntax)
var x = getValue() ?? throw new Exception();

// Throw expression in conditional
var y = condition ? result : throw new Exception();
```

Note: Full exception type support (e.g., `System.Exception`) requires parser enhancements for qualified type names.

## Namespaces & Imports

Fifth supports file-scoped namespaces and explicit imports. Declarations are file-local, while imports bring symbols into scope for that module only.

```fifth
// math.5th
namespace math;

export add(a: int, b: int): int {
    return a + b;
}
```

```fifth
// consumer.5th
import math;

main(): int {
    return add(2, 3);
}
```
