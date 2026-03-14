package com.dsnp.eventservice.application.usecase;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import static org.mockito.ArgumentMatchers.any;
import org.mockito.Mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import org.mockito.junit.jupiter.MockitoExtension;

import com.dsnp.eventservice.application.dto.CreateEventRequest;
import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.port.EventPublisher;
import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.enums.EventType;
import com.dsnp.eventservice.domain.event.EventCreatedDomainEvent;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Unit tests for {@link CreateEventUseCase}.
 */
@ExtendWith(MockitoExtension.class)
@DisplayName("CreateEventUseCase")
class CreateEventUseCaseTest {

    @Mock
    private EventRepository eventRepository;

    @Mock
    private EventPublisher eventPublisher;

    private CreateEventUseCase useCase;

    @BeforeEach
    void setUp() {
        useCase = new CreateEventUseCase(eventRepository, eventPublisher);
    }

    @Test
    @DisplayName("should create event, persist, and publish")
    void execute_shouldCreatePersistAndPublish() {
        // Arrange
        var request = new CreateEventRequest("USER_REGISTERED", "user-1", "u@e.com", null);
        when(eventRepository.save(any(Event.class))).thenAnswer(invocation -> invocation.getArgument(0));

        // Act
        EventResponse response = useCase.execute(request);

        // Assert
        assertThat(response).isNotNull();
        assertThat(response.eventType()).isEqualTo("USER_REGISTERED");
        assertThat(response.userId()).isEqualTo("user-1");
        assertThat(response.status()).isEqualTo("CREATED");
        assertThat(response.id()).isNotNull();

        verify(eventRepository).save(any(Event.class));
        verify(eventPublisher).publish(any(EventCreatedDomainEvent.class));
    }

    @Test
    @DisplayName("should publish the correct domain event")
    void execute_shouldPublishCorrectDomainEvent() {
        // Arrange
        var request = new CreateEventRequest("PAYMENT_FAILED", "user-2", "p@e.com", "2026-03-14T10:00:00");
        when(eventRepository.save(any(Event.class))).thenAnswer(invocation -> invocation.getArgument(0));

        var captor = ArgumentCaptor.forClass(EventCreatedDomainEvent.class);

        // Act
        useCase.execute(request);

        // Assert
        verify(eventPublisher).publish(captor.capture());
        EventCreatedDomainEvent published = captor.getValue();
        assertThat(published.eventType()).isEqualTo(EventType.PAYMENT_FAILED);
        assertThat(published.userId()).isEqualTo("user-2");
        assertThat(published.email()).isEqualTo("p@e.com");
    }

    @Test
    @DisplayName("should throw for unsupported event type")
    void execute_shouldThrowForUnsupportedEventType() {
        var request = new CreateEventRequest("INVALID", "u1", "e@e.com", null);

        assertThatThrownBy(() -> useCase.execute(request))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Unsupported event type");

        verify(eventRepository, never()).save(any());
        verify(eventPublisher, never()).publish(any());
    }

    @Test
    @DisplayName("should not publish if persistence fails")
    void execute_shouldNotPublishIfPersistenceFails() {
        var request = new CreateEventRequest("ORDER_SHIPPED", "u1", "e@e.com", null);
        when(eventRepository.save(any())).thenThrow(new RuntimeException("DB down"));

        assertThatThrownBy(() -> useCase.execute(request))
                .isInstanceOf(RuntimeException.class);

        verify(eventPublisher, never()).publish(any());
    }
}
