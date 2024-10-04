using ast;

namespace ast_model.Symbols
{
    public interface ISymbolTableEntry
    {
        Dictionary<string, object> Annotations { get; init; }
        IAstThing Context { get; init; }
        SourceContext SourceContext { get; init; }
        Symbol Symbol { get; init; }
    }

    public record SymTabEntry : ISymbolTableEntry
    {
        public Symbol Symbol { get; init; }
        public IAstThing Context { get; init; }
        public SourceContext SourceContext { get; init; }
        public Dictionary<string, object> Annotations { get; init; }

        public void Deconstruct(out Symbol Symbol, out IAstThing Context, out SourceContext SourceContext, out Dictionary<string, object> Annotations)
        {
            Symbol = this.Symbol;
            Context = this.Context;
            SourceContext = this.SourceContext;
            Annotations = this.Annotations;
        }
    }

    public record Symbol
    {
        public string Name { get; set; }
        public SymbolKind SymbolKind { get; set; }
    }
}
