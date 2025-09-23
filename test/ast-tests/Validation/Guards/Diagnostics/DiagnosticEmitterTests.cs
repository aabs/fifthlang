using FluentAssertions;
using TUnit.Core;
using compiler;
using compiler.Validation.GuardValidation.Diagnostics;
using compiler.Validation.GuardValidation.Infrastructure;
using ast_tests.Validation.Guards.Infrastructure;

namespace ast_tests.Validation.Guards.Diagnostics;

public class DiagnosticEmitterTests
{
    [Test]
    public void Reset_ShouldClearAllDiagnostics()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);
        emitter.EmitIncompleteError(group);

        // Act
        emitter.Reset();

        // Assert
        emitter.Diagnostics.Should().BeEmpty();
    }

    [Test]
    public void EmitIncompleteError_ShouldEmitE1001Error()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 2);

        // Act
        emitter.EmitIncompleteError(group);

        // Assert
        emitter.Diagnostics.Should().HaveCount(1);
        var diagnostic = emitter.Diagnostics[0];
        diagnostic.Level.Should().Be(DiagnosticLevel.Error);
        diagnostic.Message.Should().Contain("GUARD_INCOMPLETE (E1001)");
        diagnostic.Message.Should().Contain("testFunc/2");
    }

    [Test]
    public void EmitUnreachableWarning_ShouldEmitW1002WarningWithNote()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);
        var unreachable = new MockOverloadableFunction();
        var covering = new MockOverloadableFunction();

        // Act
        emitter.EmitUnreachableWarning(group, unreachable, covering, 2, 1);

        // Assert
        emitter.Diagnostics.Should().HaveCount(2);

        var warning = emitter.Diagnostics[0];
        warning.Level.Should().Be(DiagnosticLevel.Warning);
        warning.Message.Should().Contain("GUARD_UNREACHABLE (W1002)");
        warning.Message.Should().Contain("Overload #2");

        var note = emitter.Diagnostics[1];
        note.Level.Should().Be(DiagnosticLevel.Info);
        note.Message.Should().Contain("note:");
        note.Message.Should().Contain("overload #2 unreachable due to earlier coverage by overload #1");
    }

    [Test]
    public void EmitBaseNotLastError_ShouldEmitE1004ErrorWithNote()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);

        // Act
        emitter.EmitBaseNotLastError(group, 0, 2);

        // Assert
        emitter.Diagnostics.Should().HaveCount(2);

        var error = emitter.Diagnostics[0];
        error.Level.Should().Be(DiagnosticLevel.Error);
        error.Message.Should().Contain("GUARD_BASE_NOT_LAST (E1004)");
        error.Message.Should().Contain("subsequent overload at #2");

        var note = emitter.Diagnostics[1];
        note.Level.Should().Be(DiagnosticLevel.Info);
        note.Message.Should().Contain("note:");
        note.Message.Should().Contain("overload #2 invalid because base overload terminates overloading at #1");
    }

    [Test]
    public void EmitMultipleBaseError_ShouldEmitE1005ErrorWithNotes()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);
        var baseOverloads = new List<ast.IOverloadableFunction>
        {
            new MockOverloadableFunction(),
            new MockOverloadableFunction(),
            new MockOverloadableFunction()
        };

        // Act
        emitter.EmitMultipleBaseError(group, baseOverloads);

        // Assert
        emitter.Diagnostics.Should().HaveCount(3); // 1 error + 2 notes

        var error = emitter.Diagnostics[0];
        error.Level.Should().Be(DiagnosticLevel.Error);
        error.Message.Should().Contain("GUARD_MULTIPLE_BASE (E1005)");

        // Should have notes for the extra base overloads
        emitter.Diagnostics.Skip(1).Should().AllSatisfy(d =>
        {
            d.Level.Should().Be(DiagnosticLevel.Info);
            d.Message.Should().Contain("note:");
        });
    }

    [Test]
    public void EmitOverloadCountWarning_ShouldEmitW1101Warning()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);
        // Simulate adding 35 overloads
        for (int i = 0; i < 35; i++)
        {
            group.AddOverload(new MockOverloadableFunction());
        }

        // Act
        emitter.EmitOverloadCountWarning(group);

        // Assert
        emitter.Diagnostics.Should().HaveCount(1);
        var warning = emitter.Diagnostics[0];
        warning.Level.Should().Be(DiagnosticLevel.Warning);
        warning.Message.Should().Contain("GUARD_OVERLOAD_COUNT (W1101)");
        warning.Message.Should().Contain("(found 35)");
    }

    [Test]
    public void EmitUnknownExplosionWarning_ShouldEmitW1102Warning()
    {
        // Arrange
        var emitter = new DiagnosticEmitter();
        var group = new FunctionGroup("testFunc", 1);

        // Act
        emitter.EmitUnknownExplosionWarning(group, 75);

        // Assert
        emitter.Diagnostics.Should().HaveCount(1);
        var warning = emitter.Diagnostics[0];
        warning.Level.Should().Be(DiagnosticLevel.Warning);
        warning.Message.Should().Contain("GUARD_UNKNOWN_EXPLOSION (W1102)");
        warning.Message.Should().Contain("(75%)");
    }
}