#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'EOF'
Usage: smoke-test.sh --package-path <archive> --test-dir <dir> [--verbose] [--skip-cleanup] [--timeout <seconds>]
EOF
}

log() {
    echo "[INFO] $*" >&2
}

warn() {
    echo "[WARN] $*" >&2
}

error() {
    echo "[ERROR] $*" >&2
}

require_command() {
    if ! command -v "$1" >/dev/null 2>&1; then
        error "Required command '$1' not found in PATH"
        exit 2
    fi
}

abs_path() {
    python3 - "$1" <<'PY'
import os, sys
print(os.path.abspath(sys.argv[1]))
PY
}

now() {
    python3 <<'PY'
import time
print(time.time())
PY
}

calc_duration() {
    python3 - "$1" "$2" <<'PY'
import sys
print(round(float(sys.argv[2]) - float(sys.argv[1]), 4))
PY
}

normalize_arch() {
    local raw="$1"
    case "$raw" in
        x86_64|X86_64|amd64|AMD64)
            echo "x64"
            ;;
        arm64|ARM64|aarch64|AARCH64)
            echo "arm64"
            ;;
        i386|i686)
            echo "x86"
            ;;
        *)
            echo "$raw"
            ;;
    esac
}

encode_base64() {
    python3 -c 'import base64, sys; print(base64.b64encode(sys.stdin.buffer.read()).decode())'
}

detect_binary_arch() {
    local binary_path="$1"
    if ! command -v file >/dev/null 2>&1; then
        return 0
    fi
    local descriptor
    if ! descriptor=$(file -b "$binary_path" 2>/dev/null); then
        return 0
    fi
    case "$descriptor" in
        *"ARM aarch64"*|*"ARM64"*|*"aarch64"*)
            echo "arm64"
            ;;
        *"x86-64"*|*"x86_64"*|*"x64"*)
            echo "x64"
            ;;
        *"Intel 80386"*|*"i386"*)
            echo "x86"
            ;;
    esac
}

PACKAGE_PATH=""
TEST_DIR=""
VERBOSE=false
SKIP_CLEANUP=false
TIMEOUT=300

while [[ $# -gt 0 ]]; do
    case "$1" in
        --package-path)
            shift || { usage >&2; exit 2; }
            PACKAGE_PATH="${1:-}"
            ;;
        --test-dir)
            shift || { usage >&2; exit 2; }
            TEST_DIR="${1:-}"
            ;;
        --verbose)
            VERBOSE=true
            ;;
        --skip-cleanup)
            SKIP_CLEANUP=true
            ;;
        --timeout)
            shift || { usage >&2; exit 2; }
            TIMEOUT="${1:-}"
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            error "Unknown argument: $1"
            usage >&2
            exit 2
            ;;
    esac
    shift || true
done

if [[ -z "$PACKAGE_PATH" || -z "$TEST_DIR" ]]; then
    error "--package-path and --test-dir are required"
    usage >&2
    exit 2
fi

if [[ ! -f "$PACKAGE_PATH" ]]; then
    error "Package not found: $PACKAGE_PATH"
    exit 2
fi

if ! [[ "$TIMEOUT" =~ ^[0-9]+$ ]]; then
    error "--timeout must be an integer value"
    exit 2
fi

require_command python3
require_command dotnet

PACKAGE_FORMAT=""
case "$PACKAGE_PATH" in
    *.tar.gz|*.tgz)
        PACKAGE_FORMAT="tar"
        require_command tar
        ;;
    *.zip)
        PACKAGE_FORMAT="zip"
        require_command unzip
        ;;
        *)
            error "Unsupported archive format (expected .tar.gz or .zip)"
            exit 2
        ;;
esac

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
DEFAULT_SAMPLES="$REPO_ROOT/test/release-tests/test-samples"
SAMPLES_SRC="${FIFTH_TEST_SAMPLES:-$DEFAULT_SAMPLES}"

if [[ ! -d "$SAMPLES_SRC" ]]; then
    error "Sample files not found at $SAMPLES_SRC"
    exit 2
fi

PACKAGE_BASENAME=$(basename "$PACKAGE_PATH")
OS_NAME=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH_NAME=$(uname -m)
RUNNER_ARCH=$(normalize_arch "$ARCH_NAME")
DOTNET_RUNTIME=$(dotnet --version 2>/dev/null || echo "unknown")

