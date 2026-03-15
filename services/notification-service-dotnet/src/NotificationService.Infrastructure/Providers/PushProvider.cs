using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Simulated push notification provider.
/// In production, this would integrate with Firebase Cloud Messaging, APNs, etc.
/// </summary>
public sealed class PushProvider : INotificationSender
{
    private readonly ILogger<PushProvider> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="PushProvider"/>.
    /// </summary>
    public PushProvider(ILogger<PushProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ChannelType Channel => ChannelType.Push;

    /// <inheritdoc />
    public Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "🔔 [PUSH] Sending to {Recipient}: {Message}",
            notification.Recipient,
            notification.Message);

        // Simulate successful delivery
        return Task.FromResult(true);
    }
}
