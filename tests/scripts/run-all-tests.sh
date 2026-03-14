#!/usr/bin/env bash
# =============================================================================
# Run ALL test suites (integration + robot + selenium)
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo "=========================================="
echo "  DSNP — Running All Tests"
echo "=========================================="

"$SCRIPT_DIR/run-integration-tests.sh" "$@"
"$SCRIPT_DIR/run-robot-tests.sh" "$@"
"$SCRIPT_DIR/run-selenium-tests.sh" "$@"

echo ""
echo "✅ All test suites completed."