PACKAGE_TARGET_PLATFORM=""
PACKAGE_TARGET_ARCH=""
case "$PACKAGE_BASENAME" in
    *-linux-x64-*)
        PACKAGE_TARGET_PLATFORM="linux"
        PACKAGE_TARGET_ARCH="x64"
        ;;
    *-linux-arm64-*)
        PACKAGE_TARGET_PLATFORM="linux"
        PACKAGE_TARGET_ARCH="arm64"
        ;;
    *-osx-x64-*)
        PACKAGE_TARGET_PLATFORM="osx"
        PACKAGE_TARGET_ARCH="x64"
        ;;
    *-osx-arm64-*)
        PACKAGE_TARGET_PLATFORM="osx"
        PACKAGE_TARGET_ARCH="arm64"
        ;;
    *-win-x64-*)
        PACKAGE_TARGET_PLATFORM="win"
        PACKAGE_TARGET_ARCH="x64"
        ;;
    *-win-arm64-*)
        PACKAGE_TARGET_PLATFORM="win"
        PACKAGE_TARGET_ARCH="arm64"
        ;;
esac

log "Runner arch: $RUNNER_ARCH"
if [[ -n "$PACKAGE_TARGET_ARCH" ]]; then
    log "Package target arch (name): $PACKAGE_TARGET_ARCH"
fi

