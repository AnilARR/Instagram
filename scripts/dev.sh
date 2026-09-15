#!/usr/bin/env bash
# Starts the API and the Angular dev server together and stops both on Ctrl+C.
# Windows users: run the two commands from the README in separate terminals.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cleanup() {
  # Kill the whole process group so `dotnet run` and `ng serve` both go down.
  trap - INT TERM EXIT
  kill 0 2>/dev/null || true
}
trap cleanup INT TERM EXIT

echo "→ API      http://localhost:5065  (Swagger at /swagger)"
echo "→ Frontend http://localhost:4200"
echo

ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project "$ROOT/backend/Instagram.Api" --urls http://localhost:5065 &

(cd "$ROOT/frontend" && npm start) &

wait
