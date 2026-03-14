using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

/// <summary>
/// Persistence port for <see cref="Notification"/> aggregate.
/// Infrastructure layer provides the concrete implementation.
/// </summary>
public interface INotificationRepository
{
    /// <summary>Persists a new notification.</summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing notification.</summary>
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a notification by its unique identifier.</summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all notifications for a given event.</summary>
    Task<IReadOnlyList<Notification>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all notifications, most recent first.</summary>
    Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
}
