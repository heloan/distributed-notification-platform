package com.dsnp.eventservice.application.usecase;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.mapper.EventMapper;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Use case: Retrieve all events ordered by creation date.
 */
@Service
@Transactional(readOnly = true)
public class GetAllEventsUseCase {

    private static final Logger log = LoggerFactory.getLogger(GetAllEventsUseCase.class);

    private final EventRepository eventRepository;

    public GetAllEventsUseCase(EventRepository eventRepository) {
        this.eventRepository = eventRepository;
    }

    /**
     * Execute the use case.
     *
     * @return list of all event responses, newest first
     */
    public List<EventResponse> execute() {
        log.debug("Retrieving all events");

        return eventRepository.findAllOrderByCreatedAtDesc()
                .stream()
                .map(EventMapper::toResponse)
                .toList();
    }
}
