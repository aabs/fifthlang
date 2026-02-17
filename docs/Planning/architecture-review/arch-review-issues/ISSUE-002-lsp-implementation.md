# Implement Language Server Protocol (LSP) for IDE Integration

**Labels:** `arch-review`, `ide-support`, `lsp`, `critical`  
**Priority:** P0  
**Severity:** CRITICAL  
**Epic:** Architectural Improvements Q2 2026

## Problem Summary

The compiler has no Language Server Protocol implementation, preventing integration with modern editors (VS Code, Neovim, Emacs, etc.). This severely limits the language's adoption potential and developer experience.

## Current State

- No LSP-related code in codebase
- No language server executable
- Only basic VS Code configuration
- No incremental compilation (required for LSP)
- No real-time diagnostics
- No IDE features (autocomplete, go-to-definition, etc.)

## Impact

### Adoption Barrier
- Developers expect IDE features from modern languages
- Competing languages (Rust, TypeScript, Swift) all have excellent LSP support
- No Fifth language support for popular editors
- Makes language feel "unfinished" or "hobby project"

### Development Velocity
- Contributors cannot efficiently work on Fifth code
- No tooling to support language feature development
- Testing requires full compilation cycles
- Debugging is manual and time-consuming

### Feature Gap
Cannot implement standard IDE features:
- Hover information (type info, documentation)
- Signature help (function parameter hints)
- Code completion (context-aware suggestions)
- Go to definition/implementation
- Find all references
- Rename symbol
- Code actions/quick fixes
- Semantic tokens (syntax highlighting)
- Document/workspace symbols
- Document formatting

## Requirements

### Core LSP Server
1. Implement LSP server as separate executable
2. Support stdio communication protocol
3. Handle standard LSP lifecycle (initialize, initialized, shutdown)
4. Implement core capabilities negotiation

### Document Management
1. Track open documents in workspace
2. Synchronize document changes (didOpen, didChange, didClose)
3. Parse documents incrementally
4. Maintain AST cache per document

### Essential Features (MVP)
1. **Diagnostics** (textDocument/publishDiagnostics)
   - Real-time syntax errors
   - Real-time semantic errors
   - Error recovery support

2. **Hover** (textDocument/hover)
   - Type information
   - Function signatures
   - Symbol documentation

3. **Completion** (textDocument/completion)
   - Context-aware suggestions
   - Keyword completion
   - Symbol completion
   - Function completion with signatures

4. **Go to Definition** (textDocument/definition)
   - Navigate to symbol definition
   - Support cross-file navigation

### Advanced Features (Post-MVP)
1. Find References (textDocument/references)
2. Rename Symbol (textDocument/rename)
3. Document Symbols (textDocument/documentSymbol)
4. Code Actions (textDocument/codeAction)
5. Semantic Tokens (textDocument/semanticTokens)
6. Signature Help (textDocument/signatureHelp)

## Architecture

### Project Structure
```
src/language-server/
├── FifthLanguageServer.csproj
├── Program.cs                    # Entry point
├── LanguageServer.cs             # Main server class
├── Handlers/
│   ├── TextDocumentHandler.cs   # Document sync
│   ├── DiagnosticHandler.cs     # Error checking
│   ├── CompletionHandler.cs     # Code completion
│   ├── HoverHandler.cs          # Hover info
│   └── DefinitionHandler.cs     # Go to definition
├── Services/
│   ├── WorkspaceService.cs      # Workspace management
│   ├── DocumentService.cs       # Document tracking
│   ├── ParsingService.cs        # Incremental parsing
│   ├── DiagnosticService.cs     # Error collection
│   ├── CompletionService.cs     # Completion logic
│   └── SymbolService.cs         # Symbol queries
└── Protocol/
    └── LSPTypes.cs              # LSP protocol types
```

### Key Components

**WorkspaceService:**
- Manages open documents
- Tracks project structure
- Handles workspace-wide operations

**DocumentService:**
- Synchronizes document state
- Manages document versions
- Caches parsed ASTs

**ParsingService:**
- Incremental parsing with error recovery
- AST caching and invalidation
- Background parsing for diagnostics

**SymbolService:**
- Symbol resolution using enhanced symbol table
- Cross-file symbol queries
- Supports "find references" and "go to definition"

## Implementation Plan

### Phase 1: Infrastructure (Weeks 1-4)
1. Create language-server project
2. Add OmniSharp LSP library dependency
3. Implement basic server lifecycle
4. Set up stdio communication
5. Add VS Code extension configuration

