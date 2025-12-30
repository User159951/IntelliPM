using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the ProjectTeam entity.
/// Configures the table structure, constraints, and indexes for project-team relationships.
/// </summary>
public class ProjectTeamConfiguration : IEntityTypeConfiguration<ProjectTeam>
{
    public void Configure(EntityTypeBuilder<ProjectTeam> builder)
    {
        // Table name
        builder.ToTable("ProjectTeams");

        // Primary key
        builder.HasKey(pt => pt.Id);

        // Unique constraint: One team can only be assigned once per project
        builder.HasIndex(pt => new { pt.ProjectId, pt.TeamId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectTeams_ProjectId_TeamId");

        // Indexes for common queries
        builder.HasIndex(pt => pt.ProjectId)
            .HasDatabaseName("IX_ProjectTeams_ProjectId");

        builder.HasIndex(pt => pt.TeamId)
            .HasDatabaseName("IX_ProjectTeams_TeamId");

        builder.HasIndex(pt => pt.OrganizationId)
            .HasDatabaseName("IX_ProjectTeams_OrganizationId");

        builder.HasIndex(pt => pt.IsActive)
            .HasDatabaseName("IX_ProjectTeams_IsActive");

        // Composite index for common query: active teams in a project
        builder.HasIndex(pt => new { pt.ProjectId, pt.IsActive })
            .HasDatabaseName("IX_ProjectTeams_ProjectId_IsActive");

        // Composite index for common query: active projects for a team
        builder.HasIndex(pt => new { pt.TeamId, pt.IsActive })
            .HasDatabaseName("IX_ProjectTeams_TeamId_IsActive");

        // Relationships
        builder.HasOne(pt => pt.Project)
            .WithMany(p => p.AssignedTeams)
            .HasForeignKey(pt => pt.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Team)
            .WithMany(t => t.AssignedProjects)
            .HasForeignKey(pt => pt.TeamId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental team deletion

        builder.HasOne(pt => pt.AssignedBy)
            .WithMany()
            .HasForeignKey(pt => pt.AssignedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Required fields
        builder.Property(pt => pt.ProjectId)
            .IsRequired();

        builder.Property(pt => pt.TeamId)
            .IsRequired();

        builder.Property(pt => pt.OrganizationId)
            .IsRequired();

        builder.Property(pt => pt.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Optional fields
        builder.Property(pt => pt.AssignedById)
            .IsRequired(false);

        builder.Property(pt => pt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pt => pt.UnassignedAt)
            .IsRequired(false);
    }
}

