using FluentAssertions;
using Xunit;
using il_ast;
using code_generator;
using System.Linq;

namespace ast_tests;

/// <summary>
/// End-to-end integration tests demonstrating instruction-level IL generation
/// from Fifth language constructs to CIL instructions
/// </summary>
public class EndToEndInstructionLevelTests
{
    private readonly InstructionSequenceGenerator _generator = new();

    [Fact]
    public void EndToEnd_SimpleArithmetic_ShouldGenerateCorrectInstructionSequence()
    {
        // Arrange - Create a simple arithmetic expression: 5 + 3
        var left = new IntLiteral(5);
        var right = new IntLiteral(3);
        var addExpression = new BinaryExpression("+", left, right);

        // Act - Generate instruction sequence
        var sequence = _generator.GenerateExpression(addExpression);

        // Assert - Verify the instruction sequence matches expected CIL
        sequence.Instructions.Should().HaveCount(3);

        // Should load first operand
        var instr1 = sequence.Instructions[0] as LoadInstruction;
        instr1.Should().NotBeNull();
        instr1!.Opcode.Should().Be("ldc.i4");
        instr1.Value.Should().Be(5);

        // Should load second operand  
        var instr2 = sequence.Instructions[1] as LoadInstruction;
        instr2.Should().NotBeNull();
        instr2!.Opcode.Should().Be("ldc.i4");
        instr2.Value.Should().Be(3);

        // Should perform addition
        var instr3 = sequence.Instructions[2] as ArithmeticInstruction;
        instr3.Should().NotBeNull();
        instr3!.Opcode.Should().Be("add");
    }

    [Fact]
    public void EndToEnd_IfElseStatement_ShouldGenerateBranchInstructions()
    {
        // Arrange - Create if (x > 0) { y = 1; } else { y = 2; }
        var condition = new BinaryExpression(">", 
            new VariableReferenceExpression { Name = "x" },
            new IntLiteral(0));

        var thenStatement = new VariableAssignmentStatement
        {
            LHS = "y",
            RHS = new IntLiteral(1)
        };

        var elseStatement = new VariableAssignmentStatement
        {
            LHS = "y", 
            RHS = new IntLiteral(2)
        };

        var ifStatement = new IfStatement
        {
            Conditional = condition,
            IfBlock = new Block { Statements = { thenStatement } },
            ElseBlock = new Block { Statements = { elseStatement } }
        };

        // Act
        var sequence = _generator.GenerateIfStatement(ifStatement);

        // Assert - Verify branch-based control flow
        sequence.Instructions.Should().NotBeEmpty();

        // Should load variable x
        var loadX = sequence.Instructions.OfType<LoadInstruction>().First();
        loadX.Opcode.Should().Be("ldloc");
        loadX.Value.Should().Be("x");

        // Should have branch instruction
        var branchInstr = sequence.Instructions.OfType<BranchInstruction>().FirstOrDefault();
        branchInstr.Should().NotBeNull();
        branchInstr!.Opcode.Should().Be("brfalse");
        branchInstr.TargetLabel.Should().NotBeNullOrEmpty();

        // Should have labels for control flow
        var labels = sequence.Instructions.OfType<LabelInstruction>().ToList();
        labels.Should().HaveCount(2); // false label and end label

        // Should have store instructions for variable assignments
        var storeInstructions = sequence.Instructions.OfType<StoreInstruction>().ToList();
        storeInstructions.Should().HaveCount(2); // one for each branch
        storeInstructions.Should().AllSatisfy(store => 
        {
            store.Opcode.Should().Be("stloc");
            store.Target.Should().Be("y");
        });
    }

