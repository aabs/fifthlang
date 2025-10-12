# Justfile for the Fifth language repository
# Provides the same high-level tasks that the previous Makefile exposed.
#
# Use:
#   just <target>
# Example:
#   just build-all

default := "help"

# Build and restore
# Restore and build the full solution
build-all:
	just restore
	just run-generator
	dotnet build fifthlang.sln

# Restore NuGet packages for the solution
restore:
	dotnet restore fifthlang.sln

# Run all tests
test:
	dotnet test

# Clean then build the full solution
rebuild:
	dotnet clean fifthlang.sln && dotnet build fifthlang.sln

# CI-friendly alias: build everything then run all tests
ci:
	just build-all
	just test

# Run tests with Cobertura coverage output using runsettings
coverage:
	dotnet test fifthlang.sln --configuration Release --collect "XPlat Code Coverage" --logger "trx;LogFileName=results.trx" --results-directory "TestResults" --settings fifth.runsettings
	printf "TRX files:\n"
	find TestResults -type f -name '*.trx' 2>/dev/null | sed -n '1,20p' || true
	printf "Cobertura files:\n"
	find . -type f -name 'coverage.cobertura.xml' | sed -n '1,40p' || true

# Generate an HTML/TextSummary report from Cobertura files
coverage-report:
	dotnet tool install -g dotnet-reportgenerator-globaltool || true
	printf "Ensure dotnet tools are on PATH if needed: $HOME/.dotnet/tools\n"
	if [ -n "$(find . -type f -name 'coverage.cobertura.xml' -print -quit)" ]; then \
		reportgenerator -reports:**/coverage.cobertura.xml -targetdir:CoverageReport -reporttypes:Html;TextSummary;Cobertura; \
		printf "CoverageReport generated at ./CoverageReport\n"; \
	else \
		printf "No Cobertura files found. Run 'just coverage' first (or ensure --collect and runsettings are used).\n"; \
	fi

# Run the AST code generator (separate step)
run-generator:
	dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated

# Install git hooks (requires fish)
setup-hooks:
	command -v fish >/dev/null 2>&1 || { printf "fish shell required for setup\n"; exit 1; }
	fish scripts/setup-githooks.fish

# Clean build artifacts
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
	dotnet build src/compiler/compiler.csproj --configuration Release

build-tests:
	dotnet build test/ast-tests/ast_tests.csproj
	dotnet build test/runtime-integration-tests/runtime-integration-tests.csproj
	dotnet build test/syntax-parser-tests/syntax-parser-tests.csproj

# Granular test targets
test-ast:
	dotnet test test/ast-tests/ast_tests.csproj

test-runtime:
	dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj

# Run the entire test matrix and a CLI smoke-compile using the Roslyn backend
# This target mirrors the CI `roslyn-backend-validation` check for the roslyn backend
test-all-roslyn:
	dotnet test -p:UsePinnedRoslyn=true -v minimal

# Important: build before running to ensure updated assets are copied
test-syntax:
	dotnet clean test/syntax-parser-tests/syntax-parser-tests.csproj || true
	dotnet build test/syntax-parser-tests/syntax-parser-tests.csproj -v minimal
	dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj -v minimal --no-build

# Install compiler CLI tool
install-cli: build-compiler
	printf "Creating symlink to compiler in ~/bin as 'fifth'...\n"
	mkdir -p ~/bin
	rm -f ~/bin/fifth || true
	ln -s "$(pwd)/src/compiler/bin/Release/net8.0/compiler" ~/bin/fifth
	printf "Fifth language compiler is now available as 'fifth' in your PATH\n"
	printf "Usage: fifth [options] <source-file>\n"

# Help text is generated from comments above each recipe; use `just --summary` for a concise list
help:
	just --summary
