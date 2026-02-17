# Constructor Functions - Developer Guide

Quick reference for developers working on constructor function implementation.

## Quick Start

```bash
# Build the solution
dotnet build fifthlang.sln

# Run constructor-specific tests
dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj --filter "FullyQualifiedName~Constructor"
dotnet test test/ast-tests/ast_tests.csproj --filter "ClassName=ConstructorSynthesisTests"

# Regenerate AST after metamodel changes
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

## Example Syntax

```fifth
// Basic constructor
class Person {
    Name: string;
    Age: int;
    
    Person(name: string, age: int) {
        this.Name = name;
        this.Age = age;
    }
}

// Constructor with base call
class Employee {
    Id: int;
    
    Employee(name: string, id: int) : base(name) {
        this.Id = id;
    }
}

// Multiple overloads
class Rectangle {
    Width: int;
    Height: int;
    
    Rectangle() {
        this.Width = 0;
        this.Height = 0;
    }
    
    Rectangle(size: int) {
        this.Width = size;
        this.Height = size;
    }
}
```

## Architecture Overview

```
Parser (ANTLR)
  ↓
AstBuilderVisitor → FunctionDef (IsConstructor=true)
  ↓
ClassCtorInserter (synthesis)
  ↓
ConstructorValidator (validation)
  ↓
Symbol Table Building
  ↓
Type Annotation
  ↓
[Future] ConstructorResolver (overload resolution)
  ↓
[Future] ConstructorLowering (code generation)
```

## Key Classes

### Grammar
- **FifthLexer.g4**: `BASE` keyword
- **FifthParser.g4**: `constructor_declaration`, `base_constructor_call` rules

### AST
- **AstMetamodel.cs**: 
  - `BaseConstructorCall` record
  - `FunctionDef.BaseCall` field

### Parser
- **AstBuilderVisitor.cs**:
  - `VisitConstructor_declaration()` - Creates FunctionDef with IsConstructor=true
  - `VisitBase_constructor_call()` - Parses base constructor arguments

### Compiler Passes
- **ClassCtorInserter.cs**: Synthesizes parameterless constructors
  - Checks for explicit constructors
  - Identifies required fields
  - Emits CTOR005 when synthesis impossible
  
- **ConstructorValidator.cs**: Validates constructor semantics
  - Checks for static modifier (CTOR010)
  - Checks for value returns (CTOR009)
  - Recursively validates control flow

- **ConstructorDiagnostics.cs**: All diagnostic helper methods

## Compiler Pipeline

Current phase ordering in `ParserManager.cs`:

1. TreeLink
2. Builtins
3. **ClassCtors** ← Constructor synthesis happens here
4. **ConstructorValidation** ← Constructor validation happens here
5. SymbolTableInitial
6. PropertyToField
7. ... (other phases)

## Adding New Constructor Features

### 1. Grammar Changes

Edit `FifthParser.g4`:
```antlr
constructor_declaration:
    IDENTIFIER L_PAREN (paramdecl (COMMA paramdecl)*)? R_PAREN
    (base_constructor_call)?
    function_body;
```

Regenerate parser (happens automatically on build).

### 2. AST Changes

Edit `AstMetamodel.cs`:
```csharp
public record MyNewConstructorNode : AstThing
{
    // fields
}
```

Regenerate AST infrastructure:
```bash
dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated
```

### 3. Parser Visitor

Edit `AstBuilderVisitor.cs`:
```csharp
public override IAstThing VisitMyNewRule(FifthParser.MyNewRuleContext context)
{
    // Build AST node from parse tree
}
```

### 4. Compiler Pass

Create new visitor in `src/compiler/`:
```csharp
public class MyConstructorPass : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic>? _diagnostics;
    
    public override ClassDef VisitClassDef(ClassDef ctx)
    {
        // Process constructors
        return base.VisitClassDef(ctx);
    }
}
```

Add to pipeline in `ParserManager.cs`:
```csharp
if (upTo >= AnalysisPhase.MyNewPhase)
    ast = new MyConstructorPass(diagnostics).Visit(ast);
```

### 5. Tests

Add parser test in `ConstructorParsingTests.cs`:
```csharp
[Test]
public void MyConstructorFeature_ShouldParse()
{
    var input = @"class MyClass { MyClass() { } }";
    ParserTestUtils.AssertNoErrors(input, p => p.fifth(),
        "My feature should parse");
}
```

Add unit test in `ConstructorSynthesisTests.cs`:
```csharp
[Test]
public void MyConstructorFeature_ShouldBehaveCorrectly()
{
    // Arrange
    var classDef = /* build test AST */;
    var diagnostics = new List<Diagnostic>();
    var pass = new MyConstructorPass(diagnostics);
    
    // Act
    var result = pass.VisitClassDef(classDef);
    
    // Assert
    diagnostics.Should()./* expectations */;
}
```

## Common Patterns

### Identifying Constructors

```csharp
// In a visitor/rewriter
foreach (var member in classDef.MemberDefs)
{
    if (member is MethodDef methodDef && methodDef.FunctionDef?.IsConstructor == true)
    {
        var constructor = methodDef.FunctionDef;
        // Process constructor
    }
}
```

### Emitting Diagnostics

```csharp
_diagnostics?.Add(ConstructorDiagnostics.MyDiagnostic(
    className: "MyClass",
    details: "Additional context",
    source: constructor.Location?.OriginalText));
```

### Checking Required Fields

```csharp
private static bool IsRequiredField(FieldDef field)
{
    // Field is required if it's non-nullable and has no default
    var isNullable = field.TypeName.Value.EndsWith("?");
    return !isNullable;
}
```

## Debugging Tips

### View Parse Tree
```bash
# Add debug output in AstBuilderVisitor
if (DebugHelpers.DebugEnabled)
{
    DebugHelpers.DebugLog($"Visiting constructor: {context.GetText()}");
}
```

### Inspect AST
```csharp
// Use DumpTreeVisitor in compiler
var dumper = new DumpTreeVisitor();
dumper.Visit(ast);
```

### Check Pipeline Phases
```csharp
// In ParserManager, add logging
if (DebugHelpers.DebugEnabled)
{
    DebugHelpers.DebugLog($"Running phase: {phase}");
}
```

## Testing Strategy

1. **Parser tests**: Verify syntax is recognized
2. **Unit tests**: Test individual passes in isolation
3. **Integration tests**: Test full pipeline with .5th files
4. **Runtime tests**: Test actual execution (future)

## Known Limitations

- Object instantiation with `new` keyword not yet implemented
- Overload resolution not yet implemented
- Definite assignment analysis not yet implemented
- Base constructor chaining not yet fully supported
- Generic constructor support not yet implemented

## Useful Commands

```bash
# Run all tests
dotnet test fifthlang.sln

# Run only constructor tests
dotnet test --filter "FullyQualifiedName~Constructor"

# Build with diagnostics
dotnet build fifthlang.sln --verbosity detailed

# Clean and rebuild
dotnet clean fifthlang.sln && dotnet build fifthlang.sln

# Check for compilation errors in specific project
dotnet build src/compiler/compiler.csproj
```

## References

- **Spec**: `specs/001-constructor-functions/spec.md`
- **Tasks**: `specs/001-constructor-functions/tasks.md`
- **Status**: `specs/001-constructor-functions/IMPLEMENTATION_STATUS.md`
- **Repository README**: `README.md`
