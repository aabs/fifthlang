# Guard Validation Module

This module implements comprehensive guard clause completeness validation for the Fifth language compiler.

## Namespace Boundaries

All types in this module use the `compiler.Validation.GuardValidation` namespace with the following sub-namespaces:

- `Infrastructure` - Core data types and interfaces
- `Collection` - Overload collection and grouping
- `Normalization` - Guard predicate normalization and classification  
- `Analysis` - Completeness analysis and coverage detection
- `Diagnostics` - Diagnostic emission and error reporting
- `Instrumentation` - Performance metrics and validation instrumentation

## Layering Contract

Components must respect the following dependency layers (bottom-up):

```
┌─────────────────┐
│  Diagnostics    │ ← Error emission, diagnostic formatting
├─────────────────┤
│  Analysis       │ ← Completeness analysis, coverage detection
├─────────────────┤  
│  Normalization  │ ← Predicate processing, classification
├─────────────────┤
│  Collection     │ ← Overload gathering, grouping
├─────────────────┤
│  Infrastructure │ ← Core types, data structures
└─────────────────┘
```

**Rules:**
- Higher layers may depend on lower layers
- Lower layers may NOT depend on higher layers
- Instrumentation is orthogonal and may be used by any layer
- External dependencies (AST, compiler diagnostics) are available to all layers

## Public Surface

**Only one public type is permitted**: `GuardCompletenessValidator`

All other types must be marked `internal` to maintain encapsulation and prevent coupling to implementation details.

## Memory Policy

The guard validation system is designed for minimal allocation during validation:

- Prefer value types and structs for small data
- Use object pooling for repeated allocations when beneficial
- Avoid LINQ in hot paths to prevent delegate allocations
- Reuse collections where possible via Reset() methods
- Instrument allocation behavior for performance monitoring

## Performance Requirements

- Validation overhead must be ≤5% of compilation time
- Instrumentation captures timing and allocation metrics
- Performance regression tests validate overhead constraints
- Synthetic benchmarks validate worst-case scenarios

## Diagnostic Codes

**Errors:**
- E1001: GUARD_INCOMPLETE - Function overloads don't cover all input space
- E1004: GUARD_BASE_NOT_LAST - Base case not in final position
- E1005: GUARD_MULTIPLE_BASE - Multiple base cases detected

**Warnings:**
- W1002: GUARD_UNREACHABLE - Overload unreachable due to prior coverage
- W1101: GUARD_OVERLOAD_COUNT - Excessive overload count (>32)
- W1102: GUARD_UNKNOWN_EXPLOSION - High percentage of unknown predicates

## Architecture Notes

This module implements a multi-pass validation system:

1. **Collection**: Gather function overloads by name/arity
2. **Normalization**: Extract and classify guard predicates
3. **Analysis**: Detect completeness, subsumption, unreachability
4. **Diagnostics**: Emit structured diagnostic messages
5. **Instrumentation**: Record performance metrics

The implementation prioritizes correctness over performance optimizations, with instrumentation providing visibility into validation costs.