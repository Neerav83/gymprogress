#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LABEL="se.gymprogress.update"
PLIST="$HOME/Library/LaunchAgents/${LABEL}.plist"
LOG_DIR="$HOME/Library/Logs"
INTERVAL="${UPDATE_INTERVAL:-180}"

mkdir -p "$HOME/Library/LaunchAgents" "$LOG_DIR"
chmod +x "$ROOT/scripts/update-from-main.sh"

PATH_VALUE="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin"

cat > "$PLIST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>${LABEL}</string>
  <key>ProgramArguments</key>
  <array>
    <string>/bin/bash</string>
    <string>${ROOT}/scripts/update-from-main.sh</string>
  </array>
  <key>WorkingDirectory</key>
  <string>${ROOT}</string>
  <key>RunAtLoad</key>
  <true/>
  <key>StartInterval</key>
  <integer>${INTERVAL}</integer>
  <key>StandardOutPath</key>
  <string>${LOG_DIR}/gymprogress-update.log</string>
  <key>StandardErrorPath</key>
  <string>${LOG_DIR}/gymprogress-update.log</string>
  <key>EnvironmentVariables</key>
  <dict>
    <key>HOME</key>
    <string>${HOME}</string>
    <key>PATH</key>
    <string>${PATH_VALUE}</string>
    <key>WAIT_FOR_CI</key>
    <string>1</string>
  </dict>
</dict>
</plist>
EOF

launchctl bootout "gui/$(id -u)/${LABEL}" 2>/dev/null || true
launchctl bootstrap "gui/$(id -u)" "$PLIST"
launchctl enable "gui/$(id -u)/${LABEL}" 2>/dev/null || true
launchctl kickstart -k "gui/$(id -u)/${LABEL}" 2>/dev/null || true

echo "LaunchAgent installerad: $PLIST"
echo "Kollar origin/${UPDATE_BRANCH:-Main} var ${INTERVAL}:e sekund."
echo "Logg: ${LOG_DIR}/gymprogress-update.log"
echo
echo "Avinstallera med:"
echo "  launchctl bootout gui/$(id -u)/${LABEL}"
echo "  rm -f $PLIST"
