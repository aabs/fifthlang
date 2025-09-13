# Major Development Targets for AST Builder
.PHONY: help build-all restore test run-generator clean rebuild ci setup-hooks \
	build-ast-model build-ast-generator build-ast-generated build-parser build-compiler build-tests \
	test-ast test-runtime test-syntax \
	coverage coverage-report

help:
	@echo "Available targets:"
	@echo "  build-all         - Restore and build the full solution"
	@echo "  restore           - dotnet restore"
	@echo "  test              - Run all tests"
	@echo "  run-generator     - Run AST code generator"
	@echo "  clean             - Clean all build outputs"
	@echo "  rebuild           - Clean then build the full solution"
	@echo "  ci                - Build all, then run full test suite"
	@echo "  help              - Show this help message"
	@echo "  coverage          - Run tests with coverage (Cobertura via runsettings)"
	@echo "  coverage-report   - Generate HTML/TextSummary report from Cobertura files"
	@echo "  build-ast-model   - Build src/ast-model"
	@echo "  build-ast-generator - Build src/ast_generator"
	@echo "  build-ast-generated - Build src/ast-generated"
	@echo "  build-parser      - Build src/parser"
	@echo "  build-compiler    - Build src/compiler"
	@echo "  build-tests       - Build all test projects"
	@echo "  test-ast          - Run AST unit tests only"
	@echo "  test-runtime      - Run runtime integration tests only"
	@echo "  test-syntax       - Run isolated syntax-only parser tests"
	@echo "  setup-hooks       - Install git pre-commit and pre-push hooks"

build-all: restore run-generator
	dotnet build fifthlang.sln

restore:
	dotnet restore fifthlang.sln

test:
	dotnet test

# Clean then build the full solution
rebuild:
	dotnet clean fifthlang.sln && dotnet build fifthlang.sln

# CI-friendly alias: build everything then run all tests
ci: build-all test

# Run tests with Cobertura coverage output using runsettings

coverage:
	dotnet test fifthlang.sln --configuration Release --collect "XPlat Code Coverage" --logger "trx;LogFileName=results.trx" --results-directory "TestResults" --settings fifth.runsettings
	@echo "TRX files:"
	@{ find TestResults -type f -name '*.trx' 2>/dev/null; find test -type f -path '*/TestResults/*.trx' 2>/dev/null; } | sed -n '1,20p' || true
	@echo "Cobertura files:"
	@find . -type f -name 'coverage.cobertura.xml' | sed -n '1,40p' || true

# Generate an HTML/TextSummary report from Cobertura files
coverage-report:
	@dotnet tool install -g dotnet-reportgenerator-globaltool || true
	@echo "Ensure dotnet tools are on PATH if needed: $$HOME/.dotnet/tools"
	@if [ -n "$(shell find . -type f -name 'coverage.cobertura.xml' -print -quit)" ]; then \
		reportgenerator -reports:**/coverage.cobertura.xml -targetdir:CoverageReport -reporttypes:Html;TextSummary;Cobertura; \
		echo "CoverageReport generated at ./CoverageReport"; \
	else \
		echo "No Cobertura files found. Run 'make coverage' first (or ensure --collect and runsettings are used)."; \
	fi

run-generator:
	dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

# Install git hooks (requires fish)
setup-hooks:
	@which fish >/dev/null 2>&1 || { echo "fish shell required for setup"; exit 1; }
	fish scripts/setup-githooks.fish

clean:
	dotnet clean fifthlang.sln

# Granular build targets
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
	dotnet build test/runtime-integration-tests/runtime-integration-tests.csproj
	dotnet build test/syntax-parser-tests/syntax-parser-tests.csproj

# Granular test targets
test-ast:
	dotnet test test/ast-tests/ast_tests.csproj

test-runtime:
	dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj

# Important: build before running to ensure updated assets are copied
test-syntax:
	dotnet clean test/syntax-parser-tests/syntax-parser-tests.csproj && \
	  dotnet build test/syntax-parser-tests/syntax-parser-tests.csproj -v minimal && \
	  dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj -v minimal --no-build
