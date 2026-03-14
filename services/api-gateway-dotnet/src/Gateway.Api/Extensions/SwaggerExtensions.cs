namespace Gateway.Api.Extensions;

/// <summary>
/// Configures Swagger/OpenAPI documentation.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger services to the DI container.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "DSNP — API Gateway",
                Version = "v1",
                Description = "API Gateway for the Distributed Smart Notification Platform. " +
                              "Routes requests to downstream microservices.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Heloan Marinho",
                    Url = new Uri("https://github.com/heloan/distributed-notification-platform")
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the Swagger middleware pipeline.
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DSNP API Gateway v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "DSNP API Gateway";
            });
        }

        return app;
    }
}
