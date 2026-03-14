package com.dsnp.eventservice.application.usecase;

import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
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
import com.dsnp.eventservice.domain.exception.EventNotFoundException;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Unit tests for {@link GetEventByIdUseCase}.
 */
@ExtendWith(MockitoExtension.class)
@DisplayName("GetEventByIdUseCase")
class GetEventByIdUseCaseTest {

    @Mock
    private EventRepository eventRepository;

    private GetEventByIdUseCase useCase;

    @BeforeEach
    void setUp() {
        useCase = new GetEventByIdUseCase(eventRepository);
    }

    @Test
    @DisplayName("should return event when found")
    void execute_shouldReturnEventWhenFound() {
        Event event = Event.create(EventType.USER_REGISTERED, "user-1", "u@e.com", "{}");
        when(eventRepository.findById(event.getId())).thenReturn(Optional.of(event));

        EventResponse response = useCase.execute(event.getId());

        assertThat(response.id()).isEqualTo(event.getId());
        assertThat(response.eventType()).isEqualTo("USER_REGISTERED");
    }

    @Test
    @DisplayName("should throw EventNotFoundException when not found")
    void execute_shouldThrowWhenNotFound() {
        UUID id = UUID.randomUUID();
        when(eventRepository.findById(id)).thenReturn(Optional.empty());

        assertThatThrownBy(() -> useCase.execute(id))
                .isInstanceOf(EventNotFoundException.class)
                .hasMessageContaining(id.toString());
    }
}
