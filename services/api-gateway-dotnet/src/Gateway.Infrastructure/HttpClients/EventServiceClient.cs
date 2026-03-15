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

        var response = await _httpClient.PostAsJsonAsync("/api/events", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Event Service returned {StatusCode} for event {EventType}. Response: {ErrorBody}",
                (int)response.StatusCode,
                request.EventType,
                errorBody);

            throw new HttpRequestException(
                $"Event Service returned {(int)response.StatusCode}: {errorBody}",
                inner: null,
                statusCode: response.StatusCode);
        }

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

        var response = await _httpClient.GetAsync("/api/events", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Event Service returned {StatusCode} when retrieving events. Response: {ErrorBody}",
                (int)response.StatusCode,
                errorBody);

            throw new HttpRequestException(
                $"Event Service returned {(int)response.StatusCode}: {errorBody}",
                inner: null,
                statusCode: response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<EventResponse>>(cancellationToken);

        return result ?? Enumerable.Empty<EventResponse>();
    }

    /// <inheritdoc />
    public async Task<EventResponse?> GetEventByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving event {EventId} from Event Service", id);

        var response = await _httpClient.GetAsync($"/api/events/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Event {EventId} not found in Event Service", id);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Event Service returned {StatusCode} for event {EventId}. Response: {ErrorBody}",
                (int)response.StatusCode,
                id,
                errorBody);

            throw new HttpRequestException(
                $"Event Service returned {(int)response.StatusCode}: {errorBody}",
                inner: null,
                statusCode: response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<EventResponse>(cancellationToken);
    }
}
