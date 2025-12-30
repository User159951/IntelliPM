using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the NotificationPreference entity.
/// Configures the table structure, unique constraints, indexes, and relationships for notification preferences.
/// </summary>
public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(np => np.Id);

        // Unique constraint: one preference per user per notification type
        builder.HasIndex(np => new { np.UserId, np.NotificationType })
            .IsUnique()
            .HasDatabaseName("IX_NotificationPreferences_User_Type");

        // Index for organization queries
        builder.HasIndex(np => np.OrganizationId)
            .HasDatabaseName("IX_NotificationPreferences_OrganizationId");

        // Required fields
        builder.Property(np => np.UserId)
            .IsRequired();

        builder.Property(np => np.OrganizationId)
            .IsRequired();

        builder.Property(np => np.NotificationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(np => np.Frequency)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Instant");

        // Default values
        builder.Property(np => np.EmailEnabled)
            .HasDefaultValue(true);

        builder.Property(np => np.InAppEnabled)
            .HasDefaultValue(true);

        builder.Property(np => np.PushEnabled)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(np => np.User)
            .WithMany()
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete preferences when user deleted
    }
}

