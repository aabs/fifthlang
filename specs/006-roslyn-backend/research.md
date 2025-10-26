Title: Roslyn Backend Migration — Research Notes

Date: 2025-10-12

Decisions:
- Equivalence metric: Test-suite equivalence
- Target SDKs: .NET 8 and .NET 10-rc
- Roslyn pinning: Pin for CI/releases, SDK-provided for dev
- PDB fidelity: Full line-and-column SequencePoints
- IL preservation: Inventory and targeted preservation

Next steps: See plan.md and data-model.md for Phase 1 actions.
