#!/usr/bin/env bash
# =============================================================================
# Run Selenium browser tests
# Usage:
#   ./run-selenium-tests.sh                     # all selenium tests
#   ./run-selenium-tests.sh -k swagger          # only swagger tests
#   ./run-selenium-tests.sh -k grafana          # only grafana tests
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTS_DIR="$(dirname "$SCRIPT_DIR")"
SELENIUM_DIR="$TESTS_DIR/selenium"

echo "=========================================="
echo "  Selenium Browser Tests"
echo "=========================================="

# Create virtual environment if not present
if [ ! -d "$TESTS_DIR/.venv" ]; then
    echo "→ Creating virtual environment..."
    python3 -m venv "$TESTS_DIR/.venv"
fi

source "$TESTS_DIR/.venv/bin/activate"
pip install -q -r "$TESTS_DIR/integration/requirements.txt"

# Create reports directory
mkdir -p "$SELENIUM_DIR/reports"

# Run selenium tests via pytest
cd "$SELENIUM_DIR"
python -m pytest \
    -v \
    --tb=short \
    --html=reports/selenium-report.html \
    --self-contained-html \
    "$@"

deactivate 2>/dev/null || true
echo "✅ Selenium tests completed."
