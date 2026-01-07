using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for RBACPolicyVersion entity.
/// </summary>
public class RBACPolicyVersionConfiguration : IEntityTypeConfiguration<RBACPolicyVersion>
{
    public void Configure(EntityTypeBuilder<RBACPolicyVersion> builder)
    {
        builder.ToTable("RBACPolicyVersions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.VersionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PermissionsSnapshotJson)
            .IsRequired()
            .HasMaxLength(8000); // JSON snapshot

        builder.Property(p => p.RolePermissionsSnapshotJson)
            .IsRequired()
            .HasMaxLength(8000); // JSON snapshot

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.AppliedAt)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => p.VersionNumber)
            .HasDatabaseName("IX_RBACPolicyVersions_VersionNumber");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_RBACPolicyVersions_IsActive");

        builder.HasIndex(p => p.AppliedAt)
            .HasDatabaseName("IX_RBACPolicyVersions_AppliedAt");

        // Relationships
        builder.HasOne(p => p.AppliedByUser)
            .WithMany()
            .HasForeignKey(p => p.AppliedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

