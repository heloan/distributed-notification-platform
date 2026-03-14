using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Apache Kafka implementation of <see cref="IEventConsumer"/>.
/// Consumes messages from the <c>events</c> topic and deserialises them
/// into <see cref="EventMessage"/> DTOs.
///
/// Dependency Inversion — the Application layer defines the port;
/// this class is the infrastructure adapter.
/// </summary>
public sealed class KafkaEventConsumer : IEventConsumer
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaEventConsumer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initialises a new instance of <see cref="KafkaEventConsumer"/>.
    /// </summary>
    public KafkaEventConsumer(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaEventConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ConsumeAsync(
        Func<EventMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
                _logger.LogError("Kafka consumer error: {Error}", error.Reason))
            .Build();

        consumer.Subscribe(_settings.Topic);
        _logger.LogInformation(
            "Kafka consumer subscribed to topic '{Topic}' with group '{GroupId}'",
            _settings.Topic, _settings.GroupId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cancellationToken);
                    if (result?.Message?.Value is null) continue;

                    _logger.LogDebug(
                        "Consumed message from partition {Partition} offset {Offset}",
                        result.Partition.Value, result.Offset.Value);

                    var eventMessage = JsonSerializer.Deserialize<EventMessage>(
                        result.Message.Value, JsonOptions);

                    if (eventMessage is not null)
                    {
                        await handler(eventMessage, cancellationToken);
                    }

                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer shutting down gracefully");
        }
        finally
        {
            consumer.Close();
        }
    }
}
