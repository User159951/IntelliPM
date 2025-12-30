using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Milestone entity.
/// Configures the table structure, indexes, relationships, and constraints for milestones.
/// </summary>
public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        // Check constraint for Progress range (0-100) - configured in ToTable as per EF Core recommendation
        builder.ToTable("Milestones", t => t.HasCheckConstraint(
            "CK_Milestones_Progress_Range",
            $"[Progress] >= {MilestoneConstants.MinProgress} AND [Progress] <= {MilestoneConstants.MaxProgress}"));

        builder.HasKey(m => m.Id);

        // Property configurations
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(MilestoneConstants.MaxNameLength);

        builder.Property(m => m.Description)
            .HasMaxLength(MilestoneConstants.MaxDescriptionLength);

        builder.Property(m => m.DueDate)
            .IsRequired();

        builder.Property(m => m.Progress)
            .IsRequired()
            .HasDefaultValue(MilestoneConstants.DefaultProgress);

        // Enum conversions
        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.OrganizationId)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.CreatedById)
            .IsRequired();

        // Indexes
        // Index on ProjectId for queries like "Get all milestones for a project"
        builder.HasIndex(m => m.ProjectId)
            .HasDatabaseName("IX_Milestones_ProjectId");

        // Index on OrganizationId for multi-tenancy filtering
        builder.HasIndex(m => m.OrganizationId)
            .HasDatabaseName("IX_Milestones_OrganizationId")
            .HasFilter("[OrganizationId] IS NOT NULL");

        // Index on DueDate for date range queries (upcoming milestones, overdue milestones)
        builder.HasIndex(m => m.DueDate)
            .HasDatabaseName("IX_Milestones_DueDate");

        // Composite index on (ProjectId, DueDate) for common query pattern
        builder.HasIndex(m => new { m.ProjectId, m.DueDate })
            .HasDatabaseName("IX_Milestones_Project_DueDate");

        // Index on Status for filtering by milestone status
        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_Milestones_Status")
            .HasFilter("[Status] IS NOT NULL");

        // Relationships
        // Project relationship - cascade delete: when project is deleted, milestones are deleted
        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // CreatedBy relationship - restrict delete: don't delete milestones when user is deleted
        builder.HasOne(m => m.CreatedBy)
            .WithMany()
            .HasForeignKey(m => m.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

