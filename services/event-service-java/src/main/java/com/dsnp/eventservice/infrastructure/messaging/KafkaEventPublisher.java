package com.dsnp.eventservice.infrastructure.messaging;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.stereotype.Component;

import com.dsnp.eventservice.application.port.EventPublisher;
import com.dsnp.eventservice.domain.event.EventCreatedDomainEvent;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

/**
 * Kafka implementation of the {@link EventPublisher} output port.
 * <p>
 * Publishes domain events to the configured Kafka topic.
 * The event ID is used as the message key for partition affinity.
 * </p>
 */
@Component
public class KafkaEventPublisher implements EventPublisher {

    private static final Logger log = LoggerFactory.getLogger(KafkaEventPublisher.class);

    private final KafkaTemplate<String, String> kafkaTemplate;
    private final ObjectMapper objectMapper;
    private final String topicName;

    public KafkaEventPublisher(
            KafkaTemplate<String, String> kafkaTemplate,
            ObjectMapper objectMapper,
            @Value("${app.kafka.topic:events}") String topicName
    ) {
        this.kafkaTemplate = kafkaTemplate;
        this.objectMapper = objectMapper;
        this.topicName = topicName;
    }

    @Override
    public void publish(EventCreatedDomainEvent domainEvent) {
        try {
            String key = domainEvent.eventId().toString();
            String value = objectMapper.writeValueAsString(domainEvent);

            kafkaTemplate.send(topicName, key, value)
                    .whenComplete((result, ex) -> {
                        if (ex != null) {
                            log.error("Failed to publish event to Kafka: id={}, topic={}, error={}",
                                    domainEvent.eventId(), topicName, ex.getMessage());
                        } else {
                            log.info("Event published to Kafka: id={}, topic={}, partition={}, offset={}",
                                    domainEvent.eventId(),
                                    topicName,
                                    result.getRecordMetadata().partition(),
                                    result.getRecordMetadata().offset());
                        }
                    });

        } catch (JsonProcessingException e) {
            log.error("Failed to serialize domain event: id={}", domainEvent.eventId(), e);
            throw new RuntimeException("Failed to serialize event for Kafka", e);
        }
    }
}
