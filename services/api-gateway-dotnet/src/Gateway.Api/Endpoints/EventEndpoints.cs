using FluentValidation;
using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Endpoints;

/// <summary>
/// Defines event-related API endpoints.
/// Follows Single Responsibility — this class only handles event routing.
/// </summary>
public static class EventEndpoints
{
    /// <summary>
    /// Maps all event endpoints to the application route table.
    /// </summary>
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi();

        group.MapPost("/", SubmitEvent)
            .WithName("SubmitEvent")
            .WithSummary("Submit a new event for processing")
            .WithDescription("Receives an event and forwards it to the Event Service for persistence and publishing.")
            .Produces<EventResponse>(StatusCodes.Status202Accepted)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status502BadGateway);

        group.MapGet("/", GetEvents)
            .WithName("GetEvents")
            .WithSummary("Retrieve all processed events")
            .WithDescription("Returns a list of all events from the Event Service.")
            .Produces<IEnumerable<EventResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status502BadGateway);

        group.MapGet("/{id}", GetEventById)
            .WithName("GetEventById")
            .WithSummary("Retrieve a single event by ID")
            .WithDescription("Returns a specific event from the Event Service.")
            .Produces<EventResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status502BadGateway);
    }

    /// <summary>
    /// POST /events — Submit a new event.
    /// </summary>
    private static async Task<IResult> SubmitEvent(
        [FromBody] EventRequest request,
        IEventService eventService,
        IValidator<EventRequest> validator,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            logger.LogWarning("Event validation failed: {@Errors}", errors);

            return Results.BadRequest(new ErrorResponse(
                Title: "Validation Failed",
                Status: StatusCodes.Status400BadRequest,
                Detail: "One or more validation errors occurred.",
                Errors: errors));
        }

        logger.LogInformation(
            "Submitting event {EventType} for user {UserId}",
            request.EventType,
            request.UserId);

        var response = await eventService.SendEventAsync(request, cancellationToken);

        logger.LogInformation(
            "Event {EventType} submitted successfully. EventId: {EventId}",
            request.EventType,
            response.Id);

        return Results.Accepted($"/api/events/{response.Id}", response);
    }

    /// <summary>
    /// GET /events — Retrieve all events.
    /// </summary>
    private static async Task<IResult> GetEvents(
        IEventService eventService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all events");

        var events = await eventService.GetEventsAsync(cancellationToken);

        return Results.Ok(events);
    }

    /// <summary>
    /// GET /events/{id} — Retrieve a single event.
    /// </summary>
    private static async Task<IResult> GetEventById(
        string id,
        IEventService eventService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving event {EventId}", id);

        var eventResponse = await eventService.GetEventByIdAsync(id, cancellationToken);

        if (eventResponse is null)
        {
            return Results.NotFound(new ErrorResponse(
                Title: "Not Found",
                Status: StatusCodes.Status404NotFound,
                Detail: $"Event with ID '{id}' was not found."));
        }

        return Results.Ok(eventResponse);
    }
}
