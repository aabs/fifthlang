using FluentAssertions;
using Xunit;
using il_ast;
using ast;
using code_generator;
using System.Linq;

namespace ast_tests;

/// <summary>
/// Tests for the new instruction-level IL generation functionality
/// </summary>
public class InstructionLevelILTests
{
    private readonly AstToIlTransformationVisitor _generator = new();

    [Fact]
    public void GenerateExpression_ForIntLiteral_ShouldProduceLdcI4Instruction()
    {
        // Arrange
        var intLiteral = new Int32LiteralExp { Value = 42 };

        // Act
        var sequence = _generator.GenerateExpression(intLiteral);

        // Assert
        sequence.Instructions.Should().HaveCount(1);
        var instruction = sequence.Instructions[0] as LoadInstruction;
        instruction.Should().NotBeNull();
        instruction!.Opcode.Should().Be("ldc.i4");
        instruction.Value.Should().Be(42);
    }

    [Fact]
    public void GenerateExpression_ForBinaryAddition_ShouldProduceCorrectInstructionSequence()
    {
        // Arrange
        var left = new Int32LiteralExp { Value = 10 };
        var right = new Int32LiteralExp { Value = 20 };
        var binaryExp = new BinaryExp 
        { 
            LHS = left, 
            RHS = right, 
            Operator = Operator.ArithmeticAdd 
        };

        // Act
        var sequence = _generator.GenerateExpression(binaryExp);

        // Assert
        sequence.Instructions.Should().HaveCount(3);
        
        // First instruction should load left operand
        var firstInstr = sequence.Instructions[0] as LoadInstruction;
        firstInstr.Should().NotBeNull();
        firstInstr!.Opcode.Should().Be("ldc.i4");
        firstInstr.Value.Should().Be(10);
        
        // Second instruction should load right operand
        var secondInstr = sequence.Instructions[1] as LoadInstruction;
        secondInstr.Should().NotBeNull();
        secondInstr!.Opcode.Should().Be("ldc.i4");
        secondInstr.Value.Should().Be(20);
        
        // Third instruction should be the add operation
        var thirdInstr = sequence.Instructions[2] as ArithmeticInstruction;
        thirdInstr.Should().NotBeNull();
        thirdInstr!.Opcode.Should().Be("add");
    }

    [Fact]
    public void GenerateStatement_ForVariableAssignment_ShouldProduceLoadAndStoreInstructions()
    {
        // Arrange
        var value = new Int32LiteralExp { Value = 100 };
        var varRef = new VarRefExp { VarName = "myVar" };
        var assignment = new AssignmentStatement
        {
            LValue = varRef,
            RValue = value
        };

        // Act
        var sequence = _generator.GenerateStatement(assignment);

        // Assert
        sequence.Instructions.Should().HaveCount(2);
        
        // First instruction should load the value
        var loadInstr = sequence.Instructions[0] as LoadInstruction;
        loadInstr.Should().NotBeNull();
        loadInstr!.Opcode.Should().Be("ldc.i4");
        loadInstr.Value.Should().Be(100);
        
        // Second instruction should store to variable
        var storeInstr = sequence.Instructions[1] as StoreInstruction;
        storeInstr.Should().NotBeNull();
        storeInstr!.Opcode.Should().Be("stloc");
        storeInstr.Target.Should().Be("myVar");
    }

    [Fact]
    public void GenerateIfStatement_ShouldProduceBranchInstructions()
    {
        // Arrange
        var condition = new BooleanLiteralExp { Value = true };
        var varRef = new VarRefExp { VarName = "result" };
        var value = new Int32LiteralExp { Value = 1 };
        var thenStatement = new AssignmentStatement
        {
            LValue = varRef,
            RValue = value
        };
        var ifStatement = new IfElseStatement
        {
            Condition = condition,
            ThenBlock = new BlockStatement { Statements = { thenStatement } },
            ElseBlock = new BlockStatement { Statements = { } }
        };

        // Act
        var sequence = _generator.GenerateIfStatement(ifStatement);

        // Assert
        sequence.Instructions.Should().NotBeEmpty();
        
        // Should contain condition evaluation
        var loadInstr = sequence.Instructions[0] as LoadInstruction;
        loadInstr.Should().NotBeNull();
        loadInstr!.Opcode.Should().Be("ldc.i4");
        loadInstr.Value.Should().Be(1); // true = 1
        
        // Should contain a branch instruction
        var branchInstr = sequence.Instructions.OfType<BranchInstruction>().FirstOrDefault();
        branchInstr.Should().NotBeNull();
        branchInstr!.Opcode.Should().Be("brfalse");
        
        // Should contain label instructions
        var labelInstructions = sequence.Instructions.OfType<LabelInstruction>().ToList();
        labelInstructions.Should().HaveCount(2); // false label and end label
    }

    [Fact]
    public void InstructionSequence_CanBeConvertedToStatements()
    {
        // Arrange
        var sequence = new InstructionSequence();
        sequence.Add(new LoadInstruction("ldc.i4", 42));
        sequence.Add(new ArithmeticInstruction("add"));
        sequence.Add(new ReturnInstruction());
        
        var instructionStatement = new InstructionStatement
        {
            Instructions = sequence
        };

        // Act & Assert - verify the instruction statement can be created and contains the sequence
        instructionStatement.Should().NotBeNull();
        instructionStatement.Instructions.Should().NotBeNull();
        instructionStatement.Instructions.Instructions.Should().HaveCount(3);
        
        var loadInstr = instructionStatement.Instructions.Instructions[0] as LoadInstruction;
        loadInstr.Should().NotBeNull();
        loadInstr!.Opcode.Should().Be("ldc.i4");
        loadInstr.Value.Should().Be(42);
        
        var arithInstr = instructionStatement.Instructions.Instructions[1] as ArithmeticInstruction;
        arithInstr.Should().NotBeNull();
        arithInstr!.Opcode.Should().Be("add");
        
        var retInstr = instructionStatement.Instructions.Instructions[2] as ReturnInstruction;
        retInstr.Should().NotBeNull();
        retInstr!.Opcode.Should().Be("ret");
    }

    [Fact]
    public void InstructionSequence_ShouldAllowChaining()
    {
        // Arrange
        var sequence = new InstructionSequence();

        // Act
        sequence.Add(new LoadInstruction("ldc.i4", 1));
        sequence.Add(new LoadInstruction("ldc.i4", 2));
        sequence.Add(new ArithmeticInstruction("add"));

        // Assert
        sequence.Instructions.Should().HaveCount(3);
        sequence.Instructions[0].Should().BeOfType<LoadInstruction>();
        sequence.Instructions[1].Should().BeOfType<LoadInstruction>();
        sequence.Instructions[2].Should().BeOfType<ArithmeticInstruction>();
    }
}