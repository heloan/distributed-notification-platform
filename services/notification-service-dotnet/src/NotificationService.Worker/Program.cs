using NotificationService.Application.UseCases;
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

    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080);
    });

    // ---------------------------------------------------------------------------
    // Configure Serilog
    // ---------------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) =>
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
    // Infrastructure layer (Kafka, EF Core, Providers, Rules)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Application layer (Use Cases)
    builder.Services.AddApplicationServices();

    // Hosted services (Kafka consumer)
    builder.Services.AddHostedService<EventConsumerWorker>();

    // Health checks
    builder.Services.AddHealthChecks();

    // ---------------------------------------------------------------------------
    // Build & Configure Pipeline
    // ---------------------------------------------------------------------------
    var app = builder.Build();

    Log.Information("DSNP Notification Service built, mapping endpoints...");

    app.MapHealthChecks("/health");
    app.MapGet("/metrics", () => Results.Ok(new { status = "healthy" }));

    // --- Notification query API ---
    app.MapGet("/api/notifications", async (HttpContext context, GetNotificationsUseCase useCase) =>
    {
        var userId = context.Request.Query["userId"].FirstOrDefault();

        var notifications = string.IsNullOrWhiteSpace(userId)
            ? await useCase.GetAllAsync(context.RequestAborted)
            : await useCase.GetByUserIdAsync(userId, context.RequestAborted);

        return Results.Ok(notifications);
    });

    Log.Information("DSNP Notification Service is running on http://+:8080");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DSNP Notification Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
