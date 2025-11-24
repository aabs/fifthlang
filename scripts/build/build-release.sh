#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'EOF'
Usage: build-release.sh --version <semver> --runtime <rid> --framework <tfm> --output-dir <dir>
                        [--configuration <config>] [--verbose] [--skip-tests]
                        [--json-output <file>]
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
        exit 3
    fi
}

abs_path() {
    python3 - "$1" <<'PY'
import os, sys
print(os.path.abspath(sys.argv[1]))
PY
}

file_size_bytes() {
    python3 - "$1" <<'PY'
import os, sys
print(os.path.getsize(sys.argv[1]))
PY
}

VERSION=""
RUNTIME=""
FRAMEWORK=""
OUTPUT_DIR=""
CONFIGURATION="Release"
VERBOSE=false
SKIP_TESTS=false
JSON_OUTPUT=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --version)
            shift || { usage >&2; exit 2; }
            VERSION="${1:-}"
            ;;
        --runtime)
            shift || { usage >&2; exit 2; }
            RUNTIME="${1:-}"
            ;;
        --framework)
            shift || { usage >&2; exit 2; }
            FRAMEWORK="${1:-}"
            ;;
        --output-dir)
            shift || { usage >&2; exit 2; }
            OUTPUT_DIR="${1:-}"
            ;;
        --configuration)
            shift || { usage >&2; exit 2; }
            CONFIGURATION="${1:-}"
            ;;
        --verbose)
            VERBOSE=true
            ;;
        --skip-tests)
            SKIP_TESTS=true
            ;;
        --json-output)
            shift || { usage >&2; exit 2; }
            JSON_OUTPUT="${1:-}"
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

if [[ -z "$VERSION" || -z "$RUNTIME" || -z "$FRAMEWORK" || -z "$OUTPUT_DIR" ]]; then
    error "Missing required arguments"
    usage >&2
    exit 2
fi

require_command dotnet
require_command python3

if [[ -n "$JSON_OUTPUT" && -d "$JSON_OUTPUT" ]]; then
    error "--json-output expects a file path, but '$JSON_OUTPUT' is a directory"
    exit 2
fi

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
COMPILER_PROJECT="$REPO_ROOT/src/compiler/compiler.csproj"

if [[ ! -f "$COMPILER_PROJECT" ]]; then
    error "Compiler project not found at $COMPILER_PROJECT"
    exit 4
fi

START_TIME=$(date +%s)
WORK_DIR=$(mktemp -d "${TMPDIR:-/tmp}/fifth-build-XXXXXX")
trap 'rm -rf "$WORK_DIR"' EXIT

PUBLISH_DIR="$WORK_DIR/publish"
STAGING_DIR="$WORK_DIR/fifth-$VERSION"
mkdir -p "$PUBLISH_DIR" "$STAGING_DIR/bin" "$STAGING_DIR/lib"

log "Building Fifth compiler v$VERSION for $RUNTIME ($FRAMEWORK)"
if $SKIP_TESTS; then
    warn "--skip-tests specified; assuming prior validation"
fi

DOTNET_ARGS=(
    "publish" "$COMPILER_PROJECT"
    "-c" "$CONFIGURATION"
    "-r" "$RUNTIME"
    "-f" "$FRAMEWORK"
    "--self-contained" "true"
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "-p:Version=$VERSION"
    "-p:InformationalVersion=$VERSION"
    "-o" "$PUBLISH_DIR"
)
dotnet "${DOTNET_ARGS[@]}"

log "Staging artifacts"
EXECUTABLE_SOURCE=""
if [[ -f "$PUBLISH_DIR/compiler.exe" ]]; then
    EXECUTABLE_SOURCE="$PUBLISH_DIR/compiler.exe"
elif [[ -f "$PUBLISH_DIR/compiler" ]]; then
    EXECUTABLE_SOURCE="$PUBLISH_DIR/compiler"
else
    EXECUTABLE_SOURCE=$(find "$PUBLISH_DIR" -maxdepth 1 -type f -perm -111 | head -n1 || true)
fi

