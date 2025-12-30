using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the FeatureFlag entity.
/// Configures the table structure, constraints, and indexes for feature flags.
/// </summary>
public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        // Table name
        builder.ToTable("FeatureFlags");

        // Primary key
        builder.HasKey(f => f.Id);

        // Name: Required, MaxLength(100)
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        // IsEnabled: Required, default false
        builder.Property(f => f.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        // OrganizationId: Optional (nullable for global flags)
        // Note: No foreign key configured as Organization.Id is int but FeatureFlag.OrganizationId is int?
        // This allows for flexibility in feature flag scoping
        builder.Property(f => f.OrganizationId)
            .IsRequired(false);

        // Description: Optional, MaxLength(500)
        builder.Property(f => f.Description)
            .IsRequired(false)
            .HasMaxLength(500);

        // CreatedAt: Required, default value GETUTCDATE()
        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt: Optional
        builder.Property(f => f.UpdatedAt)
            .IsRequired(false);

        // Unique index on Name alone for quick lookups
        builder.HasIndex(f => f.Name)
            .HasDatabaseName("IX_FeatureFlags_Name");

        // Composite unique index on (Name, OrganizationId) to prevent duplicates
        // This allows:
        // - Multiple global flags (OrganizationId = null) with different names
        // - Multiple org-specific flags with same name for different orgs
        // - But prevents duplicate flags for the same org
        builder.HasIndex(f => new { f.Name, f.OrganizationId })
            .IsUnique()
            .HasDatabaseName("IX_FeatureFlags_Name_OrganizationId");

        // Index on OrganizationId for query performance
        builder.HasIndex(f => f.OrganizationId)
            .HasDatabaseName("IX_FeatureFlags_OrganizationId")
            .HasFilter("[OrganizationId] IS NOT NULL");

        // Index on IsEnabled for query performance
        builder.HasIndex(f => f.IsEnabled)
            .HasDatabaseName("IX_FeatureFlags_IsEnabled");

        // Composite index for common query: enabled flags by organization
        builder.HasIndex(f => new { f.OrganizationId, f.IsEnabled })
            .HasDatabaseName("IX_FeatureFlags_OrganizationId_IsEnabled");
    }
}

