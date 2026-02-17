#set page(
  width: 297mm,
  height: 210mm,
  margin: (top: 10mm, bottom: 10mm, left: 12mm, right: 12mm),
)

#set text(
  size: 9pt,
)

#show raw: set text(size: 7.2pt)
#let example(code) = block(inset: (left: 6pt))[#raw(block: true, lang: "fifth", code)]
#let section(title, body) = block(
  width: 100%,
  fill: rgb("#F7F9FC"),
  inset: (x: 6pt, y: 5pt),
  radius: 3pt,
  stroke: (paint: rgb("#D7DEE8"), thickness: 0.6pt),
  above: 6pt,
  below: 5pt,
)[
  #text(size: 10pt, weight: "bold")[#title]
  #v(4pt)
  #body
]

#align(center)[
  #text(size: 16pt, weight: "semibold")[Fifth Language Quick Reference]
  #text(size: 9pt, fill: rgb("#5A6777"))[
    Syntax, types, keywords, and idioms (verified against learnfifth examples)
  ]
]

#columns(3, gutter: 2em)[

#section("Declarations", [
  Module import
  #example("import Math;\nimport IO;\nimport Net;")

  Alias (IRI namespaces)
  #example("alias ex as <http://example.org/>;\nalias foaf as <http://xmlns.com/foaf/0.1/>;")

  Store declaration (SPARQL endpoint)
  #example("myStore : store = sparql_store(<http://localhost:8080/graphdb>);")

  Variable declarations
  #example("x: int;\ny: float;\ns: string;\n\nz: int;\nz = 100;")
])

#section("Literals", [
  Integers
  #example("x = 42;\nx = 0b101010;\nx = 0o52;\nx = 0x2A;\nx = 42i;")

  Floats, bool, string, null
  #example("pi = 3.14159;\npi = 3.14e0;\n\nt = true;\nf = false;\n\nplain = \"Hello\";\nraw = `Raw\\nstring`;\n\nptr = null;")
])

#section("Types", [
  Built-in (core + KG)
  numeric: int, float
  bool, string
  graph, triple, store

  Arrays, lists, generics
  #example("arr: int[10];\ndynamicArr: int[];\nmatrix: int[5][5];\n\nnumbers: [int];\noptional: Maybe<int>;")
])

#section("Operators", [
  Arithmetic, comparison, logical, bitwise
  #example("x = 10 + 5;\nx = 10 - 5;\nx = 10 * 5;\nx = 10 / 5;\nx = 10 % 3;\nx = 2 ** 8;\nx = 2 ^ 8;\n\nresult = 5 == 5;\nresult = 5 != 3;\nresult = 5 < 10;\nresult = 5 <= 5;\nresult = 10 > 5;\nresult = 10 >= 5;\n\nresult = true && false;\nresult = true || false;\nresult = !true;\nresult = true ~ false;\n\nx = 5 | 3;\nx = 5 & 3;\nx = 5 << 2;\nx = 20 >> 2;")

  Increment / compound assignment
  #example("x++;\nx--;\n++x;\n--x;\n\nx += 5;\nx -= 3;")
])

#section("Functions", [
  Declaration + call
  #example("add(x: int, y: int): int {\n    return x + y;\n}\n\nsum = add(5, 3);")

  Constrained overloads + base case
  #example("positive(x: int | x > 0): int { return x * 2; }\npositive(x: int): int { return 0; }\n\nclassify(x: int | x < 0): string { return \"negative\"; }\nclassify(x: int | x == 0): string { return \"zero\"; }\nclassify(x: int | x > 0): string { return \"positive\"; }\nclassify(x: int): string { return \"unknown\"; }")

  Parameter destructuring
  #example("class Person {\n    FirstName: string;\n    LastName: string;\n}\n\ngreetPerson(p: Person { first: FirstName, last: LastName }): string {\n    return first;\n}")
])

#section("Functional Programming", [
  Function types
  #example("f: [int] -> int;\nnoop: [] -> int;")

  Lambda expressions (fun)
  #example("inc: [int] -> int;\ninc = fun(x: int): int {\n    return x + 1;\n};")

  Higher-order function usage
  #example("applyTwice(f: [int] -> int, x: int): int {\n    return f(f(x));\n}\n\nresult = applyTwice(fun(x: int): int { return x + 1; }, 5);")
])

#section("Control Constructs", [
  If / else, while
  #example("if (x > 5) {\n    x = 1;\n}\n\nif (x < 5) {\n    x = 0;\n} else {\n    x = 1;\n}\n\nwhile (i < 10) {\n    i++;\n}")

  Blocks + expression statements
  #example("{\n    y: int;\n    y = 5;\n}\n\n5 + 3;\nx = 10;\nx > 5;")
])

#section("Lists & Comprehensions", [
  #example("empty = [];\nnumbers = [1, 2, 3, 4, 5];\n\nys = [x from x in xs where x > 2];\n\nfirst = numbers[0];")
])

#section("Classes & Objects", [
  Classes, methods, inheritance
  #example("class Rectangle {\n    Width: float;\n    Height: float;\n}\n\nclass Calculator {\n    Value: int;\n    Add(x: int): int { return Value + x; }\n    Multiply(x: int): int { return Value * x; }\n}\n\nclass Shape { Color: string; }\nclass Circle extends Shape { Radius: float; }")

  Instantiation + member access
  #example("rect = new Rectangle();\nrect = new Rectangle() { Width = 10.0, Height = 5.0 };\ncalc = new Calculator() { Value = 100 };\n\nw = rect.Width;\nrect.Width = 15.0;")
])

#section("Knowledge Graphs", [
  Triple literals
  #example("<ex:john, foaf:name, \"John Doe\">;\n<ex:john, foaf:age, 30>;\n<ex:john, ex:knows, ex:jane>;")

  Graphs + stores
  #example("g : graph in <ex:> = KG.CreateGraph();\ng += <ex:subject1, ex:predicate1, ex:object1>;\n\nmyStore += g;\nmyStore -= g;")

  Semantic classes
  #example("class Entity in <http://example.org/ontology#> {\n    Name: string;\n    Value: int;\n}")
])

#section("Idioms", [
  Constrained overloads must include a base-case (unconstrained) function.
  #linebreak()
  Use object initializers for readable construction.
  #linebreak()
  Use list comprehensions for projection + filtering.
  #linebreak()
  Graphs are mutable: add/remove with `+=` and `-=`.
])

#section("Keywords (reserved)", [
  alias, as, base, break, case, catch, class, const, continue, default, defer, else, extends, export, fallthrough, finally, for, from, func, fun, go, goto, graph, if, import, in, interface, new, namespace, package, range, return, select, sparql_store, store, struct, switch, throw, try, type, use, var, when, where, while, with, true, false, null
])
]
