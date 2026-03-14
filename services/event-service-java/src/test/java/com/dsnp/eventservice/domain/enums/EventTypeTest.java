package com.dsnp.eventservice.domain.enums;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

/**
 * Unit tests for the {@link EventType} enum.
 */
@DisplayName("EventType Enum")
class EventTypeTest {

    @ParameterizedTest
    @ValueSource(strings = {"USER_REGISTERED", "PAYMENT_FAILED", "ORDER_SHIPPED", "SECURITY_ALERT"})
    @DisplayName("fromString() should parse valid event types")
    void fromString_shouldParseValidTypes(String type) {
        EventType result = EventType.fromString(type);
        assertThat(result.name()).isEqualTo(type);
    }

    @Test
    @DisplayName("fromString() should handle lowercase input")
    void fromString_shouldHandleLowercase() {
        assertThat(EventType.fromString("user_registered")).isEqualTo(EventType.USER_REGISTERED);
    }

    @Test
    @DisplayName("fromString() should handle mixed case with spaces")
    void fromString_shouldHandleMixedCaseWithSpaces() {
        assertThat(EventType.fromString("  Payment_Failed  ")).isEqualTo(EventType.PAYMENT_FAILED);
    }

    @Test
    @DisplayName("fromString() should throw for unsupported type")
    void fromString_shouldThrowForUnsupportedType() {
        assertThatThrownBy(() -> EventType.fromString("INVALID_TYPE"))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Unsupported event type")
                .hasMessageContaining("INVALID_TYPE");
    }

    @Test
    @DisplayName("fromString() should throw for null input")
    void fromString_shouldThrowForNull() {
        assertThatThrownBy(() -> EventType.fromString(null))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Unsupported event type");
    }

    @Test
    @DisplayName("fromString() should throw for empty string")
    void fromString_shouldThrowForEmpty() {
        assertThatThrownBy(() -> EventType.fromString(""))
                .isInstanceOf(IllegalArgumentException.class);
    }
}
