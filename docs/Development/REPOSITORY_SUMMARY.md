# Repository Analysis: Fifth Language (fifthlang)

## Overview

Fifth is an experimental .NET programming language that uniquely bridges imperative systems programming with native knowledge graph and semantic web capabilities. The language provides first-class support for RDF triples, SPARQL queries, and graph operations while maintaining familiar C#-like syntax enhanced with Erlang-inspired function overloading and pattern matching.

The compiler is written in C# targeting .NET 8.0, using ANTLR 4.8 for parsing, and Roslyn for IL code generation. The project is in active pre-release development, with a comprehensive test suite and multi-platform release pipeline.

## Architecture

### Compilation Pipeline

Fifth follows a multi-pass compilation architecture:

```
Source (.5th) → Lexer → Parser → Parse Tree → AST Builder → High-Level AST
                                                                ↓
                                              Language Transformations (18+ passes)
                                                                ↓
                                                          Lowered AST
                                                                ↓
                                                    IL Transformation
                                                                ↓
                                              Roslyn Code Generation → .NET Assembly
```

### Transformation Phases

The compiler implements a sophisticated lowering pipeline with dedicated visitors/rewriters:

1. **Tree Linkage** - Establishes parent-child AST relationships
2. **Builtin Injection** - Injects built-in function definitions
3. **Constructor Insertion** - Adds default constructors where needed
4. **Symbol Table Building** - Constructs scoping information
5. **Property Expansion** - Transforms property syntax to field access
6. **Overload Gathering/Transforming** - Handles function overloading with guards
7. **Destructuring** - Processes pattern matching and destructuring
8. **Type Annotation** - Performs type inference and annotation
9. **SPARQL/TriG Lowering** - Transforms graph literals and queries
10. **Query Application** - Handles SPARQL query execution semantics
11. **Unary/Augmented Assignment Lowering** - Operator desugaring

## Key Components

### `src/parser/`
ANTLR-based lexer and parser with split grammar design:
- `FifthLexer.g4` - Token definitions, keywords, literals
- `FifthParser.g4` - Syntactic grammar rules
- `AstBuilderVisitor.cs` - Transforms parse tree to high-level AST

### `src/ast-model/`
Core AST definitions using discriminated unions (Dunet):
- `AstMetamodel.cs` - High-level AST node definitions
- `ILMetamodel.cs` - IL-level AST for code generation

### `src/ast-generated/`
Auto-generated code from the AST generator (never edit manually):
- `builders.generated.cs` - Builder pattern classes
- `visitors.generated.cs` - Visitor pattern implementations
- `rewriter.generated.cs` - AST rewriter infrastructure
- `typeinference.generated.cs` - Type inference support

### `src/ast_generator/`
Code generator that produces AST infrastructure from metamodel definitions. Uses RazorLight for template-based code generation.

### `src/compiler/`
Main compiler orchestration and transformation pipeline:
- `ParserManager.cs` - Orchestrates compilation phases
- `Compiler.cs` - Entry point and CLI
- `LanguageTransformations/` - 32 transformation visitors/rewriters
- `LoweredToRoslyn/` - Roslyn code generation translator

### `src/fifthlang.system/`
Runtime library providing knowledge graph operations:
- `KnowledgeGraphs.cs` - Graph, Triple, Store, Query wrappers
- Integration with dotNetRDF for RDF/SPARQL support

### `src/Fifth.Sdk/`
MSBuild SDK for `.5thproj` project file support, enabling dotnet CLI integration.

## Technologies Used

| Category | Technologies |
|----------|--------------|
| **Language** | C# 14, .NET 8.0 |
| **Parsing** | ANTLR 4.8 (split lexer/parser grammar) |
| **Code Generation** | Roslyn (Microsoft.CodeAnalysis) |
| **RDF/SPARQL** | dotNetRDF |
| **Testing** | xUnit, FluentAssertions |
| **Code Gen Templates** | RazorLight |
| **Discriminated Unions** | Dunet |
| **Value Objects** | Vogen |
| **Build** | MSBuild, just (Justfile) |
| **CI/CD** | GitHub Actions |

## Data Flow

```
Fifth Source Code (.5th)
        ↓
    ANTLR Lexer
        ↓ tokens
    ANTLR Parser
        ↓ parse tree
  AstBuilderVisitor
        ↓ High-Level AST
Language Transformation Pipeline (18+ passes)
        ↓ Lowered AST
 IL Transformation
        ↓ IL-Level AST
LoweredAstToRoslynTranslator
        ↓ Roslyn SyntaxTree
    Roslyn Compilation
        ↓
  .NET Assembly (.dll/.exe)
```

## Team and Ownership

| Area | Primary Maintainer |
|------|-------------------|
| Language Design | Andrew Matthews |
| Compiler Core | Andrew Matthews |
| Knowledge Graph Integration | Andrew Matthews |
| CI/CD & Release Pipeline | Andrew Matthews |
| AI-Assisted Development | copilot-swe-agent[bot] |

The project demonstrates a novel human-AI collaboration model, with GitHub Copilot's coding agent contributing approximately 44% of commits since August 2025, primarily implementing features from detailed specifications written by the human maintainer.

## Repository Statistics

- **Total Commits**: 811
- **Project Start**: September 21, 2024
- **Completed Specifications**: 14 features
- **Language Transformations**: 32 visitors/rewriters
- **Test Projects**: 8
- **Supported Platforms**: Linux, macOS, Windows (x64, arm64)
- **Target Runtimes**: .NET 8.0, .NET 10.0 (preview)

## Project Status

**Stage**: Active Development | Experimental | Pre-release

The language is functional for basic programs and knowledge graph operations. Key completed features include:
- Full .NET IL compilation via Roslyn backend
- Exception handling (try/catch/finally)
- Generics support
- Constructor functions
- TriG literal expressions for graph data
- SPARQL literal expressions for queries
- Query application with result types
- Multi-platform release packaging

**Roadmap priorities**:
- Language Server Protocol (LSP) support
- Parser error recovery for IDE integration
- Incremental compilation
- Package manager feeds (NuGet)
