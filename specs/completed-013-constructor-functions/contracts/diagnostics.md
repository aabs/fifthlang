# Diagnostics Contract: Constructor Functions

**Codes**: CTOR001–CTOR008

| Code | Trigger | Payload Fields | Hint |
|------|---------|----------------|------|
| CTOR001 | No matching overload | code, class, signatureAttempt, location, argsSummary | Add matching constructor or adjust argument types |
| CTOR002 | Ambiguous overload set | code, class, candidateSignatures[], location, argsSummary | Disambiguate by changing parameter types or removing one overload |
| CTOR003 | Definite assignment failure | code, class, constructorSignature, missingFields[], location | Assign all required fields before constructor end |
| CTOR004 | Missing base call | code, class, baseClass, location | Invoke base constructor via `: base(...)` |
| CTOR005 | Required explicit ctor (cannot synthesize) | code, class, missingFields[], location | Declare constructor assigning all required fields |
| CTOR006 | Duplicate signature | code, class, parameterTypes, location | Remove or modify duplicate constructor signature |
| CTOR007 | Invalid modifier (e.g. async) | code, class, modifier, location | Remove unsupported modifier |
| CTOR008 | Inheritance/base cycle | code, class, cyclePath[], location | Check inheritance chain for circular base requirements |

**Format**: Structured emission (stderr and optional JSON) consistent with existing diagnostic system.

**JSON Schema (Conceptual)**
```json
{
  "code": "CTOR001",
  "severity": "error",
  "message": "No matching constructor for arguments (int, string)",
  "class": "Person",
  "signatureAttempt": "Person(int,string)",
  "location": { "file": "person.5th", "line": 12, "column": 5 },
  "hint": "Add matching constructor or adjust argument types"
}
```

**Determinism Requirements**
- Candidate signatures sorted lexicographically for CTOR002.
- Missing fields sorted by declaration order for CTOR003/CTOR005.
- Cycle path lists inheritance chain order root→leaf.

**Non-Goals**
- Warning-level constructor diagnostics (all are errors initially).
