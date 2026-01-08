using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AIQuotaTemplate entity.
/// </summary>
public class AIQuotaTemplateConfiguration : IEntityTypeConfiguration<AIQuotaTemplate>
{
    public void Configure(EntityTypeBuilder<AIQuotaTemplate> builder)
    {
        builder.ToTable("AIQuotaTemplates");

        builder.HasKey(t => t.Id);

        // Unique constraint on TierName (when not deleted)
        builder.HasIndex(t => t.TierName)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL")
            .HasDatabaseName("IX_AIQuotaTemplates_TierName");

        // Index for active templates
        builder.HasIndex(t => new { t.IsActive, t.DisplayOrder })
            .HasFilter("[IsActive] = 1 AND [DeletedAt] IS NULL")
            .HasDatabaseName("IX_AIQuotaTemplates_Active_DisplayOrder");

        // Required fields
        builder.Property(t => t.TierName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.IsSystemTemplate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.AllowOverage)
            .IsRequired()
            .HasDefaultValue(false);

        // Decimal precision
        builder.Property(t => t.MaxCostPerPeriod)
            .HasPrecision(10, 2);

        builder.Property(t => t.OverageRate)
            .HasPrecision(10, 6);

        builder.Property(t => t.DefaultAlertThresholdPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(80m);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(t => t.Quotas)
            .WithOne(q => q.Template)
            .HasForeignKey(q => q.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

