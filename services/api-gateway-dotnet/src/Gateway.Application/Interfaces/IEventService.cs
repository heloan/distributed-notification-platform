using Gateway.Application.DTOs;

namespace Gateway.Application.Interfaces;

/// <summary>
/// Defines the contract for communicating with the downstream Event Service.
/// Follows Interface Segregation Principle (ISP) — lean, specific interface.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Sends an event to the Event Service for processing.
    /// </summary>
    /// <param name="request">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created event response.</returns>
    Task<EventResponse> SendEventAsync(EventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all events from the Event Service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of event responses.</returns>
    Task<IEnumerable<EventResponse>> GetEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single event by its identifier.
    /// </summary>
    /// <param name="id">The event identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The event response, or null if not found.</returns>
    Task<EventResponse?> GetEventByIdAsync(string id, CancellationToken cancellationToken = default);
}
