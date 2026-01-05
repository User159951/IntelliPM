using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for UserAIQuotaOverride entity.
/// </summary>
public class UserAIQuotaOverrideConfiguration : IEntityTypeConfiguration<UserAIQuotaOverride>
{
    public void Configure(EntityTypeBuilder<UserAIQuotaOverride> builder)
    {
        builder.ToTable("UserAIQuotaOverrides");

        builder.HasKey(q => q.Id);

        // Unique constraint: one active override per user per period
        builder.HasIndex(q => new { q.UserId, q.PeriodStartDate, q.PeriodEndDate })
            .IsUnique()
            .HasDatabaseName("IX_UserAIQuotaOverrides_User_Period");

        // Indexes for queries
        builder.HasIndex(q => q.OrganizationId)
            .HasDatabaseName("IX_UserAIQuotaOverrides_OrganizationId");

        builder.HasIndex(q => q.UserId)
            .HasDatabaseName("IX_UserAIQuotaOverrides_UserId");

        builder.HasIndex(q => new { q.OrganizationId, q.UserId })
            .HasDatabaseName("IX_UserAIQuotaOverrides_Org_User");

        // Required fields
        builder.Property(q => q.OrganizationId)
            .IsRequired();

        builder.Property(q => q.UserId)
            .IsRequired();

        builder.Property(q => q.PeriodStartDate)
            .IsRequired();

        builder.Property(q => q.PeriodEndDate)
            .IsRequired();

        // Decimal precision
        builder.Property(q => q.MaxCostPerPeriod)
            .HasPrecision(10, 2);

        // Relationships
        builder.HasOne(q => q.Organization)
            .WithMany()
            .HasForeignKey(q => q.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.CreatedByUser)
            .WithMany()
            .HasForeignKey(q => q.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

