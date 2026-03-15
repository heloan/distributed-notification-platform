using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Mappers;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.UseCases;

/// <summary>
/// Retrieves notification records — all or filtered by event ID.
/// Read-only use case following Single Responsibility.
/// </summary>
public sealed class GetNotificationsUseCase
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<GetNotificationsUseCase> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="GetNotificationsUseCase"/>.
    /// </summary>
    public GetNotificationsUseCase(
        INotificationRepository repository,
        ILogger<GetNotificationsUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all notifications, most recent first.
    /// </summary>
    public async Task<IReadOnlyList<NotificationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetAllAsync(cancellationToken);
        return notifications.Select(NotificationMapper.ToResponse).ToList();
    }

    /// <summary>
    /// Retrieves all notifications for a specific event.
    /// </summary>
    public async Task<IReadOnlyList<NotificationResponse>> GetByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByEventIdAsync(eventId, cancellationToken);
        return notifications.Select(NotificationMapper.ToResponse).ToList();
    }

    /// <summary>
    /// Retrieves all notifications for a specific user.
    /// </summary>
    public async Task<IReadOnlyList<NotificationResponse>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return notifications.Select(NotificationMapper.ToResponse).ToList();
    }
}
