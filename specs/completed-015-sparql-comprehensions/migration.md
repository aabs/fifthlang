# Migration Note — List Comprehensions Syntax Changes

This feature introduces breaking syntax changes for list comprehensions:

1. **Keyword changes** (all comprehensions):
   - `in` is replaced by `from`
   - `#` (legacy "such-that" marker) is replaced by `where`
   - The legacy `in` and `#` forms are rejected.

2. **SPARQL comprehension variable access** (SPARQL comprehensions only):
   - SPARQL variables MUST be accessed via property access on the iteration variable
   - The `?variable` syntax is no longer supported in comprehensions
   - Example: `?age` becomes `x.age` where `x` is the iteration variable

## Before → After

### Basic comprehension (non-SPARQL)

Before:

`[x in xs # x > 0]`

After:

`[x from x in xs where x > 0]`

### SPARQL comprehension with object projection

Before (this syntax never worked correctly):

```fifth
[new Person(){Id = ?p, Age = ?age, Name = ?name}
 from r <- g
 where ?age < 21, ?age > 12]
```

After (new required syntax):

```fifth
[new Person(){Id = x.p, Age = x.age, Name = x.name}
 from x in r <- g
 where x.age < 21, x.age > 12]
```

### Multiple constraints

After (comma-separated constraints, AND-ed):

`[x from x in xs where x > 0, x < 10]`

## Key Migration Steps

1. **Add iteration variable**: Add explicit iteration variable to `from` clause (e.g., `from x in`)
2. **Update SPARQL variable references**: Replace all `?variable` with `x.variable` (property access on iteration variable)
3. **Remove `?` prefix**: Property names do not include `?` (e.g., `?age` → `x.age`, not `x.?age`)
4. **Update constraints**: Use iteration variable in constraints (`x.age > 18` not `?age > 18`)

## Why this change

- Makes comprehension clauses consistent (`from` / `where`).
- Provides clear scoping for SPARQL result row access via iteration variable.
- Property access syntax (`x.age`) is more intuitive than SPARQL variable syntax (`?age`) in Fifth code.
- The iteration variable `x` represents each result row, making the semantics explicit.
- Enables SPARQL comprehensions while keeping one comprehension surface.

## SemVer bump decision

This change is breaking for any code using `in`/`#` list comprehensions or `?variable` SPARQL variable references.

**Decision**: MINOR bump (pre-1.0 project, syntax evolution expected)

**Rationale**:

Per Constitution XI and `docs/Development/release-process.md`:
- Fifth is currently pre-1.0 (active development phase)
- Breaking changes in pre-1.0 projects are acceptable as MINOR bumps
- The new syntax is more explicit and enables future SPARQL comprehensions
- Migration path is clear and well-documented
- If Fifth were post-1.0, this would require a MAJOR bump

**Release workflow**: The version is inferred from the pushed tag (see release process docs). When tagging the next release that includes this feature, use a MINOR version bump (e.g., 0.8.0 → 0.9.0).
