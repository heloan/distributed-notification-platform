package com.dsnp.eventservice.application.mapper;

import static org.assertj.core.api.Assertions.assertThat;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.enums.EventType;

/**
 * Unit tests for {@link EventMapper}.
 */
@DisplayName("EventMapper")
class EventMapperTest {

    @Test
    @DisplayName("toResponse() should map all fields correctly")
    void toResponse_shouldMapAllFields() {
        Event event = Event.create(EventType.SECURITY_ALERT, "admin", "a@e.com", "{\"alert\":true}");

        EventResponse response = EventMapper.toResponse(event);

        assertThat(response.id()).isEqualTo(event.getId());
        assertThat(response.eventType()).isEqualTo("SECURITY_ALERT");
        assertThat(response.userId()).isEqualTo("admin");
        assertThat(response.email()).isEqualTo("a@e.com");
        assertThat(response.createdAt()).isEqualTo(event.getCreatedAt());
        assertThat(response.status()).isEqualTo("CREATED");
    }
}
