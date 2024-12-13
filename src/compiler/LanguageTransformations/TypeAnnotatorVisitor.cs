namespace Fifth.Parser.LangProcessingPhases;

using System;
using System.Collections.Generic;
using AST;
using AST.Visitors;
using TypeSystem;

public class TypeAnnotatorVisitor : BaseAstVisitor
{
    private readonly Stack<IAstNode> currentFunctionDef = new();
    private List<TypeCheckingError> errors = new();

    public override void EnterFifthProgram(FifthProgram ctx)
    {
        var tc = new FunctionalTypeChecker(OnTypeInferred, OnTypeNotFound, OnTypeMismatch, OnTypeNotRelevant);
        tc.Infer(ctx);
    }

    public void OnTypeInferred(IAstNode node, IType type)
    {
        _ = node ?? throw new ArgumentNullException(nameof(node));
        _ = type ?? throw new ArgumentNullException(nameof(type));
        if (node is ITypedAstNode typedAstNode)
        {
            typedAstNode.TypeId = type.TypeId;
        }
    }

    public void OnTypeMismatch(IAstNode node, IType type1, IType type2)
    {
        errors.Add(new TypeCheckingError("Mismatch between types", node.Filename, node.Line, node.Column,
            new[] { type1, type2 }));
    }

    public void OnTypeNotFound(IAstNode node)
    {
        errors.Add(new TypeCheckingError("Unable to infer type", node.Filename, node.Line, node.Column,
            new IType[] { }));
    }

    public void OnTypeNotRelevant(IAstNode node)
    { }
}
