#!/usr/bin/env bash
set -euo pipefail

FORMAT="json"
ALLOW_DIRTY=false

usage() {
    cat <<'EOF'
Usage: version-info.sh [--format json|text|semver] [--allow-dirty]
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --format)
            shift || { usage >&2; exit 2; }
            FORMAT="${1:-}"
            ;;
        --allow-dirty)
            ALLOW_DIRTY=true
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown argument: $1" >&2
            usage >&2
            exit 2
            ;;
    esac
    shift || true
done

if ! command -v git >/dev/null 2>&1; then
    echo "git is required" >&2
    exit 7
fi

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "Not a git repository" >&2
    exit 7
fi

COMMIT_SHA=$(git rev-parse HEAD)
COMMIT_SHORT=$(git rev-parse --short HEAD)
BRANCH=$(git rev-parse --abbrev-ref HEAD)
BUILD_TS=$(date -u +%Y-%m-%dT%H:%M:%SZ)
BUILD_TS_COMPACT=$(date -u +%Y%m%d%H%M%S)

if [[ "$ALLOW_DIRTY" == false ]]; then
    if [[ -n "$(git status --porcelain)" ]]; then
        echo "Working tree has uncommitted changes. Use --allow-dirty to override." >&2
        exit 9
    fi
fi
IS_DIRTY=false
if [[ -n "$(git status --porcelain)" ]]; then
    IS_DIRTY=true
fi

trim_v_prefix() {
    local input="$1"
    if [[ "$input" == v* ]]; then
        echo "${input:1}"
    else
        echo "$input"
    fi
}

VERSION_TAG=""
IS_PRERELEASE=false
BASE_VERSION=""

# Prefer GitHub-provided tag ref when available
if [[ -n "${GITHUB_REF:-}" && "$GITHUB_REF" == refs/tags/* ]]; then
    VERSION_TAG=${GITHUB_REF#refs/tags/}
fi

if [[ -z "$VERSION_TAG" ]]; then
    if git describe --tags --exact-match >/dev/null 2>&1; then
        VERSION_TAG=$(git describe --tags --exact-match)
    fi
fi

if [[ -n "$VERSION_TAG" ]]; then
    VERSION=$(trim_v_prefix "$VERSION_TAG")
    if [[ "$VERSION" == *-* ]]; then
        IS_PRERELEASE=true
    fi
else
    if git describe --tags --abbrev=0 >/dev/null 2>&1; then
        BASE_VERSION=$(trim_v_prefix "$(git describe --tags --abbrev=0)")
    else
        BASE_VERSION="0.1.0"
    fi
    VERSION="${BASE_VERSION}-pre.$(date -u +%Y%m%d).${COMMIT_SHORT}"
    IS_PRERELEASE=true
fi

case "$FORMAT" in
    json)
        VERSION_TAG_JSON="null"
        if [[ -n "$VERSION_TAG" ]]; then
            VERSION_TAG_JSON="\"$VERSION_TAG\""
        fi
        cat <<EOF
{
  "version": "${VERSION}",
  "version_tag": ${VERSION_TAG_JSON},
  "commit_sha": "${COMMIT_SHA}",
  "commit_short": "${COMMIT_SHORT}",
  "is_prerelease": ${IS_PRERELEASE},
  "is_dirty": ${IS_DIRTY},
  "branch": "${BRANCH}",
  "build_timestamp": "${BUILD_TS}"
}
EOF
        ;;
    text)
        printf '%s\n' "$VERSION"
        ;;
    semver)
        printf '%s+%s.%s\n' "$VERSION" "$COMMIT_SHORT" "$BUILD_TS_COMPACT"
        ;;
    *)
        echo "Unknown format: $FORMAT" >&2
        usage >&2
        exit 2
        ;;
 esac
