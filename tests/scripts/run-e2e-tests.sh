#!/usr/bin/env bash
# =============================================================================
# Run end-to-end tests only (subset of integration tests)
# Requires ALL services to be running.
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=========================================="
echo "  End-to-End Tests"
echo "=========================================="

"$SCRIPT_DIR/run-integration-tests.sh" -m e2e "$@"
