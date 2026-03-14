package com.dsnp.eventservice.domain.enums;

/**
 * Supported event types in the platform.
 * <p>
 * Each type maps to a specific notification channel via the Notification Service.
 * </p>
 */
public enum EventType {

    USER_REGISTERED,
    PAYMENT_FAILED,
    ORDER_SHIPPED,
    SECURITY_ALERT;

    /**
     * Safely parse a string to an EventType.
     *
     * @param value the string representation
     * @return the matching EventType
     * @throws IllegalArgumentException if the value is not a valid event type
     */
    public static EventType fromString(String value) {
        try {
            return EventType.valueOf(value.toUpperCase().trim());
        } catch (IllegalArgumentException | NullPointerException e) {
            throw new IllegalArgumentException(
                    "Unsupported event type: '%s'. Supported types: %s"
                            .formatted(value, java.util.Arrays.toString(values()))
            );
        }
    }
}
