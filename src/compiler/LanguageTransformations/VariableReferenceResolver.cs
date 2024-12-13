namespace Fifth.Parser.LangProcessingPhases;

using System.Linq;
using AST;
using AST.Visitors;
using Symbols;
using TypeSystem;

public class VariableReferenceResolver : BaseAstVisitor
{
    public bool TryResolve(VariableReference ctx, ScopeAstNode scope, out ISymbolTableEntry ste)
    {
        if (ctx.SymTabEntry != null)
        {
            // nothing to do, it's already resolved
            ste = ctx.SymTabEntry;
            return true;
        }

        if (scope.TryResolve(ctx.Name, out ste))
        {
            ctx.SymTabEntry = ste;
            return true;
        }

        return false;
    }

    public override void EnterVariableReference(VariableReference ctx)
    {
        _ = TryResolve(ctx, ctx.NearestScope(), out var _);
    }

    /*
    public override void EnterCompoundVariableReference(CompoundVariableReference ctx)
    {
        var head = ctx.ComponentReferences.First();
        if (TryResolve(head, ctx.NearestScope(), out var ste))
        {
            var scope = ste.Context.NearestScope();
            // if (ste.SymbolKind == SymbolKind.FormalParameter || ste.SymbolKind == SymbolKind.PropertyDefinition)
            // {
            // if we get here, then the head refers to a param.  We need to resolve its
            // type so we can resolve subsequent vars in the compound var relative to
            // it's properties
            if (ste.Context is ParameterDeclaration parameterDeclaration)
            {
                if (TypeRegistry.DefaultRegistry.TryGetTypeByName(parameterDeclaration.TypeName, out var type))
                {
                    if (type is UserDefinedType userDefinedType)
                    {
                        scope = userDefinedType.Definition;
                    }
                }
            }
            else if (ste.Context is TypedAstNode tan)
            {
                var t = tan.TypeId.Lookup();
                if (t is UserDefinedType userDefinedType)
                {
                    scope = userDefinedType.Definition;
                }
            }
            else if (ste.Context is VariableDeclarationStatement vds)
            {
                IType t;
                if (vds.TypeId != null)
                {
                    t = vds.TypeId.Lookup();
                }
                else if (!string.IsNullOrWhiteSpace(vds.TypeName ?? vds.UnresolvedTypeName))
                {
                    if (TypeRegistry.DefaultRegistry.TryGetTypeByName(vds.TypeName ?? vds.UnresolvedTypeName, out var type))
                    {
                        if (type is UserDefinedType userDefinedType)
                        {
                            scope = userDefinedType.Definition;
                        }
                    }
                }
            }

            // }
            foreach (var vr in ctx.ComponentReferences.Skip(1))
            {
                if (TryResolve(vr, scope, out ste))
                {
                    scope = ste.Context.NearestScope();
                }
            }
        }
    }
*/
}
