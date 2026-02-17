# Inventory: Legacy IL emission

Date: 2025-10-12
Branch: 006-roslyn-backend

Summary
-------
This document records a conservative inventory of code locations, patterns, and tests that perform or depend on direct IL/CIL emission in the current codebase. The inventory was produced by searching the compiler and tests for IL emission-related symbols and artifacts (common tokens: "Emit", "CIL", "System.Reflection.Emit", "extcall:").

Purpose
-------
- Provide a single source of truth of where the legacy backend emits or manipulates IL
- Identify constructs and tests that may require special handling during the Roslyn migration
- Provide action items and preservation candidates for planning and implementation

Methodology
-----------
- Codebase scan (grep) for: System.Reflection.Emit, EmitInstruction, ILEmissionVisitor, PEEmitter, extcall:, InstructionEncoder, .il artifacts under test outputs
- Quick manual review of the matching files to identify responsibilities and IL patterns

Findings (high level)
---------------------

1) Primary IL emission code sites

- src/code_generator/ILEmissionVisitor.cs
  - Emits textual IL (.il) from IL AST. Handles .assembly/.module declarations, .method, .locals, .entrypoint, .maxstack and emits IL instructions such as call, newobj and ret.

- src/code_generator/PEEmitter.cs and split helpers (PEEmitter.CallInstructions.cs, PEEmitter.LoadInstructions.cs, PEEmitter.StoreInstructions.cs, PEEmitter.LoadInstructions.cs)
  - Low-level PE/metadata emission and binary IL generation using MetadataBuilder/InstructionEncoder. Contains signature building, token resolution, and final instruction encoding used by the current runtime artifacts.

- src/code_generator/InstructionEmitter/* (LoadInstructionEmitter.cs, StoreInstructionEmitter.cs, ...)
  - Instruction-to-token resolution logic (ResolveFieldToken, PropagateFieldType, etc.) and helpers invoked during binary emission.

- src/code_generator/Emit/*.cs (ExpressionEmitter.cs, StatementEmitter.cs, ControlFlowEmitter.cs, ConstantEmitter.cs, OperatorMapper.cs)
  - Emit high-level language constructs to IL AST (extcall: signatures, call instruction construction, newobj creation, control flow mapping).

- src/code_generator/AstToIlTransformationVisitor.cs
  - Converts the Fifth language AST into the IL metamodel used by the ILEmissionVisitor / PEEmitter pipeline.

- src/ast-model/ILMetamodel.cs and generated artifacts under src/ast-generated/ (il.builders.generated.cs, il.visitors.generated.cs)
  - The IL AST model that the pipeline consumes and that tests reference when checking IL-transformation behavior.

2) Test artifacts and suites that generate or consume IL

- test/runtime-integration-tests/bin/**/build_debug_il/.il
  - Tests produce textual .il artifacts during test builds; several runtime tests rely on IL generation for execution or assertions.
- test/kg-smoke-tests/bin/**/build_debug_il/.il
  - Smoke tests that output .il for debugging/inspection.
- test runtime and integration tests (BasicRuntimeTests, ControlFlowRuntimeTests, etc.) contain TODOs and skips specifically mentioning IL-generation limitations; these point to behavioral and execution assumptions in tests.

3) Package and project dependencies

- The code_generator and compiler projects reference System.Reflection.Emit (package/version used in project assets and packages.lock). Example: code_generator.csproj contains a PackageReference to System.Reflection.Emit 4.7.0.

4) Notable IL patterns and constructs observed

- extcall: encoded external method signatures
  - Example pattern: "extcall:Asm=System.Runtime;Ns=System;Type=Decimal;Method=Parse;Params=System.String;Return=System.Decimal"
  - These are parsed and rebuilt into IL call signatures during emission; requires careful translation to Roslyn calls (method binding, overload resolution, calling convention).

- Manual metadata/signature building
  - PEEmitter contains logic to construct calling-convention flags, parameter tokens and member references. This is implementation-specific and may not trivially map to Roslyn AST without reintroducing similar resolution logic.

- Locals / .locals init management and inferred local types
  - The emitter infers local types and emits .locals init declarations. Roslyn will synthesize locals differently; mapping must preserve semantics or be validated via tests.

- Token resolution and field/method token propagation
  - Several emitter helpers look up fields and methods and emit tokens directly; these are sensitive to metadata layout.

- Special-case opcodes and control flow
  - Branching opcodes (br, brtrue, brfalse), newobj, calli-like patterns and custom calling conventions are explicitly handled in the emission pipeline.

Preservation candidates (initial)
--------------------------------

These items should be considered for explicit preservation (meaning we try to produce equivalent IL or provide a documented, tested deviation):

- Entry-point generation (.entrypoint) and public API entry points used by downstream consumers
- Methods and fields referenced by reflection in tests or runtime code (needs detection)
- External call signatures that target specific runtime APIs (e.g., Decimal.Parse, String.Concat) where overload resolution matters
- P/Invoke or interop-style members (if present) — none were concretely detected in this quick scan but should be reviewed

Actionable next steps
---------------------

1. Create a canonical "preservation inventory" file listing reflection-sensitive public members and test harnesses that must be preserved or explicitly approved for change.
2. Convert the extcall: test cases to unit tests that assert runtime behavior (method resolution and behavior) instead of IL byte layout.
3. Add dedicated integration tests that exercise the PEEmitter output for a small set of preservation candidates; where necessary, implement narrow IL-preservation shims.
4. Accept that the majority of emitted IL text/byte pattern differences are allowed under the chosen equivalence policy (Test-suite equivalence), but enforce preservation for inventoried candidates.
5. Start the Roslyn POC targeting a tiny subset of constructs (calls, newobj, entrypoint, return) and validate against the baseline tests referenced above.

Appendix: quick file reference
-----------------------------
- src/code_generator/ILEmissionVisitor.cs
- src/code_generator/PEEmitter*.cs (PEEmitter.cs, PEEmitter.CallInstructions.cs, PEEmitter.LoadInstructions.cs, PEEmitter.StoreInstructions.cs)
- src/code_generator/InstructionEmitter/*.cs
- src/code_generator/Emit/*.cs (ExpressionEmitter.cs, StatementEmitter.cs, ControlFlowEmitter.cs, ConstantEmitter.cs)
- src/code_generator/AstToIlTransformationVisitor.cs
- src/ast-model/ILMetamodel.cs and src/ast-generated/il.*
- test/runtime-integration-tests/**/build_debug_il/.il
- test/kg-smoke-tests/**/build_debug_il/.il

Notes
-----
This is intentionally conservative and not exhaustive; it surfaces the highest-impact locations and patterns discovered by automated text search and quick manual review. The next inventory step is to run automated analysis to detect reflection/interop uses and to enumerate exact preservation candidates.

