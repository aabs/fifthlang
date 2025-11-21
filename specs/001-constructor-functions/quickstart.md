# Quickstart: Constructor Functions

## Goal
Add an explicit constructor to a class, build, and validate via tests.

## Steps

```bash
# 1. Ensure prerequisites
dotnet --version
java -version

# 2. Regenerate (if metamodel edited)
just run-generator

# 3. Build full solution (do not cancel)
dotnet build fifthlang.sln

# 4. Add parser & semantic tests first (TDD)
#   - syntax-parser-tests: new sample .5th with constructor forms
#   - runtime-integration-tests: object creation + inheritance

# 5. Run focused tests
dotnet test test/syntax-parser-tests/ -v minimal

# 6. Run broader suite
dotnet test fifthlang.sln -v minimal
```

## Example Class
```fifth
class Person {
    Name: string;
    Age: int;
    Person(name: string, age: int) {
        Name = name;
        Age = age;
    }
}

main(): int {
    p: Person = new Person("Ada", 37);
    return 0;
}
```

## Inheritance Example
```fifth
class Base {
    X: int;
    Base(x: int) { X = x; }
}

class Derived : Base {
    Y: int;
    Derived(x: int, y: int) : base(x) { Y = y; }
}
```

## Required Field Diagnostic
```fifth
class Bad {
    X: int;
    Bad() { /* X not assigned → CTOR003 */ }
}
```

## Common Errors
| Issue | Symptom | Fix |
|-------|---------|-----|
| Missing assignment | CTOR003 | Assign all required fields |
| Ambiguous overload | CTOR002 | Remove or differentiate signatures |
| No matching ctor | CTOR001 | Add constructor or adjust arguments |
| Missing base call | CTOR004 | Add `: base(...)` initializer |
| Duplicate signature | CTOR006 | Merge or change parameter types |

## Next Steps
Implement overloads, add inheritance tests, measure performance with synthetic stress class.
