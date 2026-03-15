using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Notification Service.
/// Maps domain entities to PostgreSQL tables.
/// </summary>
public sealed class NotificationDbContext : DbContext
{
    /// <summary>
    /// Initialises a new instance of <see cref="NotificationDbContext"/>.
    /// </summary>
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    /// <summary>Notifications table.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(n => n.EventId)
                .HasColumnName("event_id")
                .IsRequired();

            entity.Property(n => n.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(255);

            entity.Property(n => n.Channel)
                .HasColumnName("channel")
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString().ToUpperInvariant(),
                    v => Enum.Parse<ChannelType>(v, true))
                .IsRequired();

            entity.Property(n => n.Recipient)
                .HasColumnName("recipient")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(n => n.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString().ToUpperInvariant(),
                    v => Enum.Parse<NotificationStatus>(v, true))
                .IsRequired();

            entity.Property(n => n.Message)
                .HasColumnName("message");

            entity.Property(n => n.SentAt)
                .HasColumnName("sent_at");

            entity.Property(n => n.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indexes matching init.sql
            entity.HasIndex(n => n.EventId).HasDatabaseName("idx_notifications_event_id");
            entity.HasIndex(n => n.Status).HasDatabaseName("idx_notifications_status");
            entity.HasIndex(n => n.Channel).HasDatabaseName("idx_notifications_channel");
            entity.HasIndex(n => n.UserId).HasDatabaseName("idx_notifications_user_id");
        });
    }
}
