"""
Shared variables for Robot Framework tests.
Override via command line: --variable GATEWAY_URL:http://custom-host:5000
"""

GATEWAY_URL = "http://localhost:5000"
EVENT_SERVICE_URL = "http://localhost:8080"
NOTIFICATION_URL = "http://localhost:5001"

# Supported event types
VALID_EVENT_TYPES = ["USER_REGISTERED", "PAYMENT_FAILED", "ORDER_SHIPPED", "SECURITY_ALERT"]
INVALID_EVENT_TYPE = "UNKNOWN_EVENT"

# Test user data
TEST_USER_ID = "robot-test-001"
TEST_USER_EMAIL = "robot@test.com"
