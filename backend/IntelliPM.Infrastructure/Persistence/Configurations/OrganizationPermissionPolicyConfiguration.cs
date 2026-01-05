using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for OrganizationPermissionPolicy entity.
/// </summary>
public class OrganizationPermissionPolicyConfiguration : IEntityTypeConfiguration<OrganizationPermissionPolicy>
{
    public void Configure(EntityTypeBuilder<OrganizationPermissionPolicy> builder)
    {
        builder.ToTable("OrganizationPermissionPolicies");

        builder.HasKey(p => p.Id);

        // Required fields
        builder.Property(p => p.OrganizationId)
            .IsRequired();

        builder.Property(p => p.AllowedPermissionsJson)
            .IsRequired()
            .HasDefaultValue("[]")
            .HasMaxLength(8000); // JSON array of permission strings

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Unique constraint: one policy per organization
        builder.HasIndex(p => p.OrganizationId)
            .IsUnique()
            .HasDatabaseName("IX_OrganizationPermissionPolicies_OrganizationId");

        // Foreign key relationship
        builder.HasOne(p => p.Organization)
            .WithOne()
            .HasForeignKey<OrganizationPermissionPolicy>(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

