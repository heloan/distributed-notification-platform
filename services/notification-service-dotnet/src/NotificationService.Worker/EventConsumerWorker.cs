using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;

namespace NotificationService.Worker;

/// <summary>
/// Background service that continuously consumes events from Kafka
/// and delegates processing to <see cref="ProcessEventUseCase"/>.
///
/// Follows Dependency Inversion — depends on <see cref="IEventConsumer"/> port,
/// not on Kafka directly.
/// </summary>
public sealed class EventConsumerWorker : BackgroundService
{
    private readonly IEventConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventConsumerWorker> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="EventConsumerWorker"/>.
    /// </summary>
    public EventConsumerWorker(
        IEventConsumer consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<EventConsumerWorker> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventConsumerWorker started");

        await _consumer.ConsumeAsync(async (message, ct) =>
        {
            _logger.LogInformation(
                "Processing event {EventId} of type {EventType}",
                message.EventId, message.EventType);

            // Create a new scope per message to resolve scoped services (DbContext, UseCase)
            using var scope = _scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ProcessEventUseCase>();

            await useCase.ExecuteAsync(message, ct);

        }, stoppingToken);

        _logger.LogInformation("EventConsumerWorker stopped");
    }
}
