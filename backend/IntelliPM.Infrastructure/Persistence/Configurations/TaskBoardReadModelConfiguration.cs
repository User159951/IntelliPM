using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the TaskBoardReadModel entity.
/// Configures the table structure, constraints, and indexes for optimized task board queries.
/// </summary>
public class TaskBoardReadModelConfiguration : IEntityTypeConfiguration<TaskBoardReadModel>
{
    public void Configure(EntityTypeBuilder<TaskBoardReadModel> builder)
    {
        // Table name
        builder.ToTable("TaskBoardReadModels");

        // Primary key
        builder.HasKey(t => t.Id);

        // Unique constraint: One read model per project
        builder.HasIndex(t => t.ProjectId)
            .IsUnique()
            .HasDatabaseName("IX_TaskBoardReadModels_ProjectId");

        // Index for organization filtering (multi-tenancy)
        builder.HasIndex(t => t.OrganizationId)
            .HasDatabaseName("IX_TaskBoardReadModels_OrganizationId");

        // Composite index for common query: read model by project and organization
        builder.HasIndex(t => new { t.ProjectId, t.OrganizationId })
            .HasDatabaseName("IX_TaskBoardReadModels_ProjectId_OrganizationId");

        // Relationships
        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required fields
        builder.Property(t => t.ProjectId)
            .IsRequired();

        builder.Property(t => t.OrganizationId)
            .IsRequired();

        builder.Property(t => t.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // JSON columns for task data
        builder.Property(t => t.TodoTasks)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(t => t.InProgressTasks)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(t => t.DoneTasks)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");

        // Concurrency token for optimistic concurrency control
        builder.Property(t => t.Version)
            .IsConcurrencyToken()
            .IsRequired()
            .HasDefaultValue(1);

        // Default values for counts
        builder.Property(t => t.TodoCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.InProgressCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.DoneCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.TotalTaskCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Default values for story points
        builder.Property(t => t.TodoStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.InProgressStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.DoneStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.TotalStoryPoints)
            .IsRequired()
            .HasDefaultValue(0);
    }
}

