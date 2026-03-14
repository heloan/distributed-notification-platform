using NotificationService.Infrastructure;
using NotificationService.Worker;
using Serilog;

// =============================================================================
// Distributed Smart Notification Platform — Notification Service
// =============================================================================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting DSNP Notification Service");

    var builder = Host.CreateDefaultBuilder(args);

    // ---------------------------------------------------------------------------
    // Configure Serilog
    // ---------------------------------------------------------------------------
    builder.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "NotificationService")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}"));

    // ---------------------------------------------------------------------------
    // Configure Services
    // ---------------------------------------------------------------------------
    builder.ConfigureServices((context, services) =>
    {
        // Infrastructure layer (Kafka, EF Core, Providers, Rules)
        services.AddInfrastructure(context.Configuration);

        // Application layer (Use Cases)
        services.AddApplicationServices();

        // Hosted services
        services.AddHostedService<EventConsumerWorker>();

        // Health check endpoint (minimal Kestrel for /health and /metrics)
        services.AddHealthChecks();
    });

    // ---------------------------------------------------------------------------
    // Add minimal web host for health & metrics endpoints
    // ---------------------------------------------------------------------------
    builder.ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapGet("/metrics", () => Results.Ok(new { status = "healthy" }));
            });
        });

        webBuilder.UseUrls("http://+:8080");
    });

    var host = builder.Build();

    Log.Information("DSNP Notification Service is running");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DSNP Notification Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
