namespace Gateway.Application.DTOs;

/// <summary>
/// Represents the response returned after an event is successfully submitted.
/// </summary>
public sealed record EventResponse(
    string Id,
    string EventType,
    string UserId,
    string Email,
    DateTime Timestamp,
    string Status
);
