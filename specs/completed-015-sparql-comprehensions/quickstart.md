# Quickstart: SPARQL Comprehensions

This feature adds a list-comprehension form that can map a tabular SPARQL SELECT result into a typed Fifth list.

## Syntax

- General shape:

```fifth
[ <projection> from <generator> (where <constraint1>, <constraint2>, ...)? ]
```

- Projection options:
  - Variable projection: `<varName>`
  - Object projection: `new TypeName() { Prop1 = x.var1, Prop2 = x.var2 }`
  - Note: `x` is the iteration variable declared in the `from` clause

## Example: Project objects from a SELECT result

```fifth
class Person{
    Id: Uri;
    Age: int;
    Name: string;
}


main(): int {
    g: graph = @< 
        @prefix : <http://tempuri.org/etc/>.
        :andrew :age 56;
                :name "Andrew Matthews" .
        :kerry :age 55;
                :name "Kerry Matthews" .
    >;

    // create a query over the graph
    r: query = ?<
    PREFIX : <http://tempuri.org/etc/>

    SELECT ?p ?age ?name
    WHERE
    {
        ?p :age ?age;
        :name ?name .
    }>;

    // now build a list of Person objects by applying the query to the graph and filtering the results
    // Note: x represents each result row, and x.property accesses the value of ?property in that row

    people: [Person] = [new Person(){Id = x.p, Age = x.age, Name = x.name} // <-- projection
                        from x in r <- g                                    // <-- generator (x is the iteration variable)
                        where x.age < 21, x.age > 12                        // <-- constraints
                    ];
    return 0;
}
```

## Errors you can expect

- Non-SELECT query used as generator → compile-time error (generator must be tabular SELECT).
- Using `x.property` where `property` is not projected by the SELECT query → compile-time error.
- Using `?var` notation in comprehensions → compile-time error (must use property access `x.var` instead).
- If a row has an unbound value for a referenced property → runtime error when the value is accessed.

## Notes

- Legacy list comprehension syntax using `in` and `#` is rejected (breaking change).
- Migration guidance: see [migration.md](migration.md).
- Multiple `where` constraints are AND-ed.
