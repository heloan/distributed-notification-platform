using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Simulated Slack notification provider.
/// In production, this would use the Slack Web API.
/// </summary>
public sealed class SlackProvider : INotificationSender
{
    private readonly ILogger<SlackProvider> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="SlackProvider"/>.
    /// </summary>
    public SlackProvider(ILogger<SlackProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ChannelType Channel => ChannelType.Slack;

    /// <inheritdoc />
    public Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "💬 [SLACK] Sending to {Recipient}: {Message}",
            notification.Recipient,
            notification.Message);

        // Simulate successful delivery
        return Task.FromResult(true);
    }
}
