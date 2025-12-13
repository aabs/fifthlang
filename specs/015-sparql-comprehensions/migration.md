# Migration Note — List Comprehensions `in/#` → `from/where`

This feature introduces a breaking syntax change for list comprehensions:

- `in` is replaced by `from`
- `#` (legacy “such-that” marker) is replaced by `where`
- The legacy `in` and `#` forms are rejected.

## Before → After

### Basic comprehension

Before:

`[x in xs # x > 0]`

After:

`[x from xs where x > 0]`

### Multiple constraints

After (comma-separated constraints, AND-ed):

`[x from xs where x > 0, x < 10]`

## Why this change

- Makes comprehension clauses consistent (`from` / `where`).
- Enables SPARQL comprehensions while keeping one comprehension surface.

## SemVer bump decision

This change is breaking for any code using `in`/`#` list comprehensions.

Decision: **TBD (minor vs major)**

Rationale (to be filled in when tagging a release):

- Constitution XI requires a minor/major bump as appropriate.
- The release workflow infers the version from the pushed tag (see `docs/Development/release-process.md`).
