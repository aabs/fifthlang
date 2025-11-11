# Research: System KG Types in Fifth.System

## Decisions

- dotNetRDF interop pattern: SELECTED OPTION B – thin WRAPPER classes (composition) over dotNetRDF interfaces/types (`IGraph`, `Triple`, store abstractions). No inheritance from concrete classes; expose bridge methods `ToVds*` / `FromVds*` for advanced interop.
- Operator mapping for `graph += triple`: RESOLVED – implement C# operator overloads on wrappers; compiler resolves as standard operators (no special lowering).
- Global predeclared type names binding strategy: NEEDS CLARIFICATION
- Copy strategy for non-mutating graph binary operators (`+`/`-`): DECIDED – initial implementation uses eager deep copy (allocate new Graph, merge/clone triples) for determinism; future optimization path to structural sharing / copy-on-write is documented below.

## Rationale

Wrapper (Option B) chosen to:
1. Insulate Fifth surface from dotNetRDF version churn (interface stability, internal changes hidden).
2. Enable strict control of public API (we decide which members are exposed and can add validation / perf instrumentation later).
3. Facilitate testing: wrappers allow mocking without depending on dotNetRDF internals.
4. Avoid fragile inheritance semantics and potential breaking changes from sealed/internal members upstream.
5. Maintain perf parity: delegation is O(1); no mandatory copying of triples/graphs (we pass through underlying references).

## Implementation Sketch (Interop)

```csharp
namespace Fifth.System;

public sealed class Graph {
	private readonly VDS.RDF.IGraph _inner;
	private Graph(VDS.RDF.IGraph inner) => _inner = inner;
	public static Graph Create() => new(new VDS.RDF.Graph());
	public void Add(Triple t) => _inner.Assert(t.ToVdsTriple());
	public IEnumerable<Triple> Triples => _inner.Triples.Select(Triple.FromVds);
	public int Count => _inner.Triples.Count;
	public VDS.RDF.IGraph ToVds() => _inner;
	public static Graph FromVds(VDS.RDF.IGraph g) => new(g);
}

public sealed class Triple {
	private readonly VDS.RDF.INode _s, _p, _o;
	private Triple(VDS.RDF.INode s, VDS.RDF.INode p, VDS.RDF.INode o){ _s=s; _p=p; _o=o; }
	public static Triple Create(VDS.RDF.INode s, VDS.RDF.INode p, VDS.RDF.INode o) => new(s,p,o);
	public VDS.RDF.Triple ToVdsTriple() => new(_s,_p,_o);
	public static Triple FromVds(VDS.RDF.Triple t) => new(t.Subject, t.Predicate, t.Object);
}

public sealed class Store {
	private readonly VDS.RDF.IInMemoryQueryableStore _store;
	private Store(VDS.RDF.IInMemoryQueryableStore store) => _store = store;
	public static Store CreateInMemory() => new(new VDS.RDF.InMemoryQueryableStore());
	public Graph CreateGraph() { var g = new VDS.RDF.Graph(); _store.Add(g); return Graph.FromVds(g); }
	public object ExecuteQuery(string sparql) => new VDS.RDF.LeviathanQueryProcessor(_store).ProcessQuery(sparql);
}
```

## Alternatives Considered

- Subclassing concrete dotNetRDF classes: rejected (tight coupling, harder version upgrades)
- Direct exposure of dotNetRDF types in Fifth: rejected (leaks external API surface, unstable for language evolution)
- Pure method-only surface vs optional operator `+`: choose operator-first. Implement `operator +` on `Graph`; Fifth `+=` relies on C# augmented assignment (`g = g + t`).
- Implicit import alias vs true global predeclared type names: prefer global predeclared names (consistent with existing primitive-like experience).

## Open Questions Tracking

- Binding strategy: introduce builtin alias table vs injecting `using Fifth.System;` in Roslyn emission? (Need perf & simplicity comparison.)
- Duplicate triple behavior: ignore vs allow duplicates vs configurable policy? (Needs consistency with dotNetRDF default.)
- Thread safety guarantees: leave unspecified vs explicit non-thread-safe vs optional concurrency wrappers?

## Resolved Clarification

Operator mapping: Use explicit C# operator overloads implementing a two-tier scheme:
1. Non-mutating binary operators (`+`/`-`) produce new wrapper instances; originals are unchanged.
2. Mutating compound assignments (`+=`/`-=`) apply changes in-place on the LHS and return it.
Triple supports non-mutating `+`/`-` returning new `Triple`. Store supports mutating `+=`/`-=` with `Graph`.

### Copy Strategy Decision (Graph `+`/`-`)

We adopt an eager deep copy strategy for the first implementation of non-mutating graph binary operators to ensure:
1. Deterministic isolation of the result graph from operand mutation post-operation.
2. Simplified reasoning in tests (no aliasing surprises when later mutating the original).
3. Straightforward perf baselining (explicit O(n) triple enumeration cost is measurable and optimizable later).

Future Optimization Path:
- Introduce an internal immutable triple set snapshot ID and reference-counted storage; non-mutating ops create lightweight COW headers.
- Provide instrumentation hooks to measure triple re-allocation vs structural sharing hit rate.
- Defer until perf scenarios show >5% regression vs baseline for large graph operations.

### Example: Non-Mutating Addition (`Graph + Triple`)

Illustrative operator overload (wrapper form will adapt types once wrappers exist):

```csharp
public static Graph operator +(Graph g, Triple t)
{
	// Deep copy via explicit clone & then assert
	var cloned = new Graph(); // wrapper factory will be Graph.Create() later
	cloned.Merge(g);          // copies all triples
	cloned.Assert(t);         // adds the new triple
	return cloned;            // original g unchanged
}
```

Binary subtraction (`Graph - Triple`) will follow the same pattern except using `Retract(t)` after the clone; addition/subtraction with another Graph will clone first operand then merge / retract triples from second.

Test Implications:
- Ensure `g + t` leaves `g` unmodified (`ReferenceEquals(g, result)` is false, and `result` contains `t`).
- Subsequent mutation of `g` via `g += t2` does not affect previously computed `result`.
- Perf smoke: measure triple count copy time for graphs of size {0, 1, 10, 100, 1_000}.

Open Optimization Marker: switch to structural sharing once wrapper internal representation finalized.

## Next Clarification Candidate

Binding strategy for global type names (alias table vs injected using) to minimize translator complexity and retain performance; also confirm copy strategy for non-mutating graph operations (deep vs structural sharing).
