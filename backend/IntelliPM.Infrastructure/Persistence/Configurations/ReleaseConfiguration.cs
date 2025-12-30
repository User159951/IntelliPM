using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Release entity.
/// Configures the table structure, indexes, relationships, and constraints for releases.
/// </summary>
public class ReleaseConfiguration : IEntityTypeConfiguration<Release>
{
    public void Configure(EntityTypeBuilder<Release> builder)
    {
        builder.ToTable("Releases");

        builder.HasKey(r => r.Id);

        // Property configurations
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(ReleaseConstants.MaxNameLength);

        builder.Property(r => r.Version)
            .IsRequired()
            .HasMaxLength(ReleaseConstants.MaxVersionLength);

        builder.Property(r => r.Description)
            .HasMaxLength(ReleaseConstants.MaxDescriptionLength);

        builder.Property(r => r.ReleaseNotes)
            .HasMaxLength(ReleaseConstants.MaxReleaseNotesLength);

        builder.Property(r => r.ChangeLog)
            .HasMaxLength(ReleaseConstants.MaxChangeLogLength);

        builder.Property(r => r.TagName)
            .HasMaxLength(ReleaseConstants.MaxTagNameLength);

        builder.Property(r => r.PlannedDate)
            .IsRequired();

        builder.Property(r => r.IsPreRelease)
            .IsRequired()
            .HasDefaultValue(false);

        // Enum conversions
        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.OrganizationId)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedById)
            .IsRequired();

        // Indexes
        // Index on ProjectId for queries like "Get all releases for a project"
        builder.HasIndex(r => r.ProjectId)
            .HasDatabaseName("IX_Releases_ProjectId");

        // Index on OrganizationId for multi-tenancy filtering
        builder.HasIndex(r => r.OrganizationId)
            .HasDatabaseName("IX_Releases_OrganizationId")
            .HasFilter("[OrganizationId] IS NOT NULL");

        // Index on Version for quick version lookup
        builder.HasIndex(r => r.Version)
            .HasDatabaseName("IX_Releases_Version");

        // Index on Status for filtering by release status
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Releases_Status")
            .HasFilter("[Status] IS NOT NULL");

        // Index on PlannedDate for date range queries
        builder.HasIndex(r => r.PlannedDate)
            .HasDatabaseName("IX_Releases_PlannedDate");

        // Composite unique index on (ProjectId, Version) - ensures unique version per project
        builder.HasIndex(r => new { r.ProjectId, r.Version })
            .IsUnique()
            .HasDatabaseName("IX_Releases_Project_Version_Unique");

        // Composite index on (ProjectId, Status) for common query pattern
        builder.HasIndex(r => new { r.ProjectId, r.Status })
            .HasDatabaseName("IX_Releases_Project_Status");

        // Relationships
        // Project relationship - cascade delete: when project is deleted, releases are deleted
        builder.HasOne(r => r.Project)
            .WithMany(p => p.Releases)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // CreatedBy relationship - restrict delete: don't delete releases when user is deleted
        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ReleasedBy relationship - set null: if user is deleted, set ReleasedById to null
        builder.HasOne(r => r.ReleasedBy)
            .WithMany()
            .HasForeignKey(r => r.ReleasedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Sprints relationship - set null: if release is deleted, sprints can exist without release
        builder.HasMany(r => r.Sprints)
            .WithOne(s => s.Release)
            .HasForeignKey(s => s.ReleaseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

