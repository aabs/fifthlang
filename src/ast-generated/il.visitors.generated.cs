namespace il_ast_generated;
using il_ast;
using System.Collections.Generic;

public interface IAstVisitor
{
    public void EnterAssemblyDeclaration(AssemblyDeclaration ctx);
    public void LeaveAssemblyDeclaration(AssemblyDeclaration ctx);
    public void EnterAssemblyReference(AssemblyReference ctx);
    public void LeaveAssemblyReference(AssemblyReference ctx);
    public void EnterModuleDeclaration(ModuleDeclaration ctx);
    public void LeaveModuleDeclaration(ModuleDeclaration ctx);
    public void EnterVersion(Version ctx);
    public void LeaveVersion(Version ctx);
    public void EnterClassDefinition(ClassDefinition ctx);
    public void LeaveClassDefinition(ClassDefinition ctx);
    public void EnterParameterDeclaration(ParameterDeclaration ctx);
    public void LeaveParameterDeclaration(ParameterDeclaration ctx);
    public void EnterParameterSignature(ParameterSignature ctx);
    public void LeaveParameterSignature(ParameterSignature ctx);
    public void EnterMethodSignature(MethodSignature ctx);
    public void LeaveMethodSignature(MethodSignature ctx);
    public void EnterMethodHeader(MethodHeader ctx);
    public void LeaveMethodHeader(MethodHeader ctx);
    public void EnterMethodRef(MethodRef ctx);
    public void LeaveMethodRef(MethodRef ctx);
    public void EnterMethodImpl(MethodImpl ctx);
    public void LeaveMethodImpl(MethodImpl ctx);
    public void EnterMethodDefinition(MethodDefinition ctx);
    public void LeaveMethodDefinition(MethodDefinition ctx);
    public void EnterTypeReference(TypeReference ctx);
    public void LeaveTypeReference(TypeReference ctx);
    public void EnterMemberRef(MemberRef ctx);
    public void LeaveMemberRef(MemberRef ctx);
    public void EnterFieldDefinition(FieldDefinition ctx);
    public void LeaveFieldDefinition(FieldDefinition ctx);
    public void EnterPropertyDefinition(PropertyDefinition ctx);
    public void LeavePropertyDefinition(PropertyDefinition ctx);
    public void EnterLoadInstruction(LoadInstruction ctx);
    public void LeaveLoadInstruction(LoadInstruction ctx);
    public void EnterStoreInstruction(StoreInstruction ctx);
    public void LeaveStoreInstruction(StoreInstruction ctx);
    public void EnterArithmeticInstruction(ArithmeticInstruction ctx);
    public void LeaveArithmeticInstruction(ArithmeticInstruction ctx);
    public void EnterBranchInstruction(BranchInstruction ctx);
    public void LeaveBranchInstruction(BranchInstruction ctx);
    public void EnterCallInstruction(CallInstruction ctx);
    public void LeaveCallInstruction(CallInstruction ctx);
    public void EnterStackInstruction(StackInstruction ctx);
    public void LeaveStackInstruction(StackInstruction ctx);
    public void EnterReturnInstruction(ReturnInstruction ctx);
    public void LeaveReturnInstruction(ReturnInstruction ctx);
    public void EnterLabelInstruction(LabelInstruction ctx);
    public void LeaveLabelInstruction(LabelInstruction ctx);
    public void EnterInstructionSequence(InstructionSequence ctx);
    public void LeaveInstructionSequence(InstructionSequence ctx);
    public void EnterInstructionStatement(InstructionStatement ctx);
    public void LeaveInstructionStatement(InstructionStatement ctx);
    public void EnterBlock(Block ctx);
    public void LeaveBlock(Block ctx);
}

