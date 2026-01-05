using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Organization entity.
/// Configures the table structure, unique constraints, indexes, and relationships.
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        // Required fields
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // Unique constraint on Code (slug)
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .HasDatabaseName("IX_Organizations_Code");

        // Index on Name for search
        builder.HasIndex(o => o.Name)
            .HasDatabaseName("IX_Organizations_Name");

        // Relationships are configured in AppDbContext to avoid cascade delete issues
    }
}

