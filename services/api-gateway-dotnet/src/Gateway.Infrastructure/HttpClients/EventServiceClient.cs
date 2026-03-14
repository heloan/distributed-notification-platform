using System.Net;
using System.Net.Http.Json;
using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.HttpClients;

/// <summary>
/// HTTP client implementation for communicating with the downstream Event Service (Java Spring Boot).
/// Follows Dependency Inversion Principle — depends on IEventService abstraction.
/// </summary>
public sealed class EventServiceClient : IEventService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventServiceClient> _logger;

    public EventServiceClient(HttpClient httpClient, ILogger<EventServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EventResponse> SendEventAsync(EventRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Forwarding event {EventType} for user {UserId} to Event Service",
            request.EventType,
            request.UserId);

        var response = await _httpClient.PostAsJsonAsync("/events", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EventResponse>(cancellationToken);

        _logger.LogInformation(
            "Event {EventType} forwarded successfully. EventId: {EventId}",
            request.EventType,
            result?.Id);

        return result ?? throw new InvalidOperationException("Event Service returned an empty response.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventResponse>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all events from Event Service");

        var response = await _httpClient.GetAsync("/events", cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<EventResponse>>(cancellationToken);

        return result ?? Enumerable.Empty<EventResponse>();
    }

    /// <inheritdoc />
    public async Task<EventResponse?> GetEventByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving event {EventId} from Event Service", id);

        var response = await _httpClient.GetAsync($"/events/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Event {EventId} not found in Event Service", id);
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EventResponse>(cancellationToken);
    }
}
