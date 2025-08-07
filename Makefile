# Major Development Targets for AST Builder
.PHONY: help build-all restore test run-generator clean \
	build-ast-model build-ast-generator build-ast-generated build-parser build-compiler build-tests

help:
	@echo "Available targets:"
	@echo "  build-all         - Restore and build the full solution"
	@echo "  restore           - dotnet restore"
	@echo "  test              - Run all tests"
	@echo "  run-generator     - Run AST code generator"
	@echo "  clean             - Clean all build outputs"
	@echo "  build-ast-model   - Build ast-model project"
	@echo "  build-ast-generator - Build ast_generator project"
	@echo "  build-ast-generated - Build ast-generated project"
	@echo "  build-parser      - Build parser project"
	@echo "  build-compiler    - Build compiler project"
	@echo "  build-tests       - Build test projects"
	@echo "  help              - Show this help message"

build-all: restore run-generator
	dotnet build ast-builder.sln

restore:
	dotnet restore ast-builder.sln

test:
	dotnet test test/ast-tests/ast_tests.csproj

run-generator:
	dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

clean:
	dotnet clean ast-builder.sln

build-ast-model:
	dotnet build src/ast-model/ast_model.csproj

build-ast-generator:
	dotnet build src/ast_generator/ast_generator.csproj

build-ast-generated:
	dotnet build src/ast-generated/ast_generated.csproj

build-parser:
	dotnet build src/parser/parser.csproj

build-compiler:
	dotnet build src/compiler/compiler.csproj

build-tests:
	dotnet build test/ast-tests/ast_tests.csproj

