# Fifth Language Compiler (fifthc)

The Fifth Language Compiler provides a complete compilation pipeline for `.5th` source files, including parsing, semantic analysis, IL generation, and executable production.

## Usage

```bash
# Build an executable
fifthc --source hello.5th --output hello.exe

# Build and run immediately  
fifthc --command run --source hello.5th --output hello.exe --args "arg1 arg2"

# Lint/validate source without generating output
fifthc --command lint --source src/

# Show help
fifthc --command help
```

## Commands

- **build** (default): Parse, transform, generate IL, and assemble to executable
- **run**: Same as build, then execute the produced binary with provided arguments
- **lint**: Parse and apply transformations only, report issues without generating files
- **help**: Display usage information

## Options

- `--source <path>`: Source file or directory path (required for build/run/lint)
- `--output <path>`: Output executable path (required for build/run)  
- `--args <args>`: Arguments to pass to program when running
- `--keep-temp`: Keep temporary IL files for debugging
- `--diagnostics`: Enable diagnostic output showing compilation phases and timing

## Exit Codes

- **0**: Success
- **1**: General error (invalid options, unknown command)
- **2**: Parse error (syntax errors, file not found)
- **3**: Semantic error (type checking, undefined references)
- **4**: IL assembly error (ilasm not found, assembly failed)
- **5**: Runtime error (when using `run` command and program fails)

## Architecture

The compiler orchestrates several phases:

1. **Parse Phase**: Uses ANTLR-based parser to build AST from Fifth source
2. **Transform Phase**: Applies language analysis passes (symbol table building, type inference, etc.)
3. **IL Generation Phase**: Converts AST to Common Language Infrastructure IL code
4. **Assembly Phase**: Uses `ilasm` to compile IL to executable
5. **Run Phase** (optional): Executes the produced binary

## Requirements

- .NET 8.0 or later
- IL Assembler (`ilasm`) for executable generation:
  - Set `ILASM_PATH` environment variable, or
  - Set `DOTNET_ROOT` environment variable, or  
  - Ensure `dotnet` is in PATH with SDK that includes `ilasm`

## Testing

The compiler includes comprehensive unit and integration tests covering:

- Options parsing and validation
- All compilation phases
- Error handling and exit codes
- Process execution abstraction
- Cross-platform compatibility

Run tests with:
```bash
dotnet test test/ast-tests/ast_tests.csproj --filter "FullyQualifiedName~Compiler"
```