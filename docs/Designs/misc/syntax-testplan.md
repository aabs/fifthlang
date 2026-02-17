# Fifth Language Syntax Test Plan

This document enumerates a comprehensive set of syntax test cases derived from `src/parser/grammar/Fifth.g4`. Each bullet represents a testable syntactic capability. Use these to drive sample `.5th` files and parser/compile tests.

## Program Structure
- Empty program: parses with no declarations.
- Imports + aliases + declarations mixed.
- Multiple classes and functions interleaved.

## Module Imports
- Single import: `use Math;`
- Multiple imports: `use Math, IO, Net;`
- Underscore in module: `use my_lib;`
- Invalid missing semicolon: `use Math` (error)
- Invalid non-identifier: `use 123;` (error)

## Aliases
- Basic alias: `alias P as http://example;`
- Domain with dots: `alias Web as http://example.com;`
- Path segments: `alias Api as http://example.com/v1/users;`
- Trailing slash: `alias Root as http://example.com/;`
- Fragment empty: `alias Frag as http://example#;`
- Fragment named: `alias Frag2 as http://example#anchor;`
- Invalid no scheme: `alias X as ://example;` (error)
- Invalid bad domain: `alias X as http://;` (error)

## Types
- Type name identifier in params/returns/var decls: `int`, `MyType`.
- List type signature: `xs: [int];`
- Array type unsized: `arr: int[];`
- Array type sized (operand): `arr: int[10]; arr2: int[(1+2)*3];`

## Classes
- Empty class: `class Person { }`
- Properties only.
- Methods only.
- Mixed properties and methods.
- Multiple methods with same name (parser-level overload-like).

## Functions
- No params.
- Single param.
- Multiple params.
- Param with constraint: `x: int | x > 0`.
- Param with destructuring.
- Nested destructuring.
- Destructure binding with constraint.
- Invalid missing return expression for non-void: (error)

## Blocks
- Empty block: `{}`.
- Multiple statements in block.
- Nested blocks.

## Statements
- Variable declaration type only.
- Var decl with initializer.
- List-typed var decl.
- Array-typed var decl.
- Assignment statement.
- Expression statement (with expression).
- Empty expression statement: `;`.
- If without else.
- If with else.
- Else-if chain.
- While loop.
- With statement with block and single statement forms.
- Return statement.
- Invalid missing semicolons (error).

## Lists and Comprehensions
- List literal single and multiple.
- List literal nested and with expressions.
- List comprehension simple: `[x in xs]`.
- List comprehension with constraint: `[x in xs # x > 0]`.
- Invalid empty list `[]` (error by grammar as written).

## Object Instantiation
- Bare `new Type`.
- `new Type()`.
- `new Type(name: string)` (paramdecl args per grammar).
- `new Type { prop = expr, ... }`.
- Args + property init together.
- Invalid trailing comma in property init (error).
- Invalid using expression args e.g. `new T("A")` (error per grammar).

## Expressions: Operands
- Identifier.
- Parenthesized expression.
- Literals of all kinds.
- List operand.
- Object instantiation operand.

## Expressions: Member/Index
- Member access: `a.b` and chained.
- Indexing: `a[0]` and chained.
- Mixed access/index.
- Call then access/index.
- Slice syntax appears in grammar as `slice_` but is unreachable; using it should error.

## Expressions: Calls
- No-arg call.
- One-arg call.
- Multi-arg call.
- Nested calls.

## Expressions: Unary
- Prefix plus/minus.
- Logical not.
- Prefix/postfix inc/dec.
- Combined prefix chain.

## Expressions: Binary and Precedence
- Power (right-assoc): `2 ^ 3 ^ 2`.
- Multiplicative: `* / % << >> & **`.
- Additive: `+ - | ~`.
- Relational: `== != < <= > >=`.
- Logical: `&& ||`.
- Long chain to validate precedence/associativity.
- Parentheses altering precedence.

## Literals
- `null`.
- `true`, `false`.
- Integers: decimal with/without suffix, binary, octal, hex.
- Imaginary numbers with `i`.
- Runes: basic, escapes, hex, unicode.
- Reals: decimal forms with exponent/suffix.
- Strings: interpreted, interpolated, raw.

## Variable Constraints and Destructuring
- Simple constraints on params.
- Constraints with complex expressions.
- Destructure binding constraints.
- Nested destructuring with constraints.

## Trivia
- Single-line comments.
- Multi-line comments.
- Whitespace robustness.
- Explicit semicolons vs newlines (grammar treats semicolons explicitly).

## Invalid/Unused Tokens (Negative)
- Logical NAND/NOR `!&`, `!|` (unused by parser).
- Concatenation operator `<>` (unused by parser).
- Go-like tokens `:=`, `...`, `<-` (unused by parser).
- Standalone underscore `_` as identifier (token `UNDERSCORE`).
- Unused keywords in statement/expression positions (map, interface, struct, type, package, for, switch, select, defer, go, goto, range, const, var, break, continue, fallthrough, default, case).

## Error Cases
- Missing semicolons after decl/assign/expr/return.
- Mismatched braces/parens/brackets.
- Bad member access RHS: `a.`.
- Bad index: `a[]`.
- Empty list literal `[]`.
- Slice syntax: `a[1:2]`, `a[:2]`, `a[1:2:3]` (unreachable rule).
- Return outside function scope.
- Assignment to literal `1 = x;` (syntactically may pass, semantically invalid).

## Knowledge Graphs
- Canonical store declarations:
	- `name : store = sparql_store(<http://example.org/store>);`
	- `store default = sparql_store(<http://example.org/store>);`
- Graph assertion blocks:
	- Statement-form `<{ ... }>;` requires a default store and saves the constructed graph.
	- Expression-form `<{ ... }>` yields an `IGraph` value.
- Literal coverage in object position: strings, bools, chars, signed/unsigned integers, float, double, decimal.
- Lowering strategy: graph blocks lower to `Fifth.System.KG` helpers (`CreateGraph`, `CreateUri`, `CreateLiteral`, `CreateTriple`, `Assert`, `SaveGraph`).