if [[ -z "$EXECUTABLE_SOURCE" ]]; then
    error "Unable to locate published executable in $PUBLISH_DIR"
    exit 1
fi

if [[ "$RUNTIME" == win-* ]]; then
    TARGET_EXE="fifth.exe"
else
    TARGET_EXE="fifth"
fi

cp "$EXECUTABLE_SOURCE" "$STAGING_DIR/bin/$TARGET_EXE"
chmod +x "$STAGING_DIR/bin/$TARGET_EXE"

DEPENDENCY_PUBLISH_DIR="$WORK_DIR/publish-framework"
dotnet publish "$COMPILER_PROJECT" \
    -c "$CONFIGURATION" \
    -f "$FRAMEWORK" \
    -p:PublishSingleFile=false \
    -p:SelfContained=false \
    -o "$DEPENDENCY_PUBLISH_DIR"

cp -a "$DEPENDENCY_PUBLISH_DIR/." "$STAGING_DIR/lib/"
rm -f "$STAGING_DIR/lib/compiler" "$STAGING_DIR/lib/compiler.exe"

README_SRC="$REPO_ROOT/README.md"
LICENSE_SRC="$REPO_ROOT/LICENSE"

[[ -f "$README_SRC" ]] && cp "$README_SRC" "$STAGING_DIR/README.md"
[[ -f "$LICENSE_SRC" ]] && cp "$LICENSE_SRC" "$STAGING_DIR/LICENSE"
printf '%s\n' "$VERSION" > "$STAGING_DIR/VERSION.txt"

ARCHIVE_FORMAT="tar.gz"
if [[ "$RUNTIME" == win-* ]]; then
    ARCHIVE_FORMAT="zip"
fi

mkdir -p "$OUTPUT_DIR"
OUTPUT_DIR_ABS=$(abs_path "$OUTPUT_DIR")
PACKAGE_NAME="fifth-${VERSION}-${RUNTIME}-${FRAMEWORK}.${ARCHIVE_FORMAT}"
OUTPUT_PACKAGE="$OUTPUT_DIR_ABS/$PACKAGE_NAME"

ARCHIVE_JSON=$("$SCRIPT_DIR/create-archives.sh" \
    --source-dir "$STAGING_DIR" \
    --output-file "$OUTPUT_PACKAGE" \
    --format "$ARCHIVE_FORMAT" \
    --version "$VERSION" \
    --include-readme \
    --include-license)

PACKAGE_SIZE=$(file_size_bytes "$OUTPUT_PACKAGE")
BIN_SIZE=$(file_size_bytes "$STAGING_DIR/bin/$TARGET_EXE")
README_SIZE=$(file_size_bytes "$STAGING_DIR/README.md")
LICENSE_SIZE=$(file_size_bytes "$STAGING_DIR/LICENSE")
VERSION_SIZE=$(file_size_bytes "$STAGING_DIR/VERSION.txt")

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

JSON_PAYLOAD=$(cat <<EOF
{
    "success": true,
    "package_path": "${OUTPUT_PACKAGE}",
    "package_size_bytes": ${PACKAGE_SIZE},
    "runtime": "${RUNTIME}",
    "framework": "${FRAMEWORK}",
    "version": "${VERSION}",
    "build_time_seconds": ${DURATION},
    "artifacts": [
        {"type": "executable", "path": "bin/${TARGET_EXE}", "size_bytes": ${BIN_SIZE}},
        {"type": "document", "path": "README.md", "size_bytes": ${README_SIZE}},
        {"type": "document", "path": "LICENSE", "size_bytes": ${LICENSE_SIZE}},
        {"type": "metadata", "path": "VERSION.txt", "size_bytes": ${VERSION_SIZE}}
    ],
    "archive": ${ARCHIVE_JSON}
}
EOF
)

if [[ -n "$JSON_OUTPUT" ]]; then
        mkdir -p "$(dirname "$JSON_OUTPUT")"
        printf '%s\n' "$JSON_PAYLOAD" > "$JSON_OUTPUT"
else
        printf '%s\n' "$JSON_PAYLOAD"
fi