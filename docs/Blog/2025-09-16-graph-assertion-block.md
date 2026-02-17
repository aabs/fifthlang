---
title: Announcing Graph Assertion Blocks in Fifth
summary: Graph Assertion Blocks (<{ ... }>) are a new Fifth language construct that lets ordinary code produce knowledge graph facts automatically, with support for both statement-form (auto-persist to default store) and expression-form (explicit control over when and where to persist).
authors:
    - Andrew Matthews
date: 2025-06-16
---

# Announcing Graph Assertion Blocks in Fifth

Today we’re excited to announce Graph Assertion Blocks — a new, first-class construct in Fifth that lets your ordinary code produce knowledge graph facts with zero boilerplate. Wherever you can use a regular block, you can now write a Graph Assertion Block delimited by `<{` and `}>`. While the block runs, mutations to assertable objects are accumulated as assertions; on completion, you either get a graph value you can assign or persist, or, when used as a standalone statement, those assertions are written to the default knowledge graph.

TL;DR
- New syntax: `<{ ... }>` works as both a statement and an expression.
- As a statement: persists to the default graph on successful completion.
- As an expression: yields a graph value; persist it explicitly with `store += graphValue` or assign it to a graph variable.
- Nested blocks, deterministic scoping, set semantics, and clear exception behavior.

# **UPDATE**:

GABs have been discontinued.  Felt like a bad idea compared to providing a set of RDF and SPARQL literals and operators.

## What’s New

- Graph Assertion Block construct: `<{ ... }>`
  - Behaves like a normal block for variables, control flow, and object state.
  - Additionally records assertions from mutations to assertable objects executed within the block.
- Dual role (statement + expression)
  - Statement-form: commits to the default graph if the block completes without an unhandled exception.
  - Expression-form: produces a graph value; nothing is persisted until you do it explicitly.
- Explicit graph targeting
  - Assign the result to a graph l-value (in-memory) or persist to a store via `storeVar += graphValue`.
  - Named graph scoping via `in <iri-or-alias>` is supported on graph variables.
- Nesting and composition
  - Inner blocks merge their assertions into the enclosing block’s assertion set; commit happens at the outer boundary unless you explicitly persist earlier.
- Set semantics + open world
  - Identical triples deduplicate; multiple distinct values for the same predicate are allowed by design.
- Robust error model
  - On an unhandled exception: transactional l-value commits do not happen. Any explicit persist already performed to the default store isn’t rolled back.
  - Missing default graph/store yields a clear ‘Unknown Default Graph or Store’ error.

These semantics align with the Feature Specification “Graph Assertion Block” and are consistent with Fifth’s existing aliasing, scope, and type checking rules.

---

## Syntax Overview

- Statement-form (auto-persist to default graph on success):

```fifth
<{
   // Your code here; assertions come from assertable object mutations
}>;
```

- Expression-form (produce a graph value; no auto-persist):

```fifth
- g: graph = <{ /* compute facts */ }>;  // in-memory graph value
store default = sparql_store(<http://example.org/store>);
default += g;                         // explicit persist
```

- Graph variable with named graph scope:

```fifth
alias x as <http://example.org/people#>;

store default = sparql_store(<http://example.org/store>);

ericKnowledge : graph in <x:people> = <{
   d: datetime = new datetime(1926, 5, 14);
   eric.dob = d;
   eric.age = calculate_age(d);
}>;

default += ericKnowledge;  // persist to the store; scoped to <x:people>
```

- Inline expression usage:

```fifth
store default = sparql_store(<http://example.org/store>);

default += <{
   eric.age = 99;
}>;
```

- Direct triple assertions (when you want to assert triples as statements):

```fifth
store default = sparql_store(<http://example.org/store>);

main(): int {
   <{
      <http://example.org/s> <http://example.org/p> 42;
   }>;  // persists to default on success
   return 0;
}
```

Notes
- “Assertable objects” are domain/runtime objects whose mutations translate to graph assertions.
- Expression-form always yields a graph value. Assign it, transform it, or persist it explicitly.
- Statement-form targets the default graph. If no default store/graph is configured, you’ll get ‘Unknown Default Graph or Store’.

---

## Semantics at a Glance

- Execution model
  - Runs like a regular block. Only statements actually executed contribute assertions.
  - Works in loops/conditionals — only the executed paths assert.
- Persistence model
  - Statement-form: auto-persist to default on success.
  - Expression-form: no auto-persist; persist with `storeVar += graphValue`.
  - Assignment to a graph variable is transactional at the commit boundary; explicit store writes are atomic at the store level per SPARQL 1.2 semantics (no rollback for partial success unless the store supports it; currently not supported).
- Nesting
  - Inner assertions merge into the enclosing block’s assertion set; the outer boundary determines commit unless you explicitly persist earlier.
- Exceptions
  - Unhandled exception: do not commit transactional l-values; explicit default store writes already performed aren’t rolled back.
- Set semantics
  - Identical triples deduplicate; contradictory facts are allowed (open world). No last-write-wins unless functional properties are later introduced.
- Aliases
  - Prefix/alias resolution inside the block follows the normal Fifth rules and the in-scope declarations.

---

## Current Compiler Status

- Parser + AST
  - Grammar recognizes Graph Assertion Blocks `<{ ... }>` in statement and expression contexts.
  - AST models blocks uniformly; graph-producing semantics are represented explicitly in the tree.
- Code generation & lowering
  - Language transformations lower assertion blocks to a consistent internal form so all syntactic variations map to the same semantics.
- Type checking
  - Type rules come from Fifth (not KG ontology). If your program type-checks, its assertions are permissible.
- Persistence
  - Default and explicit store operations supported via `storeVar += graphValue`.
  - Error surfaced if default store/graph isn’t declared/connected.
- Testing
  - Smoke tests cover statement- and expression-forms, nested blocks, empty blocks, and persistence failure paths.

Expected build warnings
- ANTLR grammar and C# nullable warnings may appear; these are expected and safe to ignore as noted in project docs.

---

## Try It Locally

Prereqs
- .NET 8.0 SDK (global.json pins 8.0.118)
- Java 17+ (for ANTLR)

Build + test (fish shell)

```fish
# Verify prerequisites
 dotnet --version
 java -version

# Restore and build (first run can take ~1–2 minutes)
 dotnet restore fifthlang.sln
 dotnet build fifthlang.sln

# Run focused tests (optional)
 dotnet test test/ast-tests/ast_tests.csproj
 # or try KG smoke tests if available
 dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter FullyQualifiedName~GraphAssertionBlock_
```

Minimal sample

```fifth
alias x as <http://example.org/people#>;
store default = sparql_store(<http://example.org/store>);

main(): int {
   ericKnowledge : graph in <x:people> = <{
      d: datetime = new datetime(1926, 5, 14);
      eric.dob = d;
      eric.age = calculate_age(d);
   }>;
   default += ericKnowledge;
   return 0;
}
```

---

## Design Principles Reflected

- Code as facts: write ordinary imperative code; get graph assertions for free.
- Explicit persistence: expression-form never auto-writes; you stay in control.
- Predictable scope: only executed statements assert; nesting composes naturally.
- Open world: multiple values are allowed; identical triples deduplicate.

---

## What’s Next

- Ergonomics: additional sugar for common assertion patterns.
- Tooling: richer diagnostics and editor hints for assertion-producing code.
- Performance: further tuning for large assertion sets and batch persistence.

If you build something with Graph Assertion Blocks, we’d love to hear about it. Share examples, questions, and feedback in the GitHub Discussions — it helps us refine the feature and prioritize what comes next.

— The Fifth Language Team
