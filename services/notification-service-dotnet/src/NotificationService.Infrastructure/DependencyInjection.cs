using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Providers;
using NotificationService.Infrastructure.RuleEngine;

namespace NotificationService.Infrastructure;

/// <summary>
/// Registers all Infrastructure and Application layer services into the DI container.
/// Single Responsibility — this class is the only place that wires dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services (Kafka, EF Core, providers, rules).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------------------------------------------------------------------------
        // Configuration
        // ---------------------------------------------------------------------------
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));

        // ---------------------------------------------------------------------------
        // Persistence — EF Core + PostgreSQL
        // ---------------------------------------------------------------------------
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<INotificationRepository, EfNotificationRepository>();

        // ---------------------------------------------------------------------------
        // Messaging — Kafka Consumer
        // ---------------------------------------------------------------------------
        services.AddSingleton<IEventConsumer, KafkaEventConsumer>();

        // ---------------------------------------------------------------------------
        // Rule Engine
        // ---------------------------------------------------------------------------
        services.AddSingleton<IRuleEngine, InMemoryRuleEngine>();

        // ---------------------------------------------------------------------------
        // Channel Providers (Open/Closed — add new providers here)
        // ---------------------------------------------------------------------------
        services.AddSingleton<INotificationSender, EmailProvider>();
        services.AddSingleton<INotificationSender, SlackProvider>();
        services.AddSingleton<INotificationSender, SmsProvider>();

        return services;
    }

    /// <summary>
    /// Adds Application layer services (use cases).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ProcessEventUseCase>();
        services.AddScoped<GetNotificationsUseCase>();

        return services;
    }
}
