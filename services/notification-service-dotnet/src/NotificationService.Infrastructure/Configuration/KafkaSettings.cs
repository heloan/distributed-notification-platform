namespace NotificationService.Infrastructure.Configuration;

/// <summary>
/// Configuration POCO for Apache Kafka consumer settings.
/// Bound from the <c>"Kafka"</c> section in appsettings.
/// </summary>
public sealed class KafkaSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Kafka";

    /// <summary>Kafka bootstrap servers (comma-separated).</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Consumer group identifier.</summary>
    public string GroupId { get; set; } = "notification-service";

    /// <summary>Topic to subscribe to.</summary>
    public string Topic { get; set; } = "events";
}
