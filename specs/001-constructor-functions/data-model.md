# Data Model: Constructor Functions

**Purpose**: Define entities, attributes, and relationships introduced or affected by constructor function support.

## Entities

### ConstructorDef
| Field | Type | Description |
|-------|------|-------------|
| Name | Identifier | Must equal enclosing ClassDef.Name |
| Parameters | List<Parameter> | Ordered list; types may reference class type params |
| Body | BlockStatement? | Optional; if absent and all assignments satisfied via defaults/base, still valid |
| BaseCall | BaseConstructorCall? | Optional explicit base invocation |
| Synthesized | bool | True if generated implicitly (no user constructors declared & conditions met) |
| SourceLocation | Location | File/line/column for diagnostics |

### BaseConstructorCall
| Field | Type | Description |
|-------|------|-------------|
| Arguments | List<Expression> | Arguments passed to base constructor |
| ResolvedTarget | ConstructorDef? | Linked during semantic analysis |

### InstantiationExpression
| Field | Type | Description |
|-------|------|-------------|
| TargetType | FifthType | Concrete class type (with generic args bound) |
| Arguments | List<Expression> | Arguments for constructor |
| ResolvedConstructor | ConstructorDef? | Set after overload resolution |

### RequiredFieldSet (Analysis Artifact)
| Field | Type | Description |
|-------|------|-------------|
| Fields | HashSet<FieldSymbol> | Non-nullable & no-default fields requiring assignment |
| Assigned | HashSet<FieldSymbol> | Fields proven definitely assigned along all paths |

### ClassDef (Modified)
Adds:
- Constructors: List<ConstructorDef>

### Parameter (Existing)
Unchanged semantics; may shadow field names.

## Relationships
- ClassDef 1..* → ConstructorDef (composition)
- ConstructorDef 0..1 → BaseConstructorCall
- InstantiationExpression *..1 → ConstructorDef (after resolution)
- ConstructorDef uses FieldSymbols to enforce assignment (through RequiredFieldSet).

## Validation Rules
1. ConstructorDef.Name == ClassDef.Name.
2. No return type allowed; presence triggers parse error.
3. No independent type parameters; parameter types may reference class generics.
4. Overload uniqueness by ordered parameter types (ignores parameter names).
5. BaseConstructorCall required if base has no parameterless constructor.
6. Definite assignment: Assigned == RequiredFieldSet.Fields at end of body across all control flow paths.
7. Synthesized constructor created only if zero constructors declared AND RequiredFieldSet.Fields is empty after considering defaults.
8. InstantiationExpression must resolve to exactly one ConstructorDef or produce CTOR001/CTOR002.
9. Cyclic base invocation detection via ancestry traversal (ClassDef inheritance chain).

## State Transitions (Creation Flow)
1. Parse: ConstructorDef parsed where method name == class name & no return type.
2. AST Linking: Constructors attached to ClassDef.
3. Synthesis: If none declared → create Synthesized ConstructorDef.
4. Overload Resolution: InstantiationExpression resolves to ConstructorDef.
5. Assignment Analysis: Required fields computed; body analyzed → diagnostics if incomplete.
6. Lowering: InstantiationExpression lowered into allocate-init sequence referencing ConstructorDef.

## Derived Data
- ParameterSignatureHash: stable hash of ordered parameter types used for overload caching.
- ConstructorResolutionCache: Maps (ClassTypeId, ParameterSignatureHash) → ConstructorDef.

## Invariants
- Synthesized constructors never contain BaseCall.
- BaseCall cannot appear on synthesized constructors.
- ResolvedConstructor non-null required before lowering proceeds.
- RequiredFieldSet.Fields empty implies no CTOR003 for that constructor.

## Open Extensions (Future)
- Potential ThisConstructorCall entity if supporting intra-class chaining.
- Attributes for default parameter values (deferred scope).

## Diagram (Simplified)
```
ClassDef
  ├─ Fields
  └─ Constructors ──> ConstructorDef ──(optional)──> BaseConstructorCall
                                │
                                └─ Body

InstantiationExpression ──(resolution)──> ConstructorDef
RequiredFieldSet ↔ ConstructorDef (analysis)
```
