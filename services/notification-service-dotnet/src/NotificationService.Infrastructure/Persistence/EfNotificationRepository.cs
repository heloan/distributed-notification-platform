using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="INotificationRepository"/>.
/// Adapts the domain port to the relational database.
/// </summary>
public sealed class EfNotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    /// <summary>
    /// Initialises a new instance of <see cref="EfNotificationRepository"/>.
    /// </summary>
    public EfNotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetByEventIdAsync(
        Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.EventId == eventId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
