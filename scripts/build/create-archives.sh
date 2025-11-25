#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'EOF'
Usage: create-archives.sh --source-dir <path> --output-file <path> --format <tar.gz|zip> --version <version>
                         [--include-readme] [--include-license] [--verbose]
EOF
}

log() {
    echo "[INFO] $*" >&2
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

SOURCE_DIR=""
OUTPUT_FILE=""
FORMAT=""
VERSION=""
INCLUDE_README=false
INCLUDE_LICENSE=false
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --source-dir)
            shift || { usage >&2; exit 2; }
            SOURCE_DIR="${1:-}"
            ;;
        --output-file)
            shift || { usage >&2; exit 2; }
            OUTPUT_FILE="${1:-}"
            ;;
        --format)
            shift || { usage >&2; exit 2; }
            FORMAT="${1:-}"
            ;;
        --version)
            shift || { usage >&2; exit 2; }
            VERSION="${1:-}"
            ;;
        --include-readme)
            INCLUDE_README=true
            ;;
        --include-license)
            INCLUDE_LICENSE=true
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

if [[ -z "$SOURCE_DIR" || -z "$OUTPUT_FILE" || -z "$FORMAT" || -z "$VERSION" ]]; then
    error "Missing required arguments"
    usage >&2
    exit 2
fi

if [[ ! -d "$SOURCE_DIR" ]]; then
    error "Source directory '$SOURCE_DIR' not found"
    exit 4
fi

case "$FORMAT" in
    tar.gz|zip)
        ;;
    *)
        error "Unsupported format '$FORMAT'"
        exit 2
        ;;
esac

require_command python3

OS_NAME=$(uname -s 2>/dev/null || echo "")
IS_WINDOWS=false
case "$OS_NAME" in
    MINGW*|MSYS*|CYGWIN*|Windows_NT)
        IS_WINDOWS=true
        ;;
esac

to_windows_path() {
    local path="$1"
    if command -v cygpath >/dev/null 2>&1; then
        cygpath -w "$path"
    else
        python3 - "$path" <<'PY'
import os, sys
print(os.path.abspath(sys.argv[1]).replace('/', '\\'))
PY
    fi
}

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)

TARGET_ROOT=$(abs_path "$SOURCE_DIR")
OUTPUT_FILE_ABS=$(abs_path "$(dirname "$OUTPUT_FILE")")/$(basename "$OUTPUT_FILE")
mkdir -p "$(dirname "$OUTPUT_FILE_ABS")"

copy_if_missing() {
    local source_file="$1"
    local target_file="$2"
    if [[ -f "$source_file" && ! -f "$target_file" ]]; then
        cp "$source_file" "$target_file"
    fi
}

if $INCLUDE_README; then
    copy_if_missing "$REPO_ROOT/README.md" "$TARGET_ROOT/README.md"
fi

if $INCLUDE_LICENSE; then
    copy_if_missing "$REPO_ROOT/LICENSE" "$TARGET_ROOT/LICENSE"
fi

FILE_COUNT=$(find "$TARGET_ROOT" -type f | wc -l | tr -d ' \t')
SOURCE_BYTES=$(python3 - "$TARGET_ROOT" <<'PY'
import os, sys
total = 0
for root, _, files in os.walk(sys.argv[1]):
    for name in files:
        total += os.path.getsize(os.path.join(root, name))
print(total)
PY
)

TMP_OUTPUT="${OUTPUT_FILE_ABS}.tmp"
rm -f "$TMP_OUTPUT"

if [[ "$FORMAT" == "tar.gz" ]]; then
    require_command tar
    log "Creating tar.gz archive at $OUTPUT_FILE_ABS"
    tar -C "$(dirname "$TARGET_ROOT")" -czf "$TMP_OUTPUT" "$(basename "$TARGET_ROOT")"
