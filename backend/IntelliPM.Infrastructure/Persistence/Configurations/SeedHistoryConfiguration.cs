using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for SeedHistory entity.
/// </summary>
public class SeedHistoryConfiguration : IEntityTypeConfiguration<SeedHistory>
{
    public void Configure(EntityTypeBuilder<SeedHistory> builder)
    {
        builder.ToTable("SeedHistories");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Version)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(s => s.AppliedAt)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.Success)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.RecordsAffected)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(s => new { s.SeedName, s.Version })
            .IsUnique()
            .HasDatabaseName("IX_SeedHistories_SeedName_Version");

        builder.HasIndex(s => s.SeedName)
            .HasDatabaseName("IX_SeedHistories_SeedName");

        builder.HasIndex(s => s.AppliedAt)
            .HasDatabaseName("IX_SeedHistories_AppliedAt");

        builder.HasIndex(s => s.Success)
            .HasDatabaseName("IX_SeedHistories_Success");
    }
}

