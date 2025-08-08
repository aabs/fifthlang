using ast;
using ast_generated;
using ast_model.Symbols;
using ast_model.TypeSystem;
using Fifth.LangProcessingPhases;
using FluentAssertions;

namespace ast_tests;

public class VarRefResolverVisitorTests : VisitorTestsBase
{
    private readonly VarRefResolverVisitor _visitor = new();

    [Fact]
    public void TryResolve_WithValidVariableInScope_ReturnsTrue()
    {
        // Arrange
        var variableDecl = new VariableDecl
        {
            Name = "testVar",
            TypeName = TypeName.From("int"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        var functionDef = CreateFunctionDef("testFunc", "void");

        // Set up symbol table
        var symbol = new Symbol("testVar", SymbolKind.VarDeclStatement);
        var symbolTableEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            OriginatingAstThing = variableDecl,
            Annotations = []
        };

        functionDef.SymbolTable.Add(symbol, symbolTableEntry);

        // Act
        var result = _visitor.TryResolve("testVar", functionDef, out var resolvedDecl);

        // Assert
        result.Should().BeTrue();
        resolvedDecl.Should().NotBeNull();
        resolvedDecl.Should().Be(variableDecl);
    }

    [Fact]
    public void TryResolve_WithNullVarName_ReturnsFalse()
    {
        // Arrange
        var functionDef = CreateFunctionDef("testFunc", "void");

        // Act
        var result = _visitor.TryResolve(null, functionDef, out var resolvedDecl);

        // Assert
        result.Should().BeFalse();
        resolvedDecl.Should().BeNull();
    }

    [Fact]
    public void TryResolve_WithEmptyVarName_ReturnsFalse()
    {
        // Arrange
        var functionDef = CreateFunctionDef("testFunc", "void");

        // Act
        var result = _visitor.TryResolve("", functionDef, out var resolvedDecl);

        // Assert
        result.Should().BeFalse();
        resolvedDecl.Should().BeNull();
    }

    [Fact]
    public void TryResolve_WithNullScope_ReturnsFalse()
    {
        // Act
        var result = _visitor.TryResolve("testVar", null, out var resolvedDecl);

        // Assert
        result.Should().BeFalse();
        resolvedDecl.Should().BeNull();
    }

    [Fact]
    public void TryResolve_WithVariableNotInScope_ReturnsFalse()
    {
        // Arrange
        var functionDef = CreateFunctionDef("testFunc", "void");

        // Act
        var result = _visitor.TryResolve("nonExistentVar", functionDef, out var resolvedDecl);

        // Assert
        result.Should().BeFalse();
        resolvedDecl.Should().BeNull();
    }

    [Fact]
    public void TryResolve_WithNonVariableDeclInSymbolTable_ReturnsFalse()
    {
        // Arrange
        var anotherFunctionDef = CreateFunctionDef("anotherFunc", "void");
        var functionDef = CreateFunctionDef("testFunc", "void");

        // Set up symbol table with non-VariableDecl entry
        var symbol = new Symbol("anotherFunc", SymbolKind.FunctionDef);
        var symbolTableEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            OriginatingAstThing = anotherFunctionDef,
            Annotations = []
        };

        functionDef.SymbolTable.Add(symbol, symbolTableEntry);

        // Act
        var result = _visitor.TryResolve("anotherFunc", functionDef, out var resolvedDecl);

        // Assert
        result.Should().BeFalse();
        resolvedDecl.Should().BeNull();
    }

