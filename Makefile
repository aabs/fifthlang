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
	@echo "  help              - Show this help message"

build-all: restore run-generator
	dotnet build fifthlang.sln

restore:
	dotnet restore fifthlang.sln

test:
	dotnet test

run-generator:
	dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

clean:
	dotnet clean fifthlang.sln