TEST_DIR_ABS=$(abs_path "$TEST_DIR")
rm -rf "$TEST_DIR_ABS"
mkdir -p "$TEST_DIR_ABS"
EXTRACT_DIR="$TEST_DIR_ABS/extracted"
BUILD_DIR="$TEST_DIR_ABS/build"
SAMPLES_DIR="$TEST_DIR_ABS/samples"
mkdir -p "$EXTRACT_DIR" "$BUILD_DIR" "$SAMPLES_DIR"
cp "$SAMPLES_SRC"/*.5th "$SAMPLES_DIR"/

TEST_RESULTS_FILE=$(mktemp "${TMPDIR:-/tmp}/fifth-smoke-results-XXXXXX")
: >"$TEST_RESULTS_FILE"
SCRIPT_START=$(date +%s)
DEADLINE=0
if [[ "$TIMEOUT" -gt 0 ]]; then
    DEADLINE=$((SCRIPT_START + TIMEOUT))
fi
TEST_SUCCESS=false

declare -i TOTAL_TESTS=0

declare -a TEST_SEQUENCE=(
    "version_check"
    "compile_hello_world"
    "execute_hello_world"
    "compile_with_kg_features"
    "parser_grammar_loaded"
)

cleanup() {
    local exit_code=$?
    if $TEST_SUCCESS && ! $SKIP_CLEANUP && [[ -d "$TEST_DIR_ABS" ]]; then
        rm -rf "$TEST_DIR_ABS"
    elif ! $TEST_SUCCESS && ! $SKIP_CLEANUP; then
        warn "Preserving test directory for inspection: $TEST_DIR_ABS"
    fi
    rm -f "$TEST_RESULTS_FILE"
    exit $exit_code
}
trap cleanup EXIT

check_timeout() {
    if [[ "$TIMEOUT" -gt 0 ]]; then
        local now_ts=$(date +%s)
        if (( now_ts > DEADLINE )); then
            error "Test timeout exceeded (${TIMEOUT}s)"
            exit 12
        fi
    fi
}

append_result() {
    local name="$1" status="$2" duration="$3" output="$4"
    local encoded
    encoded=$(printf '%s' "$output" | encode_base64)
    printf '%s\t%s\t%s\t%s\n' "$name" "$status" "$duration" "$encoded" >>"$TEST_RESULTS_FILE"
}

log "Extracting package: $PACKAGE_BASENAME"
if [[ "$PACKAGE_FORMAT" == "tar" ]]; then
    if ! tar -xzf "$PACKAGE_PATH" -C "$EXTRACT_DIR"; then
        error "Failed to extract tar archive"
        exit 10
    fi
else
    if ! unzip -q "$PACKAGE_PATH" -d "$EXTRACT_DIR"; then
        error "Failed to extract zip archive"
        exit 10
    fi
fi

PACKAGE_ROOT=$(find "$EXTRACT_DIR" -mindepth 1 -maxdepth 1 -type d | head -n 1)
if [[ -z "$PACKAGE_ROOT" ]]; then
    error "Unable to determine package root after extraction"
    exit 10
fi

VERSION_FILE="$PACKAGE_ROOT/VERSION.txt"
if [[ ! -f "$VERSION_FILE" ]]; then
    error "VERSION.txt not found in package"
    exit 11
fi
PACKAGE_VERSION=$(tr -d '\r' <"$VERSION_FILE" | tr -d '\n')

FIFTH_BIN=""
if [[ -x "$PACKAGE_ROOT/bin/fifth" ]]; then
    FIFTH_BIN="$PACKAGE_ROOT/bin/fifth"
elif [[ -x "$PACKAGE_ROOT/bin/fifth.exe" ]]; then
    FIFTH_BIN="$PACKAGE_ROOT/bin/fifth.exe"
else
    FIFTH_BIN=$(find "$PACKAGE_ROOT/bin" -maxdepth 1 -type f -perm -111 | head -n 1 || true)
fi

if [[ -z "$FIFTH_BIN" ]]; then
    error "Fifth executable not found in package"
    exit 11
fi

BINARY_TARGET_ARCH=$(detect_binary_arch "$FIFTH_BIN")
if [[ -n "$BINARY_TARGET_ARCH" ]]; then
    log "Binary arch (file): $BINARY_TARGET_ARCH"
    if [[ -z "$PACKAGE_TARGET_ARCH" ]]; then
        PACKAGE_TARGET_ARCH="$BINARY_TARGET_ARCH"
    elif [[ "$PACKAGE_TARGET_ARCH" != "$BINARY_TARGET_ARCH" ]]; then
        warn "Archive arch '$PACKAGE_TARGET_ARCH' disagrees with binary arch '$BINARY_TARGET_ARCH'; using binary metadata"
        PACKAGE_TARGET_ARCH="$BINARY_TARGET_ARCH"
    fi
fi

log "Using compiler: $FIFTH_BIN"
log "Detected version: $PACKAGE_VERSION"

SKIP_REASON=""
if [[ -n "$PACKAGE_TARGET_ARCH" && -n "$RUNNER_ARCH" && "$PACKAGE_TARGET_ARCH" != "$RUNNER_ARCH" ]]; then
    SKIP_REASON="Package architecture ($PACKAGE_TARGET_ARCH) is incompatible with runner architecture ($RUNNER_ARCH)"
fi

TEST_LAST_OUTPUT=""

run_version_check() {
    local output
    if ! output=$("$FIFTH_BIN" --version 2>&1); then
        TEST_LAST_OUTPUT="$output"
        return 1
    fi
    if [[ "$output" != *"$PACKAGE_VERSION"* ]]; then
        TEST_LAST_OUTPUT="Version mismatch. Expected $PACKAGE_VERSION but saw: $output"
        return 1
    fi
    TEST_LAST_OUTPUT="$output"
    return 0
}

run_compile_hello_world() {
    local target="$BUILD_DIR/hello.dll"
    local output
    if ! output=$("$FIFTH_BIN" --source "$SAMPLES_DIR/hello.5th" --output "$target" 2>&1); then
        TEST_LAST_OUTPUT="$output"
        return 1
    fi
    if [[ ! -f "$target" ]]; then
        TEST_LAST_OUTPUT="Expected output assembly not found: $target"
        return 1
    fi
    TEST_LAST_OUTPUT="$output"
    return 0
}

run_execute_hello_world() {
    local assembly="$BUILD_DIR/hello.dll"
    if [[ ! -f "$assembly" ]]; then
        TEST_LAST_OUTPUT="hello.dll missing; compile step must pass first"
        return 1
    fi
    local output
    if ! output=$(dotnet "$assembly" 2>&1); then
        TEST_LAST_OUTPUT="$output"
        return 1
    fi
    TEST_LAST_OUTPUT="$output"
    return 0
}

run_compile_with_kg_features() {
    local target="$BUILD_DIR/kg-test.dll"
    local output
    if ! output=$("$FIFTH_BIN" --source "$SAMPLES_DIR/kg-test.5th" --output "$target" 2>&1); then
        TEST_LAST_OUTPUT="$output"
        return 1
    fi
    if [[ ! -f "$target" ]]; then
        TEST_LAST_OUTPUT="Expected output assembly not found: $target"
        return 1
    fi
    TEST_LAST_OUTPUT="$output"
    return 0
}

run_parser_grammar_loaded() {
    local output
    if ! output=$("$FIFTH_BIN" --command lint --source "$SAMPLES_DIR/parser-test.5th" --diagnostics 2>&1); then
        TEST_LAST_OUTPUT="$output"
        return 1
    fi
    TEST_LAST_OUTPUT="$output"
    return 0
}

execute_test_case() {
    local name="$1"
    shift
    check_timeout
    [[ "$VERBOSE" == true ]] && log "Running test: $name"
    local start_ts=$(now)
    if "$@"; then
        local status="passed"
        local end_ts=$(now)
        local duration=$(calc_duration "$start_ts" "$end_ts")
        append_result "$name" "$status" "$duration" "$TEST_LAST_OUTPUT"
        ((TOTAL_TESTS++))
        log "[PASS] $name (${duration}s)"
        check_timeout
        return 0
    else
        local status="failed"
        local end_ts=$(now)
        local duration=$(calc_duration "$start_ts" "$end_ts")
        append_result "$name" "$status" "$duration" "$TEST_LAST_OUTPUT"
        ((TOTAL_TESTS++))
        error "Test failed: $name"
        return 1
    fi
}

FAILED=false
SKIPPED_ALL_TESTS=false

if [[ -n "$SKIP_REASON" ]]; then
    SKIPPED_ALL_TESTS=true
    for test_name in "${TEST_SEQUENCE[@]}"; do
        append_result "$test_name" "skipped" "0" "$SKIP_REASON"
        ((TOTAL_TESTS++))
    done
else
    for test_name in "${TEST_SEQUENCE[@]}"; do
        if ! execute_test_case "$test_name" "run_${test_name}"; then
            FAILED=true
            break
        fi
    done
fi

TOTAL_DURATION=$(( $(date +%s) - SCRIPT_START ))

if $SKIPPED_ALL_TESTS; then
    warn "Smoke tests skipped: $SKIP_REASON"
    TEST_SUCCESS=true
elif $FAILED; then
    warn "Smoke tests failed"
    TEST_SUCCESS=false
else
    log "All smoke tests passed (${TOTAL_TESTS}/${#TEST_SEQUENCE[@]})"
    TEST_SUCCESS=true
fi

python3 - "$TEST_RESULTS_FILE" "$PACKAGE_BASENAME" "$TOTAL_DURATION" "$OS_NAME" "$ARCH_NAME" "$DOTNET_RUNTIME" "$PACKAGE_PATH" "${SKIP_REASON:-}" <<'PY'
import base64
import json
import sys

path, package, duration, os_name, arch, dotnet_version, package_path = sys.argv[1:8]
skip_reason = sys.argv[8] if len(sys.argv) > 8 else ""
tests = []
passed = 0
failed = 0
skipped = 0
with open(path, 'r', encoding='utf-8') as handle:
    for line in handle:
        if not line.strip():
            continue
        name, status, seconds, payload = line.rstrip('\n').split('\t')
        output = base64.b64decode(payload.encode()).decode(errors='replace')
        tests.append({
            "name": name,
            "status": status,
            "duration_seconds": float(seconds),
            "output": output
        })
        if status == "passed":
            passed += 1
        elif status == "skipped":
            skipped += 1
        else:
            failed += 1

summary = {
    "total": len(tests),
    "passed": passed,
    "failed": failed,
    "skipped": skipped,
    "duration_seconds": float(duration)
}

payload = {
    "success": failed == 0,
    "package": package,
    "package_path": package_path,
    "test_summary": summary,
    "test_cases": tests,
    "environment": {
        "os": os_name,
        "arch": arch,
        "dotnet_runtime": dotnet_version
    }
}

if skip_reason:
    payload["skip_reason"] = skip_reason

print(json.dumps(payload, indent=2))
if failed != 0:
    sys.exit(1)
PY

exit 0
