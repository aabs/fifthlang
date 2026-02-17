# Quickstart: Namespace Import Directives

## Problem Statement
Projects now rely on multiple modules contributing to shared namespaces, but the compiler only honors single-file declarations. Import directives in source files are ignored, causing symbol resolution failures and confusing diagnostics.

## Steps To Reproduce
1. Create two files:
   - `math.5th`
     ```fifth
     namespace Utilities.Math;
     export add(int a, int b): int => a + b;
     ```
   - `consumer.5th`
     ```fifth
     namespace App.Core;
     import Utilities.Math;

     main(): int {
         return add(2, 3);
     }
     ```
2. Compile the project with both files included.

## Expected Result
- `consumer.5th` should resolve `add` via the namespace import and compile successfully.
- Diagnostics should surface only if `Utilities.Math` is undeclared or multiple modules declare conflicting symbols.

## Actual Result
- The compiler reports `add` as undefined because imports are ignored and namespaces are not aggregated across modules.

## Quick Verification After Fix
Run the minimal scenario above and ensure:
- No diagnostics are produced.
- The generated program executes with exit code `5` when run via the runtime tests (`dotnet test test/runtime-integration-tests --filter FullyQualifiedName~NamespaceImport_Smoke`).
