#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOCK_DIR="$ROOT/.update.lock"
WAIT_FOR_CI="${WAIT_FOR_CI:-1}"
BRANCH="${UPDATE_BRANCH:-Main}"

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') $*"
}

if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  log "Redan igång, hoppar över."
  exit 0
fi
trap 'rmdir "$LOCK_DIR"' EXIT

cd "$ROOT"

if [[ ! -f .env ]]; then
  log "Saknar .env i $ROOT — avbryter."
  exit 1
fi

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  log "Inte ett git-repo."
  exit 1
fi

current="$(git branch --show-current || true)"
if [[ "$current" != "$BRANCH" ]]; then
  log "Står på '$current', inte $BRANCH. Hoppar över (så osparad kod inte skrivs över)."
  exit 0
fi

if [[ -n "$(git status --porcelain)" ]]; then
  log "Working tree är inte rent. Hoppar över."
  exit 0
fi

git fetch origin "refs/heads/$BRANCH:refs/remotes/origin/$BRANCH"
local_sha="$(git rev-parse HEAD)"
remote_sha="$(git rev-parse "origin/$BRANCH")"

if [[ "$local_sha" == "$remote_sha" ]]; then
  log "Redan på senaste $BRANCH ($local_sha)."
  exit 0
fi

log "Ny kod på origin/$BRANCH: $local_sha → $remote_sha"

if [[ "$WAIT_FOR_CI" == "1" ]] && command -v gh >/dev/null 2>&1; then
  status="$(gh run list --commit "$remote_sha" --workflow CI --limit 1 --json status --jq '.[0].status // empty' 2>/dev/null || true)"
  conclusion="$(gh run list --commit "$remote_sha" --workflow CI --limit 1 --json conclusion --jq '.[0].conclusion // empty' 2>/dev/null || true)"

  if [[ -z "$status" ]]; then
    log "CI har inte startat för $remote_sha än. Väntar till nästa körning."
    exit 0
  fi
  if [[ "$status" != "completed" ]]; then
    log "CI pågår ($status) för $remote_sha. Väntar."
    exit 0
  fi
  if [[ "$conclusion" != "success" ]]; then
    log "CI misslyckades ($conclusion) för $remote_sha. Deployar inte."
    exit 0
  fi
  log "CI är grön för $remote_sha."
elif [[ "$WAIT_FOR_CI" == "1" ]]; then
  log "gh saknas, hoppar över CI-kollen."
fi

git pull --ff-only origin "$BRANCH"
log "Bygger och startar Docker-stacken."
docker compose up -d --build --wait --wait-timeout 180
log "Servern är uppdaterad till $(git rev-parse --short HEAD)."
