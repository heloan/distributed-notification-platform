#!/usr/bin/env bash
# =============================================================================
# Run integration tests (pytest)
# Usage:
#   ./run-integration-tests.sh                  # all integration tests
#   ./run-integration-tests.sh -m gateway       # only gateway tests
#   ./run-integration-tests.sh -m e2e           # only end-to-end tests
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTS_DIR="$(dirname "$SCRIPT_DIR")"
INTEGRATION_DIR="$TESTS_DIR/integration"

echo "=========================================="
echo "  Integration Tests (pytest)"
echo "=========================================="

# Create virtual environment if not present
if [ ! -d "$TESTS_DIR/.venv" ]; then
    echo "→ Creating virtual environment..."
    python3 -m venv "$TESTS_DIR/.venv"
fi

# Activate and install deps
source "$TESTS_DIR/.venv/bin/activate"
pip install -q -r "$INTEGRATION_DIR/requirements.txt"

# Create reports directory
mkdir -p "$INTEGRATION_DIR/reports"

# Run tests
cd "$INTEGRATION_DIR"
python -m pytest "$@"

deactivate 2>/dev/null || true
echo "✅ Integration tests completed."
