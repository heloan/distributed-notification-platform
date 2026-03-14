package com.dsnp.eventservice.application.usecase;

import java.util.UUID;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.mapper.EventMapper;
import com.dsnp.eventservice.domain.exception.EventNotFoundException;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Use case: Retrieve a single event by ID.
 */
@Service
@Transactional(readOnly = true)
public class GetEventByIdUseCase {

    private static final Logger log = LoggerFactory.getLogger(GetEventByIdUseCase.class);

    private final EventRepository eventRepository;

    public GetEventByIdUseCase(EventRepository eventRepository) {
        this.eventRepository = eventRepository;
    }

    /**
     * Execute the use case.
     *
     * @param id the event UUID
     * @return the event response
     * @throws EventNotFoundException if the event does not exist
     */
    public EventResponse execute(UUID id) {
        log.debug("Retrieving event: id={}", id);

        return eventRepository.findById(id)
                .map(EventMapper::toResponse)
                .orElseThrow(() -> new EventNotFoundException(id.toString()));
    }
}
