namespace NotificationService.Infrastructure.Configuration;

/// <summary>
/// Configuration POCO for database connection settings.
/// Bound from <c>ConnectionStrings:DefaultConnection</c> in appsettings.
/// </summary>
public sealed class DatabaseSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "ConnectionStrings";

    /// <summary>PostgreSQL connection string.</summary>
    public string DefaultConnection { get; set; } = string.Empty;
}
