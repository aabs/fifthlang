namespace il_ast_generated;
using il_ast;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Result of rewriting an AST node, carrying the rewritten node and any statements that should be hoisted.
/// </summary>
public record RewriteResult(AstThing Node, List<Statement> Prologue)
{
    /// <summary>
    /// Creates a RewriteResult with no prologue statements.
    /// </summary>
    public static RewriteResult From(AstThing node) => new(node, []);
}

/// <summary>
/// Interface for AST rewriting with statement-level desugaring support.
/// </summary>
public interface IAstRewriter
{
    RewriteResult Rewrite(AstThing ctx);
    RewriteResult VisitAssemblyDeclaration(AssemblyDeclaration ctx);
    RewriteResult VisitAssemblyReference(AssemblyReference ctx);
    RewriteResult VisitModuleDeclaration(ModuleDeclaration ctx);
    RewriteResult VisitVersion(Version ctx);
    RewriteResult VisitClassDefinition(ClassDefinition ctx);
    RewriteResult VisitParameterDeclaration(ParameterDeclaration ctx);
    RewriteResult VisitParameterSignature(ParameterSignature ctx);
    RewriteResult VisitMethodSignature(MethodSignature ctx);
    RewriteResult VisitMethodHeader(MethodHeader ctx);
    RewriteResult VisitMethodRef(MethodRef ctx);
    RewriteResult VisitMethodImpl(MethodImpl ctx);
    RewriteResult VisitMethodDefinition(MethodDefinition ctx);
    RewriteResult VisitTypeReference(TypeReference ctx);
    RewriteResult VisitMemberRef(MemberRef ctx);
    RewriteResult VisitFieldDefinition(FieldDefinition ctx);
    RewriteResult VisitPropertyDefinition(PropertyDefinition ctx);
    RewriteResult VisitLoadInstruction(LoadInstruction ctx);
    RewriteResult VisitStoreInstruction(StoreInstruction ctx);
    RewriteResult VisitArithmeticInstruction(ArithmeticInstruction ctx);
    RewriteResult VisitBranchInstruction(BranchInstruction ctx);
    RewriteResult VisitCallInstruction(CallInstruction ctx);
    RewriteResult VisitStackInstruction(StackInstruction ctx);
    RewriteResult VisitReturnInstruction(ReturnInstruction ctx);
    RewriteResult VisitLabelInstruction(LabelInstruction ctx);
    RewriteResult VisitInstructionSequence(InstructionSequence ctx);
    RewriteResult VisitInstructionStatement(InstructionStatement ctx);
    RewriteResult VisitBlock(Block ctx);
}

/// <summary>
/// Default AST rewriter that performs structure-preserving rewrites while aggregating prologue statements.
/// Prologue statements are hoisted upward until consumed by a BlockStatement.
/// </summary>
public class DefaultAstRewriter : IAstRewriter
{
    public virtual RewriteResult Rewrite(AstThing ctx)
    {
        if(ctx == null) return RewriteResult.From(ctx);
        return ctx switch
        {
             AssemblyDeclaration node => VisitAssemblyDeclaration(node),
             AssemblyReference node => VisitAssemblyReference(node),
             ModuleDeclaration node => VisitModuleDeclaration(node),
             Version node => VisitVersion(node),
             ClassDefinition node => VisitClassDefinition(node),
             ParameterDeclaration node => VisitParameterDeclaration(node),
             ParameterSignature node => VisitParameterSignature(node),
             MethodSignature node => VisitMethodSignature(node),
             MethodHeader node => VisitMethodHeader(node),
             MethodRef node => VisitMethodRef(node),
             MethodImpl node => VisitMethodImpl(node),
             MethodDefinition node => VisitMethodDefinition(node),
             TypeReference node => VisitTypeReference(node),
             MemberRef node => VisitMemberRef(node),
             FieldDefinition node => VisitFieldDefinition(node),
             PropertyDefinition node => VisitPropertyDefinition(node),
             LoadInstruction node => VisitLoadInstruction(node),
             StoreInstruction node => VisitStoreInstruction(node),
             ArithmeticInstruction node => VisitArithmeticInstruction(node),
             BranchInstruction node => VisitBranchInstruction(node),
             CallInstruction node => VisitCallInstruction(node),
             StackInstruction node => VisitStackInstruction(node),
             ReturnInstruction node => VisitReturnInstruction(node),
             LabelInstruction node => VisitLabelInstruction(node),
             InstructionSequence node => VisitInstructionSequence(node),
             InstructionStatement node => VisitInstructionStatement(node),
             Block node => VisitBlock(node),

            { } node => RewriteResult.From(null),
        };
    }

