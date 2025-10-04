# IL Generation Gap Analysis
**Date:** 3 October 2025  
**Comparing:** `ILEmissionVisitor.cs` vs `AstToIlTransformationVisitor.cs`

## Executive Summary

The new `AstToIlTransformationVisitor.cs` has successfully implemented basic IL instruction generation but has **critical gaps** that cause widespread test failures (149 of 192 tests failing with exit code 134 - InvalidProgramException).

## Critical Issues Identified

### 1. **Function Call Resolution (HIGH PRIORITY)**
**Location:** `AstToIlTransformationVisitor.cs` lines 727-737

**Problem:**
```csharp
case ast.FuncCallExp funcCall:
    // ... generate args ...
    // ❌ HARDCODED to Console.WriteLine!
    sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)") { ArgCount = argCountForFunc });
```

**What's needed:**
- Resolve actual Fifth function being called
- Check if it's an external call (needs `extcall:` format)
- Check if it's an internal Fifth function (needs proper method signature)
- Handle instance vs static methods
- Proper argument count validation

**Reference implementation exists:** `ILEmissionVisitor.EmitCallInstruction` (lines 348-367) shows proper external call handling with `ParseExtCallSignature`.

---

### 2. **Arithmetic Instruction Mapping (HIGH PRIORITY)**
**Location:** `AstToIlTransformationVisitor.cs` lines 615-640

**Problem - Incomplete operator mapping:**
```csharp
private string GetUnaryOpCode(string op)
{
    return op switch
    {
        "-" => "neg",
        "!" => "ldc.i4.0",  // ❌ WRONG - should be "not" or special handling
        _ => "nop",         // ❌ Falls back to nop!
    };
}
```

**ILEmissionVisitor has correct implementation** (lines 330-347):
```csharp
case "not": // Logical not
    EmitLine("ldc.i4.0");
    EmitLine("ceq");
    break;
case "cle": // <=
    EmitLine("cgt");
    EmitLine("ldc.i4.0");
    EmitLine("ceq");
    break;
case "cge": // >=
    EmitLine("clt");
    EmitLine("ldc.i4.0");
    EmitLine("ceq");
    break;
```

**What's needed:**
- Fix `GetBinaryOpCode` for `<=` and `>=` (currently returns wrong opcodes)
- Fix `GetUnaryOpCode` for `!` (logical not)
- Handle `!=` properly (needs `ceq` followed by negation)
- These compound operations need multi-instruction sequences

---

### 3. **Variable Reference Lowering Bug (CRITICAL)**
**Location:** `AstToIlTransformationVisitor.cs` lines 710-717

**Problem - Stack underflow in assignments:**

Test case: `count = count + 1;`

**Expected IL:**
```il
ldloc count    // Load current value
ldc.i4 1       // Push 1
add            // Add them
stloc count    // Store result
```

**Actual IL generated:**
```il
ldc.i4 0       // ❌ WRONG VALUE and missing ldloc!
add            // ❌ Stack underflow - only one value pushed!
stloc count
```

**Root cause:** The `VarRefExp` case (lines 710-717) appears correct, but the issue is likely:
- Variables aren't being tracked in `_currentParameterNames` or `_localVariableTypes`
- Variable initialization in `GenerateStatement` isn't populating these dictionaries correctly
- The assignment RHS lowering is somehow skipping the VarRef

**Evidence from error logs:**
```
PEEmitter: Stack underflow in method 'main' at statement #2. cumulative=0, delta=-1.
Sequence: ... LoadInstruction { Opcode = ldc.i4, Value = 0 }; ArithmeticInstruction { Opcode = add }; ...
```

---

### 4. **Missing Expression Types**
**Location:** `AstToIlTransformationVisitor.cs` GenerateExpression switch

**Not handled:**
- `MemberAccessExp` - only partially handled in type inference
- `ListLiteralExp` - arrays/lists
- `NewObjectExp` - object construction
- `IndexerExp` - array indexing
- `TernaryExp` - conditional expressions
- `LambdaExp` - lambda expressions
- `GraphAssertionBlockExp` - already has type-form lowering but not expression-form

---

### 5. **Missing Statement Types**
**Location:** `AstToIlTransformationVisitor.cs` lines 768-824