    [Fact]
    public void VisitVarRefExp_WithAlreadyResolvedVariableDecl_ReturnsUnchanged()
    {
        // Arrange
        var existingVariableDecl = new VariableDecl
        {
            Name = "testVar",
            TypeName = TypeName.From("int"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        var varRefExp = new VarRefExp
        {
            VarName = "testVar",
            VariableDecl = existingVariableDecl,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        // Act
        var result = _visitor.VisitVarRefExp(varRefExp);

        // Assert
        result.Should().NotBeNull();
        result.VarName.Should().Be("testVar");
        result.VariableDecl.Should().Be(existingVariableDecl);
    }

    [Fact]
    public void VisitVarRefExp_WithNoNearestScope_ReturnsUnchanged()
    {
        // Arrange
        var varRefExp = new VarRefExp
        {
            VarName = "testVar",
            VariableDecl = null,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        // Act
        var result = _visitor.VisitVarRefExp(varRefExp);

        // Assert
        result.Should().NotBeNull();
        result.VarName.Should().Be("testVar");
        result.VariableDecl.Should().BeNull();
    }

    [Fact]
    public void VisitVarRefExp_WithValidScope_ResolvesVariableDecl()
    {
        // Arrange
        var variableDecl = new VariableDecl
        {
            Name = "testVar",
            TypeName = TypeName.From("int"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        var functionDef = CreateFunctionDef("testFunc", "void");

        var varRefExp = new VarRefExp
        {
            VarName = "testVar",
            VariableDecl = null,
            Annotations = [],
            Location = CreateLocation(),
            Parent = functionDef
        };

        // Set up symbol table
        var symbol = new Symbol("testVar", SymbolKind.VarDeclStatement);
        var symbolTableEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            OriginatingAstThing = variableDecl,
            Annotations = []
        };

        functionDef.SymbolTable.Add(symbol, symbolTableEntry);

        // Act
        var result = _visitor.VisitVarRefExp(varRefExp);

        // Assert
        result.Should().NotBeNull();
        result.VarName.Should().Be("testVar");
        result.VariableDecl.Should().NotBeNull();
        result.VariableDecl.Should().Be(variableDecl);
    }

    [Fact]
    public void VisitVarRefExp_WithVariableNotInScope_ReturnsUnresolved()
    {
        // Arrange
        var functionDef = CreateFunctionDef("testFunc", "void");

        var varRefExp = new VarRefExp
        {
            VarName = "nonExistentVar",
            VariableDecl = null,
            Annotations = [],
            Location = CreateLocation(),
            Parent = functionDef
        };

        // Act
        var result = _visitor.VisitVarRefExp(varRefExp);

        // Assert
        result.Should().NotBeNull();
        result.VarName.Should().Be("nonExistentVar");
        result.VariableDecl.Should().BeNull();
    }

    [Fact]
    public void VisitVarRefExp_WithHierarchicalScopes_ResolvesFromNearestScope()
    {
        // Arrange
        var innerVariableDecl = new VariableDecl
        {
            Name = "testVar",
            TypeName = TypeName.From("string"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        var outerVariableDecl = new VariableDecl
        {
            Name = "testVar",
            TypeName = TypeName.From("int"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        // Create nested scopes - class contains function
        var classDef = CreateClassDef("TestClass");
        var functionDef = CreateFunctionDef("testFunc", "void");
        functionDef.Parent = classDef;

        var varRefExp = new VarRefExp
        {
            VarName = "testVar",
            VariableDecl = null,
            Annotations = [],
            Location = CreateLocation(),
            Parent = functionDef
        };

        // Set up symbol tables - class scope has int, function scope has string
        var outerSymbol = new Symbol("testVar", SymbolKind.VarDeclStatement);
        var outerSymbolTableEntry = new SymbolTableEntry
        {
            Symbol = outerSymbol,
            OriginatingAstThing = outerVariableDecl,
            Annotations = []
        };
        classDef.SymbolTable.Add(outerSymbol, outerSymbolTableEntry);

        var innerSymbol = new Symbol("testVar", SymbolKind.VarDeclStatement);
        var innerSymbolTableEntry = new SymbolTableEntry
        {
            Symbol = innerSymbol,
            OriginatingAstThing = innerVariableDecl,
            Annotations = []
        };
        functionDef.SymbolTable.Add(innerSymbol, innerSymbolTableEntry);

        // Act
        var result = _visitor.VisitVarRefExp(varRefExp);

        // Assert
        result.Should().NotBeNull();
        result.VarName.Should().Be("testVar");
        result.VariableDecl.Should().NotBeNull();
        result.VariableDecl.Should().Be(innerVariableDecl); // Should resolve to inner scope
        result.VariableDecl.TypeName.Value.Should().Be("string");
    }

    [Fact]
    public void VisitVarRefExp_ProcessesCompleteAstTree()
    {
        // Arrange - create a simple function with a variable declaration and reference
        var variableDecl = new VariableDecl
        {
            Name = "localVar",
            TypeName = TypeName.From("int"),
            CollectionType = CollectionType.SingleInstance,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null,
            Visibility = Visibility.Public
        };

        var varDeclStatement = new VarDeclStatement
        {
            VariableDecl = variableDecl,
            InitialValue = new Int32LiteralExp { Value = 42, Annotations = [], Location = CreateLocation(), Parent = null },
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        var varRefExp = new VarRefExp
        {
            VarName = "localVar",
            VariableDecl = null,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        var retStatement = new ReturnStatement
        {
            ReturnValue = varRefExp,
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        var blockStatement = new BlockStatement
        {
            Statements = [varDeclStatement, retStatement],
            Annotations = [],
            Location = CreateLocation(),
            Parent = null
        };

        var functionDef = CreateFunctionDef("testFunc", "int");
        functionDef.Body = blockStatement;

        // Set up parent relationships
        variableDecl.Parent = varDeclStatement;
        varDeclStatement.Parent = blockStatement;
        varRefExp.Parent = retStatement;
        retStatement.Parent = blockStatement;
        blockStatement.Parent = functionDef;

        // Set up symbol table
        var symbol = new Symbol("localVar", SymbolKind.VarDeclStatement);
        var symbolTableEntry = new SymbolTableEntry
        {
            Symbol = symbol,
            OriginatingAstThing = variableDecl,
            Annotations = []
        };
        functionDef.SymbolTable.Add(symbol, symbolTableEntry);

        // Act
        var result = (FunctionDef)_visitor.VisitFunctionDef(functionDef);

        // Assert
        result.Should().NotBeNull();
        result.Body.Should().NotBeNull();
        result.Body.Statements.Should().HaveCount(2);
        
        var returnStatement = result.Body.Statements[1] as ReturnStatement;
        returnStatement.Should().NotBeNull();
        
        var resolvedVarRefExp = returnStatement.ReturnValue as VarRefExp;
        resolvedVarRefExp.Should().NotBeNull();
        resolvedVarRefExp.VarName.Should().Be("localVar");
        resolvedVarRefExp.VariableDecl.Should().NotBeNull();
        resolvedVarRefExp.VariableDecl.Name.Should().Be("localVar");
    }
}