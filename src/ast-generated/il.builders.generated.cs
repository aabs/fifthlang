namespace il_ast_generated;
using ast_generated;
using il_ast;
using System.Collections.Generic;
#nullable disable

public class AssemblyDeclarationBuilder : IBuilder<il_ast.AssemblyDeclaration>
{
    
    public il_ast.AssemblyDeclaration Build()
    {
        return new il_ast.AssemblyDeclaration(){
        };
    }
}
public class AssemblyReferenceBuilder : IBuilder<il_ast.AssemblyReference>
{
    
    public il_ast.AssemblyReference Build()
    {
        return new il_ast.AssemblyReference(){
        };
    }
}
public class ModuleDeclarationBuilder : IBuilder<il_ast.ModuleDeclaration>
{
    
    public il_ast.ModuleDeclaration Build()
    {
        return new il_ast.ModuleDeclaration(){
        };
    }
}
public class VersionBuilder : IBuilder<il_ast.Version>
{
    
    public il_ast.Version Build()
    {
        return new il_ast.Version(){
        };
    }
}
public class ClassDefinitionBuilder : IBuilder<il_ast.ClassDefinition>
{
    
    public il_ast.ClassDefinition Build()
    {
        return new il_ast.ClassDefinition(){
        };
    }
}
public class ParameterDeclarationBuilder : IBuilder<il_ast.ParameterDeclaration>
{
    
    public il_ast.ParameterDeclaration Build()
    {
        return new il_ast.ParameterDeclaration(){
        };
    }
}
public class ParameterSignatureBuilder : IBuilder<il_ast.ParameterSignature>
{
    
    public il_ast.ParameterSignature Build()
    {
        return new il_ast.ParameterSignature(){
        };
    }
}
public class MethodSignatureBuilder : IBuilder<il_ast.MethodSignature>
{
    
    public il_ast.MethodSignature Build()
    {
        return new il_ast.MethodSignature(){
        };
    }
}
public class MethodHeaderBuilder : IBuilder<il_ast.MethodHeader>
{
    
    public il_ast.MethodHeader Build()
    {
        return new il_ast.MethodHeader(){
        };
    }
}
public class MethodRefBuilder : IBuilder<il_ast.MethodRef>
{
    
    public il_ast.MethodRef Build()
    {
        return new il_ast.MethodRef(){
        };
    }
}
public class MethodImplBuilder : IBuilder<il_ast.MethodImpl>
{
    
    public il_ast.MethodImpl Build()
    {
        return new il_ast.MethodImpl(){
        };
    }
}
public class MethodDefinitionBuilder : IBuilder<il_ast.MethodDefinition>
{
    
    public il_ast.MethodDefinition Build()
    {
        return new il_ast.MethodDefinition(){
        };
    }
}
public class TypeReferenceBuilder : IBuilder<il_ast.TypeReference>
{
    
    public il_ast.TypeReference Build()
    {
        return new il_ast.TypeReference(){
        };
    }
}
public class MemberRefBuilder : IBuilder<il_ast.MemberRef>
{
    
    public il_ast.MemberRef Build()
    {
        return new il_ast.MemberRef(){
        };
    }
}
public class FieldDefinitionBuilder : IBuilder<il_ast.FieldDefinition>
{
    
    public il_ast.FieldDefinition Build()
    {
        return new il_ast.FieldDefinition(){
        };
    }
}
public class PropertyDefinitionBuilder : IBuilder<il_ast.PropertyDefinition>
{
    
    public il_ast.PropertyDefinition Build()
    {
        return new il_ast.PropertyDefinition(){
        };
    }
}
public class LoadInstructionBuilder : IBuilder<il_ast.LoadInstruction>
{
    private System.Object _Value;
    
    public il_ast.LoadInstruction Build()
    {
        return new il_ast.LoadInstruction(){
             Value = this._Value // from LoadInstruction
        };
    }
    public LoadInstructionBuilder WithValue(System.Object value){
        _Value = value;
        return this;
    }

}
public class StoreInstructionBuilder : IBuilder<il_ast.StoreInstruction>
{
    private System.String _Target;
    
    public il_ast.StoreInstruction Build()
    {
        return new il_ast.StoreInstruction(){
             Target = this._Target // from StoreInstruction
        };
    }
    public StoreInstructionBuilder WithTarget(System.String value){
        _Target = value;
        return this;
    }

}
public class ArithmeticInstructionBuilder : IBuilder<il_ast.ArithmeticInstruction>
{
    
    public il_ast.ArithmeticInstruction Build()
    {
        return new il_ast.ArithmeticInstruction(){
        };
    }
}
public class BranchInstructionBuilder : IBuilder<il_ast.BranchInstruction>
{
    private System.String _TargetLabel;
    
    public il_ast.BranchInstruction Build()
    {
        return new il_ast.BranchInstruction(){
             TargetLabel = this._TargetLabel // from BranchInstruction
        };
    }
    public BranchInstructionBuilder WithTargetLabel(System.String value){
        _TargetLabel = value;
        return this;
    }

}
public class CallInstructionBuilder : IBuilder<il_ast.CallInstruction>
{
    private System.String _MethodSignature;
    
    public il_ast.CallInstruction Build()
    {
        return new il_ast.CallInstruction(){
             MethodSignature = this._MethodSignature // from CallInstruction
        };
    }
    public CallInstructionBuilder WithMethodSignature(System.String value){
        _MethodSignature = value;
        return this;
    }

}
public class StackInstructionBuilder : IBuilder<il_ast.StackInstruction>
{
    
    public il_ast.StackInstruction Build()
    {
        return new il_ast.StackInstruction(){
        };
    }
}
public class ReturnInstructionBuilder : IBuilder<il_ast.ReturnInstruction>
{
    
    public il_ast.ReturnInstruction Build()
    {
        return new il_ast.ReturnInstruction(){
        };
    }
}
public class LabelInstructionBuilder : IBuilder<il_ast.LabelInstruction>
{
    private System.String _Label;
    
    public il_ast.LabelInstruction Build()
    {
        return new il_ast.LabelInstruction(){
             Label = this._Label // from LabelInstruction
        };
    }
    public LabelInstructionBuilder WithLabel(System.String value){
        _Label = value;
        return this;
    }

}
public class InstructionSequenceBuilder : IBuilder<il_ast.InstructionSequence>
{
    
    public il_ast.InstructionSequence Build()
    {
        return new il_ast.InstructionSequence(){
        };
    }
}
public class InstructionStatementBuilder : IBuilder<il_ast.InstructionStatement>
{
    
    public il_ast.InstructionStatement Build()
    {
        return new il_ast.InstructionStatement(){
        };
    }
}
public class BlockBuilder : IBuilder<il_ast.Block>
{
    
    public il_ast.Block Build()
    {
        return new il_ast.Block(){
        };
    }
}

#nullable restore
