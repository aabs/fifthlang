# Quickstart: Guard Validation Phase

This guide shows how the Guarded Overload Completeness Validation phase behaves using minimal Fifth programs and the resulting diagnostics.

## 1. Base + Guards (Valid)
```
func compute(int x) => 0;           // base (index 0)
func compute(int x) when x == 5 => 5; // unreachable (later guard) but allowed (will warn if subsumed logic activated)
```
Outcome: No completeness error. (If duplicate or subsumption detected in future implementation, W1002 emitted.)

## 2. Missing Base (Incomplete)
```
func f(bool b) when b == true => 1;
```
Diagnostic:
E1001 GUARD_INCOMPLETE: Function 'f/1' has guarded overloads but no base case and guards are not exhaustive.

## 3. Boolean Exhaustive Guards (No Base Needed)
```
func flag(bool b) when b == true => 1;
func flag(bool b) when b == false => 0;
```
Outcome: No E1001 (guards collectively exhaustive for boolean domain).

## 4. Multiple Base Overloads
```
func g(int x) => 0;      // base
func g(int x) => 1;      // second base (error)
```
Diagnostic:
E1005 GUARD_MULTIPLE_BASE (primary on second base) + note referencing first.

## 5. Base Not Last
```
func h(int x) => 0;            // base
func h(int x) when x == 1 => 1; // invalid after base
```
Diagnostic:
E1004 GUARD_BASE_NOT_LAST on second overload.

## 6. UNKNOWN Explosion
(8 overloads, 5 UNKNOWN => 62%)
```
func u(int x) when complex(x) => 1; // unknown (call)
func u(int x) when other(x) => 2;   // unknown
func u(int x) when x < 0 => -1;     // analyzable
func u(int x) when another(x) => 3; // unknown
func u(int x) when x > 0 => 1;      // analyzable
func u(int x) when different(x) => 4; // unknown
func u(int x) when yet(x) => 5;     // unknown
func u(int x) when x == 0 => 0;     // analyzable
```
Diagnostics:
E1001 (incomplete) + W1102 GUARD_UNKNOWN_EXPLOSION (unknown=62%).

## 7. Overload Count Warning
(33 overloads triggers W1101 once.)

## 8. Unreachable Guard
```
func k(int x) when x > -100 && x < 100 => 0;
func k(int x) when x > 0 && x < 10 => 1; // unreachable (subsumed)
```
Diagnostic: W1002 GUARD_UNREACHABLE on second overload.

## 9. Duplicate Guard
```
func d(int x) when x == 5 => 1;
func d(int x) when x == 5 => 2; // duplicate
```
Diagnostic: W1002 GUARD_UNREACHABLE (duplicate).

## 10. Tautology Base
```
func t(int x) when true => 1; // base-like
```
Outcome: Completeness satisfied.

## Invocation Flow (Compiler)
1. Parse & build AST
2. Symbol/type binding
3. GuardValidationPhase executes
4. Diagnostics collected
5. Lowering and codegen proceed if no blocking errors.

Use env var for instrumentation:
```
FIFTH_GUARD_VALIDATION_PROFILE=1 fifthc program.5th
```
Outputs JSON per overload group to stderr.

## Determinism Check (Internal)
Run integration diagnostics twice; expect identical ordering & messages.

## Extending
- New atomic type: extend normalization & formatter.
- New UNKNOWN reason: add enum member + test + spec update.