    [Fact]
    public void EndToEnd_ComplexExpression_ShouldGenerateStackBasedInstructions()
    {
        // Arrange - Create: (a + b) * (c - d)
        var a = new VariableReferenceExpression { Name = "a" };
        var b = new VariableReferenceExpression { Name = "b" };
        var c = new VariableReferenceExpression { Name = "c" };
        var d = new VariableReferenceExpression { Name = "d" };

        var leftExpression = new BinaryExpression("+", a, b);   // a + b
        var rightExpression = new BinaryExpression("-", c, d);  // c - d
        var multiplyExpression = new BinaryExpression("*", leftExpression, rightExpression); // (a + b) * (c - d)

        // Act
        var sequence = _generator.GenerateExpression(multiplyExpression);

        // Assert - Verify proper stack-based evaluation order
        sequence.Instructions.Should().HaveCount(7); // 4 loads + 2 arithmetic + 1 multiply

        // Should load variables in correct order for stack-based evaluation
        var loadInstructions = sequence.Instructions.OfType<LoadInstruction>().ToList();
        loadInstructions.Should().HaveCount(4);
        loadInstructions[0].Value.Should().Be("a"); // Load a
        loadInstructions[1].Value.Should().Be("b"); // Load b  
        loadInstructions[2].Value.Should().Be("c"); // Load c
        loadInstructions[3].Value.Should().Be("d"); // Load d

        var arithInstructions = sequence.Instructions.OfType<ArithmeticInstruction>().ToList();
        arithInstructions.Should().HaveCount(3);
        arithInstructions[0].Opcode.Should().Be("add"); // a + b
        arithInstructions[1].Opcode.Should().Be("sub"); // c - d
        arithInstructions[2].Opcode.Should().Be("mul"); // (a + b) * (c - d)
    }

    [Fact]
    public void EndToEnd_MethodWithInstructions_ShouldAllowInstructionStatements()
    {
        // Arrange - Create a method body using instruction statements
        var instructionSequence = new InstructionSequence();
        instructionSequence.Add(new LoadInstruction("ldc.i4", 10));
        instructionSequence.Add(new LoadInstruction("ldc.i4", 20));
        instructionSequence.Add(new ArithmeticInstruction("add"));
        instructionSequence.Add(new ReturnInstruction());

        var instructionStatement = new InstructionStatement
        {
            Instructions = instructionSequence
        };

        var methodBody = new Block
        {
            Statements = { instructionStatement }
        };

        // Act & Assert - Verify the method body can contain instruction statements
        methodBody.Statements.Should().HaveCount(1);
        var stmt = methodBody.Statements[0] as InstructionStatement;
        stmt.Should().NotBeNull();
        stmt!.Instructions.Instructions.Should().HaveCount(4);

        // Verify the instructions are properly structured
        stmt.Instructions.Instructions[0].Should().BeOfType<LoadInstruction>();
        stmt.Instructions.Instructions[1].Should().BeOfType<LoadInstruction>();
        stmt.Instructions.Instructions[2].Should().BeOfType<ArithmeticInstruction>();
        stmt.Instructions.Instructions[3].Should().BeOfType<ReturnInstruction>();
    }

    [Fact]
    public void EndToEnd_NestedExpressions_ShouldMaintainEvaluationOrder()
    {
        // Arrange - Create: x + (y * z)
        var x = new VariableReferenceExpression { Name = "x" };
        var y = new VariableReferenceExpression { Name = "y" };
        var z = new VariableReferenceExpression { Name = "z" };

        var multiply = new BinaryExpression("*", y, z);    // y * z
        var add = new BinaryExpression("+", x, multiply);  // x + (y * z)

        // Act
        var sequence = _generator.GenerateExpression(add);

        // Assert - Verify correct evaluation order for nested expressions
        sequence.Instructions.Should().HaveCount(5); // 3 loads + 1 multiply + 1 add

        var instructions = sequence.Instructions.ToList();

        // First: load x for the outer addition
        var instr1 = instructions[0] as LoadInstruction;
        instr1!.Value.Should().Be("x");

        // Then: evaluate the nested expression (y * z)
        var instr2 = instructions[1] as LoadInstruction;
        instr2!.Value.Should().Be("y");

        var instr3 = instructions[2] as LoadInstruction;
        instr3!.Value.Should().Be("z");

        var instr4 = instructions[3] as ArithmeticInstruction;
        instr4!.Opcode.Should().Be("mul");

        // Finally: perform the outer addition
        var instr5 = instructions[4] as ArithmeticInstruction;
        instr5!.Opcode.Should().Be("add");
    }
}