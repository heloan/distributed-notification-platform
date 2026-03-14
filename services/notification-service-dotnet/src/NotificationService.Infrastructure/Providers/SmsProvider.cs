using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Simulated SMS notification provider.
/// In production, this would integrate with Twilio, AWS SNS, etc.
/// </summary>
public sealed class SmsProvider : INotificationSender
{
    private readonly ILogger<SmsProvider> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="SmsProvider"/>.
    /// </summary>
    public SmsProvider(ILogger<SmsProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ChannelType Channel => ChannelType.Sms;

    /// <inheritdoc />
    public Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📱 [SMS] Sending to {Recipient}: {Message}",
            notification.Recipient,
            notification.Message);

        // Simulate successful delivery
        return Task.FromResult(true);
    }
}
