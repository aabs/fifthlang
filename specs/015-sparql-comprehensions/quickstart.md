# Quickstart: SPARQL Comprehensions

This feature adds a list-comprehension form that can map a tabular SPARQL SELECT result into a typed Fifth list.

## Syntax

- General shape:

```fifth
[ <projection> from <generator> (where <constraint1>, <constraint2>, ...)? ]
```

- Projection options:
  - Variable projection: `<varName>`
  - Object projection: `new TypeName() { Prop1 = ?var1, Prop2 = ?var2 }`

## Example: Project objects from a SELECT result

```fifth
class Person {
    Name: string;
}

store default = sparql_store(<http://example.org/store>);

main(): int {
    // Execute a SELECT query (tabular result)
    res: result = ?<
        SELECT ?name
        WHERE { ?s <http://example.org/name> ?name . }
    > <- default;

    // Project rows into typed objects
    people: [Person] = [
        new Person() { Name = ?name }
        from res
        // Constraint syntax is finalized in planning; recommended: `it.<prop>`
        where it.Name != nil
    ];

    return 0;
}
```

## Errors you can expect

- Non-SELECT query used as generator → compile-time error (generator must be tabular SELECT).
- Using a `?var` that is not projected by the SELECT query → compile-time error.
- If a row has an unbound value for a referenced `?var` → runtime error when the value is accessed.

## Notes

- Legacy list comprehension syntax using `in` and `#` is rejected (breaking change).
- Multiple `where` constraints are AND-ed.
