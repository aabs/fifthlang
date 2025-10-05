# Data Model: Namespace Import Directives

## Module
- **Description**: Represents a `.5th` source file participating in the compilation.
- **Attributes**:
  - `Path` (string, absolute path)
  - `DeclaredNamespace` (string|null, matches namespace identifier or `null` for global scope)
  - `Imports` (collection of `ImportDirective`)
  - `Declarations` (collection of `SymbolEntry` defined in the file)
- **Relationships**:
  - Each module owns zero or one file-scoped namespace declaration.
  - A module may import multiple namespaces.
- **State Transitions**:
  1. **Discovered** → located via MSBuild manifest.
  2. **Parsed** → syntax tree built with namespace/import metadata captured.
  3. **Resolved** → merged into namespace scopes and validated for entry-point uniqueness.

## NamespaceScope
- **Description**: Logical aggregation of all symbols sharing the same namespace name across modules.
- **Attributes**:
  - `Name` (string, empty for global namespace)
  - `Modules` (collection of `Module` contributing declarations)
  - `Symbols` (collection of `SymbolEntry` merged across modules)
- **Relationships**:
  - Many-to-many with `Module` (a namespace spans modules; a module contributes to exactly one namespace scope—the declared namespace or global).
  - Serves as the target for `ImportDirective` resolution.
- **State Transitions**:
  1. **Declared** → created when the first module claims the namespace.
  2. **Aggregated** → symbols from all modules merged; duplicates flagged.
  3. **Imported** → exposed to modules that issue `import` directives.

## ImportDirective
- **Description**: File-scoped instruction that requests visibility of a namespace.
- **Attributes**:
  - `Module` (reference to owning module)
  - `NamespaceName` (string)
  - `ResolvedScope` (reference to `NamespaceScope`, nullable until resolved)
  - `IsIdempotent` (boolean flag enforcing deduplication during processing)
- **Relationships**:
  - Belongs to exactly one `Module`.
  - References zero or one `NamespaceScope` (null when undeclared, triggering a warning).
- **State Transitions**:
  1. **Parsed** → namespace name captured from syntax.
  2. **Resolved** → linked to an existing scope or marked undeclared.
  3. **Applied** → its symbols added to module-level lookup tables.

## SymbolEntry
- **Description**: Represents a declared type, function, variable, or other symbol within a namespace.
- **Attributes**:
  - `Name` (string)
  - `QualifiedName` (string, `NamespaceName.Name` or just `Name` for global scope)
  - `DeclarationKind` (enum: Type, Function, Variable, etc.)
  - `DefinitionModule` (reference to `Module` where declared)
- **Relationships**:
  - Owned by a `NamespaceScope`.
  - Referenced by modules importing the namespace during semantic analysis.
- **State Transitions**:
  1. **Declared** → created during AST visitor symbol-table build.
  2. **Aggregated** → deduplicated while merging namespace scopes.
  3. **Shadowed** → flagged when a module declares a local symbol with identical name (local symbol wins according to spec).

## NamespaceImportGraph
- **Description**: Directed graph capturing namespace import relationships to detect cycles and short-circuit duplicate processing.
- **Attributes**:
  - `Nodes` (set of `NamespaceScope` identifiers)
  - `Edges` (map from `NamespaceScope` → imported `NamespaceScope`s)
  - `Visited` (set used during traversal to avoid infinite recursion)
- **Relationships**:
  - Derived from `ImportDirective` relationships across all modules.
- **State Transitions**:
  1. **Built** → edges recorded while resolving imports.
  2. **Validated** → cycles detected but allowed; traversal uses `Visited` to prevent reprocessing.
  3. **Applied** → informs namespace loading order for deterministic symbol availability.
