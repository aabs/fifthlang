# Contract: Namespace Import Resolution

## Purpose
Define the responsibilities and observable behaviors for resolving namespace import directives across modules.

## Provider Responsibilities
- Parser emits `ImportDirective` nodes for each `import <namespace>;` statement and records them in the module AST.
- Symbol loader aggregates modules by declared namespace and exposes a lookup API `ResolveNamespace(string name)` returning a `NamespaceScope` or `null`.
- Import resolution component walks each module's directives, binds them to scopes, and populates the module symbol table.
- Diagnostic emitter issues warning `WNS0001` when an import references a namespace with no declaring modules and includes module path + line span metadata.
- Resolution cache guarantees idempotent processing when multiple modules import the same namespace.

## Consumer Expectations
- Compiler phases querying the module symbol table see imported symbols as if declared locally, but local declarations always shadow imported ones.
- Consumers can inspect `ImportDirective.ResolvedScope` to determine availability; `null` indicates the warning scenario.
- Downstream transformations can enumerate contributing modules through `NamespaceScope.Modules` for code generation ordering.

## Preconditions
- All module paths provided by MSBuild manifest are available and parse without syntax errors.
- Namespace names follow the validated identifier grammar.

## Postconditions
- Module symbol tables include all symbols from resolved imports.
- Diagnostics are recorded for missing namespaces without halting compilation.
- Import cycle detection prevents infinite recursion.
