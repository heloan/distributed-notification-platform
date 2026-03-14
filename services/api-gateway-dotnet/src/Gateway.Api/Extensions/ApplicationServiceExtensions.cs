using FluentValidation;
using Gateway.Application.DTOs;
using Gateway.Application.Validators;

namespace Gateway.Api.Extensions;

/// <summary>
/// Registers Application layer services into the DI container.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adds Application layer services (validators, etc.).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register FluentValidation validators
        services.AddScoped<IValidator<EventRequest>, EventRequestValidator>();

        return services;
    }
}
