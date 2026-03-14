namespace Gateway.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the downstream Event Service.
/// Bound from appsettings.json section "EventService".
/// </summary>
public sealed class EventServiceSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "EventService";

    /// <summary>
    /// Base URL of the Event Service (e.g., http://event-service:8080).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for HTTP requests to the Event Service.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Duration of circuit breaker open state in seconds.
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
