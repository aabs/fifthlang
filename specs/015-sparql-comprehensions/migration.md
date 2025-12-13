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

**Decision**: MINOR bump (pre-1.0 project, syntax evolution expected)

**Rationale**:

Per Constitution XI and `docs/Development/release-process.md`:
- Fifth is currently pre-1.0 (active development phase)
- Breaking changes in pre-1.0 projects are acceptable as MINOR bumps
- The new syntax is more explicit and enables future SPARQL comprehensions
- Migration path is clear and well-documented
- If Fifth were post-1.0, this would require a MAJOR bump

**Release workflow**: The version is inferred from the pushed tag (see release process docs). When tagging the next release that includes this feature, use a MINOR version bump (e.g., 0.8.0 → 0.9.0).
