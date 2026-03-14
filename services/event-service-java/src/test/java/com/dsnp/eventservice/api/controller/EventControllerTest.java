package com.dsnp.eventservice.api.controller;

import java.time.Instant;
import java.util.List;
import java.util.UUID;

import static org.hamcrest.Matchers.containsString;
import static org.hamcrest.Matchers.hasItem;
import static org.hamcrest.Matchers.hasSize;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.usecase.CreateEventUseCase;
import com.dsnp.eventservice.application.usecase.GetAllEventsUseCase;
import com.dsnp.eventservice.application.usecase.GetEventByIdUseCase;
import com.dsnp.eventservice.domain.exception.EventNotFoundException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

/**
 * Web layer tests for {@link EventController} using MockMvc.
 */
@WebMvcTest(EventController.class)
@DisplayName("EventController")
class EventControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private CreateEventUseCase createEventUseCase;

    @MockBean
    private GetEventByIdUseCase getEventByIdUseCase;

    @MockBean
    private GetAllEventsUseCase getAllEventsUseCase;

    private ObjectMapper objectMapper;

    @BeforeEach
    void setUp() {
        objectMapper = new ObjectMapper().registerModule(new JavaTimeModule());
    }

    // -----------------------------------------------------------------------
    // POST /api/events
    // -----------------------------------------------------------------------
    @Nested
    @DisplayName("POST /api/events")
    class CreateEvent {

        @Test
        @DisplayName("should return 201 for valid request")
        void shouldReturn201ForValidRequest() throws Exception {
            var response = new EventResponse(UUID.randomUUID(), "USER_REGISTERED", "u1", "u@e.com", Instant.now(), "CREATED");
            when(createEventUseCase.execute(any())).thenReturn(response);

            mockMvc.perform(post("/api/events")
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {"eventType":"USER_REGISTERED","userId":"u1","email":"u@e.com"}
                                    """))
                    .andExpect(status().isCreated())
                    .andExpect(jsonPath("$.id").exists())
                    .andExpect(jsonPath("$.eventType").value("USER_REGISTERED"))
                    .andExpect(jsonPath("$.status").value("CREATED"));
        }

        @Test
        @DisplayName("should return 400 when eventType is empty")
        void shouldReturn400WhenEventTypeEmpty() throws Exception {
            mockMvc.perform(post("/api/events")
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {"eventType":"","userId":"u1","email":"u@e.com"}
                                    """))
                    .andExpect(status().isBadRequest())
                    .andExpect(jsonPath("$.details", hasItem(containsString("eventType"))));
        }

        @Test
        @DisplayName("should return 400 when userId is missing")
        void shouldReturn400WhenUserIdMissing() throws Exception {
            mockMvc.perform(post("/api/events")
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {"eventType":"USER_REGISTERED","email":"u@e.com"}
                                    """))
                    .andExpect(status().isBadRequest())
                    .andExpect(jsonPath("$.details", hasItem(containsString("userId"))));
        }

        @Test
        @DisplayName("should return 400 when email is invalid")
        void shouldReturn400WhenEmailInvalid() throws Exception {
            mockMvc.perform(post("/api/events")
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {"eventType":"USER_REGISTERED","userId":"u1","email":"not-an-email"}
                                    """))
                    .andExpect(status().isBadRequest())
                    .andExpect(jsonPath("$.details", hasItem(containsString("email"))));
        }

        @Test
        @DisplayName("should return 400 for unsupported event type")
        void shouldReturn400ForUnsupportedEventType() throws Exception {
            when(createEventUseCase.execute(any())).thenThrow(new IllegalArgumentException("Unsupported event type: 'INVALID'"));

            mockMvc.perform(post("/api/events")
                            .contentType(MediaType.APPLICATION_JSON)
                            .content("""
                                    {"eventType":"INVALID","userId":"u1","email":"u@e.com"}
                                    """))
                    .andExpect(status().isBadRequest())
                    .andExpect(jsonPath("$.message", containsString("Unsupported event type")));
        }
    }

    // -----------------------------------------------------------------------
    // GET /api/events/{id}
    // -----------------------------------------------------------------------
    @Nested
    @DisplayName("GET /api/events/{id}")
    class GetEventById {

        @Test
        @DisplayName("should return 200 when event exists")
        void shouldReturn200WhenExists() throws Exception {
            UUID id = UUID.randomUUID();
            var response = new EventResponse(id, "ORDER_SHIPPED", "u1", "u@e.com", Instant.now(), "CREATED");
            when(getEventByIdUseCase.execute(id)).thenReturn(response);

            mockMvc.perform(get("/api/events/{id}", id))
                    .andExpect(status().isOk())
                    .andExpect(jsonPath("$.id").value(id.toString()))
                    .andExpect(jsonPath("$.eventType").value("ORDER_SHIPPED"));
        }

        @Test
        @DisplayName("should return 404 when event does not exist")
        void shouldReturn404WhenNotExists() throws Exception {
            UUID id = UUID.randomUUID();
            when(getEventByIdUseCase.execute(id)).thenThrow(new EventNotFoundException(id.toString()));

            mockMvc.perform(get("/api/events/{id}", id))
                    .andExpect(status().isNotFound())
                    .andExpect(jsonPath("$.error").value("Not Found"));
        }
    }

    // -----------------------------------------------------------------------
    // GET /api/events
    // -----------------------------------------------------------------------
    @Nested
    @DisplayName("GET /api/events")
    class GetAllEvents {

        @Test
        @DisplayName("should return 200 with event list")
        void shouldReturn200WithList() throws Exception {
            var e1 = new EventResponse(UUID.randomUUID(), "USER_REGISTERED", "u1", "u1@e.com", Instant.now(), "CREATED");
            var e2 = new EventResponse(UUID.randomUUID(), "PAYMENT_FAILED", "u2", "u2@e.com", Instant.now(), "CREATED");
            when(getAllEventsUseCase.execute()).thenReturn(List.of(e1, e2));

            mockMvc.perform(get("/api/events"))
                    .andExpect(status().isOk())
                    .andExpect(jsonPath("$", hasSize(2)))
                    .andExpect(jsonPath("$[0].eventType").value("USER_REGISTERED"))
                    .andExpect(jsonPath("$[1].eventType").value("PAYMENT_FAILED"));
        }

        @Test
        @DisplayName("should return 200 with empty list")
        void shouldReturn200WithEmptyList() throws Exception {
            when(getAllEventsUseCase.execute()).thenReturn(List.of());

            mockMvc.perform(get("/api/events"))
                    .andExpect(status().isOk())
                    .andExpect(jsonPath("$", hasSize(0)));
        }
    }
}
