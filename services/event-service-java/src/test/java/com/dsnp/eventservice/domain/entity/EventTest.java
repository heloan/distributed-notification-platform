package com.dsnp.eventservice.domain.entity;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import com.dsnp.eventservice.domain.enums.EventType;

/**
 * Unit tests for the {@link Event} domain entity.
 */
@DisplayName("Event Domain Entity")
class EventTest {

    @Test
    @DisplayName("create() should generate ID and timestamp")
    void create_shouldGenerateIdAndTimestamp() {
        Event event = Event.create(EventType.USER_REGISTERED, "user-1", "u@e.com", "{}");

        assertThat(event.getId()).isNotNull();
        assertThat(event.getCreatedAt()).isNotNull();
        assertThat(event.getEventType()).isEqualTo(EventType.USER_REGISTERED);
        assertThat(event.getUserId()).isEqualTo("user-1");
        assertThat(event.getEmail()).isEqualTo("u@e.com");
    }

    @Test
    @DisplayName("create() should throw when eventType is null")
    void create_shouldThrowWhenEventTypeNull() {
        assertThatThrownBy(() -> Event.create(null, "user-1", "u@e.com", "{}"))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("eventType");
    }

    @Test
    @DisplayName("create() should throw when userId is null")
    void create_shouldThrowWhenUserIdNull() {
        assertThatThrownBy(() -> Event.create(EventType.USER_REGISTERED, null, "u@e.com", "{}"))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("userId");
    }

    @Test
    @DisplayName("create() should throw when email is null")
    void create_shouldThrowWhenEmailNull() {
        assertThatThrownBy(() -> Event.create(EventType.USER_REGISTERED, "user-1", null, "{}"))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("email");
    }

    @Test
    @DisplayName("reconstitute() should restore all fields")
    void reconstitute_shouldRestoreAllFields() {
        Event original = Event.create(EventType.PAYMENT_FAILED, "user-2", "p@e.com", "{\"key\":\"value\"}");

        Event restored = Event.reconstitute(
                original.getId(),
                original.getEventType(),
                original.getUserId(),
                original.getEmail(),
                original.getPayload(),
                original.getCreatedAt()
        );

        assertThat(restored.getId()).isEqualTo(original.getId());
        assertThat(restored.getEventType()).isEqualTo(original.getEventType());
        assertThat(restored.getUserId()).isEqualTo(original.getUserId());
    }

    @Test
    @DisplayName("equals() should compare by ID")
    void equals_shouldCompareById() {
        Event event1 = Event.create(EventType.ORDER_SHIPPED, "u1", "e@e.com", "{}");
        Event event2 = Event.reconstitute(event1.getId(), EventType.ORDER_SHIPPED, "u1", "e@e.com", "{}", event1.getCreatedAt());

        assertThat(event1).isEqualTo(event2);
        assertThat(event1.hashCode()).isEqualTo(event2.hashCode());
    }

    @Test
    @DisplayName("toString() should contain event info")
    void toString_shouldContainEventInfo() {
        Event event = Event.create(EventType.SECURITY_ALERT, "admin", "a@e.com", "{}");

        assertThat(event.toString())
                .contains("SECURITY_ALERT")
                .contains("admin");
    }
}
