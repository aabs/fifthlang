using ast;
using ast_model.Symbols;
using ast_model.TypeSystem;
using Symbol = ast_model.Symbols.Symbol;

namespace ast_model
{
    public abstract record ScopeAstThing : AstThing, IScope
    {
        [IgnoreDuringVisit]
        public IScope EnclosingScope { get; init; }
        [IgnoreDuringVisit]
        public ISymbolTable SymbolTable { get; init; }

        public bool TryResolve(Symbol symbol, out ISymbolTableEntry result)
        {
            result = null;
            var tmp = SymbolTable.Resolve(symbol);
            if (tmp != null)
            {
                result = tmp;
                return true;
            }

            return this.Parent.NearestScope()?.TryResolve(symbol, out result) ?? false;
        }

        public void Declare(Symbol symbol, IAstThing astThing, SourceContext srcContext, Dictionary<string, object> annotations)
        {
            var symTabEntry = new SymTabEntry
            {
                Symbol = symbol,
                Annotations = annotations,
                SourceContext = srcContext,
                Context = astThing
            };
            SymbolTable[symbol] = symTabEntry;
        }

        public ISymbolTableEntry Resolve(Symbol symbol)
        {
            if (TryResolve(symbol, out var ste))
            {
                return ste;
            }

            throw new CompilationException($"Unable to resolve symbol {symbol.Name}");
        }
    }
}
