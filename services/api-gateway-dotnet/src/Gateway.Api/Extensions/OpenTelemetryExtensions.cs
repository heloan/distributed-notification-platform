using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Gateway.Api.Extensions;

/// <summary>
/// Configures OpenTelemetry for distributed tracing and metrics.
/// </summary>
public static class OpenTelemetryExtensions
{
    private const string ServiceName = "dsnp-api-gateway";

    /// <summary>
    /// Adds OpenTelemetry tracing and metrics to the service.
    /// </summary>
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
