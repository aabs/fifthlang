# Feature Specification: Namespace Import Directives

**Feature Branch**: `004-namespace-import-directives`  
**Created**: 2025-10-05  
**Status**: Draft  
**Input**: User description outlining namespace declarations, import directives, and associated semantics for Fifth language modules.

## Execution Flow (main)
```
1. Project build resolves the full module set either from the MSBuild configuration or from an explicit list of source files supplied on the CLI.
2. Each module is parsed; an optional top-level namespace declaration is read and registered.
3. Compiler aggregates namespace symbol tables across all modules sharing the same namespace name.
4. For every import directive in a module, the compiler links the referenced namespace symbol table into that module's lexical scope.
5. During semantic analysis, symbol lookups respect file-scoped namespaces first, then imported namespaces, then the global namespace.
6. Build output validates there is exactly one entry-point function across all modules before packaging artifacts.
```

## ⚡ Quick Guidelines
- Emphasize namespace declarations as the primary collision-avoidance mechanism for symbols across modules.
- Preserve backward compatibility for projects without explicit namespace declarations by treating them as members of the global namespace.
- Ensure import directives act only within the declaring module and remain idempotent when repeated.
- Allow import cycles while preventing repeated work through short-circuiting of already processed namespaces.
- Keep namespace and import configuration discoverable through existing project structure to avoid separate configuration files.
- Support two entry modes: MSBuild-provided module manifests and direct CLI enumeration of multiple source files, keeping resolution semantics identical in both cases.

## Clarifications

### Session 2025-10-05
- Q: When a module imports a namespace that no module declares, what diagnostic should occur? → A: Emit a warning but continue, treating the namespace as empty.
- Q: When a module defines a symbol that matches one from an imported namespace, how is the conflict handled? → A: The local symbol shadows the imported symbol within that module.
- Q: What compile-time performance target should namespace resolution meet for large projects? → A: Complete namespace resolution within 2 seconds for 100 modules.
- Q: Do namespace resolution diagnostics need additional detail? → A: Emit structured compiler diagnostics with namespace and module identifiers.
- Q: How should the CLI enumerate modules when no MSBuild manifest is provided? → A: Accept multiple `.5th` file paths on the CLI and treat that list as the module set.

## User Scenarios & Testing *(mandatory)*

### Primary User Story
A project maintainer can organize multiple Fifth source files into namespaces, import the namespaces they need, and rely on the compiler to resolve symbols without naming collisions or manual include chains.

### Acceptance Scenarios
1. **Given** a project with modules `alpha.5th` and `beta.5th` that declare the same namespace, **When** a third module imports that namespace, **Then** all symbols from both modules are available during compilation without qualification conflicts.
2. **Given** a module without a namespace declaration, **When** it imports a named namespace and defines its own symbols, **Then** its symbols remain in the global namespace while imported symbols are available only inside that file.

### Edge Cases
- What happens when two modules in the same namespace define symbols with identical names? The system must flag the duplicate during namespace aggregation.
- How does system handle an import cycle involving three namespaces A → B → C → A? The compiler must terminate import resolution without recursion errors and leave symbol scope consistent.
- How are modules handled when no entry-point function is present across the entire module set? The build should fail with an actionable diagnostic.
- What happens when a module imports a namespace that is not declared by any module? The compiler must emit a warning, treat the namespace as empty, and allow the build to continue.
- What happens when a module defines a symbol with the same name as one from an imported namespace? The module’s local definition must shadow the imported symbol for that file’s compilation scope.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST allow each module to declare at most one optional file-scoped namespace using the `namespace` keyword.
- **FR-002**: System MUST treat all modules lacking a namespace declaration as members of the implicit global namespace with no prefix.
- **FR-003**: System MUST provide an `import` directive that brings the full symbol table of a named namespace into scope for the importing module only.
- **FR-004**: System MUST merge symbols from all modules that declare the same namespace name into a single logical namespace visible to importers.
- **FR-005**: System MUST detect and report duplicate symbol names within the same namespace regardless of defining module.
- **FR-006**: System MUST short-circuit namespace loading to prevent infinite processing when imports form cycles while still allowing compilation to proceed.
- **FR-007**: System MUST enforce that exactly one `main` function exists across the resolved module set and produce an error otherwise.
- **FR-008**: System MUST remove legacy `use` directives and the `module_import` grammar rule from accepted syntax to avoid conflicting semantics.
- **FR-009**: System MUST emit a warning when an imported namespace is undeclared across the module set while treating it as an empty scope so compilation continues.
- **FR-010**: System MUST ensure that locally declared symbols take precedence over imported symbols with the same name within the declaring module’s scope.
- **FR-011**: System MUST produce structured diagnostics for namespace resolution warnings and errors that include the originating module and namespace identifiers to aid troubleshooting.
- **FR-012**: CLI invocation MUST accept multiple `.5th` file paths and resolve namespaces using that enumerated set when no MSBuild manifest is provided, ensuring feature parity with MSBuild-based projects.

### Non-Functional Quality Attributes
- **NFR-001**: Namespace resolution must complete within 2 seconds for projects containing up to 100 modules to keep incremental build latency acceptable for developers.

### Key Entities *(include if feature involves data)*
- **Module**: Represents a `.5th` source file, including its optional namespace declaration, local declarations, and list of import directives.
- **Namespace**: Logical scope aggregating symbols from one or more modules that share the same namespace name; maintains the unioned symbol table and duplication diagnostics.
- **Import Directive**: File-scoped instruction referencing a namespace name whose symbols become available within the module during compilation.

## Symbol Visibility (clarification)

- All module-scope symbols are public by default for this feature.
- Importing a namespace brings the full symbol table of that namespace into the current module’s scope (subject to shadowing in FR-010).
- If `export` appears in examples or tests, treat it as optional/no-op under current semantics; a future feature may introduce explicit export controls.

## Diagnostic Codes (namespace imports)

Prefix: WNS (Namespace warnings/errors)

- WNS0001 (Warning): Undeclared namespace import
	- Condition: Emitted when a module imports a namespace that is not declared by any module in the resolved module set.
	- Required fields: code=WNS0001, severity=Warning, file=<module path>, namespace=<imported name>, message="Import targets undeclared namespace: '<namespace>'"
	- Notes: Compilation continues; the undeclared namespace is treated as empty (see FR-009).

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous  
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Execution Status
*Updated by main() during processing*

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [x] Review checklist passed

