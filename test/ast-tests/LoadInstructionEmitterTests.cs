using FluentAssertions;
using code_generator;
using code_generator.InstructionEmitter;
using il_ast;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace ast_tests;

/// <summary>
/// Unit tests for LoadInstructionEmitter to verify it emits instructions correctly
/// with explicit state passing through EmissionContext
/// </summary>
public class LoadInstructionEmitterTests
{
    [Test]
    public void EmitLoadConstantI4_ShouldEmitCorrectInstruction()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new LoadInstruction("ldc.i4", 42);
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Instruction should have been emitted");
    }
    
    [Test]
    public void EmitLoadConstantR4_ShouldHandleFloatValues()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new LoadInstruction("ldc.r4", 3.14f);
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Float load instruction should have been emitted");
    }
    
    [Test]
    public void EmitLoadString_ShouldRemoveQuotesAndEmit()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new LoadInstruction("ldstr", "\"Hello World\"");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "String load instruction should have been emitted");
    }
    
    [Test]
    public void EmitLoadNull_ShouldClearTrackingState()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        context.MetadataManager.LastLoadedLocal = "someVar";
        context.MetadataManager.LastLoadedParam = "someParam";
        var instruction = new LoadInstruction("ldnull");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        context.MetadataManager.LastLoadedLocal.Should().BeNull("State should be cleared");
        context.MetadataManager.LastLoadedParam.Should().BeNull("State should be cleared");
        context.MetadataManager.PendingStackTopClassType.Should().BeNull("State should be cleared");
    }
    
    [Test]
    public void EmitLoadLocal_WithValidVariable_ShouldLoadAndTrackState()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "var1", "var2", "var3" };
        var instruction = new LoadInstruction("ldloc", "var2");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Load local instruction should have been emitted");
        context.MetadataManager.LastLoadedLocal.Should().Be("var2", "Should track loaded local");
    }
    
    [Test]
    public void EmitLoadLocal_WithUnknownVariable_ShouldEmitFallback()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "var1", "var2" };
        var instruction = new LoadInstruction("ldloc", "unknownVar");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Fallback instruction should have been emitted");
        context.MetadataManager.LastLoadedLocal.Should().BeNull("Unknown var should clear state");
    }
    
    [Test]
    public void EmitLoadArgument_WithValidArgument_ShouldLoadAndTrackState()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentParamIndexMap = new Dictionary<string, int> { { "arg1", 0 }, { "arg2", 1 } };
        var instruction = new LoadInstruction("ldarg", "arg2");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Load argument instruction should have been emitted");
        context.MetadataManager.LastLoadedParam.Should().Be("arg2", "Should track loaded parameter");
        context.MetadataManager.LastLoadedLocal.Should().BeNull("Local should be cleared when loading param");
    }
    
    [Test]
    public void EmitDup_ShouldEmitDupInstruction()
    {
        // Arrange
        var emitter = new LoadInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new LoadInstruction("dup");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Dup instruction should have been emitted");
    }
    
    // Helper method to create a test context
    private EmissionContext CreateTestContext()
    {
        var metadataBuilder = new MetadataBuilder();
        var metadataManager = new MetadataManager();
        
        // Add a minimal assembly reference for testing
        var sysRuntimeRef = metadataBuilder.AddAssemblyReference(
            metadataBuilder.GetOrAddString("System.Runtime"),
            new System.Version(8, 0, 0, 0),
            default, default, default, default);
        metadataManager.RegisterAssemblyReference("system.runtime", sysRuntimeRef);
        
        // Create a System.Int32 type reference for testing
        var int32TypeRef = metadataBuilder.AddTypeReference(
            sysRuntimeRef,
            metadataBuilder.GetOrAddString("System"),
            metadataBuilder.GetOrAddString("Int32"));
        metadataManager.SystemInt32TypeRef = int32TypeRef;
        
        return new EmissionContext
        {
            MetadataBuilder = metadataBuilder,
            MetadataManager = metadataManager
        };
    }
}
