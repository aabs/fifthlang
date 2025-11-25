#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'EOF'
Usage: generate-checksums.sh --package-dir <dir> --output-file <file> [--format <gnu|bsd>] [--verify]
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

calculate_checksum() {
    local file="$1"
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$file" | awk '{print $1}'
    else
        shasum -a 256 "$file" | awk '{print $1}'
    fi
}

PACKAGE_DIR=""
OUTPUT_FILE=""
FORMAT="gnu"
VERIFY=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --package-dir)
            shift || { usage >&2; exit 2; }
            PACKAGE_DIR="${1:-}"
            ;;
        --output-file)
            shift || { usage >&2; exit 2; }
            OUTPUT_FILE="${1:-}"
            ;;
        --format)
            shift || { usage >&2; exit 2; }
            FORMAT="${1:-}"
            ;;
        --verify)
            VERIFY=true
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

if [[ -z "$PACKAGE_DIR" || -z "$OUTPUT_FILE" ]]; then
    error "Missing required arguments"
    usage >&2
    exit 2
fi

if [[ ! -d "$PACKAGE_DIR" ]]; then
    error "Package directory '$PACKAGE_DIR' not found"
    exit 4
fi

case "$FORMAT" in
    gnu|bsd)
        ;;
    *)
        error "Unsupported format '$FORMAT'"
        exit 2
        ;;
esac

require_command python3

PACKAGE_DIR_ABS=$(abs_path "$PACKAGE_DIR")
OUTPUT_FILE_ABS=$(abs_path "$(dirname "$OUTPUT_FILE")")/$(basename "$OUTPUT_FILE")

cd "$PACKAGE_DIR_ABS"

if $VERIFY; then
    if [[ ! -f "$OUTPUT_FILE_ABS" ]]; then
        error "Checksum manifest '$OUTPUT_FILE_ABS' not found for verification"
        exit 6
    fi
    log "Verifying checksums in $PACKAGE_DIR_ABS"
    STATUS=0
    while IFS= read -r line || [[ -n "$line" ]]; do
        [[ -z "$line" ]] && continue
        filename=""
        expected=""
        if [[ "$line" =~ ^SHA256\ \((.*)\)\ =\ ([0-9a-fA-F]{64})$ ]]; then
            filename="${BASH_REMATCH[1]}"
            expected="${BASH_REMATCH[2]}"
        elif [[ "$line" =~ ^([0-9a-fA-F]{64})[[:space:]]{2}(.*)$ ]]; then
            expected="${BASH_REMATCH[1]}"
            filename="${BASH_REMATCH[2]}"
        else
            warn "Unrecognized manifest line format: $line"
            STATUS=6
            continue
        fi
        filename="${filename##[[:space:]*]*}"
        file_path="$filename"
        if [[ ! -f "$file_path" ]]; then
            warn "File '$file_path' referenced in manifest is missing"
            STATUS=6
            continue
        fi
        actual=$(calculate_checksum "$file_path")
        if [[ "$actual" != "$expected" ]]; then
            warn "Checksum mismatch for $file_path"
            STATUS=6
        fi
    done < "$OUTPUT_FILE_ABS"
    if (( STATUS != 0 )); then
        exit 6
    fi
    log "Checksum verification passed"
    exit 0
fi

log "Generating checksums in $PACKAGE_DIR_ABS"
mkdir -p "$(dirname "$OUTPUT_FILE_ABS")"

mapfile -t FILES < <(find . -maxdepth 1 -type f \( -name '*.tar.gz' -o -name '*.zip' \) | sort)
if [[ ${#FILES[@]} -eq 0 ]]; then
    warn "No package files found in $PACKAGE_DIR_ABS"
fi

TMP_MANIFEST="${OUTPUT_FILE_ABS}.tmp"
rm -f "$TMP_MANIFEST"

declare -a ENTRY_DATA

for rel_path in "${FILES[@]}"; do
    file="${rel_path#./}"
    [[ -z "$file" ]] && continue
    sum=$(calculate_checksum "$file")
    if [[ "$FORMAT" == "gnu" ]]; then
        printf '%s  %s\n' "$sum" "$file" >> "$TMP_MANIFEST"
    else
        printf 'SHA256 (%s) = %s\n' "$file" "$sum" >> "$TMP_MANIFEST"
    fi
    size=$(file_size_bytes "$file")
    ENTRY_DATA+=("$file|$sum|$size")
done

mv "$TMP_MANIFEST" "$OUTPUT_FILE_ABS"

PACKAGE_COUNT=${#ENTRY_DATA[@]}
JSON_ENTRIES="[]"
if [[ ${#ENTRY_DATA[@]} -gt 0 ]]; then
    JSON_ENTRIES=$(python3 - <<'PY' "${ENTRY_DATA[@]}"
import json, sys
rows = sys.argv[1:]
entries = []
for row in rows:
    name, checksum, size = row.split('|', 2)
    entries.append({
        "filename": name,
        "sha256": checksum,
        "size_bytes": int(size)
    })
print(json.dumps(entries))
PY
    )
fi

cat <<EOF
{
  "success": true,
  "manifest_path": "${OUTPUT_FILE_ABS}",
  "package_count": ${PACKAGE_COUNT},
  "format": "${FORMAT}",
  "entries": ${JSON_ENTRIES}
}
EOF