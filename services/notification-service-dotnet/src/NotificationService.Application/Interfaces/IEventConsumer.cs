using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

/// <summary>
/// Event consumer port — abstracts the messaging infrastructure (Kafka).
/// The worker service depends on this interface, not on Kafka directly.
/// Follows Dependency Inversion Principle (DIP).
/// </summary>
public interface IEventConsumer
{
    /// <summary>
    /// Starts consuming events. The <paramref name="handler"/> is invoked for
    /// every event received from the broker.
    /// </summary>
    /// <param name="handler">Callback to process each event.</param>
    /// <param name="cancellationToken">Cancellation token to stop consuming.</param>
    Task ConsumeAsync(
        Func<EventMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