**Currently handles:**
- ✅ VarDeclStatement
- ✅ ExpStatement  
- ✅ AssignmentStatement
- ✅ ReturnStatement
- ✅ IfElseStatement (just added)
- ✅ WhileStatement (just added)

**Not handled:**
- ❌ ForStatement
- ❌ ForEachStatement
- ❌ SwitchStatement
- ❌ TryStatement / CatchStatement
- ❌ ThrowStatement
- ❌ BreakStatement / ContinueStatement
- ❌ BlockStatement

---

## Architecture Differences

### Old Approach (ILEmissionVisitor.cs)
- **Single-pass emission:** Directly writes IL text to StringBuilder
- **Direct control:** Complete control over IL syntax
- **Type inference:** Infers local variable types from preceding call instructions (lines 177-191)
- **Local mapping:** Maps Fifth variable names to IL locals `V_0`, `V_1`, etc. (lines 204-216)

### New Approach (AstToIlTransformationVisitor.cs)
- **Two-stage:** Generate `InstructionSequence` → Emit IL via `ILEmissionVisitor`
- **Abstraction:** Uses typed instruction objects (`LoadInstruction`, `CallInstruction`, etc.)
- **Reusability:** Instruction sequences can be analyzed, optimized, debugged
- **Delegation:** `ILEmissionVisitor` handles final emission from instruction sequences

**Advantage of new approach:** Better separation of concerns, easier to add optimizations and diagnostics.

---

## Recommended Fix Priority

### Phase 1: Make Simple Tests Pass (Immediate)
1. **Fix variable tracking in assignments**
   - Debug why `count = count + 1` generates wrong IL
   - Ensure `_localVariableTypes` is populated during `VarDeclStatement`
   - Trace through `BinaryExp` lowering to find where VarRef gets lost

2. **Fix arithmetic operator mapping**
   - Implement `ceq_neg` for `!=` (emit `ceq` + `ldc.i4.0` + `ceq`)
   - Implement `not` for `!` (emit `ldc.i4.0` + `ceq`)
   - Implement `cle` for `<=` (emit `cgt` + `ldc.i4.0` + `ceq`)
   - Implement `cge` for `>=` (emit `clt` + `ldc.i4.0` + `ceq`)

**Target:** Make tests with only arithmetic and return pass (e.g., `return 5 + 3;`)

### Phase 2: Fix Function Calls (High Priority)
3. **Implement proper function call resolution**
   - Check if call target is Fifth function vs external
   - For Fifth functions: build internal call signature
   - For external functions: use existing `extcall:` format
   - Handle instance methods properly
   - Validate argument count matches parameters

**Target:** Make `FunctionRuntimeTests.SimpleFunctionCall` pass

### Phase 3: Fix Remaining Expression Types (Medium Priority)
4. **Add missing expression lowering**
   - MemberAccessExp (method calls on objects)
   - ListLiteralExp (array creation)
   - IndexerExp (array access)
   - NewObjectExp (object construction)

**Target:** Make comprehensive syntax tests pass

### Phase 4: Add Missing Statement Types (Lower Priority)
5. **Add remaining control flow**
   - ForStatement, ForEachStatement
   - SwitchStatement
   - Try/Catch/Finally
   - Break/Continue

**Target:** Full language support

---

## Specific Code Fixes Needed

### Fix 1: Arithmetic Instructions
**File:** `src/code_generator/AstToIlTransformationVisitor.cs`

Change `GetBinaryOpCode` and `GetUnaryOpCode` to return special markers that `ILEmissionVisitor.EmitArithmeticInstruction` already knows how to handle:

```csharp
private string GetBinaryOpCode(string op)
{
    return op switch
    {
        "+" => "add",
        "-" => "sub",
        "*" => "mul",
        "/" => "div",
        "==" => "ceq",
        "!=" => "ceq_neg",  // ✅ Special handling in ILEmissionVisitor
        "<" => "clt",
        ">" => "cgt",
        "<=" => "cle",      // ✅ Special handling in ILEmissionVisitor
        ">=" => "cge",      // ✅ Special handling in ILEmissionVisitor
        _ => "add",
    };
}

private string GetUnaryOpCode(string op)
{
    return op switch
    {
        "-" => "neg",
        "!" => "not",       // ✅ Special handling in ILEmissionVisitor
        _ => "nop",
    };
}
```

