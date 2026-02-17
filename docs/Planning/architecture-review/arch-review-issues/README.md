# Architectural Review Issues

This directory contains issue templates for the major architectural findings from the 2025 architectural review.

## Issue Templates

| Issue | Title | Severity | Priority | Effort |
|-------|-------|----------|----------|--------|
| [ISSUE-001](ISSUE-001-error-recovery.md) | Parser Needs Error Recovery for IDE Support | CRITICAL | P0 | 8 weeks |
| [ISSUE-002](ISSUE-002-lsp-implementation.md) | Implement Language Server Protocol (LSP) | CRITICAL | P0 | 20 weeks |
| [ISSUE-003](ISSUE-003-incremental-compilation.md) | Implement Incremental Compilation | CRITICAL | P0 | 20 weeks |
| [ISSUE-004](ISSUE-004-diagnostic-system.md) | Redesign Diagnostic System | HIGH | P1 | 8 weeks |
| [ISSUE-005](ISSUE-005-composable-pipeline.md) | Refactor to Composable Pipeline | HIGH | P1 | 10 weeks |
| [ISSUE-006](ISSUE-006-symbol-table.md) | Enhance Symbol Table Architecture | MEDIUM | P2 | 8 weeks |
| [ISSUE-007](ISSUE-007-testing-architecture.md) | Restructure Testing Architecture | MEDIUM | P2 | 10 weeks |

## Creating GitHub Issues

Since GitHub CLI or API tools are not available in this environment, these templates need to be converted to GitHub issues manually. Here's how:

### Option 1: GitHub Web UI

For each issue template:

1. Go to https://github.com/aabs/fifthlang/issues/new
2. Copy the issue title (after the `#` on line 1)
3. Copy the content starting from "**Labels:**" section
4. Add labels as specified in the template
5. Create the issue

### Option 2: GitHub CLI (if available)

```bash
# Install GitHub CLI if not available
# See: https://cli.github.com/

# Create issues from templates
gh issue create \
  --repo aabs/fifthlang \
  --title "Parser Needs Error Recovery for IDE Support" \
  --body-file ISSUE-001-error-recovery.md \
  --label "arch-review,parser,ide-support,critical"

# Repeat for each issue template
```

### Option 3: Automated Script

```bash
#!/bin/bash
# create-arch-review-issues.sh

REPO="aabs/fifthlang"

# Issue 001
gh issue create --repo $REPO \
  --title "Parser Needs Error Recovery for IDE Support" \
  --body-file ISSUE-001-error-recovery.md \
  --label "arch-review,parser,ide-support,critical"

# Issue 002
gh issue create --repo $REPO \
  --title "Implement Language Server Protocol (LSP) for IDE Integration" \
  --body-file ISSUE-002-lsp-implementation.md \
  --label "arch-review,ide-support,lsp,critical"

# Issue 003
gh issue create --repo $REPO \
  --title "Implement Incremental Compilation for Performance and IDE Support" \
  --body-file ISSUE-003-incremental-compilation.md \
  --label "arch-review,performance,ide-support,critical"

# Issue 004
gh issue create --repo $REPO \
  --title "Redesign Diagnostic System for Quality Error Messages and IDE Support" \
  --body-file ISSUE-004-diagnostic-system.md \
  --label "arch-review,diagnostics,developer-experience,high"

# Issue 005
gh issue create --repo $REPO \
  --title "Refactor Transformation Pipeline to Composable Architecture" \
  --body-file ISSUE-005-composable-pipeline.md \
  --label "arch-review,maintainability,performance,high"

# Issue 006
gh issue create --repo $REPO \
  --title "Enhance Symbol Table for Performance and IDE Features" \
  --body-file ISSUE-006-symbol-table.md \
  --label "arch-review,symbol-table,performance,medium"

# Issue 007
gh issue create --repo $REPO \
  --title "Restructure Testing Architecture for Better Coverage and Maintainability" \
  --body-file ISSUE-007-testing-architecture.md \
  --label "arch-review,testing,quality,medium"
```

## Labels Required

Ensure these labels exist in the repository:

- `arch-review` (main label for all architectural review issues)
- `parser`
- `ide-support`
- `lsp`
- `performance`
- `diagnostics`
- `developer-experience`
- `maintainability`
- `symbol-table`
- `testing`
- `quality`
- `critical`
- `high`
- `medium`

## Implementation Timeline

Based on the architectural review recommendations:

### Q1 2026 (Weeks 1-13)
- **ISSUE-001:** Error Recovery (Weeks 1-8)
- **ISSUE-004:** Diagnostic System (Weeks 1-8)
- **ISSUE-007:** Testing Architecture (Weeks 1-10, ongoing)

### Q2 2026 (Weeks 14-39)
- **ISSUE-002:** LSP Implementation (Weeks 14-33)
- **ISSUE-003:** Incremental Compilation (Weeks 14-33)
- **ISSUE-005:** Composable Pipeline (Weeks 14-23)
- **ISSUE-006:** Symbol Table Enhancement (Weeks 24-31)

## Dependencies

```
ISSUE-001 (Error Recovery)
    ├─> ISSUE-002 (LSP) [BLOCKS]
    └─> ISSUE-004 (Diagnostics) [ENABLES]

ISSUE-003 (Incremental Compilation)
    └─> ISSUE-002 (LSP) [ENABLES]

ISSUE-005 (Composable Pipeline)
    └─> ISSUE-007 (Testing) [ENABLES]

ISSUE-006 (Symbol Table)
    └─> ISSUE-002 (LSP) [ENABLES]
```

## Priority Rationale

**P0 (Critical Path):**
- Error Recovery: Foundation for all IDE work
- LSP: Critical for adoption and developer experience
- Incremental Compilation: Required for performance at scale

**P1 (High Impact):**
- Diagnostic System: Improves developer experience
- Composable Pipeline: Improves maintainability

**P2 (Important but not blocking):**
- Symbol Table: Performance optimization
- Testing Architecture: Quality improvement

## References

- Full Architectural Review: `../architectural-review-2025.md`
- Implementation Roadmap: See review document Section "Implementation Roadmap"
- Priority Matrix: See review document Section "Recommendations Priority Matrix"

## Notes

These issue templates are comprehensive and include:
- Problem description
- Current state analysis
- Requirements and architecture
- Implementation plans
- Acceptance criteria
- Code examples
- References and dependencies
- Effort estimates

Feel free to adjust based on project priorities and available resources.
