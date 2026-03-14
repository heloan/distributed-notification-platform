using Gateway.Application.Interfaces;
using Gateway.Infrastructure.Configuration;
using Gateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Gateway.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services into the DI container.
/// Follows Single Responsibility — this class only handles infrastructure wiring.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services (HTTP clients with resilience policies).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings
        var settings = configuration
            .GetSection(EventServiceSettings.SectionName)
            .Get<EventServiceSettings>() ?? new EventServiceSettings();

        services.Configure<EventServiceSettings>(
            configuration.GetSection(EventServiceSettings.SectionName));

        // Register typed HttpClient with Polly resilience policies
        services.AddHttpClient<IEventService, EventServiceClient>(client =>
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy(settings.RetryCount))
            .AddPolicyHandler(GetCircuitBreakerPolicy(settings.CircuitBreakerDurationSeconds));

        return services;
    }

    /// <summary>
    /// Retry policy with exponential backoff for transient HTTP failures.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    // Logging is handled by Polly's built-in logging middleware
                });
    }

    /// <summary>
    /// Circuit breaker policy to prevent cascading failures.
    /// Opens after 5 consecutive failures, stays open for the configured duration.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int durationSeconds)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(durationSeconds));
    }
}
