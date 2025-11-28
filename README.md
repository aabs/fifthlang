# Fifth Language

[![CI](https://github.com/aabs/fifthlang/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/aabs/fifthlang/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)

A .NET systems programming language with native support for knowledge graphs and semantic web technologies.  
Fifth is still under active development, and is not yet ready for mission-critical use.  
I invite you to get involved and play with it, and tell me what you do and don't love.

---

## Language Features

Fifth uniquely combines imperative programming with first-class RDF and SPARQL support.  Mostly, it's a lot like C#, but it takes the syntax for Function overloading, Destructuring, and nested Guard Clauses from languages like Erlang.  For a tour of the language, take a look at the [Learn X=5th in Y Minutes Guide](https://raw.githubusercontent.com/aabs/fifthlang/refs/heads/master/docs/learn5thInYMinutes.md).  

More docs [here](https://fifth-lang.org) including [installation](https://fifth-lang.org/Getting-Started/installation/) instructions.

### Basic Language Features
- Classes with methods and properties
- Function overloading with parameter constraints (guards)
- Parameter Destructuring with guard clauses
- All of the usual control-constructs
- Exception handling: try/catch/finally blocks
- Multiple module support with namespaces
- Namespace imports with aliasing support
- Type system: Primitives, classes, lists, Arrays
- List comprehensions with filtering

### Knowledge Graph Primitives
- Native RDF types: `graph`, `triple`, `store`, `query` are built-in language primitives
- Built-in KG runtime: `Fifth.System.KG` provides graph creation, triple management, and store operations
- Triple literals: `<subject, predicate, object>` syntax for inline RDF construction
- TriG blocks: Multi-line graph literals with full TriG syntax support
- SPARQL literals: Embed SPARQL queries directly in source code with `?<SELECT...>`
- Operator syntax provides clean and intuitive ways to work with triples, graphs, triple-stores and queries.
- Transparent persistence: Save graphs to remote stores with simple assignment: `myStore += graph;`

### What Works
- Full .NET IL compilation pipeline (via Roslyn back-end)
- Multi-platform support (Linux, macOS, Windows)
- MSBuild integration with `.5thproj` project files (very basic at this stage)
- Parameter destructuring in functions
- Classes with methods and properties
- Control flow statements (if/else, while)
- Exception handling with try/catch/finally
- Function overloading with parameter guards
- List comprehensions
- Knowledge graph operations (TriG literals, SPARQL literals, graph operations)
- Comprehensive test suite (TUnit + FluentAssertions)

### Planned Improvements
See our [architectural roadmap](docs/NEXT-STEPS.md) for detailed plans. Key priorities:

- Published MSBuild SDK and compiler support via Nuget
- Direct Consumption of Query Results in List Comprehensions
- Architectural Improvements to support modern compiler tool chains: auto-complete, LSP, go to definition &c
- Parser error recovery: Better handling of syntax errors for IDE support
- Incremental compilation: Faster rebuild times for large projects

Full analysis available in [architectural review](docs/architectural-review-2025.md).

## Quick Start

### Prerequisites
- .NET SDK 8.0+ ([download](https://dotnet.microsoft.com/download))

### Installation

Download the latest release from the [releases page](https://github.com/aabs/fifthlang/releases) or build from source:

```bash
git clone https://github.com/aabs/fifthlang.git
cd fifthlang
dotnet build fifthlang.sln
```

### Your First Fifth Program

Create a file `hello.5th`:

```fifth
main(): int {
    x: int = 42;
    return x;
}
```

Build and run with a `.5thproj` file (see below for project setup).

### Working with Knowledge Graphs

Create a file `kg-example.5th`:

```fifth
// Connect to a SPARQL store
alias x as <http://example.com/blah#>;
alias rdf as <http://www.w3.org/1999/02/22-rdf-syntax-ns#>;
myStore : store = sparql_store(<http://localhost:8080/graphdb>);

main(): int {

    // Create a graph and add triples
    g: graph = @< >;
    g += <x:Alice, x:age, 42>;
    g += <x:Alice, rdf:type, x:Person>;
    
    // Save to the store
    myStore += g;
    
    // SPARQL Literals embedded in 5th code...
    age: int = 42;
    rq: query = ?<
        PREFIX x: <http://example.com/blah#>
        SELECT ?person
        WHERE {
            ?person x:age age .
        }
    >;
    
    fortyTwoYearOlds: result = rq <- myStore ; // query application on a store
    
    // go do something with the results

    return 0;
}
```

---

## Creating Fifth Projects

Fifth integrates with .NET's build system using `.5thproj` files:

```xml
<!-- MyApp.5thproj -->
<Project Sdk="Fifth.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

Build like any .NET project:

```bash
dotnet build MyApp.5thproj
dotnet run --project MyApp.5thproj
```

See [Fifth.Sdk documentation](src/Fifth.Sdk/README.md) for more details.

---

## Roadmap
- Introduction of Dataset and separation of datasets and stores.
- Graph/Dataset Destructuring Into Object Instances
- SPARQL integration into List Comprehensions. ([ideas](https://github.com/aabs/fifthlang/wiki/List-Comprehension-Syntax))
- MSBUILD SDK Support for full Visual Studio integration with Project Templates published via Nuget.
- Inference Support

### Recently Completed
- Multi-platform release pipeline (spec 014)
- Constructors (spec 013)
- Generics (spec 012)
- Query Application (spec 011)
- TriG literal expressions (spec 009) - Multi-line graph blocks with TriG syntax
- SPARQL literal expressions (spec 010) - Embedded SPARQL queries
- System KG types (spec 008) - Runtime graph operations via Fifth.System.KG
- Roslyn backend (spec 006) - IL emission and compilation pipeline
- Exception handling (spec 005) - Try/catch/finally control flow
- Guard clauses (spec 002) - Parameter constraints for function overloading
- Namespace imports (spec 004) - Import directives with aliasing
1. Q1 2026: Error recovery + diagnostic improvements
2. Q2 2026: Language Server Protocol (LSP) + incremental compilation
3. Q3 2026: Symbol table enhancements + testing architecture

See [roadmap details](docs/NEXT-STEPS.md) and [issue templates](docs/arch-review-issues/).

---

## Documentation

### Getting Started
- [Learn Fifth in Y Minutes](docs/learn5thInYMinutes.md) - Quick language tour
- [Knowledge Graphs Guide](docs/knowledge-graphs.md) - RDF/SPARQL features
- [Example Programs](docs/examples/) - Real Fifth code

### Language Reference
- [Architectural Review](docs/architectural-review-2025.md) - Compiler design deep dive
- [Language Specifications](specs/) - Detailed feature specs
- [Completed Features](specs/) - See `completed-*` directories

### Community
- [GitHub Discussions](https://github.com/aabs/fifthlang/discussions) - Ask questions, share ideas
- [Issues](https://github.com/aabs/fifthlang/issues) - Bug reports and feature requests
- [Contributing](AGENTS.md) - Development guidelines

---

## Contributing

We welcome contributions from the community. Areas where help is particularly valuable:

- Language design feedback and suggestions
- Documentation improvements and examples
- Bug reports with minimal reproductions
- Feature proposals with use cases

To get started:
1. Check open issues tagged [`good-first-issue`](https://github.com/aabs/fifthlang/labels/good-first-issue)
2. Read [development instructions](AGENTS.md) if you want to work on the compiler
3. Start a discussion for questions or proposals

---

## License

Fifth is distributed under the MIT License. See [LICENSE](src/LICENSE) for details.

---

## Project Structure

```
src/
├── parser/              ANTLR-based parser (FifthLexer.g4, FifthParser.g4)
├── ast-model/           Core AST definitions (AstMetamodel.cs)
├── ast-generated/       Auto-generated builders & visitors
├── compiler/            Transformation pipeline (18 phases)
├── code_generator/      IL emission (Roslyn-based)
├── fifthlang.system/    Runtime library (KG operations)
└── Fifth.Sdk/           MSBuild integration

test/
├── ast-tests/           AST builder & visitor tests
├── syntax-parser-tests/ Grammar & parsing tests
├── runtime-integration-tests/ End-to-end execution tests
└── kg-smoke-tests/      Knowledge graph feature tests
```

Built with: C# 14, .NET 8.0, ANTLR 4.8, dotNetRDF, Roslyn, TUnit

Status: Active development | Experimental | Pre-release