else
    if $IS_WINDOWS; then
        require_command pwsh
        log "Creating zip archive at $OUTPUT_FILE_ABS (Compress-Archive)"
        TARGET_PARENT_WIN=$(to_windows_path "$(dirname "$TARGET_ROOT")")
        TARGET_BASENAME=$(basename "$TARGET_ROOT")
        TMP_OUTPUT_WIN=$(to_windows_path "$TMP_OUTPUT")
        ZIP_SCRIPT=$(mktemp "${TMPDIR:-/tmp}/zip-script-XXXXXX.ps1")
        cat <<'PWSH' > "$ZIP_SCRIPT"
param(
    [string]$SourceRoot,
    [string]$FolderName,
    [string]$Destination
)
$sourcePath = Join-Path $SourceRoot $FolderName
if (Test-Path $Destination) { Remove-Item $Destination -Force }
Compress-Archive -Path $sourcePath -DestinationPath $Destination -Force
PWSH
        set +u
        pwsh -NoLogo -NoProfile -File "$ZIP_SCRIPT" -SourceRoot "$TARGET_PARENT_WIN" -FolderName "$TARGET_BASENAME" -Destination "$TMP_OUTPUT_WIN"
        ZIP_STATUS=$?
        rm -f "$ZIP_SCRIPT"
        set -u
        if [[ "$ZIP_STATUS" -ne 0 ]]; then
            error "Compress-Archive failed"
            exit "$ZIP_STATUS"
        fi
    else
        require_command zip
        require_command unzip
        pushd "$(dirname "$TARGET_ROOT")" >/dev/null
        log "Creating zip archive at $OUTPUT_FILE_ABS"
        zip -qry "$TMP_OUTPUT" "$(basename "$TARGET_ROOT")"
        popd >/dev/null
    fi
fi

if [[ "$FORMAT" == "tar.gz" ]]; then
    tar -tzf "$TMP_OUTPUT" >/dev/null
else
    if $IS_WINDOWS; then
        require_command pwsh
        TMP_OUTPUT_WIN=$(to_windows_path "$TMP_OUTPUT")
        VALIDATE_SCRIPT=$(mktemp "${TMPDIR:-/tmp}/zip-validate-XXXXXX.ps1")
        cat <<'PWSH' > "$VALIDATE_SCRIPT"
param(
    [string]$Archive
)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($Archive)
$zip.Dispose()
PWSH
        set +u
        pwsh -NoLogo -NoProfile -File "$VALIDATE_SCRIPT" -Archive "$TMP_OUTPUT_WIN"
        VALIDATE_STATUS=$?
        rm -f "$VALIDATE_SCRIPT"
        set -u
        if [[ "$VALIDATE_STATUS" -ne 0 ]]; then
            error "Zip validation failed"
            exit "$VALIDATE_STATUS"
        fi
    else
        unzip -tqq "$TMP_OUTPUT" >/dev/null
    fi
fi

mv "$TMP_OUTPUT" "$OUTPUT_FILE_ABS"

ARCHIVE_BYTES=$(file_size_bytes "$OUTPUT_FILE_ABS")
ARCHIVE_JSON=$(ARCHIVE_PATH="$OUTPUT_FILE_ABS" ARCHIVE_BYTES="$ARCHIVE_BYTES" FILE_COUNT="$FILE_COUNT" SOURCE_BYTES="$SOURCE_BYTES" FORMAT="$FORMAT" VERSION="$VERSION" python3 <<'PY'
import json
import os

archive_path = os.environ["ARCHIVE_PATH"]
archive_bytes = int(os.environ["ARCHIVE_BYTES"])
file_count = int(os.environ["FILE_COUNT"])
source_bytes = int(os.environ.get("SOURCE_BYTES", "0"))
ratio = 0.0 if source_bytes == 0 else archive_bytes / source_bytes
payload = {
    "success": True,
    "archive_path": archive_path,
    "archive_size_bytes": archive_bytes,
    "file_count": file_count,
    "compression_ratio": f"{ratio:.4f}",
    "format": os.environ["FORMAT"],
    "version": os.environ["VERSION"],
}
print(json.dumps(payload))
PY
)

printf '%s\n' "$ARCHIVE_JSON"