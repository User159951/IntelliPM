using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for UserAIUsageCounter entity.
/// </summary>
public class UserAIUsageCounterConfiguration : IEntityTypeConfiguration<UserAIUsageCounter>
{
    public void Configure(EntityTypeBuilder<UserAIUsageCounter> builder)
    {
        builder.ToTable("UserAIUsageCounters");

        builder.HasKey(c => c.Id);

        // Unique constraint: one counter per user per period
        builder.HasIndex(c => new { c.UserId, c.PeriodStartDate, c.PeriodEndDate })
            .IsUnique()
            .HasDatabaseName("IX_UserAIUsageCounters_User_Period");

        // Indexes for queries
        builder.HasIndex(c => c.OrganizationId)
            .HasDatabaseName("IX_UserAIUsageCounters_OrganizationId");

        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_UserAIUsageCounters_UserId");

        builder.HasIndex(c => new { c.OrganizationId, c.UserId })
            .HasDatabaseName("IX_UserAIUsageCounters_Org_User");

        // Required fields
        builder.Property(c => c.OrganizationId)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.PeriodStartDate)
            .IsRequired();

        builder.Property(c => c.PeriodEndDate)
            .IsRequired();

        // Decimal precision
        builder.Property(c => c.CostAccumulated)
            .HasPrecision(10, 2);

        // Relationships
        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

