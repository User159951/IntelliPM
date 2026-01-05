using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for OrganizationAIQuota entity.
/// </summary>
public class OrganizationAIQuotaConfiguration : IEntityTypeConfiguration<OrganizationAIQuota>
{
    public void Configure(EntityTypeBuilder<OrganizationAIQuota> builder)
    {
        builder.ToTable("OrganizationAIQuotas", table =>
        {
            // ResetDayOfMonth validation (1-31)
            table.HasCheckConstraint(
                "CK_OrganizationAIQuotas_ResetDayOfMonth",
                "[ResetDayOfMonth] IS NULL OR ([ResetDayOfMonth] >= 1 AND [ResetDayOfMonth] <= 31)");
        });

        builder.HasKey(oq => oq.Id);

        // Required fields
        builder.Property(oq => oq.OrganizationId)
            .IsRequired();

        builder.Property(oq => oq.MonthlyTokenLimit)
            .IsRequired();

        builder.Property(oq => oq.IsAIEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(oq => oq.CreatedAt)
            .IsRequired();

        // Unique constraint: one quota per organization
        builder.HasIndex(oq => oq.OrganizationId)
            .IsUnique()
            .HasDatabaseName("IX_OrganizationAIQuotas_OrganizationId");

        // Foreign key relationship
        builder.HasOne(oq => oq.Organization)
            .WithMany()
            .HasForeignKey(oq => oq.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

