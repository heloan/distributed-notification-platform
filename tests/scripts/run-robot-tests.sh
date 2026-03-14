#!/usr/bin/env bash
# =============================================================================
# Run Robot Framework tests
# Usage:
#   ./run-robot-tests.sh                        # all robot tests
#   ./run-robot-tests.sh --include gateway      # only gateway tag
#   ./run-robot-tests.sh --include e2e          # only end-to-end tag
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTS_DIR="$(dirname "$SCRIPT_DIR")"
ROBOT_DIR="$TESTS_DIR/robot"

echo "=========================================="
echo "  Robot Framework Tests"
echo "=========================================="

# Create virtual environment if not present
if [ ! -d "$TESTS_DIR/.venv" ]; then
    echo "→ Creating virtual environment..."
    python3 -m venv "$TESTS_DIR/.venv"
fi

source "$TESTS_DIR/.venv/bin/activate"
pip install -q -r "$TESTS_DIR/integration/requirements.txt"

# Create output directory
mkdir -p "$ROBOT_DIR/results"

# Run robot tests
robot \
    --outputdir "$ROBOT_DIR/results" \
    --loglevel INFO \
    "$@" \
    "$ROBOT_DIR"

deactivate 2>/dev/null || true
echo "✅ Robot Framework tests completed."