### Phase 2: Document Synchronization (Weeks 5-8)
1. Implement document tracking
2. Handle didOpen/didChange/didClose
3. Set up incremental parsing
4. Add document AST caching

### Phase 3: Core Features (Weeks 9-12)
1. **Diagnostics:**
   - Real-time syntax error reporting
   - Semantic error reporting
   - Error recovery integration

2. **Hover:**
   - Type information display
   - Function signature display
   - Symbol documentation

3. **Completion:**
   - Keyword completion
   - Symbol completion
   - Context-aware filtering

### Phase 4: Navigation Features (Weeks 13-16)
1. **Go to Definition:**
   - Symbol resolution
   - Cross-file navigation
   - Handle multiple definitions

2. **Find References:**
   - Build reference index
   - Query all usages
   - Workspace-wide search

### Phase 5: Polish & Testing (Weeks 17-20)
1. Performance optimization
2. Integration testing
3. VS Code extension
4. Documentation and examples

## Acceptance Criteria

- [ ] Language server starts and responds to LSP messages
- [ ] Real-time diagnostics work in VS Code
- [ ] Hover shows type information
- [ ] Code completion provides context-aware suggestions
- [ ] Go to definition navigates to symbol
- [ ] Works with multiple open files
- [ ] Performance: <100ms response time for most operations
- [ ] VS Code extension published (optional)
- [ ] Documentation for setup and usage

## Technical Requirements

### Dependencies
```xml
<ItemGroup>
  <PackageReference Include="OmniSharp.Extensions.LanguageServer" Version="0.19.x" />
  <ProjectReference Include="..\compiler\compiler.csproj" />
  <ProjectReference Include="..\parser\parser.csproj" />
</ItemGroup>
```

### VS Code Extension
```json
{
  "name": "fifth-language",
  "displayName": "Fifth Language Support",
  "description": "Language support for Fifth",
  "version": "0.1.0",
  "engines": { "vscode": "^1.75.0" },
  "activationEvents": ["onLanguage:fifth"],
  "contributes": {
    "languages": [{
      "id": "fifth",
      "extensions": [".5th"],
      "configuration": "./language-configuration.json"
    }],
    "grammars": [{
      "language": "fifth",
      "scopeName": "source.fifth",
      "path": "./syntaxes/fifth.tmLanguage.json"
    }]
  }
}
```

## Example Implementation

**Hover Handler:**
```csharp
public class HoverHandler : IRequestHandler<HoverParams, Hover>
{
    private readonly DocumentService _documentService;
    private readonly SymbolService _symbolService;
    
    public async Task<Hover> Handle(HoverParams request, CancellationToken token)
    {
        var document = _documentService.GetDocument(request.TextDocument.Uri);
        var position = request.Position;
        
        // Get AST for document (from cache or parse)
        var (ast, _) = await document.GetASTAsync(resilient: true);
        
        // Find symbol at position
        var symbol = _symbolService.GetSymbolAt(ast, position);
        if (symbol == null)
            return null;
        
        // Build hover content
        var content = new MarkedStringsOrMarkupContent(
            new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = FormatSymbolInfo(symbol)
            });
        
        return new Hover
        {
            Contents = content,
            Range = symbol.Range
        };
    }
}
```

## References

- Architectural Review: `docs/architectural-review-2025.md` - Finding #2
- LSP Specification: https://microsoft.github.io/language-server-protocol/
- OmniSharp LSP library: https://github.com/OmniSharp/csharp-language-server-protocol
- Rust-analyzer (reference impl): https://github.com/rust-lang/rust-analyzer
- Related Issues: Requires #ISSUE-001 (Error Recovery), #ISSUE-003 (Incremental Compilation), #ISSUE-006 (Symbol Table)

## Estimated Effort

**20 weeks** (5 months)
- Weeks 1-4: Infrastructure and setup
- Weeks 5-8: Document synchronization
- Weeks 9-12: Core features (diagnostics, hover, completion)
- Weeks 13-16: Navigation features
- Weeks 17-20: Polish, testing, and documentation

## Dependencies

- Issue #001: Error Recovery (CRITICAL - must complete first)
- Issue #003: Incremental Compilation (for performance)
- Issue #006: Enhanced Symbol Table (for navigation features)

## Success Metrics

- Language server handles 1000+ document operations without restart
- Response time <100ms for 90% of operations
- Diagnostics appear within 500ms of typing
- Zero crashes in normal usage
- Positive user feedback on IDE experience
