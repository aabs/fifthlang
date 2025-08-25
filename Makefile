# Major Development Targets for AST Builder
.PHONY: help build-all restore test run-generator clean \
	build-ast-model build-ast-generator build-ast-generated build-parser build-compiler build-tests \
	coverage coverage-report

help:
	@echo "Available targets:"
	@echo "  build-all         - Restore and build the full solution"
	@echo "  restore           - dotnet restore"
	@echo "  test              - Run all tests"
	@echo "  run-generator     - Run AST code generator"
	@echo "  clean             - Clean all build outputs"
	@echo "  help              - Show this help message"
	@echo "  coverage          - Run tests with coverage (Cobertura via runsettings)"
	@echo "  coverage-report   - Generate HTML/TextSummary report from Cobertura files"

build-all: restore run-generator
	dotnet build fifthlang.sln

restore:
	dotnet restore fifthlang.sln

test:
	dotnet test

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

clean:
	dotnet clean fifthlang.sln
