#!/usr/bin/env bash
set -euo pipefail

root="Deploy/AssetServer"
port="8000"
bind="0.0.0.0"
pid_file="Deploy/asset-server.pid"
foreground=0

usage() {
    cat <<'USAGE'
Usage: start-local-asset-server.sh [options]

Options:
  --root PATH       Static server root. Default: Deploy/AssetServer
  --port PORT       HTTP port. Default: 8000
  --bind ADDRESS    Bind address. Default: 0.0.0.0
  --foreground      Run in the current terminal instead of writing a PID file.
  -h, --help        Show this help.
USAGE
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --root)
            root="$2"
            shift 2
            ;;
        --port)
            port="$2"
            shift 2
            ;;
        --bind)
            bind="$2"
            shift 2
            ;;
        --foreground)
            foreground=1
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

mkdir -p "$root" "$(dirname "$pid_file")"
root_full="$(cd "$root" && pwd)"

if [[ -f "$pid_file" ]]; then
    old_pid="$(tr -d '[:space:]' < "$pid_file")"
    if [[ -n "$old_pid" ]] && kill -0 "$old_pid" 2>/dev/null; then
        echo "Asset server already running: pid=$old_pid"
        echo "Root: $root_full"
        echo "LocalUrl: http://127.0.0.1:$port/"
        exit 0
    fi
fi

python_bin="${PYTHON:-}"
if [[ -z "$python_bin" ]]; then
    python_bin="$(command -v python3 || true)"
fi
if [[ -z "$python_bin" ]]; then
    echo "python3 was not found. Install Python or set PYTHON=/path/to/python." >&2
    exit 2
fi

if [[ "$foreground" == "1" ]]; then
    echo "Root: $root_full"
    echo "LocalUrl: http://127.0.0.1:$port/"
    if command -v ifconfig >/dev/null 2>&1; then
        ifconfig | awk '/inet / && $2 !~ /^127\./ { print "LanUrl: http://" $2 ":'"$port"'/"}'
    fi
    exec "$python_bin" -m http.server "$port" --bind "$bind" --directory "$root_full"
fi

out_log="Deploy/asset-server.out.log"
err_log="Deploy/asset-server.err.log"
nohup "$python_bin" -m http.server "$port" --bind "$bind" --directory "$root_full" >"$out_log" 2>"$err_log" </dev/null &
pid="$!"
sleep 0.5

if ! kill -0 "$pid" 2>/dev/null; then
    echo "Asset server exited immediately." >&2
    [[ -f "$err_log" ]] && cat "$err_log" >&2
    exit 3
fi

echo "$pid" > "$pid_file"

probe_host="$bind"
if [[ "$probe_host" == "0.0.0.0" || "$probe_host" == "::" ]]; then
    probe_host="127.0.0.1"
fi
if [[ "$probe_host" == *:* ]]; then
    probe_url="http://[$probe_host]:$port/"
else
    probe_url="http://$probe_host:$port/"
fi

probe_once() {
    if command -v curl >/dev/null 2>&1; then
        curl -fsS --max-time 1 "$probe_url" >/dev/null 2>&1
    else
        "$python_bin" - "$probe_url" >/dev/null 2>&1 <<'PY'
import sys
import urllib.request

with urllib.request.urlopen(sys.argv[1], timeout=1) as response:
    response.read(1)
PY
    fi
}

ready=0
for _ in {1..20}; do
    if ! kill -0 "$pid" 2>/dev/null; then
        echo "Asset server exited before responding." >&2
        [[ -f "$err_log" ]] && cat "$err_log" >&2
        rm -f "$pid_file"
        exit 3
    fi
    if probe_once; then
        ready=1
        break
    fi
    sleep 0.25
done

if [[ "$ready" != "1" ]]; then
    echo "Asset server did not respond at $probe_url" >&2
    [[ -f "$err_log" ]] && cat "$err_log" >&2
    kill "$pid" 2>/dev/null || true
    rm -f "$pid_file"
    exit 4
fi

echo "ProcessId: $pid"
echo "Root: $root_full"
echo "LocalUrl: http://127.0.0.1:$port/"
if command -v ifconfig >/dev/null 2>&1; then
    ifconfig | awk '/inet / && $2 !~ /^127\./ { print "LanUrl: http://" $2 ":'"$port"'/"}'
fi
echo "PidFile: $pid_file"
