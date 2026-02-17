# Testing Contracts

## Overview

This document defines the interface contracts for test scripts that validate release packages. All test scripts follow the same CLI discipline as build scripts: structured output, clear exit codes, deterministic behavior.

---

## Contract: smoke-test.sh

**Purpose**: Validate that a release package contains a functional Fifth compiler.

### Input Interface

**Required Arguments**:
- `--package-path <path>`: Path to package archive file
- `--test-dir <path>`: Directory for test execution (extracted package + test files)

**Optional Arguments**:
- `--verbose`: Enable detailed test output
- `--skip-cleanup`: Leave test directory intact after completion
- `--timeout <seconds>`: Test timeout (default: 300)

**Environment Variables**:
- `FIFTH_TEST_SAMPLES`: Path to test .5th files (default: ./test-samples)

### Output Interface

**Exit Codes**:
- `0`: All tests passed
- `1`: One or more tests failed
- `2`: Invalid arguments
- `10`: Package extraction failed
- `11`: Compiler not found in package
- `12`: Test timeout exceeded

**stdout** (JSON format):
```json
{
  "success": true,
  "package": "fifth-1.2.3-linux-x64-net8.0.tar.gz",
  "test_summary": {
    "total": 5,
    "passed": 5,
    "failed": 0,
    "skipped": 0,
    "duration_seconds": 12.3
  },
  "test_cases": [
    {
      "name": "version_check",
      "status": "passed",
      "duration_seconds": 1.2,
      "output": "Fifth Language Compiler v1.2.3"
    },
    {
      "name": "compile_hello_world",
      "status": "passed",
      "duration_seconds": 8.5,
      "output": "Compilation successful: hello.exe"
    },
    {
      "name": "execute_hello_world",
      "status": "passed",
      "duration_seconds": 1.5,
      "output": "Hello, World!"
    },
    {
      "name": "compile_with_kg_features",
      "status": "passed",
      "duration_seconds": 0.8,
      "output": "Compilation successful: kg-test.exe"
    },
    {
      "name": "parser_grammar_loaded",
      "status": "passed",
      "duration_seconds": 0.3,
      "output": "Parser initialized successfully"
    }
  ],
  "environment": {
    "os": "linux",
    "arch": "x86_64",
    "dotnet_runtime": "8.0.11"
  }
}
```

**stderr** (human-readable progress):
```
[INFO] Extracting package: fifth-1.2.3-linux-x64-net8.0.tar.gz
[INFO] Package extracted to /tmp/smoke-test-abc123/
[INFO] Running test: version_check
[PASS] version_check (1.2s)
[INFO] Running test: compile_hello_world
[PASS] compile_hello_world (8.5s)
[INFO] Running test: execute_hello_world
[PASS] execute_hello_world (1.5s)
[INFO] Running test: compile_with_kg_features
[PASS] compile_with_kg_features (0.8s)
[INFO] Running test: parser_grammar_loaded
[PASS] parser_grammar_loaded (0.3s)
[INFO] All tests passed (5/5) in 12.3s
```

### Test Cases

The smoke test runs the following test cases in order:

#### 1. version_check
- **Purpose**: Verify compiler reports correct version
- **Command**: `fifth --version`
- **Expected**: Output matches package version
- **Failure**: Compiler not executable, wrong version, crashes

#### 2. compile_hello_world
- **Purpose**: Verify basic compilation works
- **Input**: Simple .5th file with main() function
- **Command**: `fifth compile hello.5th -o hello.exe`
- **Expected**: Exit code 0, output file created
- **Failure**: Compilation error, missing dependencies, crash

#### 3. execute_hello_world
- **Purpose**: Verify compiled programs execute
- **Input**: Compiled hello.exe from previous test
- **Command**: `dotnet hello.exe` (or direct execution if native)
- **Expected**: Exit code 0, expected output
- **Failure**: Runtime error, missing runtime dependencies

#### 4. compile_with_kg_features
- **Purpose**: Verify knowledge graph features work
- **Input**: .5th file using TriG/SPARQL literals
- **Command**: `fifth compile kg-test.5th -o kg-test.exe`
- **Expected**: Exit code 0, output file created
- **Failure**: Parser error, codegen error for RDF features

#### 5. parser_grammar_loaded
- **Purpose**: Verify ANTLR grammar loaded correctly
- **Input**: .5th file with various language constructs
- **Command**: `fifth parse test.5th --ast-dump`
- **Expected**: AST dumped successfully, no parse errors
- **Failure**: Grammar file missing, ANTLR runtime error

### Behavior Contracts

1. **Isolation**: Each test case runs in clean subdirectory
2. **Order Independence**: Tests can run in any order (use --only flag)
3. **Fast Failure**: Stop on first failure unless --continue-on-error
4. **Cleanup**: Remove test directory unless --skip-cleanup or tests fail
5. **Reproducibility**: Same package + test cases → same results

### Example Usage

```bash
# Run full smoke test
./scripts/test/smoke-test.sh \
  --package-path ./dist/fifth-1.2.3-linux-x64-net8.0.tar.gz \
  --test-dir /tmp/smoke-test

# Run with verbose output
./scripts/test/smoke-test.sh \
  --package-path ./dist/fifth-1.2.3-win-x64-net8.0.zip \
  --test-dir /tmp/smoke-test \
  --verbose

# Run single test case (future enhancement)
./scripts/test/smoke-test.sh \
  --package-path ./dist/fifth-1.2.3-linux-x64-net8.0.tar.gz \
  --test-dir /tmp/smoke-test \
  --only version_check
```

