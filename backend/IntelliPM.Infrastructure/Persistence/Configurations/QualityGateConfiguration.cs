using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the QualityGate entity.
/// Configures the table structure, indexes, relationships, and constraints for quality gates.
/// </summary>
public class QualityGateConfiguration : IEntityTypeConfiguration<QualityGate>
{
    public void Configure(EntityTypeBuilder<QualityGate> builder)
    {
        builder.ToTable("QualityGates");

        builder.HasKey(qg => qg.Id);

        // Property configurations
        builder.Property(qg => qg.ReleaseId)
            .IsRequired();

        builder.Property(qg => qg.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(qg => qg.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(qg => qg.IsRequired)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(qg => qg.IsBlocking)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(qg => qg.Threshold)
            .HasPrecision(5, 2); // Supports values like 99.99 for percentages

        builder.Property(qg => qg.ActualValue)
            .HasPrecision(5, 2); // Supports values like 99.99 for percentages

        builder.Property(qg => qg.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(qg => qg.Details)
            .HasMaxLength(2000);

        builder.Property(qg => qg.CreatedAt)
            .IsRequired();

        // Indexes
        // Index on ReleaseId for queries like "Get all quality gates for a release"
        builder.HasIndex(qg => qg.ReleaseId)
            .HasDatabaseName("IX_QualityGates_ReleaseId");

        // Composite unique index on (ReleaseId, Type) - one quality gate per type per release
        builder.HasIndex(qg => new { qg.ReleaseId, qg.Type })
            .IsUnique()
            .HasDatabaseName("IX_QualityGates_ReleaseId_Type");

        // Index on Status for filtering by quality gate status
        builder.HasIndex(qg => qg.Status)
            .HasDatabaseName("IX_QualityGates_Status");

        // Relationships
        // Release relationship - cascade delete: when release is deleted, quality gates are deleted
        builder.HasOne(qg => qg.Release)
            .WithMany(r => r.QualityGates)
            .HasForeignKey(qg => qg.ReleaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // CheckedByUser relationship - set null: don't delete quality gates when user is deleted
        builder.HasOne(qg => qg.CheckedByUser)
            .WithMany()
            .HasForeignKey(qg => qg.CheckedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

