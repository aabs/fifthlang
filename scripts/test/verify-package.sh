#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'EOF'
Usage: verify-package.sh --package-path <archive> [--format <tar.gz|zip>] [--verbose]
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

file_size_bytes() {
    python3 - "$1" <<'PY'
import os, sys
print(os.path.getsize(sys.argv[1]))
PY
}

append_check() {
    local name="$1" status="$2" message="$3"
    printf '%s\t%s\t%s\n' "$name" "$status" "$message" >>"$CHECKS_FILE"
}

PACKAGE_PATH=""
FORMAT_OVERRIDE=""
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --package-path)
            shift || { usage >&2; exit 2; }
            PACKAGE_PATH="${1:-}"
            ;;
        --format)
            shift || { usage >&2; exit 2; }
            FORMAT_OVERRIDE="${1:-}"
            ;;
        --verbose)
            VERBOSE=true
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

if [[ -z "$PACKAGE_PATH" ]]; then
    error "--package-path is required"
    usage >&2
    exit 2
fi

if [[ ! -f "$PACKAGE_PATH" ]]; then
    error "Package not found: $PACKAGE_PATH"
    exit 2
fi

require_command python3

PACKAGE_FORMAT=""
if [[ -n "$FORMAT_OVERRIDE" ]]; then
    case "$FORMAT_OVERRIDE" in
        tar.gz)
            PACKAGE_FORMAT="tar"
            ;;
        zip)
            PACKAGE_FORMAT="zip"
            ;;
        *)
            error "Unsupported format override: $FORMAT_OVERRIDE"
            exit 2
            ;;
    esac
else
    case "$PACKAGE_PATH" in
        *.tar.gz|*.tgz)
            PACKAGE_FORMAT="tar"
            ;;
        *.zip)
            PACKAGE_FORMAT="zip"
            ;;
        *)
            error "Unable to infer archive format from filename"
            exit 2
            ;;
    esac
fi

if [[ "$PACKAGE_FORMAT" == "tar" ]]; then
    require_command tar
else
    require_command unzip
fi

PACKAGE_BASENAME=$(basename "$PACKAGE_PATH")
IS_WINDOWS_PACKAGE=false
if [[ "$PACKAGE_FORMAT" == "zip" ]]; then
    IS_WINDOWS_PACKAGE=true
fi

WORK_DIR=$(mktemp -d "${TMPDIR:-/tmp}/fifth-verify-XXXXXX")
CHECKS_FILE=$(mktemp "${TMPDIR:-/tmp}/fifth-verify-checks-XXXXXX")
trap 'rm -rf "$WORK_DIR"; rm -f "$CHECKS_FILE"' EXIT
EXTRACT_DIR="$WORK_DIR/extracted"
mkdir -p "$EXTRACT_DIR"
VALID=true
EXIT_CODE=0

log "Verifying package: $PACKAGE_BASENAME"
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
append_check "archive_extractable" "passed" "Archive extracted successfully"

PACKAGE_ROOT=$(find "$EXTRACT_DIR" -mindepth 1 -maxdepth 1 -type d | head -n 1)
INSPECTION_AVAILABLE=true
ROOT_NAME=""
if [[ -z "$PACKAGE_ROOT" ]]; then
    INSPECTION_AVAILABLE=false
    VALID=false
    EXIT_CODE=1
else
    ROOT_NAME=$(basename "$PACKAGE_ROOT")
fi

FILE_COUNT=0
if $INSPECTION_AVAILABLE; then
    FILE_COUNT=$(find "$PACKAGE_ROOT" -type f | wc -l | tr -d ' \t')
fi
ARCHIVE_SIZE=$(file_size_bytes "$PACKAGE_PATH")

VERSION_STATUS="failed"
VERSION_MESSAGE="VERSION.txt not found"
PACKAGE_VERSION=""
if $INSPECTION_AVAILABLE; then
    VERSION_FILE="$PACKAGE_ROOT/VERSION.txt"
    if [[ -f "$VERSION_FILE" ]]; then
        PACKAGE_VERSION=$(tr -d '\r' <"$VERSION_FILE" | tr -d '\n')
        if [[ -n "$PACKAGE_VERSION" ]]; then
            VERSION_STATUS="passed"
            VERSION_MESSAGE="VERSION.txt present ($PACKAGE_VERSION)"
        else
            VERSION_MESSAGE="VERSION.txt is empty"
        fi
    fi
else
    VERSION_MESSAGE="Cannot verify VERSION.txt without package root"
fi

append_check "has_version_file" "$VERSION_STATUS" "$VERSION_MESSAGE"
if [[ "$VERSION_STATUS" == "failed" ]]; then
    VALID=false
    if [[ "$VERSION_MESSAGE" == "VERSION.txt not found" ]]; then
        EXIT_CODE=11
    else
        (( EXIT_CODE < 1 )) && EXIT_CODE=1
    fi
fi

STRUCTURE_STATUS="failed"
STRUCTURE_MESSAGE="Unable to determine package root"
if $INSPECTION_AVAILABLE && [[ -n "$PACKAGE_VERSION" ]]; then
    EXPECTED_ROOT="fifth-$PACKAGE_VERSION"
    if [[ "$ROOT_NAME" == "$EXPECTED_ROOT" ]]; then
        STRUCTURE_STATUS="passed"
        STRUCTURE_MESSAGE="Root directory matches $EXPECTED_ROOT"
    else
        STRUCTURE_MESSAGE="Root directory '$ROOT_NAME' does not match $EXPECTED_ROOT"
    fi
elif ! $INSPECTION_AVAILABLE; then
    STRUCTURE_MESSAGE="Cannot verify directory structure without package root"
elif [[ -z "$PACKAGE_VERSION" ]]; then
    STRUCTURE_MESSAGE="Cannot verify directory structure without version information"
fi

append_check "correct_directory_structure" "$STRUCTURE_STATUS" "$STRUCTURE_MESSAGE"
if [[ "$STRUCTURE_STATUS" == "failed" ]]; then
    VALID=false
    (( EXIT_CODE < 1 )) && EXIT_CODE=1
fi

if $INSPECTION_AVAILABLE; then
    LICENSE_PATH="$PACKAGE_ROOT/LICENSE"
    if [[ -f "$LICENSE_PATH" ]]; then
        append_check "has_license" "passed" "LICENSE file present"
    else
        append_check "has_license" "failed" "LICENSE file missing"
        VALID=false
        EXIT_CODE=11
    fi
else
    append_check "has_license" "failed" "Cannot verify without package root"
    VALID=false
    (( EXIT_CODE < 1 )) && EXIT_CODE=1
fi

if $INSPECTION_AVAILABLE; then
    README_PATH="$PACKAGE_ROOT/README.md"
    if [[ -f "$README_PATH" ]]; then
        append_check "has_readme" "passed" "README.md present"
    else
        append_check "has_readme" "failed" "README.md missing"
        VALID=false
        EXIT_CODE=11
    fi
else
    append_check "has_readme" "failed" "Cannot verify without package root"
    VALID=false
    (( EXIT_CODE < 1 )) && EXIT_CODE=1
fi

if $INSPECTION_AVAILABLE; then
    EXECUTABLE_PATH=""
    if [[ -f "$PACKAGE_ROOT/bin/fifth" ]]; then
        EXECUTABLE_PATH="$PACKAGE_ROOT/bin/fifth"
    elif [[ -f "$PACKAGE_ROOT/bin/fifth.exe" ]]; then
        EXECUTABLE_PATH="$PACKAGE_ROOT/bin/fifth.exe"
    fi

    if [[ -n "$EXECUTABLE_PATH" ]]; then
        append_check "has_executable" "passed" "Executable found: ${EXECUTABLE_PATH#"$PACKAGE_ROOT/"}"
    else
        append_check "has_executable" "failed" "Compiler executable not found in bin/"
        VALID=false
        EXIT_CODE=11
    fi

    if [[ -n "$EXECUTABLE_PATH" ]]; then
        if $IS_WINDOWS_PACKAGE; then
            append_check "executable_permissions" "skipped" "Executable permissions not enforced for Windows packages"
        else
            if [[ -x "$EXECUTABLE_PATH" ]]; then
                append_check "executable_permissions" "passed" "Executable has +x permissions"
            else
                append_check "executable_permissions" "failed" "Executable missing +x permissions"
                VALID=false
                EXIT_CODE=12
            fi
        fi
    else
        append_check "executable_permissions" "failed" "Executable missing; cannot verify permissions"
        VALID=false
        [[ $EXIT_CODE -lt 11 ]] && EXIT_CODE=11
    fi

    if [[ -d "$PACKAGE_ROOT/bin" && -d "$PACKAGE_ROOT/lib" ]]; then
        append_check "directory_layout" "passed" "bin/ and lib/ directories present"
    else
        append_check "directory_layout" "failed" "Expected bin/ and lib/ directories"
        VALID=false
        (( EXIT_CODE < 1 )) && EXIT_CODE=1
    fi
else
    append_check "has_executable" "failed" "Cannot verify without package root"
    append_check "executable_permissions" "failed" "Cannot verify without package root"
    append_check "directory_layout" "failed" "Cannot verify without package root"
    VALID=false
    (( EXIT_CODE < 1 )) && EXIT_CODE=1
fi

SOFT_LIMIT=$((150 * 1024 * 1024))
if (( ARCHIVE_SIZE > SOFT_LIMIT )); then
    append_check "size_within_limits" "warning" "Package size ${ARCHIVE_SIZE} bytes exceeds 150MB soft limit"
else
    append_check "size_within_limits" "passed" "Package size ${ARCHIVE_SIZE} bytes within soft limit"
fi

python3 - "$CHECKS_FILE" "$PACKAGE_BASENAME" "$VALID" "$FILE_COUNT" "$ARCHIVE_SIZE" <<'PY'
import json
import sys

path, package, valid, file_count, archive_size = sys.argv[1:6]
valid_flag = valid.lower() == 'true'
checks = []
with open(path, 'r', encoding='utf-8') as handle:
    for line in handle:
        if not line.strip():
            continue
        name, status, message = line.rstrip('\n').split('\t', 2)
        checks.append({
            "name": name,
            "status": status,
            "message": message
        })

payload = {
    "valid": valid_flag,
    "package": package,
    "checks": checks,
    "file_count": int(file_count or 0),
    "size_bytes": int(archive_size)
}

print(json.dumps(payload, indent=2))
PY

if ! $VALID && (( EXIT_CODE == 0 )); then
    EXIT_CODE=1
fi

exit $EXIT_CODE
