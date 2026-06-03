#!/usr/bin/env bash
set -euo pipefail

pid_file="${1:-Deploy/asset-server.pid}"

if [[ ! -f "$pid_file" ]]; then
    echo "No PID file found: $pid_file"
    exit 0
fi

pid="$(tr -d '[:space:]' < "$pid_file")"
if [[ -z "$pid" ]]; then
    echo "PID file is empty: $pid_file"
    rm -f "$pid_file"
    exit 0
fi

if kill -0 "$pid" 2>/dev/null; then
    kill "$pid"
    echo "Stopped asset server process $pid"
else
    echo "Asset server process is not running: $pid"
fi

rm -f "$pid_file"
