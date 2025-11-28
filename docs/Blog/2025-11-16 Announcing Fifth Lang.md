---
title: Announcing Fifth - A New Language For Knowledge Graphs
summary: Fifth is a new experimental language built on .NET, designed to make working with knowledge graphs feel natural and expressive. It removes much of the boilerplate and friction typically involved in graph-centric development, offering clean syntax and built-in support for graph structures and queries. If you're exploring semantic models or graph-powered workflows in .NET, Fifth might be worth a look.
authors:
    - Andrew Matthews
date: 2025-11-16
---

# Announcing Fifth: A New Language For Knowledge Graphs

For a long time I’ve found working with RDF, graphs, and SPARQL more awkward than it should be. While mainstream languages give us straightforward ways to handle lists, classes, and functions, the moment you step into semantic web technologies, the experience often feels bolted-on and cumbersome.

I wanted to see if it was possible to design a language where RDF and SPARQL felt like natural parts of the syntax—no different from writing a loop or defining a class. That idea led to Fifth, a small language built on .NET. It’s strongly typed, multi-paradigm, and borrows familiar constructs from languages like C# and Erlang, but with RDF and SPARQL built in as first-class features.

## What Fifth Offers Today

- Classes, methods, properties, and control flow you’d expect from a general-purpose language.
- Function overloading with guard clauses and parameter destructuring.
- Native RDF primitives: graph, triple, store, query.
- Inline triple literals (`<subject, predicate, object>`).
- Multi-line TriG blocks for graph construction.
- Embedded SPARQL queries as literals (`?<SELECT ...>`).
- Operator syntax for clean graph and store manipulation.
- A working Roslyn-based compiler pipeline with IL emission.

## Status

Fifth is a **working language**, but it’s not mature or polished. Think of it as a prototype you can experiment with rather than something ready for production. It compiles, runs, and supports knowledge graph operations, but there are plenty of rough edges and missing features.

## Why Share This?

The project isn’t intended to compete with established languages or claim any big theoretical advances. It’s simply an exploration of whether RDF and SPARQL can feel “native” in a programming language. If you’ve ever thought “_why is RDF integration so clunky?_”, Fifth might be interesting to try. I’m producing the language for my own satisfaction, but I would really love to know if you find it useful.

## Get Involved

- The project is open-source: [GitHub repo](https://github.com/aabs/fifthlang)
- You can clone, build, and run simple programs today.
- Contributions, bug reports, and feature suggestions are VERY welcome.
- Even just trying it out and sharing what works—or doesn’t—would be valuable.

## Closing Thoughts

Fifth is an experiment, but it’s already usable. If you’re curious about compilers, semantic web tech, or enjoy tinkering with new languages, please take a look. Feedback, ideas, and constructive criticism are all appreciated.