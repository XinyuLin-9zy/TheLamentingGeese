#!/usr/bin/env bash
set -euo pipefail

source_root="BuildAssetBundles"
output_root="Deploy/AssetServer"
game_id="theweepingswan"
version="1.0.0"
platforms=("Android" "iOS")
clean=0

usage() {
    cat <<'USAGE'
Usage: prepare-asset-bundle-deploy.sh [options]

Options:
  --source-root PATH     AssetBundle build output root. Default: BuildAssetBundles
  --output-root PATH     Static server root. Default: Deploy/AssetServer
  --game-id ID           URL game id path. Default: theweepingswan
  --version VERSION      URL version path. Default: 1.0.0
  --platforms LIST       Comma-separated platforms. Default: Android,iOS
  --platform NAME        Add one platform. Can be repeated.
  --clean                Remove the version output before copying.
  -h, --help             Show this help.
USAGE
}

set_platforms_from_csv() {
    IFS=',' read -r -a platforms <<< "$1"
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --source-root)
            source_root="$2"
            shift 2
            ;;
        --output-root)
            output_root="$2"
            shift 2
            ;;
        --game-id)
            game_id="$2"
            shift 2
            ;;
        --version)
            version="$2"
            shift 2
            ;;
        --platforms)
            set_platforms_from_csv "$2"
            shift 2
            ;;
        --platform)
            if [[ "${platforms[*]}" == "Android iOS" ]]; then
                platforms=()
            fi
            platforms+=("$2")
            shift 2
            ;;
        --clean)
            clean=1
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage >&2
            exit 1
            ;;
    esac
done

if [[ ! -d "$source_root" ]]; then
    echo "SourceRoot does not exist: $source_root" >&2
    exit 2
fi

mkdir -p "$output_root"
source_root_full="$(cd "$source_root" && pwd)"
output_root_full="$(cd "$output_root" && pwd)"
version_root="$output_root_full/$game_id/$version"

if [[ "$clean" == "1" && -d "$version_root" ]]; then
    case "$version_root" in
        "$output_root_full"/*) rm -rf "$version_root" ;;
        *) echo "Refusing to clean outside OutputRoot: $version_root" >&2; exit 3 ;;
    esac
fi

mkdir -p "$version_root"
missing=0

printf "%-12s %-16s %s\n" "Platform" "Status" "Output"
for platform in "${platforms[@]}"; do
    platform="${platform//[[:space:]]/}"
    [[ -z "$platform" ]] && continue

    src="$source_root_full/$platform"
    dst="$version_root/$platform"
    if [[ ! -d "$src" ]]; then
        printf "%-12s %-16s %s\n" "$platform" "MissingSource" "$src"
        missing=1
        continue
    fi

    rm -rf "$dst"
    mkdir -p "$dst"
    cp -R "$src"/. "$dst"/

    if [[ -f "$dst/$platform" ]]; then
        printf "%-12s %-16s %s\n" "$platform" "Ready" "$dst"
    else
        printf "%-12s %-16s %s\n" "$platform" "MissingManifest" "$dst/$platform"
        missing=1
    fi
done

echo
echo "ServerUrl should point to the version root, not the platform folder:"
echo "  http://<host>/$game_id/$version"
echo "Local default:"
echo "  http://127.0.0.1:8000/$game_id/$version"

exit "$missing"