**Good news:** `ILEmissionVisitor.EmitArithmeticInstruction` already handles these special cases correctly!

### Fix 2: Function Call Resolution
**File:** `src/code_generator/AstToIlTransformationVisitor.cs`

Replace the hardcoded Console.WriteLine with proper resolution:

```csharp
case ast.FuncCallExp funcCall:
    // Generate arguments first
    if (funcCall.InvocationArguments != null)
    {
        foreach (var arg in funcCall.InvocationArguments)
        {
            sequence.AddRange(GenerateExpression(arg).Instructions);
        }
    }
    
    var argCount = funcCall.InvocationArguments?.Count ?? 0;
    
    // Check if this is an external call (has ExternalMethodName annotation)
    if (funcCall.Annotations != null &&
        funcCall.Annotations.TryGetValue("ExternalType", out var extTypeObj) && extTypeObj is Type extType &&
        funcCall.Annotations.TryGetValue("ExternalMethodName", out var extMethodObj) && extMethodObj is string extMethod)
    {
        // Build extcall: signature
        var mi = ResolveExternalMethod(extType, extMethod, funcCall.InvocationArguments ?? new List<ast.Expression>());
        if (mi != null)
        {
            var sig = BuildExternalCallSignature(mi);
            sequence.Add(new CallInstruction("call", sig) { ArgCount = argCount });
        }
        else
        {
            // Fallback
            sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)") { ArgCount = argCount });
        }
    }
    // Check if this references a Fifth function
    else if (funcCall.FunctionDef != null)
    {
        // Build internal call signature: "returnType FunctionName(param1Type, param2Type, ...)"
        var fifthSig = BuildFifthFunctionSignature(funcCall.FunctionDef);
        sequence.Add(new CallInstruction("call", fifthSig) { ArgCount = argCount });
    }
    else
    {
        // Unknown - fallback
        sequence.Add(new CallInstruction("call", "void [System.Console]System.Console::WriteLine(object)") { ArgCount = argCount });
    }
    break;
```

---

## Testing Strategy

### Minimal Test Suite (Create these)
1. **test_return_literal.5th**
   ```fifth
   main(): int { return 42; }
   ```

2. **test_arithmetic.5th**
   ```fifth
   main(): int { return 5 + 3; }
   ```

3. **test_variable.5th**
   ```fifth
   main(): int { x: int = 5; return x; }
   ```

4. **test_assignment.5th**
   ```fifth
   main(): int { x: int = 5; x = x + 1; return x; }
   ```

5. **test_function_call.5th**
   ```fifth
   add(a: int, b: int): int { return a + b; }
   main(): int { return add(5, 3); }
   ```

Run these in order and fix issues as they arise.

---

## Debug Approach

### Add Diagnostic Logging
```csharp
public InstructionSequence GenerateExpression(ast.Expression expression)
{
    var sequence = new InstructionSequence();
    
    Console.WriteLine($"[DEBUG] GenerateExpression: {expression.GetType().Name}");
    
    switch (expression)
    {
        case VarRefExp varRef:
            Console.WriteLine($"[DEBUG] VarRef: {varRef.VarName}, IsParam={_currentParameterNames.Contains(varRef.VarName)}, HasLocalType={_localVariableTypes.ContainsKey(varRef.VarName)}");
            // ... rest of case
```

### Examine IL Dumps
The `build_debug_il/` directory contains per-method dumps. Check these to see actual IL sequences generated.

### Stack Simulation
`PEEmitter` already does stack depth tracking. Pay attention to its error messages - they pinpoint exactly where stack underflow/overflow occurs.

---

## Conclusion

The new IL generation infrastructure is architecturally sound but has **implementation gaps** in:

1. **Variable tracking** (causing stack underflow in assignments)
2. **Operator mapping** (incorrect IL for `<=`, `>=`, `!`, `!=`)
3. **Function call resolution** (everything hardcoded to Console.WriteLine)

**Good news:**
- Control flow lowering (if/while) is now in place ✅
- `ILEmissionVisitor` already handles compound operations correctly ✅
- The two-stage architecture is working ✅

**Next steps:**
1. Fix operator mapping (quick win - just update return values)
2. Debug variable tracking in assignments (needs investigation)
3. Implement proper function call resolution (moderate effort)

Once these three are fixed, most tests should start passing.
