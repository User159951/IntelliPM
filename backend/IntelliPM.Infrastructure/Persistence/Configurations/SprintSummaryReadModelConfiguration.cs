using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the SprintSummaryReadModel entity.
/// Configures the table structure, constraints, and indexes for optimized sprint summary queries.
/// </summary>
public class SprintSummaryReadModelConfiguration : IEntityTypeConfiguration<SprintSummaryReadModel>
{
    public void Configure(EntityTypeBuilder<SprintSummaryReadModel> builder)
    {
        // Table name
        builder.ToTable("SprintSummaryReadModels");

        // Primary key
        builder.HasKey(s => s.Id);

        // Unique constraint: One read model per sprint
        builder.HasIndex(s => s.SprintId)
            .IsUnique()
            .HasDatabaseName("IX_SprintSummaryReadModels_SprintId");

        // Indexes for common queries
        builder.HasIndex(s => s.ProjectId)
            .HasDatabaseName("IX_SprintSummaryReadModels_ProjectId");

        builder.HasIndex(s => s.OrganizationId)
            .HasDatabaseName("IX_SprintSummaryReadModels_OrganizationId");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_SprintSummaryReadModels_Status");

        // Composite index for common query: sprints by project and status
        builder.HasIndex(s => new { s.ProjectId, s.Status })
            .HasDatabaseName("IX_SprintSummaryReadModels_ProjectId_Status");

        // Relationships
        builder.HasOne(s => s.Sprint)
            .WithMany()
            .HasForeignKey(s => s.SprintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

        // Required fields
        builder.Property(s => s.SprintId)
            .IsRequired();

        builder.Property(s => s.ProjectId)
            .IsRequired();

        builder.Property(s => s.OrganizationId)
            .IsRequired();

        builder.Property(s => s.SprintName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired();

        builder.Property(s => s.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // JSON column for burndown data
        builder.Property(s => s.BurndownData)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        // Decimal precision for percentage calculations
        builder.Property(s => s.CompletionPercentage)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.VelocityPercentage)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CapacityUtilization)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.AverageVelocity)
            .HasPrecision(10, 2)
            .IsRequired()
            .HasDefaultValue(0);

        // Default values for counts
        builder.Property(s => s.TotalTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CompletedTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.InProgressTasks)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.TodoTasks)
            .IsRequired()
            .HasDefaultValue(0);

        // Default values for story points
        builder.Property(s => s.TotalStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CompletedStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.InProgressStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.RemainingStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.EstimatedDaysRemaining)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.IsOnTrack)
            .IsRequired()
            .HasDefaultValue(true);

        // Concurrency token for optimistic concurrency control
        builder.Property(s => s.Version)
            .IsConcurrencyToken()
            .IsRequired()
            .HasDefaultValue(1);
    }
}

