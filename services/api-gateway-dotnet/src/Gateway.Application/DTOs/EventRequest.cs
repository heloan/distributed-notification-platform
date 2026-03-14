namespace Gateway.Application.DTOs;

/// <summary>
/// Represents an incoming event request from a client.
/// </summary>
public sealed record EventRequest(
    string EventType,
    string UserId,
    string Email,
    DateTime? Timestamp = null
);