public partial class BaseAstVisitor : IAstVisitor
{
    public virtual void EnterAssemblyDeclaration(AssemblyDeclaration ctx){}
    public virtual void LeaveAssemblyDeclaration(AssemblyDeclaration ctx){}
    public virtual void EnterAssemblyReference(AssemblyReference ctx){}
    public virtual void LeaveAssemblyReference(AssemblyReference ctx){}
    public virtual void EnterModuleDeclaration(ModuleDeclaration ctx){}
    public virtual void LeaveModuleDeclaration(ModuleDeclaration ctx){}
    public virtual void EnterVersion(Version ctx){}
    public virtual void LeaveVersion(Version ctx){}
    public virtual void EnterClassDefinition(ClassDefinition ctx){}
    public virtual void LeaveClassDefinition(ClassDefinition ctx){}
    public virtual void EnterParameterDeclaration(ParameterDeclaration ctx){}
    public virtual void LeaveParameterDeclaration(ParameterDeclaration ctx){}
    public virtual void EnterParameterSignature(ParameterSignature ctx){}
    public virtual void LeaveParameterSignature(ParameterSignature ctx){}
    public virtual void EnterMethodSignature(MethodSignature ctx){}
    public virtual void LeaveMethodSignature(MethodSignature ctx){}
    public virtual void EnterMethodHeader(MethodHeader ctx){}
    public virtual void LeaveMethodHeader(MethodHeader ctx){}
    public virtual void EnterMethodRef(MethodRef ctx){}
    public virtual void LeaveMethodRef(MethodRef ctx){}
    public virtual void EnterMethodImpl(MethodImpl ctx){}
    public virtual void LeaveMethodImpl(MethodImpl ctx){}
    public virtual void EnterMethodDefinition(MethodDefinition ctx){}
    public virtual void LeaveMethodDefinition(MethodDefinition ctx){}
    public virtual void EnterTypeReference(TypeReference ctx){}
    public virtual void LeaveTypeReference(TypeReference ctx){}
    public virtual void EnterMemberRef(MemberRef ctx){}
    public virtual void LeaveMemberRef(MemberRef ctx){}
    public virtual void EnterFieldDefinition(FieldDefinition ctx){}
    public virtual void LeaveFieldDefinition(FieldDefinition ctx){}
    public virtual void EnterPropertyDefinition(PropertyDefinition ctx){}
    public virtual void LeavePropertyDefinition(PropertyDefinition ctx){}
    public virtual void EnterLoadInstruction(LoadInstruction ctx){}
    public virtual void LeaveLoadInstruction(LoadInstruction ctx){}
    public virtual void EnterStoreInstruction(StoreInstruction ctx){}
    public virtual void LeaveStoreInstruction(StoreInstruction ctx){}
    public virtual void EnterArithmeticInstruction(ArithmeticInstruction ctx){}
    public virtual void LeaveArithmeticInstruction(ArithmeticInstruction ctx){}
    public virtual void EnterBranchInstruction(BranchInstruction ctx){}
    public virtual void LeaveBranchInstruction(BranchInstruction ctx){}
    public virtual void EnterCallInstruction(CallInstruction ctx){}
    public virtual void LeaveCallInstruction(CallInstruction ctx){}
    public virtual void EnterStackInstruction(StackInstruction ctx){}
    public virtual void LeaveStackInstruction(StackInstruction ctx){}
    public virtual void EnterReturnInstruction(ReturnInstruction ctx){}
    public virtual void LeaveReturnInstruction(ReturnInstruction ctx){}
    public virtual void EnterLabelInstruction(LabelInstruction ctx){}
    public virtual void LeaveLabelInstruction(LabelInstruction ctx){}
    public virtual void EnterInstructionSequence(InstructionSequence ctx){}
    public virtual void LeaveInstructionSequence(InstructionSequence ctx){}
    public virtual void EnterInstructionStatement(InstructionStatement ctx){}
    public virtual void LeaveInstructionStatement(InstructionStatement ctx){}
    public virtual void EnterBlock(Block ctx){}
    public virtual void LeaveBlock(Block ctx){}
}


public interface IAstRecursiveDescentVisitor
{
    public AstThing Visit(AstThing ctx);
    public AssemblyDeclaration VisitAssemblyDeclaration(AssemblyDeclaration ctx);
    public AssemblyReference VisitAssemblyReference(AssemblyReference ctx);
    public ModuleDeclaration VisitModuleDeclaration(ModuleDeclaration ctx);
    public Version VisitVersion(Version ctx);
    public ClassDefinition VisitClassDefinition(ClassDefinition ctx);
    public ParameterDeclaration VisitParameterDeclaration(ParameterDeclaration ctx);
    public ParameterSignature VisitParameterSignature(ParameterSignature ctx);
    public MethodSignature VisitMethodSignature(MethodSignature ctx);
    public MethodHeader VisitMethodHeader(MethodHeader ctx);
    public MethodRef VisitMethodRef(MethodRef ctx);
    public MethodImpl VisitMethodImpl(MethodImpl ctx);
    public MethodDefinition VisitMethodDefinition(MethodDefinition ctx);
    public TypeReference VisitTypeReference(TypeReference ctx);
    public MemberRef VisitMemberRef(MemberRef ctx);
    public FieldDefinition VisitFieldDefinition(FieldDefinition ctx);
    public PropertyDefinition VisitPropertyDefinition(PropertyDefinition ctx);
    public LoadInstruction VisitLoadInstruction(LoadInstruction ctx);
    public StoreInstruction VisitStoreInstruction(StoreInstruction ctx);
    public ArithmeticInstruction VisitArithmeticInstruction(ArithmeticInstruction ctx);
    public BranchInstruction VisitBranchInstruction(BranchInstruction ctx);
    public CallInstruction VisitCallInstruction(CallInstruction ctx);
    public StackInstruction VisitStackInstruction(StackInstruction ctx);
    public ReturnInstruction VisitReturnInstruction(ReturnInstruction ctx);
    public LabelInstruction VisitLabelInstruction(LabelInstruction ctx);
    public InstructionSequence VisitInstructionSequence(InstructionSequence ctx);
    public InstructionStatement VisitInstructionStatement(InstructionStatement ctx);
    public Block VisitBlock(Block ctx);
}

