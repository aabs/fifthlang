.PHONY: build-all run-generator

build-all:
	@echo "Building all components..."
	dotnet build ast-builder.sln

run-generator:
	@echo "Running code generator..."
	dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated