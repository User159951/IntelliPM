using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the ProjectOverviewReadModel entity.
/// Configures the table structure, constraints, and indexes for optimized project overview queries.
/// </summary>
public class ProjectOverviewReadModelConfiguration : IEntityTypeConfiguration<ProjectOverviewReadModel>
{
    public void Configure(EntityTypeBuilder<ProjectOverviewReadModel> builder)
    {
        // Table name
        builder.ToTable("ProjectOverviewReadModels");

        // Primary key
        builder.HasKey(p => p.Id);

        // Unique constraint: One read model per project
        builder.HasIndex(p => p.ProjectId)
            .IsUnique()
            .HasDatabaseName("IX_ProjectOverviewReadModels_ProjectId");

        // Indexes for filtering
        builder.HasIndex(p => p.OrganizationId)
            .HasDatabaseName("IX_ProjectOverviewReadModels_OrganizationId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_ProjectOverviewReadModels_Status");

        builder.HasIndex(p => p.HealthStatus)
            .HasDatabaseName("IX_ProjectOverviewReadModels_HealthStatus");

        // Composite index for common query: projects by organization and status
        builder.HasIndex(p => new { p.OrganizationId, p.Status })
            .HasDatabaseName("IX_ProjectOverviewReadModels_OrganizationId_Status");

        // Composite index for health monitoring
        builder.HasIndex(p => new { p.OrganizationId, p.HealthStatus })
            .HasDatabaseName("IX_ProjectOverviewReadModels_OrganizationId_HealthStatus");

        // Relationships
        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required fields
        builder.Property(p => p.ProjectId)
            .IsRequired();

        builder.Property(p => p.OrganizationId)
            .IsRequired();

        builder.Property(p => p.ProjectName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ProjectType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.Property(p => p.OwnerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.HealthStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Unknown");

        builder.Property(p => p.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // JSON columns
        builder.Property(p => p.TeamMembersJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(p => p.VelocityTrendJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(p => p.RiskFactors)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        // Decimal precision for metrics
        builder.Property(p => p.AverageVelocity)
            .HasPrecision(10, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.LastSprintVelocity)
            .HasPrecision(10, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ProjectHealth)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(p => p.OverallProgress)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.SprintProgress)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        // Default values for counts
        builder.Property(p => p.TotalMembers)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ActiveMembers)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TotalSprints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ActiveSprintsCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CompletedSprintsCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TotalTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CompletedTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.InProgressTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TodoTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.BlockedTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.OverdueTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TotalStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CompletedStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.RemainingStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TotalDefects)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.OpenDefects)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CriticalDefects)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ActivitiesLast7Days)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ActivitiesLast30Days)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.DaysUntilNextMilestone)
            .IsRequired()
            .HasDefaultValue(0);

        // Optional fields
        builder.Property(p => p.CurrentSprintId)
            .IsRequired(false);

        builder.Property(p => p.CurrentSprintName)
            .HasMaxLength(200)
            .IsRequired(false);

        // Concurrency token for optimistic concurrency control
        builder.Property(p => p.Version)
            .IsConcurrencyToken()
            .IsRequired()
            .HasDefaultValue(1);
    }
}

