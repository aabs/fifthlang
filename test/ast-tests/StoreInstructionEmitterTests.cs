using FluentAssertions;
using code_generator;
using code_generator.InstructionEmitter;
using il_ast;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace ast_tests;

/// <summary>
/// Unit tests for StoreInstructionEmitter to verify it emits instructions correctly
/// with explicit state passing through EmissionContext
/// </summary>
public class StoreInstructionEmitterTests
{
    [Test]
    public void EmitStoreLocal_WithValidVariable_ShouldStoreAndUpdateState()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "var1", "var2", "var3" };
        var instruction = new StoreInstruction("stloc", "var2");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Store local instruction should have been emitted");
    }
    
    [Test]
    public void EmitStoreLocal_AfterNewobj_ShouldTrackObjectType()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "obj" };
        context.MetadataManager.LastWasNewobj = true;
        context.MetadataManager.PendingNewobjTypeName = "TestType";
        
        // Register a test type
        var typeDefHandle = context.MetadataBuilder.AddTypeDefinition(
            default,
            default,
            context.MetadataBuilder.GetOrAddString("TestType"),
            default,
            default,
            default);
        context.MetadataManager.RegisterType("TestType", typeDefHandle);
        
        var instruction = new StoreInstruction("stloc", "obj");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        context.MetadataManager.LocalVarClassTypeHandles.Should().ContainKey("obj", 
            "Object type should be tracked for the local variable");
        context.MetadataManager.LastWasNewobj.Should().BeFalse("State should be cleared after store");
    }
    
    [Test]
    public void EmitStoreLocal_WithUnknownVariable_ShouldEmitFallback()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "var1", "var2" };
        var instruction = new StoreInstruction("stloc", "unknownVar");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Fallback instruction should have been emitted");
    }
    
    [Test]
    public void EmitStoreArg_ShouldEmitPop()
    {
        // Arrange  
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new StoreInstruction("starg", "arg1");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Pop instruction should have been emitted for starg");
    }
    
    [Test]
    public void EmitStoreElementI4_ShouldEmitCorrectInstruction()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new StoreInstruction("stelem.i4");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Store element instruction should have been emitted");
    }
    
    [Test]
    public void EmitStoreField_WithUnknownField_ShouldEmitPops()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new StoreInstruction("stfld", "unknownField");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Pop instructions should have been emitted for unknown field");
    }
    
    [Test]
    public void EmitStoreStaticField_WithUnknownField_ShouldEmitPop()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        var instruction = new StoreInstruction("stsfld", "UnknownType::unknownField");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        builder.Count.Should().BeGreaterThan(0, "Pop instruction should have been emitted for unknown static field");
    }
    
    [Test]
    public void EmitStore_ShouldClearPendingState()
    {
        // Arrange
        var emitter = new StoreInstructionEmitter();
        var context = CreateTestContext();
        context.CurrentLocalVarNames = new List<string> { "var1" };
        context.MetadataManager.LastWasNewobj = true;
        context.MetadataManager.PendingNewobjTypeName = "TestType";
        var instruction = new StoreInstruction("stloc", "var1");
        var builder = new BlobBuilder();
        var controlFlow = new ControlFlowBuilder();
        var il = new InstructionEncoder(builder, controlFlow);
        
        // Act
        emitter.Emit(il, instruction, context);
        
        // Assert
        context.MetadataManager.LastWasNewobj.Should().BeFalse("LastWasNewobj should be cleared");
        context.MetadataManager.PendingNewobjTypeName.Should().BeNull("PendingNewobjTypeName should be cleared");
        context.MetadataManager.PendingStackTopClassType.Should().BeNull("PendingStackTopClassType should be cleared");
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
        
        return new EmissionContext
        {
            MetadataBuilder = metadataBuilder,
            MetadataManager = metadataManager
        };
    }
}
