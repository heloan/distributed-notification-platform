namespace Gateway.Api.Endpoints;

/// <summary>
/// Defines health check and diagnostics endpoints.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps health check endpoints to the application route table.
    /// </summary>
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", GetHealth)
            .WithName("HealthCheck")
            .WithTags("Health")
            .WithSummary("Check gateway health status")
            .WithOpenApi()
            .Produces<HealthResponse>(StatusCodes.Status200OK);

        app.MapGet("/health/ready", GetReadiness)
            .WithName("ReadinessCheck")
            .WithTags("Health")
            .WithSummary("Check gateway readiness (includes dependency checks)")
            .WithOpenApi()
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/health/live", GetLiveness)
            .WithName("LivenessCheck")
            .WithTags("Health")
            .WithSummary("Liveness probe — always returns 200 if the process is running")
            .WithOpenApi()
            .Produces<HealthResponse>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// GET /health — Basic liveness check.
    /// </summary>
    private static IResult GetHealth()
    {
        return Results.Ok(new HealthResponse(
            Status: "Healthy",
            Service: "API Gateway",
            Timestamp: DateTime.UtcNow));
    }

    /// <summary>
    /// GET /health/live — Liveness probe.
    /// </summary>
    private static IResult GetLiveness()
    {
        return Results.Ok(new HealthResponse(
            Status: "Healthy",
            Service: "API Gateway",
            Timestamp: DateTime.UtcNow));
    }

    /// <summary>
    /// GET /health/ready — Readiness check including downstream dependencies.
    /// </summary>
    private static async Task<IResult> GetReadiness(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<Program> logger)
    {
        var baseUrl = configuration["EventService:BaseUrl"] ?? "http://event-service:8080";

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync($"{baseUrl}/actuator/health");

            if (response.IsSuccessStatusCode)
            {
                return Results.Ok(new HealthResponse(
                    Status: "Ready",
                    Service: "API Gateway",
                    Timestamp: DateTime.UtcNow));
            }

            logger.LogWarning("Event Service health check returned {StatusCode}", response.StatusCode);

            return Results.Json(
                new HealthResponse(
                    Status: "Degraded",
                    Service: "API Gateway",
                    Timestamp: DateTime.UtcNow),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event Service is unreachable");

            return Results.Json(
                new HealthResponse(
                    Status: "Unhealthy",
                    Service: "API Gateway",
                    Timestamp: DateTime.UtcNow),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}

/// <summary>
/// Health check response model.
/// </summary>
public sealed record HealthResponse(
    string Status,
    string Service,
    DateTime Timestamp
);
