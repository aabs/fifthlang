Title: Compilation error: member access resolves to wrong type in `op_member_access.5th` (test failure)

Summary

- The runtime-integration test `op_member_access_ShouldCompileAndReturnZero` fails during compilation for `TestPrograms/Syntax/op_member_access.5th`.
- The compiler reports that a member access is being performed on a primitive (`Int32`) rather than on the declared class type `Foo`.
- This prevents a simple program that sets and reads an instance field from compiling — expected program exit code is 42.

Reproduction

From the repository root:

```fish
# Build and run the single integration test
dotnet build fifthlang.sln
dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~op_member_access"
# Or run the full test suite used in CI
just test
```

Failing test & program

- Test: `runtime-integration-tests.ComprehensiveSyntaxTests.op_member_access_ShouldCompileAndReturnZero`
- Source file: `test/runtime-integration-tests/TestPrograms/Syntax/op_member_access.5th`

Program contents:

```fifth
class Foo {
    a: int;
}

main(): int {
    f: Foo;
    f.a = 42;
    return f.a;
}
```

Observed behavior

- Test run fails with an InvalidOperationException wrapping a compilation-failed diagnostic.
- Diagnostics show:
  - `Error: Cannot access member on primitive type 'Int32' at :1:22`

Expected behavior

- The program should compile and execute, returning 42.

Root-cause hypothesis

- Member-access resolution or type binding incorrectly resolves the left-hand-side expression (`f`) to a primitive (`Int32`) instead of `Foo`.
- Potential causes:
  - Semantic analysis returns an incorrect inferred type for identifier expressions when the variable is declared but uninitialized.
  - Code generation for member access confuses instance vs static resolution and uses the evaluated runtime type instead of the declared/semantic type.

Files/areas to inspect

- `src/compiler/CodeGeneration/MemberAccessEmitter.cs` — lowering/emission for member-access expressions
- Semantic/type inference passes under `src/compiler/` and `src/ast-model/`
- `test/runtime-integration-tests/ComprehensiveSyntaxTests.cs` — test harness and expected behavior

Suggested fix

- Ensure semantic analysis binds identifier/variable references to their declared symbol/type and propagates that to codegen.
- In `MemberAccessEmitter` (or equivalent codegen), when the LHS is an identifier or variable reference, use the declared/semantic type to determine instance vs static member access.
- Add a clear diagnostic when member access is attempted on non-object types.
- Add unit/integration tests to validate that declared but uninitialized class-local variables still resolve to the declared class type for field access.

Validation steps after fix

1. dotnet build fifthlang.sln
2. dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj --filter "FullyQualifiedName~op_member_access"
3. just test (optional; run full test suite to check for regressions)

Labels

- bug
- compiler
- area-codegen
- needs-investigation
- tests

Assignees

- @aabs

Notes

- A prior attempted fix that special-cased type-qualified member access in `MemberAccessEmitter` was reverted; the log above provides the failing diagnostic to help reproduce.
- If needed I can open a PR with the minimal fix and tests targeting this issue.