public class DefaultRecursiveDescentVisitor : IAstRecursiveDescentVisitor
{
    public virtual AstThing Visit(AstThing ctx){
        if(ctx == null) return ctx;
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

            { } node => null,
        };
    }

    public virtual AssemblyDeclaration VisitAssemblyDeclaration(AssemblyDeclaration ctx)
    {
        List<il_ast.AssemblyReference> tmpAssemblyReferences = [];
        tmpAssemblyReferences.AddRange(ctx.AssemblyReferences.Select(x => (il_ast.AssemblyReference)Visit(x)));
     return ctx with {
         Version = (il_ast.Version)Visit((AstThing)ctx.Version)
        ,PrimeModule = (il_ast.ModuleDeclaration)Visit((AstThing)ctx.PrimeModule)
        ,AssemblyReferences = tmpAssemblyReferences
        };
    }
    public virtual AssemblyReference VisitAssemblyReference(AssemblyReference ctx)
    {
     return ctx with {
         Version = (il_ast.Version)Visit((AstThing)ctx.Version)
        };
    }
    public virtual ModuleDeclaration VisitModuleDeclaration(ModuleDeclaration ctx)
    {
        List<il_ast.ClassDefinition> tmpClasses = [];
        tmpClasses.AddRange(ctx.Classes.Select(x => (il_ast.ClassDefinition)Visit(x)));
        List<il_ast.MethodDefinition> tmpFunctions = [];
        tmpFunctions.AddRange(ctx.Functions.Select(x => (il_ast.MethodDefinition)Visit(x)));
     return ctx with {
         Classes = tmpClasses
        ,Functions = tmpFunctions
        };
    }
    public virtual Version VisitVersion(Version ctx)
    {
     return ctx with {
        };
    }
    public virtual ClassDefinition VisitClassDefinition(ClassDefinition ctx)
    {
        List<il_ast.FieldDefinition> tmpFields = [];
        tmpFields.AddRange(ctx.Fields.Select(x => (il_ast.FieldDefinition)Visit(x)));
        List<il_ast.PropertyDefinition> tmpProperties = [];
        tmpProperties.AddRange(ctx.Properties.Select(x => (il_ast.PropertyDefinition)Visit(x)));
        List<il_ast.MethodDefinition> tmpMethods = [];
        tmpMethods.AddRange(ctx.Methods.Select(x => (il_ast.MethodDefinition)Visit(x)));
        List<il_ast.ClassDefinition> tmpBaseClasses = [];
        tmpBaseClasses.AddRange(ctx.BaseClasses.Select(x => (il_ast.ClassDefinition)Visit(x)));
     return ctx with {
         Fields = tmpFields
        ,Properties = tmpProperties
        ,Methods = tmpMethods
        ,BaseClasses = tmpBaseClasses
        ,ParentAssembly = (il_ast.AssemblyDeclaration)Visit((AstThing)ctx.ParentAssembly)
        };
    }
    public virtual ParameterDeclaration VisitParameterDeclaration(ParameterDeclaration ctx)
    {
     return ctx with {
        };
    }
    public virtual ParameterSignature VisitParameterSignature(ParameterSignature ctx)
    {
     return ctx with {
         TypeReference = (il_ast.TypeReference)Visit((AstThing)ctx.TypeReference)
        };
    }
    public virtual MethodSignature VisitMethodSignature(MethodSignature ctx)
    {
        List<il_ast.ParameterSignature> tmpParameterSignatures = [];
        tmpParameterSignatures.AddRange(ctx.ParameterSignatures.Select(x => (il_ast.ParameterSignature)Visit(x)));
     return ctx with {
         ParameterSignatures = tmpParameterSignatures
        ,ReturnTypeSignature = (il_ast.TypeReference)Visit((AstThing)ctx.ReturnTypeSignature)
        };
    }
    public virtual MethodHeader VisitMethodHeader(MethodHeader ctx)
    {
     return ctx with {
        };
    }
    public virtual MethodRef VisitMethodRef(MethodRef ctx)
    {
     return ctx with {
         ClassDefinition = (il_ast.ClassDefinition)Visit((AstThing)ctx.ClassDefinition)
        ,Sig = (il_ast.MethodSignature)Visit((AstThing)ctx.Sig)
        ,Field = (il_ast.FieldDefinition)Visit((AstThing)ctx.Field)
        };
    }
    public virtual MethodImpl VisitMethodImpl(MethodImpl ctx)
    {
     return ctx with {
         Body = (il_ast.Block)Visit((AstThing)ctx.Body)
        };
    }
    public virtual MethodDefinition VisitMethodDefinition(MethodDefinition ctx)
    {
     return ctx with {
         Header = (il_ast.MethodHeader)Visit((AstThing)ctx.Header)
        ,Signature = (il_ast.MethodSignature)Visit((AstThing)ctx.Signature)
        ,Impl = (il_ast.MethodImpl)Visit((AstThing)ctx.Impl)
        ,TheType = (il_ast.TypeReference)Visit((AstThing)ctx.TheType)
        ,ParentClass = (il_ast.ClassDefinition)Visit((AstThing)ctx.ParentClass)
        ,AssociatedProperty = (il_ast.PropertyDefinition)Visit((AstThing)ctx.AssociatedProperty)
        };
    }
    public virtual TypeReference VisitTypeReference(TypeReference ctx)
    {
     return ctx with {
        };
    }
    public virtual MemberRef VisitMemberRef(MemberRef ctx)
    {
     return ctx with {
         ClassDefinition = (il_ast.ClassDefinition)Visit((AstThing)ctx.ClassDefinition)
        ,Sig = (il_ast.MethodSignature)Visit((AstThing)ctx.Sig)
        ,Field = (il_ast.FieldDefinition)Visit((AstThing)ctx.Field)
        };
    }
    public virtual FieldDefinition VisitFieldDefinition(FieldDefinition ctx)
    {
     return ctx with {
         TheType = (il_ast.TypeReference)Visit((AstThing)ctx.TheType)
        ,ParentClass = (il_ast.ClassDefinition)Visit((AstThing)ctx.ParentClass)
        ,AssociatedProperty = (il_ast.PropertyDefinition)Visit((AstThing)ctx.AssociatedProperty)
        };
    }
    public virtual PropertyDefinition VisitPropertyDefinition(PropertyDefinition ctx)
    {
     return ctx with {
         FieldDefinition = (il_ast.FieldDefinition)Visit((AstThing)ctx.FieldDefinition)
        ,TheType = (il_ast.TypeReference)Visit((AstThing)ctx.TheType)
        ,ParentClass = (il_ast.ClassDefinition)Visit((AstThing)ctx.ParentClass)
        ,AssociatedProperty = (il_ast.PropertyDefinition)Visit((AstThing)ctx.AssociatedProperty)
        };
    }
    public virtual LoadInstruction VisitLoadInstruction(LoadInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual StoreInstruction VisitStoreInstruction(StoreInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual ArithmeticInstruction VisitArithmeticInstruction(ArithmeticInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual BranchInstruction VisitBranchInstruction(BranchInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual CallInstruction VisitCallInstruction(CallInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual StackInstruction VisitStackInstruction(StackInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual ReturnInstruction VisitReturnInstruction(ReturnInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual LabelInstruction VisitLabelInstruction(LabelInstruction ctx)
    {
     return ctx with {
        };
    }
    public virtual InstructionSequence VisitInstructionSequence(InstructionSequence ctx)
    {
        List<il_ast.CilInstruction> tmpInstructions = [];
        tmpInstructions.AddRange(ctx.Instructions.Select(x => (il_ast.CilInstruction)Visit(x)));
     return ctx with {
         Instructions = tmpInstructions
        };
    }
    public virtual InstructionStatement VisitInstructionStatement(InstructionStatement ctx)
    {
     return ctx with {
         Instructions = (il_ast.InstructionSequence)Visit((AstThing)ctx.Instructions)
        };
    }
    public virtual Block VisitBlock(Block ctx)
    {
     return ctx with {
        };
    }

}
