using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for UserAIQuota entity.
/// </summary>
public class UserAIQuotaConfiguration : IEntityTypeConfiguration<UserAIQuota>
{
    public void Configure(EntityTypeBuilder<UserAIQuota> builder)
    {
        builder.ToTable("UserAIQuotas");

        builder.HasKey(uq => uq.Id);

        // Required fields
        builder.Property(uq => uq.UserId)
            .IsRequired();

        builder.Property(uq => uq.OrganizationId)
            .IsRequired();

        builder.Property(uq => uq.CreatedAt)
            .IsRequired();

        // Unique constraint: one quota override per user
        builder.HasIndex(uq => uq.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserAIQuotas_UserId");

        // Index on OrganizationId for fast filtering
        builder.HasIndex(uq => uq.OrganizationId)
            .HasDatabaseName("IX_UserAIQuotas_OrganizationId");

        // Foreign key relationships
        builder.HasOne(uq => uq.User)
            .WithMany()
            .HasForeignKey(uq => uq.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uq => uq.Organization)
            .WithMany()
            .HasForeignKey(uq => uq.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

