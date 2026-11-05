#!/usr/bin/env bash
set -Eeuo pipefail

if [[ $# -ne 2 || ! "$1" =~ ^ghcr\.io/[a-z0-9._/-]+:sha-[a-f0-9]{40}$ || ! "$2" =~ ^[0-9]{1,5}$ ]] || (( 10#$2 < 1 || 10#$2 > 65535 )); then
  echo 'Usage: deploy.sh ghcr.io/owner/repository:sha-<40-char-commit> <port>' >&2
  exit 64
fi

cd "$HOME/credit-app"
export CREDIT_APP_IMAGE="$1" API_PORT="$2"
compose=(docker compose -f compose.production.yml)

for secret in postgres_password connection_string jwt_signing_key; do
  [[ -s "secrets/$secret.txt" ]] || { echo "Missing secrets/$secret.txt" >&2; exit 1; }
done

"${compose[@]}" config --quiet
"${compose[@]}" pull
"${compose[@]}" up -d --wait --wait-timeout 120 db
"${compose[@]}" run --rm --no-deps migrate
"${compose[@]}" up -d --no-deps api

for attempt in {1..30}; do
  status=$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 2 "http://127.0.0.1:${API_PORT}/api/credit-applications" || true)
  if [[ "$status" == 401 ]]; then
    "${compose[@]}" ps
    echo "Deployment ready: $CREDIT_APP_IMAGE"
    exit 0
  fi
  sleep 2
done

echo 'API failed its unauthenticated HTTP check after deployment.' >&2
"${compose[@]}" logs --no-color --tail=50 api >&2
exit 1
