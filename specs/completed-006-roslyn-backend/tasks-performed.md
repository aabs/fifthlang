# Tasks: Roslyn Backend Migration

Inventory legacy IL patterns (run)
- Status: completed (initial scan)
- Completed: 2025-10-12
- Output: `inventory-il.md` (file created in this spec folder)
- Next actions:
  - Create preservation inventory for reflection-sensitive members
  - Add unit tests for extcall signature resolution
  - Create a small runtime validation test that compiles a simple program using PEEmitter and verifies execution

Spike & POC
- Status: not-started
- Notes: Start with call/newobj/entrypoint POC after minimal unit tests for extcall resolution exist