---

## Contract: verify-package.sh

**Purpose**: Verify package structure and contents without executing compiler.

### Input Interface

**Required Arguments**:
- `--package-path <path>`: Path to package archive file

**Optional Arguments**:
- `--verbose`: Enable detailed verification output
- `--format <tar.gz|zip>`: Expected archive format (auto-detect if omitted)

### Output Interface

**Exit Codes**:
- `0`: Package valid
- `1`: Package invalid (structure/contents)
- `2`: Invalid arguments
- `10`: Cannot extract archive
- `11`: Missing required files
- `12`: Incorrect permissions

**stdout** (JSON format):
```json
{
  "valid": true,
  "package": "fifth-1.2.3-linux-x64-net8.0.tar.gz",
  "checks": [
    {
      "name": "archive_extractable",
      "status": "passed",
      "message": "Archive extracted successfully"
    },
    {
      "name": "has_executable",
      "status": "passed",
      "message": "Executable found: bin/fifth"
    },
    {
      "name": "executable_permissions",
      "status": "passed",
      "message": "Executable has +x permission"
    },
    {
      "name": "has_license",
      "status": "passed",
      "message": "LICENSE file present"
    },
    {
      "name": "has_readme",
      "status": "passed",
      "message": "README.md present"
    },
    {
      "name": "has_version_file",
      "status": "passed",
      "message": "VERSION.txt present with correct version"
    },
    {
      "name": "correct_directory_structure",
      "status": "passed",
      "message": "Archive has fifth-1.2.3/ root directory"
    },
    {
      "name": "size_within_limits",
      "status": "warning",
      "message": "Package size: 89.4 MB (under 150 MB soft limit)"
    }
  ],
  "file_count": 47,
  "size_bytes": 89432064
}
```

**stderr** (human-readable progress):
```
[INFO] Verifying package: fifth-1.2.3-linux-x64-net8.0.tar.gz
[PASS] Archive extractable
[PASS] Executable present: bin/fifth
[PASS] Executable permissions correct (+x)
[PASS] LICENSE file present
[PASS] README.md present
[PASS] VERSION.txt present and correct
[PASS] Directory structure correct
[WARN] Package size: 89.4 MB (under 150 MB soft limit)
[INFO] Package verification passed (8/8 checks, 0 warnings)
```

### Verification Checks

1. **archive_extractable**: Archive can be extracted without errors
2. **has_executable**: Compiler executable exists at expected path
3. **executable_permissions**: Executable has correct permissions (Unix only)
4. **has_license**: LICENSE file present
5. **has_readme**: README.md present with installation instructions
6. **has_version_file**: VERSION.txt contains correct version string
7. **correct_directory_structure**: Archive has expected directory layout
8. **size_within_limits**: Package size check (150MB soft limit - logs warning if exceeded, does not fail per FR-031)

### Behavior Contracts

1. **Non-Destructive**: Does not modify or execute package contents
2. **Platform-Aware**: Skips permission checks on Windows
3. **Fast Execution**: Completes in < 5 seconds
4. **Detailed Failures**: Reports all failing checks, not just first
5. **Soft Size Limit** (FR-031): Package size > 150MB logs warning but does not fail (exit code 0 with warning annotation)
5. **Deterministic**: Same package → same result

### Example Usage

```bash
# Verify package structure
./scripts/test/verify-package.sh \
  --package-path ./dist/fifth-1.2.3-linux-x64-net8.0.tar.gz

# Verify with verbose output
./scripts/test/verify-package.sh \
  --package-path ./dist/fifth-1.2.3-win-x64-net8.0.zip \
  --verbose
```

---

## Test Sample Files

Smoke tests require sample .5th files. These must be included in the test suite:

### test-samples/hello.5th
```fifth
main(): int {
    std.print("Hello, World!");
    return 0;
}
```

### test-samples/kg-test.5th
```fifth
store default = sparql_store(<http://example.org/test>);

main(): int {
    g: graph = KG.CreateGraph();
    t: triple = <http://example.org/s> <http://example.org/p> "test";
    g += t;
    return 0;
}
```

### test-samples/parser-test.5th
```fifth
class Person {
    name: string;
    age: int;
}

factorial(n: int): int {
    return (n <= 1) ? 1 : n * factorial(n - 1);
}

main(): int {
    p: Person = Person { name: "Alice", age: 30 };
    result: int = factorial(5);
    std.print(result);
    return 0;
}
```

## CI Integration

These test contracts are designed to integrate with GitHub Actions:

```yaml
- name: Run smoke tests
  run: |
    ./scripts/test/smoke-test.sh \
      --package-path ./dist/fifth-${{ matrix.version }}-${{ matrix.runtime }}.tar.gz \
      --test-dir /tmp/smoke-test
  
- name: Verify package structure
  run: |
    ./scripts/test/verify-package.sh \
      --package-path ./dist/fifth-${{ matrix.version }}-${{ matrix.runtime }}.tar.gz
```

Both scripts output JSON to stdout, which can be parsed by CI for reporting.
