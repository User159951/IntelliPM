using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AIQuota entity.
/// Configures the table structure, unique constraints, indexes, and relationships for AI quota tracking.
/// </summary>
public class AIQuotaConfiguration : IEntityTypeConfiguration<AIQuota>
{
    public void Configure(EntityTypeBuilder<AIQuota> builder)
    {
        builder.ToTable("AIQuotas");

        builder.HasKey(q => q.Id);

        // Unique constraint: one active quota per organization
        builder.HasIndex(q => new { q.OrganizationId, q.IsActive })
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_AIQuotas_Organization_Active");

        // Indexes for queries
        builder.HasIndex(q => q.OrganizationId)
            .HasDatabaseName("IX_AIQuotas_OrganizationId");

        builder.HasIndex(q => q.TierName)
            .HasDatabaseName("IX_AIQuotas_TierName");

        builder.HasIndex(q => q.PeriodEndDate)
            .HasDatabaseName("IX_AIQuotas_PeriodEndDate");

        builder.HasIndex(q => q.IsQuotaExceeded)
            .HasDatabaseName("IX_AIQuotas_IsQuotaExceeded");

        // Index for expiring quotas
        builder.HasIndex(q => new { q.IsActive, q.PeriodEndDate })
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_AIQuotas_Active_PeriodEndDate");

        // Required fields
        builder.Property(q => q.OrganizationId)
            .IsRequired();

        builder.Property(q => q.TierName)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Free");

        builder.Property(q => q.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(q => q.EnforceQuota)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(q => q.UsageByAgentJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(q => q.UsageByDecisionTypeJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        // Decimal precision
        builder.Property(q => q.MaxCostPerPeriod)
            .HasPrecision(10, 2);

        builder.Property(q => q.CostAccumulated)
            .HasPrecision(10, 2);

        builder.Property(q => q.AlertThresholdPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(80m);

        builder.Property(q => q.OverageRate)
            .HasPrecision(10, 6);

        builder.Property(q => q.OverageCost)
            .HasPrecision(10, 2);

        // Relationships
        builder.HasOne(q => q.Organization)
            .WithMany()
            .HasForeignKey(q => q.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