    public virtual RewriteResult VisitAssemblyDeclaration(AssemblyDeclaration ctx)
    {
        var prologue = new List<Statement>();
        List<il_ast.AssemblyReference> tmpAssemblyReferences = [];
        foreach (var item in ctx.AssemblyReferences)
        {
            var rr = Rewrite(item);
            tmpAssemblyReferences.Add((il_ast.AssemblyReference)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Version = (il_ast.Version)Rewrite((AstThing)ctx.Version).Node
        ,PrimeModule = (il_ast.ModuleDeclaration)Rewrite((AstThing)ctx.PrimeModule).Node
        ,AssemblyReferences = tmpAssemblyReferences
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitAssemblyReference(AssemblyReference ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Version = (il_ast.Version)Rewrite((AstThing)ctx.Version).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitModuleDeclaration(ModuleDeclaration ctx)
    {
        var prologue = new List<Statement>();
        List<il_ast.ClassDefinition> tmpClasses = [];
        foreach (var item in ctx.Classes)
        {
            var rr = Rewrite(item);
            tmpClasses.Add((il_ast.ClassDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<il_ast.MethodDefinition> tmpFunctions = [];
        foreach (var item in ctx.Functions)
        {
            var rr = Rewrite(item);
            tmpFunctions.Add((il_ast.MethodDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Classes = tmpClasses
        ,Functions = tmpFunctions
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitVersion(Version ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitClassDefinition(ClassDefinition ctx)
    {
        var prologue = new List<Statement>();
        List<il_ast.FieldDefinition> tmpFields = [];
        foreach (var item in ctx.Fields)
        {
            var rr = Rewrite(item);
            tmpFields.Add((il_ast.FieldDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<il_ast.PropertyDefinition> tmpProperties = [];
        foreach (var item in ctx.Properties)
        {
            var rr = Rewrite(item);
            tmpProperties.Add((il_ast.PropertyDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<il_ast.MethodDefinition> tmpMethods = [];
        foreach (var item in ctx.Methods)
        {
            var rr = Rewrite(item);
            tmpMethods.Add((il_ast.MethodDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        List<il_ast.ClassDefinition> tmpBaseClasses = [];
        foreach (var item in ctx.BaseClasses)
        {
            var rr = Rewrite(item);
            tmpBaseClasses.Add((il_ast.ClassDefinition)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Fields = tmpFields
        ,Properties = tmpProperties
        ,Methods = tmpMethods
        ,BaseClasses = tmpBaseClasses
        ,ParentAssembly = (il_ast.AssemblyDeclaration)Rewrite((AstThing)ctx.ParentAssembly).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitParameterDeclaration(ParameterDeclaration ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitParameterSignature(ParameterSignature ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         TypeReference = (il_ast.TypeReference)Rewrite((AstThing)ctx.TypeReference).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodSignature(MethodSignature ctx)
    {
        var prologue = new List<Statement>();
        List<il_ast.ParameterSignature> tmpParameterSignatures = [];
        foreach (var item in ctx.ParameterSignatures)
        {
            var rr = Rewrite(item);
            tmpParameterSignatures.Add((il_ast.ParameterSignature)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         ParameterSignatures = tmpParameterSignatures
        ,ReturnTypeSignature = (il_ast.TypeReference)Rewrite((AstThing)ctx.ReturnTypeSignature).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodHeader(MethodHeader ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodRef(MethodRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         ClassDefinition = (il_ast.ClassDefinition)Rewrite((AstThing)ctx.ClassDefinition).Node
        ,Sig = (il_ast.MethodSignature)Rewrite((AstThing)ctx.Sig).Node
        ,Field = (il_ast.FieldDefinition)Rewrite((AstThing)ctx.Field).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodImpl(MethodImpl ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Body = (il_ast.Block)Rewrite((AstThing)ctx.Body).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMethodDefinition(MethodDefinition ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Header = (il_ast.MethodHeader)Rewrite((AstThing)ctx.Header).Node
        ,Signature = (il_ast.MethodSignature)Rewrite((AstThing)ctx.Signature).Node
        ,Impl = (il_ast.MethodImpl)Rewrite((AstThing)ctx.Impl).Node
        ,TheType = (il_ast.TypeReference)Rewrite((AstThing)ctx.TheType).Node
        ,ParentClass = (il_ast.ClassDefinition)Rewrite((AstThing)ctx.ParentClass).Node
        ,AssociatedProperty = (il_ast.PropertyDefinition)Rewrite((AstThing)ctx.AssociatedProperty).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitTypeReference(TypeReference ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitMemberRef(MemberRef ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         ClassDefinition = (il_ast.ClassDefinition)Rewrite((AstThing)ctx.ClassDefinition).Node
        ,Sig = (il_ast.MethodSignature)Rewrite((AstThing)ctx.Sig).Node
        ,Field = (il_ast.FieldDefinition)Rewrite((AstThing)ctx.Field).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitFieldDefinition(FieldDefinition ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         TheType = (il_ast.TypeReference)Rewrite((AstThing)ctx.TheType).Node
        ,ParentClass = (il_ast.ClassDefinition)Rewrite((AstThing)ctx.ParentClass).Node
        ,AssociatedProperty = (il_ast.PropertyDefinition)Rewrite((AstThing)ctx.AssociatedProperty).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitPropertyDefinition(PropertyDefinition ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         FieldDefinition = (il_ast.FieldDefinition)Rewrite((AstThing)ctx.FieldDefinition).Node
        ,TheType = (il_ast.TypeReference)Rewrite((AstThing)ctx.TheType).Node
        ,ParentClass = (il_ast.ClassDefinition)Rewrite((AstThing)ctx.ParentClass).Node
        ,AssociatedProperty = (il_ast.PropertyDefinition)Rewrite((AstThing)ctx.AssociatedProperty).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitLoadInstruction(LoadInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitStoreInstruction(StoreInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitArithmeticInstruction(ArithmeticInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBranchInstruction(BranchInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitCallInstruction(CallInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitStackInstruction(StackInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitReturnInstruction(ReturnInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitLabelInstruction(LabelInstruction ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInstructionSequence(InstructionSequence ctx)
    {
        var prologue = new List<Statement>();
        List<il_ast.CilInstruction> tmpInstructions = [];
        foreach (var item in ctx.Instructions)
        {
            var rr = Rewrite(item);
            tmpInstructions.Add((il_ast.CilInstruction)rr.Node);
            prologue.AddRange(rr.Prologue);
        }
        var rebuilt = ctx with {
         Instructions = tmpInstructions
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitInstructionStatement(InstructionStatement ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
         Instructions = (il_ast.InstructionSequence)Rewrite((AstThing)ctx.Instructions).Node
        };
        return new RewriteResult(rebuilt, prologue);
    }
    public virtual RewriteResult VisitBlock(Block ctx)
    {
        var prologue = new List<Statement>();
        var rebuilt = ctx with {
        };
        return new RewriteResult(rebuilt, prologue);
    }

}
