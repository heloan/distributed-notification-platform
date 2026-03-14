namespace Gateway.Application.DTOs;

/// <summary>
/// Standard API error response.
/// </summary>
public sealed record ErrorResponse(
    string Title,
    int Status,
    string Detail,
    IDictionary<string, string[]>? Errors = null
);
