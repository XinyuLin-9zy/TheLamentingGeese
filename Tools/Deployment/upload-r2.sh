#!/usr/bin/env bash
set -euo pipefail

bucket=""
local_root="Deploy/AssetServer"
prefix=""

usage() {
    cat <<'USAGE'
Usage: upload-r2.sh --bucket BUCKET [options]

Options:
  --bucket BUCKET      Cloudflare R2 bucket name. Required.
  --local-root PATH    Local static server root. Default: Deploy/AssetServer
  --prefix PREFIX      Optional object key prefix.
  -h, --help           Show this help.
USAGE
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bucket)
            bucket="$2"
            shift 2
            ;;
        --local-root)
            local_root="$2"
            shift 2
            ;;
        --prefix)
            prefix="$2"
            shift 2
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

if [[ -z "$bucket" ]]; then
    echo "--bucket is required" >&2
    usage >&2
    exit 1
fi
if [[ ! -d "$local_root" ]]; then
    echo "LocalRoot does not exist: $local_root" >&2
    exit 2
fi
if ! command -v wrangler >/dev/null 2>&1; then
    echo "wrangler was not found. Install it with: npm install -g wrangler" >&2
    exit 3
fi

root_full="$(cd "$local_root" && pwd)"
count=0

while IFS= read -r -d '' file; do
    relative="${file#$root_full/}"
    key="$relative"
    if [[ -n "$prefix" ]]; then
        key="${prefix%/}/$relative"
    fi
    echo "Uploading $relative -> r2://$bucket/$key"
    wrangler r2 object put "$bucket/$key" --file "$file"
    count=$((count + 1))
done < <(find "$root_full" -type f -print0)

echo "Uploaded $count files to bucket $bucket"
