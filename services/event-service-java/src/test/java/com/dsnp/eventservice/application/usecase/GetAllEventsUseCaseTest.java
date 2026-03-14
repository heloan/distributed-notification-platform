package com.dsnp.eventservice.application.usecase;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import static org.mockito.Mockito.when;
import org.mockito.junit.jupiter.MockitoExtension;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.enums.EventType;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Unit tests for {@link GetAllEventsUseCase}.
 */
@ExtendWith(MockitoExtension.class)
@DisplayName("GetAllEventsUseCase")
class GetAllEventsUseCaseTest {

    @Mock
    private EventRepository eventRepository;

    private GetAllEventsUseCase useCase;

    @BeforeEach
    void setUp() {
        useCase = new GetAllEventsUseCase(eventRepository);
    }

    @Test
    @DisplayName("should return all events mapped to DTOs")
    void execute_shouldReturnAllEvents() {
        var e1 = Event.create(EventType.USER_REGISTERED, "u1", "e1@e.com", "{}");
        var e2 = Event.create(EventType.PAYMENT_FAILED, "u2", "e2@e.com", "{}");
        when(eventRepository.findAllOrderByCreatedAtDesc()).thenReturn(List.of(e1, e2));

        List<EventResponse> result = useCase.execute();

        assertThat(result).hasSize(2);
        assertThat(result.get(0).eventType()).isEqualTo("USER_REGISTERED");
        assertThat(result.get(1).eventType()).isEqualTo("PAYMENT_FAILED");
    }

    @Test
    @DisplayName("should return empty list when no events exist")
    void execute_shouldReturnEmptyList() {
        when(eventRepository.findAllOrderByCreatedAtDesc()).thenReturn(List.of());

        List<EventResponse> result = useCase.execute();

        assertThat(result).isEmpty();
    }
}
