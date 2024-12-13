namespace Fifth.Parser.LangProcessingPhases;
using AST.Visitors;
using Fifth.AST;
using Fifth.AST.Builders;

/// <summary>
///   DestructuringPatternFlattenerVisitor is responsible for desugaring the recursive structure
///   representing a destructuring pattern definition
/// </summary>
/// <remarks>
///   See the discussion of destructuring semantics and the resolution of names in the semantics doc
///   at doc\Semantics\other\parameter_declaration.md
/// </remarks>
public class DestructuringPatternFlattenerVisitor : DefaultMutatorVisitor<DummyContext>
{
    public override FunctionDefinition ProcessFunctionDefinition(FunctionDefinition node, DummyContext ctx)
    {
        var fb = FunctionDefinitionBuilder.CreateFunctionDefinition()
            .WithName(node.Name)
            .WithTypename(node.Typename)
            .WithIsInstanceFunction(node.IsInstanceFunction)
            .WithIsEntryPoint(node.IsEntryPoint)
            .WithFunctionKind(node.FunctionKind)
            .WithReturnType(node.ReturnType);

        // gather the destructuring paramdecls, before they are erased
        var statementGatherer = new PropertyBindingToVariableDeclarationTransformer();
        // 1. enumerate all param decls 1.1. gather all bindings and statements to add
        node.Accept(statementGatherer);
        fb.WithParameterDeclarations((ParameterDeclarationList)Process(node.ParameterDeclarations, ctx));

        var bb = BlockBuilder.CreateBlock();
        var sl = statementGatherer.Statements.Build();
        foreach (var s in sl.Statements)
        {
            bb.AddingItemToStatements(s);
        }
        foreach (var s in node.Body.Statements)
        {
            bb.AddingItemToStatements(s);
        }
        // 3. Generate body with additional statements
        fb.WithBody(bb.Build());
        fb.WithFunctionKind(node.FunctionKind);
        var result = fb.Build();
        return result;
    }

    public override ParameterDeclaration ProcessParameterDeclaration(ParameterDeclaration node, DummyContext ctx)
    {
        if (node.DestructuringDecl == null)
        {
            return node;
        }

        var pdb = ParameterDeclarationBuilder.CreateParameterDeclaration()
            .WithParameterName(node.ParameterName)
            .WithTypeName(node.TypeName);
        // TODO Process the bindings and add statements
        var result = pdb.Build();
        return node.CopyMetadataInto(result);
    }
}
