using Gateway.Api.Endpoints;
using Gateway.Api.Extensions;
using Gateway.Api.Middleware;
using Gateway.Infrastructure;
using Serilog;

// =============================================================================
// Distributed Smart Notification Platform — API Gateway
// =============================================================================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting DSNP API Gateway");

    var builder = WebApplication.CreateBuilder(args);

    // ---------------------------------------------------------------------------
    // Configure Serilog
    // ---------------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "ApiGateway")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ---------------------------------------------------------------------------
    // Register Services (Dependency Injection)
    // ---------------------------------------------------------------------------
    builder.Services.AddApplicationServices();                             // Application layer
    builder.Services.AddInfrastructure(builder.Configuration);             // Infrastructure layer
    builder.Services.AddSwaggerDocumentation();                            // Swagger/OpenAPI
    builder.Services.AddObservability();                                   // OpenTelemetry
    builder.Services.AddHttpClient();                                      // Generic HttpClient for health checks
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    // ---------------------------------------------------------------------------
    // Build Application
    // ---------------------------------------------------------------------------
    var app = builder.Build();

    // ---------------------------------------------------------------------------
    // Configure Middleware Pipeline
    // ---------------------------------------------------------------------------
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseCors();
    app.UseSerilogRequestLogging();

    // Swagger (Development only)
    app.UseSwaggerDocumentation();

    // Prometheus metrics endpoint
    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    // ---------------------------------------------------------------------------
    // Map Endpoints
    // ---------------------------------------------------------------------------
    app.MapEventEndpoints();
    app.MapHealthEndpoints();

    // ---------------------------------------------------------------------------
    // Run
    // ---------------------------------------------------------------------------
    Log.Information("DSNP API Gateway is running on {Urls}", string.Join(", ", app.Urls));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DSNP API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// =============================================================================
// Make Program class accessible for integration tests (WebApplicationFactory)
// =============================================================================
namespace Gateway.Api
{
    public partial class Program { }
}
